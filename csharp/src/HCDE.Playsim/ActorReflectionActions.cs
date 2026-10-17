namespace HCDE.Playsim;

public delegate bool ReflectionAngleHandler(Actor missile, Actor reflector, ref double angleDegrees);

public static class ActorReflectionActions
{
    public static bool ReflectOffActor(Actor missile, Actor blocking)
    {
        ArgumentNullException.ThrowIfNull(missile);
        ArgumentNullException.ThrowIfNull(blocking);
        if (!blocking.Reflective) return false;
        if (!blocking.ThruReflect)
        {
            var angle = blocking.AngleTo(missile);
            if (missile.AdjustReflectionAngle(blocking, ref angle) || missile.Destroyed || blocking.Destroyed) return false;
            var sim = missile.Simulation ?? throw new InvalidOperationException("Reflection requires an attached simulation.");
            var target = blocking.AimReflect
                ? AcsActorPointer.Resolve(sim, missile, AcsActorPointer.Target)
                    ?? AcsActorPointer.Resolve(sim, blocking, AcsActorPointer.Target) : null;
            if (target is not null)
            {
                var dx = target.X.ToDouble() - missile.X.ToDouble();
                var dy = target.Y.ToDouble() - missile.Y.ToDouble();
                var dz = target.Z.ToDouble() + target.Height.ToDouble() * 0.5 - missile.Z.ToDouble();
                var length = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                var scale = length > 0 ? missile.MovementSpeed.ToDouble() / length : 0;
                missile.VelocityX = Fixed.FromDouble(dx * scale);
                missile.VelocityY = Fixed.FromDouble(dy * scale);
                missile.VelocityZ = Fixed.FromDouble(dz * scale);
            }
            else if (blocking.MirrorReflect)
            {
                missile.Angle = BamAngle.FromDegrees(missile.Angle.ToDegrees() + 180);
                missile.VelocityX = Fixed.FromDouble(missile.VelocityX.ToDouble() * -0.5);
                missile.VelocityY = Fixed.FromDouble(missile.VelocityY.ToDouble() * -0.5);
                missile.VelocityZ = Fixed.FromDouble(missile.VelocityZ.ToDouble() * -0.5);
            }
            else
            {
                missile.Angle = BamAngle.FromDegrees(angle);
                missile.VelFromAngle(missile.MovementSpeed.ToDouble() * 0.5);
                missile.VelocityZ = Fixed.FromDouble(missile.VelocityZ.ToDouble() * -0.5);
            }
        }
        if (missile.SeekerMissile) missile.TracerId = missile.TargetId;
        missile.SetTargetPointer(blocking);
        return true;
    }

    public static bool AdjustReflectionAngle(Actor missile, Actor blocking, ref double angleDegrees)
    {
        ArgumentNullException.ThrowIfNull(missile);
        ArgumentNullException.ThrowIfNull(blocking);
        if (missile.DontReflect) return true;
        if (blocking.ThruReflect) return false;
        if (blocking.ShieldReflect && (missile.NoShieldReflect
            || Math.Abs(Actor.Normalize180(angleDegrees - blocking.Angle.ToDegrees())) > 45)) return true;
        var sim = missile.Simulation ?? throw new InvalidOperationException("Reflection requires an attached simulation.");
        var random = sim.NextReflectByte();
        angleDegrees += blocking.ShieldReflect || blocking.Deflect ? random < 128 ? 45 : -45 : random % 16 - 8;
        if (blocking.AimReflect)
        {
            var target = AcsActorPointer.Resolve(sim, missile, AcsActorPointer.Target)
                ?? AcsActorPointer.Resolve(sim, blocking, AcsActorPointer.Target);
            if (target is not null) ActorOrientationActions.Face(missile, target);
        }
        return false;
    }
}
