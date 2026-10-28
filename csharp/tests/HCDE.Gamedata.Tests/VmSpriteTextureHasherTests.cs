namespace HCDE.Gamedata.Tests;

public class VmSpriteTextureHasherTests
{
    [Fact]
    public void BuildSpriteTextureChains_LinksSamePrefix()
    {
        var textures = new List<(int, string, bool)>
        {
            (0, "TROOA1", true),
            (1, "PLAYA1", true),
            (2, "TROOB1", true)
        };
        var next = new int[3];
        var head = new int[3];
        for (var i = 0; i < 3; i++) head[i] = -1;
        VmSpriteTextureHasher.BuildSpriteTextureChains(textures, next, head);
        Assert.NotEqual(-1, head[VmSpriteTextureHasher.SpriteNameId("TROO") % 3]);
    }
}
