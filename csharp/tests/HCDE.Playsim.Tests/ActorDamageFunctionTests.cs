using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorDamageFunctionTests
{
    [Theory]
    [InlineData(-7)]
    [InlineData(0)]
    [InlineData(19)]
    public void FunctionRunsOnceAndBypassesMaskWithoutSimulation(int amount)
    {
        var actor = new Actor { Damage = -1 }; var calls = 0;
        actor.DamageFunction = self => { Assert.Same(actor, self); calls++; return amount; };
        Assert.False(actor.IsZeroDamage());
        Assert.Equal(amount, actor.GetMissileDamage(7, 1)); Assert.Equal(1, calls);
        actor.SetDamage(0); Assert.Null(actor.DamageFunction); Assert.True(actor.IsZeroDamage());
        Assert.Equal(0, actor.GetMissileDamage(0, 1));
    }

    [Fact]
    public void ChargeCollisionUsesFunctionWithoutMissileDice()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 512 }],
            Things = [new LevelThing { Type = 1, X = 64 }, new LevelThing { Type = 3006 }] });
        var soul = sim.Actors.Single(a => a.DoomEdNum == 3006); var target = sim.Players.Single();
        target.NoPain = true; soul.Damage = -1; var calls = 0;
        soul.DamageFunction = actor => { Assert.Same(soul, actor); calls++; return 11; };
        soul.Brain!.StartCharge(soul, target);
        for (var i = 0; i < 10 && soul.Brain.Charging; i++) soul.Tick();
        Assert.Equal(89, target.Health); Assert.Equal(1, calls);
        Assert.Null(sim.CaptureState().MissileDamageRandomState);
    }

    [Fact]
    public void ProjectileSupportsFunctionAndSetDamageClearsBothFunctionForms()
    {
        var missile = new ProjectileActor(new Actor(), ProjectileKind.Plasma);
        missile.DamageFunction = _ => 13;
        Assert.Equal(13, missile.GetMissileDamage(7, 1));
        missile.DamageExpression = _ => 17;
        Assert.Equal(17, missile.GetMissileDamage(7, 1));
        missile.SetDamage(0);
        Assert.Null(missile.DamageFunction); Assert.Null(missile.DamageExpression);
        Assert.True(missile.IsZeroDamage());
    }
}
