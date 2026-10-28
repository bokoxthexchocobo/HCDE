namespace HCDE.Playsim;

/// <summary>Native <c>SetActorAngle</c>, <c>SetActorPitch</c>, and <c>SetActorRoll</c> from <c>p_acs.cpp</c>.</summary>
public static class AcsActorOrientationService
{
    public static void SetAngle(AuthoritySimulation sim, int tid, Actor? activator, uint angleRaw, bool interpolate = true)
    {
        var angle = AcsAngles.ToBamAngle(angleRaw);
        Apply(sim, tid, activator, actor =>
        {
            actor.Angle = angle;
            _ = interpolate;
        });
    }

    public static void SetPitch(AuthoritySimulation sim, int tid, Actor? activator, uint pitchRaw, bool interpolate = true)
    {
        var pitch = AcsAngles.PitchDegreesFromAcs(pitchRaw);
        Apply(sim, tid, activator, actor =>
        {
            actor.PitchDegrees = pitch;
            _ = interpolate;
        });
    }

    public static void SetRoll(AuthoritySimulation sim, int tid, Actor? activator, uint rollRaw, bool interpolate = true)
    {
        var roll = AcsAngles.ToBamAngle(rollRaw);
        Apply(sim, tid, activator, actor =>
        {
            actor.Roll = roll;
            _ = interpolate;
        });
    }

    private static void Apply(AuthoritySimulation sim, int tid, Actor? activator, Action<Actor> mutate)
    {
        if (tid == 0)
        {
            if (activator is { Destroyed: false })
                mutate(activator);
            return;
        }

        foreach (var actor in AcsActorTid.AllFromTid(sim, tid))
            mutate(actor);
    }
}

internal static class AcsAngles
{
    internal static BamAngle ToBamAngle(uint angleRaw) => new(unchecked(angleRaw << 16));

    internal static double PitchDegreesFromAcs(uint angleRaw) =>
        unchecked((short)angleRaw) * (360.0 / 65536);
}
