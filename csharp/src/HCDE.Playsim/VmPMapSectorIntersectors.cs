namespace HCDE.Playsim;

/// <summary>
/// Sector height adjust and intersector discovery from <c>p_map.cpp</c> (6482–6700).
/// </summary>
public static class VmPMapSectorIntersectors
{
    public const int Mf2Passmobj = 1;
    public const int Mf3Ismonster = 1;
    public const int Mf4Actlikebridge = 1;
    public const int MfNoclip = 1;
    public const int MfSolid = 1 << 1;
    public const int MfSpecial = 1 << 2;
    public const int MfCorpse = 1 << 3;
    public const int MfIcecorpse = 1 << 4;
    public const int Compatf2DrCrusher = 1;

    public static bool PassMobjMonsterCheck(int flags2, int flags3) =>
        (flags2 & Mf2Passmobj) != 0 || (flags3 & Mf3Ismonster) != 0;

    public static bool MoveMidtexForcesPassmobj(bool moveMidtex) => moveMidtex;

    public static bool BridgeSkipsFloorSync(int flags4) => (flags4 & Mf4Actlikebridge) != 0;

    public static bool RestorePassmobjFlag(int savedPassmobj, int currentFlags2) =>
        (currentFlags2 & ~Mf2Passmobj | savedPassmobj) == currentFlags2;

    public static bool SkipNoclip(int flags) => (flags & MfNoclip) != 0;

    public static bool RequiresSolid(int flags) => (flags & MfSolid) == 0;

    public static bool BlockDistOutOfRange(double dx, double dy, double blockDist) =>
        dx >= blockDist || dy >= blockDist;

    public static bool SkipNonSolidThing(int thingFlags) => (thingFlags & MfSolid) == 0;

    public static bool SkipSpecialPickup(int thingFlags) => (thingFlags & MfSpecial) != 0;

    public static bool SkipCorpseUnlessIce(int thingFlags, int actorFlags) =>
        (thingFlags & MfCorpse) != 0 && (actorFlags & MfIcecorpse) == 0;

    public static bool SkipSelf(bool same) => same;

    public readonly record struct IntersectorPairFlags(
        int ActorFlags2,
        int ThingFlags2,
        int ActorFlags3,
        int ThingFlags3);

    public static bool SkipAlwaysBlockingPair(IntersectorPairFlags f) =>
        ((f.ThingFlags2 | f.ActorFlags2) & Mf2Passmobj) == 0
        && ((f.ThingFlags3 | f.ActorFlags3) & Mf3Ismonster) == 0;

    public static bool AboveIntersection(double thingZ, double actorZ, double actorTop) =>
        thingZ > actorZ && thingZ <= actorTop;

    public static bool BelowIntersection(double thingTop, double actorTop, double actorZ) =>
        thingTop <= actorTop && thingTop > actorZ;

    public static bool CrusherCompatSkipsCorpse(
        int compatFlags2,
        bool hasPlayer,
        int thingFlags) =>
        (compatFlags2 & Compatf2DrCrusher) != 0
        && !hasPlayer
        && (thingFlags & MfCorpse) != 0
        && (thingFlags & MfIcecorpse) == 0;

    public static bool MarkNoFit(bool crushPositive) => crushPositive;

    public static bool ApplyCrushDamageOnCadence(int crushChange, int mapTime) =>
        crushChange > 0 && (mapTime & 3) == 0;

    public static bool FromPMoveMonster(bool passmobj, bool isMonster) =>
        passmobj && isMonster;

    public static bool OrPassmobjFlags(int current, int passmobjBit) => (current | passmobjBit) != 0;

    public readonly record struct BlockIteratorSkipContext(
        double Dx,
        double Dy,
        double BlockDist,
        int ThingFlags,
        int ActorFlags,
        bool SameActor,
        int ActorFlags2,
        int ThingFlags2,
        int ActorFlags3,
        int ThingFlags3);

    public static bool ShouldSkipIteratorCandidate(BlockIteratorSkipContext ctx) =>
        BlockDistOutOfRange(ctx.Dx, ctx.Dy, ctx.BlockDist)
        || SkipNonSolidThing(ctx.ThingFlags)
        || SkipSpecialPickup(ctx.ThingFlags)
        || SkipCorpseUnlessIce(ctx.ThingFlags, ctx.ActorFlags)
        || SkipSelf(ctx.SameActor)
        || SkipAlwaysBlockingPair(new IntersectorPairFlags(
            ctx.ActorFlags2, ctx.ThingFlags2, ctx.ActorFlags3, ctx.ThingFlags3));

    public static bool CrushRequiresGrind(bool grindResult, bool hasCpos) => grindResult && hasCpos;

    public static bool InvulnerableOrDormant(int flags2) => (flags2 & (1 | 2)) != 0;

    public static bool HideBloodWhenTypeLow(int bloodType) => bloodType <= 1;

    public static bool PlayCrushSoundWhenIdle(bool soundPlaying) => !soundPlaying;
}
