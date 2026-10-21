namespace HCDE.Playsim;

/// <summary><c>P_LookForPlayers</c>, <c>A_Look</c>, and <c>A_Wander</c> guards from <c>p_enemy.cpp</c>.</summary>
public static class VmPEnemyLookPlayers
{
    public const int Mf3Huntplayers = 1;
    public const int Mf4Lookallaround = 1;
    public const int MfStealth = 1;
    public const int MfShootable = 1;
    public const int MfAmbush = 1;
    public const int Mf2Boss = 1;
    public const int Mf8Fullvolsee = 1;
    public const int Mf5Inconversation = 1;

    public static bool LastEnemyRetarget(bool hasLast, int health, bool isFriend) =>
        hasLast && health > 0 && !isFriend;

    public static bool TidHateUsesLookForTid(int tidHate) => tidHate != 0;

    public static bool HuntPlayersRequired(int flags3) => (flags3 & Mf3Huntplayers) != 0;

    public static bool SinglePlayerDeadMonsters(bool doomFamily, bool primary, bool mp, int p0Health, bool goalNull, bool title) =>
        doomFamily && primary && !mp && p0Health <= 0 && goalNull && !title;

    public static bool GoalReactionGate(bool targetIsGoal, int maptime, int reactiontime) =>
        targetIsGoal && reactiontime > maptime;

    public static bool AmbushNeedsSight(bool ambush, bool sight) => ambush && !sight;

    public static bool LookAllaround(int flags4) => (flags4 & Mf4Lookallaround) != 0;

    public static bool BossSeeSound(bool boss, bool fullVol) => boss || fullVol;

    public static bool MaxHearDistBlocks(double dist, double max) => max != 0 && dist > max;

    public static bool MinSeeDist(double dist, double min) => min == 0 || dist > min;

    public static bool MaxSeeDist(double dist, double max) => max == 0 || dist < max;
}
