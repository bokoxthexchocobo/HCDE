namespace HCDE.Playsim;

/// <summary>
/// Latency heartbeat, dead-client reap, and desync diagnostics from d_net.cpp 6228-6517.
/// </summary>
public static class VmNet1443Slice
{
    public const int BandStart = 6228;
    public const int BandEnd = 6517;
    public const int MaxPlayers = 64;
    public const int BackupTics = 175;
    public const int MaxSendTics = 35;
    public const int LocalCmdTics = 525;
    public const int Latency = 0x01;

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

    public sealed class Body
    {
        public double X;
        public double Y;
        public double Z;
        public int Yaw;
        public int Pitch;
        public int Health;
    }

    public sealed class Peer
    {
        public int CurrentLatency;
        public bool NewLatency;
        public int AverageLatency;
        public ulong[] Sent = new ulong[MaxSendTics];
        public ulong[] Recv = new ulong[MaxSendTics];
        public ulong LastPacket;
        public bool InGame;
        public bool Setup;
        public string Name = "";
        public int CurrentNet = -1;
        public int LastVerified = -1;
        public int Flags;
        public int CurrentSequence = -1;
        public int SequenceAck = -1;
        public int ResendSequence = -1;
        public int ConsistencyAck = -1;
        public int ResendConsistency = -1;
        public int Stability;
        public bool Waiting;
        public bool Inconsistent;
        public int PlayerState;
        public int Health;
        public Body? Actor;
        public short[] Local = new short[BackupTics];
        public short[] Net = new short[BackupTics];
        public Move[] Commands = new Move[BackupTics];
        public uint Tx;
        public uint Rx;
        public uint Duplicates;
        public Peer()
        {
            for (int i = 0; i < BackupTics; i++)
                Commands[i] = new Move();
        }
    }

    public sealed class World
    {
        public int Console;
        public int Arbitrator;
        public int Reserved = -1;
        public int MaxClients = MaxPlayers;
        public bool NetGame;
        public bool Demo;
        public bool LocalAuthority;
        public bool DesyncDebug;
        public int Timeout;
        public int LastLatencyUpdate;
        public ulong Now;
        public int Room;
        public int GameTic;
        public int ClientTic;
        public int TicDup = 1;
        public int Enter;
        public int LastGame;
        public int LevelStatus;
        public int LevelDelay;
        public int LevelDebug;
        public int Lag;
        public bool CommandsReset;
        public int CommandsAhead;
        public int SkipTimer;
        public int SkipAmount;
        public int FullLatency = 105;
        public string? Map;
        public string AppData = "app";
        public int SpawnSeed;
        public int AcsSeed;
        public int ChaseSeed;
        public int DamageSeed;
        public int CurrentConsistency;
        public int LastSentConsistency;
        public List<int> Clients = new();
        public Peer[] Peers = new Peer[MaxPlayers];
        public Move[] Local = new Move[LocalCmdTics];
        public List<string> Logs = new();
        public List<int> Disconnects = new();
        public List<(int Client, byte[] Bytes)> Sent = new();
        public int Dumps;
        public string Saved = "";

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

        public bool IsReserved(int client)
        {
            return Reserved >= 0 && client == Reserved;
        }
    }

    public static void Heartbeat(World w)
    {
        foreach (int client in w.Clients)
        {
            if (client == w.Console || w.Peers[client].Setup)
                continue;
            Peer state = w.Peers[client];
            if (w.LastLatencyUpdate >= MaxSendTics)
            {
                int delta = 0;
                int completed = 0;
                int start = (state.CurrentLatency - MaxSendTics) & 0xFF;
                for (int i = 0; i < MaxSendTics; i++)
                {
                    int tic = (start + i) % MaxSendTics;
                    if (state.Sent[tic] == 0 || state.Recv[tic] < state.Sent[tic])
                        continue;
                    ulong raw = state.Recv[tic] - state.Sent[tic];
                    if (raw > 5000)
                        raw = 5000;
                    delta += (int)raw;
                    completed++;
                }
                if (completed > 0)
                    state.AverageLatency = delta / completed;
            }
            if (state.NewLatency)
            {
                state.Sent[state.CurrentLatency % MaxSendTics] = w.Now;
                state.NewLatency = false;
            }
            w.Sent.Add((client, new byte[] { Latency, (byte)state.CurrentLatency }));
        }
    }

    public static void CheckConsistencies(World w)
    {
        foreach (int client in w.Clients)
            w.Peers[client].LastVerified = w.Peers[client].CurrentNet;
    }

    public static void CheckDead(World w)
    {
        if (!w.NetGame || w.Demo || !w.LocalAuthority || w.Timeout <= 0)
            return;
        var due = new List<int>();
        foreach (int client in w.Clients)
        {
            if (client < 0 || client >= MaxPlayers)
                continue;
            if (w.IsAuthority(client) || w.IsReserved(client))
                continue;
            Peer state = w.Peers[client];
            if (!state.InGame || state.Setup)
                continue;
            if (state.LastPacket == 0)
            {
                state.LastPacket = w.Now;
                continue;
            }
            if (w.Now - state.LastPacket >= (ulong)w.Timeout)
                due.Add(client);
        }
        foreach (int client in due)
        {
            Peer state = w.Peers[client];
            w.Logs.Add("NetGame:: Client " + client + " '" + state.Name + "' timed out after "
                + w.Timeout + " ms with no packets at gametic=" + w.GameTic + " clienttic=" + w.ClientTic
                + " room=" + w.Room + "\n");
            w.Disconnects.Add(client);
        }
    }

    public static uint SumSeeds(World w)
    {
        return (uint)w.SpawnSeed + (uint)w.AcsSeed + (uint)w.ChaseSeed + (uint)w.DamageSeed;
    }

    public static short Consistency(World w, int client, uint seed)
    {
        Peer who = w.Peers[client];
        if (who.Actor != null)
        {
            Body body = who.Actor;
            int pose = (int)((body.X + body.Y + body.Z) * 257) + body.Yaw + body.Pitch;
            seed += (uint)pose;
            seed ^= (uint)who.Health;
        }
        return (short)((seed & 0xFFFF) != 0 ? seed : 1u);
    }

    public static bool UsesAuthoritative(World w)
    {
        return w.NetGame;
    }

    public static void TraceCmd(World w, string? label, int client, int tic, Move cmd)
    {
        w.Logs.Add("desync " + (label ?? "cmd") + " client=" + client + " tic=" + tic
            + " buttons=0x" + cmd.Buttons.ToString("x8") + " pitch=" + cmd.Pitch + " yaw=" + cmd.Yaw
            + " roll=" + cmd.Roll + " fwd=" + cmd.Forward + " side=" + cmd.Side + " up=" + cmd.Up);
    }

    public static void Dump(World w, int client, int consistency, short local, short net)
    {
        if (!w.DesyncDebug)
        {
            w.Dumps++;
            return;
        }
        Peer who = w.Peers[client];
        w.Logs.Add("DESYNC REPORT BEGIN player=" + client + " name=" + who.Name
            + " consistency=" + consistency + " local=" + local + " net=" + net);
        w.Logs.Add("desync session role=" + (w.LocalAuthority ? "authority" : "client")
            + " authority-slot=" + w.Arbitrator + " console=" + w.Console + " room=" + w.Room
            + " map=" + (w.Map ?? "<none>") + " gametic=" + w.GameTic + " clienttic=" + w.ClientTic
            + " ticdup=" + w.TicDup + " enter=" + w.Enter + " last-game=" + w.LastGame);
        w.Logs.Add("desync gates levelstart=" + StatusName(w.LevelStatus) + " delay=" + w.LevelDelay
            + " debug=" + w.LevelDebug + " lag=" + LagName(w.Lag) + " commands-reset=" + (w.CommandsReset ? 1 : 0)
            + " ahead=" + w.CommandsAhead + " skip-timer=" + w.SkipTimer + " skip-amount=" + w.SkipAmount
            + " full-latency-cycle=" + w.FullLatency);
        w.Logs.Add("desync rng spawn=" + w.SpawnSeed + " acs=" + w.AcsSeed + " chase=" + w.ChaseSeed
            + " damagemobj=" + w.DamageSeed + " sum=" + SumSeeds(w) + " current-consistency=" + w.CurrentConsistency
            + " last-sent-consistency=" + w.LastSentConsistency);
        foreach (int player in w.Clients)
        {
            Peer peer = w.Peers[player];
            w.Logs.Add("desync client=" + player + " name=" + peer.Name + " in-game=" + (peer.InGame ? 1 : 0)
                + " waiting=" + (peer.Waiting ? 1 : 0) + " inconsistent=" + (peer.Inconsistent ? 1 : 0)
                + " state=" + peer.PlayerState + " flags=0x" + peer.Flags.ToString("x")
                + " seq=" + peer.CurrentSequence + " ack=" + peer.SequenceAck + " resend-seq=" + peer.ResendSequence
                + " con=" + peer.CurrentNet + " con-ack=" + peer.ConsistencyAck + " resend-con=" + peer.ResendConsistency
                + " verified=" + peer.LastVerified + " avg-lat=" + peer.AverageLatency
                + " cur-lat=" + peer.CurrentLatency + " stability=" + peer.Stability);
            if (peer.Actor == null)
                w.Logs.Add("desync pawn client=" + player + " missing");
            else
                w.Logs.Add("desync pawn client=" + player + " pos=(" + F3(peer.Actor.X)
                    + "," + F3(peer.Actor.Y) + "," + F3(peer.Actor.Z) + ")");
            for (int offset = -2; offset <= 2; offset++)
            {
                int con = consistency + offset;
                if (con < 0)
                    continue;
                int index = con % BackupTics;
                w.Logs.Add("desync con-window client=" + player + " con=" + con
                    + " local=" + peer.Local[index] + " net=" + peer.Net[index]
                    + " marker=" + (con == consistency ? "mismatch" : "nearby"));
            }
            int start = Math.Max(peer.CurrentSequence - 3, 0);
            for (int seq = start; seq <= peer.CurrentSequence + 1; seq++)
            {
                if (seq < 0)
                    continue;
                TraceCmd(w, "netcmd", player, seq, peer.Commands[seq % BackupTics]);
            }
            w.Logs.Add("desync live-peer client=" + player + " tx=" + peer.Tx + " rx=" + peer.Rx
                + " dup=" + peer.Duplicates);
        }
        if (w.Console >= 0 && w.Console < MaxPlayers)
        {
            for (int tic = Math.Max(w.ClientTic - 6, 0); tic <= w.ClientTic + 1; tic++)
                TraceCmd(w, "localcmd", w.Console, tic, w.Local[tic % LocalCmdTics]);
        }
        w.Saved = w.AppData + "/hcde-net-desync-" + w.Now + ".txt";
        w.Logs.Add("DESYNC REPORT END");
        w.Dumps++;
    }

    static string F3(double value)
    {
        return value.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture);
    }

    static string StatusName(int status)
    {
        if (status == 0) return "ready";
        if (status == 1) return "host";
        if (status == 2) return "waiting";
        return "unknown";
    }

    static string LagName(int lag)
    {
        if (lag == 0) return "none";
        if (lag == 1) return "predicting";
        if (lag == 2) return "waiting";
        if (lag == 3) return "skipping";
        return "unknown";
    }
}
