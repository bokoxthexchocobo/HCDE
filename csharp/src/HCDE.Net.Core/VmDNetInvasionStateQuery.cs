namespace HCDE.Net.Core;

/// <summary>Invasion state query helpers (<c>d_net.cpp</c> 9701+).</summary>
public static class VmDNetInvasionStateQuery
{
    public const int InvsCountdown = 1;
    public const int InvsSpawning = 2;
    public const int InvsCleanup = 3;
    public const int InvsIntermission = 4;
    public const int InvsVictory = 5;
    public const int InvsFailure = 6;
    public const int InvsWaiting = 7;
    public const int InvsDisabled = 8;

    public static int NonNegativeWaveNative(int wave) => Math.Max(wave, 0);

    public static int NonNegativeMaxWavesNative(int maxWaves) => Math.Max(maxWaves, 0);

    public static int NonNegativeBudgetNative(int budget) => Math.Max(budget, 0);

    public static int NonNegativeSpawnedNative(int spawned) => Math.Max(spawned, 0);

    public static int NonNegativeClearedNative(int cleared) => Math.Max(cleared, 0);

    public static int ReplicatedActiveMonsterCountNative(bool invasionAuthority, int replicated, int directorCount) =>
        !invasionAuthority ? Math.Max(replicated, 0) : Math.Max(directorCount, 0);

    public static bool IsCountdownStateNative(int invasionState, int invsCountdown) =>
        invasionState == invsCountdown;

    public const uint InvWavefBoss = 1;

    public static bool IsBossWaveNative(uint waveFlags, uint bossFlag) => (waveFlags & bossFlag) != 0;

    public static bool ShouldCountArchvileNative(object? mobj, int health, uint euthanizeFlag, bool isArchvile) =>
        mobj is not null && health > 0 && (euthanizeFlag & 0) == 0 && isArchvile;

    public static int SpawnSpotCountNative(int totalSpotCount) => Math.Max(totalSpotCount, 0);

    public static int ActiveSpawnSpotCountNative(int activeSpotCount) => Math.Max(activeSpotCount, 0);

    public static int SpawnPlanBudgetNative(int totalSpawnBudget) => Math.Max(totalSpawnBudget, 0);

    public static bool InvasionControlAllowedNative(bool invasionAuthority, bool invasionModeEnabled) =>
        invasionAuthority && invasionModeEnabled;

    public static int InvasionControlNextWaveNative(int invctlNextWave, int action) =>
        action == invctlNextWave ? 1 : 0;

    public static int InvasionControlVictoryNative(int invctlVictory, int action) =>
        action == invctlVictory ? 1 : 0;

    public static int InvasionControlFailureNative(int invctlFailure, int action) =>
        action == invctlFailure ? 1 : 0;

    public static int InvasionBossWaveAsIntNative(bool bossWave) => bossWave ? 1 : 0;

    public static int InvasionGetWaveExportNative(int wave) => NonNegativeWaveNative(wave);

    public static int InvasionGetMaxWavesExportNative(int maxWaves) => NonNegativeMaxWavesNative(maxWaves);

    public static int InvasionGetActiveMonsterExportNative(bool invasionAuthority, int replicated, int directorCount) =>
        ReplicatedActiveMonsterCountNative(invasionAuthority, replicated, directorCount);

    public static int InvasionGetArchvileExportNative(int count) => Math.Max(count, 0);

    public static bool ArchvileActorEligibleNative(int health, uint objectFlags, uint euthanizeMe) =>
        health > 0 && (objectFlags & euthanizeMe) == 0;

    public static bool InvasionSpawnHasActiveTagNative(int activeTag) => activeTag != 0;

    public static int InvasionControlDefaultNative(int action, int invctlNextWave, int invctlVictory, int invctlFailure) =>
        action != invctlNextWave && action != invctlVictory && action != invctlFailure ? 0 : 1;

    public static int InvasionGetWaveBudgetExportNative(int budget) => NonNegativeBudgetNative(budget);

    public static int InvasionGetWaveSpawnedExportNative(int spawned) => NonNegativeSpawnedNative(spawned);

    public static int InvasionGetWaveClearedExportNative(int cleared) => NonNegativeClearedNative(cleared);
}
