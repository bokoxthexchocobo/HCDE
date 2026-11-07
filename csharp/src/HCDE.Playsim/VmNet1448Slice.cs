namespace HCDE.Playsim;

/// <summary>
/// Passive resend stall and HCDE live replay from d_net.cpp 2418-2626.
/// </summary>
public static class VmNet1448Slice
{
    public const int BandStart = 2418;
    public const int BandEnd = 2626;
    public const int MaxPlayers = 64;
    public const int TicRate = 35;
    public const int StaleTics = TicRate / 4;
    public const int Quit = 1;
    public const int MissingSeq = 2;
    public const int Control = 1;
    public const int ClientCommands = 2;
    public const int ServerSnapshot = 3;
    public const int HeaderSize = 15;

    public sealed class Peer
    {
        public int LastAck;
        public int AckChanged;
        public int LastRequest;
        public uint Tx;
        public uint Rx;
        public uint RxControl;
        public uint RxCommand;
        public uint RxSnapshot;
        public uint Duplicates;
        public uint Strikes;
        public ulong Window;
        public ulong Suppressed;
        public int Flags;
        public int Sequence;
        public int SequenceAck;
        public string Name = "";
    }

    public sealed class World
    {
        public int GameTic;
        public int ClientTic;
        public int Room;
        public ulong Now = 1;
        public ulong LastReject;
        public bool LocalAuthority;
        public int AuthoritySlot;
        public int Suppressions;
        public int Requests;
        public int Escalations;
        public Peer[] Peers = new Peer[MaxPlayers];
        public List<string> Logs = new();
        public List<string> Prints = new();

        public World()
        {
            for (int i = 0; i < MaxPlayers; i++)
                Peers[i] = new Peer();
        }
    }

    public static bool ShouldFire(World w, int client, int ack)
    {
        if (client < 0 || client >= MaxPlayers || ack < 0)
            return false;
        var peer = w.Peers[client];
        if (peer.LastAck != ack || peer.AckChanged == 0)
        {
            peer.LastAck = ack;
            peer.AckChanged = w.GameTic;
            peer.LastRequest = 0;
            return false;
        }
        if (w.GameTic - peer.AckChanged < StaleTics)
            return false;
        if (peer.LastRequest != 0 && w.GameTic - peer.LastRequest < StaleTics)
            return false;
        peer.LastRequest = w.GameTic;
        return true;
    }

    public static byte[] BeginPacket(World w, int client, int type)
    {
        var peer = w.Peers[client];
        peer.Tx++;
        if (peer.Tx == 0)
            peer.Tx++;
        var data = new byte[HeaderSize];
        data[1] = (byte)'H';
        data[2] = (byte)'L';
        data[3] = (byte)'I';
        data[4] = (byte)'V';
        data[5] = 1;
        data[6] = (byte)type;
        Be32(data, 7, peer.Tx);
        Be32(data, 11, peer.Rx);
        return data;
    }

    public static bool IsFresh(World w, int client, int type, uint sequence)
    {
        var peer = w.Peers[client];
        if (sequence == 0)
        {
            w.Logs.Add("ignored HCDE live " + Name(type) + " from client=" + client + " with zero sequence");
            return false;
        }
        uint typed = Lane(peer, type);
        if (sequence <= typed)
        {
            peer.Duplicates++;
            w.Logs.Add(
                "ignored duplicate HCDE live " + Name(type)
                + " client=" + client
                + " seq=" + sequence
                + " last-type=" + typed
                + " last-any=" + peer.Rx
                + " duplicates=" + peer.Duplicates);
            if ((type == ClientCommands || type == ServerSnapshot) && ReportDue(w))
            {
                w.Prints.Add(
                    "HCDE live packet stale: type=" + Name(type)
                    + " from=" + client
                    + " seq=" + sequence
                    + " last-type=" + typed
                    + " last-any=" + peer.Rx
                    + " duplicates=" + peer.Duplicates
                    + " gametic=" + w.GameTic
                    + " clienttic=" + w.ClientTic);
            }
            return false;
        }
        return true;
    }

    public static void Accept(World w, int client, int type, uint sequence)
    {
        var peer = w.Peers[client];
        SetLane(peer, type, sequence);
        if (sequence > peer.Rx)
            peer.Rx = sequence;
    }

    public static void ClearPressure(World w, int client, string? reason)
    {
        if (client < 0 || client >= MaxPlayers)
            return;
        var peer = w.Peers[client];
        if (peer.Strikes == 0 && peer.Window == 0 && peer.Suppressed == 0)
            return;
        w.Logs.Add(
            "cleared HCDE live replay pressure client=" + client
            + " reason=" + (reason ?? "unknown")
            + " strikes=" + peer.Strikes
            + " suppressed-until=" + peer.Suppressed
            + " room=" + (uint)w.Room);
        peer.Strikes = 0;
        peer.Window = 0;
        peer.Suppressed = 0;
    }

    public static void RequestReplay(World w, int client, string? reason)
    {
        if (client < 0 || client >= MaxPlayers)
            return;
        var peer = w.Peers[client];
        string why = reason ?? "unknown";
        if (peer.Suppressed != 0 && w.Now < peer.Suppressed)
        {
            w.Suppressions++;
            w.Logs.Add(
                "suppressed HCDE live replay request client=" + client
                + " reason=" + why
                + " strikes=" + peer.Strikes
                + " suppress-ms-left=" + (peer.Suppressed - w.Now)
                + " seq=" + peer.Sequence
                + " ack=" + peer.SequenceAck
                + " room=" + (uint)w.Room);
            return;
        }
        if (peer.Window == 0 || w.Now < peer.Window || w.Now - peer.Window > 5000)
        {
            peer.Window = w.Now;
            peer.Strikes = 0;
            peer.Suppressed = 0;
        }
        peer.Strikes++;
        w.Requests++;
        if (peer.Strikes >= 6)
        {
            w.Escalations++;
            if (w.LocalAuthority && client != w.AuthoritySlot)
            {
                w.Prints.Add(
                    "NetGame:: Disconnecting client " + client
                    + " '" + peer.Name + "' for repeated HCDE live replay requests reason=" + why
                    + " strikes=" + peer.Strikes
                    + " at gametic=" + w.GameTic
                    + " clienttic=" + w.ClientTic
                    + " room=" + (uint)w.Room);
                w.Logs.Add(
                    "disconnecting replay-loop source client=" + client
                    + " name=" + peer.Name
                    + " reason=" + why
                    + " strikes=" + peer.Strikes
                    + " seq=" + peer.Sequence
                    + " ack=" + peer.SequenceAck
                    + " rx-snapshot=" + peer.RxSnapshot
                    + " rx-command=" + peer.RxCommand
                    + " room=" + (uint)w.Room);
                peer.Flags |= Quit;
                peer.Strikes = 0;
                peer.Window = w.Now;
                peer.Suppressed = 0;
                return;
            }
            peer.Suppressed = w.Now + 5000;
            w.Logs.Add(
                "HCDE live replay strike limit reached but peer is authority/not locally kickable client=" + client
                + " reason=" + why
                + " strikes=" + peer.Strikes
                + " suppress-ms=5000"
                + " seq=" + peer.Sequence
                + " ack=" + peer.SequenceAck
                + " rx-snapshot=" + peer.RxSnapshot
                + " rx-command=" + peer.RxCommand
                + " room=" + (uint)w.Room);
            return;
        }
        peer.Flags |= MissingSeq;
        w.Logs.Add(
            "requested HCDE live replay client=" + client
            + " reason=" + why
            + " strikes=" + peer.Strikes
            + " window-ms=" + (w.Now - peer.Window)
            + " seq=" + peer.Sequence
            + " ack=" + peer.SequenceAck
            + " rx-snapshot=" + peer.RxSnapshot
            + " rx-command=" + peer.RxCommand
            + " room=" + (uint)w.Room);
    }

    static bool ReportDue(World w)
    {
        if (w.LastReject != 0 && w.Now - w.LastReject < 2000)
            return false;
        w.LastReject = w.Now;
        return true;
    }

    static uint Lane(Peer peer, int type)
    {
        if (type == Control)
            return peer.RxControl;
        if (type == ClientCommands)
            return peer.RxCommand;
        if (type == ServerSnapshot)
            return peer.RxSnapshot;
        return peer.Rx;
    }

    static void SetLane(Peer peer, int type, uint sequence)
    {
        if (type == Control)
            peer.RxControl = sequence;
        else if (type == ClientCommands)
            peer.RxCommand = sequence;
        else if (type == ServerSnapshot)
            peer.RxSnapshot = sequence;
        else
            peer.Rx = sequence;
    }

    static string Name(int type)
    {
        if (type == Control)
            return "control";
        if (type == ClientCommands)
            return "client commands";
        if (type == ServerSnapshot)
            return "server snapshot";
        return "unknown";
    }

    static void Be32(byte[] data, int offset, uint value)
    {
        data[offset] = (byte)(value >> 24);
        data[offset + 1] = (byte)(value >> 16);
        data[offset + 2] = (byte)(value >> 8);
        data[offset + 3] = (byte)value;
    }
}
