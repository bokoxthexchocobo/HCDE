namespace HCDE.MapLoader.Tests;

public class UdmfNoFallingDamageTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SectorFlagLoadsAndCopies(bool value)
    {
        var text = "namespace = \"ZDoom\"; sector { nofallingdamage = " + (value ? "true" : "false") + "; }";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        var level = LevelBuilder.FromUdmf(map, "MAP01");
        Assert.Equal(value, level.Sectors.Single().NoFallingDamage);
        Assert.Equal(value, level.CopyForSimulation().Sectors.Single().NoFallingDamage);
    }
}
