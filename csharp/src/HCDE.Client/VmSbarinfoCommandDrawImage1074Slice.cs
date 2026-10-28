namespace HCDE.Client;

/// <summary>
/// <c>CommandDrawImage</c> parse tail, <c>Tick</c> texture policy, and <c>GetIcon</c> scale head from <c>sbarinfo_commands.cpp</c> (**151–300**).
/// Chunk **1074**; follows **1073** parse head (**1–150**). Draw-in-box / alternate-on-fail flags align with <see cref="VmSbarinfoCommandDrawImage1073Slice"/>.
/// </summary>
public static class VmSbarinfoCommandDrawImage1074Slice
{
    public const int OffsetCenter = 0x2 | 0x40;
    public const int OffsetCenterBottom = 0x4 | 0x40;
    public const int DefaultCommandAlpha = 1;

    public const uint DiSkipIcon = 0x1;
    public const uint DiSkipAltIcon = 0x2;
    public const uint DiSkipSpawn = 0x4;
    public const uint DiSkipReady = 0x8;
    public const uint DiAltIconFirst = 0x10;

    public static (int MaxWidth, uint Flags) ApplyParsedMaxWidth(int parsedNumber, uint flags)
    {
        if (parsedNumber > 0)
            return (parsedNumber, flags | VmSbarinfoCommandDrawImage1073Slice.DiDrawInBox);
        return (VmSbarinfoCommandDrawImage1073Slice.ForceDimensionUnset, flags);
    }

    public static (int MaxHeight, uint Flags) ApplyParsedMaxHeight(int parsedNumber, uint flags)
    {
        if (parsedNumber > 0)
            return (parsedNumber, flags | VmSbarinfoCommandDrawImage1073Slice.DiDrawInBox);
        return (VmSbarinfoCommandDrawImage1073Slice.ForceDimensionUnset, flags);
    }

    public static (bool Recognized, int Offset, bool InvalidKeyword) TryResolveDrawImageAlignment(string identifier)
    {
        if (identifier.Equals("center", StringComparison.OrdinalIgnoreCase))
            return (true, OffsetCenter, false);
        if (identifier.Equals("centerbottom", StringComparison.OrdinalIgnoreCase))
            return (true, OffsetCenterBottom, false);
        return (false, 0, true);
    }

    public static (uint Flags, bool UnknownKeyword) ApplyDrawImageParseFlag(string identifier, uint flags)
    {
        if (identifier.Equals("skipicon", StringComparison.OrdinalIgnoreCase))
            return (flags | DiSkipIcon, false);
        if (identifier.Equals("skipalticon", StringComparison.OrdinalIgnoreCase))
            return (flags | DiSkipAltIcon, false);
        if (identifier.Equals("skipspawn", StringComparison.OrdinalIgnoreCase))
            return (flags | DiSkipSpawn, false);
        if (identifier.Equals("skipready", StringComparison.OrdinalIgnoreCase))
            return (flags | DiSkipReady, false);
        if (identifier.Equals("alticonfirst", StringComparison.OrdinalIgnoreCase))
            return (flags | DiAltIconFirst, false);
        if (identifier.Equals("forcescale", StringComparison.OrdinalIgnoreCase))
        {
            if ((flags & VmSbarinfoCommandDrawImage1073Slice.DiDrawInBox) != 0)
                return (flags | VmSbarinfoCommandDrawImage1073Slice.DiForceScale, false);
            return (flags, false);
        }
        if (identifier.Equals("alternateonfail", StringComparison.OrdinalIgnoreCase))
            return (flags | VmSbarinfoCommandDrawImage1073Slice.DiAlternateOnFail, false);
        return (flags, true);
    }

    public static bool ShouldContinueDrawImageFlagList(bool sawPipeSeparator, bool sawCommaSeparator) =>
        sawPipeSeparator || sawCommaSeparator;

    public static bool ShouldDelegateParseToFlowControl(uint flags) =>
        (flags & VmSbarinfoCommandDrawImage1073Slice.DiAlternateOnFail) != 0;

    public static (double Alpha, bool ApplyScale, double SpawnScaleX, double SpawnScaleY) ResetTickDrawImageState(
        bool hadApplyScale,
        double priorSpawnScaleX,
        double priorSpawnScaleY)
    {
        if (!hadApplyScale)
            return (DefaultCommandAlpha, false, priorSpawnScaleX, priorSpawnScaleY);
        return (DefaultCommandAlpha, false, 1.0, 1.0);
    }

    public static bool ShouldResolveArmorTickIcon(bool armorPresent, int armorAmount) =>
        armorPresent && armorAmount != 0;

    public static int ResolveHexenArmorSlotIndex(VmSbarinfoCommandDrawImage1073Slice.DrawImageType imageType) =>
        (int)imageType - (int)VmSbarinfoCommandDrawImage1073Slice.DrawImageType.HexenArmorArmor;

    public static bool ShouldAbortHexenArmorTick(double slotFill, double slotIncrement) =>
        slotFill <= 0 || slotIncrement <= 0;

    public static double CombineHexenArmorFillAlpha(double commandAlpha, double slotFill, double slotIncrement) =>
        commandAlpha * Math.Min(1.0, slotFill / slotIncrement);

    public static bool ResolveAlternateOnFailTruth(bool textureMissing, bool textureInvalid) =>
        textureMissing || textureInvalid;

    public static (bool ApplyScale, double SpawnScaleX, double SpawnScaleY) ApplyInventoryIconSpawnScale(
        bool applyScaleFromIcon,
        double itemScaleX,
        double itemScaleY) =>
        applyScaleFromIcon ? (true, itemScaleX, itemScaleY) : (false, 1.0, 1.0);
}
