namespace HCDE.Gamedata.Tests;

public class VmActorClassRegistryTests
{
    [Fact]
    public void EnsureDefaultSprites_SeedsPlaceholderSprites()
    {
        var cat = new VmSpriteCatalog();
        VmActorClassRegistry.EnsureDefaultSprites(cat);
        Assert.Equal(0, cat.GetSpriteIndex("TNT1", false));
        Assert.Equal(1, cat.GetSpriteIndex("----", false));
        Assert.Equal(2, cat.GetSpriteIndex("####", false));
    }

    [Fact]
    public void TrySetReplacement_RequiresKnownActor()
    {
        var replacees = new Dictionary<string, string>();
        var known = new HashSet<string> { "DoomImp" };
        Assert.False(VmActorClassRegistry.TrySetReplacement(replacees, "Custom", "Unknown", known));
        Assert.True(VmActorClassRegistry.TrySetReplacement(replacees, "Custom", "DoomImp", known));
        Assert.Equal("DoomImp", replacees["Custom"]);
    }
}
