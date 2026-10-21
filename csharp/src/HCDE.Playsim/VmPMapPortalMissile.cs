namespace HCDE.Playsim;

/// <summary><c>PIT_CheckPortal</c>, rip/infight helpers, and <c>P_DoMissileDamage</c> from <c>p_map.cpp</c> (1134–1366).</summary>
public static class VmPMapPortalMissile
{
    public const int FfcfNoceiling = 2;
    public const int FfcfNofloor = 1;
    public const int Mf2Rip = 1;
    public const int Mf6Forcepain = 1;
    public const int Mf7Causepain = 1;
    public const int Mf3Bloodlessimpact = 1;
    public const int MfNoblood = 1;
    public const int Mf2Invulnerable = 1;
    public const int Mf2Dormant = 1;
    public const int Mf2Reflective = 1;
    public const int Mf4Strifedamage = 1;

    public static bool IgnorePortalInVerticalSlice(int portalFlags) =>
        (portalFlags & (FfcfNoceiling | FfcfNofloor)) != 0;

    public static bool SkipWhenBoxMisses(bool inRange, int boxSide) => !inRange || boxSide != -1;

public static bool AdjustCeilingWhenLower(double openTop, double zofs, double ceilingZ) =>
        openTop - zofs < ceilingZ;

    public static bool AdjustFloorWhenHigher(double openBottom, double zofs, double floorZ) =>
        openBottom - zofs > floorZ;

    public static bool UpdateDropoffWhenLower(double lowFloor, double zofs, double dropoffZ) =>
        lowFloor - zofs < dropoffZ;

public static bool CheckRipLevel(int victimMin, int victimMax, int projectileLevel)
    {
        if (victimMin > 0 && projectileLevel < victimMin) return false;
        if (victimMax > 0 && projectileLevel > victimMax) return false;
        return true;
    }

    public static bool ProjectileImmuneDefaultSpecies(bool sameSpecies, bool doHarmSpecies) =>
        sameSpecies && !doHarmSpecies;

    public static bool ProjectileImmuneSameGroup(int targetGroup, int sourceGroup) =>
        targetGroup != 0 && targetGroup == sourceGroup;

public static bool MonstersBlockedWhenNoInfight(int infight, bool bothMonsters) =>
        infight < 0 && bothMonsters;

public static bool HateTidAllowsDamage(int victimTid, int shooterHateTid) =>
        victimTid != 0 && shooterHateTid == victimTid;

    public static bool FriendsBlockedUnlessHarmFriends(bool friends, bool harmFriendsFlag) =>
        friends && !harmFriendsFlag;

    public static bool SharedHateBlocksDamage(int victimHate, int shooterHate) =>
        victimHate != 0 && victimHate == shooterHate;

    public static bool RipperMissile(int flags2) => (flags2 & Mf2Rip) != 0;

    public static int RipperDamageRollSides() => 3;

    public static int NormalMissileDamageRollSides(bool strife) => strife ? 3 : 7;

    public static bool ShouldDealDamage(int damage, int flags6, int flags7) =>
        damage > 0 || (flags6 & Mf6Forcepain) != 0 || (flags7 & Mf7Causepain) != 0;

    public static bool SkipBloodFx(int flags3, int victimFlags, int victimFlags2) =>
        (flags3 & Mf3Bloodlessimpact) != 0
        || (victimFlags & MfNoblood) != 0
        || (victimFlags2 & (Mf2Invulnerable | Mf2Dormant | Mf2Reflective)) != 0;

    public static bool BloodSplatterRoll(int roll, int threshold) => roll < threshold;

    public static bool HealOnZeroDamage(int damage) => damage <= 0;

    public static int HealAmountFromNegativeDamage(int damage) => -damage;

    public const int Mf7Forceinfighting = 1;
    public const int Mf7Harmfriends = 1;
    public const int Mf3Ismonster = 1;
    public const int MfShootable = 4;

    public static int InfightWithForce(int infight, int victimFlags7) =>
        infight < 0 && (victimFlags7 & Mf7Forceinfighting) != 0 ? 0 : infight;

    public static bool NoInfightMonsterBlock(bool shooterShootable, bool victimMonster, bool hostile, int victimTid, int shooterHateTid) =>
        shooterShootable && victimMonster && !hostile && (victimTid == 0 || shooterHateTid != victimTid);

    public static bool FriendBlocksUnlessHarm(bool friends, bool harmFriends) =>
        friends && !harmFriends;

    public static bool SharedHateTeammateBlock(int victimHate, int shooterHate) =>
        victimHate != 0 && victimHate == shooterHate;

    public static bool ProjectileImmuneSpeciesBlock(bool immune, bool hostile, int victimTid, int shooterHateTid) =>
        immune && !hostile && (victimTid == 0 || shooterHateTid != victimTid);

    public readonly record struct CanAttackHurtContext(
        bool VictimPlayer,
        bool ShooterPlayer,
        int Infight,
        int VictimFlags7,
        bool ShooterShootable,
        bool VictimIsMonster,
        bool VictimHostile,
        int VictimTid,
        int ShooterTidToHate,
        bool VictimFriend,
        bool ShooterHarmFriends,
        int VictimTidToHate,
        bool ProjectileImmune);

    /// <summary>Infighting policy for monster/monster damage from <c>CanAttackHurt</c>.</summary>
    public static bool EvaluateCanAttackHurt(CanAttackHurtContext ctx)
    {
        if (ctx.VictimPlayer || ctx.ShooterPlayer) return true;
        var infight = InfightWithForce(ctx.Infight, ctx.VictimFlags7);
        if (infight < 0)
        {
            if (NoInfightMonsterBlock(ctx.ShooterShootable, ctx.VictimIsMonster, ctx.VictimHostile, ctx.VictimTid, ctx.ShooterTidToHate))
                return false;
        }
        else if (infight == 0)
        {
            if (FriendBlocksUnlessHarm(ctx.VictimFriend, ctx.ShooterHarmFriends)) return false;
            if (SharedHateTeammateBlock(ctx.VictimTidToHate, ctx.ShooterTidToHate)) return false;
            if (ProjectileImmuneSpeciesBlock(ctx.ProjectileImmune, ctx.VictimHostile, ctx.VictimTid, ctx.ShooterTidToHate))
                return false;
        }
        return true;
    }
}
