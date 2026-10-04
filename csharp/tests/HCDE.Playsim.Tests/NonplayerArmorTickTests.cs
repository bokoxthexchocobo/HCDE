namespace HCDE.Playsim.Tests;

public class NonplayerArmorTickTests
{
    [Fact]
    public void NonplayerTickRestoresLimitedArmorAllowance()
    {
        var actor = new Actor { Armor = 100, ArmorSavePercent = 100, MaxAbsorb = 5 };
        Assert.Equal(5, ActorDamage.Apply(actor, 10).ArmorLost);
        Assert.Equal(0, ActorDamage.Apply(actor, 10).ArmorLost);
        Assert.Equal(5, actor.AbsorbCount);
        actor.Tick();
        Assert.Equal(0, actor.AbsorbCount);
        Assert.Equal(5, ActorDamage.Apply(actor, 10).ArmorLost);
        Assert.Equal(90, actor.Armor);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(100, true)]
    public void RetainedCounterResetsForDeadAndDormantActors(int health, bool dormant)
    {
        var actor = new Actor { Health = health, Dormant = dormant, AbsorbCount = 7,
            Armor = 80, ArmorSavePercent = 50 };
        actor.Tick();
        Assert.Equal(0, actor.AbsorbCount);
        Assert.Equal(80, actor.Armor);
        Assert.Equal(50, actor.ArmorSavePercent);
    }
}
