namespace HCDE.Net.Core;

/// <summary><c>HCDEAppendLegacyRunArgs</c> and <c>HCDEAppendLegacyEventPayload</c> from <c>d_net.cpp</c> (4111–4299).</summary>
public static class VmDNetLegacyEventPayload
{
    public const int DemSuicide = 15;
    public const int DemKillbots = 17;
    public const int DemInvuseall = 18;
    public const int DemPause = 20;
    public const int DemCenterView = 36;
    public const int DemCrouch = 39;
    public const int DemCheckautosave = 41;
    public const int DemDoautosave = 42;
    public const int DemConvclose = 60;
    public const int DemConvnull = 61;
    public const int DemRunspecial = 62;
    public const int DemRunnamedscript = 65;
    public const int DemRevertcamera = 66;
    public const int DemSetslotpnum = 67;
    public const int DemFinishgame = 69;
    public const int DemEndscreenjob = 73;
    public const int DemReadied = 77;
    public const int DemUseflechette = 79;
    public const int DemSinfchanged = 9;
    public const int DemSinfchangedxor = 33;
    public const int DemRunscript = 32;
    public const int DemRunscript2 = 40;
    public const int DemSetslot = 57;

    public static bool IsZeroPayloadEvent(int eventType) => eventType switch
    {
        DemSuicide or DemKillbots or DemInvuseall or DemPause or DemCenterView or DemCrouch
            or DemCheckautosave or DemDoautosave or DemConvclose or DemConvnull or DemRevertcamera
            or DemFinishgame or DemEndscreenjob or DemReadied or DemUseflechette => true,
        _ => false,
    };

    public static bool TryPlanLegacyRunArgs(bool named, out int leadingFieldBytes, out bool maskArgCountToSevenBits)
    {
        if (named)
        {
            leadingFieldBytes = 0;
            maskArgCountToSevenBits = true;
            return true;
        }

        leadingFieldBytes = 2;
        maskArgCountToSevenBits = false;
        return true;
    }

    public static int MaskRunArgCount(int raw, bool named) => named ? raw & 127 : raw;

    public static int RunArgPayloadBytes(int argCount) => argCount * 4;

    public static bool TryReadSetSlotHeader(ReadOnlySpan<byte> input, ref int cursor, out byte slot, out byte count)
    {
        slot = 0;
        count = 0;
        if (cursor + 2 > input.Length)
            return false;
        slot = input[cursor++];
        count = input[cursor++];
        return true;
    }

    public static bool SetSlotNeedsPnumPrefix(int eventType) => eventType == DemSetslotpnum;

    public static int SetSlotLeadingFieldBytes(int eventType) => SetSlotNeedsPnumPrefix(eventType) ? 1 : 0;

    public static int GiveTakeCheatTrailingBytes => 4;

    public static int SetInvTrailingBytes => 5;

    public static int NetEventTrailingBytes => 14;

    public static int Summon2TrailingBytes => 25;

    public static int WarpCheatFieldBytes => 6;

    public static int ConvReplyFieldBytes => 3;

    public static int ChangeMap2LeadingBytes => 1;

    public static bool UsesLegacyCVarChange(int eventType) =>
        eventType == DemSinfchanged || eventType == DemSinfchangedxor;

    public static bool UsesRunArgs(int eventType) =>
        eventType is DemRunscript or DemRunscript2 or DemRunspecial;

    public static bool UsesNamedRunArgs(int eventType) => eventType == DemRunnamedscript;
}
