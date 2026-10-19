namespace HCDE.Playsim;

/// <summary>Native <c>p_enemy.cpp</c> <c>A_MonsterRail</c> hitscan subset (no true rail pierce).</summary>
public static class ActorMonsterRailActions
{
    private const double MissileRange = 2048;

    public static void MonsterRail(Actor actor)
    {
        if (actor.Destroyed) return;
        var sim = actor.Simulation ?? throw new InvalidOperationException("Monster rail requires a simulation.");
        var target = AcsActorPointer.Resolve(sim, actor, AcsActorPointer.Target);
        if (target is null || target.Destroyed) return;
        var savedPitch = actor.PitchDegrees;
        if (actor.Stealth) actor.VisibilityDirection = 1;
        actor.Ambush = false;
        ActorOrientationActions.Face(actor, target);
        var dx = target.X.ToDouble() - actor.X.ToDouble();
        var dy = target.Y.ToDouble() - actor.Y.ToDouble();
        var pitch = BamAngle.FromDegrees(-Math.Atan2(target.Center() - actor.AttackOriginZ(),
            Math.Max(1, Math.Sqrt(dx * dx + dy * dy))) * 180 / Math.PI);
        var hit = CombatTrace.TraceLineAttack(sim, actor, actor.Angle, pitch, MissileRange);
        if (hit.Victim is null)
            actor.PitchDegrees = pitch.ToDegrees();
        var leadYaw = Math.Atan2(target.Y.ToDouble() + target.VelocityY.ToDouble() * 3 - actor.Y.ToDouble(),
            target.X.ToDouble() + target.VelocityX.ToDouble() * 3 - actor.X.ToDouble()) * 180 / Math.PI;
        actor.Angle = BamAngle.FromDegrees(leadYaw);
        var damage = actor.GetMissileDamage(0, 1);
        hit = CombatTrace.TraceLineAttack(sim, actor, actor.Angle, BamAngle.FromDegrees(actor.PitchDegrees), MissileRange);
        if (hit.Victim is { } victim)
            LineAttackImpact.ApplyActorDamage(sim, actor, victim, hit, actor.Angle, damage, actor.DamageType ?? "Hitscan", melee: false);
        else
        {
            GeometryLineAttack.Apply(sim, hit, damage);
            LineAttackImpact.SpawnGeometryPuff(sim, actor, hit, actor.Angle, BamAngle.FromDegrees(actor.PitchDegrees));
        }
        actor.PitchDegrees = savedPitch;
    }
}
