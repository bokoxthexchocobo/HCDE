namespace HCDE.Playsim;

/// <summary>
/// NetUpdate send window from d_net.cpp 6940-7136.
/// </summary>
public static class VmNet1446Slice
{
    public const int BandStart = 6940;
    public const int BandEnd = 7136;
    public const int MaxPlayers = 64;
    public const int MaxSendTics = 35;
    public const int Slack = 4;
    public const int Quit = 1;
    public const int Missing = 10;
    public const int RetransmitSeq = 4;
    public const int RetransmitCon = 16;
    public const int Retransmit = 20;
    public const int NcmdRetransmit = 0x40;

    public sealed class Peer
    {
        public int Flags;
        public int Sequence = -1;
        public int Net = -1;
        public int SequenceAck = -1;
        public int ConsistencyAck = -1;
        public int ResendSequence = -1;
        public int ResendConsistency = -1;
        public int ResendId;
        public bool Fire;
    }

    public sealed class Outbound
    {
        public int Client;
        public byte[] Header = new byte[10];
        public int SequenceNum;
        public int NumTics;
    }

    public sealed class World
    {
        public bool Demo;
        public bool LocalAuthority;
        public bool Stopped;
        public int Arbitrator;
        public int Console = -1;
        public int MaxClients = MaxPlayers;
        public int TicDup = 1;
        public int ClientTic;
        public int CommandStart;
        public int GameTic;
        public int Room;
        public int Players = 1;
        public int MaxCommands = MaxSendTics;
        public int PlayerLoops = 1;
        public bool ResendOnly;
        public int StartSequence;
        public int EndSequence;
        public int AuthorityResends;
        public int ClientResends;
        public List<int> Clients = new();
        public List<int> Quits = new();
        public List<Outbound> Outbound = new();
        public List<string> Logs = new();
        public Peer[] Peers = new Peer[MaxPlayers];

        public World()
        {
            for (int i = 0; i < MaxPlayers; i++)
                Peers[i] = new Peer();
        }

        public bool IsAuthority(int client)
        {
            return client >= 0 && client < MaxClients && client == Arbitrator;
        }
    }

    public static void SendWindow(World w)
    {
        int newest = w.ClientTic / w.TicDup;
        if (w.Demo)
        {
            foreach (int client in w.Clients)
                w.Peers[client].Sequence = newest;
            w.Stopped = true;
            return;
        }

        int start = w.CommandStart / w.TicDup;
        int end = newest;
        int players = 1;
        int maxCommands = MaxSendTics;
        if (w.LocalAuthority)
        {
            start = w.GameTic / w.TicDup;
            foreach (int client in w.Clients)
            {
                if (w.IsAuthority(client))
                    continue;
                if ((w.Peers[client].Flags & Quit) != 0)
                    w.Quits.Add(client);
                else
                    players++;
            }
            if (players > 1)
            {
                maxCommands = 2;
                if (players >= 8 && players < 16)
                    maxCommands = 2 + (16 - players) / 4;
                else if (players >= 4 && players < 8)
                    maxCommands = 12 - players;
                else if (players < 4)
                    maxCommands = 8 + (4 - players) * 4;
            }
        }

        w.Players = players;
        w.MaxCommands = maxCommands;
        w.PlayerLoops = (int)Math.Ceiling(players / 16.0);
        w.ResendOnly = start == end && w.ClientTic % w.TicDup != 0;
        w.StartSequence = start;
        w.EndSequence = end;
        foreach (int client in w.Clients)
        {
            if (!w.LocalAuthority && !w.IsAuthority(client))
                continue;
            Peer state = w.Peers[client];
            if ((state.Flags & Quit) != 0 || (w.ResendOnly && (state.Flags & (Retransmit | Missing)) == 0))
                continue;
            var header = new byte[10];
            header[0] = (state.Flags & Missing) != 0 ? (byte)NcmdRetransmit : (byte)0;
            state.Flags &= ~Missing;
            header[1] = (byte)(((state.Flags & RetransmitSeq) != 0) ? state.ResendId : w.Room);
            int lastSeq = state.Sequence;
            int lastCon = state.Net;
            if (!w.LocalAuthority)
            {
                foreach (int other in w.Clients)
                {
                    if (w.Peers[other].Sequence < lastSeq)
                        lastSeq = w.Peers[other].Sequence;
                    if (w.Peers[other].Net < lastCon)
                        lastCon = w.Peers[other].Net;
                }
            }
            PutBe(header, 2, lastSeq);
            PutBe(header, 6, lastCon);
            if ((state.Flags & RetransmitSeq) != 0)
            {
                state.Flags &= ~RetransmitSeq;
                if (state.ResendSequence < 0)
                    state.ResendSequence = state.SequenceAck + 1;
            }
            int passive = -1;
            if (w.LocalAuthority)
            {
                if (!IsSelf(w, client) && state.SequenceAck >= 0 && start - state.SequenceAck > Slack && state.Fire)
                {
                    passive = state.SequenceAck + 1;
                    w.AuthorityResends++;
                    w.Logs.Add("passive authority resend client=" + client + " from=" + passive
                        + " start=" + start + " end=" + end + " seq=" + state.Sequence
                        + " ack=" + state.SequenceAck + " room=" + w.Room);
                }
            }
            else if (ClientPassive(w, newest, out int ack, out int lag))
            {
                passive = ack + 1;
                w.ClientResends++;
                w.Logs.Add("passive client resend to authority=" + client + " from=" + passive
                    + " newest=" + newest + " ack=" + ack + " lag=" + lag + " room=" + w.Room);
            }
            int sequenceNum = state.ResendSequence >= 0
                ? state.ResendSequence
                : passive >= 0 ? passive : start;
            if ((state.Flags & RetransmitCon) != 0)
            {
                state.Flags &= ~RetransmitCon;
                if (state.ResendConsistency < 0)
                    state.ResendConsistency = state.ConsistencyAck + 1;
            }
            w.Outbound.Add(new Outbound
            {
                Client = client,
                Header = header,
                SequenceNum = sequenceNum,
                NumTics = Clamp(end - sequenceNum, 0, MaxSendTics),
            });
        }
    }

    static bool IsSelf(World w, int client)
    {
        return client == w.Console;
    }

    static bool ClientPassive(World w, int newest, out int ack, out int lag)
    {
        bool valid = w.Console >= 0 && w.Console < MaxPlayers;
        ack = valid ? w.Peers[w.Console].SequenceAck : -1;
        lag = ack >= 0 ? newest - ack : 0;
        return valid && ack >= 0 && lag > Slack && w.Peers[w.Console].Fire;
    }

    static void PutBe(byte[] header, int at, int value)
    {
        header[at] = (byte)(value >> 24);
        header[at + 1] = (byte)(value >> 16);
        header[at + 2] = (byte)(value >> 8);
        header[at + 3] = (byte)value;
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
