namespace HCDE.Playsim;

[Flags]
public enum BfgSprayFlags
{
    None = 0,
    HurtSource = 1,
    MissileOrigin = 2,
}

public static class BfgSprayActions
{
    /// <summary>Native A_BFGSpray parameter defaults and default damage context. Vertical autoaim is not implemented.</summary>
    public static void Apply(AuthoritySimulation sim, Actor missile, Actor? owner, int numRays = 40,
        int damageCount = 15, double spread = 90, double distance = 1024, int fixedDamage = 0,
        BfgSprayFlags flags = BfgSprayFlags.None, Func<Actor, Actor?>? spawnSpray = null)
    {
        ArgumentNullException.ThrowIfNull(sim);
        ArgumentNullException.ThrowIfNull(missile);
        if (!double.IsFinite(spread)) throw new ArgumentOutOfRangeException(nameof(spread));
        if (!double.IsFinite(distance)) throw new ArgumentOutOfRangeException(nameof(distance));
        if (numRays <= 0) numRays = 40;
        if (damageCount <= 0) damageCount = 15;
        if (spread == 0) spread = 90;
        if (distance <= 0) distance = 1024;
        if (owner == null) return;
        var originator = flags.HasFlag(BfgSprayFlags.MissileOrigin) ? missile : owner;
        for (var ray = 0; ray < numRays; ray++)
        {
            var yaw = missile.Angle.ToDegrees() - spread / 2 + spread * ray / numRays;
            var target = CombatTrace.TraceLineAttack(sim, originator, BamAngle.FromDegrees(yaw), default,
                distance, allowDestroyedSource: true, allowDeadSource: true).Victim;
            if (target == null) continue;
            var spray = spawnSpray == null ? new Actor { DamageType = "BFGSplash" } : spawnSpray(target);
            var damageFlags = DamageFlags.None;
            var damageType = "BFGSplash";
            if (spray != null)
            {
                if (spray.MThruSpecies && owner.SharesContactSpecies(target)
                    || ReferenceEquals(target, owner) && !flags.HasFlag(BfgSprayFlags.HurtSource))
                {
                    spray.Destroy();
                    continue;
                }
                if (spray.FoilBuddha) damageFlags |= DamageFlags.FoilBuddha;
                if (spray.FoilInvul) damageFlags |= DamageFlags.FoilInvulnerability;
                damageType = spray.DamageType;
            }
            var damage = fixedDamage;
            if (fixedDamage == 0)
                for (var die = 0; die < damageCount; die++) damage += 1 + (int)(sim.NextCombatRandom() % 8);
            ActorDamage.Apply(target, damage, owner, damageFlags, damageType, originator);
        }
    }
}
