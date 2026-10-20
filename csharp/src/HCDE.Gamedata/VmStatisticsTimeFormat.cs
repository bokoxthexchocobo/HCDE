namespace HCDE.Gamedata;

public static class VmStatisticsTimeFormat
{
    public const int TicRate = 35;

    public static int Hours(int tics) => tics / (60 * 60 * TicRate);
    public static int Minutes(int tics) => (tics % (60 * 60 * TicRate)) / (60 * TicRate);
    public static int Seconds(int tics) => (tics % (60 * TicRate)) / TicRate;
}
