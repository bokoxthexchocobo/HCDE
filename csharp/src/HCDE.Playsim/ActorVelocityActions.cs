namespace HCDE.Playsim;

/// <summary>Native actions.zs velocity actions for managed state tables.</summary>
public static class ActorVelocityActions
{
    public static void Stop(Actor actor) => Assign(actor, 0, 0, 0);

    public static void ScaleVelocity(Actor actor, double scale, int pointerSelector = 0)
    {
        if (!double.IsFinite(scale)) throw new ArgumentOutOfRangeException(nameof(scale));
        var subject = Subject(actor, pointerSelector);
        if (subject is null) return;
        Assign(subject, subject.VelocityX.ToDouble() * scale, subject.VelocityY.ToDouble() * scale,
            subject.VelocityZ.ToDouble() * scale);
    }

    public static void ChangeVelocity(Actor actor, double x = 0, double y = 0, double z = 0,
        int flags = 0, int pointerSelector = 0)
    {
        if (!double.IsFinite(x) || !double.IsFinite(y) || !double.IsFinite(z))
            throw new ArgumentOutOfRangeException(nameof(x), "Velocity components must be finite.");
        if ((flags & ~3) != 0) throw new NotSupportedException("Unknown velocity action flags.");
        var subject = Subject(actor, pointerSelector);
        if (subject is null) return;
        if ((flags & 1) != 0)
        {
            var radians = subject.Angle.Raw * (Math.PI / 0x80000000u);
            var cosine = Math.Cos(radians); var sine = Math.Sin(radians);
            (x, y) = (x * cosine - y * sine, x * sine + y * cosine);
        }
        if ((flags & 2) == 0)
        {
            x += subject.VelocityX.ToDouble(); y += subject.VelocityY.ToDouble(); z += subject.VelocityZ.ToDouble();
        }
        Assign(subject, x, y, z);
    }

    private static void Assign(Actor actor, double x, double y, double z)
    {
        var vx = Fixed.FromDouble(x); var vy = Fixed.FromDouble(y); var vz = Fixed.FromDouble(z);
        actor.VelocityX = vx; actor.VelocityY = vy; actor.VelocityZ = vz;
    }

    private static Actor? Subject(Actor actor, int pointerSelector)
    {
        var subject = pointerSelector == AcsActorPointer.Default ? actor
            : pointerSelector == AcsActorPointer.Null ? null
            : AcsActorPointer.Resolve(actor.Simulation
                ?? throw new InvalidOperationException("Velocity pointer selection requires a simulation."), actor, pointerSelector);
        return subject is { Destroyed: false } ? subject : null;
    }
}
