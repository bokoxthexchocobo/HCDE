namespace HCDE.Playsim;

/// <summary>
/// <c>SF_LevelNum</c> tail through <c>SF_SetColor</c> sector loop from <c>fragglescript/t_func.cpp</c> (**3545–3694**); chunk **940**.
/// </summary>
public static class VmFScriptParserMobjLevel940Slice
{
    public static bool MobjGeomRequiresOneArg(int argc) => argc >= 1;

    public static bool MobjGeomAssignsWhenPresent(int argc, object? actor) =>
        argc > 1 && actor != null;

    public static double MobjGeomReturnDouble(double value, object? actor) =>
        actor != null ? value : 0.0;

    public static bool ThingCountCountsMapActorsOnly(int argc, int livingModeArg, bool classIsInventory) =>
        argc < 2 || livingModeArg == 0 || classIsInventory;

    public static bool ThingCountCountsLiving(int argc, int livingModeArg, bool classIsInventory) =>
        !ThingCountCountsMapActorsOnly(argc, livingModeArg, classIsInventory);

    public static bool ThingCountActorQualifies(bool mapActorsOnly, bool isMapActor, int health) =>
        mapActorsOnly ? isMapActor : health > 0;

    public static bool ThingCountShouldRepeatForReplacement(bool alreadyRepeated, bool replacementDiffers) =>
        !alreadyRepeated && replacementDiffers;

    public static bool ThingCountRequiresOneArg(int argc) => argc >= 1;

    public static bool ThingCountAbortsWithoutClass(object? resolvedClass) => resolvedClass is null;

    public static int CombineThingCountPasses(int primary, int replacementPass, bool ranReplacement)
    {
        if (!ranReplacement)
            return primary;
        return primary + replacementPass;
    }

    public static bool SetColorRequiresTwoArgs(int argc) => argc >= 2;

    public static bool SetColorSectorTagMissing(int firstSectorIndex) => firstSectorIndex < 0;

    public enum SetColorEncoding
    {
        Invalid,
        PackedInt,
        RgbComponents,
    }

    public static SetColorEncoding SetColorEncodingFromArgc(int argc) => argc switch
    {
        > 1 and < 4 => SetColorEncoding.PackedInt,
        >= 4 => SetColorEncoding.RgbComponents,
        _ => SetColorEncoding.Invalid,
    };

    public static int ComposePalEntryRgb(int r, int g, int b) => r | (g << 8) | (b << 16);

    public static int SetColorRgbWithClearedAlpha(int r, int g, int b) =>
        ComposePalEntryRgb(r, g, b) & 0xFFFFFF;

    public readonly record struct SetColorApply(int Tag, int Color);

    public static SetColorApply? BuildSetColorApply(int argc, int tag, int firstSector, int packedOrR, int g, int b)
    {
        if (!SetColorRequiresTwoArgs(argc) || SetColorSectorTagMissing(firstSector))
            return null;
        return SetColorEncodingFromArgc(argc) switch
        {
            SetColorEncoding.PackedInt => new(tag, packedOrR),
            SetColorEncoding.RgbComponents => new(tag, SetColorRgbWithClearedAlpha(packedOrR, g, b)),
            _ => null,
        };
    }

    public static bool SectorTagIteratorActive(int index) => index >= 0;
}
