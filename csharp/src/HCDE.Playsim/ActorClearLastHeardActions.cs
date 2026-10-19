namespace HCDE.Playsim;

/// <summary>Native <c>p_enemy.cpp</c> <c>A_ClearLastHeard</c>.</summary>
public static class ActorClearLastHeardActions
{
    public static void ClearLastHeard(Actor actor) => actor.LastHeardTargetId = null;
}
