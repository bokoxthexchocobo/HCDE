namespace HCDE.Net.Core;

/// <summary>Invasion query helpers (<c>d_net.cpp</c> <c>Net_GetInvasion*</c>).</summary>
public static class VmDNetInvasionGetters
{
    public static int ClassicInvasionStateNative(int invasionState) =>
        invasionState switch
        {
            1 => 5,
            2 or 3 => 6,
            4 => 7,
            5 => 8,
            6 => 9,
            _ => 0,
        };

    public static int NonNegativeWaveNative(int wave) => Math.Max(wave, 0);

    public static int NonNegativeMaxWavesNative(int maxWaves) => Math.Max(maxWaves, 0);

    public static int NonNegativeBudgetNative(int budget) => Math.Max(budget, 0);

    public static int NonNegativeSpawnedNative(int spawned) => Math.Max(spawned, 0);

    public static int NonNegativeClearedNative(int cleared) => Math.Max(cleared, 0);

    public static int ReplicatedActiveMonsterCountNative(bool invasionAuthority, int replicated, int directorCount) =>
        !invasionAuthority ? Math.Max(replicated, 0) : Math.Max(directorCount, 0);

    public const uint InvWavefBoss = 1;

    public static bool IsBossWaveNative(uint waveFlags, uint bossFlag) => (waveFlags & bossFlag) != 0;

    public static int SpawnSpotCountNative(int totalSpotCount) => Math.Max(totalSpotCount, 0);

    public static int ActiveSpawnSpotCountNative(int activeSpotCount) => Math.Max(activeSpotCount, 0);

    public static int SpawnPlanBudgetNative(int totalSpawnBudget) => Math.Max(totalSpawnBudget, 0);

    public static bool InvasionSpawnHasActiveTagNative(int activeTag) => activeTag != 0;

    public static bool InvasionControlAllowedNative(bool invasionAuthority, bool invasionModeEnabled) =>
        invasionAuthority && invasionModeEnabled;

    public static int InvasionControlNextWaveNative(int invctlNextWave, int action) =>
        action == invctlNextWave ? 1 : 0;

    public static int InvasionControlVictoryNative(int invctlVictory, int action) =>
        action == invctlVictory ? 1 : 0;

    public static int InvasionControlFailureNative(int invctlFailure, int action) =>
        action == invctlFailure ? 1 : 0;

    public static int InvasionControlDefaultNative(int action, int invctlNextWave, int invctlVictory, int invctlFailure) =>
        action != invctlNextWave && action != invctlVictory && action != invctlFailure ? 0 : 1;

    public static bool ArchvileMonsterEligibleNative(int health, uint objectFlags, uint euthanizeMe) =>
        health > 0 && (objectFlags & euthanizeMe) == 0;

    public static int IncrementArchvileCountNative(int count) => count + 1;

    public static int ClientReplicatedArchvileCountNative(int replicated) => Math.Max(replicated, 0);

    public static bool UseReplicatedArchvileCountNative(bool invasionAuthority) => !invasionAuthority;
}
