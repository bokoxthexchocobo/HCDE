namespace HCDE.Playsim;

/// <summary>
/// Player search and <c>A_Look</c> from <c>p_enemy.cpp</c> (<b>1778–2062</b>).
/// Chunk <b>1149</b>. The MBF friend block is not compiled. <c>A_LookEx</c> starts at <b>2063</b>.
/// </summary>
public static class VmPEnemyWake1149Slice
{
    public const int MaxPlayers = 64;
    public const int DontChaseGoal = 4;
    public const int HuntPlayers = unchecked((int)0x80000000);
    public const int Friendly = 0x08000000;
    public const int SeeFriendlyMonsters = 0x08000000;
    public const int NoSector = 0x00000008;
    public const int Ambush = 0x00000020;
    public const int Stealth = 0x40000000;
    public const int Boss = 0x00008000;
    public const int Shootable = 0x00000004;
    public const int LookAllAround = 0x00010000;
    public const int ChaseGoal = 0x00000080;
    public const int InConversation = 0x20000000;
    public const int FullVolumeSee = 0x00010000;
    public const int NoTarget = 1 << 3;
    public const int CompatSoundTarget = 1 << 11;
    public const int DoomStrifeChex = 1 | 8 | 16;
    public const int TitleLevel = 7;
    public const int ThingSetGoal = 229;
    public const int TicRate = 35;
    public const int TeamNone = 255;

    public readonly record struct Heard(bool Exists, int Health, bool IsPlayer, int Cheats, bool Friendly, bool Shootable, bool IsFriend, bool IsGoal);

    public readonly record struct PlayerLook(
        bool Found,
        bool ClaimedWithoutTarget,
        int? PlayerSlot,
        bool TookGoal,
        bool TookLastEnemy,
        bool ClearedLastEnemy,
        bool ClearedReaction,
        int? LastLook,
        bool LookedForMonsters,
        bool LookedForEnemies);

    public readonly record struct Wake(
        bool Stopped,
        bool ThresholdCleared,
        bool GoalSet,
        int Special,
        int ReactionTime,
        int Flags5,
        int? VisDir,
        int LookCalls,
        bool Wandered,
        bool EarlySeeState,
        bool KeptHeardTarget,
        bool ClearedGoalTarget,
        bool SeeSound,
        bool FullVolume,
        bool EnteredSeeState);

    public static PlayerLook LookForPlayers(
        int tidToHate,
        bool foundTid,
        int flags,
        int flags3,
        int flags8,
        int friendPlayer,
        bool teamplay,
        int team,
        bool deathmatch,
        bool foundEnemies,
        bool hasParams,
        int lookFlags,
        int gameType,
        bool primaryLevel,
        bool multiplayer,
        int player0Health,
        bool hasGoal,
        int gameState,
        bool foundMonsters,
        int look2Roll,
        int lastLookPlayer,
        bool hasTarget,
        bool targetIsGoal,
        bool lastEnemyAlive,
        bool lastEnemyIsFriend,
        Func<int, bool>? playerInGame,
        Func<int, bool>? targetable)
    {
        bool chaseGoal = hasParams ? (lookFlags & DontChaseGoal) == 0 : true;
        if (tidToHate != 0)
        {
            if (foundTid)
                return new PlayerLook(true, false, null, false, false, false, false, null, false, false);
            if ((flags3 & HuntPlayers) == 0)
                return new PlayerLook(false, false, null, false, false, false, false, null, false, false);
        }
        else if ((flags & Friendly) != 0)
        {
            if (friendPlayer == 0 && (!teamplay || team == TeamNone))
                return new PlayerLook(foundEnemies, false, null, false, false, false, false, null, false, true);
            if (foundEnemies || !deathmatch)
                return new PlayerLook(true, !foundEnemies, null, false, false, false, false, null, false, true);
        }
        else if ((flags8 & SeeFriendlyMonsters) != 0)
        {
            if (foundEnemies)
                return new PlayerLook(true, false, null, false, false, false, false, null, false, true);
        }

        if ((gameType & DoomStrifeChex) == 0 && primaryLevel && !multiplayer && player0Health <= 0 && !hasGoal && gameState != TitleLevel)
            return new PlayerLook(foundMonsters, false, null, false, false, false, false, null, true, false);

        int pnum = tidToHate != 0 ? (look2Roll & (MaxPlayers - 1)) : lastLookPlayer;
        int? lastLook = null;
        for (int counted = 0; ; counted++)
        {
            if (counted < MaxPlayers)
            {
                pnum = (pnum + 1) & (MaxPlayers - 1);
                if (playerInGame == null || !playerInGame(pnum))
                    continue;
                if (tidToHate == 0)
                    lastLook = pnum;
                if (targetable != null && targetable(pnum))
                {
                    bool clearReaction = hasGoal && targetIsGoal;
                    return new PlayerLook(true, false, pnum, false, false, false, clearReaction, lastLook, false, false);
                }
            }
            else
            {
                bool clearedLast = false;
                if (!hasTarget)
                {
                    if (hasGoal && chaseGoal)
                        return new PlayerLook(true, false, null, true, false, false, false, lastLook, false, false);
                    if (lastEnemyAlive)
                    {
                        if (!lastEnemyIsFriend)
                            return new PlayerLook(true, false, null, false, true, true, false, lastLook, false, false);
                        clearedLast = true;
                    }
                }

                bool already = hasTarget && targetIsGoal && hasGoal;
                return new PlayerLook(already, false, null, false, false, clearedLast, false, lastLook, false, false);
            }
        }
    }

    public static Wake Look(
        bool inConversation,
        int special,
        int arg0,
        int arg2,
        int arg3,
        int mapTime,
        bool patrolFound,
        int reactionTime,
        int flags,
        int flags2,
        int flags5,
        int flags8,
        int tidToHate,
        bool compatSoundTarget,
        Heard sectorSound,
        Heard lastHeard,
        Heard currentTarget,
        bool ambushSight,
        bool lookTargetIsGoal,
        bool seeSoundValid,
        bool hasSeeState,
        IReadOnlyList<bool> looks)
    {
        if (inConversation)
            return new Wake(true, false, false, special, reactionTime, flags5, null, 0, false, false, false, false, false, false, false);

        bool goalSet = false;
        if (special == ThingSetGoal && arg0 == 0)
        {
            goalSet = patrolFound;
            special = 0;
            reactionTime = arg2 * TicRate + mapTime;
            if (arg3 == 0)
                flags5 &= ~ChaseGoal;
            else
                flags5 |= ChaseGoal;
        }

        Heard heard = currentTarget;
        if (tidToHate == 0)
            heard = compatSoundTarget || (flags & NoSector) != 0 ? sectorSound : lastHeard;
        if (tidToHate == 0 && heard.Exists && heard.Health <= 0)
            heard = default;
        if (tidToHate == 0 && heard.Exists && heard.IsPlayer && ((heard.Cheats & NoTarget) != 0 || !heard.Friendly))
            return new Wake(true, true, goalSet, special, reactionTime, flags5, null, 0, false, false, false, false, false, false, false);

        int? visDir = (flags & Stealth) != 0 ? -1 : null;
        int calls = 0;
        bool wandered = false;
        bool earlySee = false;
        bool haveTarget = false;
        bool onGoal = false;
        bool keptHeard = false;

        bool NextLook()
        {
            bool found = looks[calls];
            calls++;
            return found;
        }

        if (heard.Exists && heard.Shootable)
        {
            if (heard.IsFriend)
            {
                if (NextLook())
                {
                    haveTarget = true;
                    onGoal = lookTargetIsGoal;
                }
                else if (hasSeeState)
                {
                    earlySee = true;
                }
                else
                {
                    wandered = true;
                }
            }
            else if ((flags & Ambush) == 0 || ambushSight)
            {
                haveTarget = true;
                onGoal = heard.IsGoal;
            }
            else
            {
                keptHeard = true;
            }
        }

        if (!haveTarget)
        {
            if (!NextLook())
                return new Wake(true, true, goalSet, special, reactionTime, flags5, visDir, calls, wandered, earlySee, keptHeard, false, false, false, false);
            haveTarget = true;
            onGoal = lookTargetIsGoal;
            keptHeard = false;
        }

        bool clearedGoal = false;
        bool seeSound = false;
        bool fullVolume = false;
        if (onGoal)
        {
            if (reactionTime > mapTime)
            {
                clearedGoal = true;
                haveTarget = false;
            }
        }
        else if (seeSoundValid)
        {
            seeSound = true;
            fullVolume = (flags2 & Boss) != 0 || (flags8 & FullVolumeSee) != 0;
        }

        bool entered = haveTarget && hasSeeState;
        return new Wake(false, true, goalSet, special, reactionTime, flags5, visDir, calls, wandered, earlySee, false, clearedGoal, seeSound, fullVolume, entered);
    }
}
