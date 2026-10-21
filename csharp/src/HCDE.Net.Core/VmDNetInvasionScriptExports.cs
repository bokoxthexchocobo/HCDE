namespace HCDE.Net.Core;

/// <summary>ZScript invasion export thunks (<c>d_net.cpp</c> <c>InvasionGet*</c>).</summary>
public static class VmDNetInvasionScriptExports
{
    public static int InvasionGetStateExportNative(int invasionState) => invasionState;

    public static int InvasionGetWaveExportNative(int wave) => Math.Max(wave, 0);

    public static int InvasionGetMaxWavesExportNative(int maxWaves) => Math.Max(maxWaves, 0);

    public static int InvasionGetWaveBudgetExportNative(int budget) => Math.Max(budget, 0);

    public static int InvasionGetWaveSpawnedExportNative(int spawned) => Math.Max(spawned, 0);

    public static int InvasionGetWaveClearedExportNative(int cleared) => Math.Max(cleared, 0);

    public static int InvasionGetActiveMonsterExportNative(int count) => Math.Max(count, 0);

    public static int InvasionGetArchvileExportNative(int count) => Math.Max(count, 0);

    public static int InvasionBossWaveAsIntNative(bool bossWave) => bossWave ? 1 : 0;

    public static int InvasionGetSpawnSpotCountExportNative(int count) => Math.Max(count, 0);

    public static int InvasionGetActiveSpawnSpotCountExportNative(int count) => Math.Max(count, 0);

    public static int InvasionGetSpawnPlanBudgetExportNative(int budget) => Math.Max(budget, 0);

    public static int InvasionIsSpawnUsingFallbackExportNative(bool usingFallback) => usingFallback ? 1 : 0;

    public static int InvasionControlExportNative(int result) => result;
}
