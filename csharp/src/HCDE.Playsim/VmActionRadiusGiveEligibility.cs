namespace HCDE.Playsim;

/// <summary>Species/class filters and <c>DoRadiusGive</c> eligibility from <c>p_actionfunctions.cpp</c> (3751–3889).</summary>
public static class VmActionRadiusGiveEligibility
{
    public const int RgfGiveself = 1 << 0;
    public const int RgfMissiles = 1 << 11;
    public const int RgfItems = 1 << 13;
    public const int RgfCorpses = 1 << 5;
    public const int RgfKilled = 1 << 14;
    public const int RgfMonsters = 1 << 2;
    public const int RgfObjects = 1 << 3;
    public const int RgfPlayers = 1 << 1;
    public const int RgfVoodoo = 1 << 4;
    public const int RgfExfilter = 1 << 15;
    public const int RgfExspecies = 1 << 16;
    public const int RgfEither = 1 << 17;
    public const int RgfNotarget = 1 << 6;
    public const int RgfNomaster = 1 << 8;
    public const int RgfNotracer = 1 << 7;
    public const int RgfInclusive = 1 << 12;
    public const int RgfCube = 1 << 9;
    public const int RgfNosight = 1 << 10;

    public static bool SpeciesPasses(string? filterSpecies, string actorSpecies, bool exclude) =>
        filterSpecies is null || (exclude ? actorSpecies != filterSpecies : actorSpecies == filterSpecies);

    public static bool ClassPasses(bool filterNull, bool sameClass, bool exclude) =>
        filterNull || (exclude ? !sameClass : sameClass);

    public static bool GiveSelfAllowed(bool isSelf, int flags) =>
        isSelf && (flags & RgfGiveself) != 0;

    public static bool MissilePass(bool isMissile, int flags) =>
        isMissile && (flags & RgfMissiles) != 0;

    public static bool FilterSpeciesCombined(bool filterPass, bool speciesPass, int flags) =>
        (flags & RgfEither) != 0 ? filterPass || speciesPass : filterPass && speciesPass;

    public static bool PointerRelationPass(bool targetPass, bool masterPass, bool tracerPass, int flags) =>
        (flags & RgfInclusive) != 0
            ? targetPass || masterPass || tracerPass
            : targetPass && masterPass && tracerPass;

    public static bool OutsideCube(double dx, double dy, double dz, double distance, double mindist) =>
        dx > distance || dy > distance || dz > distance
        || (mindist > 0 && dx < mindist && dy < mindist && dz < mindist);

    public static bool OutsideSphere(double lengthSquared, double distance, double mindist) =>
        lengthSquared > distance * distance
        || (mindist > 0 && lengthSquared < mindist * mindist);
}
