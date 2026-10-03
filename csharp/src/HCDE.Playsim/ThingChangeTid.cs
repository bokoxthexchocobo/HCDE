namespace HCDE.Playsim;

/// <summary>Native Thing_ChangeTID selection and success semantics.</summary>
public static class ThingChangeTid
{
    public static bool? ExecuteSpecial(AuthoritySimulation sim, int special, Actor? activator, int oldTid, int newTid)
    {
        if (special != 176) return null;
        var targets = oldTid == 0
            ? activator is { Destroyed: false } ? new[] { activator } : Array.Empty<Actor>()
            : sim.Actors.Where(actor => !actor.Destroyed && actor.ThingId == oldTid).ToArray();
        foreach (var actor in targets) actor.ThingId = newTid;
        return true;
    }
}