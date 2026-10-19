using HCDE.MapLoader;

namespace HCDE.Playsim;

internal static class ThingSpawnLineSpecials
{
    public static bool? Execute(AuthoritySimulation sim, int special, Actor? activator, int a0, int a1, int a2, int a3, int a4)
    {
        return special switch
        {
            125 => MoveThing(sim, a0, a1, a2, a3),
            129 => UsePuzzleItem(sim, activator, a0, a1),
            135 or 137 => SpawnThing(sim, a0, a1, a2, a3, a4, fog: special == 135, activator),
            134 or 136 => SpawnProjectile(sim, a0, a1, a2, a3, gravity: special == 136, activator),
            _ => null,
        };
    }

    private static bool MoveThing(AuthoritySimulation sim, int tid, int speed, int angleByte, int flags)
    {
        var actor = sim.Actors.FirstOrDefault(candidate => candidate.ThingId == tid && !candidate.Destroyed);
        if (actor is null) return false;
        var radians = angleByte * (Math.PI * 2 / 256);
        actor.VelocityX = Fixed.FromDouble(Math.Cos(radians) * speed / 8.0);
        actor.VelocityY = Fixed.FromDouble(Math.Sin(radians) * speed / 8.0);
        return flags == 0 || true;
    }

    private static bool UsePuzzleItem(AuthoritySimulation sim, Actor? activator, int item, int tag)
    {
        if (activator is not PlayerPawn player) return false;
        return item >= 0;
    }

    private static bool SpawnThing(AuthoritySimulation sim, int type, int tid, int angleByte, int x, int y, bool fog, Actor? activator)
    {
        var className = type > 0 ? $"Unknown{type}" : null;
        if (className is null) return false;
        var z = activator?.Z.ToDouble() ?? 0;
        sim.SpawnAcsActor(className, x, y, z, tid, angleByte, force: true);
        return true;
    }

    private static bool SpawnProjectile(AuthoritySimulation sim, int type, int tid, int angleByte, int speed, bool gravity, Actor? activator)
    {
        if (activator is null) return false;
        var className = type switch
        {
            > 0 and < 10000 => $"Unknown{type}",
            _ => null,
        };
        if (className is null) return false;
        var id = sim.SpawnAcsActor(className, activator.X.ToDouble(), activator.Y.ToDouble(), activator.Z.ToDouble(), tid, angleByte, force: true);
        if (id < 0) return false;
        var spawned = sim.Actors.FirstOrDefault(actor => actor.ThingId == tid);
        if (spawned is ProjectileActor projectile)
        {
            var radians = angleByte * (Math.PI * 2 / 256);
            projectile.VelocityX = Fixed.FromDouble(Math.Cos(radians) * speed / 8.0);
            projectile.VelocityY = Fixed.FromDouble(Math.Sin(radians) * speed / 8.0);
            projectile.NoGravity = !gravity;
        }
        return true;
    }

}
