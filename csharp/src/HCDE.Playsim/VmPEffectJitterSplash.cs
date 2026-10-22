namespace HCDE.Playsim;

/// <summary>
/// Jitter particles, fountain helper, and splash drawers from <c>p_effect.cpp</c> (401–660).
/// </summary>
public static class VmPEffectJitterSplash
{
    public const int SpfFadeInOut = 1;
    public const int SpfFadeInHoldOut = 1 << 1;
    public const int SpfNegativeFadeStep = 1 << 2;
    public const int SpfLocalAnim = 1 << 3;

    public static float SpawnParticleFadeStep(int lifetime) => 1f / lifetime;

    public static float AdjustFadeInOutStep(float step, int flags) =>
        (flags & SpfFadeInOut) != 0 ? step * -2 : step;

    public static float AdjustFadeInHoldOutStep(float step, int flags) =>
        (flags & SpfFadeInHoldOut) != 0 ? step * -3 : step;

    public static bool UsesComputedFadeStep(double fadestep, int flags) =>
        (fadestep < 0 && (flags & SpfNegativeFadeStep) == 0) || fadestep <= -1.0;

    public static bool InitLocalAnim(int flags) => (flags & SpfLocalAnim) != 0;

    public static int JitterParticleTtlDefault => 1;

    public static double JitterDriftVelocity(int randomByte, double drift) =>
        (1.0 / 4096) * (randomByte - 128) * drift;

    public static double JitterDriftAcceleration(int randomByte, double drift) =>
        (1.0 / 16384) * (randomByte - 128) * drift;

    public static bool ShouldSkipFountainOnOddTic(int levelTime) => (levelTime & 1) != 0;

    public static double FountainOutRadius(double actorRadius, int randomByte) =>
        actorRadius * randomByte / 256.0;

    public static bool FountainInnerJet(double out, double actorRadius) => out < actorRadius / 8;

    public static float FountainInnerZBoost() => 10f / 3;
    public static float FountainOuterZBoost() => 3;
    public static float FountainGravityAccel() => -1f / 11;

    public static int PickFountainColor(int randomRoll, int color1, int color2) =>
        randomRoll < 30 ? color2 : color1;

    public static int PickFountainSize(int randomRoll) => randomRoll < 30 ? 4 : 6;

    public static bool SplashKindSupported(int kind) => kind == 1;

    public static int PickSplashColor(int randomRoll, int color1, int color2) =>
        (randomRoll & 0x80) != 0 ? color1 : color2;

    public static int Splash2ColorKind(int kind, int blood1, int blood2, int grey3, int grey5, int grey1) =>
        kind switch
        {
            0 => blood1,
            1 => grey3,
            2 => grey3,
            _ => kind,
        };

    public static double Splash2ZSpread(bool updown) => updown ? -6000 / 65536.0 : 6000 / 65536.0;

    public static int Splash2ZAdd(int updown) => updown == 2 ? -128 : 0;

    public static bool Splash2UsesDirectionalVelocity(int kind) => kind != 0;
}
