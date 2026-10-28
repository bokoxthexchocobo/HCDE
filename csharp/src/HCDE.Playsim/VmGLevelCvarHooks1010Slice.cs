namespace HCDE.Playsim;

/// <summary>
/// Level globals, save chunk IDs, and GL CVAR level propagation from <c>g_level.cpp</c> (**1–150**).
/// </summary>
public static class VmGLevelCvarHooks1010Slice
{
    public const int SnapId = 0x70416E73; // 'snAp'
    public const int DsnpId = 0x704E7364; // 'dsNp'
    public const int VistId = 0x74536976; // 'viSt'
    public const int AcsdId = 0x64536361; // 'acSd'
    public const int RclsId = 0x734C6372; // 'rcLs'
    public const int PclsId = 0x734C6370; // 'pcLs'

    public const int GlMapLightModeInactive = -1;
    public const int GlMapLightModeMax = 4;

    public const int GlLightModeDefault = 1;
    public const int GlLightModeMax = 2;

    public sealed class LevelInfoModel
    {
        public int BrightFog { get; set; } = -1;
        public int LightAdditiveSurfaces { get; set; } = -1;
        public int NoTextureFill { get; set; } = -1;
    }

    public sealed class LevelModel
    {
        public LevelInfoModel? Info { get; set; }
        public bool BrightFog { get; set; }
        public bool LightAdditiveSurfaces { get; set; }
        public bool NoTextureFill { get; set; }
    }

    public static bool ShouldApplyLevelInfoDefault(int infoField) =>
        infoField == -1;

    public static void ApplyBrightFogToLevel(LevelModel level, bool value)
    {
        if (level.Info == null || ShouldApplyLevelInfoDefault(level.Info.BrightFog))
            level.BrightFog = value;
    }

    public static void ApplyLightAdditiveSurfacesToLevel(LevelModel level, bool value)
    {
        if (level.Info == null || ShouldApplyLevelInfoDefault(level.Info.LightAdditiveSurfaces))
            level.LightAdditiveSurfaces = value;
    }

    public static void ApplyNoTextureFillToLevel(LevelModel level, bool value)
    {
        if (level.Info == null || ShouldApplyLevelInfoDefault(level.Info.NoTextureFill))
            level.NoTextureFill = value;
    }

    public static void PropagateBrightFog(IReadOnlyList<LevelModel> levels, bool value)
    {
        for (var i = 0; i < levels.Count; i++)
            ApplyBrightFogToLevel(levels[i], value);
    }

    public static void PropagateLightAdditiveSurfaces(IReadOnlyList<LevelModel> levels, bool value)
    {
        for (var i = 0; i < levels.Count; i++)
            ApplyLightAdditiveSurfacesToLevel(levels[i], value);
    }

    public static int ClampGlMapLightMode(int value)
    {
        if (value > GlMapLightModeMax || value < GlMapLightModeInactive)
            return GlMapLightModeInactive;
        return value;
    }

    public static int ClampGlLightMode(int value) =>
        value switch
        {
            < 0 => 1,
            > 2 => 1,
            _ => value,
        };
}
