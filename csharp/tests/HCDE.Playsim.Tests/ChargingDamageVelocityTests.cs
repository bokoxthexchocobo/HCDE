namespace HCDE.Playsim.Tests;

public class ChargingDamageVelocityTests
{
    [Theory]
    [InlineData(20)]
    [InlineData(0)]
    public void DormantActorClearsChargeVelocityBeforeRejectingDamage(int damage)
    {
        var actor = new Actor { Health = 100, Dormant = true, Brain = new MonsterBrain(MonsterAttack.Hitscan) };
        actor.Brain.StartCharge(actor, new Actor { X = Fixed.FromInt(100) });
        actor.VelocityZ = Fixed.FromInt(3);
        Assert.Equal(default, ActorDamage.Apply(actor, damage, flags: DamageFlags.NoPain));
        Assert.Equal(100, actor.Health); Assert.True(actor.Dormant); Assert.True(actor.Brain.Charging);
        Assert.Equal(0, actor.VelocityX.Raw); Assert.Equal(0, actor.VelocityY.Raw); Assert.Equal(0, actor.VelocityZ.Raw);
    }

    [Fact]
    public void InvulnerableMonsterRejectsDamageBeforeChargeVelocityReset()
    {
        var actor = new Actor { Health = 100, Invulnerable = true, Brain = new MonsterBrain(MonsterAttack.Hitscan) };
        actor.Brain.StartCharge(actor, new Actor { X = Fixed.FromInt(100) });
        var velocity = actor.VelocityX.Raw;
        Assert.NotEqual(0, velocity);
        Assert.Equal(default, ActorDamage.Apply(actor, 20));
        Assert.Equal(velocity, actor.VelocityX.Raw); Assert.True(actor.Brain.Charging);
    }
    [Theory]
    [InlineData(20)]
    [InlineData(0)]
    public void DamageClearsChargeVelocityWithoutEndingCharge(int damage)
    {
        var actor = new Actor { Health = 100, Brain = new MonsterBrain(MonsterAttack.Hitscan) };
        actor.Brain.StartCharge(actor, new Actor { X = Fixed.FromInt(100) });
        actor.VelocityZ = Fixed.FromInt(3);
        ActorDamage.Apply(actor, damage, flags: DamageFlags.NoPain);
        Assert.Equal(0, actor.VelocityX.Raw); Assert.Equal(0, actor.VelocityY.Raw);
        Assert.Equal(0, actor.VelocityZ.Raw); Assert.True(actor.Brain.Charging);
    }

    [Fact]
    public void NonChargingVelocityRemainsUnchanged()
    {
        var actor = new Actor { Health = 100, VelocityX = Fixed.FromInt(5), VelocityZ = Fixed.FromInt(3) };
        ActorDamage.Apply(actor, 20, flags: DamageFlags.NoPain);
        Assert.Equal(Fixed.FromInt(5).Raw, actor.VelocityX.Raw);
        Assert.Equal(Fixed.FromInt(3).Raw, actor.VelocityZ.Raw);
    }
}
