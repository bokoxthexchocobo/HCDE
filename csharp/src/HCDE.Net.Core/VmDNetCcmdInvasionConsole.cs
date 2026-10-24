namespace HCDE.Net.Core;

/// <summary>Invasion console CCMD guards (<c>d_net.cpp</c> 10169–10287).</summary>
public static class VmDNetCcmdInvasionConsole
{
    public static bool InvasionControlRejectedNative(int controlResult) => controlResult == 0;

    public static int ClampedMapWaveLimitNative(int invasionWaveLimit, int maxLimit) =>
        Math.Clamp(invasionWaveLimit, 0, maxLimit);

    public static int ClampedMapDuelLimitNative(int duelLimit) => Math.Clamp(duelLimit, 0, 255);

    public static int ClampedLegacyDuelLimitNative(int duellimitCompat) => Math.Clamp(duellimitCompat, 0, 255);

    public static bool FastMonstersDmflagNative(int dmflags, int fastMonstersMask) =>
        (dmflags & fastMonstersMask) != 0;

    public static int MapWaveLimitWhenLevelPresentNative(bool hasLevel, int invasionWaveLimit, int maxLimit) =>
        hasLevel ? ClampedMapWaveLimitNative(invasionWaveLimit, maxLimit) : 0;

    public static int MapDuelLimitWhenLevelPresentNative(bool hasLevel, int duelLimit) =>
        hasLevel ? ClampedMapDuelLimitNative(duelLimit) : 0;

    public static int OverrideCmpgninfFlagNative(bool canOverride) => canOverride ? 1 : 0;

    public static int ExitOnVictoryFlagNative(bool exitOnVictory) => exitOnVictory ? 1 : 0;
}
