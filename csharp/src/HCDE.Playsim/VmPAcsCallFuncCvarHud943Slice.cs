namespace HCDE.Playsim;

/// <summary>
/// ACS call-function CVar, line-attack, and sound HUD cluster from <c>p_acs.cpp</c> (**6000–6149**); chunk **943**.
/// </summary>
public static class VmPAcsCallFuncCvarHud943Slice
{
    public const int FhfNoRandomPuffZ = 1 << 0;
    public const int FhfNoImpactDecal = 1 << 1;
    public const int LafNoRandomPuffZ = 1 << 0;
    public const int LafNoImpactDecal = 1 << 1;
    public const int ChanBody = 5;
    public const int ChanfLocal = 1 << 8;
    public const int ChanfLoop = 1 << 9;
    public const int ChanfNostop = 1 << 10;

    public static bool MinArgCountMet(int argCount, int required) => argCount >= required;

    public static bool SetCVarUsesStringOperand(bool asString) => asString;

    public static int MapLineAttackFlags(int acsFlags)
    {
        var fh = 0;
        if ((acsFlags & FhfNoRandomPuffZ) != 0)
            fh |= LafNoRandomPuffZ;
        if ((acsFlags & FhfNoImpactDecal) != 0)
            fh |= LafNoImpactDecal;
        return fh;
    }

    public static bool GetUserCVarAbortsWhenPlayerUnresolved(bool resolved) => !resolved;

    public static int SelectGetUserCVarResult(bool resolved, int value) => resolved ? value : 0;

    public static bool LineAttackUsesActivator(int tid) => tid == 0;

    public static bool ShouldAssignPuffTid(object? puff, int puffTid) => puff != null && puffTid != 0;

    public static bool PlaySoundNeedsLookup(int funcIndex, int soundId) =>
        funcIndex != 0 || soundId != 0;

    public static object? ResolvePlaySoundSpot(int tid, object? activator, object? iteratorNext) =>
        tid == 0 ? activator : iteratorNext;

    public static int ComposePlayChannel(int baseChannel, bool local, bool looping)
    {
        var chan = baseChannel;
        if (local)
            chan |= ChanfLocal;
        if (looping)
            chan |= ChanfLoop | ChanfNostop;
        return chan;
    }

    public static int PlaySoundChannelMask(int composedChannel) => composedChannel & 7;

    public static bool HasValidSoundId(int soundId) => soundId != 0;

    public static bool StopSoundUsesActivator(int tid) => tid == 0;

    public static int DefaultStopChannel(int argCount, int channelArg) =>
        argCount > 1 ? channelArg : ChanBody;

    public readonly record struct UserCVarRead(bool AsString, int PlayerArgIndex, int NameArgIndex);

    public static UserCVarRead UserCVarStringRead() => new(true, 0, 1);

    public static UserCVarRead UserCVarNumericRead() => new(false, 0, 1);

    public static bool SetUserCVarRequiresThreeArgs(int argCount) => argCount >= 3;

    public static int LineAttackOptionalArg(int argCount, int index, int value, int defaultValue) =>
        argCount > index ? value : defaultValue;

    public static bool PlayActorSoundBypassesLookup(int funcIndex) => funcIndex != 0;
}
