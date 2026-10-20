namespace HCDE.Gamedata;

/// <summary>Additional MAPINFO scalar/media options from <c>g_mapinfo.cpp</c>.</summary>
public static class VmMapInfoMapOptionsScalars
{
    public static void ApplySkyMist(VmMapInfoLevelOptionsDraft draft, string mistPic, double speed, bool hexenHack)
    {
        draft.SkyMistPic = mistPic;
        draft.SkySpeed1 = hexenHack ? speed / 256 : speed;
    }

    public static double ClampSkyMistYScale(double value) => Math.Clamp(value, 0.002, 544.0);

    public static void ApplySkyBox(VmMapInfoLevelOptionsDraft draft, string sky)
    {
        draft.SkyPic1 = sky;
        draft.SkySpeed1 = 0;
    }

    public static void ApplyParTime(VmMapInfoLevelOptionsDraft draft, int tics) => draft.LevelNum = tics;

    public static int ClampWaveLimit(int value) => Math.Clamp(value, 0, 3000);

    public static int ClampDuelLimit(int value) => Math.Clamp(value, 0, 255);

    public static void ApplySuckTime(VmMapInfoLevelOptionsDraft draft, int tics) =>
        draft.LevelNum = tics;

    public static (string Music, int Order) ParseMusicPair(string music, int order) => (music, order);
}
