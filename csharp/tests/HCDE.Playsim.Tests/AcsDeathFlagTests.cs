namespace HCDE.Playsim.Tests;

public class AcsDeathFlagTests
{
    [Theory]
    [InlineData("noicedeath")]
    [InlineData("extremedeath")]
    [InlineData("noextremedeath")]
    [InlineData("iceshatter")]
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
    public void ScriptNoIceDeathBlocksGenericFreeze()
    {
        var target = new Actor { Health = 10, IsMonster = true, GenericFreezeDeath = 3 };
        Assert.True(AcsActorFlags.TrySet(target, "NOICEDEATH", true));
        ActorDamage.Apply(target, 10, damageType: "Ice");
        Assert.Equal(2, target.States.Current);
    }

    [Theory]
    [InlineData(false, 3)]
    [InlineData(true, 2)]
    public void ScriptNoExtremeDeathOverridesInflictorExtremeDeath(bool suppress, int state)
    {
        var inflictor = new Actor();
        Assert.True(AcsActorFlags.TrySet(inflictor, "EXTREMEDEATH", true));
        Assert.True(AcsActorFlags.TrySet(inflictor, "NOEXTREMEDEATH", suppress));
        var target = new Actor { Health = 100, ExtremeDeathState = 3 };
        ActorDamage.Apply(target, 100, inflictor: inflictor);
        Assert.Equal(state, target.States.Current);
    }

    [Fact]
    public void ScriptIceShatterAllowsIceHitOnFrozenCorpse()
    {
        var inflictor = new Actor();
        var corpse = new Actor { Health = 0, Shootable = true, IceCorpse = true };
        ActorDamage.Apply(corpse, 10, damageType: "Ice", inflictor: inflictor);
        Assert.False(corpse.Shattering);
        Assert.True(AcsActorFlags.TrySet(inflictor, "ICESHATTER", true));
        ActorDamage.Apply(corpse, 10, damageType: "Ice", inflictor: inflictor);
        Assert.True(corpse.Shattering);
    }
}
