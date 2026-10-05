namespace HCDE.Playsim.Tests;

public class AcsActorStateFlagTests
{
    [Theory]
    [InlineData("justhit")]
    [InlineData("actlikebridge")]
    [InlineData("icecorpse")]
    [InlineData("shattering")]
    [InlineData("noautooffskullfly")]
    public void StateFlagsSupportNativeCaseInsensitiveNames(string name)
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
    [InlineData(false)]
    [InlineData(true)]
    public void ScriptIceCorpseControlsFrozenCorpseShattering(bool frozen)
    {
        var corpse = new Actor { Health = 0, Shootable = true };
        Assert.True(AcsActorFlags.TrySet(corpse, "ICECORPSE", frozen));
        ActorDamage.Apply(corpse, 10);
        Assert.Equal(frozen, corpse.Shattering);
    }
}
