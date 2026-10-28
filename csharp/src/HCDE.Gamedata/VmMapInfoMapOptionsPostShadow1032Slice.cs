namespace HCDE.Gamedata;

/// <summary>
/// MAPINFO toggles, skyrotate, script lumps, and flag-handler table head from <c>g_mapinfo.cpp</c> (**1827–1976**).
/// Chunk **1032**; primary **3050–3199** aborted (file EOF **3151**, **3067–3114** `VmMapInfoParseMapInfoLoop` off-tally).
/// </summary>
public static class VmMapInfoMapOptionsPostShadow1032Slice
{
    public const uint LevelNoIntermission = 1u << 0;
    public const uint LevelDoubleSky = 1u << 1;
    public const uint Level3E1M8Special = 1u << 2;

    public enum MapInfoFlagHandlerKind
    {
        Ignore,
        EatNext,
        SetFlag,
        ClearFlag,
        SetScFlags,
        SetFlag2,
        ClearFlag2,
        SetScFlags2,
        SetFlag3,
        ClearFlag3,
        SetScFlags3,
        CompatFlag,
        ClearCompatFlag
    }

    public sealed class LevelToggleDraft
    {
        public bool NoTextureFill { get; set; }
        public bool LightAdditiveSurfaces { get; set; }
        public bool FsNoCheckPosition { get; set; }
    }

    public static bool ParseOptionalBoolFlag(bool hasAssign, int numberValue, bool whenOmitted) =>
        VmMapInfoToggleOptions.ParseOptionalBool(hasAssign, numberValue, whenOmitted);

    public static void ApplyNoTextureFill(LevelToggleDraft draft, bool value) => draft.NoTextureFill = value;

    public static void ApplyLightAdditiveSurfaces(LevelToggleDraft draft, bool value) =>
        draft.LightAdditiveSurfaces = value;

    public static void ApplyFsNoCheckPosition(LevelToggleDraft draft, bool value) =>
        draft.FsNoCheckPosition = value;

    public static bool TryParseSkyRotateVector(
        IReadOnlyList<string> tokens,
        ref int index,
        out VmVec3 vector) =>
        VmMapInfoSkyRotateParser.TryParseVector(tokens, ref index, out vector);

    public static void ApplyScriptLumps(VmMapInfoLevelInfo info, string? edName, string? acsName, string? intro, string? outro)
    {
        if (edName is not null)
            VmMapInfoMapOptionsScript.ApplyEdName(info, edName);
        if (acsName is not null)
            VmMapInfoMapOptionsScript.ApplyAcsName(info, acsName);
        if (intro is not null)
            VmMapInfoMapOptionsScript.ApplyIntroCutscene(info, intro);
        if (outro is not null)
            VmMapInfoMapOptionsScript.ApplyOutroCutscene(info, outro);
    }

    public static MapInfoFlagHandlerKind ResolveHandlerKind(string nativeTypeToken)
    {
        return nativeTypeToken switch
        {
            "MITYPE_IGNORE" => MapInfoFlagHandlerKind.Ignore,
            "MITYPE_SETFLAG" => MapInfoFlagHandlerKind.SetFlag,
            "MITYPE_CLRFLAG" => MapInfoFlagHandlerKind.ClearFlag,
            "MITYPE_SETFLAG3" => MapInfoFlagHandlerKind.SetFlag3,
            "MITYPE_SCFLAGS" => MapInfoFlagHandlerKind.SetScFlags,
            "MITYPE_SETFLAG2" => MapInfoFlagHandlerKind.SetFlag2,
            _ => MapInfoFlagHandlerKind.Ignore
        };
    }

    public static bool ShouldIgnoreFlagHandler(MapInfoFlagHandlerKind kind) =>
        kind == MapInfoFlagHandlerKind.Ignore;

    public static uint ResolveSetFlagMask(MapInfoFlagHandlerKind kind, uint data1) =>
        kind switch
        {
            MapInfoFlagHandlerKind.SetFlag => data1,
            MapInfoFlagHandlerKind.SetFlag3 => data1,
            MapInfoFlagHandlerKind.SetFlag2 => data1,
            _ => 0
        };

    public static bool MatchesFlagHandlerKeyword(string keyword, string expected) =>
        keyword.Equals(expected, StringComparison.OrdinalIgnoreCase);
}
