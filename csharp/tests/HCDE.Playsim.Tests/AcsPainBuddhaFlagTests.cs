namespace HCDE.Playsim.Tests;

public class AcsPainBuddhaFlagTests
{
    [Theory]
    [InlineData("buddha")]
    [InlineData("foilbuddha")]
    [InlineData("forcepain")]
    [InlineData("painless")]
    public void FlagsSupportCaseInsensitiveSetQueryAndClear(string name)
    {
        var actor = new Actor();
        Assert.True(AcsActorFlags.TrySet(actor, name, true));
        Assert.True(AcsActorFlags.TryGet(actor, name.ToUpperInvariant(), out var enabled));
        Assert.True(enabled);
        Assert.True(AcsActorFlags.TrySet(actor, name.ToUpperInvariant(), false));
        Assert.True(AcsActorFlags.TryGet(actor, name, out enabled));
        Assert.False(enabled);
    }

    [Fact]
    public void ScriptBuddhaAndFoilBuddhaControlMonsterSurvival()
    {
        var target = new Actor { Health = 10 };
        var inflictor = new Actor();
        Assert.True(AcsActorFlags.TrySet(target, "BUDDHA", true));
        Assert.False(ActorDamage.Apply(target, 20, inflictor: inflictor).Killed);
        Assert.Equal(1, target.Health);
        Assert.True(AcsActorFlags.TrySet(inflictor, "FOILBUDDHA", true));
        Assert.True(ActorDamage.Apply(target, 20, inflictor: inflictor).Killed);
    }

    [Fact]
    public void ScriptPainlessOverridesForcedPain()
    {
        var target = new Actor { Health = 100, PainChance = 0 };
        var inflictor = new Actor();
        Assert.True(AcsActorFlags.TrySet(inflictor, "FORCEPAIN", true));
        ActorDamage.Apply(target, 10, inflictor: inflictor);
        Assert.Equal(target.PainState, target.States.Current);
        target.States.Enter(target, target.SpawnState);
        Assert.True(AcsActorFlags.TrySet(inflictor, "PAINLESS", true));
        ActorDamage.Apply(target, 10, inflictor: inflictor);
        Assert.Equal(target.SpawnState, target.States.Current);
        Assert.Equal(80, target.Health);
    }
}
