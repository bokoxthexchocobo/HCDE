namespace HCDE.Playsim;

/// <summary>
/// Map CVAR defaults, <c>P_ShouldPassThroughPlayer</c>, and <c>P_CanCollideWith</c> head from <c>p_map.cpp</c> (**1–150**).
/// Exclusive to chunk **961**; injected seams on later map chunks remain uncredited here.
/// </summary>
public static class VmPMapFileHead961Slice
{
    public const int Df3NoPlayerClip = 1 << 0;
    public const uint UnresolvedVirtualIndex = uint.MaxValue;

    public const bool DefaultClBloodSplats = true;
    public const int DefaultSvSmartAim = 0;
    public const bool DefaultClDoAutoaim = false;
    public const bool DefaultSvAutocompat = true;

    public static bool NoPlayerClipEnabled(int dmflags3) => (dmflags3 & Df3NoPlayerClip) != 0;

    public static bool OtherActorIsControllingPlayer(bool hasPlayer, bool playerMoMatchesSelf) =>
        hasPlayer && playerMoMatchesSelf;

    public static bool ShouldPassThroughPlayer(
        int dmflags3,
        bool otherIsControllingPlayer,
        bool selfIsFriendOfOther) =>
        NoPlayerClipEnabled(dmflags3) && otherIsControllingPlayer && selfIsFriendOfOther;

    public static uint ResolveCanCollideVirtualIndex(uint cachedIndex, uint resolvedIndex) =>
        cachedIndex == UnresolvedVirtualIndex ? resolvedIndex : cachedIndex;

    public static bool VirtualIndexReady(uint index) => index != UnresolvedVirtualIndex;

    public static bool ActorVirtualTableLargeEnough(int virtualCount, uint index) =>
        virtualCount > index;

    public static bool VmCollideCallBlocked(int retval) => retval == 0;

    public static bool CollidePairAllowed(int firstActorResult, int secondActorResult) =>
        firstActorResult != 0 && secondActorResult != 0;

    public static bool ReverseCollideMateParam(bool firstPass) => !firstPass;

    public static bool ShouldRunSecondCanCollideVirtual(bool firstBlocked) => !firstBlocked;

    public static bool EvaluateFirstCanCollideVirtual(
        int virtualsSize,
        uint virtualIndex,
        bool hasVirtualFunction,
        int vmReturnValue,
        out bool blocked)
    {
        blocked = false;
        if (!hasVirtualFunction || !ActorVirtualTableLargeEnough(virtualsSize, virtualIndex))
            return false;
        blocked = VmCollideCallBlocked(vmReturnValue);
        return true;
    }

    public static bool EvaluateSecondCanCollideVirtual(
        int virtualsSize,
        uint virtualIndex,
        bool hasVirtualFunction,
        int vmReturnValue,
        bool mateParam,
        out bool blocked)
    {
        blocked = false;
        if (!mateParam || !hasVirtualFunction || !ActorVirtualTableLargeEnough(virtualsSize, virtualIndex))
            return false;
        blocked = VmCollideCallBlocked(vmReturnValue);
        return true;
    }

    public static bool RecordSpecialLineHit(int special) => special != 0;

    public static bool RecordPortalLineHit(bool isPortal) => isPortal;

    public static void AppendSpechitWhenNeeded(
        int special,
        bool isPortal,
        Action recordSpechit,
        Action recordPortalhit)
    {
        if (RecordSpecialLineHit(special))
            recordSpechit();
        if (RecordPortalLineHit(isPortal))
            recordPortalhit();
    }
}
