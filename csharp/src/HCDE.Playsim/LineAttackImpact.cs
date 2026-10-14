namespace HCDE.Playsim;

internal static class LineAttackImpact
{
    internal static DamageResult ApplyActorDamage(AuthoritySimulation sim, Actor source, Actor target,
        CombatTrace.LineAttackHit hit, BamAngle angle, int damage, string damageType, bool melee,
        int puffTid = 0, bool noRandomPuffZ = false)
    {
        var temporary = !target.NoBlood && !target.Invulnerable && !target.Dormant;
        if (temporary && damage == 0) return default;
        var puff = sim.SpawnHitscanPuff(hit.X, hit.Y, hit.Z, noRandomZ: noRandomPuffZ);
        puff.DamageSource = source;
        puff.Angle = temporary ? BamAngle.FromDegrees(180) : new BamAngle(unchecked(angle.Raw + 0x80000000u));
        if (melee) puff.States.Enter(puff, 2);
        try
        {
            var result = damage == 0 ? default : ActorDamage.Apply(target, damage, source, damageType: damageType, inflictor: puff);
            if (!temporary) puff.ThingId = puffTid;
            return result;
        }
        finally { if (temporary) sim.DiscardTemporaryPuff(puff); }
    }

    internal static PuffActor? SpawnGeometryPuff(AuthoritySimulation sim, Actor source,
        CombatTrace.LineAttackHit hit, BamAngle angle, BamAngle pitch,
        int thingId = 0, bool noRandomPuffZ = false, bool melee = false)
    {
        if (!hit.Hit || hit.Victim is not null || hit.Wall?.Special == 9) return null;
        var yawRadians = angle.ToDegrees() * Math.PI / 180;
        var pitchRadians = pitch.ToDegrees() * Math.PI / 180;
        var horizontal = Math.Cos(pitchRadians);
        var puff = sim.SpawnHitscanPuff(hit.X - 4 * horizontal * Math.Cos(yawRadians),
            hit.Y - 4 * horizontal * Math.Sin(yawRadians), hit.Z + 4 * Math.Sin(pitchRadians),
            thingId, noRandomZ: noRandomPuffZ);
        puff.Radius = new Fixed(1);
        puff.Angle = new BamAngle(unchecked(angle.Raw + 0x80000000u));
        puff.DamageSource = source;
        if (melee) puff.States.Enter(puff, 2);
        return puff;
    }
}
