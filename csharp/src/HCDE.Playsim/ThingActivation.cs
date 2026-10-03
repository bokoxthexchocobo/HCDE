namespace HCDE.Playsim;

/// <summary>Native Thing_Activate/Deactivate base monster state handling.</summary>
public static class ThingActivation
{
    public static bool Execute(AuthoritySimulation sim, Actor? activator, int tid, bool activate)
    {
        var targets = tid == 0
            ? activator is { Destroyed: false } ? new[] { activator } : Array.Empty<Actor>()
            : sim.Actors.Where(actor => !actor.Destroyed && actor.ThingId == tid).ToArray();
        foreach (var actor in targets)
        {
            if (actor.Destroyed || !actor.IsMonster || actor.Health <= 0 && !actor.IceCorpse || actor.Dormant == !activate) continue;
            actor.Dormant = !activate;
            var state = activate ? actor.ActiveState : actor.InactiveState;
            if (state >= 0 && actor.States.HasState(state)) actor.States.Enter(actor, state);
            else actor.States.ForceRemainingTics(activate ? 1 : -1);
        }
        return targets.Length != 0;
    }
}
