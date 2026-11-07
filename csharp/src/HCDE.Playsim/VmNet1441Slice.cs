namespace HCDE.Playsim;

/// <summary>
/// Authority handoff and level-start release from d_net.cpp 5457-5792.
/// </summary>
public static class VmNet1441Slice
{
    public const int BandStart = 5457;
    public const int BandEnd = 5792;
    public const int MaxPlayers = 64;
    public const int TicRate = 35;
    public const int Ready = 0;
    public const int Host = 1;
    public const int Waiting = 2;
    public const int Level = 0;
    public const int LevelReady = 0x10;
    public const int Quit = 1;
    public const int LatencyCap = 350;

    public sealed class Peer
    {
        public int Latency;
        public int Flags;
        public int CurrentSequence;
        public int SequenceAck;
        public int AppliedSequence;
        public string Name = "";
        public bool InGame;
        public bool Late;
    }

    public sealed class Packet
    {
        public int Client;
        public byte[] Bytes = new byte[8];
    }

    public sealed class World
    {
        public int Arbitrator;
        public int Console;
        public int MaxClients = MaxPlayers;
        public int Reserved = -1;
        public bool Dedicated;
        public bool LocalAuthority;
        public bool NetGame = true;
        public bool Demo;
        public int GameState = Level;
        public int GameTic;
        public int ClientTic;
        public int Room;
        public string? Map;
        public int Status = Ready;
        public string StatusReason = "";
        public int Delay;
        public int DebugDelay;
        public ulong Ack;
        public int DesiredLead;
        public int TicDup = 1;
        public byte[] Buffer = new byte[8];
        public List<int> Clients = new();
        public Peer[] Peers = new Peer[MaxPlayers];
        public string AuthorityName = "";
        public List<string> Logs = new();
        public List<Packet> Sent = new();
        public List<string> Watchdogs = new();
        public int Inits;
        public int WaitingSets;
        public int Disconnects;
        public int DemoChecks;
        public int Clears;
        public int JitterResets;
        public bool? ResetCommands;

        public World()
        {
            for (int i = 0; i < MaxPlayers; i++)
                Peers[i] = new Peer();
        }

        public bool IsAuthoritySlot(int client)
        {
            return client >= 0 && client < MaxClients && client == Arbitrator;
        }

        public bool IsReserved(int client)
        {
            return Reserved >= 0 && client == Reserved;
        }
    }

    public static void SetArbitrator(World w, int client)
    {
        w.Arbitrator = client;
        w.Inits++;
        w.Logs.Add(w.AuthorityName + " is the new authority\n");
        foreach (int id in w.Clients)
            w.Peers[id].Latency = 0;
        w.ResetCommands = false;
        w.WaitingSets++;
    }

    public static int SelectNext(World w, int leaving)
    {
        foreach (int client in w.Clients)
        {
            if (client != leaving && !w.IsReserved(client))
                return client;
        }
        foreach (int client in w.Clients)
        {
            if (client != leaving)
                return client;
        }
        if (w.Dedicated)
            return w.Reserved;
        return leaving;
    }

    public static void ClientQuit(World w, int client, int newHost)
    {
        if (client < 0 || client >= MaxPlayers || !w.Peers[client].InGame)
            return;
        if (!w.IsAuthoritySlot(client))
        {
            if (!w.LocalAuthority)
            {
                w.Logs.Add("NetGame:: Ignored unexpected disconnect packet from non-authority client "
                    + client + " at gametic=" + w.GameTic + " clienttic=" + w.ClientTic + " room=" + w.Room + "\n");
                w.Logs.Add("Received disconnect packet from client " + client + " erroneously\n");
            }
            else
            {
                w.Logs.Add("NetGame:: Client " + client + " '" + w.Peers[client].Name
                    + "' sent exit; queueing quit broadcast at gametic=" + w.GameTic
                    + " clienttic=" + w.ClientTic + " room=" + w.Room + "\n");
                w.Peers[client].Flags |= Quit;
            }
            return;
        }
        w.Disconnects++;
        if (w.IsAuthoritySlot(client))
        {
            bool valid = newHost >= 0 && newHost < MaxPlayers && w.Peers[newHost].InGame
                && newHost != client && !w.IsReserved(newHost);
            SetArbitrator(w, valid ? newHost : SelectNext(w, client));
        }
        if (w.Demo)
            w.DemoChecks++;
    }

    public static bool MapLoaded(World w)
    {
        return w.GameState == Level;
    }

    public static ulong PlayableMask(World w)
    {
        ulong mask = 0;
        foreach (int client in w.Clients)
        {
            if (!w.IsAuthoritySlot(client) && !w.Peers[client].Late)
                mask |= 1ul << client;
        }
        return mask;
    }

    public static int AfterRelease()
    {
        return Ready;
    }

    public static int PlayableCount(World w)
    {
        int count = 0;
        foreach (int client in w.Clients)
        {
            if (!w.IsAuthoritySlot(client) && !w.Peers[client].Late)
                count++;
        }
        return count;
    }

    public static bool HasPlayable(World w)
    {
        return PlayableCount(w) > 0;
    }

    public static bool TryRelease(World w)
    {
        ulong mask = PlayableMask(w);
        if (mask == 0 || (w.Ack & mask) != mask || !MapLoaded(w))
            return false;
        int highest = 0;
        foreach (int client in w.Clients)
        {
            if (w.IsAuthoritySlot(client))
                continue;
            int latency = Math.Min(w.Peers[client].Latency, LatencyCap);
            if (latency > highest)
                highest = latency;
        }
        foreach (int client in w.Clients)
        {
            int delay = 0;
            if (!w.IsAuthoritySlot(client))
            {
                int latency = Math.Min(w.Peers[client].Latency, LatencyCap);
                delay = (int)Math.Floor((highest - latency) * (1.0 / 1000.0) * TicRate);
            }
            WriteReady(w, delay);
            Send(w, client);
        }
        w.Ack = 0;
        SetStatus(w, AfterRelease(), "try-release-level-start");
        w.Delay = 0;
        w.DebugDelay = 0;
        w.Watchdogs.Add("authority-release");
        w.Logs.Add("NetGame:: Authority released level start at gametic=" + w.GameTic
            + " clienttic=" + w.ClientTic + " room=" + w.Room + " map=" + MapName(w)
            + " transport-clients=" + w.Clients.Count + " playable-clients=" + PlayableCount(w) + "\n");
        return true;
    }

    public static void CheckLevelStart(World w, int client, int delayTics)
    {
        if (w.Status != Waiting)
        {
            if (w.LocalAuthority && client != w.Console)
            {
                WriteReady(w, 0);
                Send(w, client);
            }
            return;
        }
        if (w.IsAuthoritySlot(client))
        {
            w.Ack = 0;
            SetStatus(w, AfterRelease(), "check-level-start-authority");
            w.Delay = delayTics;
            w.DebugDelay = delayTics;
            int serverTic = (w.Buffer[4] << 24) | (w.Buffer[5] << 16) | (w.Buffer[6] << 8) | w.Buffer[7];
            if (w.GameTic != serverTic)
            {
                w.Logs.Add("NetGame:: Aligning local gametic=" + w.GameTic + " to server gametic=" + serverTic + "\n");
                w.GameTic = serverTic;
            }
            if (w.Dedicated && !w.LocalAuthority && w.GameTic > w.ClientTic)
                Seat(w);
            if (w.Dedicated && !w.LocalAuthority && w.Console >= 0 && w.Console < MaxPlayers)
                ClampLead(w);
            w.Watchdogs.Add("authority-start");
            w.Logs.Add("NetGame:: Authority started level for client at gametic=" + w.GameTic
                + " clienttic=" + w.ClientTic + " room=" + w.Room + " map=" + MapName(w)
                + " delay=" + delayTics + "\n");
            return;
        }
        w.Ack |= 1ul << client;
        TryRelease(w);
    }

    static void Seat(World w)
    {
        int dup = Math.Max(w.TicDup, 1);
        int lead = Math.Clamp(w.DesiredLead, 0, 8);
        w.ClientTic = w.GameTic + lead * dup;
        int sequence = w.ClientTic / dup;
        if (w.Console >= 0 && w.Console < MaxPlayers)
        {
            Peer seat = w.Peers[w.Console];
            seat.CurrentSequence = Math.Max(seat.CurrentSequence, sequence);
            seat.SequenceAck = Math.Max(seat.SequenceAck, sequence);
            seat.AppliedSequence = Math.Max(seat.AppliedSequence, sequence);
        }
        w.Clears++;
    }

    static void ClampLead(World w)
    {
        int dup = Math.Max(w.TicDup, 1);
        int lead = Math.Clamp(w.DesiredLead, 0, 8);
        int maxTic = w.GameTic + lead * dup;
        if (w.ClientTic > maxTic)
        {
            w.ClientTic = maxTic;
            w.Clears++;
        }
        Peer local = w.Peers[w.Console];
        int playTic = Math.Max(w.GameTic / dup, 0);
        local.AppliedSequence = Math.Min(local.CurrentSequence, playTic);
    }

    static void SetStatus(World w, int status, string reason)
    {
        if (w.Status == status)
            return;
        w.StatusReason = reason;
        w.Status = status;
        if (status == Ready && w.NetGame)
            w.JitterResets++;
    }

    static void WriteReady(World w, int delay)
    {
        w.Buffer[0] = LevelReady;
        w.Buffer[1] = (byte)w.Room;
        w.Buffer[2] = (byte)(delay >> 8);
        w.Buffer[3] = (byte)delay;
        w.Buffer[4] = (byte)(w.GameTic >> 24);
        w.Buffer[5] = (byte)(w.GameTic >> 16);
        w.Buffer[6] = (byte)(w.GameTic >> 8);
        w.Buffer[7] = (byte)w.GameTic;
    }

    static void Send(World w, int client)
    {
        w.Sent.Add(new Packet { Client = client, Bytes = (byte[])w.Buffer.Clone() });
    }

    static string MapName(World w)
    {
        return w.Map ?? "<none>";
    }
}
