namespace HCDE.Playsim;

[Flags]
public enum ChainsawHitFlags
{
    None = 0,
    NoPullIn = 32,
    NoTurn = 64,
}

[Flags]
public enum ChainsawAttackFlags
{
    None = 0,
    NoRandom = 1,
    RandomLightMiss = 2,
    RandomLightHit = 4,
    RandomLightBoth = RandomLightMiss | RandomLightHit,
    NoPullIn = 32,
    NoTurn = 64,
    StealArmor = 128,
    NoRandomPuffZ = 256,
}

/// <summary>Supported native A_Saw attack and post-impact behavior.</summary>
public static class ChainsawActions
{
    public static void Attack(PlayerPawn player, int damage = 2, double range = 0,
        double spread = 2.8125, ChainsawAttackFlags flags = ChainsawAttackFlags.None,
        string fullSound = "weapons/sawfull", string hitSound = "weapons/sawhit",
        double lifeSteal = 0, int lifeStealMax = 0, double verticalSpread = 0)
    {
        if (!double.IsFinite(range)) throw new ArgumentOutOfRangeException(nameof(range));
        if (!double.IsFinite(spread)) throw new ArgumentOutOfRangeException(nameof(spread));
        if (!double.IsFinite(lifeSteal)) throw new ArgumentOutOfRangeException(nameof(lifeSteal));
        if (!double.IsFinite(verticalSpread)) throw new ArgumentOutOfRangeException(nameof(verticalSpread));
        if ((flags & ~(ChainsawAttackFlags.NoRandom | ChainsawAttackFlags.RandomLightBoth | ChainsawAttackFlags.NoPullIn
            | ChainsawAttackFlags.NoTurn | ChainsawAttackFlags.StealArmor | ChainsawAttackFlags.NoRandomPuffZ)) != 0)
            throw new NotSupportedException("The requested chainsaw attack flags are not supported.");
        var sim = player.Simulation ?? throw new InvalidOperationException("Chainsaw attacks require an attached simulation.");
        if (damage == 0) damage = 2;
        if ((flags & ChainsawAttackFlags.NoRandom) == 0) damage = unchecked(damage * sim.NextSawDamageMultiplier());
        if (range == 0) range = player.MeleeRange.ToDouble() + 20 + 1.0 / Fixed.Unit;
        var angle = BamAngle.FromDegrees(player.Angle.ToDegrees() + sim.NextSawSpread() * spread);
        var pitch = BamAngle.FromDegrees(player.PitchDegrees + sim.NextSawSpread() * verticalSpread);
        var hit = CombatTrace.TraceLineAttack(sim, player, angle, pitch, range);
        var noRandomPuffZ = (flags & ChainsawAttackFlags.NoRandomPuffZ) != 0;
        if (hit.Victim is { } target)
        {
            var facing = BamAngle.FromDegrees(player.AngleTo(target));
            var result = LineAttackImpact.ApplyActorDamage(sim, player, target, hit, angle, damage, "Melee", false,
                noRandomPuffZ: noRandomPuffZ);
            if ((flags & ChainsawAttackFlags.RandomLightHit) != 0)
                ApplyRandomLight(player, sim.NextSawByte(), true);
            if (lifeSteal != 0 && !target.DontDrain)
            {
                var amount = (int)(result.HealthLost * lifeSteal);
                if ((flags & ChainsawAttackFlags.StealArmor) != 0)
                    PickupCatalog.GiveArmorBonus(player.Inventory, PickupCatalog.ScaleArmor(amount, player),
                        lifeStealMax > 0 ? lifeStealMax : sim.DehackedMaxArmor);
                else player.GiveBody(amount, lifeStealMax);
            }
            ApplyHitEffects(player, facing, (ChainsawHitFlags)((int)flags & 96), hitSound);
        }
        else
        {
            GeometryLineAttack.Apply(sim, hit, damage);
            LineAttackImpact.SpawnGeometryPuff(sim, player, hit, angle, pitch, noRandomPuffZ: noRandomPuffZ);
            if ((flags & ChainsawAttackFlags.RandomLightMiss) != 0)
                ApplyRandomLight(player, sim.NextSawByte(), false);
            ActorSoundActions.StartSound(player, fullSound, channel: 1);
        }
    }

    internal static void ApplyRandomLight(PlayerPawn player, int randomByte, bool hit)
    {
        if (hit) player.ExtraLight = randomByte < 64 ? 0 : randomByte < 160 ? 1 : 2;
        else if (randomByte > 64) player.ExtraLight = player.ExtraLight == 0 ? 1 : 0;
    }

    public static void ApplyHitEffects(PlayerPawn player, BamAngle targetAngle,
        ChainsawHitFlags flags = ChainsawHitFlags.None, string hitSound = "weapons/sawhit")
    {
        if ((flags & ~(ChainsawHitFlags.NoPullIn | ChainsawHitFlags.NoTurn)) != 0)
            throw new NotSupportedException("The requested chainsaw hit flags are not supported.");
        ActorSoundActions.StartSound(player, hitSound, channel: 1);
        if ((flags & ChainsawHitFlags.NoTurn) == 0)
        {
            var difference = Actor.Normalize180(targetAngle.ToDegrees() - player.Angle.ToDegrees());
            var sawAngle = difference < 0
                ? difference < -4.5 ? targetAngle.ToDegrees() + 90.0 / 21 : player.Angle.ToDegrees() - 4.5
                : difference > 4.5 ? targetAngle.ToDegrees() - 90.0 / 21 : player.Angle.ToDegrees() + 4.5;
            player.Angle = BamAngle.FromDegrees(sawAngle);
        }
        if ((flags & ChainsawHitFlags.NoPullIn) == 0) player.JustAttacked = true;
    }
}
