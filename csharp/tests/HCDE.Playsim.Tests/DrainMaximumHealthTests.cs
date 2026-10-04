namespace HCDE.Playsim.Tests;

public class DrainMaximumHealthTests
{
    [Theory]
    [InlineData(250, 145, 155)]
    [InlineData(150, 145, 150)]
    [InlineData(50, 45, 50)]
    [InlineData(50, 80, 80)]
    [InlineData(0, 95, 100)]
    [InlineData(-7, 95, 100)]
    public void DrainUsesEffectiveMaximumAndPreservesOverCapHealth(int maximum, int health, int expected)
    {
        var source = new PlayerPawn { Health = health, MaxHealth = maximum, DrainStrength = 0.5 };
        var target = new Actor { Health = 100 };
        ActorDamage.Apply(target, 20, source);
        Assert.Equal(expected, source.Health);
        Assert.Equal(80, target.Health);
    }

    [Fact]
    public void DrainClampsBodyGrantBeforeAdding()
    {
        var source = new PlayerPawn { Health = 100, MaxHealth = int.MaxValue, DrainStrength = 1 };
        ActorDamage.Apply(new Actor { Health = 500000 }, 200000, source);
        Assert.Equal(65636, source.Health);
    }

    [Fact]
    public void DrainNearIntegerMaximumDoesNotOverflow()
    {
        var source = new PlayerPawn { Health = int.MaxValue - 5, MaxHealth = int.MaxValue, DrainStrength = 1 };
        ActorDamage.Apply(new Actor { Health = 100 }, 20, source);
        Assert.Equal(int.MaxValue, source.Health);
    }

    [Fact]
    public void DrainUsesPostArmorDamage()
    {
        var source = new PlayerPawn { Health = 100, MaxHealth = 250, DrainStrength = 0.5 };
        var target = new Actor { Health = 100, Armor = 100, ArmorSavePercent = 50 };
        var result = ActorDamage.Apply(target, 20, source);
        Assert.Equal(10, result.ArmorLost);
        Assert.Equal(105, source.Health);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(100, true)]
    public void DrainDoesNotHealDeadSourceOrExcludedTarget(int health, bool excluded)
    {
        var source = new PlayerPawn { Health = health, MaxHealth = 250, DrainStrength = 1 };
        ActorDamage.Apply(new Actor { Health = 100, DontDrain = excluded }, 20, source);
        Assert.Equal(health, source.Health);
    }
}
