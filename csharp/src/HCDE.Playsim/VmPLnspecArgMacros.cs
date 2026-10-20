namespace HCDE.Playsim;

/// <summary>Native <c>SPEED</c>/<c>CHANGE</c>/<c>CRUSH</c> helpers from <c>p_lnspec.cpp</c> (60–80).</summary>
public static class VmPLnspecArgMacros
{
    private static readonly byte[] ChangeMap = { 0, 1, 5, 3, 7, 2, 6, 0 };

    public static double Speed(int raw) => raw / 8.0;

    public static int Crush(int raw) => raw > 0 ? raw : -1;

    public static int Change(int raw) =>
        raw >= 0 && raw <= 7 ? ChangeMap[raw] : 0;

    public static bool CrushTypeDoomHexen(int mode, int gameHexen) =>
        mode == 1 ? false : mode == 2 ? true : gameHexen == 2;

    public static int CeilingCrushStopRemove(int arg1, int gameHexen) =>
        arg1 switch
        {
            1 => 0,
            2 => 1,
            _ => gameHexen == 2 ? 1 : 0
        };

    public static bool CeilingCrushStopShouldRemove(int arg1, int gameHexen) =>
        CeilingCrushStopRemove(arg1, gameHexen) != 0;

    public const int TicRate = 35;

    public static int Tics(int delay) => delay * TicRate / 8;

    public static int PlatTics(int delay) => delay * TicRate / 35;

    public static int Octics(int delay) => delay * TicRate / 8;

    public static int AcsBacksideFlag(bool backSide) => backSide ? 1 : 0;

    public static int AcsAlwaysFlag => 2;

    public static int AcsWantResultFlag => 4;
}
