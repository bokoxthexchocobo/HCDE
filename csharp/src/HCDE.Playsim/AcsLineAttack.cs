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
        bool absoluteAngles = false,
        bool noRandomPuffZ = false)
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
            LineAttackImpact.SpawnGeometryPuff(sim, source, trace, angle, pitch, puffTid, noRandomPuffZ);
            return 0;
        }

        LineAttackImpact.ApplyActorDamage(sim, source, trace.Victim, trace, angle, damage,
            damageType ?? "None", melee: false, puffTid, noRandomPuffZ);
        return 1;
    }

}
