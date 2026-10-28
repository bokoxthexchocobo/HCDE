namespace HCDE.Gamedata;

public sealed class VmOneLevelStats
{
    public int TotalKills { get; set; }
    public int KillCount { get; set; }
    public int TotalItems { get; set; }
    public int ItemCount { get; set; }
    public int TotalSecrets { get; set; }
    public int SecretCount { get; set; }
    public int LevelTime { get; set; }
    public string LevelName { get; set; } = "";
}

public static class VmStatisticsConfig
{
    public const int SaveStatisticsDefault = 0;
    public const string StatFileDefault = "zdoomstat.txt";
}
