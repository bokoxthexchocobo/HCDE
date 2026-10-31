namespace HCDE.Playsim;

/// <summary>
/// Hub intermission totals from <c>g_hub.cpp</c> (<b>1–183</b>). Chunk <b>1187</b>.
/// <c>deathmatch</c> is the int cvar, so only zero keeps the single-player counters.
/// </summary>
public static class VmGHub1187Slice
{
    public const int MaxPlayers = 64;
    public const int FinishSameHub = 0;
    public const int FinishNextHub = 1;
    public const int FinishNoHub = 2;
    public const int ClusterHub = 0x00000001;
    public const int ClusterLookupName = 0x00000040;
    public const int InvalidTexture = -1;

    public static bool Multiplayer;
    public static int Deathmatch;
    public static readonly List<HubInfo> HubData = new();
    public static readonly Dictionary<string, string> Strings = new(StringComparer.OrdinalIgnoreCase);

    public sealed class Player
    {
        public int Skills;
        public int Sitems;
        public int Ssecret;
        public int Stime;
        public int Fragcount;
        public int[] Frags = new int[MaxPlayers];

        public Player Clone()
        {
            var copy = new Player
            {
                Skills = Skills,
                Sitems = Sitems,
                Ssecret = Ssecret,
                Stime = Stime,
                Fragcount = Fragcount,
            };
            Array.Copy(Frags, copy.Frags, MaxPlayers);
            return copy;
        }
    }

    public sealed class Start
    {
        public int FinishedEp;
        public int TotalKills;
        public int MaxKills;
        public int MaxItems;
        public int MaxSecret;
        public int MaxFrags;
        public string ThisName = "";
        public string ThisAuthor = "";
        public int LName0;
        public Player[] Players = NewPlayers();
    }

    public sealed class HubInfo
    {
        public int Levelnum;
        public int TotalKills;
        public int MaxKills;
        public int MaxItems;
        public int MaxSecret;
        public int MaxFrags;
        public Player[] Players = NewPlayers();

        public void CopyFrom(Start wbs)
        {
            Levelnum = wbs.FinishedEp;
            TotalKills = wbs.TotalKills;
            MaxKills = wbs.MaxKills;
            MaxSecret = wbs.MaxSecret;
            MaxItems = wbs.MaxItems;
            MaxFrags = wbs.MaxFrags;
            for (var i = 0; i < MaxPlayers; i++)
                Players[i] = wbs.Players[i].Clone();
        }
    }

    public sealed class Level
    {
        public int Levelnum;
        public int KilledMonsters;
        public int FoundItems;
        public int FoundSecrets;
    }

    public sealed class Cluster
    {
        public int Flags;
        public string ClusterName = "";
    }

    public sealed class Archive
    {
        public bool Writing = true;
        public bool OpenObjects = true;
        public bool ArraysOpen = true;
        public readonly List<string> Trace = new();

        public bool BeginObject(string? key)
        {
            if (!OpenObjects)
                return false;
            Trace.Add(key == null ? "object" : "object " + key);
            return true;
        }

        public void EndObject()
        {
            Trace.Add("end-object");
        }

        public bool BeginArray(string? key)
        {
            if (!Writing && !ArraysOpen)
                return false;
            Trace.Add(key == null ? "array" : "array " + key);
            return true;
        }

        public void EndArray()
        {
            Trace.Add("end-array");
        }

        public void Field(string? key, int value)
        {
            Trace.Add(key == null ? "#" + value : key + "=" + value);
        }
    }

    public static Player[] NewPlayers()
    {
        var players = new Player[MaxPlayers];
        for (var i = 0; i < MaxPlayers; i++)
            players[i] = new Player();
        return players;
    }

    public static string GetString(string name)
    {
        if (Strings.TryGetValue(name, out var value))
            return value;
        return name;
    }

    public static void LeavingHub(Level level, int mode, Cluster cluster, Start wbs)
    {
        if ((cluster.Flags & ClusterHub) != 0)
        {
            var i = 0;
            for (; i < HubData.Count; i++)
            {
                if (HubData[i].Levelnum == level.Levelnum)
                {
                    HubData[i].CopyFrom(wbs);
                    break;
                }
            }

            if (i == HubData.Count)
            {
                var created = new HubInfo();
                created.CopyFrom(wbs);
                HubData.Add(created);
            }

            var slot = HubData[i];
            slot.Levelnum = level.Levelnum;
            if (!Multiplayer && Deathmatch == 0)
            {
                slot.Players[0].Skills = level.KilledMonsters;
                slot.Players[0].Sitems = level.FoundItems;
                slot.Players[0].Ssecret = level.FoundSecrets;
            }

            if (mode != FinishSameHub)
            {
                wbs.TotalKills = level.KilledMonsters;
                wbs.MaxKills = 0;
                wbs.MaxItems = 0;
                wbs.MaxSecret = 0;
                for (var p = 0; p < MaxPlayers; p++)
                {
                    wbs.Players[p].Sitems = 0;
                    wbs.Players[p].Skills = 0;
                    wbs.Players[p].Ssecret = 0;
                }

                for (var h = 0; h < HubData.Count; h++)
                {
                    wbs.MaxKills += HubData[h].MaxKills;
                    wbs.MaxItems += HubData[h].MaxItems;
                    wbs.MaxSecret += HubData[h].MaxSecret;
                    for (var p = 0; p < MaxPlayers; p++)
                    {
                        wbs.Players[p].Sitems += HubData[h].Players[p].Sitems;
                        wbs.Players[p].Skills += HubData[h].Players[p].Skills;
                        wbs.Players[p].Ssecret += HubData[h].Players[p].Ssecret;
                    }
                }

                if (cluster.ClusterName.Length != 0)
                {
                    if ((cluster.Flags & ClusterLookupName) != 0)
                        wbs.ThisName = GetString(cluster.ClusterName);
                    else
                    {
                        wbs.ThisName = cluster.ClusterName;
                        wbs.ThisAuthor = "";
                    }

                    wbs.LName0 = InvalidTexture;
                }
            }
        }

        if (mode != FinishSameHub)
            HubData.Clear();
    }

    public static void SerializePlayer(Archive arc, string? key, Player player)
    {
        if (!arc.BeginObject(key))
            return;
        arc.Field("kills", player.Skills);
        arc.Field("items", player.Sitems);
        arc.Field("secrets", player.Ssecret);
        arc.Field("time", player.Stime);
        arc.Field("fragcount", player.Fragcount);
        if (arc.BeginArray("frags"))
        {
            for (var i = 0; i < MaxPlayers; i++)
                arc.Field(null, player.Frags[i]);
            arc.EndArray();
        }

        arc.EndObject();
    }

    public static void SerializeHubInfo(Archive arc, string? key, HubInfo hub)
    {
        if (!arc.BeginObject(key))
            return;
        arc.Field("levelnum", hub.Levelnum);
        arc.Field("totalkills", hub.TotalKills);
        arc.Field("maxkills", hub.MaxKills);
        arc.Field("maxitems", hub.MaxItems);
        arc.Field("maxsecret", hub.MaxSecret);
        arc.Field("maxfrags", hub.MaxFrags);
        if (arc.BeginArray("players"))
        {
            for (var i = 0; i < MaxPlayers; i++)
                SerializePlayer(arc, null, hub.Players[i]);
            arc.EndArray();
        }

        arc.EndObject();
    }

    public static void SerializeHub(Archive arc)
    {
        if (arc.Writing && HubData.Count == 0)
            return;
        if (!arc.BeginArray("hubinfo"))
        {
            if (!arc.Writing)
                HubData.Clear();
            return;
        }

        for (var i = 0; i < HubData.Count; i++)
            SerializeHubInfo(arc, null, HubData[i]);
        arc.EndArray();
    }

    public static void ClearHubInfo()
    {
        HubData.Clear();
    }
}
