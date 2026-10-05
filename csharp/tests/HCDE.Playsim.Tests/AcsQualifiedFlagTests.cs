namespace HCDE.Playsim.Tests;

public class AcsQualifiedFlagTests
{
    [Theory]
    [InlineData("Actor.FOILINVUL", "foilinvul")]
    [InlineData("aCtOr.ISMONSTER", "ismonster")]
    [InlineData("Actor.NORADIUSDMG", "noradiusdmg")]
    [InlineData("Actor.SLIDESONWALLS", "slidesonwalls")]
    public void QualifiedActorFlagUsesSamePropertyAsUnqualifiedName(string qualified, string plain)
    {
        var actor = new Actor();
        Assert.True(AcsActorFlags.TrySet(actor, qualified, true));
        Assert.True(AcsActorFlags.TryGet(actor, plain, out var enabled));
        Assert.True(enabled);
        Assert.True(AcsActorFlags.TrySet(actor, plain, false));
        Assert.True(AcsActorFlags.TryGet(actor, qualified, out enabled));
        Assert.False(enabled);
    }

    [Theory]
    [InlineData("Inventory.FOILINVUL")]
    [InlineData("Monster.FOILINVUL")]
    [InlineData("Actor.Inventory.ALWAYSPICKUP")]
    [InlineData("Actor.ALWAYSPICKUP")]
    [InlineData("Actor.")]
    public void WrongFlagScopesAreRejectedWithoutMutation(string name)
    {
        var actor = new Actor { DoomEdNum = PickupCatalog.HealthBonus };
        Assert.False(AcsActorFlags.TrySet(actor, name, true));
        Assert.False(AcsActorFlags.TryGet(actor, name, out _));
        Assert.False(actor.FoilInvul);
        Assert.Null(actor.AlwaysPickupOverride);
    }
}
