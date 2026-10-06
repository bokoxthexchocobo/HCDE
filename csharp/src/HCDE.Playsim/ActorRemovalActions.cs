namespace HCDE.Playsim;

public static class ActorRemovalActions
{
    public static void Remove(Actor actor, int pointerSelector = 0, int flags = 0,
        string? classFilter = null, string? speciesFilter = null)
    {
        if ((flags & ~127) != 0) throw new ArgumentOutOfRangeException(nameof(flags));
        var target = pointerSelector == AcsActorPointer.Default ? actor
            : pointerSelector == AcsActorPointer.Null ? null
            : AcsActorPointer.Resolve(actor.Simulation ?? throw new InvalidOperationException("Removal pointer selection requires a simulation."), actor, pointerSelector);
        RemoveResolved(target, flags, classFilter, speciesFilter);
    }

    public static void RemoveTarget(Actor actor, int flags = 0, string? classFilter = null, string? speciesFilter = null)
        => Remove(actor, AcsActorPointer.Target, flags, classFilter, speciesFilter);

    public static void RemoveTracer(Actor actor, int flags = 0, string? classFilter = null, string? speciesFilter = null)
        => Remove(actor, AcsActorPointer.Tracer, flags, classFilter, speciesFilter);

    public static void RemoveMaster(Actor actor, int flags = 0, string? classFilter = null, string? speciesFilter = null)
        => Remove(actor, AcsActorPointer.Master, flags, classFilter, speciesFilter);

    public static void RemoveChildren(Actor actor, bool removeAll = false, int flags = 0, string? classFilter = null, string? speciesFilter = null)
        => RemoveFamily(actor, removeAll, flags, classFilter, speciesFilter, false);

    public static void RemoveSiblings(Actor actor, bool removeAll = false, int flags = 0, string? classFilter = null, string? speciesFilter = null)
        => RemoveFamily(actor, removeAll, flags, classFilter, speciesFilter, true);

    private static void RemoveFamily(Actor actor, bool removeAll, int flags, string? classFilter, string? speciesFilter, bool siblings)
    {
        if ((flags & ~127) != 0) throw new ArgumentOutOfRangeException(nameof(flags));
        var sim = actor.Simulation ?? throw new InvalidOperationException("Family removal requires a simulation.");
        var master = siblings ? AcsActorPointer.Resolve(sim, actor, AcsActorPointer.Master) : actor;
        if (master is null) return;
        foreach (var target in sim.Actors.Where(target => !target.Destroyed && target.MasterId == master.Id
            && (!siblings || target != actor) && (target.Health <= 0 || removeAll)).ToArray())
            RemoveResolved(target, flags, classFilter, speciesFilter);
    }

    private static void RemoveResolved(Actor? target, int flags, string? classFilter, string? speciesFilter)
    {
        if (target is null || target.Destroyed || target is PlayerPawn) return;
        if (!ActorActionFilters.Matches(target, classFilter, speciesFilter, (flags & 16) != 0, (flags & 32) != 0, (flags & 64) != 0)) return;
        var missile = target is ProjectileActor;
        if ((flags & 8) != 0 || (flags & 4) != 0 && !(target.IsMonster && missile)
            || target.IsMonster && (flags & 2) == 0 || missile && (flags & 1) != 0)
            target.Destroy();
    }
}
