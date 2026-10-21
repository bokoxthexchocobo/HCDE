namespace HCDE.Playsim;

/// <summary>Inventory damage/sprite walks, active/push sounds, blood type, and portal transitions from <c>p_mobj.cpp</c> (3904–3962, 4203–4306).</summary>
public static class VmPMobjInventorySoundPortal
{
    public const int Mf3Fullvolactive = 1;
    public const int ChanVoice = 2;
    public const int ChanBody = 4;
    public const int AttnNone = 0;
    public const int AttnIdle = 1;
    public const int AttnNorm = 1;

    public static bool InventoryWalkContinues(int index, int count) => index >= 0 && index < count;

    public static int ReverseInventoryIndex(int forwardIndex, int count) => count - 1 - forwardIndex;

public static bool ActiveChannelFree(bool playingVoice) => !playingVoice;

    public static float ActiveAttenuation(int flags3) =>
        (flags3 & Mf3Fullvolactive) != 0 ? AttnNone : AttnIdle;

    public static bool PushSoundNotPlaying(bool playingBody) => !playingBody;

public static bool CeilingPortalOpen(bool blocksMovement) => !blocksMovement;

    public static bool TestZAboveCeilingPortal(double testZ, double portalZ) => testZ >= portalZ;

    public static bool FloorPortalOpen(bool blocksMovement) => !blocksMovement;

    public static bool TestZBelowFloorPortal(double testZ, double portalZ) => testZ < portalZ;

public readonly record struct PlayActiveSoundContext(
        bool SoundValid,
        bool VoiceChannelBusy,
        int Flags3);

    public static bool EvaluatePlayActiveSound(PlayActiveSoundContext ctx) =>
        ActiveSoundValid(ctx.SoundValid) && ActiveChannelFree(ctx.VoiceChannelBusy);

    public readonly record struct PlayPushSoundContext(bool BodyChannelBusy);

    public static bool EvaluatePlayPushSound(PlayPushSoundContext ctx) =>
        PushSoundNotPlaying(ctx.BodyChannelBusy);

    public readonly record struct PortalTransitionProbeContext(
        double TestZ,
        double ByOffset,
        bool CeilingBlocks,
        double CeilingPortalZ,
        bool FloorBlocks,
        double FloorPortalZ,
        bool MovedCeiling);

    public static PortalTransitionResult EvaluateGetPortalTransition(PortalTransitionProbeContext ctx)
    {
        var testZ = ctx.TestZ + ctx.ByOffset;
        var moved = ctx.MovedCeiling;
        if (CeilingPortalOpen(ctx.CeilingBlocks))
        {
            while (CeilingPortalOpen(ctx.CeilingBlocks)
                   && TestZAboveCeilingPortal(testZ, ctx.CeilingPortalZ))
            {
                moved = true;
                break;
            }
        }
        if (!moved && FloorPortalOpen(ctx.FloorBlocks)
            && TestZBelowFloorPortal(testZ, ctx.FloorPortalZ))
            moved = true;
        return new PortalTransitionResult(moved, testZ);
    }

    public readonly record struct PortalTransitionResult(bool Moved, double TestZ);

    public readonly record struct CheckPortalTransitionContext(
        double Z,
        double CeilingPortalZ,
        double FloorPortalZ,
        double FloorZ,
        bool CeilingBlocks,
        bool FloorBlocks,
        bool IsLinked,
        bool AlreadyMoved);

    public static bool EvaluateCeilingPortalStep(CheckPortalTransitionContext ctx) =>
        CeilingPortalOpen(ctx.CeilingBlocks) && ctx.Z >= ctx.CeilingPortalZ;

    public static bool EvaluateFloorPortalStep(CheckPortalTransitionContext ctx) =>
        FloorPortalOpen(ctx.FloorBlocks)
        && ctx.Z < ctx.FloorPortalZ
        && ctx.FloorZ < ctx.FloorPortalZ;

    public static bool ShouldRelink(bool isLinked, bool moved) => isLinked && moved;

    public static int EvaluateModelTimer(bool clientSide, int localTimer, int totalTime) =>
        ClientSideModelTimer(clientSide) ? LocalWorldTimer(localTimer) : AuthorityTotalTime(totalTime);

    public readonly record struct InventoryChainContext(int ItemCount, int ForwardIndex);

    public static IEnumerable<int> ReverseInventoryOrder(int itemCount)
    {
        for (var i = itemCount - 1; i >= 0; i--)
            yield return i;
    }

    public static bool EvaluateAlterWeaponWalk(InventoryChainContext ctx, int reverseIndex) =>
        InventoryWalkContinues(reverseIndex, ctx.ItemCount)
        && ReverseInventoryIndex(reverseIndex, ctx.ItemCount) >= 0;
}
