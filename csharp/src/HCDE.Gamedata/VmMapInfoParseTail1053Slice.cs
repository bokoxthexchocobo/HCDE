namespace HCDE.Gamedata;

/// <summary>
/// <c>level_info_t::Reset</c> tail, <c>LookupLevelName</c>, <c>CheckLevelRedirect</c>, and <c>isValid</c> from <c>g_mapinfo.cpp</c> (**301–450**).
/// Chunk **1053**; follows **1052** (**151–300**). Uncredited <see cref="VmMapInfoLookupLevelName"/> / <see cref="VmMapInfoLevelReset"/> — seam only.
/// </summary>
public static class VmMapInfoParseTail1053Slice
{
    public const uint LevelLookupLevelNameFlag = 0x04000000;
    public const int NameNone = VmMapInfoParseMid1052Slice.NameNone;
    public const uint HazardColorDefault = 0xff004200;
    public const uint HazardFlashDefault = 0xff00ff00;
    public const int UnsetMapinfoToggle = -1;

    public static void ApplyLevelInfoResetTail(VmMapInfoLevelResetTail1053 draft)
    {
        draft.InterMusic = string.Empty;
        draft.InterMusicOrder = 0;
        draft.SoundInfo = string.Empty;
        draft.SndSeq = string.Empty;
        draft.BorderTexture = string.Empty;
        draft.TeamDamage = 0;
        draft.InvasionWaveLimit = 0;
        draft.DuelLimit = 0;
        draft.HazardColor = HazardColorDefault;
        draft.HazardFlash = HazardFlashDefault;
        draft.FogDensity = 0;
        draft.OutsideFogDensity = 0;
        draft.SkyFog = 0;
        draft.ThickFogDistance = -1f;
        draft.ThickFogMultiplier = 30f;
        draft.PixelStretch = 1.2f;
        draft.SpecialActionsCleared = true;
        draft.DefaultEnvironment = 0;
        draft.EnvironmentCleared = true;
        draft.PrecacheSoundsCleared = true;
        draft.BrightFog = UnsetMapinfoToggle;
        draft.LightMode = VmMapInfoLightMode.NotSet; // native ELightMode::NotSet
        draft.NoTextureFill = UnsetMapinfoToggle;
        draft.LightAdditiveSurfaces = UnsetMapinfoToggle;
        draft.HcdeShadowProfileOverride = UnsetMapinfoToggle;
        draft.HcdeShadowQualityCap = UnsetMapinfoToggle;
        draft.HcdeShadowMaxLightsCap = UnsetMapinfoToggle;
        draft.SkyRotate = VmMapInfoSkyRotateVector.UnitZ;
        draft.SkyRotate2 = VmMapInfoSkyRotateVector.UnitZ;
        draft.LightBlendMode = VmMapInfoLightBlendMode.Default;
        draft.Tonemap = VmMapInfoTonemapMode.None;
    }

    public static string ResolveLookupLevelName(
        uint levelFlags,
        string levelName,
        string mapName,
        Func<string, string?> checkString)
    {
        if ((levelFlags & LevelLookupLevelNameFlag) == 0)
            return levelName;

        var lookedUp = checkString(levelName);
        if (lookedUp == null)
            return levelName;

        var prefix = BuildLocalizedHeaderPrefix(mapName);
        if (prefix.Length == 0)
            return lookedUp;

        var idx = lookedUp.IndexOf(prefix, StringComparison.Ordinal);
        return idx < 0 ? lookedUp : lookedUp[(idx + prefix.Length)..];
    }

    public static string BuildLocalizedHeaderPrefix(string mapName)
    {
        if (mapName.Length > 3 && mapName[0] == 'E' && mapName[2] == 'M')
            return $"{mapName}: ";
        if (mapName.Length > 3
            && mapName.StartsWith("MAP", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(mapName.AsSpan(3), out var mapNum))
            return $"{mapNum}: ";
        if (mapName.Length > 5
            && mapName.StartsWith("LEVEL", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(mapName.AsSpan(5), out var levelNum))
            return $"{levelNum}: ";
        return string.Empty;
    }

    public static bool ShouldEvaluateInventoryRedirect(int redirectTypeName) =>
        redirectTypeName != NameNone;

    public static bool ShouldEvaluateCvarRedirect(int redirectCvarName, bool isBoolCvar, bool cvarIgnored) =>
        redirectCvarName != NameNone && isBoolCvar && !cvarIgnored;

    public static bool UsesPerPlayerCvarRedirect(uint cvarFlags, uint cvarUserInfoFlag) =>
        (cvarFlags & cvarUserInfoFlag) != 0;

    public static bool TryResolveRedirectWhenMapExists(bool mapDataExists, string redirectMapName, out string targetMap)
    {
        targetMap = redirectMapName;
        return mapDataExists;
    }

    public static bool IsLevelInfoValid(string mapName, bool isDefaultLevelRecord) =>
        mapName.Length > 0 || isDefaultLevelRecord;
}

public readonly struct VmMapInfoSkyRotateVector(float x, float y, float z)
{
    public float X { get; } = x;
    public float Y { get; } = y;
    public float Z { get; } = z;

    public static VmMapInfoSkyRotateVector UnitZ => new(0, 0, 1);
}

public sealed class VmMapInfoLevelResetTail1053
{
    public string InterMusic { get; set; } = "";
    public int InterMusicOrder { get; set; }
    public string SoundInfo { get; set; } = "";
    public string SndSeq { get; set; } = "";
    public string BorderTexture { get; set; } = "";
    public float TeamDamage { get; set; }
    public int InvasionWaveLimit { get; set; }
    public int DuelLimit { get; set; }
    public uint HazardColor { get; set; }
    public uint HazardFlash { get; set; }
    public int FogDensity { get; set; }
    public int OutsideFogDensity { get; set; }
    public int SkyFog { get; set; }
    public float ThickFogDistance { get; set; }
    public float ThickFogMultiplier { get; set; }
    public float PixelStretch { get; set; }
    public bool SpecialActionsCleared { get; set; }
    public int DefaultEnvironment { get; set; }
    public bool EnvironmentCleared { get; set; }
    public bool PrecacheSoundsCleared { get; set; }
    public int BrightFog { get; set; }
    public VmMapInfoLightMode LightMode { get; set; }
    public int NoTextureFill { get; set; }
    public int LightAdditiveSurfaces { get; set; }
    public int HcdeShadowProfileOverride { get; set; }
    public int HcdeShadowQualityCap { get; set; }
    public int HcdeShadowMaxLightsCap { get; set; }
    public VmMapInfoSkyRotateVector SkyRotate { get; set; }
    public VmMapInfoSkyRotateVector SkyRotate2 { get; set; }
    public VmMapInfoLightBlendMode LightBlendMode { get; set; }
    public VmMapInfoTonemapMode Tonemap { get; set; }
}
