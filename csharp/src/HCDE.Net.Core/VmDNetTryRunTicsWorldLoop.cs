namespace HCDE.Net.Core;

/// <summary><c>TryRunTics</c> world tick loop and client-side tail (<c>d_net.cpp</c> 7981–8268).</summary>
public static class VmDNetTryRunTicsWorldLoop
{
    public static bool ShouldUnpredictBeforeWorldLoopNative(bool runTicsPositive) => runTicsPositive;

    public static int SelectLagStateBeforeWorldNative(int clientTic, int startCommand) =>
        clientTic > startCommand ? 0 : 1;

    public static bool ShouldStabilizeTickNative(bool stabilizeFlag) => stabilizeFlag;

    public static bool ShouldBreakOnCommandsResetNative(bool commandsReset) => commandsReset;

    public static int MirrorVisualWorldStepsNative(int worldStepsUsed, bool netgame, bool invasionAuthority) =>
        !invasionAuthority && netgame && worldStepsUsed > 0 ? Math.Clamp(worldStepsUsed, 1, 32) : 1;

    public static bool ShouldRunCoopInterpolationNative(bool authority, bool netgame, bool deathmatch, int gameType) =>
        !authority && netgame && !deathmatch && gameType != 4;

    public static int ClientSideLogicIterationsNative(int totalTics) => totalTics;

    public static int SoundWorldTimerNative(int localWorldTimer, int worldTimerAtStart) =>
        localWorldTimer - Math.Min(localWorldTimer, worldTimerAtStart);

    public static bool ShouldApplyPendingHealthRepairNative() => true;

    public static bool ShouldPrepareInvasionLodNative(bool invasionAuthority) => invasionAuthority;

    public static bool ShouldTickInvasionStateNative(bool invasionAuthority) => invasionAuthority;

    public static bool ShouldTickInvasionMirrorNative(bool invasionAuthority) => !invasionAuthority;

    public static bool ShouldTickBandwidthAutoNative() => true;

    public static bool ShouldTickInvasionAnnouncementsNative() => true;

    public static bool ShouldMakeConsistenciesNative() => true;

    public static bool ShouldRewindServerTickNative() => true;

    public static bool ShouldRecordWorldTicProfileNative(ulong elapsedMicros) => elapsedMicros >= 0;

    public static bool ShouldResetInvasionMirrorBudgetNative() => true;

    public static int DefaultMirrorVisualTickBudgetNative() => 1;

    public static bool ShouldVerifyPredictedEntitiesAfterLoopNative() => true;

    public static bool ShouldRefreshLagHudAfterLoopNative() => true;

    public static bool ShouldAdvanceDemoNative(bool advancedemo) => advancedemo;

    public static bool ShouldIncrementGameticNative() => true;

    public static bool ShouldChecksumTickNative() => true;

    public static bool ShouldBlackboxRecordTicNative() => true;

    public static bool ShouldTickModeActorMigrationNative() => true;

    public static bool ShouldClearWaitingFlagsNative() => true;

    public static void ClearClientWaitingFlagsNative(Action<int> setWaiting, IEnumerable<int> clients)
    {
        foreach (var client in clients)
            setWaiting(client);
    }

    public static int WorldStepsUsedIncrementNative(int used) => used + 1;

    public static bool ShouldRunMirrorVisualFrameNative(bool netgame, int worldStepsUsed) =>
        netgame && worldStepsUsed > 0;

    public static int ClampMirrorVisualStepsNative(int steps) => Math.Clamp(steps, 1, 32);

    public static bool ShouldResetMirrorVisualBudgetAfterLoopNative() => true;

    public static int FinalMirrorVisualStepsNative() => 1;

    public static bool ShouldPredictClientAfterLoopNative() => true;

    public static int LagHudRunTicsArgumentNative(int runTicsAfterLoop) => runTicsAfterLoop;

    public static bool ShouldPassTicGateStalledFalseToLagHudNative() => false;

    public readonly record struct WorldTickStepPlan(
        bool Stabilize,
        bool AdvanceDemo,
        bool InvasionAuthority,
        bool CommandsReset);

    public static WorldTickStepPlan PlanWorldTickStepNative(
        bool stabilize,
        bool advancedemo,
        bool invasionAuthority,
        bool commandsReset) =>
        new(stabilize, advancedemo, invasionAuthority, commandsReset);

    public static bool ShouldInvokeGTickerNative(WorldTickStepPlan plan) => true;

    public static bool ShouldInvokeInvasionLodNative(WorldTickStepPlan plan) =>
        plan.InvasionAuthority;

    public static bool ShouldInvokeInvasionMirrorTickNative(WorldTickStepPlan plan) =>
        !plan.InvasionAuthority;

    public static bool ShouldStopWorldLoopNative(WorldTickStepPlan plan) => plan.CommandsReset;

    public static int SelectLagStateAfterCommandsNative(int clientTic, int startCommand, int lagNone, int lagSkipping) =>
        clientTic > startCommand ? lagNone : lagSkipping;

    public static bool ShouldUpdateLastGameUpdateNative(int runTicsPositive) => runTicsPositive;

    public static bool ShouldRunWhileLoopNative(int runTicsRemaining) => runTicsRemaining > 0;

    public static int DecrementRunTicsNative(int runTics) => runTics - 1;

    public static ulong ProfileWorldTicStartNative() => 0;

    public static ulong ProfileWorldTicElapsedNative(ulong start, ulong end) => end - start;

    public static bool ShouldRunClientSideLogicLoopNative(int totalTics) => totalTics > 0;

    public static int ClientSideLogicIndexNative(int i) => i;

    public static bool ShouldUpdateSoundsAfterLoopNative(int totalTics) => totalTics > 0;

    public static int MinWorldTimerNative(int local, int start) => Math.Min(local, start);

    public static bool ShouldTickHcdeBandwidthAutoNative() => true;

    public static bool ShouldTickInvasionAnnouncementsEachWorldStepNative() => true;

    public static bool ShouldRecordBlackboxTicIndexNative() => true;

    public static bool ShouldInvokeHcdeRewindOnServerTickNative() => true;

    public static int WorldStepsUsedThisFrameStartNative() => 0;

    public static bool ShouldApplyInvasionMirrorVisualBudgetNative(bool netgame, int steps) =>
        netgame && steps > 0;
}
