namespace HCDE.Playsim;

/// <summary>Native ThrustThingZ argument and targeting semantics.</summary>
public static class ThingThrustZ
{
    public static bool? ExecuteSpecial(AuthoritySimulation sim, int special, Actor? activator,
        int tid, int thrust, int down, int add)
    {
        if (special != 128) return null;
        var velocity = Fixed.FromDouble((down != 0 ? -1.0 : 1.0) * thrust / 4.0);
        var targets = tid == 0
            ? activator is { Destroyed: false } ? new[] { activator } : Array.Empty<Actor>()
            : sim.Actors.Where(actor => !actor.Destroyed && actor.ThingId == tid).ToArray();
        foreach (var actor in targets)
            actor.VelocityZ = add == 0 ? velocity : Fixed.FromDouble(actor.VelocityZ.ToDouble() + velocity.ToDouble());
        return tid != 0 || targets.Length != 0;
    }
}