namespace HCDE.Playsim;

/// <summary>Native <c>P_Thing_CanRaise</c> subset aligned with archvile resurrection gates.</summary>
internal static class ActorRaise
{
    public static bool CanRaise(AuthoritySimulation sim, Actor actor) =>
        !actor.Destroyed
        && actor.IsDead
        && actor.RaiseDuration > 0
        && actor.ResurrectionHealth > 0
        && actor.States.Current == ActorStateMachine.Corpse
        && actor.Brain != null
        && ActorPhysics.CanOccupy(sim, actor);

    public static bool CanRaiseAll(AuthoritySimulation sim, int tid, Actor? activator)
    {
        if (tid == 0)
        {
            var actor = activator is { Destroyed: false } ? activator : null;
            return actor != null && CanRaise(sim, actor);
        }

        foreach (var actor in AcsActorTid.AllFromTid(sim, tid))
        {
            if (!CanRaise(sim, actor))
                return false;
        }

        return true;
    }
}
