using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Ready, invasion console commands, pings, and kick from d_net.cpp 10127-10385.
/// </summary>
public static class VmNet1454Slice
{
    public const int BandStart = 10127;
    public const int BandEnd = 10385;
    public const int Readied = 77;
    public const int KickCommand = 76;
    public const int FastMonsters = 1 << 15;
    public const int MaxPlayers = 64;
    public const int MaxWavesLimit = 3000;

    public sealed class Spot
    {
        public string Source = "";
        public int Tag;
        public double X, Y, Z;
        public int First, Start, Max, Delay, Round, Planned, Spawned, NextTic;
    }

    public sealed class World
    {
        public bool Netgame;
        public bool Demo;
        public bool Multiplayer;
        public bool LocalService;
        public int Console = -1;
        public int Countdown;
        public bool StartedCutscene;
        public bool Readied;
        public int GameType;
        public string? TraceReason;
        public double CountdownSeconds = 30, SpawnSeconds = 8, CleanupSeconds = 4, IntermissionSeconds = 6, ResultSeconds = 8;
        public bool ExitOnVictory = true;
        public int Debug, Waves = 8, WaveLimit, DuelLimit = 0, BaseBudget = 24, BudgetStep = 8, PerPlayer = 6;
        public bool UseMapWaveLimit = true;
        public double SpawnInterval = 0.35;
        public int Burst = 3, MaxActive, BossEvery = 5, BossBonus = 20;
        public bool UseMapTags, Fallback = true;
        public int DmFlags;
        public string StateName = "disabled";
        public int StateTics, Wave, MaxWaves, Budget, Spawned, Cleared, Active, Cap;
        public double Speed = 1;
        public bool Boss;
        public bool HasMap;
        public int MapWaveLimit, MapDuelLimit, ResolvedWaves;
        public bool CanOverride;
        public int TotalSpots, ActiveSpots, SpotBudget, ActiveTag, SpawnedThisWave, GameTic;
        public bool UsingFallback;
        public string FallbackSource = "none";
        public List<Spot> Spots = new();
        public List<int> Clients = new();
        public int[] Latency = new int[MaxPlayers];
        public string[] Names = new string[MaxPlayers];
        public bool[] InGame = new bool[MaxPlayers];
        public bool[] Authority = new bool[MaxPlayers];
        public bool ControlOk = true;
        public List<int> Writes = new();
        public List<string> Logs = new();
        public List<string> Actions = new();
    }

    public static void Ready(World w)
    {
        if (!w.Netgame || w.Demo)
            return;
        if (w.Console >= 0 && w.Console < MaxPlayers)
            w.Readied = true;
        w.Writes.Add(Readied);
    }

    public static void ResetTimer(World w) => w.StartedCutscene = true;

    public static int ReadyTimer(World w) => w.Countdown;

    public static void Victory(World w) => Command(w, "victory");
    public static void Failure(World w) => Command(w, "failure");
    public static void NextWave(World w) => Command(w, "nextwave");

    public static void ApplyPreset(World w, string? reason)
    {
        if (!w.LocalService)
        {
            w.Logs.Add("Invasion preset rejected (authority required)");
            return;
        }
        w.GameType = 4;
        w.TraceReason = reason ?? "invasion-preset";
        w.CountdownSeconds = 30;
        w.SpawnSeconds = 8;
        w.CleanupSeconds = 4;
        w.IntermissionSeconds = 6;
        w.ResultSeconds = 8;
        w.ExitOnVictory = true;
        w.Debug = 0;
        w.Waves = 8;
        w.WaveLimit = 0;
        w.DuelLimit = 0;
        w.UseMapWaveLimit = true;
        w.BaseBudget = 24;
        w.BudgetStep = 8;
        w.PerPlayer = 6;
        w.SpawnInterval = 0.35;
        w.Burst = 3;
        w.MaxActive = 0;
        w.BossEvery = 5;
        w.BossBonus = 20;
        w.UseMapTags = false;
        w.Fallback = true;
        w.DmFlags &= ~FastMonsters;
        w.Logs.Add(
            "Applied Invasion preset (sv_gametype=4, fastmonsters=0, waves=" + w.Waves
            + " explicit-wavelimit=" + w.WaveLimit
            + " use-map-settings=" + (w.UseMapWaveLimit ? 1 : 0)
            + " exit-victory=" + (w.ExitOnVictory ? 1 : 0)
            + " budget=" + w.BaseBudget + "+" + w.BudgetStep + "/wave, per-player=" + w.PerPlayer + ")");
    }

    public static void Preset(World w) => ApplyPreset(w, "invasion-preset");
    public static void Standard(World w) => ApplyPreset(w, "invasion-standard-alias");

    public static void Status(World w)
    {
        int mapWaves = w.HasMap ? Math.Clamp(w.MapWaveLimit, 0, MaxWavesLimit) : 0;
        int mapDuel = w.HasMap ? Math.Clamp(w.MapDuelLimit, 0, 255) : 0;
        w.Logs.Add(
            "Invasion status: state=" + w.StateName + " (" + w.StateTics + " tics) wave=" + w.Wave + "/" + w.MaxWaves
            + " budget=" + w.Budget + " spawned=" + w.Spawned + " cleared=" + w.Cleared + " active=" + w.Active
            + " cap=" + w.Cap + " speedscale=" + w.Speed.ToString("0.00", CultureInfo.InvariantCulture)
            + " boss=" + (w.Boss ? 1 : 0) + " fastmonsters=" + ((w.DmFlags & FastMonsters) != 0 ? 1 : 0));
        w.Logs.Add(
            "Invasion settings: count=" + w.CountdownSeconds.ToString("0.00", CultureInfo.InvariantCulture)
            + " spawn=" + w.SpawnSeconds.ToString("0.00", CultureInfo.InvariantCulture)
            + " cleanup=" + w.CleanupSeconds.ToString("0.00", CultureInfo.InvariantCulture)
            + " inter=" + w.IntermissionSeconds.ToString("0.00", CultureInfo.InvariantCulture)
            + " result=" + w.ResultSeconds.ToString("0.00", CultureInfo.InvariantCulture)
            + " exit-victory=" + (w.ExitOnVictory ? 1 : 0)
            + " debug=" + w.Debug
            + " interval=" + w.SpawnInterval.ToString("0.00", CultureInfo.InvariantCulture)
            + " burst=" + w.Burst + " maxactive=" + w.MaxActive + " waves=" + w.Waves
            + " explicit-wavelimit=" + w.WaveLimit
            + " use-map-wavelimit=" + (w.UseMapWaveLimit ? 1 : 0)
            + " map-wavelimit=" + mapWaves
            + " resolved-wavelimit=" + w.ResolvedWaves
            + " map-duellimit=" + mapDuel
            + " legacy-duellimit=" + Math.Clamp(w.DuelLimit, 0, 255)
            + " allow-offline-overrides=" + (w.CanOverride ? 1 : 0)
            + " base=" + w.BaseBudget + " step=" + w.BudgetStep + " perplayer=" + w.PerPlayer
            + " boss-every=" + w.BossEvery + " boss-bonus=" + w.BossBonus
            + " map-tags=" + (w.UseMapTags ? 1 : 0) + " fallback=" + (w.Fallback ? 1 : 0));
    }

    public static void Spots(World w)
    {
        w.Logs.Add(
            "Invasion spots: total=" + w.TotalSpots + " active=" + w.ActiveSpots + " budget=" + w.SpotBudget
            + " tag=" + w.ActiveTag + " fallback=" + (w.UsingFallback ? 1 : 0) + " source=" + w.FallbackSource
            + " wave=" + w.Wave + " spawned=" + w.SpawnedThisWave);
        for (int i = 0; i < w.Spots.Count; i++)
        {
            var spot = w.Spots[i];
            w.Logs.Add(
                "#" + i + " src=" + spot.Source + " tid=" + spot.Tag
                + " pos=(" + spot.X.ToString("0.0", CultureInfo.InvariantCulture)
                + ", " + spot.Y.ToString("0.0", CultureInfo.InvariantCulture)
                + ", " + spot.Z.ToString("0.0", CultureInfo.InvariantCulture) + ")"
                + " first=" + spot.First + " start=" + spot.Start + " max=" + spot.Max
                + " delay=" + spot.Delay + " round=" + spot.Round + " planned=" + spot.Planned
                + " spawned=" + spot.Spawned + " ready=" + (w.GameTic >= spot.NextTic ? 1 : 0));
        }
    }

    public static void Pings(World w)
    {
        if (!w.Netgame)
        {
            w.Logs.Add("This command can only be used when playing in a net game");
            return;
        }
        if (w.Clients.Count <= 1)
            return;
        foreach (int client in w.Clients)
        {
            if (!w.Authority[client])
                w.Logs.Add(w.Latency[client] + "ms " + w.Names[client] + " [" + client + "]");
        }
    }

    public static void Kick(World w, params string[] args)
    {
        if (args.Length < 1)
        {
            w.Logs.Add("Usage: kick <client numbers>");
            return;
        }
        if (!w.Netgame)
        {
            w.Logs.Add("This command can only be used when playing in a net game");
            return;
        }
        if (!w.LocalService)
        {
            w.Logs.Add("This command is only accessible to the authority");
            return;
        }
        var chosen = new List<int>();
        foreach (string arg in args)
        {
            if (!int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out int client)
                || client < 0 || client >= MaxPlayers)
                w.Logs.Add("Bad client number " + arg);
            else if (client != w.Console && !chosen.Contains(client))
                chosen.Add(client);
        }
        foreach (int client in chosen)
        {
            if (!w.InGame[client])
                w.Logs.Add("Client " + client + " is not in game");
            else
            {
                w.Writes.Add(KickCommand);
                w.Writes.Add(client);
            }
        }
    }

    static void Command(World w, string name)
    {
        if (!w.ControlOk)
            w.Logs.Add("Invasion control rejected (authority + sv_gametype 4 required)");
        else
            w.Actions.Add(name);
    }
}
