namespace HCDE.Playsim;

internal static class ThingSetSpecial
{
    internal static bool? Execute(AuthoritySimulation sim, int special, Actor? activator,
        int tid, int actorSpecial, int arg0, int arg1, int arg2)
    {
        if (special != 127) return null;
        Set(sim, activator, tid, actorSpecial, [arg0, arg1, arg2]);
        return true;
    }

    internal static void Set(AuthoritySimulation sim, Actor? activator, int tid, int actorSpecial, ReadOnlySpan<int> args)
    {
        var targets = tid == 0
            ? activator is { Destroyed: false } ? new[] { activator } : Array.Empty<Actor>()
            : AcsActorTid.AllFromTid(sim, tid).ToArray();
        foreach (var actor in targets)
        {
            actor.Special = actorSpecial;
            args.CopyTo(actor.SpecialArgs);
        }
    }
}
