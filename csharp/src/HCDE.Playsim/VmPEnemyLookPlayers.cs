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

    public static bool TidHateUsesLookForTid(int tidHate) => tidHate != 0;

    public static bool HuntPlayersRequired(int flags3) => (flags3 & Mf3Huntplayers) != 0;

    public static bool SeeFriendlyMonstersBranch(int flags8) => (flags8 & 1) != 0;

    public static bool SinglePlayerDeadMonsters(bool doomFamily, bool primary, bool mp, int p0Health, bool goalNull, bool title) =>
        doomFamily && primary && !mp && p0Health <= 0 && goalNull && !title;

    public static bool PlayerRotationContinue(int count) => count < 32;


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

    public static bool LookAllaround(int flags4) => (flags4 & Mf4Lookallaround) != 0;

    public static bool AmbushNeedsSight(bool ambush, bool sight) => ambush && !sight;

    public static bool GoalReactionGate(bool targetIsGoal, int maptime, int reactiontime) =>
        targetIsGoal && reactiontime > maptime;

    public static bool BossSeeSound(bool boss, bool fullVol) => boss || fullVol;

    public static bool ReactiontimeZeroOnGoal(bool hasGoal, bool targetIsGoal) => hasGoal && targetIsGoal;

    public static bool DeathmatchFriendlyBranch(bool friendPlayerZero, bool teamplay, int team) =>
        friendPlayerZero && (!teamplay || team == 0);

    public static bool DeathmatchFallthrough(bool result, bool dm) => result || !dm;

    public static bool PrLook2Pnum(int tidHate) => tidHate != 0;

    public static bool DoneLookingUseGoal(bool targetNull, bool chase) => targetNull && chase;

    public static bool DoneLookingLastEnemy(bool targetNull, bool hasLast, int health, bool friend) =>
        targetNull && hasLast && health > 0 && !friend;

    public static bool DoneReturnGoalTarget(bool targetIsGoal, bool goalNonNull) => targetIsGoal && goalNonNull;

    public static bool ThingSetGoalSpecial(int special) => special == 14;

    public static bool ArgsChaseGoal(int arg3) => arg3 != 0;

    public static bool FullVolumeSee(bool boss, bool fullvol) => boss || fullvol;

    public static bool TargetAndSeeState(bool target, bool see) => target && see;



    public static bool AmbushWithoutSight(bool ambush, bool sight) => ambush && !sight;


    public static bool LookExNoSoundCheck(int flags) => (flags & 1) == 0;

    public static bool MaxHearDistBlocks(double dist, double max) => max != 0 && dist > max;

    public static bool NoJumpFlag(int flags) => (flags & 256) != 0;

    public static bool FullVolSeeFlag(int flags) => (flags & 512) != 0;

    public static bool NoSeeSoundFlag(int flags) => (flags & 1024) != 0;

    public static bool NoSightCheckFlag(int flags) => (flags & 2048) != 0;

    public static bool MinSeeDist(double dist, double min) => min == 0 || dist > min;

    public static bool MaxSeeDist(double dist, double max) => max == 0 || dist < max;

}
