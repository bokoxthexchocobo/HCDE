namespace HCDE.Playsim;

/// <summary>
/// Packet trace, live endian helpers, and legacy gameplay reject from d_net.cpp 1770-1985.
/// </summary>
public static class VmNet1457Slice
{
    public const int BandStart = 1770;
    public const int BandEnd = 1985;
    public const int MaxPlayers = 64;
    public const byte Latency = 0x01;
    public const byte LatencyAck = 0x02;
    public const byte Compressed = 0x04;
    public const int Quit = 1;

    public static readonly (ulong Mask, string Name)[] CapabilityNames =
    {
        (1UL << 0, "control-v1"),
        (1UL << 1, "client-input-v5"),
        (1UL << 2, "server-snapshot-v4"),
        (1UL << 3, "server-world-delta-v2"),
        (1UL << 4, "invasion-snapshot-v2"),
        (1UL << 16, "actor-registry-v1"),
        (1UL << 17, "actor-delta-v2"),
        (1UL << 18, "lane-budgets-v1"),
        (1UL << 19, "authority-events-v1"),
    };

    public sealed class Peer
    {
        public ulong Negotiated;
        public int Unsupported;
        public int Flags;
        public int Sequence;
        public int Ack;
        public string Name = "";
        public bool Setup;
        public bool AuthoritySlot;
        public bool UsesHcde;
    }

    public sealed class World
    {
        public ulong LastLog;
        public uint InCount;
        public uint OutCount;
        public int GameTic;
        public int ClientTic;
        public int Room;
        public int Console = -1;
        public bool Netgame;
        public bool Demo;
        public bool NativeOnly;
        public bool LocalAuthority;
        public int LegacyRejected;
        public Peer[] Peers = new Peer[MaxPlayers];
        public List<string> Logs = new();

        public World()
        {
            for (int i = 0; i < MaxPlayers; i++)
                Peers[i] = new Peer();
        }
    }

    public static ulong LocalCapabilities()
    {
        ulong mask = 0;
        foreach (var entry in CapabilityNames)
            mask |= entry.Mask;
        return mask;
    }

    public static bool ShouldAggregate(byte header)
    {
        return (header & (Latency | LatencyAck)) != 0
            && (header & ~(Latency | LatencyAck | Compressed)) == 0;
    }

    public static void Flush(World w, ulong now)
    {
        if (now - w.LastLog < 1000 && w.InCount + w.OutCount < 32)
            return;
        if (w.InCount > 0)
        {
            w.Logs.Add("net.in aggregate packets=" + w.InCount + " gametic=" + w.GameTic + " room=" + (uint)w.Room);
            w.InCount = 0;
        }
        if (w.OutCount > 0)
        {
            w.Logs.Add("net.out aggregate packets=" + w.OutCount + " gametic=" + w.GameTic + " room=" + (uint)w.Room);
            w.OutCount = 0;
        }
        w.LastLog = now;
    }

    public static void TraceIn(World w, int client, byte header, int size, ulong now)
    {
        if (ShouldAggregate(header))
        {
            w.InCount++;
            Flush(w, now);
            return;
        }
        w.Logs.Add("net.in client=" + client + " flags=0x" + header.ToString("x2")
            + " size=" + size + " gametic=" + w.GameTic + " clienttic=" + w.ClientTic + " room=" + (uint)w.Room);
    }

    public static void TraceOut(World w, int client, byte header, int size, ulong now)
    {
        if (client == w.Console)
            return;
        if (ShouldAggregate(header))
        {
            w.OutCount++;
            Flush(w, now);
            return;
        }
        w.Logs.Add("net.out client=" + client + " flags=0x" + header.ToString("x2")
            + " size=" + size + " gametic=" + w.GameTic + " room=" + (uint)w.Room);
    }

    public static ushort ReadBE16(byte[] data, int at) => (ushort)((data[at] << 8) | data[at + 1]);

    public static uint ReadBE32(byte[] data, int at)
        => ((uint)data[at] << 24) | ((uint)data[at + 1] << 16) | ((uint)data[at + 2] << 8) | data[at + 3];

    public static ulong ReadBE64(byte[] data, int at)
    {
        ulong value = 0;
        for (int i = 0; i < 8; i++)
            value = (value << 8) | data[at + i];
        return value;
    }

    public static void WriteBE16(byte[] data, int at, ushort value)
    {
        data[at] = (byte)(value >> 8);
        data[at + 1] = (byte)value;
    }

    public static void WriteBE32(byte[] data, int at, uint value)
    {
        data[at] = (byte)(value >> 24);
        data[at + 1] = (byte)(value >> 16);
        data[at + 2] = (byte)(value >> 8);
        data[at + 3] = (byte)value;
    }

    public static void WriteBE64(byte[] data, int at, ulong value)
    {
        for (int i = 0; i < 8; i++)
            data[at + i] = (byte)(value >> (56 - 8 * i));
    }

    public static string MessageName(byte type) => type switch
    {
        1 => "control",
        2 => "client commands",
        3 => "server snapshot",
        _ => "unknown",
    };

    public static ulong Negotiated(World w, int client)
    {
        if (client < 0 || client >= MaxPlayers)
            return 0;
        return w.Peers[client].Negotiated;
    }

    public static bool HasCapability(World w, int client, ulong capability)
        => (Negotiated(w, client) & capability) == capability;

    public static bool HasRequired(World w, int client)
    {
        ulong required = LocalCapabilities();
        return (Negotiated(w, client) & required) == required;
    }

    public static bool EnforcesNative(World w, int client)
    {
        return w.Netgame && !w.Demo && w.NativeOnly
            && client >= 0 && client < MaxPlayers
            && client != w.Console
            && !w.Peers[client].Setup;
    }

    public static void RejectLegacy(World w, int client, string? direction, string? reason)
    {
        if (client < 0 || client >= MaxPlayers)
            return;
        var peer = w.Peers[client];
        peer.Unsupported++;
        w.LegacyRejected++;
        w.Logs.Add("legacy network path hit: rejected legacy gameplay " + (direction ?? "gameplay")
            + " client=" + client
            + " reason=" + (reason ?? "unknown")
            + " hcde=" + (peer.UsesHcde ? 1 : 0)
            + " negotiated=0x" + peer.Negotiated.ToString("x")
            + " required=0x" + LocalCapabilities().ToString("x")
            + " seq=" + peer.Sequence
            + " ack=" + peer.Ack
            + " room=" + (uint)w.Room + " native-only");
        if (w.LocalAuthority && !peer.AuthoritySlot)
        {
            w.Logs.Add("NetGame:: Disconnecting client " + client + " '" + peer.Name
                + "' for legacy gameplay while HCDE native-only mode is enabled (reason=" + (reason ?? "unknown") + ")");
            peer.Flags |= Quit;
        }
    }
}
