namespace HCDE.Gamedata;

/// <summary>
/// <c>ParseMapDefinition</c> flag-handler <c>switch</c> body and registered-option fallback from <c>g_mapinfo.cpp</c> (**2127–2276**).
/// Chunk **1034**; follows **1033** (**1977–2126**).
/// </summary>
public static class VmMapInfoParseMapDefinition1034Slice
{
    public const int FmtNew = VmMapInfoParseMapInfo1029Slice.FmtNew;

    public static bool HandlerTypeIsIgnore(VmMapInfoFlagHandlerType type) =>
        type == VmMapInfoFlagHandlerType.Ignore;

    public static void ApplyClearFlag3(VmMapInfoLevelFlags level, uint clearMask, uint orMask)
    {
        level.Flags3 &= ~clearMask;
        level.Flags3 |= orMask;
    }

    public static void ApplyScFlags3(ref uint flags3, uint setMask, uint andMask) =>
        flags3 = (flags3 & andMask) | setMask;

    public static int ResolveCompatSetValue(
        int formatType,
        bool newFormatUsesCheckAssign,
        bool hasAssignFromCheck,
        int numberFromMustGet,
        bool oldFormatHadNumber,
        int numberFromCheckNumber)
    {
        if (formatType == FmtNew)
        {
            if (newFormatUsesCheckAssign && hasAssignFromCheck)
                return numberFromMustGet;
            return 1;
        }

        return oldFormatHadNumber ? numberFromCheckNumber : 1;
    }

    public static void ApplyCompatHandler(
        VmMapInfoLevelFlags level,
        uint data1,
        uint data2,
        bool set)
    {
        VmMapInfoFlagApply.ApplyCompat(level, data1, data2, set);
    }

    public static void DispatchMapFlagHandler(
        VmMapInfoLevelFlags level,
        VmMapInfoFlagHandlerType type,
        uint data1,
        uint data2,
        bool hasAssign,
        int assignNumber)
    {
        switch (type)
        {
            case VmMapInfoFlagHandlerType.Ignore:
                break;

            case VmMapInfoFlagHandlerType.SetFlag:
                VmMapInfoFlagApply.ApplySetFlag(level, data1, hasAssign, assignNumber, data2);
                break;

            case VmMapInfoFlagHandlerType.ClearFlag:
                VmMapInfoFlagApply.ApplyClearFlag(level, data1, data2);
                break;

            case VmMapInfoFlagHandlerType.SetScFlags:
            {
                var flags = level.Flags;
                VmMapInfoFlagApply.ApplyScFlags(ref flags, data1, data2);
                level.Flags = flags;
                break;
            }

            case VmMapInfoFlagHandlerType.SetFlag2:
                VmMapInfoFlagApply.ApplySetFlag2(level, data1, hasAssign, assignNumber, data2);
                break;

            case VmMapInfoFlagHandlerType.ClearFlag2:
                VmMapInfoFlagApply.ApplyClearFlag2(level, data1, data2);
                break;

            case VmMapInfoFlagHandlerType.SetScFlags2:
            {
                var flags2 = level.Flags2;
                VmMapInfoFlagApply.ApplyScFlags2(ref flags2, data1, data2);
                level.Flags2 = flags2;
                break;
            }

            case VmMapInfoFlagHandlerType.SetFlag3:
                VmMapInfoFlagApply.ApplySetFlag3(level, data1, hasAssign, assignNumber, data2);
                break;

            case VmMapInfoFlagHandlerType.ClearFlag3:
                ApplyClearFlag3(level, data1, data2);
                break;

            case VmMapInfoFlagHandlerType.SetScFlags3:
            {
                var flags3 = level.Flags3;
                ApplyScFlags3(ref flags3, data1, data2);
                level.Flags3 = flags3;
                break;
            }

            case VmMapInfoFlagHandlerType.ClearCompatFlag:
                VmMapInfoFlagApply.ApplyClearCompat(level, data1, data2);
                break;

            case VmMapInfoFlagHandlerType.CompatFlag:
                throw new InvalidOperationException("Use ApplyCompatHandler after ResolveCompatSetValue");
        }
    }

    public static bool ShouldFatalOnOldFormatOption(bool optionRequiresNewFormat, int formatType) =>
        optionRequiresNewFormat && formatType != FmtNew;

    public static bool TryDispatchRegisteredOption(
        string token,
        int formatType,
        IReadOnlyList<VmMapInfoOptionRegistration> options,
        out VmMapInfoOptionRegistration? matched)
    {
        if (!VmMapInfoOptionDispatch.TryDispatchOption(
                token,
                formatType == FmtNew,
                options,
                out matched))
        {
            return false;
        }

        if (ShouldFatalOnOldFormatOption(matched!.RequiresNewFormat, formatType))
            throw new InvalidOperationException(
                $"MAPINFO option '{token}' requires the new MAPINFO format");

        return true;
    }

    public static bool ShouldBreakMapBodyLoop(bool closeBraceParsed) => closeBraceParsed;

    public static bool ShouldEmitUnknownPropertyMessage(bool closeBraceParsed) => !closeBraceParsed;
}
