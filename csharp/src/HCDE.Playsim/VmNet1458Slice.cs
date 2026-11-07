using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Capability-gated live reject and adaptive bandwidth tick from d_net.cpp 1993-2251.
/// </summary>
public static class VmNet1458Slice
{
    public const int BandStart = 1993;
    public const int BandEnd = 2251;
    public const int MaxPlayers = 64;
    public const int LaneCount = 7;
    public const int ActorDeltaLane = 4;
    public const ulong SampleInterval = 500;

    public const int Light = 0;
    public const int Medium = 1;
    public const int Heavy = 2;

    static readonly string[] ModeNames = { "light", "medium", "heavy" };

    static readonly int[,] Budgets =
    {
        { 96, 4096, 384, 4096, 900, 512, 512 },
        { 96, 4096, 640, 4096, 1800, 768, 512 },
        { 96, 4096, 896, 4096, 3000, 1024, 512 },
    };

    public sealed class Peer
    {
        public int Unsupported;
        public ulong Negotiated;
        public uint Suppressed;
        public ulong LastRejectLog;
    }

    public sealed class AutoState
    {
        public int Resolved = Medium;
        public ulong LastSample;
        public ulong LastChange;
        public ulong LastClampSample;
        public ulong LastDeferredSample;
        public int CleanWindow;
        public double LastClampsPerSec;
        public double LastDeferredPerSec;
        public int LastMaxRtt;
        public int LastPlayerCount;
        public int LastPlayerCap = Heavy;
        public string LastReason = "";
    }

    public sealed class World
    {
        public Peer[] Peers = new Peer[MaxPlayers];
        public int CapabilityRejects;
        public int LegacyControls;
        public AutoState Auto = new();
        public string? Bandwidth = "auto";
        public float DemoteClamps = 4f;
        public float DemoteDeferred = 8f;
        public int DemoteRtt = 120;
        public int PromoteRtt = 50;
        public int PromoteWindowSec = 5;
        public int CooldownSec = 2;
        public bool LocalAuthority = true;
        public int AuthoritySlot;
        public ulong Clamps;
        public ulong Deferred;
        public List<int> Clients = new();
        public int[] Latency = new int[MaxPlayers];
        public List<string> Logs = new();

        public World()
        {
            for (int i = 0; i < MaxPlayers; i++)
                Peers[i] = new Peer();
        }
    }

    public static void Reject(World w, int client, byte type, string? direction, bool inbound, ulong now)
    {
        if (client < 0 || client >= MaxPlayers)
            return;
        var peer = w.Peers[client];
        if (inbound)
            peer.Unsupported++;
        w.CapabilityRejects++;
        if (peer.LastRejectLog != 0 && now - peer.LastRejectLog < 1000)
        {
            peer.Suppressed++;
            return;
        }
        uint suppressed = peer.Suppressed;
        peer.Suppressed = 0;
        peer.LastRejectLog = now;
        w.Logs.Add("rejected HCDE live " + VmNet1457Slice.MessageName(type) + " "
            + (direction ?? "gameplay")
            + " client=" + client
            + " negotiated=0x" + peer.Negotiated.ToString("x")
            + " required=0x" + VmNet1457Slice.LocalCapabilities().ToString("x")
            + " suppressed=" + suppressed + " native-only");
    }

    public static int MaxByPlayerCount(int remotePeers)
    {
        if (remotePeers >= 6) return Light;
        if (remotePeers >= 4) return Medium;
        return Heavy;
    }

    public static int MaxRemoteRtt(World w, out int peers)
    {
        int worst = 0;
        int count = 0;
        foreach (int pNum in w.Clients)
        {
            if (pNum == w.AuthoritySlot) continue;
            if (pNum < 0 || pNum >= MaxPlayers) continue;
            count++;
            if (w.Latency[pNum] > worst) worst = w.Latency[pNum];
        }
        peers = count;
        return worst;
    }

    public static bool TryParseMode(string? text, out int mode)
    {
        mode = Light;
        if (text == null) return false;
        for (int i = 0; i < ModeNames.Length; i++)
        {
            if (string.Equals(text, ModeNames[i], StringComparison.OrdinalIgnoreCase))
            {
                mode = i;
                return true;
            }
        }
        return false;
    }

    public static string ModeName(int mode) => mode >= 0 && mode < ModeNames.Length ? ModeNames[mode] : "unknown";

    public static int Resolve(World w)
    {
        if (string.IsNullOrEmpty(w.Bandwidth))
            return Light;
        if (string.Equals(w.Bandwidth, "auto", StringComparison.OrdinalIgnoreCase))
            return w.Auto.Resolved;
        return TryParseMode(w.Bandwidth, out int mode) ? mode : Light;
    }

    public static int DefaultBudget(World w, int lane)
    {
        if (lane < 0 || lane >= LaneCount)
            return 0;
        return Budgets[Resolve(w), lane];
    }

    public static void Tick(World w, ulong now)
    {
        if (!w.LocalAuthority)
            return;
        if (w.Bandwidth == null || !string.Equals(w.Bandwidth, "auto", StringComparison.OrdinalIgnoreCase))
            return;
        var auto = w.Auto;
        if (auto.LastSample == 0)
        {
            auto.LastSample = now;
            auto.LastClampSample = w.Clamps;
            auto.LastDeferredSample = w.Deferred;
            return;
        }
        ulong elapsed = now - auto.LastSample;
        if (elapsed < SampleInterval)
            return;
        ulong deltaClamps = w.Clamps >= auto.LastClampSample ? w.Clamps - auto.LastClampSample : 0;
        ulong deltaDeferred = w.Deferred >= auto.LastDeferredSample ? w.Deferred - auto.LastDeferredSample : 0;
        auto.LastClampSample = w.Clamps;
        auto.LastDeferredSample = w.Deferred;
        auto.LastSample = now;
        double clampsPerSec = deltaClamps * 1000.0 / elapsed;
        double deferredPerSec = deltaDeferred * 1000.0 / elapsed;
        int maxRtt = MaxRemoteRtt(w, out int remotePeers);
        int playerCap = MaxByPlayerCount(remotePeers);
        auto.LastClampsPerSec = clampsPerSec;
        auto.LastDeferredPerSec = deferredPerSec;
        auto.LastMaxRtt = maxRtt;
        auto.LastPlayerCount = remotePeers;
        auto.LastPlayerCap = playerCap;
        ulong cooldown = (ulong)w.CooldownSec * 1000;
        ulong sinceChange = now >= auto.LastChange ? now - auto.LastChange : 0;
        string? reason = null;
        bool demote = false;
        if (clampsPerSec > w.DemoteClamps)
        {
            demote = true;
            reason = "actor-delta-clamp-rate";
        }
        else if (deferredPerSec > w.DemoteDeferred)
        {
            demote = true;
            reason = "actor-delta-deferred-rate";
        }
        else if (maxRtt > w.DemoteRtt)
        {
            demote = true;
            reason = "max-remote-rtt";
        }
        else if (auto.Resolved > playerCap)
        {
            demote = true;
            reason = "player-count-cap";
        }
        int newMode = auto.Resolved;
        bool changed = false;
        if (demote)
        {
            if (sinceChange < cooldown)
                return;
            int target = auto.Resolved > 0 ? auto.Resolved - 1 : 0;
            newMode = Math.Min(target, playerCap);
            auto.CleanWindow = 0;
            changed = newMode != auto.Resolved;
        }
        else
        {
            bool clean = clampsPerSec <= 0 && deferredPerSec <= 0 && maxRtt < w.PromoteRtt;
            if (clean)
                auto.CleanWindow += (int)elapsed;
            else
                auto.CleanWindow = 0;
            int promoteWindow = w.PromoteWindowSec * 1000;
            if (auto.CleanWindow >= promoteWindow && auto.Resolved < playerCap)
            {
                if (sinceChange < cooldown)
                    return;
                newMode = auto.Resolved + 1;
                auto.CleanWindow = 0;
                reason = "clean-window-promote";
                changed = newMode != auto.Resolved;
            }
        }
        if (!changed)
            return;
        int oldMode = auto.Resolved;
        auto.Resolved = newMode;
        auto.LastChange = now;
        string text = (reason ?? "?") + ": clamps/sec=" + F2(clampsPerSec)
            + " deferred/sec=" + F2(deferredPerSec)
            + " rtt=" + maxRtt + "ms peers=" + remotePeers + " cap=" + ModeName(playerCap);
        if (text.Length > 159)
            text = text[..159];
        auto.LastReason = text;
        w.Logs.Add("auto switched " + ModeName(oldMode) + " -> " + ModeName(newMode) + " (" + text + ")");
    }

    static string F2(double value) => value.ToString("0.00", CultureInfo.InvariantCulture);
}
