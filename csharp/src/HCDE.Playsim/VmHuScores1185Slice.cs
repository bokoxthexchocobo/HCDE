namespace HCDE.Playsim;

/// <summary>
/// Scoreboard order, draw, and toggle from <c>hu_scores.cpp</c> (<b>1–156</b>). Chunk <b>1185</b>.
/// The file ends at 156. The commented <c>HU_SortPlayers</c> block is not live code.
/// </summary>
public static class VmHuScores1185Slice
{
    // EColorRange starts at CR_BRICK = 0, so grey is 2, green is 3, and red is 6.
    public const int CrGrey = 2;
    public const int CrGreen = 3;
    public const int CrRed = 6;

    public sealed class ScorePlayer
    {
        public int FragCount;
        public int KillCount;
        public int Team;
        public string Name = "";
    }

    public sealed class ScoreCvar
    {
        public string Name = "";
        public bool IsBool;
        public bool BoolDefault;
        public int IntDefault;
        public bool Archive;
    }

    public static readonly ScoreCvar[] Cvars =
    {
        new() { Name = "sb_cooperative_enable", IsBool = true, BoolDefault = true, Archive = true },
        new() { Name = "sb_cooperative_headingcolor", IntDefault = CrRed, Archive = true },
        new() { Name = "sb_cooperative_yourplayercolor", IntDefault = CrGreen, Archive = true },
        new() { Name = "sb_cooperative_otherplayercolor", IntDefault = CrGrey, Archive = true },
        new() { Name = "sb_deathmatch_enable", IsBool = true, BoolDefault = true, Archive = true },
        new() { Name = "sb_deathmatch_headingcolor", IntDefault = CrRed, Archive = true },
        new() { Name = "sb_deathmatch_yourplayercolor", IntDefault = CrGreen, Archive = true },
        new() { Name = "sb_deathmatch_otherplayercolor", IntDefault = CrGrey, Archive = true },
        new() { Name = "sb_teamdeathmatch_enable", IsBool = true, BoolDefault = true, Archive = true },
        new() { Name = "sb_teamdeathmatch_headingcolor", IntDefault = CrRed, Archive = true },
    };

    public static int ComparePoints(bool deathmatch, ScorePlayer first, ScorePlayer second)
    {
        var diff = deathmatch
            ? second.FragCount - first.FragCount
            : second.KillCount - first.KillCount;
        if (diff == 0)
            diff = string.Compare(first.Name, second.Name, StringComparison.OrdinalIgnoreCase);
        return diff;
    }

    public static int CompareTeams(ScorePlayer first, ScorePlayer second)
    {
        var diff = first.Team - second.Team;
        if (diff == 0)
        {
            diff = second.FragCount - first.FragCount;
            if (diff == 0)
                diff = string.Compare(first.Name, second.Name, StringComparison.OrdinalIgnoreCase);
        }

        return diff;
    }

    public static void DrawScores(ref bool toggled, bool hasStatusBar, Func<double, int> drawScoreboard, double ticFrac)
    {
        var drawn = false;
        if (hasStatusBar)
            drawn = drawScoreboard(ticFrac) != 0;
        if (!drawn)
            toggled = false;
    }

    public static void ToggleScoreboard(ref bool toggled)
    {
        toggled = !toggled;
    }
}
