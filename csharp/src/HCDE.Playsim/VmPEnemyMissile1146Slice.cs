namespace HCDE.Playsim;

/// <summary>
/// Missile range, friend-in-the-way, lift, and crushing-ceiling direction from
/// <c>p_enemy.cpp</c> (<b>301–450</b>). Chunk <b>1146</b>. Sector-damage under a
/// crusher continues at <b>451</b>.
/// </summary>
public static class VmPEnemyMissile1146Slice
{
    public const int SectorNoAttack = 2048;
    public const int SeePastBlockEverything = 4;
    public const int JustHit = 0x40;
    public const int Friendly = 0x08000000;
    public const int Lift = 2048;
    public const int SkillAggressiveness = 5;
    public const int CeilingUp = 1;
    public const int CeilingWaiting = 0;
    public const int CeilingDown = -1;

    public readonly record struct MissileDecision(bool Attack, bool ClearedJustHit);

    public readonly record struct CrushContact(bool CrushingCeiling, int Direction);

    public static MissileDecision CheckMissileRange(
        int sectorFlags,
        bool hasSight,
        int actorFlags,
        int targetHealth,
        bool isFriend,
        bool targetIsPlayer,
        int targetFlags,
        int defectRoll,
        int reactionTime,
        bool hitFriend,
        double distance2D,
        bool hasMeleeState,
        double maxTargetRange,
        double meleeThreshold,
        double missileChanceMult,
        double minMissileChance,
        double aggressiveness,
        int missileRangeRoll)
    {
        if ((sectorFlags & SectorNoAttack) != 0)
            return new MissileDecision(false, false);
        if (!hasSight)
            return new MissileDecision(false, false);
        if ((actorFlags & JustHit) != 0)
        {
            if ((actorFlags & Friendly) == 0)
                return new MissileDecision(true, true);
            if (targetHealth <= 0)
                return new MissileDecision(false, true);
            if (!isFriend)
                return new MissileDecision(true, true);
            if (targetIsPlayer)
                return new MissileDecision(defectRoll > 128, true);
            bool strike = (targetFlags & JustHit) == 0 && defectRoll > 128;
            return new MissileDecision(strike, true);
        }

        if (reactionTime != 0)
            return new MissileDecision(false, false);
        if (isFriend)
            return new MissileDecision(false, false);
        if ((actorFlags & Friendly) != 0 && hitFriend)
            return new MissileDecision(false, false);

        double dist = distance2D - 64;
        if (!hasMeleeState)
            dist -= 128;
        if (maxTargetRange > 0 && dist > maxTargetRange)
            return new MissileDecision(false, false);
        if (hasMeleeState && dist < meleeThreshold)
            return new MissileDecision(false, false);
        dist *= missileChanceMult;
        int mmc = (int)(minMissileChance * aggressiveness);
        int threshold = Math.Min((int)dist, mmc);
        return new MissileDecision(missileRangeRoll >= threshold, false);
    }

    public static bool HitFriend(int flags, bool hasTarget, bool lineTargetIsOther, bool otherIsFriend)
    {
        if ((flags & Friendly) != 0 && hasTarget)
        {
            if (lineTargetIsOther)
                return otherIsFriend;
        }
        return false;
    }

    public static bool IsOnLift(bool floorIsPlat, int sectorMoreFlags)
    {
        if (floorIsPlat)
            return true;
        if ((sectorMoreFlags & Lift) != 0)
            return true;
        return false;
    }

    public static int AccumulateCrushDirection(IReadOnlyList<CrushContact> contacts)
    {
        int dir = 0;
        foreach (CrushContact contact in contacts)
        {
            if (contact.CrushingCeiling)
                dir |= contact.Direction;
        }
        return dir;
    }
}
