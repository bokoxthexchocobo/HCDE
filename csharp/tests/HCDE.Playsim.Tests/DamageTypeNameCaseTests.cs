namespace HCDE.Playsim.Tests;

public class DamageTypeNameCaseTests
{
    [Theory]
    [InlineData("fire")]
    [InlineData("FIRE")]
    public void TypedPainUsesBothFrameAndChanceRegardlessOfCase(string type)
    {
        var actor = new Actor { Health = 100, PainChance = 0 };
        actor.SetTypedPain("Fire", 3, chance: 256);
        ActorDamage.Apply(actor, 10, damageType: type);
        Assert.Equal(3, actor.States.Current);
    }

    [Theory]
    [InlineData("fire")]
    [InlineData("FIRE")]
    public void TypedWoundUsesNativeNameCase(string type)
    {
        var actor = new Actor { Health = 100, WoundHealth = 95 };
        actor.SetTypedWound("Fire", 3);
        ActorDamage.Apply(actor, 10, damageType: type);
        Assert.Equal(3, actor.States.Current);
    }

    [Theory]
    [InlineData("none")]
    [InlineData("NONE")]
    public void ReservedNamesAreRejected(string type)
    {
        Assert.Throws<ArgumentException>(() => new Actor().SetTypedPain(type, 1));
        Assert.Throws<ArgumentException>(() => new Actor().SetTypedWound(type, 1));
    }

    [Fact]
    public void MixedCaseDrowningBypassesArmor()
    {
        var actor = new Actor { Health = 100, Armor = 100, ArmorSavePercent = 50 };
        var result = ActorDamage.Apply(actor, 20, damageType: "dRoWnInG");
        Assert.Equal(20, result.HealthLost);
        Assert.Equal(0, result.ArmorLost);
        Assert.Equal(100, actor.Armor);
    }

    [Fact]
    public void MixedCaseIceStillRequiresShatterPermission()
    {
        var corpse = new Actor { Health = 0, Shootable = true, IceCorpse = true };
        ActorDamage.Apply(corpse, 10, damageType: "iCe", inflictor: new Actor());
        Assert.False(corpse.Shattering);
        ActorDamage.Apply(corpse, 10, damageType: "iCe", inflictor: new Actor { IceShatter = true });
        Assert.True(corpse.Shattering);
    }
}
