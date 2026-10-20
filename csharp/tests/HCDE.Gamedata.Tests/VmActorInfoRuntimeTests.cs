namespace HCDE.Gamedata.Tests;

public class VmActorInfoRuntimeTests
{
    [Fact]
    public void MatchesCallerType_RejectsWrongClass()
    {
        var action = new VmStateActionBinding { ImplicitArgs = 1, RequiredCallerType = "Actor" };
        Assert.False(VmActorStateRuntime.MatchesCallerType(action, "Item", null));
    }

    [Fact]
    public void GetSpriteIndex_CachesLastLookup()
    {
        var cat = new VmSpriteCatalog();
        Assert.Equal(0, cat.GetSpriteIndex("tnt1", true));
        Assert.Equal(0, cat.GetSpriteIndex("tnt1", false));
    }

    [Fact]
    public void AltHud_DefaultsForHeretic()
    {
        var (health, berserk) = VmAltHudLoader.GetDefaultHudPics(VmGameTypeHud.Heretic);
        Assert.Equal("ARTIPTN2", health);
        Assert.Equal(-1, berserk);
    }

    [Fact]
    public void DamageFactor_AppliesSpecificType()
    {
        var factors = new Dictionary<string, double> { ["Fire"] = 2 };
        Assert.Equal(20, VmDamageTypeDefinitionRuntime.ApplyActorFactors(factors, "Fire", 10));
    }

    [Fact]
    public void Summon_BuildsExtendedMessage()
    {
        var msg = VmSummonCommandBuilder.BuildSummonMessage("ZombieMan", 90, 5);
        Assert.NotNull(msg);
        Assert.Equal(0x02, msg![0]);
    }

}
