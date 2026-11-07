namespace HCDE.Playsim;

/// <summary>
/// NetUpdate packet bodies from d_net.cpp 7138-7389.
/// </summary>
public static class VmNet1447Slice
{
    public const int BandStart = 7138;
    public const int BandEnd = 7389;
    public const int MaxPlayers = 64;
    public const int BackupTics = 175;
    public const int LocalCmdTics = 525;
    public const int MaxSendTics = 35;
    public const int PlayersPerPacket = 16;
    public const int Quitters = 0x08;
    public const int Quit = 1;
    public const int EmptyCmd = 2;
    public const int UserCmd = 1;

    public sealed class Move
    {
        public uint Buttons;
        public int Pitch;
        public int Yaw;
        public int Roll;
        public int Forward;
        public int Side;
        public int Up;
    }

    public sealed class Peer
    {
        public int Flags;
        public bool InGame;
        public int AverageLatency;
        public int Stability;
        public int Sequence;
        public int SequenceNum;
        public int NumTics;
        public int ResendSequence = -1;
        public int ResendConsistency = -1;
        public byte[] Header = new byte[10];
        public short[] Consist = new short[BackupTics];
        public Move[] Commands = new Move[BackupTics];
        public byte[]?[] Events = new byte[BackupTics][];
        public Peer()
        {
            for (int i = 0; i < BackupTics; i++)
                Commands[i] = new Move();
        }
    }

    public sealed class Packet
    {
        public int Client;
        public string Channel = "";
        public byte[] Bytes = System.Array.Empty<byte>();
    }

    public sealed class World
    {
        public bool LocalAuthority;
        public bool ResendOnly;
        public bool Extra;
        public bool NativeClient;
        public bool NativeSnapshot;
        public int Arbitrator;
        public int Console = -1;
        public int MaxClients = MaxPlayers;
        public int TicDup = 1;
        public int Newest;
        public int EndSequence;
        public int Players = 1;
        public int MaxCommands = MaxSendTics;
        public int PlayerLoops = 1;
        public int CurrentConsistency;
        public int LastSent;
        public int Stability;
        public int Packets;
        public List<int> Clients = new();
        public List<int> Quits = new();
        public List<int> Destinations = new();
        public List<int> Disconnects = new();
        public List<Packet> Sent = new();
        public Peer[] Peers = new Peer[MaxPlayers];
        public Move[] Local = new Move[LocalCmdTics];
        public byte[]?[] Events = new byte[BackupTics][];

        public World()
        {
            for (int i = 0; i < MaxPlayers; i++)
                Peers[i] = new Peer();
            for (int i = 0; i < LocalCmdTics; i++)
                Local[i] = new Move();
        }

        public bool IsAuthority(int client)
        {
            return client >= 0 && client < MaxClients && client == Arbitrator;
        }
    }

    public static void WriteBodies(World w)
    {
        foreach (int client in w.Destinations)
        {
            Peer dest = w.Peers[client];
            bool self = client == w.Console;
            int basis = dest.ResendConsistency >= 0 ? dest.ResendConsistency : w.LastSent;
            int ran = w.LocalAuthority ? Clamp(w.CurrentConsistency - basis, 0, MaxSendTics) : 0;
            int ticLoops = (int)Math.Ceiling(Math.Max(dest.NumTics, ran) / (double)w.MaxCommands);
            if (self || ticLoops == 0)
                ticLoops = 1;
            int playerLoops = self ? 1 : w.PlayerLoops;
            int quitsLeft = w.Quits.Count;
            for (int ticOfs = 0, ticLoop = 0; ticLoop < ticLoops; ticLoop++, ticOfs += w.MaxCommands)
            {
                for (int playerOfs = 0, playerLoop = 0; playerLoop < playerLoops; playerLoop++, playerOfs += PlayersPerPacket)
                {
                    var buf = new List<byte>(dest.Header);
                    int packetQuits = quitsLeft > 0 ? quitsLeft : 0;
                    if (quitsLeft > 0)
                    {
                        buf[0] |= Quitters;
                        buf.Add((byte)quitsLeft);
                        for (int i = 0; i < w.Quits.Count && i < packetQuits; i++)
                            buf.Add((byte)w.Quits[i]);
                        quitsLeft = 0;
                    }
                    else
                    {
                        buf[0] = (byte)(buf[0] & ~Quitters);
                    }
                    int playerCount = self ? w.Players : Math.Min(w.Players - playerOfs, PlayersPerPacket);
                    buf.Add((byte)playerCount);
                    var nums = new int[MaxPlayers];
                    if (w.Players > 1)
                    {
                        int seen = 0;
                        foreach (int other in w.Clients)
                        {
                            if ((w.Peers[other].Flags & Quit) != 0)
                                continue;
                            if (seen >= playerOfs)
                                nums[seen - playerOfs] = other;
                            seen++;
                            if (!self && seen >= playerOfs + PlayersPerPacket)
                                break;
                        }
                    }
                    else
                    {
                        nums[0] = w.Console;
                    }
                    int sendTics = self ? dest.NumTics : Clamp(dest.NumTics - ticOfs, 0, w.MaxCommands);
                    if (dest.ResendSequence >= 0)
                    {
                        dest.ResendSequence += sendTics;
                        if (dest.ResendSequence >= w.EndSequence)
                            dest.ResendSequence = -1;
                    }
                    buf.Add((byte)sendTics);
                    if (sendTics > 0)
                        PutBe(buf, dest.SequenceNum + ticOfs);
                    int sendCon = self ? ran : Clamp(ran - ticOfs, 0, w.MaxCommands);
                    if (dest.ResendConsistency >= 0)
                    {
                        dest.ResendConsistency += sendCon;
                        if (dest.ResendConsistency >= w.CurrentConsistency)
                            dest.ResendConsistency = -1;
                    }
                    buf.Add((byte)sendCon);
                    if (sendCon > 0)
                        PutBe(buf, basis + ticOfs);
                    int ahead = w.LocalAuthority
                        ? (w.IsAuthority(client) ? 0 : Math.Max(dest.Sequence + dest.Stability - w.Newest, 0))
                        : Math.Max(w.Stability, 0);
                    buf.Add((byte)ahead);
                    for (int i = 0; i < playerCount; i++)
                    {
                        int who = nums[i];
                        buf.Add((byte)who);
                        if (w.LocalAuthority)
                            Put16(buf, w.Peers[who].AverageLatency);
                        for (int r = 0; r < sendCon; r++)
                        {
                            buf.Add((byte)r);
                            int tic = (basis + ticOfs + r) % BackupTics;
                            Put16(buf, w.Peers[who].Consist[tic]);
                        }
                        for (int t = 0; t < sendTics; t++)
                        {
                            buf.Add((byte)t);
                            int cur = dest.SequenceNum + ticOfs + t;
                            int last = cur - 1;
                            if (who == w.Console)
                            {
                                int real = (cur * w.TicDup) % LocalCmdTics;
                                int realLast = (last * w.TicDup) % LocalCmdTics;
                                AddBytes(buf, w.Events[cur % BackupTics]);
                                WriteCmd(buf, w.Local[real], realLast >= 0 ? w.Local[realLast] : null);
                            }
                            else
                            {
                                Peer actor = w.Peers[who];
                                AddBytes(buf, actor.Events[cur % BackupTics]);
                                WriteCmd(buf, actor.Commands[cur % BackupTics], last >= 0 ? actor.Commands[last % BackupTics] : null);
                            }
                        }
                    }
                    bool canClient = !w.LocalAuthority && !self && playerCount == 1 && nums[0] == w.Console;
                    bool sentClient = canClient && w.NativeClient;
                    bool sentSnapshot = w.LocalAuthority && !self && w.NativeSnapshot;
                    string channel = sentClient ? "client" : sentSnapshot ? "snapshot" : "live";
                    w.Sent.Add(new Packet { Client = client, Channel = channel, Bytes = buf.ToArray() });
                    if (w.Extra && !self)
                        w.Sent.Add(new Packet { Client = client, Channel = channel, Bytes = buf.ToArray() });
                }
            }
        }
        if (w.LocalAuthority && w.Quits.Count > 0)
        {
            foreach (int quitter in w.Quits)
            {
                if (w.Peers[quitter].InGame)
                    w.Disconnects.Add(quitter);
            }
        }
        if (!w.ResendOnly)
            w.LastSent = w.CurrentConsistency;
        w.Packets++;
    }

    static void AddBytes(List<byte> buf, byte[]? bytes)
    {
        if (bytes != null)
            buf.AddRange(bytes);
    }

    static void WriteCmd(List<byte> buf, Move cmd, Move? basis)
    {
        bool live = cmd.Buttons != 0 || cmd.Pitch != 0 || cmd.Yaw != 0 || cmd.Roll != 0
            || cmd.Forward != 0 || cmd.Side != 0 || cmd.Up != 0;
        if (basis == null)
        {
            if (live)
            {
                buf.Add(UserCmd);
                Pack(buf, cmd, null);
                return;
            }
        }
        else if (cmd.Buttons != basis.Buttons || cmd.Yaw != basis.Yaw || cmd.Pitch != basis.Pitch || cmd.Roll != basis.Roll
            || cmd.Forward != basis.Forward || cmd.Side != basis.Side || cmd.Up != basis.Up
            || cmd.Yaw != 0 || cmd.Pitch != 0 || cmd.Forward != 0 || cmd.Side != 0 || cmd.Up != 0)
        {
            buf.Add(UserCmd);
            Pack(buf, cmd, basis);
            return;
        }
        buf.Add(EmptyCmd);
    }

    static void Pack(List<byte> buf, Move cmd, Move? basis)
    {
        Move prior = basis ?? new Move();
        int at = buf.Count;
        buf.Add(0);
        int flags = 0;
        uint changed = cmd.Buttons ^ prior.Buttons;
        if (changed != 0)
        {
            flags |= 0x01;
            byte b0 = (byte)(cmd.Buttons & 0x7F);
            byte b1 = (byte)((cmd.Buttons >> 7) & 0x7F);
            byte b2 = (byte)((cmd.Buttons >> 14) & 0x7F);
            byte b3 = (byte)((cmd.Buttons >> 21) & 0xFF);
            if ((changed & 0xFFFFFF80) != 0)
            {
                b0 |= 0x80;
                if ((changed & 0xFFFFC000) != 0)
                {
                    b1 |= 0x80;
                    if ((changed & 0xFFE00000) != 0)
                        b2 |= 0x80;
                }
            }
            buf.Add(b0);
            if ((b0 & 0x80) != 0)
            {
                buf.Add(b1);
                if ((b1 & 0x80) != 0)
                {
                    buf.Add(b2);
                    if ((b2 & 0x80) != 0)
                        buf.Add(b3);
                }
            }
        }
        if (cmd.Pitch != prior.Pitch || cmd.Pitch != 0)
        {
            flags |= 0x02;
            Put16(buf, cmd.Pitch);
        }
        if (cmd.Yaw != prior.Yaw || cmd.Yaw != 0)
        {
            flags |= 0x04;
            Put16(buf, cmd.Yaw);
        }
        if (cmd.Forward != prior.Forward || cmd.Forward != 0)
        {
            flags |= 0x08;
            Put16(buf, cmd.Forward);
        }
        if (cmd.Side != prior.Side || cmd.Side != 0)
        {
            flags |= 0x10;
            Put16(buf, cmd.Side);
        }
        if (cmd.Up != prior.Up || cmd.Up != 0)
        {
            flags |= 0x20;
            Put16(buf, cmd.Up);
        }
        if (cmd.Roll != prior.Roll)
        {
            flags |= 0x40;
            Put16(buf, cmd.Roll);
        }
        buf[at] = (byte)flags;
    }

    static void PutBe(List<byte> buf, int value)
    {
        buf.Add((byte)(value >> 24));
        buf.Add((byte)(value >> 16));
        buf.Add((byte)(value >> 8));
        buf.Add((byte)value);
    }

    static void Put16(List<byte> buf, int value)
    {
        buf.Add((byte)(value >> 8));
        buf.Add((byte)value);
    }

    static int Clamp(int value, int low, int high)
    {
        if (value < low)
            return low;
        if (value > high)
            return high;
        return value;
    }
}
