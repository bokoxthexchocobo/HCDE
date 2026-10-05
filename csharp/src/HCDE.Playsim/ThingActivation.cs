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
        foreach (var actor in targets)
        {
            var flag = activate ? 256 : 512;
            if ((actor.ActivationType & flag) != 0)
            {
                actor.ActivationType &= ~flag;
                if ((actor.ActivationType & 1024) != 0) actor.ActivationType |= activate ? 512 : 256;
            }
            if (activate) actor.Activate(activator); else actor.Deactivate(activator);
        }
        return targets.Length != 0;
    }

    internal static void InitializeSpawn(Actor actor, bool mapDormant, bool mapSpawn = true)
    {
        actor.SpawnDormant = mapDormant;
        actor.ResurrectionRadius ??= actor.Radius;
        actor.ResurrectionHeight ??= actor.Height;
        actor.ResurrectionCollisionFlags ??= ActorSpawner.CollisionFlagsOf(actor);
        actor.ResurrectionDefenseFlags ??= ActorSpawner.DefenseFlagsOf(actor);
        actor.ResurrectionMovementFlags ??= ActorSpawner.MovementFlagsOf(actor);
        actor.BeginPlay();
        if (actor.Destroyed) return;
        if (mapSpawn) actor.LevelSpawned();
    }

    internal static void Apply(Actor actor, bool activate)
    {
        if (actor.Destroyed || !actor.IsMonster || actor.Health <= 0 && !actor.IceCorpse || actor.Dormant == !activate) return;
        actor.Dormant = !activate;
        var state = activate ? actor.ActiveState : actor.InactiveState;
        if (state >= 0 && actor.States.HasState(state)) actor.States.Enter(actor, state);
        else actor.States.ForceRemainingTics(activate ? 1 : -1);
    }
}
