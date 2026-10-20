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

    public static bool FudgeNullBacksector(bool backIsNull) => backIsNull;

    public static bool AdjustCeilingWhenLower(double openTop, double zofs, double ceilingZ) =>
        openTop - zofs < ceilingZ;

    public static bool AdjustFloorWhenHigher(double openBottom, double zofs, double floorZ) =>
        openBottom - zofs > floorZ;

    public static bool UpdateDropoffWhenLower(double lowFloor, double zofs, double dropoffZ) =>
        lowFloor - zofs < dropoffZ;

    public static bool RestoreBacksectorAfterProbe(bool hadBack) => hadBack;

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

    public static bool PlayersAlwaysFight() => true;

    public static bool MonstersBlockedWhenNoInfight(int infight, bool bothMonsters) =>
        infight < 0 && bothMonsters;

    public static bool HostileAllowsDamage(bool hostile) => hostile;

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
}
