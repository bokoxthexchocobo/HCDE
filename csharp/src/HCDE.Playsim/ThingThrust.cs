namespace HCDE.Playsim;

/// <summary>Native ThrustThing horizontal impulse and component limits.</summary>
public static class ThingThrust
{
    public static bool? ExecuteSpecial(AuthoritySimulation sim, int special, Actor? activator,
        int angle, int force, int noLimit, int tid)
    {
        if (special != 72) return null;
        var radians = (angle % 256) * (Math.PI * 2 / 256);
        var dx = Math.Cos(radians) * force;
        var dy = Math.Sin(radians) * force;
        var targets = tid == 0
            ? activator is { Destroyed: false } ? new[] { activator } : Array.Empty<Actor>()
            : sim.Actors.Where(actor => !actor.Destroyed && actor.ThingId == tid).ToArray();
        foreach (var actor in targets)
        {
            var x = actor.VelocityX.ToDouble() + dx;
            var y = actor.VelocityY.ToDouble() + dy;
            if (noLimit == 0) { x = Math.Clamp(x, -30, 30); y = Math.Clamp(y, -30, 30); }
            actor.VelocityX = Fixed.FromDouble(x); actor.VelocityY = Fixed.FromDouble(y);
        }
        return tid != 0 || targets.Length != 0;
    }
}