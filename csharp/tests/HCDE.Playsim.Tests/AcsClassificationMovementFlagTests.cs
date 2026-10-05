namespace HCDE.Playsim.Tests;

public class AcsClassificationMovementFlagTests
{
    [Theory]
    [InlineData("ismonster")]
    [InlineData("noblockmonst")]
    [InlineData("spawnceiling")]
    [InlineData("dropoff")]
    [InlineData("onmobj")]
    [InlineData("infloat")]
    public void FlagsSupportNativeCaseInsensitiveNames(string name)
    {
        var actor = new Actor();
        Assert.True(AcsActorFlags.TrySet(actor, name, true));
        Assert.True(AcsActorFlags.TryGet(actor, name.ToUpperInvariant(), out var enabled));
        Assert.True(enabled);
        Assert.True(AcsActorFlags.TrySet(actor, name.ToUpperInvariant(), false));
        Assert.True(AcsActorFlags.TryGet(actor, name, out enabled));
        Assert.False(enabled);
    }

    [Theory]
    [InlineData(false, 2)]
    [InlineData(true, 3)]
    public void ScriptIsMonsterControlsGenericFreezeEligibility(bool monster, int deathState)
    {
        var actor = new Actor { Health = 10, GenericFreezeDeath = 3 };
        Assert.True(AcsActorFlags.TrySet(actor, "ISMONSTER", monster));
        ActorDamage.Apply(actor, 10, damageType: "Ice");
        Assert.Equal(deathState, actor.States.Current);
    }
}
