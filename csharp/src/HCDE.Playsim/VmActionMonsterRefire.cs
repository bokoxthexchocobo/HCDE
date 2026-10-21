namespace HCDE.Playsim;

/// <summary>Refire and angle/pitch set guards from <c>p_actionfunctions.cpp</c> (2946–3004).</summary>
public static class VmActionMonsterRefire
{
    public static bool RefireRollBelowProbability(int roll, int prob) => roll < prob;

    public static bool RefireShouldJump(
        object? target,
        bool hitFriend,
        int targetHealth,
        bool inSight) =>
        target is null || hitFriend || targetHealth <= 0 || !inSight;

    public static bool ApplyAngleWhenRef(object? refActor, bool hasRef) =>
        hasRef && refActor is not null;

    public static bool ApplyPitchWhenRef(object? refActor) => refActor is not null;
}
