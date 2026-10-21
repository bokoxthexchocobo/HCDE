namespace HCDE.Net.Core;

/// <summary><c>TryRunTics</c> zero-runTics stall, prediction fault, and lag path (<c>d_net.cpp</c> 7831–7980).</summary>
public static class VmDNetTryRunTicsStall
{
    public static bool ShouldSuppressTicGateStallWarningNative(
        bool predictionLeadHeldWorld,
        bool netgame,
        bool demoPlayback,
        int levelStartStatus,
        int levelStartDelay,
        bool dedicatedClient,
        int availableTics,
        int predictionLead,
        int clientTic,
        int gameTic,
        int ticDup) =>
        predictionLeadHeldWorld
        || (netgame && !demoPlayback && (levelStartStatus != 0 || levelStartDelay > 0))
        || (netgame && !demoPlayback && !dedicatedClient && availableTics <= 0
            && Math.Clamp(predictionLead, 0, 8) > 0
            && (clientTic / Math.Max(ticDup, 1) - gameTic / Math.Max(ticDup, 1))
                <= Math.Clamp(predictionLead, 0, 8) + 1);

    public static bool IsSustainedDedicatedTicGateStallNative(int availableTics, int commandBacklog, int enterTic, int lastGameUpdate, int ticRate, int ticDup) =>
        availableTics < 0
        || (availableTics <= 0 && commandBacklog > ticDup * 3)
        || (availableTics <= 0 && enterTic - lastGameUpdate > ticRate / 2);

    public static bool ShouldRunTicStabilityWaitNative(int totalTics) => totalTics <= 0;

    public static int SelectLagStateWhenNoRunTicsNative(int totalTics) => totalTics <= 0 ? 1 : 0;

    public static bool ShouldAdvanceCommandPredictionNative(int clientTic, int startCommand) => clientTic > startCommand;

    public static bool IsStaleAuthorityGateNative(int availableTics, int commandBacklog, int enterTic, int lastGameUpdate, int ticRate, int ticDup) =>
        availableTics <= 0 && (commandBacklog > ticDup * 4 || enterTic - lastGameUpdate > ticRate / 2);

    public static bool ShouldReportPredictionFaultNative(
        bool netgame,
        int totalTics,
        bool invasionAuthority,
        int enterTic,
        int pauseGraceUntil,
        bool staleAuthorityGate,
        bool wantsAttack,
        bool wantsMove,
        bool compatPredicting) =>
        netgame && totalTics > 0 && !invasionAuthority && enterTic >= pauseGraceUntil
        && staleAuthorityGate && (wantsAttack || wantsMove || compatPredicting);

    public static bool ShouldRefreshLagHudOnStallNative(int runTics, int totalTics) => runTics <= 0 && totalTics > 0;

    public static bool IsTicGateStalledNative(int runTics, int totalTics, int availableTics, int clientTic, int gameTic, int ticDup, int enterTic, int lastGameUpdate, int ticRate) =>
        runTics <= 0 && totalTics > 0
        && (availableTics < 0
            || (availableTics <= 0 && clientTic - gameTic > ticDup * 3)
            || (availableTics <= 0 && enterTic - lastGameUpdate > ticRate / 2));

    public static bool ShouldRunClientSideLogicWhenIdleNative(int totalTics) => totalTics > 0;

    public static bool ShouldVerifyPredictedEntitiesNative(int totalTics) => totalTics > 0;

    public static bool ShouldUnpredictOnBacklogNative(int availableTics, int commandBacklog, int unpredictLead, int ticDup) =>
        availableTics <= 0 && commandBacklog > (Math.Max(unpredictLead, 0) + 2) * Math.Max(ticDup, 1);

    public static int SelectLagStateOnBacklogNative(bool unpredict) => unpredict ? 1 : 2;

    public static bool ShouldTickStalledCutsceneNative(bool ticGateStalled) => ticGateStalled;

    public static bool ShouldLogDedicatedIdleHudNative(bool hcdeHudDebug, int totalTics, int availableTics) =>
        hcdeHudDebug && totalTics > 0 && availableTics <= 0;

    public static bool ShouldMirrorVisualWhilePredictingNative(bool netgame, int totalTics, bool invasionAuthority) =>
        netgame && totalTics > 0 && !invasionAuthority;

    public static bool ShouldRunCoopInterpolationWhilePredictingNative(
        bool netgame,
        int totalTics,
        bool authority,
        bool deathmatch,
        int gameType) =>
        netgame && totalTics > 0 && !authority && !deathmatch && gameType != 4;

    public static ulong PredictionFaultBundleCooldownMsNative() => 60000;

    public static bool ShouldWritePredictionFaultBundleNative(ulong lastBundleMs, ulong nowMs) =>
        lastBundleMs == 0 || nowMs - lastBundleMs >= PredictionFaultBundleCooldownMsNative();
}
