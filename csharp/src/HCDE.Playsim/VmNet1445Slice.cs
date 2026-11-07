namespace HCDE.Playsim;

/// <summary>
/// Command generation inside NetUpdate from d_net.cpp 6727-6939.
/// </summary>
public static class VmNet1445Slice
{
    public const int BandStart = 6727;
    public const int BandEnd = 6939;
    public const int MaxPlayers = 64;
    public const int BackupTics = 175;
    public const int LocalCmdTics = 525;
    public const int MaxSendTics = 35;
    public const int LeadLimit = 87;
    public const int Attack = 1;
    public const int Use = 2;
    public const int LevelReady = 0x10;

    public sealed class Move
    {
        public uint Buttons;
        public int Pitch;
        public int Yaw;
        public int Roll;
        public int Forward;
        public int Side;
        public int Up;
        public Move Copy()
        {
            return new Move { Buttons = Buttons, Pitch = Pitch, Yaw = Yaw, Roll = Roll, Forward = Forward, Side = Side, Up = Up };
        }
    }

    public sealed class Peer
    {
        public Move[] Commands = new Move[BackupTics];
    }

    public sealed class World
    {
        public bool NetGame;
        public bool Demo;
        public bool LocalAuthority;
        public bool MapLoaded;
        public bool Playable = true;
        public bool Pause;
        public bool NetGood = true;
        public int Arbitrator;
        public int Console;
        public int Reserved = -1;
        public int Room;
        public int GameTic;
        public int ClientTic;
        public int TicDup = 1;
        public int LevelStatus;
        public int LevelDelay;
        public int LastLatencyUpdate;
        public int FullLatency = 105;
        public int SkipAmount;
        public uint LastButtons;
        public int ReadyResets;
        public int Packets;
        public int Maintenance;
        public int DeadChecks;
        public int Heartbeats;
        public int LiveControls;
        public int Masters;
        public int Consistencies;
        public int LevelStarts;
        public int Releases;
        public int NewTics;
        public int Starts;
        public int Events;
        public List<Move> Script = new();
        public Move[] Local = new Move[LocalCmdTics];
        public Peer[] Peers = new Peer[MaxPlayers];
        public List<int> Built = new();
        public List<int> Attacks = new();
        public List<int> Uses = new();
        public List<string> Logs = new();
        public List<(int Client, byte[] Bytes)> Sent = new();

        public World()
        {
            for (int i = 0; i < MaxPlayers; i++)
                Peers[i] = new Peer();
            for (int i = 0; i < LocalCmdTics; i++)
                Local[i] = new Move();
        }
    }

    public static void NetUpdate(World w, int tics)
    {
        w.Packets++;
        w.Maintenance++;
        w.DeadChecks++;
        if (tics <= 0)
            return;
        if (w.NetGame && !w.Demo)
        {
            if (w.LocalAuthority)
            {
                w.Logs.Add("host heartbeat gametic=" + w.GameTic);
                w.LastLatencyUpdate += tics;
                if (w.FullLatency > 0)
                    w.FullLatency = Math.Max(w.FullLatency - tics, 0);
                w.Heartbeats++;
                w.LiveControls++;
                w.Masters++;
                if (w.LastLatencyUpdate >= MaxSendTics)
                    w.LastLatencyUpdate = 0;
            }
            else
            {
                w.LiveControls++;
            }
            w.Consistencies++;
        }
        if (w.LevelStatus != 0)
        {
            if (w.LevelStatus == 2)
            {
                if (!w.Playable)
                    w.LevelStarts++;
                else if (w.LocalAuthority)
                    w.Releases++;
                else if (w.MapLoaded)
                    w.Sent.Add((w.Arbitrator, new byte[] { LevelReady, (byte)w.Room }));
            }
            else if (w.LevelStatus == 1 && w.LocalAuthority)
            {
                w.Logs.Add("host -> ready reason=authority-host-gate-skip");
                w.LevelStatus = 0;
                if (w.NetGame)
                    w.ReadyResets++;
            }
        }
        else if (w.LevelDelay > 0)
        {
            if (w.LevelDelay < tics)
                tics -= w.LevelDelay;
            w.LevelDelay = Math.Max(w.LevelDelay - tics, 0);
        }
        bool netGood = w.NetGood;
        int start = w.ClientTic;
        tics = Math.Min(tics, MaxSendTics * w.TicDup);
        if ((start + tics - w.GameTic) / w.TicDup > LeadLimit)
        {
            tics = (w.GameTic + LeadLimit * w.TicDup) - start;
            if (tics <= 0)
            {
                tics = 1;
                netGood = false;
            }
        }
        for (int i = 0; i < tics; i++)
        {
            w.Starts++;
            w.Events++;
            if (w.Pause || !netGood)
                break;
            if (w.SkipAmount > 0)
            {
                w.SkipAmount--;
                continue;
            }
            Move cmd = w.Script.Count > 0 ? w.Script[0].Copy() : new Move();
            if (w.Script.Count > 0)
                w.Script.RemoveAt(0);
            int tic = w.ClientTic++;
            w.Local[tic % LocalCmdTics] = cmd;
            w.Built.Add(tic);
            if (w.LocalAuthority && w.Console >= 0 && w.Console < MaxPlayers && w.Console != w.Reserved && w.TicDup == 1)
                w.Peers[w.Console].Commands[(w.ClientTic - 1) % BackupTics] = cmd;
            uint rising = cmd.Buttons & ~w.LastButtons;
            if ((rising & Attack) != 0)
                w.Attacks.Add(w.ClientTic - 1);
            if ((rising & Use) != 0)
                w.Uses.Add(w.ClientTic - 1);
            w.LastButtons = cmd.Buttons;
            if (w.TicDup == 1)
            {
                w.NewTics++;
            }
            else
            {
                int diff = w.ClientTic % w.TicDup;
                if (diff != 0)
                {
                    int from = w.ClientTic - diff;
                    for (int j = w.ClientTic - 1; j > from; j--)
                        w.Local[(j - 1) % LocalCmdTics].Buttons |= w.Local[j % LocalCmdTics].Buttons;
                }
                else
                {
                    int last = w.ClientTic - w.TicDup;
                    for (int j = w.ClientTic - 1; j > last; j--)
                        w.Local[(j - 1) % LocalCmdTics].Buttons |= w.Local[j % LocalCmdTics].Buttons;
                    int pitch = 0, yaw = 0, roll = 0, forward = 0, side = 0, up = 0;
                    for (int j = 0; j < w.TicDup; j++)
                    {
                        Move sample = w.Local[(last + j) % LocalCmdTics];
                        pitch += sample.Pitch;
                        yaw += sample.Yaw;
                        roll += sample.Roll;
                        forward += sample.Forward;
                        side += sample.Side;
                        up += sample.Up;
                    }
                    pitch /= w.TicDup;
                    yaw /= w.TicDup;
                    roll /= w.TicDup;
                    forward /= w.TicDup;
                    side /= w.TicDup;
                    up /= w.TicDup;
                    for (int j = 0; j < w.TicDup; j++)
                    {
                        Move sample = w.Local[(last + j) % LocalCmdTics];
                        sample.Pitch = pitch;
                        sample.Yaw = yaw;
                        sample.Roll = roll;
                        sample.Forward = forward;
                        sample.Side = side;
                        sample.Up = up;
                    }
                    w.NewTics++;
                }
            }
        }
    }
}
