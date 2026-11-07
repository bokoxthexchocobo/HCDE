namespace HCDE.Playsim;

/// <summary>
/// Inbound net packet pump from d_net.cpp 5848-6227.
/// The strike counter and the empty user-command skipper are callees.
/// </summary>
public static class VmNet1442Slice
{
    public const int BandStart = 5848;
    public const int BandEnd = 6227;
    public const int MaxPlayers = 64;
    public const int BackupTics = 175;
    public const int MaxSendTics = 35;
    public const int Exit = 0x80;
    public const int Setup = 0x20;
    public const int LevelReady = 0x10;
    public const int Quitters = 0x08;
    public const int LatencyAck = 0x02;
    public const int Latency = 0x01;
    public const int MissingSeq = 2;
    public const int MissingCon = 8;
    public const int Quit = 1;
    public const int UserCmd = 1;
    public const int EmptyCmd = 2;
    public const int StrikeLimit = 6;
    public const ulong StrikeWindow = 5000;

    public sealed class Peer
    {
        public int Flags;
        public int CurrentLatency;
        public bool NewLatency;
        public int AverageLatency;
        public ulong[] RecvTime = new ulong[MaxSendTics];
        public ulong LastPacket;
        public int Stability;
        public int SequenceAck = -1;
        public int CurrentSequence = -1;
        public int ConsistencyAck = -1;
        public int CurrentNetConsistency = -1;
        public short[] NetConsistency = new short[BackupTics];
        public int Strikes;
        public ulong Window;
        public string Name = "";
    }

    public sealed class Packet
    {
        public int Client;
        public byte[] Bytes = System.Array.Empty<byte>();
        public bool Live;
        public bool LiveOk;
        public bool SetupBusy;
        public bool Enforce;
        public bool UsesService;
        public bool Stale;
    }

    public sealed class Cmd
    {
        public int Player;
        public int Sequence;
        public byte[] Payload = System.Array.Empty<byte>();
    }

    public sealed class World
    {
        public int Arbitrator;
        public int MaxClients = MaxPlayers;
        public bool LocalAuthority;
        public int CommandsAhead;
        public int Room;
        public int GameTic;
        public int ClientTic;
        public ulong Now;
        public int Setups;
        public int Connects;
        public int Blackboxes;
        public List<Packet> Inbox = new();
        public Peer[] Peers = new Peer[MaxPlayers];
        public List<string> Logs = new();
        public List<string> Reasons = new();
        public List<(int Client, int Host)> Quits = new();
        public List<(int Client, int Delay)> LevelStarts = new();
        public List<(int Client, string Reason)> Rejects = new();
        public List<(int Client, int Room, int SequenceAck, int ConsistencyAck)> Headers = new();
        public List<int> Disconnects = new();
        public List<(int Player, int From)> Ensured = new();
        public List<Cmd> Commands = new();
        public List<(int Client, byte[] Bytes)> Sent = new();

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

    public static void GetPackets(World w)
    {
        var acks = new List<(int Client, byte Seq)>();
        foreach (Packet packet in w.Inbox)
        {
            byte[] buf = packet.Bytes;
            int len = buf.Length;
            int client = packet.Client;
            Peer state = w.Peers[client];
            state.LastPacket = w.Now;
            w.Blackboxes++;
            byte header = len > 0 ? buf[0] : (byte)0;
            if ((header & Exit) != 0)
            {
                int host = w.IsAuthority(client) && len > 1 ? buf[1] : -1;
                w.Logs.Add("NetGame:: Received exit packet from client " + client
                    + " at gametic=" + w.GameTic + " clienttic=" + w.ClientTic + " room=" + w.Room + "\n");
                w.Quits.Add((client, host));
                continue;
            }
            if ((header & Setup) != 0)
            {
                w.Setups++;
                w.Connects++;
                continue;
            }
            if ((header & Latency) != 0)
            {
                bool seen = false;
                foreach (var ack in acks)
                    if (ack.Client == client) { seen = true; break; }
                if (!seen && len > 1)
                    acks.Add((client, buf[1]));
                continue;
            }
            if ((header & LatencyAck) != 0)
            {
                if (len > 1 && buf[1] == (byte)state.CurrentLatency)
                {
                    state.RecvTime[state.CurrentLatency % MaxSendTics] = w.Now;
                    state.CurrentLatency = (state.CurrentLatency + 1) & 0xFF;
                    state.NewLatency = true;
                }
                continue;
            }
            if ((header & LevelReady) != 0)
            {
                if (len > 1 && buf[1] == (byte)w.Room)
                {
                    int delay = 0;
                    if (w.IsAuthority(client) && len > 3)
                        delay = (buf[2] << 8) | buf[3];
                    w.LevelStarts.Add((client, delay));
                }
                continue;
            }
            if (packet.Live)
            {
                if (packet.LiveOk)
                    continue;
            }
            if (packet.SetupBusy)
                continue;
            if (packet.Enforce)
            {
                string why = packet.UsesService ? "raw-ncmd-from-hcde-peer" : "raw-ncmd-from-non-hcde-peer";
                w.Rejects.Add((client, why));
                continue;
            }
            if (len < 10)
            {
                Note(w, client, "header-too-short");
                state.Flags |= MissingSeq;
                continue;
            }
            int sequenceAck = (buf[2] << 24) | (buf[3] << 16) | (buf[4] << 8) | buf[5];
            int consistencyAck = (buf[6] << 24) | (buf[7] << 16) | (buf[8] << 8) | buf[9];
            w.Headers.Add((client, w.Room, sequenceAck, consistencyAck));
            if (packet.Stale)
                continue;
            int cur = 10;
            bool badFields = false;
            if ((header & Quitters) != 0)
            {
                if (!Has(cur, 1, len))
                {
                    Note(w, client, "quitters-count-missing");
                    state.Flags |= MissingSeq;
                    continue;
                }
                int quitting = buf[cur++];
                for (int i = 0; i < quitting; i++)
                {
                    if (!Has(cur, 1, len))
                    {
                        badFields = true;
                        break;
                    }
                    int quitter = buf[cur++];
                    w.Logs.Add("NetGame:: Authority reported client " + quitter
                        + " quit at gametic=" + w.GameTic + " clienttic=" + w.ClientTic + " room=" + w.Room + "\n");
                    w.Disconnects.Add(quitter);
                }
                if (badFields)
                {
                    Note(w, client, "quitters-entry-malformed");
                    state.Flags |= MissingSeq;
                    continue;
                }
            }
            if (!Has(cur, 1, len))
            {
                Note(w, client, "player-count-missing");
                state.Flags |= MissingSeq;
                continue;
            }
            int players = buf[cur++];
            if (!Has(cur, 1, len))
            {
                Note(w, client, "tic-count-missing");
                state.Flags |= MissingSeq;
                continue;
            }
            int totalTics = buf[cur++];
            int baseSequence = -1;
            if (totalTics > 0)
            {
                if (!Has(cur, 4, len))
                {
                    Note(w, client, "base-sequence-missing");
                    state.Flags |= MissingSeq;
                    continue;
                }
                baseSequence = (buf[cur++] << 24) | (buf[cur++] << 16) | (buf[cur++] << 8) | buf[cur++];
            }
            if (!Has(cur, 1, len))
            {
                Note(w, client, "consistency-count-missing");
                state.Flags |= MissingSeq;
                continue;
            }
            int ranTics = buf[cur++];
            int baseConsistency = -1;
            if (ranTics > 0)
            {
                if (!Has(cur, 4, len))
                {
                    Note(w, client, "base-consistency-missing");
                    state.Flags |= MissingSeq;
                    continue;
                }
                baseConsistency = (buf[cur++] << 24) | (buf[cur++] << 16) | (buf[cur++] << 8) | buf[cur++];
            }
            if (!Has(cur, 1, len))
            {
                Note(w, client, "stability-byte-missing");
                state.Flags |= MissingSeq;
                continue;
            }
            if (w.IsAuthority(client))
                w.CommandsAhead = buf[cur];
            else if (w.LocalAuthority)
                state.Stability = buf[cur];
            cur++;
            for (int p = 0; p < players; p++)
            {
                if (!Has(cur, 1, len))
                {
                    badFields = true;
                    break;
                }
                int player = buf[cur++];
                if (player >= MaxPlayers)
                {
                    badFields = true;
                    w.Logs.Add("malformed packet player index=" + player + " max=" + MaxPlayers
                        + " from=" + client + " room=" + w.Room);
                    break;
                }
                Peer who = w.Peers[player];
                if (w.IsAuthority(client))
                {
                    w.Ensured.Add((player, client));
                    if (!Has(cur, 2, len))
                    {
                        badFields = true;
                        break;
                    }
                    if (!w.LocalAuthority)
                        who.AverageLatency = (buf[cur++] << 8) | buf[cur++];
                    else
                        cur += 2;
                }
                if (Trust(w, client, player))
                    who.ConsistencyAck = consistencyAck;
                var consistencies = new List<short>();
                for (int r = 0; r < ranTics; r++)
                {
                    if (!Has(cur, 3, len))
                    {
                        badFields = true;
                        break;
                    }
                    int ofs = buf[cur++];
                    short value = (short)((buf[cur++] << 8) | buf[cur++]);
                    Insert(consistencies, ofs, value, (short)0);
                }
                if (badFields)
                    break;
                for (int i = 0; i < consistencies.Count; i++)
                {
                    int tic = baseConsistency + i;
                    if (tic <= who.CurrentNetConsistency)
                        continue;
                    if (tic > who.CurrentNetConsistency + 1 || consistencies[i] == 0)
                    {
                        state.Flags |= MissingCon;
                        break;
                    }
                    who.NetConsistency[tic % BackupTics] = consistencies[i];
                    who.CurrentNetConsistency = tic;
                }
                var records = new List<byte[]?>();
                bool badCommand = false;
                ulong seen = 0;
                for (int t = 0; t < totalTics; t++)
                {
                    if (cur >= len)
                    {
                        badCommand = true;
                        break;
                    }
                    int ofs = buf[cur++];
                    if (ofs >= totalTics || ofs >= 64)
                    {
                        badCommand = true;
                        break;
                    }
                    ulong mask = 1ul << ofs;
                    if ((seen & mask) != 0)
                    {
                        badCommand = true;
                        break;
                    }
                    seen |= mask;
                    if (cur > len)
                    {
                        badCommand = true;
                        break;
                    }
                    if (!TryCmd(buf, cur, len, out int used) || used > len - cur)
                    {
                        badCommand = true;
                        break;
                    }
                    byte[] payload = new byte[used];
                    System.Array.Copy(buf, cur, payload, 0, used);
                    Insert<byte[]?>(records, ofs, payload, null);
                    cur += used;
                }
                if (badCommand)
                {
                    Note(w, client, "command-record-malformed");
                    state.Flags |= MissingSeq;
                    continue;
                }
                for (int i = 0; i < records.Count; i++)
                {
                    int seq = baseSequence + i;
                    if (seq <= who.CurrentSequence)
                        continue;
                    if (seq > who.CurrentSequence + 1 || records[i] == null)
                    {
                        state.Flags |= MissingSeq;
                        break;
                    }
                    w.Commands.Add(new Cmd { Player = player, Sequence = seq, Payload = records[i]! });
                    if (Trust(w, client, player))
                        who.CurrentSequence = seq;
                    if (!w.LocalAuthority && !w.IsAuthority(player))
                        who.SequenceAck = seq;
                }
            }
            if (badFields)
            {
                Note(w, client, "packet-fields-malformed");
                state.Flags |= MissingSeq;
                continue;
            }
            state.Strikes = 0;
            state.Window = 0;
        }
        foreach (var ack in acks)
            w.Sent.Add((ack.Client, new byte[] { LatencyAck, ack.Seq }));
        w.Inbox.Clear();
    }

    static bool Trust(World w, int sender, int player)
    {
        return !w.LocalAuthority || w.IsAuthority(player) || !w.IsAuthority(sender);
    }

    static bool Has(int cur, int count, int len)
    {
        return count >= 0 && cur >= 0 && cur + count <= len;
    }

    static void Note(World w, int client, string reason)
    {
        w.Reasons.Add(reason);
        if (client < 0 || client >= MaxPlayers)
            return;
        Peer state = w.Peers[client];
        if (state.Window == 0 || w.Now < state.Window || w.Now - state.Window > StrikeWindow)
        {
            state.Window = w.Now;
            state.Strikes = 0;
        }
        state.Strikes++;
        if (state.Strikes < StrikeLimit)
            return;
        state.Strikes = 0;
        state.Window = w.Now;
        if (!w.LocalAuthority || w.IsAuthority(client))
            return;
        w.Logs.Add("NetGame:: Disconnecting client " + client + " '" + state.Name
            + "' for repeated malformed gameplay packets at gametic=" + w.GameTic
            + " clienttic=" + w.ClientTic + " room=" + w.Room + "\n");
        state.Flags |= Quit;
    }

    static void Insert<T>(List<T> list, int index, T item, T blank)
    {
        if (index >= list.Count)
        {
            while (list.Count < index)
                list.Add(blank);
            list.Add(item);
            return;
        }
        list.Insert(index, item);
    }

    static bool TryCmd(byte[] buf, int cur, int len, out int used)
    {
        used = 0;
        if (cur >= len)
            return false;
        if (buf[cur] == EmptyCmd)
        {
            used = 1;
            return true;
        }
        if (buf[cur] == UserCmd && cur + 1 < len && buf[cur + 1] == 0)
        {
            used = 2;
            return true;
        }
        return false;
    }
}
