namespace HCDE.Playsim.Tests;

public class ZeroMissileForcePainTests
{
    [Theory]
    [InlineData(false, false, false, true)]
    [InlineData(true, false, false, false)]
    [InlineData(false, true, false, false)]
    [InlineData(false, false, true, false)]
    public void ZeroMissileForcesPainWithoutHealthLossOrDice(bool noPain, bool painless, bool damageNoPain, bool flinches)
    {
        var source = new Actor();
        var missile = new ProjectileActor(source, ProjectileKind.Plasma)
        { Damage = 0, DamageExpression = _ => 0, ForcePain = true, Painless = painless };
        var target = new Actor { Health = 100, Armor = 100, ArmorSavePercent = 100,
            PainChance = 0, PainThreshold = 1000, NoPain = noPain, ReactionTime = 12 };
        var result = damageNoPain
            ? ActorDamage.Apply(target, 0, source, DamageFlags.NoPain, inflictor: missile)
            : missile.DoMissileDamage(target);
        Assert.Equal(default, result); Assert.Equal(100, target.Health); Assert.Equal(100, target.Armor);
        Assert.Equal(flinches ? 1 : 0, target.States.Current);
        Assert.Equal(0, target.ReactionTime);
    }

    [Fact]
    public void ConstantZeroStillConsumesMissileDiceButSkipsPainRoll()
    {
        var sim = AuthoritySimulation.Start(new HCDE.MapLoader.PlayLevel());
        var source = sim.AddBot(-1000, 0); source.Brain = null;
        var missile = sim.SpawnProjectile(source, ProjectileKind.Plasma); missile.SetDamage(0); missile.ForcePain = true;
        var target = new Actor { Health = 100, PainChance = 0 };
        var prediction = AuthoritySimulation.Start(new HCDE.MapLoader.PlayLevel());
        prediction.NextCombatRandom();
        var health = target.Health;
        missile.DoMissileDamage(target);
        Assert.Equal(health, target.Health); Assert.Equal(1, target.States.Current);
        Assert.Equal(prediction.CombatRandomState, sim.CombatRandomState);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void InvulnerabilityAndDormancyStillBlockZeroForcedPain(bool invulnerable, bool dormant)
    {
        var target = new Actor { Health = 100, Invulnerable = invulnerable, Dormant = dormant, ReactionTime = 12 };
        ActorDamage.Apply(target, 0, inflictor: new Actor { ForcePain = true });
        Assert.Equal(0, target.States.Current); Assert.Equal(12, target.ReactionTime);
    }
}
