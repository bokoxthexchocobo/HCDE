namespace HCDE.MapLoader.Tests;

public class UdmfPassUseTests
{
    [Theory]
    [InlineData(true, true, false, true)]
    [InlineData(true, false, true, false)]
    [InlineData(false, true, false, false)]
    [InlineData(false, false, false, false)]
    public void PassUseReplacesOrdinaryUseOnlyWhenEnabled(bool use, bool pass, bool expectedUse, bool expectedThrough)
    {
        var text = "namespace = \"ZDoom\"; vertex { x = 0; y = -64; } vertex { x = 0; y = 64; } "
            + "sector { heightceiling = 128; } sidedef { sector = 0; } "
            + "linedef { v1 = 0; v2 = 1; sidefront = 0; playeruseback = true; playercross = true; "
            + "playeruse = " + use.ToString().ToLowerInvariant() + "; passuse = " + pass.ToString().ToLowerInvariant() + "; }";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        var line = Assert.Single(LevelBuilder.FromUdmf(map!, "MAP01").Lines);
        Assert.Equal(expectedUse, line.PlayerUse);
        Assert.Equal(expectedThrough, line.UseThrough);
        Assert.True(line.PlayerUseBack);
        Assert.True(line.PlayerCross);
    }
}
