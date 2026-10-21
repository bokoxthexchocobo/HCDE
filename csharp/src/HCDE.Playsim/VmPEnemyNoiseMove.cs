namespace HCDE.Playsim;

/// <summary>Noise propagation and move-assist helpers from <c>p_enemy.cpp</c> (122–474).</summary>
public static class VmPEnemyNoiseMove
{
    public const int Mf4Nosplashalert = 1;
    public const int MfFriendly = 1;
    public const int Mf6Falling = 1;
    public const int Mf9Nosectordamage = 1;
    public const int SecmfLift = 1;
    public const int SecmfHurtinair = 1;
    public const int SecmfHurtmonsters = 1;
    public const int PortfSoundtraverse = 1;

    public static bool NoiseAlertEmitterNull(bool emitterNull) => emitterNull;

    public static bool NoiseAlertNoTarget(bool hasTarget, bool noTargetCheat) =>
        hasTarget && noTargetCheat;

    public static bool NoiseSectorAlreadyFlooded(bool sameValidcount, bool traversedBlocks) =>
        sameValidcount && traversedBlocks;

    public static bool NoiseSplashActorAlert(bool splash, int flags4) =>
        !splash || (flags4 & Mf4Nosplashalert) == 0;

    public static bool NoiseWithinMaxDistance(bool hasMaxdist, double distance, double maxdist) =>
        !hasMaxdist || distance <= maxdist;

    public static bool NoisePortalTraverse(bool hasPortal, int portalFlags) =>
        hasPortal && (portalFlags & PortfSoundtraverse) != 0;

    public static bool NoiseLineTwoSided(bool hasBackSide, bool twoSided) =>
        hasBackSide && twoSided;

    public static bool NoiseIntraSectorLine(bool sameSector) => sameSector;

    public static bool NoiseClosedDoor(bool closed) => closed;

    public static bool NoiseSoundBlockLine(bool soundBlock, bool alreadyBlocked) =>
        soundBlock && !alreadyBlocked;

    public static bool HitFriendRequiresFriendly(bool friendly, bool hasTarget) =>
        (friendly ? MfFriendly : 0) != 0 && hasTarget;

    public static bool HitFriendBlocksShot(bool friendlyHit, bool linetargetIsFriend) =>
        friendlyHit && linetargetIsFriend;

    public static bool OnLiftActivePlat(bool hasFloorData, bool isPlat) =>
        hasFloorData && isPlat;

    public static bool OnLiftSectorFlag(int moreFlags) => (moreFlags & SecmfLift) != 0;

    public static bool UnderDamageCrushDirection(int crushDir) => crushDir != 0;

    public static bool SectorDamageHarmInAir(int moreFlags) => (moreFlags & SecmfHurtinair) != 0;

    public static bool SectorDamageHurtMonsters(int moreFlags) => (moreFlags & SecmfHurtmonsters) != 0;

    public static bool SkipSectorDamage(int flags9) => (flags9 & Mf9Nosectordamage) != 0;

    public static bool HarmPlayerInSector(bool hasPlayer) => hasPlayer;

    public static bool HarmMonsterInSector(bool noPlayer, int moreFlags) =>
        noPlayer && SectorDamageHurtMonsters(moreFlags);

    public static bool CheckTagsSameSector(bool sec1HasTags, bool sec2HasTags, bool samePtr) =>
        !sec1HasTags || !sec2HasTags ? samePtr : false;

    public static bool CheckTagsPrimaryMatch(int tag1, int tag2) => tag1 == tag2;

    public readonly record struct NoiseFloodContext(
        bool SameValidcount,
        int Soundtraversed,
        int Soundblocks);

    public static bool ShouldSkipNoiseFlood(NoiseFloodContext ctx) =>
        ctx.SameValidcount && ctx.Soundtraversed <= ctx.Soundblocks + 1;

    public readonly record struct UnderDamageContext(
        int CrushDirection,
        int DamageAmount,
        int MoreFlags,
        bool AtSectorFloor,
        bool InWater,
        bool HasPlayer,
        int Flags9);

    public static int EvaluateUnderDamage(UnderDamageContext ctx)
    {
        if (ctx.CrushDirection != 0)
            return ctx.CrushDirection;
        if (SkipSectorDamage(ctx.Flags9) || ctx.DamageAmount <= 0)
            return 0;
        if (SectorDamageHarmInAir(ctx.MoreFlags) || ctx.AtSectorFloor || ctx.InWater)
        {
            if (HarmPlayerInSector(ctx.HasPlayer))
                return -1;
            if (HarmMonsterInSector(!ctx.HasPlayer, ctx.MoreFlags))
                return 0;
        }
        return 0;
    }
}
