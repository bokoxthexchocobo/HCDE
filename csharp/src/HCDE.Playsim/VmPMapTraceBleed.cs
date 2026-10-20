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

    public static bool LineTargetBleedSkip(bool lineTargetNull, int puffFlags3) =>
        lineTargetNull || (puffFlags3 & Mf3Bloodlessimpact) != 0;

    public static double RandomBleedPitch(int rngByte) => (rngByte - 128) * (360.0 / 65536.0);

    public static double RandomBleedAngle(int rngByte) => rngByte * (360.0 / 256.0);

    public const int TraceHitActor = 2;
    public const int TraceCrossingPortal = 1;
    public const int Mf3Ghost = 1;
    public const int Mf8Stoprails = 1;

    public enum RailTraceStatus { Stop, Skip, Continue }

    public readonly record struct RailHitContext(
        bool ThruActors,
        bool UseThruBits,
        uint ThruBits,
        uint ActorThruBits,
        bool MThruSpecies,
        bool ThruSpecies,
        bool ThruGhosts,
        bool CallerIsPlayer,
        bool PassThroughPlayer,
        bool SameSpeciesAsCaller,
        bool SameSpeciesAsPuff,
        bool ActorGhost,
        bool StopAtInvul,
        int ActorFlags2,
        bool StopAtOne,
        bool HasLimit,
        int Limit,
        int Count,
        int ActorFlags8);

    public static RailTraceStatus EvaluateRailActorHit(RailHitContext ctx)
    {
        if (ctx.ThruActors
            || ctx.UseThruBits && (ctx.ThruBits & ctx.ActorThruBits) != 0
            || ctx.MThruSpecies && ctx.SameSpeciesAsCaller
            || ctx.ThruSpecies && ctx.SameSpeciesAsPuff
            || ctx.ThruGhosts && ctx.ActorGhost
            || ctx.CallerIsPlayer && ctx.PassThroughPlayer)
            return RailTraceStatus.Skip;
        if (ctx.StopAtInvul && (ctx.ActorFlags2 & Mf2Invulnerable) != 0)
            return RailTraceStatus.Stop;
        if (ctx.StopAtOne || ctx.HasLimit && ctx.Count >= ctx.Limit
            || (ctx.ActorFlags8 & Mf8Stoprails) != 0)
            return RailTraceStatus.Stop;
        return RailTraceStatus.Continue;
    }

    public static RailTraceStatus PortalCrossing(int hitType) =>
        hitType == TraceCrossingPortal ? RailTraceStatus.Continue : RailTraceStatus.Stop;

    public static bool NonActorHitStops(int hitType) => hitType != TraceHitActor;
}
