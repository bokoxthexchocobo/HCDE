namespace HCDE.Playsim;

/// <summary>Native raise actions for supported managed monster classes.</summary>
public static class ActorRaiseActions
{
    public static bool RaiseSelf(Actor actor, int flags = 0) => RaiseActor(actor, actor, flags);

    public static bool RaiseActor(Actor raiser, Actor? target, int flags = 0)
    {
        if ((flags & ~3) != 0) throw new ArgumentOutOfRangeException(nameof(flags));
        if (target is null || !ActorRaise.HasRaiseState(target)) return false;
        var sim = target.Simulation ?? throw new InvalidOperationException("Resurrection requires a simulation.");
        target.VelocityX = target.VelocityY = default;
        if ((flags & 2) == 0 && !ActorRaise.CheckPosition(sim, target)) return false;
        ActorRaise.ReviveSupported(target);
        if ((flags & 1) != 0)
        {
            // Copy the friendship fields represented by the managed actor model.
            ActorPropertyActions.CopySupportedFriendship(target, raiser);
        }
        return true;
    }

    public static void RaiseMaster(Actor actor, int flags = 0)
        => RaiseActor(actor, AcsActorPointer.Resolve(RequireSimulation(actor), actor, AcsActorPointer.Master), flags);

    public static void RaiseChildren(Actor actor, int flags = 0) => RaiseFamily(actor, flags, false);

    public static void RaiseSiblings(Actor actor, int flags = 0) => RaiseFamily(actor, flags, true);

    private static void RaiseFamily(Actor actor, int flags, bool siblings)
    {
        if ((flags & ~3) != 0) throw new ArgumentOutOfRangeException(nameof(flags));
        var sim = RequireSimulation(actor);
        var master = siblings ? AcsActorPointer.Resolve(sim, actor, AcsActorPointer.Master) : actor;
        if (master is null) return;
        foreach (var target in sim.Actors.Where(target => !target.Destroyed && target.MasterId == master.Id
            && (!siblings || target != actor)).ToArray())
            RaiseActor(actor, target, flags);
    }

    private static AuthoritySimulation RequireSimulation(Actor actor)
        => actor.Simulation ?? throw new InvalidOperationException("Family resurrection requires a simulation.");
}
