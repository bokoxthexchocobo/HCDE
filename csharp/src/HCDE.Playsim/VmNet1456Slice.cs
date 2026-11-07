namespace HCDE.Playsim;

/// <summary>
/// Invasion announcements, level-start status, and lag HUD from d_net.cpp 1506-1738.
/// </summary>
public static class VmNet1456Slice
{
    public const int BandStart = 1506;
    public const int BandEnd = 1738;
    public const int TicRate = 35;
    public const int Level = 0;
    public const string Red = "\x1cG";

    public sealed class Hud
    {
        public int GameTic, ClientTic, Backlog, Available, Run, Total, WorldSteps, Stability;
        public int Stale;
        public bool TicGate, DedicatedClient, InvasionAuthority;
        public string Lag = "";
        public string Invasion = "";
        public int Wave, Mirrors, PendingSpawns, PendingEvents;
        public double LastMirror, AvgMirror, MaxMirror, AvgWorld, MaxWorld;
    }

    public sealed class World
    {
        public bool Demo;
        public bool Netgame;
        public bool LocalService;
        public bool Dedicated;
        public int GameType;
        public int State;
        public int StateTics;
        public int Cutscene;
        public int Wave;
        public int PendingWave;
        public int AnnouncedState;
        public int AnnouncedWave;
        public int LastSecond = -1;
        public int GameState = Level;
        public int StartStatus;
        public int GameTic;
        public int ClientTic;
        public int Room;
        public string? Map;
        public int WipeGrace = 4;
        public bool Debug;
        public int MaxWaves, Spawned, Budget, Cleared, Active, Participants, Alive;
        public int LagState;
        public int EnterTic, LastUpdate, Available, Run, Total, WorldSteps, Stability;
        public bool TicGate;
        public bool UsesDedicatedSlot;
        public int Mirrors, PendingSpawns, PendingEvents;
        public ulong MirrorMicros, MirrorPasses, MirrorMax;
        public ulong WorldMicros, WorldTics, WorldMax;
        public bool BaselineReset, JitterReset, DiagReady;
        public List<string> Mid = new();
        public List<string> Console = new();
        public List<string> Logs = new();
        public Hud Hud = new();
    }

    public static bool LocalAuthority(World w) => !w.Netgame || w.LocalService;

    public static bool RoundActive(int state) => state is 3 or 4 or 5;

    public static string Ordinal(int number)
    {
        int rem = number % 100;
        if (rem >= 11 && rem <= 13)
            return number + "th";
        return (number % 10) switch
        {
            1 => number + "st",
            2 => number + "nd",
            3 => number + "rd",
            _ => number + "th",
        };
    }

    public static void Show(World w, string? text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        if (!w.Dedicated && w.GameState == Level)
            w.Mid.Add(text);
        else
            w.Console.Add(text);
    }

    public static void ResetAnnouncements(World w)
    {
        w.AnnouncedState = 0;
        w.AnnouncedWave = 0;
        w.LastSecond = -1;
    }

    public static void TickAnnouncements(World w)
    {
        if (w.Demo || w.GameType != 4)
        {
            ResetAnnouncements(w);
            return;
        }
        int wave = Math.Max(w.Wave, 0);
        if (w.State != w.AnnouncedState || wave != w.AnnouncedWave)
        {
            if ((w.State == 5 || w.State == 6) && wave > 0
                && w.AnnouncedState is not 0 and not 1 and not 2)
                Show(w, Red + "Wave " + wave + " Complete!");
            if (w.State == 3 && wave > 0 && w.AnnouncedState != 3)
                Show(w, Red + "Begin!");
            if (w.State != 2)
                w.LastSecond = -1;
            w.AnnouncedState = w.State;
            w.AnnouncedWave = wave;
        }
        if (w.State == 2)
        {
            int tics = StateTics(w);
            int seconds = (tics + TicRate - 1) / TicRate;
            if (seconds > 0 && seconds != w.LastSecond)
            {
                int next = Math.Max(w.PendingWave, 1);
                Show(w, Red + "Prepare for Invasion!\n" + Red + Ordinal(next) + " wave starting in: " + seconds);
                w.LastSecond = seconds;
            }
        }
        else if (w.State == 5)
        {
            int seconds = (Math.Max(w.StateTics, 0) + TicRate - 1) / TicRate;
            if (seconds > 0 && seconds != w.LastSecond)
            {
                int next = Math.Max(wave + 1, 1);
                Show(w, Red + "Prepare for Invasion!\n" + Red + Ordinal(next) + " wave starting in: " + seconds);
                w.LastSecond = seconds;
            }
        }
    }

    public static void SetStartStatus(World w, int status, string? reason)
    {
        if (w.StartStatus == status)
            return;
        w.Logs.Add(StatusName(w.StartStatus) + " -> " + StatusName(status)
            + " reason=" + (reason ?? "?")
            + " gametic=" + w.GameTic + " clienttic=" + w.ClientTic
            + " room=" + (uint)w.Room + " map=" + (w.Map ?? "<none>"));
        w.StartStatus = status;
        if (status == 2 && w.Netgame && !w.LocalService)
            w.BaselineReset = true;
        if (status == 0 && w.Netgame)
        {
            w.JitterReset = true;
            w.DiagReady = true;
        }
    }

    public static void RefreshHud(World w)
    {
        var hud = w.Hud;
        hud.GameTic = w.GameTic;
        hud.ClientTic = w.ClientTic;
        hud.Backlog = w.ClientTic - w.GameTic;
        hud.Available = w.Available;
        hud.Run = w.Run;
        hud.Total = w.Total;
        hud.WorldSteps = w.WorldSteps;
        hud.Stability = w.Stability;
        hud.Stale = Math.Max(w.EnterTic - w.LastUpdate, 0);
        hud.TicGate = w.TicGate;
        hud.DedicatedClient = w.UsesDedicatedSlot && !w.LocalService;
        hud.InvasionAuthority = LocalAuthority(w);
        hud.Lag = LagName(w.LagState);
        hud.Invasion = StateName(w.State);
        hud.Wave = w.Wave;
        hud.Mirrors = w.Mirrors;
        hud.PendingSpawns = w.PendingSpawns;
        hud.PendingEvents = w.PendingEvents;
        hud.LastMirror = w.MirrorMicros / 1000.0;
        hud.AvgMirror = Average(w.MirrorMicros, w.MirrorPasses) / 1000.0;
        hud.MaxMirror = w.MirrorMax / 1000.0;
        hud.AvgWorld = Average(w.WorldMicros, w.WorldTics) / 1000.0;
        hud.MaxWorld = w.WorldMax / 1000.0;
    }

    public static void SetInvasionState(World w, int state, int tics, string? reason)
    {
        tics = Math.Max(tics, 0);
        if (w.State == state && w.StateTics == tics)
            return;
        int previous = w.State;
        w.State = state;
        w.StateTics = tics;
        if (!RoundActive(state))
            w.WipeGrace = 0;
        string why = reason ?? "none";
        string map = w.Map ?? "<none>";
        w.Logs.Add("state " + StateName(previous) + " -> " + StateName(state)
            + " tics=" + tics + " reason=" + why
            + " gametic=" + w.GameTic + " room=" + (uint)w.Room + " map=" + map);
        if (w.Debug)
        {
            w.Console.Add("HCDE invasion state: " + StateName(previous) + " -> " + StateName(state)
                + " tics=" + tics + " reason=" + why
                + " wave=" + w.Wave + "/" + w.MaxWaves
                + " spawned=" + w.Spawned + "/" + w.Budget
                + " cleared=" + w.Cleared + " active=" + w.Active
                + " participants=" + w.Participants + " alive=" + w.Alive
                + " gametic=" + w.GameTic + " map=" + map);
        }
    }

    static int StateTics(World w)
    {
        int authoritative = w.StateTics > 0 ? w.StateTics : 0;
        return LocalAuthority(w) && w.State == 2 && w.Cutscene > authoritative ? w.Cutscene : authoritative;
    }

    static double Average(ulong total, ulong count) => count > 0 ? (double)total / count : 0;

    static string StateName(int state) => state switch
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

    static string StatusName(int status) => status switch
    {
        0 => "ready",
        1 => "host",
        2 => "waiting",
        _ => "unknown",
    };

    static string LagName(int state) => state switch
    {
        0 => "none",
        1 => "predicting",
        2 => "waiting",
        3 => "skipping",
        _ => "unknown",
    };
}
