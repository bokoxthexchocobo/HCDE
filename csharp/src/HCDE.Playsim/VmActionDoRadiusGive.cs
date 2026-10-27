namespace HCDE.Playsim;

/// <summary><c>DoRadiusGive</c> and <c>A_RadiusGive</c> setup from <c>p_actionfunctions.cpp</c> (3774–3985).</summary>
public static class VmActionDoRadiusGive
{
    public const int RgfMask =
        VmActionRadiusGiveEligibility.RgfGiveself
        | VmActionRadiusGiveEligibility.RgfPlayers
        | VmActionRadiusGiveEligibility.RgfMonsters
        | VmActionRadiusGiveEligibility.RgfObjects
        | VmActionRadiusGiveEligibility.RgfVoodoo
        | VmActionRadiusGiveEligibility.RgfCorpses
        | VmActionRadiusGiveEligibility.RgfKilled
        | VmActionRadiusGiveEligibility.RgfMissiles
        | VmActionRadiusGiveEligibility.RgfItems;

    public static bool RadiusGiveAbortsEarly(object? item, int flags, double distance, double mindist, bool itemIsInventory) =>
        item is null
        || (flags & RgfMask) == 0
        || flags == 0
        || distance <= 0
        || mindist >= distance
        || !itemIsInventory;

    public static int NormalizeGiveAmount(int amount) => amount == 0 ? 1 : amount;

    public static bool UnlimitedGiveLimit(int limit) => limit <= 0;

    public static bool ShouldIterateMissiles(int flags) =>
        (flags & VmActionRadiusGiveEligibility.RgfMissiles) != 0;

    public static bool TryPassSelf(bool isSelf, int flags)
    {
        if (!isSelf) return false;
        return (flags & VmActionRadiusGiveEligibility.RgfGiveself) != 0;
    }

    public static bool TryPassMissile(bool isMissile, int flags)
    {
        if (!isMissile) return false;
        return (flags & VmActionRadiusGiveEligibility.RgfMissiles) != 0;
    }

    public static bool TryPassItemCorpseOrKilled(int flags, bool isInventory, bool isCorpse, bool isKilled) =>
        ((flags & VmActionRadiusGiveEligibility.RgfItems) != 0 && isInventory)
        || ((flags & VmActionRadiusGiveEligibility.RgfCorpses) != 0 && isCorpse)
        || ((flags & VmActionRadiusGiveEligibility.RgfKilled) != 0 && isKilled);

    public static bool TryPassLivingActor(
        int flags,
        int health,
        bool isPlayerMo,
        bool isVoodoo,
        bool isMonster,
        bool isShootableObject)
    {
        if (health <= 0) return false;
        if (isPlayerMo)
        {
            if ((flags & VmActionRadiusGiveEligibility.RgfPlayers) != 0 && !isVoodoo) return true;
            if ((flags & VmActionRadiusGiveEligibility.RgfVoodoo) != 0 && isVoodoo) return true;
            return false;
        }

        if ((flags & VmActionRadiusGiveEligibility.RgfMonsters) != 0 && isMonster) return true;
        return (flags & VmActionRadiusGiveEligibility.RgfObjects) != 0
            && !isMonster
            && isShootableObject;
    }

    public static bool AnyActorCategoryFlag(int flags) =>
        (flags & (VmActionRadiusGiveEligibility.RgfMonsters
            | VmActionRadiusGiveEligibility.RgfObjects
            | VmActionRadiusGiveEligibility.RgfPlayers
            | VmActionRadiusGiveEligibility.RgfVoodoo)) != 0;

    public static bool FilterSpeciesGate(
        bool filterPass,
        bool speciesPass,
        int flags,
        bool isSelf) =>
        VmActionRadiusGiveEligibility.FilterSpeciesCombined(filterPass, speciesPass, flags)
        || isSelf;

    public static bool PointerFlagsActive(int flags) =>
        (flags & (VmActionRadiusGiveEligibility.RgfNotarget
            | VmActionRadiusGiveEligibility.RgfNomaster
            | VmActionRadiusGiveEligibility.RgfNotracer)) != 0;

    public static bool TargetPass(bool isTarget, int flags) =>
        !isTarget || (flags & VmActionRadiusGiveEligibility.RgfNotarget) == 0;

    public static bool MasterPass(bool isMaster, int flags) =>
        !isMaster || (flags & VmActionRadiusGiveEligibility.RgfNomaster) == 0;

    public static bool TracerPass(bool isTracer, int flags) =>
        !isTracer || (flags & VmActionRadiusGiveEligibility.RgfNotracer) == 0;

    public static bool PointerRelationGate(
        bool isSelf,
        bool isTarget,
        bool isMaster,
        bool isTracer,
        int flags)
    {
        if (isSelf || !PointerFlagsActive(flags)) return true;
        return VmActionRadiusGiveEligibility.PointerRelationPass(
            TargetPass(isTarget, flags),
            MasterPass(isMaster, flags),
            TracerPass(isTracer, flags),
            flags);
    }

    public static double RangeCheckHeightOffset(double thingHeight) => thingHeight * 0.5;

    public static bool OutsideRange(
        int flags,
        double absDx,
        double absDy,
        double absDz,
        double lengthSquared,
        double distance,
        double mindist) =>
        (flags & VmActionRadiusGiveEligibility.RgfCube) != 0
            ? VmActionRadiusGiveEligibility.OutsideCube(absDx, absDy, absDz, distance, mindist)
            : VmActionRadiusGiveEligibility.OutsideSphere(lengthSquared, distance, mindist);
}
