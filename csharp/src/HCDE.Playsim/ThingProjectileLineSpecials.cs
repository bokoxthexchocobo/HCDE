using HCDE.Gamedata;

namespace HCDE.Playsim;

internal static class ThingProjectileLineSpecials
{
    internal const int ProjectileIntercept = 175;
    internal const int ProjectileAimed = 178;

    internal static bool? Execute(AuthoritySimulation sim, int special, Actor? activator, int spotTid, int type,
        int speed, int destTid, int newTid)
    {
        return special switch
        {
            ProjectileIntercept => Launch(sim, spotTid, type, speed, destTid, newTid, leadTarget: true, activator),
            ProjectileAimed => Launch(sim, spotTid, type, speed, destTid, newTid, leadTarget: false, activator),
            _ => null,
        };
    }

    private static bool Launch(AuthoritySimulation sim, int spotTid, int type, int speed, int destTid, int newTid,
        bool leadTarget, Actor? activator)
    {
        if (!ActorCombatActions.TryClassNameForProjectileType(type, out var className)
            && !DoomActorCatalog.TrySpawnClassName(type, out className)
            || !ActorCombatActions.TryResolveProjectileKind(className, out var kind)) return false;
        var spots = spotTid == 0
            ? activator is { Destroyed: false } ? new[] { activator } : Array.Empty<Actor>()
            : AcsActorTid.AllFromTid(sim, spotTid).ToArray();
        if (spots.Length == 0) return false;
        var launched = false;
        foreach (var spot in spots)
        {
            var targets = destTid == 0
                ? new Actor?[] { null }
                : AcsActorTid.AllFromTid(sim, destTid).Cast<Actor?>().ToArray();
            foreach (var target in targets)
            {
                var missile = sim.SpawnProjectile(spot, kind, target);
                if (newTid != 0) missile.ThingId = newTid;
                missile.SetTargetPointer(spot);
                AimMissile(missile, target, speed / 8.0, leadTarget);
                launched = true;
            }
        }
        return launched;
    }

    private static void AimMissile(ProjectileActor missile, Actor? target, double speed, bool leadTarget)
    {
        if (target is null || target.Destroyed)
        {
            missile.VelFromAngle(speed);
            return;
        }
        var tx = target.X.ToDouble();
        var ty = target.Y.ToDouble();
        var tz = target.Z.ToDouble() + target.Height.ToDouble() / 2;
        if (leadTarget)
        {
            var dx = tx - missile.X.ToDouble();
            var dy = ty - missile.Y.ToDouble();
            var dist = Math.Max(1, Math.Sqrt(dx * dx + dy * dy));
            var time = dist / Math.Max(1, speed);
            tx += target.VelocityX.ToDouble() * time;
            ty += target.VelocityY.ToDouble() * time;
            tz += target.VelocityZ.ToDouble() * time;
        }
        var mx = missile.X.ToDouble();
        var my = missile.Y.ToDouble();
        var mz = missile.Z.ToDouble();
        var yaw = Math.Atan2(ty - my, tx - mx) * 180 / Math.PI;
        var horiz = Math.Max(1e-6, Math.Sqrt((tx - mx) * (tx - mx) + (ty - my) * (ty - my)));
        var pitch = -Math.Atan2(tz - mz, horiz) * 180 / Math.PI;
        missile.Angle = BamAngle.FromDegrees(yaw);
        missile.PitchDegrees = pitch;
        var horizontalSpeed = Math.Cos(pitch * Math.PI / 180) * speed;
        missile.VelocityZ = Fixed.FromDouble(-Math.Sin(pitch * Math.PI / 180) * speed);
        missile.VelFromAngle(horizontalSpeed);
        if (missile.SeekerMissile) missile.RestoreTracerTarget(target.Id);
    }
}
