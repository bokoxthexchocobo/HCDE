namespace HCDE.Playsim;

/// <summary>
/// Monster sight, targetable players, block enemies, and the look-for-enemies
/// scan from <c>p_enemy.cpp</c> (<b>1258–1488</b>). Chunk <b>1148</b>.
/// <c>P_LookForMonsters</c> is already credited from <b>1489</b>.
/// </summary>
public static class VmPEnemyLook1148Slice
{
    public const int Shootable = 0x00000004;
    public const int Shadow = 0x00040000;
    public const int Dormant = 0x10000000;
    public const int Ghost = 0x00000008;
    public const int IsMonster = 0x00002000;
    public const int SeeInvisible = 0x01000000;
    public const int NeverTarget = 0x00000001;
    public const int Friendly = 0x08000000;
    public const int MonsterVisBlocked = 0x00002000;
    public const int SeeFriendlyMonsters = 0x08000000;
    public const int NoTarget = 1 << 3;
    public const int CompatInvisibility = 1 << 16;
    public const int SeePastShootableLines = 2;
    public const int MapBlockUnits = 128;
    public const int MonsLookRange = 20 * 64;
    public const int MonsLookLimit = 64;
    public const double SneakSpeedSquared = 25;

    public readonly record struct LookCandidate(bool HasPlayer, bool ValidEnemy, bool TargetablePlayer);

    public readonly record struct EnemyLook(bool Aborted, double Range, int Accepted);

    public static bool IsVisible(
        bool otherExists,
        int otherFlags8,
        bool hasParams,
        double paramMaxDist,
        double paramMinDist,
        double paramFov,
        bool allAround,
        double distance,
        double angleDelta,
        double meleeRange,
        double radius,
        bool hasSight)
    {
        if (!otherExists || (otherFlags8 & MonsterVisBlocked) != 0)
            return false;

        double maxDist;
        double minDist;
        double fov;
        if (hasParams)
        {
            maxDist = paramMaxDist;
            minDist = paramMinDist;
            fov = allAround ? 0 : paramFov;
        }
        else
        {
            maxDist = 0;
            minDist = 0;
            fov = allAround ? 0 : 180;
        }

        if (maxDist != 0 && distance > maxDist)
            return false;
        if (minDist != 0 && distance < minDist)
            return false;
        if (fov != 0 && angleDelta > fov / 2)
        {
            if (minDist != 0 || distance > meleeRange + radius)
                return false;
        }

        return hasSight;
    }

    public static bool IsTargetablePlayer(
        bool pawnExists,
        int pawnFlags,
        int pawnFlags3,
        bool isFriend,
        int cheats,
        int playerHealth,
        bool visible,
        int lookerFlags6,
        int compatFlags,
        double distance,
        double speedSquared,
        int lookRoll)
    {
        if (!pawnExists)
            return false;
        if ((pawnFlags & Shootable) == 0)
            return false;
        if (isFriend)
            return false;
        if ((cheats & NoTarget) != 0)
            return false;
        if (playerHealth <= 0)
            return false;
        if (!visible)
            return false;
        if ((lookerFlags6 & SeeInvisible) == 0)
        {
            bool hidden = ((pawnFlags & Shadow) != 0 && (compatFlags & CompatInvisibility) == 0) || (pawnFlags3 & Ghost) != 0;
            if (hidden)
            {
                if (distance > 128 && speedSquared < SneakSpeedSquared)
                    return false;
                if (lookRoll < 225)
                    return false;
            }
        }

        return true;
    }

    public static bool ValidEnemy(
        int lookerFlags,
        int lookerFlags8,
        int otherFlags,
        int otherFlags2,
        int otherFlags3,
        int otherFlags7,
        int otherHealth,
        bool isSelf,
        bool otherIsFriend,
        bool otherHasTarget,
        int targetFlags,
        int targetFlags2,
        int targetHealth,
        double targetSpawnHealth,
        bool targetTargetsOther,
        bool lookerIsFriendOfTarget,
        int skipRoll,
        bool visible)
    {
        if ((otherFlags & Shootable) == 0)
            return false;
        if (isSelf)
            return false;
        if (otherHealth <= 0)
            return false;
        if ((otherFlags2 & Dormant) != 0)
            return false;
        if ((otherFlags3 & IsMonster) == 0)
            return false;
        if ((otherFlags7 & NeverTarget) != 0)
            return false;

        bool keep = false;
        if ((lookerFlags & Friendly) != 0)
        {
            if ((otherFlags & Friendly) != 0)
            {
                if (!otherIsFriend)
                {
                    keep = true;
                }
                else if (otherHasTarget && (targetFlags & Friendly) == 0)
                {
                    if ((targetFlags & Shootable) == 0 || targetHealth <= 0 || (targetFlags2 & Dormant) != 0)
                        return false;
                }
            }
            else
            {
                keep = true;
            }
        }
        else if ((lookerFlags8 & SeeFriendlyMonsters) != 0 && (otherFlags & Friendly) != 0)
        {
            keep = true;
        }

        if (keep && otherHasTarget && targetTargetsOther && skipRoll > 100 && lookerIsFriendOfTarget && targetHealth * 2 >= targetSpawnHealth)
            return false;
        if (!keep || !visible)
            return false;
        return true;
    }

    public static EnemyLook LookForEnemies(
        bool hasTargetList,
        double range,
        double friendlySeeBlocks,
        bool noPlayers,
        int initialCount,
        IReadOnlyList<LookCandidate> things)
    {
        if (!hasTargetList)
            return new EnemyLook(true, range, 0);
        if (range == -1)
            range = friendlySeeBlocks * MapBlockUnits;
        int accepted = initialCount;
        foreach (LookCandidate thing in things)
        {
            if ((!thing.HasPlayer && thing.ValidEnemy) || (!noPlayers && thing.HasPlayer && thing.TargetablePlayer))
                accepted++;
        }
        return new EnemyLook(false, range, accepted);
    }
}
