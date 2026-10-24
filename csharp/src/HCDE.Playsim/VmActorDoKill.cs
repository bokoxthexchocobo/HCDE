namespace HCDE.Playsim;

/// <summary><c>DoKill</c> and <c>A_Kill*</c> / <c>A_DamageSiblings</c> bands from <c>p_actionfunctions.cpp</c> (4212–4422).</summary>
public static class VmActorDoKill
{
    [Flags]
    public enum KilsFlags
    {
        FoilInvul = 1 << 0,
        KillMissiles = 1 << 1,
        NoMonsters = 1 << 2,
        FoilBuddha = 1 << 3,
        ExFilter = 1 << 4,
        ExSpecies = 1 << 5,
        Either = 1 << 6,
    }

    public enum KillPlan
    {
        Skip,
        ExplodeMissile,
        DamageMobj,
        ExplodeAndDamage,
    }

    public static bool PassesKillFilter(bool filterPass, bool speciesPass, KilsFlags flags) =>
        (flags & KilsFlags.Either) != 0 ? filterPass || speciesPass : filterPass && speciesPass;

    public static bool PassesClassGate(bool filterPass, bool excludeFilter) =>
        filterPass;

    public static bool PassesSpeciesGate(bool speciesPass, bool excludeSpecies) =>
        speciesPass;

    public static int BaseKillDamageFlags => 8 | 4;

    public static int AddFoilFlags(int dmgFlags, KilsFlags flags)
    {
        if ((flags & KilsFlags.FoilInvul) != 0) dmgFlags |= 1;
        if ((flags & KilsFlags.FoilBuddha) != 0) dmgFlags |= 2;
        return dmgFlags;
    }

    public static bool ShouldExplodeMissile(bool isMissile, KilsFlags flags) =>
        isMissile && (flags & KilsFlags.KillMissiles) != 0;

    public static bool MissileKillAllowed(bool invulnerable, bool buddha, bool noDamage, KilsFlags flags) =>
        (!(invulnerable) || (flags & KilsFlags.FoilInvul) != 0)
        && (!(buddha) || (flags & KilsFlags.FoilBuddha) != 0)
        && !noDamage;

    public static bool ShouldDamageMobj(KilsFlags flags) => (flags & KilsFlags.NoMonsters) == 0;

    public static KillPlan PlanDoKill(
        bool passesFilter,
        bool isMissile,
        bool invulnerable,
        bool buddha,
        bool noDamage,
        KilsFlags flags)
    {
        if (!passesFilter)
            return KillPlan.Skip;

        var explode = ShouldExplodeMissile(isMissile, flags) && MissileKillAllowed(invulnerable, buddha, noDamage, flags);
        var damage = ShouldDamageMobj(flags);

        if (explode && damage)
            return KillPlan.ExplodeAndDamage;
        if (explode)
            return KillPlan.ExplodeMissile;
        if (damage)
            return KillPlan.DamageMobj;
        return KillPlan.Skip;
    }

    public static bool DamageSiblingsSameMaster(bool hasMaster, bool sameMaster, bool notSelf) =>
        hasMaster && sameMaster && notSelf;

    public static bool KillSiblingsLoop(bool hasMaster, bool sameMaster, bool notSelf) =>
        hasMaster && sameMaster && notSelf;

}
