namespace HCDE.Gamedata.Tests;

public class VmGameInfoStaticSchemaTests
{
    [Fact]
    public void GameNames_IncludesDoom()
    {
        Assert.Equal("Doom", VmGameInfoStaticSchema.GameNames[1]);
    }

    [Fact]
    public void ResolveBorderPreset_HereticUsesFourBySixteen()
    {
        var b = VmGameInfoStaticSchema.ResolveBorderPreset("HereticBorder");
        Assert.Equal(4, b.Size);
        Assert.Equal(16, b.Offset);
    }
}
