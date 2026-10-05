namespace HCDE.Playsim.Tests;

public class OnDrainCallbackTests
{
    private sealed class DrainPlayer : PlayerPawn
    {
        public int Result { get; init; }
        public int Input { get; private set; }
        public Actor? Target { get; private set; }
        public string? Type { get; private set; }
        public override int OnDrain(Actor target, int amount, string? damageType)
        { Input = amount; Target = target; Type = damageType; return Result; }
    }

    [Theory]
    [InlineData(0, 50)]
    [InlineData(20, 70)]
    [InlineData(-100, 100)]
    public void CallbackControlsBodyGrant(int result, int health)
    {
        var source = new DrainPlayer { Health = 50, DrainStrength = 0.5, Result = result };
        var target = new Actor { Health = 100 };
        ActorDamage.Apply(target, 20, source, damageType: "Fire");
        Assert.Equal(10, source.Input); Assert.Same(target, source.Target); Assert.Equal("Fire", source.Type);
        Assert.Equal(health, source.Health);
    }

    [Fact]
    public void DeadSourceStillReceivesCallbackButCannotReceiveHealth()
    {
        var source = new DrainPlayer { Health = 0, DrainStrength = 0.5, Result = 20 };
        var target = new Actor { Health = 100 };
        ActorDamage.Apply(target, 20, source, damageType: "Fire");
        Assert.Same(target, source.Target);
        Assert.Equal(10, source.Input);
        Assert.Equal(0, source.Health);
    }

    [Fact]
    public void FullyAbsorbedPlayerHitDoesNotInvokeDrainCallback()
    {
        var source = new DrainPlayer { Health = 50, DrainStrength = 0.5, Result = 20 };
        var target = new PlayerPawn();
        target.Inventory.Armor = 100;
        target.Inventory.ArmorSavePercent = 100;
        var result = ActorDamage.Apply(target, 20, source);
        Assert.Equal(0, result.HealthLost);
        Assert.Equal(20, result.ArmorLost);
        Assert.Null(source.Target);
        Assert.Equal(50, source.Health);
    }

    [Fact]
    public void FullyAbsorbedMonsterHitDoesNotInvokeDrainCallback()
    {
        var source = new DrainPlayer { Health = 50, DrainStrength = 0.5, Result = 20 };
        var target = new Actor { Health = 100, Armor = 100, ArmorSavePercent = 100 };
        ActorDamage.Apply(target, 20, source);
        Assert.Null(source.Target);
        Assert.Equal(50, source.Health);
    }

    [Fact]
    public void DrainCanHealAboveBaseMaximumIntoUpgrades()
    {
        var source = new PlayerPawn { Health = 100, BonusHealth = 20, DrainStrength = 1 };
        ActorDamage.Apply(new Actor { Health = 100 }, 20, source);
        Assert.Equal(120, source.Health);
    }
}
