namespace HCDE.Playsim.Tests;

public class AcsMovementPolicyFlagTests
{
    [Theory]
    [InlineData("notelefrag")]
    [InlineData("alwaystelefrag")]
    [InlineData("noteleport")]
    [InlineData("slidesonwalls")]
    [InlineData("dormant")]
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

    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 20)]
    public void ScriptDormantBlocksOrdinaryDamageButAllowsForcedDamage(bool forced, int lost)
    {
        var target = new Actor { Health = 100 };
        Assert.True(AcsActorFlags.TrySet(target, "DORMANT", true));
        Assert.Equal(lost, ActorDamage.Apply(target, 20, flags: forced ? DamageFlags.Forced : DamageFlags.None).HealthLost);
    }

    [Fact]
    public void ScriptFlagMutationRejectsDestroyedActor()
    {
        var actor = new Actor();
        actor.Destroy();
        Assert.False(AcsActorFlags.TrySet(actor, "NOTELEFRAG", true));
        Assert.False(AcsActorFlags.TryGet(actor, "NOTELEFRAG", out _));
        Assert.False(actor.NoTelefrag);
    }
}
