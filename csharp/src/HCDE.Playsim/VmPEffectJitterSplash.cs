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

    public static bool FountainInnerJet(double radialOut, double actorRadius) => radialOut < actorRadius / 8;

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

    public readonly record struct SpawnParticleFadeContext(
        double FadeStep,
        int Flags,
        int Lifetime,
        double FadeOutStep);

    public static (float FadeStep, float FadeOutStep) ResolveSpawnFadeSteps(SpawnParticleFadeContext ctx)
    {
        if (!UsesComputedFadeStep(ctx.FadeStep, ctx.Flags))
            return ((float)ctx.FadeStep, (float)ctx.FadeOutStep);

        var step = SpawnParticleFadeStep(ctx.Lifetime);
        step = AdjustFadeInOutStep(step, ctx.Flags);
        step = AdjustFadeInHoldOutStep(step, ctx.Flags);
        return (step, -step);
    }

    public static int SplashSparkColor1() => 0; // palette indices resolved at seam
    public static int SplashSparkColor2() => 0;

    public static double SplashVelocityZ(int randomByte) => -randomByte / 128.0;

    public static double SplashAccelZ() => -1.0 / 8;

    public static double SplashOffsetZ(int randomByte) => -randomByte / 64.0;

    public static int Splash2DefaultTtl() => 12;

    public static float Splash2Alpha() => 1f;

    public static int Splash2Size() => 4;

    public static double Splash2VelocityZ(int randomByte, double zVelScale) => randomByte * zVelScale;

    public static double Splash2AccelZ() => -1 / 22.0;

    public static double Splash2VelocityXY(int randomByte, double trig) => randomByte * trig / 2048.0;

    public static double Splash2AccelFromVel(double vel) => vel / 16.0;

    public static double Splash2PosOffset(int randomByte, double trig, int spread) =>
        (randomByte - spread) * trig;

    public static double Splash2PosZ(int randomByte, int zAdd, double zSpread) =>
        (randomByte + zAdd - 128) * zSpread;
}
