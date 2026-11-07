namespace HCDE.Playsim;

/// <summary>
/// Bandwidth reset, lane budgets, and live control capabilities from d_net.cpp 2253-2402.
/// </summary>
public static class VmNet1459Slice
{
    public const int BandStart = 2253;
    public const int BandEnd = 2402;
    public const int MaxPlayers = 64;
    public const int Header = 15;
    public const int MinPayload = 20;
    public const int FullPayload = 24;
    public const ulong LaneBudgets = 1UL << 18;
    public const ulong Predator = 1UL << 5;

    public sealed class Peer
    {
        public ulong Negotiated;
        public ulong Remote;
        public ulong UnsupportedCaps;
        public int LegacyControl;
        public int CapabilityControl;
        public int Unsupported;
        public uint RemoteSession;
        public int[] Clamps = new int[7];
    }

    public sealed class World
    {
        public Peer[] Peers = new Peer[MaxPlayers];
        public int LegacyControls;
        public int CapabilityControls;
        public int[] ProfileClamps = new int[7];
        public VmNet1458Slice.World Bandwidth = new();
        public byte[] Buffer = System.Array.Empty<byte>();
        public uint LocalSession;
        public bool LocalAuthority;
        public bool Netgame;
        public List<string> Logs = new();

        public World()
        {
            for (int i = 0; i < MaxPlayers; i++)
                Peers[i] = new Peer();
        }
    }

    public static ulong KnownMask() => VmNet1457Slice.LocalCapabilities() | Predator;

    public static void Reset(World w) => w.Bandwidth.Auto = new VmNet1458Slice.AutoState();

    public static int BudgetBytes(World w, int client, int lane)
    {
        ulong negotiated = client >= 0 && client < MaxPlayers ? w.Peers[client].Negotiated : 0;
        if ((negotiated & LaneBudgets) != LaneBudgets)
            return 0;
        return VmNet1458Slice.DefaultBudget(w.Bandwidth, lane);
    }

    public static int BudgetEnd(World w, int client, int lane, int cursor, int capacity)
    {
        int budget = BudgetBytes(w, client, lane);
        if (budget == 0 || cursor >= capacity)
            return capacity;
        int capped = Math.Min(capacity - cursor, budget);
        int end = cursor + capped;
        if (end < capacity)
            Clamp(w, lane, client);
        return end;
    }

    public static string PrintNames(ulong capabilities, ulong unsupported)
    {
        var parts = new List<string>();
        foreach (var entry in VmNet1457Slice.CapabilityNames)
            if ((capabilities & entry.Mask) != 0)
                parts.Add(entry.Name);
        if (unsupported != 0)
            parts.Add("unknown-0x" + unsupported.ToString("x"));
        return parts.Count == 0 ? "none" : string.Join(" ", parts);
    }

    public static void Append(byte[] output, int payloadOffset, uint session)
    {
        int magic = payloadOffset + 6;
        output[magic] = (byte)'H';
        output[magic + 1] = (byte)'C';
        output[magic + 2] = (byte)'A';
        output[magic + 3] = (byte)'P';
        output[payloadOffset + 10] = 1;
        output[payloadOffset + 11] = 1;
        VmNet1457Slice.WriteBE64(output, payloadOffset + 12, VmNet1457Slice.LocalCapabilities());
        VmNet1457Slice.WriteBE32(output, payloadOffset + 20, session);
    }

    public static void Apply(World w, int client, int payloadSize)
    {
        var peer = w.Peers[client];
        if (payloadSize < MinPayload || !Magic(w.Buffer))
        {
            Clear(peer);
            peer.LegacyControl++;
            w.LegacyControls++;
            return;
        }
        byte version = w.Buffer[Header + 10];
        byte flags = w.Buffer[Header + 11];
        if (version == 0 || version > 1 || (flags & ~1) != 0)
        {
            Clear(peer);
            peer.Unsupported++;
            w.Logs.Add("ignored HCDE live capabilities from client=" + client
                + " version=" + version + " flags=0x" + flags.ToString("x"));
            return;
        }
        ulong previous = peer.Negotiated;
        peer.Remote = VmNet1457Slice.ReadBE64(w.Buffer, Header + 12);
        peer.UnsupportedCaps = peer.Remote & ~KnownMask();
        peer.Negotiated = peer.Remote & VmNet1457Slice.LocalCapabilities();
        peer.CapabilityControl++;
        w.CapabilityControls++;
        if (previous != peer.Negotiated)
            w.Logs.Add("HCDE live capabilities client=" + client
                + " remote=0x" + peer.Remote.ToString("x")
                + " negotiated=0x" + peer.Negotiated.ToString("x"));
        if ((flags & 1) != 0 && payloadSize >= FullPayload)
            Correlate(w, client, VmNet1457Slice.ReadBE32(w.Buffer, Header + 20), peer.Negotiated);
        if (peer.UnsupportedCaps != 0)
            w.Logs.Add("HCDE live control from client=" + client
                + " advertised unknown capabilities=0x" + peer.UnsupportedCaps.ToString("x"));
    }

    public static bool LooksLike(byte[] data, int length)
    {
        return length >= Header && data[0] == 0
            && data[1] == (byte)'H' && data[2] == (byte)'L' && data[3] == (byte)'I' && data[4] == (byte)'V';
    }

    public static bool ReportElapsed(ref ulong last, ulong interval, ulong now)
    {
        if (last != 0 && now - last < interval)
            return false;
        last = now;
        return true;
    }

    static void Clear(Peer peer)
    {
        peer.Remote = 0;
        peer.Negotiated = 0;
        peer.UnsupportedCaps = 0;
    }

    static bool Magic(byte[] data)
    {
        int at = Header + 6;
        return data.Length >= at + 4
            && data[at] == (byte)'H' && data[at + 1] == (byte)'C'
            && data[at + 2] == (byte)'A' && data[at + 3] == (byte)'P';
    }

    static void Correlate(World w, int client, uint session, ulong negotiated)
    {
        if (client < 0 || client >= MaxPlayers)
            return;
        w.Peers[client].RemoteSession = session;
        string role = w.LocalAuthority ? "authority" : w.Netgame ? "client" : "local";
        w.Logs.Add("PEER_CORRELATE local_sess=" + w.LocalSession.ToString("X8")
            + " client=" + client
            + " remote_sess=" + session.ToString("X8")
            + " negotiated=0x" + negotiated.ToString("x")
            + " role=" + role);
    }

    static void Clamp(World w, int lane, int client)
    {
        if (lane < 0 || lane >= 7)
            return;
        w.ProfileClamps[lane]++;
        if (client >= 0 && client < MaxPlayers)
            w.Peers[client].Clamps[lane]++;
    }
}
