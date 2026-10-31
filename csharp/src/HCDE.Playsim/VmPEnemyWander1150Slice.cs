namespace HCDE.Playsim;

/// <summary>
/// Clear-last-heard, wander, and <c>A_Look2</c> from <c>p_enemy.cpp</c>
/// (<b>2245–2396</b>). Chunk <b>1150</b>. <c>A_DoChase</c> begins at <b>2397</b>.
/// </summary>
public static class VmPEnemyWander1150Slice
{
    public const int InCombat = 0x00008000;
    public const int StandStill = 0x00020000;
    public const int InConversation = 0x20000000;
    public const int Friendly = 0x08000000;
    public const int Ambush = 0x00000020;
    public const int Shootable = 0x00000004;
    public const int NoAllies = 0x20000000;
    public const int NoDir = 8;
    public const int SeePastBlockEverything = 4;
    public const int BossStrafeRange = 64 * 10;

    public const int FastChase = 1;
    public const int NoPlayActive = 2;
    public const int NightmareFast = 4;
    public const int Resurrect = 8;
    public const int DontMove = 16;
    public const int NoRandomTurn = 32;
    public const int NoDirectionTurn = 64;
    public const int NoPostAttackTurn = 128;
    public const int StopIfBlocked = 256;
    public const int DontIdle = 512;
    public const int DontLookAllAround = 1024;

    public readonly record struct Wander(
        int Flags4,
        int ReactionTime,
        double Yaw,
        int MoveCount,
        bool ChoseNewDirection);

    public readonly record struct Look2(
        bool Stopped,
        bool ThresholdCleared,
        int Threshold,
        bool Saw,
        bool InCombat,
        int? SpawnFrame,
        int? IdleFrame);

    public static double Normalize180(double degrees)
    {
        double rounded = Math.Round(degrees * (0x40000000 / 90d), MidpointRounding.ToEven);
        uint wrapped = unchecked((uint)(long)rounded);
        int signedBam = unchecked((int)wrapped);
        return (90d / 0x40000000) * signedBam;
    }

    public static bool ClearLastHeard(bool hadHeard)
    {
        hadHeard = false;
        return hadHeard;
    }

    public static Wander WanderStep(
        int flags4,
        int flags5,
        int reactionTime,
        int moveDir,
        double yaw,
        int moveCount,
        int chaseFlags,
        Func<bool> smartMove)
    {
        flags4 &= ~InCombat;
        if ((flags5 & InConversation) != 0 || (flags4 & StandStill) != 0)
            return new Wander(flags4, reactionTime, yaw, moveCount, false);
        if (reactionTime != 0)
            return new Wander(flags4, reactionTime - 1, yaw, moveCount, false);

        if ((chaseFlags & NoDirectionTurn) == 0 && moveDir < NoDir)
        {
            yaw = Math.Floor(yaw / 45d) * 45d;
            double delta = Normalize180(moveDir * 45d - yaw);
            if (delta < 0)
                yaw -= 45;
            else if (delta > 0)
                yaw += 45;
        }

        moveCount--;
        bool chase = (moveCount < 0 && (chaseFlags & NoRandomTurn) == 0)
            || (!smartMove() && (chaseFlags & StopIfBlocked) == 0);
        if (chase)
            moveCount += 5;
        return new Wander(flags4, reactionTime, yaw, moveCount, chase);
    }

    public static Look2 Look(
        int flags,
        int flags4,
        int flags5,
        int levelFlags,
        bool heardExists,
        int heardHealth,
        int heardFlags,
        bool ambushSight,
        bool foundPlayer,
        IReadOnlyList<int> rolls)
    {
        if ((flags5 & InConversation) != 0)
            return new Look2(true, false, 0, false, false, null, null);

        int roll = 0;
        int Next()
        {
            int value = rolls[roll];
            roll++;
            return value;
        }

        bool alive = heardExists && heardHealth > 0;
        if (alive && (heardFlags & Shootable) != 0)
        {
            bool hostile = (levelFlags & NoAllies) != 0
                || ((flags & Friendly) != 0) != ((heardFlags & Friendly) != 0);
            if (hostile)
            {
                if ((flags & Ambush) == 0 || ambushSight)
                    return new Look2(false, true, 10, true, false, null, null);
            }
            else if (foundPlayer)
            {
                return new Look2(false, true, 0, true, true, null, null);
            }
        }

        int? fidget = null;
        int first = Next();
        if (first < 30)
            fidget = (Next() & 1) + 1;
        int? idle = null;
        if ((flags4 & StandStill) == 0 && Next() < 40)
            idle = 3;
        return new Look2(false, true, 0, false, false, fidget, idle);
    }
}
