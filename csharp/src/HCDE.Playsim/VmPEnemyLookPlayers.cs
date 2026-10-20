namespace HCDE.Playsim;

/// <summary><c>P_LookForPlayers</c> and <c>A_Look</c> from <c>p_enemy.cpp</c> (1765–2054).</summary>
public static class VmPEnemyLookPlayers
{
    public const int MfFriendly = 1;
    public const int Mf3Huntplayers = 1;
    public const int Mf4Lookallaround = 1;
    public const int MfStealth = 1;
    public const int MfShootable = 1;
    public const int MfAmbush = 1;
    public const int Mf2Boss = 1;
    public const int Mf8Fullvolsee = 1;
    public const int Mf5Inconversation = 1;
    public const int CfNotarget = 1;
    public const int LofDontchasegoal = 1;

    public static bool LastEnemyRetarget(bool hasLast, int health, bool isFriend) =>
        hasLast && health > 0 && !isFriend;

    public static bool ClearLastEnemyWhenFriend(bool isFriend) => isFriend;

    public static bool TidHateUsesLookForTid(int tidHate) => tidHate != 0;

    public static bool HuntPlayersRequired(int flags3) => (flags3 & Mf3Huntplayers) != 0;

    public static bool FriendlyLooksEnemies(bool friendly) => friendly;

    public static bool SeeFriendlyMonstersBranch(int flags8) => (flags8 & 1) != 0;

    public static bool SinglePlayerDeadMonsters(bool doomFamily, bool primary, bool mp, int p0Health, bool goalNull, bool title) =>
        doomFamily && primary && !mp && p0Health <= 0 && goalNull && !title;

    public static bool PlayerRotationContinue(int count) => count < 32;

    public static bool SkipNotInGame(bool inGame) => !inGame;

    public static bool RememberLastLookPlayer(bool tidZero) => tidZero;

    public static bool GoalFallback(bool targetNull, bool hasGoal, bool chaseGoal) =>
        targetNull && hasGoal && chaseGoal;

    public static bool ConversationExit(int flags5) => (flags5 & Mf5Inconversation) != 0;

    public static bool PatrolGoalSetup(int special, int arg0) => special == 14 && arg0 == 0;

    public static bool ChaseGoalFlag(int arg3) => arg3 != 0;

    public static bool SoundTargetCompat(bool compat, bool nosector, bool useSector) =>
        compat || nosector ? useSector : !useSector;

    public static bool DeadSoundTargetSkips(int health) => health <= 0;

    public static bool PlayerNotargetSkips(bool notarget, bool friendly) => notarget || !friendly;

    public static bool StealthVisdir(int flags) => (flags & MfStealth) != 0;

    public static bool ShootableTarget(int flags) => (flags & MfShootable) != 0;

    public static bool FriendlyTargetBranch(bool isFriend) => isFriend;

    public static bool LookAllaround(int flags4) => (flags4 & Mf4Lookallaround) != 0;

    public static bool WanderWhenNoSee(bool seeNull) => seeNull;

    public static bool AmbushNeedsSight(bool ambush, bool sight) => ambush && !sight;

    public static bool GoalReactionGate(bool targetIsGoal, int maptime, int reactiontime) =>
        targetIsGoal && reactiontime > maptime;

    public static bool BossSeeSound(bool boss, bool fullVol) => boss || fullVol;

    public static bool SetSeeState(bool hasSee) => hasSee;

    public static bool ReactiontimeZeroOnGoal(bool hasGoal, bool targetIsGoal) => hasGoal && targetIsGoal;

    public static bool DeathmatchFriendlyBranch(bool friendPlayerZero, bool teamplay, int team) =>
        friendPlayerZero && (!teamplay || team == 0);

    public static bool DeathmatchFallthrough(bool result, bool dm) => result || !dm;

    public static bool MissileStateReset(bool hasMissile) => hasMissile;

    public static bool ClearJustHit(bool set) => set;

    public static bool PrLook2Pnum(int tidHate) => tidHate != 0;

    public static bool LastLookPlayerAdvance(bool tidZero) => tidZero;

    public static bool DoneLookingUseGoal(bool targetNull, bool chase) => targetNull && chase;

    public static bool DoneLookingLastEnemy(bool targetNull, bool hasLast, int health, bool friend) =>
        targetNull && hasLast && health > 0 && !friend;

    public static bool DoneReturnGoalTarget(bool targetIsGoal, bool goalNonNull) => targetIsGoal && goalNonNull;

    public static bool IsTargetablePlayerGate(bool ok) => ok;

    public static bool PatrolPointIterator(bool hasNext) => hasNext;

    public static bool ThingSetGoalSpecial(int special) => special == 14;

    public static bool ArgsChaseGoal(int arg3) => arg3 != 0;

    public static bool CompatSoundTarget(bool compat) => compat;

    public static bool NosectorSoundTarget(bool nosector) => nosector;

    public static bool LastHeardChain(bool useLast) => useLast;

    public static bool SeeYouLabel(bool reached) => reached;

    public static bool FullVolumeSee(bool boss, bool fullvol) => boss || fullvol;

    public static bool SeeSoundValid(bool valid) => valid;

    public static bool TargetAndSeeState(bool target, bool see) => target && see;

    public static bool WanderAfterFriendlyLook(bool looked) => !looked;

    public static bool SetSeeWhenNoWander(bool seeNull) => seeNull;

    public static bool NonFriendSetsTarget(bool friend) => !friend;

    public static bool AmbushWithoutSight(bool ambush, bool sight) => ambush && !sight;

    public static bool DirectSeeYou(bool nonAmbush) => nonAmbush;

    public static bool LookFailedReturn(bool found) => !found;

    public static bool GoalTimeBlocks(bool blocks) => blocks;

    public static bool AttnNone(bool boss) => boss;

    public static bool AttnNorm(bool notBoss) => notBoss;
}
