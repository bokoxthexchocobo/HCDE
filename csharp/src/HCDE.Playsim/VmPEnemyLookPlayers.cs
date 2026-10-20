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

    public static bool LookExNoSoundCheck(int flags) => (flags & 1) == 0;

    public static bool MaxHearDistBlocks(double dist, double max) => max != 0 && dist > max;

    public static bool ClearLastHeardOnFar(bool far) => far;

    public static bool StandstillSkips(bool stand) => stand;

    public static bool InchaseSkipsWander(bool inchase) => inchase;

    public static bool SeeStateParam(bool state) => state;

    public static bool NoJumpFlag(int flags) => (flags & 256) != 0;

    public static bool FullVolSeeFlag(int flags) => (flags & 512) != 0;

    public static bool NoSeeSoundFlag(int flags) => (flags & 1024) != 0;

    public static bool NoSightCheckFlag(int flags) => (flags & 2048) != 0;

    public static bool MinSeeDist(double dist, double min) => min == 0 || dist > min;

    public static bool MaxSeeDist(double dist, double max) => max == 0 || dist < max;

    public static bool SelfTargetClear(bool selfTarget) => selfTarget;

    public static bool LookExFovDefault(bool nullFov) => nullFov;

    public static bool ParamsStructFill(bool fill) => fill;

    public static bool LookExStealth(bool stealth) => stealth;

    public static bool LookExPatrolGoal(bool special) => special;

    public static bool LookExThresholdZero(bool zero) => zero;

    public static bool LookExTidHate(bool hate) => hate;

    public static bool LookExShootable(bool shoot) => shoot;

    public static bool LookExFriendBranch(bool friend) => friend;

    public static bool LookExWanderAllowed(bool wander) => wander;

    public static bool LookExAmbushDist(bool ambush) => ambush;

    public static bool LookExSeeYou(bool see) => see;

    public static bool LookExGoalTime(bool gate) => gate;

    public static bool LookExSeeSound(bool sound) => sound;

    public static bool LookExSetSeeState(bool set) => set;

    public static bool LookExInChaseSet(bool inchase) => inchase;

    public static bool LookExNoJumpSet(bool nojump) => nojump;

    public static bool LookExClearSelfTarget(bool clear) => clear;

    public static bool LookExDistanceCheck(bool ok) => ok;

    public static bool LookExCompatSound(bool compat) => compat;

    public static bool LookExNosector(bool nosector) => nosector;

    public static bool LookExLastHeard(bool heard) => heard;

    public static bool LookExNotargetCheat(bool cheat) => cheat;

    public static bool LookExStandstill(bool stand) => stand;

    public static bool LookExSeeStateNull(bool null) => null;

    public static bool LookExWanderCall(bool call) => call;

    public static bool LookExFullVolume(bool full) => full;

    public static bool LookExNormVolume(bool norm) => norm;

    public static bool LookExSkipSight(bool skip) => skip;

    public static bool LookExReturnZero(bool zero) => zero;

    public static bool LookExGoalMatch(bool match) => match;

    public static bool LookExReactionBlock(bool block) => block;

    public static bool LookExTargetGoal(bool goal) => goal;

    public static bool LookExSeeStateParam(bool param) => param;

    public static bool LookExDefaultSee(bool see) => see;

    public static bool LookExChaseFlag(bool chase) => chase;

    public static bool LookExPatrolArg(bool arg) => arg;

    public static bool LookExMaptimeReaction(bool react) => react;

    public static bool LookExFlagsChaseGoal(bool flag) => flag;

    public static bool LookExIteratorNext(bool next) => next;

    public static bool LookExSpecialClear(bool clear) => clear;

    public static bool LookExGoalAssign(bool assign) => assign;

    public static bool LookExReactionSet(bool set) => set;

    public static bool LookExChaseGoalOn(bool on) => on;

    public static bool LookExChaseGoalOff(bool off) => off;

    public static bool ALookPatrolSetup(bool ok) => ok;

    public static bool ALookThreshold(bool zero) => zero;

    public static bool ALookTidBranch(bool tid) => tid;

    public static bool ALookSoundTarget(bool targ) => targ;

    public static bool ALookDeadSkip(bool dead) => dead;

    public static bool ALookNotargetSkip(bool skip) => skip;

    public static bool ALookStealthDir(bool stealth) => stealth;

    public static bool ALookFriendWander(bool friend) => friend;

    public static bool ALookPlayerScan(bool scan) => scan;

    public static bool ALookAmbushSight(bool ambush) => ambush;

    public static bool ALookSeeYouJump(bool jump) => jump;

    public static bool ALookGoalWait(bool wait) => wait;

    public static bool ALookBossSound(bool boss) => boss;

    public static bool ALookSetSee(bool set) => set;

    public static bool ALookFailReturn(bool fail) => fail;

    public static bool ALookExParamFov(bool fov) => fov;

    public static bool ALookExParamDist(bool dist) => dist;

    public static bool ALookExParamFlags(bool flags) => flags;

    public static bool ALookExParamState(bool state) => state;

    public static bool ALookExHearLimit(bool limit) => limit;

    public static bool ALookExStandstillBlock(bool block) => block;

    public static bool ALookExInChaseBlock(bool block) => block;

    public static bool ALookExMinDistOk(bool ok) => ok;

    public static bool ALookExMaxDistOk(bool ok) => ok;

    public static bool ALookExSelfTargetNull(bool null) => null;

    public static bool ALookExSightRequired(bool req) => req;

    public static bool ALookExNoSoundPath(bool path) => path;

    public static bool ALookExCompatPath(bool path) => path;

    public static bool ALookExLastHeardPath(bool path) => path;

    public static bool ALookExPlayerCheat(bool cheat) => cheat;

    public static bool ALookExWanderFallback(bool wander) => wander;

    public static bool ALookExSeeStatePick(bool pick) => pick;

    public static bool ALookExVolumePick(bool pick) => pick;

    public static bool ALookExJumpAllowed(bool allow) => allow;

    public static bool ALookExStateParam(bool param) => param;

    public static bool ALookExDefaultState(bool def) => def;

    public static bool ALookExChaseGuard(bool guard) => guard;

    public static bool ALookExGoalReaction(bool react) => react;

    public static bool ALookExIterator(bool it) => it;

    public static bool ALookExSpecialZero(bool zero) => zero;

    public static bool ALookExGoalFromIter(bool goal) => goal;

    public static bool ALookExReactionFromArgs(bool react) => react;

    public static bool ALookExChaseGoalBit(bool bit) => bit;
}
