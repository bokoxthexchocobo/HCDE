namespace HCDE.Playsim;

/// <summary>Native Thing_Activate/Deactivate base monster state handling.</summary>
public static class ThingActivation
{
    internal static bool? ExecuteSpecial(AuthoritySimulation sim, int special, Actor? activator, int tid) =>
        special is 130 or 131 ? Execute(sim, activator, tid, special == 130) : null;

    public static bool Execute(AuthoritySimulation sim, Actor? activator, int tid, bool activate)
    {
        var targets = tid == 0
            ? activator is { Destroyed: false } ? new[] { activator } : Array.Empty<Actor>()
            : sim.Actors.Where(actor => !actor.Destroyed && actor.ThingId == tid).ToArray();
        foreach (var actor in targets) Apply(actor, activate);
        return targets.Length != 0;
    }

    internal static void InitializeSpawn(Actor actor, bool mapDormant)
    {
        if (actor.Dormant)
        {
            actor.Dormant = false;
            Apply(actor, false);
        }
        if (mapDormant) Apply(actor, false);
    }

    private static void Apply(Actor actor, bool activate)
    {
        if (actor.Destroyed || !actor.IsMonster || actor.Health <= 0 && !actor.IceCorpse || actor.Dormant == !activate) return;
        actor.Dormant = !activate;
        var state = activate ? actor.ActiveState : actor.InactiveState;
        if (state >= 0 && actor.States.HasState(state)) actor.States.Enter(actor, state);
        else actor.States.ForceRemainingTics(activate ? 1 : -1);
    }
}