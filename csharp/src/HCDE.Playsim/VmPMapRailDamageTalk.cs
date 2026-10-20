namespace HCDE.Playsim;

/// <summary><c>P_RailAttack</c> hit loop and conversation probes from <c>p_map.cpp</c> (5568–5810).</summary>
public static class VmPMapRailDamageTalk
{
    public const int MfNoblood = 1;
    public const int Mf2Dormant = 1;
    public const int Mf2Invulnerable = 1;
    public const int Mf3Foilinvul = 1;
    public const int Mf3Alwayspuff = 1;
    public const int Mf3Bloodlessimpact = 1;
    public const int Mf3Skyexplode = 1;
    public const int Mf7Forcedecal = 1;
    public const int PfHitthing = 1;
    public const int PfHitthingbleed = 1 << 1;
    public const int DmgInflictorIsPuff = 1;
    public const int DmgFoilinvul = 1 << 1;
    public const int DmgFoilbuddha = 1 << 2;
    public const int DmgUseangle = 1 << 3;
    public const int DmgPlayerattack = 1 << 4;
    public const int DmgRailgun = 1 << 5;
    public const int Mf4Incombat = 1;
    public const int TraceHitWall = 3;
    public const int TraceHitFloor = 4;
    public const int TraceHitCeiling = 5;
    public const int LineHorizon = 1;

    public static bool SpawnPuffForNoblood(bool hasPuffClass) => hasPuffClass;

    public static bool AlwaysPuffSpawn(int puffFlags3, bool hasClass) =>
        hasClass && (puffFlags3 & Mf3Alwayspuff) != 0;

    public static int ActorPuffFlags(int baseFlags, bool bleed) =>
        bleed ? baseFlags | PfHitthingbleed : baseFlags;

    public static bool ShouldBleed(int puffFlags3) => (puffFlags3 & Mf3Bloodlessimpact) == 0;

    public static bool FallbackPuff(bool hitPuffIsNull, bool thePuffExists) =>
        hitPuffIsNull && thePuffExists;

    public static int BuildDamageFlags(bool foilInvul, bool foilBuddha, bool sourceIsPlayer) =>
        DmgInflictorIsPuff
        | (foilInvul ? DmgFoilinvul : 0)
        | (foilBuddha ? DmgFoilbuddha : 0)
        | DmgUseangle
        | DmgRailgun
        | (sourceIsPlayer ? DmgPlayerattack : 0);

    public static int DamageForBleed(int newDam, int requestDam) =>
        newDam > 0 ? newDam : requestDam;

    public static bool PoisonOnHit(int poisonDamage, int poisonDuration) =>
        poisonDamage > 0 && poisonDuration != int.MinValue;

    public static bool DestroyHorizonPuff(bool horizonLine, int puffFlags3) =>
        horizonLine && (puffFlags3 & Mf3Skyexplode) == 0;

    public static bool ForceDecalSpawn(int flags7, bool hasGenerator) =>
        (flags7 & Mf7Forcedecal) != 0 && hasGenerator;

    public static bool SkyFloorDestroy(int hitType, int floorPic, int skyFlat) =>
        hitType == TraceHitFloor && floorPic == skyFlat;

    public static bool DeepSplash(bool crossed3d, bool crossedWater) => crossed3d || crossedWater;

    public static bool HitWaterOnFloor(int hitType, bool heightSec) =>
        hitType == TraceHitFloor && !heightSec;

    public static bool CanTalk(bool hasConversation, int health, int flags4) =>
        hasConversation && health > 0 && (flags4 & Mf4Incombat) == 0;

    public static bool HasConversation(bool hasConversation) => hasConversation;

    public readonly record struct RailHitPuffContext(
        int HitFlags,
        int HitFlags2,
        int PuffFlags3,
        bool HasPuffClass,
        bool PuffDefaultsNull);

    public static (bool SpawnPuff, bool Bleed, int PuffFlags) EvaluateRailHitPuff(RailHitPuffContext ctx)
    {
        var noblood = (ctx.HitFlags & MfNoblood) != 0;
        var dormant = (ctx.HitFlags2 & Mf2Dormant) != 0;
        var invuln = (ctx.HitFlags2 & Mf2Invulnerable) != 0 && (ctx.PuffFlags3 & Mf3Foilinvul) == 0;
        if (noblood || dormant || invuln)
            return (ctx.HasPuffClass, false, PfHitthing);
        var always = (ctx.PuffFlags3 & Mf3Alwayspuff) != 0;
        var bleed = ShouldBleed(ctx.PuffFlags3);
        return (ctx.HasPuffClass && always, bleed, ActorPuffFlags(PfHitthing, bleed));
    }
}
