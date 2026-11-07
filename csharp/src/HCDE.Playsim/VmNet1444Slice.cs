namespace HCDE.Playsim;

/// <summary>
/// Consistency markers and the net status gate from d_net.cpp 6518-6726.
/// </summary>
public static class VmNet1444Slice
{
    public const int BandStart = 6518;
    public const int BandEnd = 6726;
    public const int MaxPlayers = 64;
    public const int BackupTics = 175;
    public const int MaxSendTics = 35;
    public const int TicRate = 35;
    public const int Missing = 10;
    public const int Updated = 32;

    public sealed class Peer
    {
        public int Flags;
        public int Sequence = -1;
        public int SequenceAck = -1;
        public bool Waiting;
        public short[] Local = new short[BackupTics];
    }

    public sealed class World
    {
        public bool NetGame;
        public bool Demo;
        public bool MapLoaded;
        public bool LocalAuthority;
        public bool Dedicated;
        public bool NonPlayerAuthority;
        public bool AuthorityWaiting;
        public int Arbitrator;
        public int TicDup = 1;
        public int GameTic;
        public int ClientTic;
        public int Enter;
        public int LastEnter;
        public int LastGame;
        public int Grace;
        public int LevelStatus;
        public int LevelDelay;
        public int Room;
        public string? Map;
        public int CommandsAhead;
        public int SkipTimer;
        public int SkipAmount;
        public int Stability;
        public int DesiredLead;
        public int CurrentConsistency;
        public int ReadyResets;
        public ulong LateJoin;
        public List<int> Clients = new();
        public Peer[] Peers = new Peer[MaxPlayers];
        public List<string> Logs = new();

        public World()
        {
            for (int i = 0; i < MaxPlayers; i++)
                Peers[i] = new Peer();
        }

        public bool IsAuthority(int client)
        {
            return client >= 0 && client < MaxPlayers && client == Arbitrator;
        }
    }

    public static void MakeConsistencies(World w)
    {
        if (!w.NetGame || w.Demo || (w.GameTic % w.TicDup) != 0 || !w.MapLoaded)
            return;
        foreach (int client in w.Clients)
            w.Peers[client].Local[w.CurrentConsistency % BackupTics] = 1;
        w.CurrentConsistency++;
    }

    public static bool UpdateStatus(World w)
    {
        if (!w.NetGame || w.Demo || w.Clients.Count <= 1)
            return true;
        if (w.LevelStatus == 2 || w.LevelDelay > 0)
            return false;
        if (w.LastEnter - w.LastGame >= MaxSendTics * w.TicDup && w.LastEnter >= w.Grace)
        {
            w.LastGame = w.Enter;
            if (w.LocalAuthority)
            {
                int cur = w.GameTic / w.TicDup;
                foreach (int client in w.Clients)
                {
                    if (w.IsAuthority(client) || Late(w, client))
                        continue;
                    if (w.Peers[client].Sequence < cur)
                    {
                        w.Peers[client].Flags |= Missing;
                        w.Peers[client].Waiting = true;
                    }
                    else
                    {
                        w.Peers[client].Waiting = false;
                    }
                }
            }
            else
            {
                w.Peers[w.Arbitrator].Flags |= Missing;
                SetWaiting(w, true);
                w.Logs.Add("authority wait armed client=" + w.Arbitrator + " gametic=" + w.GameTic
                    + " clienttic=" + w.ClientTic + " room=" + w.Room + " map=" + (w.Map ?? "<none>"));
            }
        }
        if (w.LevelStatus == 1 && w.LocalAuthority)
            SetStatus(w, 0, "authority-host-gate-skip");
        if (!w.LocalAuthority)
        {
            foreach (int client in w.Clients)
            {
                if (Late(w, client))
                    continue;
                if (w.Peers[client].Waiting)
                    return false;
            }
        }
        bool updated = false;
        int lowest = int.MaxValue;
        if (w.GameTic > TicRate * 2 && w.GameTic % w.TicDup == 0)
        {
            if (w.LocalAuthority)
            {
                bool all = true;
                int cur = w.ClientTic / w.TicDup;
                foreach (int client in w.Clients)
                {
                    if (!w.IsAuthority(client))
                    {
                        if ((w.Peers[client].Flags & Updated) != 0)
                        {
                            updated = true;
                            int diff = cur - w.Peers[client].Sequence;
                            if (diff < lowest)
                                lowest = diff;
                        }
                        else
                        {
                            all = false;
                        }
                    }
                    w.Peers[client].Flags &= ~Updated;
                }
                if (all)
                {
                    int diff = cur - w.GameTic / w.TicDup;
                    if (diff > 1)
                        lowest = diff;
                }
            }
            else if ((w.Peers[w.Arbitrator].Flags & Updated) != 0)
            {
                updated = true;
                w.Peers[w.Arbitrator].Flags &= ~Updated;
                if (w.Dedicated)
                {
                    int dup = Math.Max(w.TicDup, 1);
                    int lead = Math.Clamp(w.DesiredLead, 0, 8);
                    lowest = (w.ClientTic / dup - w.GameTic / dup) - lead;
                }
                else
                {
                    lowest = w.CommandsAhead;
                }
            }
        }
        if (updated)
        {
            lowest -= w.Stability;
            if (lowest > 0)
            {
                if (!w.LocalAuthority && w.SkipTimer++ > TicRate / 2)
                {
                    w.SkipTimer = 0;
                    if (w.SkipAmount <= 0)
                        w.SkipAmount = Math.Min(lowest, Math.Max(TicRate / 8, 1)) * w.TicDup;
                }
            }
            else
            {
                w.SkipTimer = 0;
            }
        }
        return true;
    }

    static bool Late(World w, int client)
    {
        return client >= 0 && client < MaxPlayers && (w.LateJoin & (1UL << client)) != 0;
    }

    static void SetWaiting(World w, bool waiting)
    {
        if (w.NonPlayerAuthority)
        {
            w.AuthorityWaiting = waiting;
            w.Peers[w.Arbitrator].Waiting = false;
            return;
        }
        w.Peers[w.Arbitrator].Waiting = waiting;
    }

    static void SetStatus(World w, int status, string reason)
    {
        if (w.LevelStatus == status)
            return;
        w.Logs.Add(Name(w.LevelStatus) + " -> " + Name(status) + " reason=" + reason);
        w.LevelStatus = status;
        if (status == 0 && w.NetGame)
            w.ReadyResets++;
    }

    static string Name(int status)
    {
        if (status == 0) return "ready";
        if (status == 1) return "host";
        if (status == 2) return "waiting";
        return "unknown";
    }
}
