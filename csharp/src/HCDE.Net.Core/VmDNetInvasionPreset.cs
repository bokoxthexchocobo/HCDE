namespace HCDE.Net.Core;

/// <summary><c>Net_ApplyInvasionPreset</c> and preset CCMD mirrors (<c>d_net.cpp</c> 10189–10236).</summary>
public static class VmDNetInvasionPreset
{
    public const int DefaultSvGametype = 4;
    public const int FastMonstersDmflagMask = 0x4000000;
    public const float DefaultCountdownTime = 30.0f;
    public const float DefaultSpawnTime = 8.0f;
    public const float DefaultCleanupTime = 4.0f;
    public const float DefaultIntermissionTime = 6.0f;
    public const float DefaultResultTime = 8.0f;
    public const int DefaultInvasionWaves = 8;
    public const int DefaultBaseBudget = 24;
    public const int DefaultBudgetStep = 8;
    public const int DefaultPerPlayer = 6;
    public const float DefaultSpawnInterval = 0.35f;
    public const int DefaultSpawnBurst = 3;
    public const int DefaultBossWaveEvery = 5;
    public const int DefaultBossBonus = 20;

    public static bool RejectWithoutAuthorityNative(bool localAuthority) => !localAuthority;

    public static int ClearFastMonstersDmflagsNative(int dmflags) => dmflags & ~FastMonstersDmflagMask;

    public static int ApplySvGametypeNative(int current, int presetGametype) =>
        current != presetGametype ? presetGametype : current;

    public static bool DefaultExitOnVictoryNative() => true;

    public static int DefaultInvasionDebugNative() => 0;

    public static int DefaultMaxActiveNative() => 0;

    public static int ClearCompatWaveLimitNative() => 0;

    public static int ClearCompatDuelLimitNative() => 0;

    public static bool DefaultUseMapSettingsWaveLimitNative() => true;

    public static bool DefaultSpotUseMapTagsNative() => false;

    public static bool DefaultSpotFallbackNative() => true;

    public static string ResolveTraceReasonNative(string? traceReason, string defaultReason) =>
        traceReason ?? defaultReason;

    public static string InvasionPresetTraceReasonNative() => "invasion-preset";

    public static string InvasionStandardAliasTraceReasonNative() => "invasion-standard-alias";
}
