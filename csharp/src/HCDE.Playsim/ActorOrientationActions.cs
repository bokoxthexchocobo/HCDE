namespace HCDE.Playsim;

/// <summary>Native A_SetAngle, A_SetPitch and A_SetRoll for managed state tables.</summary>
public static class ActorOrientationActions
{
    public static void FaceTarget(Actor actor, double maxTurn = 0, double maxPitch = 270,
        double angleOffset = 0, double pitchOffset = 0, int flags = 0, double zOffset = 0)
        => Face(actor, FacingSubject(actor, AcsActorPointer.Target), maxTurn, maxPitch, angleOffset, pitchOffset, flags, zOffset);

    public static void FaceMaster(Actor actor, double maxTurn = 0, double maxPitch = 270,
        double angleOffset = 0, double pitchOffset = 0, int flags = 0, double zOffset = 0)
        => Face(actor, FacingSubject(actor, AcsActorPointer.Master), maxTurn, maxPitch, angleOffset, pitchOffset, flags, zOffset);

    public static void FaceTracer(Actor actor, double maxTurn = 0, double maxPitch = 270,
        double angleOffset = 0, double pitchOffset = 0, int flags = 0, double zOffset = 0)
        => Face(actor, FacingSubject(actor, AcsActorPointer.Tracer), maxTurn, maxPitch, angleOffset, pitchOffset, flags, zOffset);

    private static Actor? FacingSubject(Actor actor, int selector)
        => AcsActorPointer.Resolve(actor.Simulation
            ?? throw new InvalidOperationException("Facing pointer selection requires a simulation."), actor, selector);

    public static void Face(Actor actor, Actor? other, double maxTurn = 0, double maxPitch = 270,
        double angleOffset = 0, double pitchOffset = 0, int flags = 0, double zOffset = 0)
    {
        if (other is null || other.Destroyed) return;
        if (!double.IsFinite(maxTurn) || !double.IsFinite(maxPitch) || !double.IsFinite(angleOffset)
            || !double.IsFinite(pitchOffset) || !double.IsFinite(zOffset))
            throw new ArgumentOutOfRangeException(nameof(maxTurn), "Facing parameters must be finite.");
        if ((flags & ~15) != 0) throw new NotSupportedException("Unknown facing body-position flags.");
        actor.Ambush = false;
        var dx = other.X.ToDouble() - actor.X.ToDouble(); var dy = other.Y.ToDouble() - actor.Y.ToDouble();
        var desired = Math.Atan2(dy, dx) * 180 / Math.PI;
        var current = actor.Angle.ToDegrees(); var delta = AngleDelta(current, desired);
        actor.Angle = BamAngle.FromDegrees(maxTurn != 0 && maxTurn < Math.Abs(delta)
            ? current + (delta >= 0 ? 1 : -1) * (maxTurn + angleOffset) : desired + angleOffset);
        if (maxPitch > 180) return;
        var sourceZ = actor.Z.ToDouble() + 32;
        var targetZ = other.Z.ToDouble() + 32;
        if (sourceZ >= actor.Z.ToDouble() + actor.Height.ToDouble()) sourceZ = actor.Z.ToDouble() + actor.Height.ToDouble() / 2;
        if (targetZ >= other.Z.ToDouble() + other.Height.ToDouble()) targetZ = other.Z.ToDouble() + other.Height.ToDouble() / 2;
        // Native applies each flag in order, so TOP wins over MIDDLE and BOTTOM.
        if ((flags & 1) != 0) targetZ = other.Z.ToDouble();
        if ((flags & 2) != 0) targetZ = other.Z.ToDouble() + other.Height.ToDouble() / 2;
        if ((flags & 4) != 0) targetZ = other.Z.ToDouble() + other.Height.ToDouble();
        var desiredPitch = -Math.Atan2(targetZ + zOffset - sourceZ, Math.Sqrt(dx * dx + dy * dy)) * 180 / Math.PI;
        if (maxPitch == 0) actor.PitchDegrees = desiredPitch;
        else
        {
            var pitchDelta = Math.Abs(desiredPitch - actor.PitchDegrees) % 360;
            actor.PitchDegrees += (actor.PitchDegrees > desiredPitch ? -1 : 1) * Math.Min(maxPitch, pitchDelta);
        }
        actor.PitchDegrees += pitchOffset;
    }

    public static bool SetSpriteAngle(Actor actor, double degrees = 0, int pointerSelector = 0)
    {
        var subject = Subject(actor, degrees, 0, pointerSelector);
        if (subject is null) return false;
        subject.SpriteAngle = degrees; subject.HasSpriteOrientationOverride = true;
        return true;
    }

    public static bool SetSpriteRotation(Actor actor, double degrees = 0, int pointerSelector = 0)
    {
        var subject = Subject(actor, degrees, 0, pointerSelector);
        if (subject is null) return false;
        subject.SpriteRotation = degrees; subject.HasSpriteOrientationOverride = true;
        return true;
    }

    public static bool FaceMovementDirection(Actor actor, double offset = 0, double angleLimit = 0,
        double pitchLimit = 0, int flags = 0, int pointerSelector = 0)
    {
        if (!double.IsFinite(offset)) throw new ArgumentOutOfRangeException(nameof(offset));
        if (!double.IsFinite(angleLimit)) throw new ArgumentOutOfRangeException(nameof(angleLimit));
        if (!double.IsFinite(pitchLimit)) throw new ArgumentOutOfRangeException(nameof(pitchLimit));
        // Movement flags use bit 4 for suppressing yaw, unlike the setter flags.
        if ((flags & ~7) != 0) throw new NotSupportedException("Unknown movement orientation flags.");
        var subject = Subject(actor, offset, (flags & 5) == 5 ? 0 : flags & 2, pointerSelector);
        if (subject is null || (flags & 5) == 5) return false;
        var x = subject.VelocityX.ToDouble(); var y = subject.VelocityY.ToDouble();
        if ((flags & 4) == 0 && (x != 0 || y != 0))
        {
            var desired = Math.Atan2(y, x) * 180 / Math.PI;
            var current = subject.Angle.ToDegrees(); var delta = AngleDelta(current, desired);
            var yaw = angleLimit > 0 && Math.Abs(delta) > angleLimit
                ? current + Math.Sign(delta) * (angleLimit + offset) : desired + offset;
            subject.Angle = BamAngle.FromDegrees(yaw);
        }
        if ((flags & 1) == 0)
        {
            var desired = -Math.Atan2(subject.VelocityZ.ToDouble(), Math.Sqrt(x * x + y * y)) * 180 / Math.PI;
            var current = subject.PitchDegrees; var delta = AngleDelta(current, desired);
            // Native limited pitch subtracts the delta direction, unlike limited yaw.
            subject.PitchDegrees = pitchLimit > 0 && Math.Abs(delta) > pitchLimit
                ? current - Math.Sign(delta) * pitchLimit : desired;
        }
        return true;
    }

    private static double AngleDelta(double current, double desired)
    {
        var delta = (desired - current) % 360;
        if (delta >= 180) delta -= 360;
        if (delta < -180) delta += 360;
        return delta;
    }

    public static void SetAngle(Actor actor, double degrees = 0, int flags = 0, int pointerSelector = 0)
    {
        var subject = Subject(actor, degrees, flags, pointerSelector);
        if (subject is not null) subject.Angle = BamAngle.FromDegrees(degrees);
    }

    public static void SetPitch(Actor actor, double degrees, int flags = 0, int pointerSelector = 0)
    {
        var subject = Subject(actor, degrees, flags, pointerSelector);
        if (subject is not null) subject.PitchDegrees = (flags & 1) != 0 ? Math.Clamp(degrees, -89, 89) : degrees;
    }

    public static void SetRoll(Actor actor, double degrees, int flags = 0, int pointerSelector = 0)
    {
        var subject = Subject(actor, degrees, flags, pointerSelector);
        if (subject is not null) subject.Roll = BamAngle.FromDegrees(degrees);
    }

    private static Actor? Subject(Actor actor, double degrees, int flags, int pointerSelector)
    {
        if (!double.IsFinite(degrees)) throw new ArgumentOutOfRangeException(nameof(degrees));
        if ((flags & ~7) != 0) throw new NotSupportedException("Unknown orientation action flags.");
        var subject = pointerSelector == AcsActorPointer.Default ? actor
            : pointerSelector == AcsActorPointer.Null ? null
            : AcsActorPointer.Resolve(actor.Simulation
                ?? throw new InvalidOperationException("Orientation pointer selection requires a simulation."), actor, pointerSelector);
        if (subject is null || subject.Destroyed) return null;
        if (subject is PlayerPawn && (flags & 6) != 0)
            throw new NotSupportedException("Player orientation interpolation requires renderer support.");
        return subject;
    }
}
