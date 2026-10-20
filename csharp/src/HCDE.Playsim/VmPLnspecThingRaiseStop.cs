namespace HCDE.Playsim;

/// <summary>Native <c>LS_Thing_Raise</c>, <c>LS_Thing_Stop</c>, <c>LS_Thing_SetGoal</c> from <c>p_lnspec.cpp</c> (1759–1847).</summary>
public static class VmPLnspecThingRaiseStop
{
    public const int MfShootable = 1 << 0;
    public const int Mf5ChaseGoal = 1 << 0;

    public static bool ShouldRaiseActivator(int tid) => tid == 0;

    public static bool ShouldIterateByTid(int tid) => tid != 0;

    public static bool ShouldZeroVelocity(bool hasActor) => hasActor;

    public static bool ShouldZeroPlayerVelocity(bool hasPlayer) => hasPlayer;

    public static bool ShouldAssignPatrolGoal(bool shootable) => shootable;

    public static bool ShouldClearTargetWhenGoalEqualsTarget(bool targetingGoal) => targetingGoal;

    public static int ApplyChaseGoalFlag(int flags5, int chaseGoalArg) =>
        chaseGoalArg == 0 ? flags5 & ~Mf5ChaseGoal : flags5 | Mf5ChaseGoal;

    public static int ReactionTimeTicks(int delaySeconds) => delaySeconds * VmPLnspecArgMacros.TicRate;

    public static bool ShouldSetReactionTime(bool hasTarget) => !hasTarget;
}
