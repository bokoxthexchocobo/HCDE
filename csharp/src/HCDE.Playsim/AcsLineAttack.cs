namespace HCDE.Playsim;

/// <summary>Native ACS <c>LineAttack</c> subset via <see cref="CombatTrace.TraceLineAttack"/>.</summary>
internal static class AcsLineAttack
{
    public static int Attack(
        AuthoritySimulation sim,
        Actor? source,
        int angleOffsetRaw,
        int pitchOffsetRaw,
        int damage,
        string? damageType,
        double range,
        int puffTid = 0,
        bool absoluteAngles = false)
    {
        if (source is not { Destroyed: false })
            return 0;
        if (!double.IsFinite(range))
            return 0;

        var angle = new BamAngle(unchecked((absoluteAngles ? 0u : source.Angle.Raw) + (uint)angleOffsetRaw));
        var pitch = new BamAngle(unchecked((absoluteAngles ? 0u : BamAngle.FromDegrees(source.PitchDegrees).Raw) + (uint)pitchOffsetRaw));
        var trace = CombatTrace.TraceLineAttack(sim, source, angle, pitch, range);
        if (!trace.Hit)
            return 0;

        GeometryLineAttack.Apply(sim, trace, damage);
        if (trace.Victim is null)
        {
            if (trace.Wall?.Special != 9)
            {
                var yawRadians = angle.ToDegrees() * Math.PI / 180;
                var pitchRadians = pitch.ToDegrees() * Math.PI / 180;
                var horizontal = Math.Cos(pitchRadians);
                var puff = sim.SpawnHitscanPuff(trace.X - 4 * horizontal * Math.Cos(yawRadians),
                    trace.Y - 4 * horizontal * Math.Sin(yawRadians), trace.Z + 4 * Math.Sin(pitchRadians), puffTid);
                puff.Radius = new Fixed(1);
                puff.Angle = new BamAngle(unchecked(angle.Raw + 0x80000000u));
                puff.DamageSource = source;
            }
            return 0;
        }

        var actorPuff = sim.SpawnHitscanPuff(trace.X, trace.Y, trace.Z, puffTid);
        actorPuff.Angle = new BamAngle(unchecked(angle.Raw + 0x80000000u));
        actorPuff.DamageSource = source;
        ActorDamage.Apply(trace.Victim, damage, source, DamageFlags.None, damageType ?? "None", actorPuff);
        return 1;
    }

}
