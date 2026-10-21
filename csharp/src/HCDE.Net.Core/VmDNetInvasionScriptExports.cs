namespace HCDE.Net.Core;

/// <summary>ZScript invasion export thunks (<c>d_net.cpp</c> 9914–10100).</summary>
public static class VmDNetInvasionScriptExports
{
    public static int InvasionGetStateTicsThunkNative(
        bool invasionAuthority,
        bool countdown,
        int invasionStateTics,
        int cutsceneCountdown) =>
        countdown && !invasionAuthority ? invasionStateTics : cutsceneCountdown;

    public static int InvasionGetWaveThunkNative(int wave) =>
        VmDNetInvasionGetters.NonNegativeWaveNative(wave);

    public static int InvasionGetMaxWavesThunkNative(int maxWaves) =>
        VmDNetInvasionGetters.NonNegativeMaxWavesNative(maxWaves);

    public static int InvasionGetWaveBudgetThunkNative(int budget) =>
        VmDNetInvasionGetters.NonNegativeBudgetNative(budget);

    public static int InvasionGetWaveSpawnedThunkNative(int spawned) =>
        VmDNetInvasionGetters.NonNegativeSpawnedNative(spawned);

    public static int InvasionGetWaveClearedThunkNative(int cleared) =>
        VmDNetInvasionGetters.NonNegativeClearedNative(cleared);

    public static int InvasionGetActiveMonsterThunkNative(bool invasionAuthority, int replicated, int directorCount) =>
        VmDNetInvasionGetters.ReplicatedActiveMonsterCountNative(invasionAuthority, replicated, directorCount);

    public static int InvasionGetArchvileThunkNative(
        bool invasionAuthority,
        int replicatedCount,
        ReadOnlySpan<ArchvileMonsterSnapshot> activeMonsters,
        uint euthanizeMe,
        bool archvileClassResolved) =>
        invasionAuthority
            ? VmDNetInvasionGetters.CountArchvilesAmongActiveMonstersNative(
                activeMonsters, euthanizeMe, archvileClassResolved)
            : VmDNetInvasionGetters.ClientReplicatedArchvileCountNative(replicatedCount);

    public static int InvasionIsBossWaveThunkNative(uint waveFlags, uint bossFlag) =>
        VmDNetInvasionGetters.IsBossWaveNative(waveFlags, bossFlag) ? 1 : 0;

    public static int InvasionGetSpawnSpotCountThunkNative(int count) =>
        VmDNetInvasionGetters.SpawnSpotCountNative(count);

    public static int InvasionGetActiveSpawnSpotCountThunkNative(int count) =>
        VmDNetInvasionGetters.ActiveSpawnSpotCountNative(count);

    public static int InvasionGetSpawnPlanBudgetThunkNative(int budget) =>
        VmDNetInvasionGetters.SpawnPlanBudgetNative(budget);

    public static int InvasionGetSpawnActiveTagThunkNative(int tag) =>
        VmDNetInvasionGetters.InvasionSpawnHasActiveTagNative(tag) ? tag : 0;

    public static int InvasionIsSpawnUsingFallbackThunkNative(bool usingFallback) => usingFallback ? 1 : 0;

    public static int InvasionControlThunkNative(
        int action,
        int invctlNextWave,
        int invctlVictory,
        int invctlFailure,
        bool invasionAuthority,
        bool invasionModeEnabled) =>
        VmDNetInvasionGetters.ResolveControlInvasionNative(
            invasionAuthority, invasionModeEnabled, action, invctlNextWave, invctlVictory, invctlFailure);
}
