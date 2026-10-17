namespace HCDE.Playsim;

[Flags]
public enum ActorSeekerFlags
{
    Look = 1,
    Precise = 2,
    CurrentSpeed = 4,
}

/// <summary>Native P_SeekerMissile steering for managed state actions.</summary>
public static class ActorSeekerActions
{
    /// <summary>Native A_SeekerMissile over the represented rough-search actor model.</summary>
    public static void SeekerMissile(Actor actor, int threshold, int turnMax, ActorSeekerFlags flags = 0,
        int chance = 50, int distance = 10)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if ((flags & ~(ActorSeekerFlags)7) != 0) throw new ArgumentOutOfRangeException(nameof(flags));
        var sim = actor.Simulation ?? throw new InvalidOperationException("Seeker actions require an attached simulation.");
        var look = (flags & ActorSeekerFlags.Look) != 0;
        var target = AcsActorPointer.Resolve(sim, actor, AcsActorPointer.Target);
        var tracer = AcsActorPointer.Resolve(sim, actor, AcsActorPointer.Tracer);
        if (!look && target is PlayerPawn && tracer is not PlayerPawn) actor.TracerId = target.Id;
        if (look && tracer is null && sim.NextSeekerMissileByte() < chance)
            actor.TracerId = ActorEnemySearchActions.RoughMonsterSearch(actor, distance, onlySeekable: true)?.Id;
        if (!Seek(actor, Math.Max(threshold, 0), Math.Max(turnMax, 0),
                (flags & ActorSeekerFlags.CurrentSpeed) != 0, (flags & ActorSeekerFlags.Precise) != 0) && look)
            actor.TracerId = null;
    }

    public static bool Seek(Actor actor, double threshold, double turnMax, bool useCurrentSpeed = false, bool precise = false)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if (!double.IsFinite(threshold)) throw new ArgumentOutOfRangeException(nameof(threshold));
        if (!double.IsFinite(turnMax)) throw new ArgumentOutOfRangeException(nameof(turnMax));
        var sim = actor.Simulation ?? throw new InvalidOperationException("Seeker steering requires an attached simulation.");
        var speed = useCurrentSpeed ? actor.VelToSpeed() : actor.MovementSpeed.ToDouble();
        var target = AcsActorPointer.Resolve(sim, actor, AcsActorPointer.Tracer);
        if (target is null || !actor.CanSeek(target)) return false;
        if (!target.Shootable)
        {
            actor.TracerId = null;
            return false;
        }
        if (speed == 0) return true;
        var difference = Actor.Normalize180(actor.AngleTo(target) - actor.Angle.ToDegrees());
        var delta = Math.Abs(difference);
        if (delta > threshold) delta = Math.Min(delta / 2, turnMax);
        actor.Angle = BamAngle.FromDegrees(actor.Angle.ToDegrees() + (difference > 0 ? delta : -delta));
        if (precise)
        {
            var pitch = 0.0;
            if (!actor.FloorHugger && !actor.CeilingHugger)
            {
                var aimZ = target is PlayerPawn player ? player.GetViewZ() : target.Center();
                pitch = -Math.Atan2(aimZ - actor.Center(), Math.Max(1, actor.Distance2D(target))) * 180 / Math.PI;
            }
            actor.Vel3DFromAngle(pitch, speed);
            return true;
        }
        actor.VelFromAngle(speed);
        if (!actor.FloorHugger && !actor.CeilingHugger
            && (actor.Z.ToDouble() + actor.Height.ToDouble() < target.Z.ToDouble()
                || target.Z.ToDouble() + target.Height.ToDouble() < actor.Z.ToDouble()))
        {
            actor.VelocityZ = Fixed.FromDouble((target.Center() - actor.Center()) / actor.DistanceBySpeed(target, speed));
        }
        return true;
    }
}
