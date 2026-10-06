namespace HCDE.Playsim;

/// <summary>Native actor.zs speed and pain-threshold setters for managed state tables.</summary>
public static class ActorTuningActions
{
    public static void SetSpeed(Actor actor, double speed, int pointerSelector = 0)
    {
        if (!double.IsFinite(speed)) throw new ArgumentOutOfRangeException(nameof(speed));
        var subject = Subject(actor, pointerSelector);
        if (subject is not null) { subject.MovementSpeed = Fixed.FromDouble(speed); subject.HasTuningOverride = true; }
    }

    public static void SetFloatSpeed(Actor actor, double speed, int pointerSelector = 0)
    {
        if (!double.IsFinite(speed)) throw new ArgumentOutOfRangeException(nameof(speed));
        var subject = Subject(actor, pointerSelector);
        if (subject is not null) { subject.FloatSpeed = speed; subject.HasTuningOverride = true; }
    }

    public static void SetPainThreshold(Actor actor, int threshold, int pointerSelector = 0)
    {
        var subject = Subject(actor, pointerSelector);
        if (subject is not null) { subject.PainThreshold = threshold; subject.HasTuningOverride = true; }
    }

    private static Actor? Subject(Actor actor, int pointerSelector)
    {
        var subject = pointerSelector == AcsActorPointer.Default ? actor
            : pointerSelector == AcsActorPointer.Null ? null
            : AcsActorPointer.Resolve(actor.Simulation
                ?? throw new InvalidOperationException("Actor tuning pointer selection requires a simulation."), actor, pointerSelector);
        return subject is { Destroyed: false } ? subject : null;
    }
}
