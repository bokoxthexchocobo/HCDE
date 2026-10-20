namespace HCDE.Playsim;

/// <summary><c>P_TraceBleed</c> blood decal spray from <c>p_map.cpp</c> (5245–5357).</summary>
public static class VmPMapTraceBleed
{
    public const int MfNoblood = 1;
    public const int Mf5Noblooddecals = 1;
    public const int Mf2Invulnerable = 1;
    public const int Mf2Dormant = 1 << 1;
    public const int Mf3Bloodlessimpact = 1;
    public const int CfGodmode = 1;

    public static bool BloodSplatsDisabled(bool clBloodSplats) => !clBloodSplats;

    public static bool SkipBleedActor(int flags, int flags5, int flags2, bool playerGodmode) =>
        (flags & MfNoblood) != 0
        || (flags5 & Mf5Noblooddecals) != 0
        || (flags2 & (Mf2Invulnerable | Mf2Dormant)) != 0
        || playerGodmode;

    public readonly record struct BleedSprayPlan(int Count, double Noise, string BloodType);

    public static BleedSprayPlan PlanBleedSpray(int damage, int rngByte)
    {
        if (damage < 15)
        {
            if (damage <= 10 && rngByte < 160) return new BleedSprayPlan(0, 0, "BloodSplat");
            return new BleedSprayPlan(1, 11.25 / 256, "BloodSplat");
        }
        if (damage < 25) return new BleedSprayPlan(2, 22.5 / 256, "BloodSplat");
        if (rngByte < 24) return new BleedSprayPlan(1, 45.0 / 256, "BloodSmear");
        return new BleedSprayPlan(3, 45.0 / 256, "BloodSplat");
    }

    public static double DefaultBleedDistance(double configured) =>
        configured <= 0 ? 172.0 : configured;

    public static double JitterAngle(int rngByte, double noise) => (rngByte - 128) * noise;

    public static (double X, double Y, double Z) BleedDirection(
        double pitchCos,
        double angleCos,
        double angleSin,
        double pitchSin,
        double jitterA,
        double jitterP)
    {
        var angCos = Math.Cos(jitterA);
        var angSin = Math.Sin(jitterA);
        var cosp = pitchCos * Math.Cos(jitterP);
        return (cosp * angCos, cosp * angSin, -pitchSin);
    }

    public static bool WallHitCreatesDecal(int hitType) => hitType == 3;

    public static (int R, int G, int B, int A) DimBloodColor(int r, int g, int b) =>
        (r >> 1, g >> 1, b >> 1, 1);

    public static bool UseBloodTranslation(bool hasColor) => hasColor;

    public static bool MissileBleedSkip(bool targetNull, int missileFlags3) =>
        targetNull || (missileFlags3 & Mf3Bloodlessimpact) != 0;

    public static double PitchFromMissileVel(double velZ, double distance2d)
    {
        if (distance2d == 0) return 0;
        return -Math.Atan(velZ / distance2d);
    }
}
