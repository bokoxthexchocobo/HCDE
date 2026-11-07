namespace HCDE.Playsim;

/// <summary>
/// Live packet wrap and gameplay envelope prefix from d_net.cpp 2628-2807.
/// </summary>
public static class VmNet1449Slice
{
    public const int BandStart = 2628;
    public const int BandEnd = 2807;
    public const int MaxPlayers = 64;
    public const int MaxMessage = 14000;
    public const int GameplayHeader = 12;
    public const int LiveHeader = 15;
    public const int ClientInputs = 3;
    public const int RepairFlag = 1;

    public sealed class Peer
    {
        public int Unsupported;
        public bool Settings;
        public bool Waiting;
        public string Name = "";
    }

    public sealed class World
    {
        public bool NetGame;
        public bool Demo;
        public bool LocalLiveAuthority;
        public bool DedicatedSlot;
        public bool DedicatedMode;
        public bool LocalAuthority;
        public bool ServerCanControl;
        public int AuthoritySlot;
        public int Arbitrator;
        public int Console = -1;
        public int Room;
        public int GameTic;
        public byte OutgoingFlags;
        public string ServerName = "";
        public bool[] Reserved = new bool[MaxPlayers];
        public bool[] SendInput = new bool[MaxPlayers];
        public bool[] SendSnapshot = new bool[MaxPlayers];
        public Peer[] Peers = new Peer[MaxPlayers];
        public byte[] Buffer = new byte[LiveHeader + GameplayHeader];
        public List<string> Logs = new();
        public List<string> ServerCalls = new();

        public World()
        {
            for (int i = 0; i < MaxPlayers; i++)
                Peers[i] = new Peer();
        }
    }

    public readonly record struct Opened(bool Ok, uint RemoteTic);

    public static bool WrapClient(World w, int client)
    {
        return w.NetGame && !w.Demo && w.SendInput[client];
    }

    public static bool WrapSnapshot(World w, int client)
    {
        return w.NetGame && !w.Demo && w.SendSnapshot[client];
    }

    public static bool EnforcesInputAuthority(World w)
    {
        return w.NetGame && !w.Demo && w.LocalLiveAuthority && w.DedicatedSlot;
    }

    public static bool InputAuthorized(World w, int sender, int player, int count)
    {
        if (!EnforcesInputAuthority(w))
            return true;
        bool authorized = count <= 1 && player == sender;
        if (!authorized)
            w.Logs.Add("reject-unauthorized-player player=" + player + " count=" + (uint)count);
        return authorized;
    }

    public static bool DeltasMutate(World w)
    {
        return w.DedicatedSlot;
    }

    public static bool NonPlayerAuthority(World w)
    {
        return w.DedicatedMode && Reserved(w, w.AuthoritySlot);
    }

    public static bool NonPlayerSlot(World w, int slot)
    {
        return w.DedicatedMode && Reserved(w, slot);
    }

    public static string AuthorityName(World w)
    {
        if (NonPlayerAuthority(w))
            return w.ServerName;
        return w.Peers[w.AuthoritySlot].Name;
    }

    public static void SetSettings(World w, bool enabled)
    {
        int slot = w.AuthoritySlot;
        if (NonPlayerAuthority(w))
        {
            w.ServerCalls.Add("settings " + enabled);
            w.Peers[slot].Settings = false;
            return;
        }
        w.Peers[slot].Settings = enabled;
    }

    public static void SetWaiting(World w, bool waiting)
    {
        int slot = w.AuthoritySlot;
        if (NonPlayerAuthority(w))
        {
            w.ServerCalls.Add("waiting " + waiting);
            w.Peers[slot].Waiting = false;
            return;
        }
        w.Peers[slot].Waiting = waiting;
    }

    public static void Initialize(World w)
    {
        int slot = w.Arbitrator;
        bool nonPlayer = NonPlayerSlot(w, slot);
        string name = nonPlayer ? "HCDE dedicated server" : w.Peers[slot].Name;
        w.ServerCalls.Add("state " + slot + " " + (!nonPlayer) + " true false " + name);
        SetSettings(w, true);
    }

    public static bool LocalCanControl(World w)
    {
        if (!w.NetGame)
            return true;
        if (w.DedicatedMode && w.LocalAuthority)
            return w.ServerCanControl;
        if (w.Console < 0 || w.Console >= MaxPlayers)
            return false;
        return w.Peers[w.Console].Settings;
    }

    public static string PayloadName(int kind)
    {
        if (kind == 1)
            return "reserved legacy client commands";
        if (kind == 2)
            return "reserved legacy server snapshot";
        if (kind == ClientInputs)
            return "client inputs";
        if (kind == 4)
            return "server snapshot";
        return "unknown";
    }

    public static void WriteEnvelope(World w, byte[] data, int kind)
    {
        data[0] = (byte)'H';
        data[1] = (byte)'G';
        data[2] = (byte)'P';
        data[3] = (byte)'L';
        data[4] = 1;
        data[5] = (byte)kind;
        data[6] = (byte)w.Room;
        data[7] = w.OutgoingFlags;
        w.OutgoingFlags = 0;
        uint tic = (uint)Math.Max(w.GameTic, 0);
        data[8] = (byte)(tic >> 24);
        data[9] = (byte)(tic >> 16);
        data[10] = (byte)(tic >> 8);
        data[11] = (byte)tic;
    }

    public static Opened UnwrapPrefix(World w, int client, int payloadSize, string label, int expected)
    {
        var peer = w.Peers[client];
        if (payloadSize < GameplayHeader || payloadSize > MaxMessage)
        {
            peer.Unsupported++;
            w.Logs.Add("malformed HCDE live " + label + " payload from client=" + client + " size=" + payloadSize);
            return new Opened(false, 0);
        }
        var envelope = w.Buffer;
        int at = LiveHeader;
        if (envelope[at] != (byte)'H' || envelope[at + 1] != (byte)'G' || envelope[at + 2] != (byte)'P' || envelope[at + 3] != (byte)'L')
        {
            peer.Unsupported++;
            w.Logs.Add("ignored HCDE live " + label + " without gameplay envelope from client=" + client);
            return new Opened(false, 0);
        }
        byte version = envelope[at + 4];
        byte kind = envelope[at + 5];
        byte flags = envelope[at + 7];
        uint remote = (uint)((envelope[at + 8] << 24) | (envelope[at + 9] << 16) | (envelope[at + 10] << 8) | envelope[at + 11]);
        int permitted = expected == ClientInputs ? RepairFlag : 0;
        if (version != 1 || kind != (byte)expected || (flags & ~permitted) != 0)
        {
            peer.Unsupported++;
            w.Logs.Add(
                "ignored HCDE live " + label
                + " envelope from client=" + client
                + " version=" + version
                + " kind=" + PayloadName(kind)
                + " flags=" + flags);
            return new Opened(false, remote);
        }
        return new Opened(true, remote);
    }

    static bool Reserved(World w, int slot)
    {
        return slot >= 0 && slot < MaxPlayers && w.Reserved[slot];
    }
}
