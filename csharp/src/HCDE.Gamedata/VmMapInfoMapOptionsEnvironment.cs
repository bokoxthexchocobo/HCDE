namespace HCDE.Gamedata;

/// <summary>MAPINFO environment options from <c>g_mapinfo.cpp</c>.</summary>
public static class VmMapInfoMapOptionsEnvironment
{
    public static void ApplyEvenLighting(VmMapInfoLevelOptionsDraft draft)
    {
        draft.SkySpeed1 = 0;
        draft.SkySpeed2 = 0;
    }

    public static int ParseCdIdHex(string text) =>
        int.TryParse(text, System.Globalization.NumberStyles.HexNumber, null, out var value) ? value : 0;

    public static int ClampWallShade(int raw) => Math.Clamp(raw / 2, -128, 127);

    public static double ApplyNoGravity() => double.MaxValue;

    public static void ApplyGravity(VmMapInfoLevelOptionsDraft draft, double gravity) =>
        draft.SkySpeed2 = gravity;

    public static void ApplyAirSupply(VmMapInfoLevelOptionsDraft draft, int tics) =>
        draft.LevelNum = tics;
}
