namespace HCDE.Playsim;

/// <summary>
/// Latency and invasion accessors from d_net.cpp 9679-9911.
/// </summary>
public static class VmNet1453Slice
{
    public const int BandStart = 9679;
    public const int BandEnd = 9911;
    public const int TicRate = 35;
    public const int MaxPlayers = 64;
    public const int Boss = 1;

    public sealed class Monster
    {
        public int Health;
        public bool Euthanized;
        public bool Archvile;
    }

    public sealed class World
    {
        public int ClientTic;
        public int GameTic;
        public int Console = -1;
        public int[] Latency = new int[MaxPlayers];
        public int State;
        public int StateTics;
        public int Cutscene;
        public bool Netgame;
        public bool LocalService;
        public int GameType;
        public int Wave;
        public int MaxWaves;
        public int Budget;
        public int Spawned;
        public int Cleared;
        public int ReplicatedMonsters;
        public int ReplicatedArchviles;
        public int WaveFlags;
        public int Spots;
        public int ActiveSpots;
        public int PlanBudget;
        public List<Monster> Monsters = new();
        public List<string> Calls = new();
    }

    public static bool LocalAuthority(World w) => !w.Netgame || w.LocalService;

    public static (int Severity, int Local, int Arbitrator) Latency(World w)
    {
        int delay = (w.ClientTic - w.GameTic) * 1000 / TicRate;
        int severity = delay >= 160 ? 3 : delay >= 120 ? 2 : delay >= 80 ? 1 : 0;
        int arbitrator = w.Console >= 0 && w.Console < MaxPlayers ? w.Latency[w.Console] : 0;
        return (severity, delay, arbitrator);
    }

    public static int State(World w) => w.State;

    public static string StateName(World w)
    {
        return w.State switch
        {
            0 => "disabled",
            1 => "waiting",
            2 => "countdown",
            3 => "spawning",
            4 => "cleanup",
            5 => "intermission",
            6 => "victory",
            7 => "failure",
            _ => "unknown",
        };
    }

    public static int StateTics(World w)
    {
        int authoritative = w.StateTics > 0 ? w.StateTics : 0;
        bool countdown = w.State == 2;
        return LocalAuthority(w) && countdown && w.Cutscene > authoritative ? w.Cutscene : authoritative;
    }

    public static int ClassicState(World w)
    {
        return w.State switch
        {
            2 => 5,
            3 or 4 => 6,
            5 => 7,
            6 => 8,
            7 => 9,
            _ => 0,
        };
    }

    public static int Wave(World w) => Math.Max(w.Wave, 0);
    public static int MaxWaves(World w) => Math.Max(w.MaxWaves, 0);
    public static int WaveBudget(World w) => Math.Max(w.Budget, 0);
    public static int WaveSpawned(World w) => Math.Max(w.Spawned, 0);
    public static int WaveCleared(World w) => Math.Max(w.Cleared, 0);

    public static int ActiveMonsters(World w)
    {
        if (!LocalAuthority(w))
            return Math.Max(w.ReplicatedMonsters, 0);
        return Math.Max(w.Monsters.Count, 0);
    }

    public static int Archviles(World w)
    {
        if (!LocalAuthority(w))
            return Math.Max(w.ReplicatedArchviles, 0);
        int count = 0;
        foreach (var monster in w.Monsters)
        {
            if (monster.Health > 0 && !monster.Euthanized && monster.Archvile)
                count++;
        }
        return count;
    }

    public static bool BossWave(World w) => (w.WaveFlags & Boss) != 0;

    public static int ExportWave(World w) => Wave(w);
    public static int ExportMaxWaves(World w) => MaxWaves(w);
    public static int ExportActive(World w) => ActiveMonsters(w);
    public static int ExportBudget(World w) => WaveBudget(w);
    public static int ExportSpawned(World w) => WaveSpawned(w);
    public static int ExportCleared(World w) => WaveCleared(w);
    public static int ExportSpots(World w) => Math.Max(w.Spots, 0);
    public static int ExportActiveSpots(World w) => Math.Max(w.ActiveSpots, 0);
    public static int ExportPlan(World w) => Math.Max(w.PlanBudget, 0);

    public static int Control(World w, int action, string? reason)
    {
        if (!LocalAuthority(w) || w.GameType != 4)
            return 0;
        switch (action)
        {
            case 1:
                w.Calls.Add(reason ?? "api-nextwave");
                return 1;
            case 2:
                w.Calls.Add(reason ?? "api-victory");
                return 1;
            case 3:
                w.Calls.Add(reason ?? "api-failure");
                return 1;
            default:
                return 0;
        }
    }
}
