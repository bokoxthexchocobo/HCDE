namespace HCDE.Playsim;

/// <summary>
/// Recursive sound lines, the noise alert, and melee range from <c>p_enemy.cpp</c>
/// (<b>151–300</b>). Chunk <b>1145</b>. Missile range begins at <b>307</b>.
/// </summary>
public static class VmPEnemyMelee1145Slice
{
    public const int TwoSided = 4;
    public const int SoundBlock = 0x40;
    public const int PortalSound = 4;
    public const int NoTarget = 1 << 3;
    public const int SectorNoAttack = 2048;
    public const int NoVerticalMelee = 0x04000000;

    public static bool MarkAdjacentPlane(bool planeAllowsSound)
    {
        if (planeAllowsSound)
            return true;
        return false;
    }

    public static bool MarkSoundPortal(bool hasPortal, int portalFlags, bool hasDestination)
    {
        if (hasPortal && (portalFlags & PortalSound) != 0 && hasDestination)
            return true;
        return false;
    }

    public static bool DoorBlocksSound(
        double sectorFloor1, double otherCeiling1, double sectorFloor2, double otherCeiling2,
        double otherFloor1, double sectorCeiling1, double otherFloor2, double sectorCeiling2)
    {
        if (sectorFloor1 >= otherCeiling1 && sectorFloor2 >= otherCeiling2)
            return true;
        if (otherFloor1 >= sectorCeiling1 && otherFloor2 >= sectorCeiling2)
            return true;
        if (otherFloor1 >= otherCeiling1 && otherFloor2 >= otherCeiling2)
            return true;
        return false;
    }

    public static (bool Visit, int SoundBlocks) VisitOtherSector(
        bool hasBack, int lineFlags, bool sameSector, bool closed, int soundBlocks)
    {
        if (!hasBack || (lineFlags & TwoSided) == 0)
            return (false, soundBlocks);
        if (sameSector || closed)
            return (false, soundBlocks);
        if ((lineFlags & SoundBlock) != 0)
        {
            if (soundBlocks == 0)
                return (true, 1);
            return (false, soundBlocks);
        }
        return (true, soundBlocks);
    }

    public static (bool Run, int ValidCount) BeginNoiseAlert(
        bool hasEmitter, bool targetIsPlayer, int playerCheats, int validCount)
    {
        if (!hasEmitter)
            return (false, validCount);
        if (targetIsPlayer && (playerCheats & NoTarget) != 0)
            return (false, validCount);
        return (true, validCount + 1);
    }

    public static int CheckMeleeRange(
        bool hasTarget,
        double distance,
        double range,
        double meleeRange,
        double targetRadius,
        bool targetIsGoal,
        int sectorFlags,
        int actorFlags5,
        double targetZ,
        double actorTop,
        double targetTop,
        double actorZ,
        bool isFriend,
        bool hasSight)
    {
        if (!hasTarget)
            return 0;
        if (range < 0)
            range = meleeRange;
        if (distance >= range + targetRadius)
            return 0;
        if (targetIsGoal)
            return 1;
        if ((sectorFlags & SectorNoAttack) != 0)
            return 0;
        if ((actorFlags5 & NoVerticalMelee) == 0)
        {
            if (targetZ > actorTop)
                return 0;
            if (targetTop < actorZ)
                return 0;
        }
        if (isFriend)
            return 0;
        if (!hasSight)
            return 0;
        return 1;
    }
}
