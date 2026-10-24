namespace HCDE.Playsim;

/// <summary>Native <c>p_enemy.cpp</c> <c>A_Look2</c> (Strife-style last-heard wake).</summary>
public static class ActorLook2Actions
{
    public static void Look2(Actor actor)
    {
        if (actor.Destroyed || actor.InConversation) return;
        var sim = actor.Simulation ?? throw new InvalidOperationException("Look2 requires a simulation.");
        var brain = actor.Brain ?? throw new NotSupportedException("Look2 requires a represented monster brain.");
        brain.SetChaseThreshold(0, setDefault: false);
        var heard = actor.LastHeardTargetId is { } heardId
            ? sim.Actors.FirstOrDefault(candidate => candidate.Id == heardId && !candidate.Destroyed) : null;
        if (heard is { IsDead: true }) heard = null;
        if (heard is { Shootable: true })
        {
            var hostile = actor.Friendly != heard.Friendly;
            if (hostile)
            {
                if (actor.Ambush && !CombatTrace.CheckSight(sim, actor, heard, ActorSightFlags.SeePastBlockEverything))
                    goto idleFidget;
                actor.SetTargetPointer(heard);
                brain.SetChaseThreshold(10, setDefault: false);
                if (actor.States.HasState(actor.SeeState)) actor.States.Enter(actor, actor.SeeState);
                return;
            }
            if (brain.FindForLook(sim, actor, new ActorLookParameters(), actor.LookAllAround, chaseGoal: false) is not null)
            {
                actor.InCombat = true;
                if (actor.States.HasState(actor.SeeState)) actor.States.Enter(actor, actor.SeeState);
                return;
            }
        }
    idleFidget:
        if (sim.NextLook2Byte() < 30 && actor.States.HasState(actor.SpawnState + 1))
            actor.States.Enter(actor, actor.SpawnState + (sim.NextLook2Byte() & 1) + 1);
        if (!actor.StandStill && sim.NextLook2Byte() < 40 && actor.States.HasState(actor.SpawnState + 3))
            actor.States.Enter(actor, actor.SpawnState + 3);
    }
}
