namespace HCDE.MapLoader.Tests;

public class UdmfGravityTests
{
    [Theory]
    [InlineData("ZDoom", true)]
    [InlineData("ZDoomTranslated", true)]
    [InlineData("Vavoom", true)]
    [InlineData("Doom", false)]
    [InlineData("Hexen", false)]
    [InlineData("Strife", false)]
    public void SectorGravityRespectsNativeNamespace(string ns, bool enabled)
    {
        var text = $$"""namespace = "{{ns}}"; sector { gravity = 0.25; }""";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        Assert.Equal(enabled ? 0.25 : 1, Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Sectors).Gravity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.5)]
    [InlineData(0.125)]
    [InlineData(2.5)]
    public void ImportsGravityWithoutClamping(double gravity)
    {
        var text = FormattableString.Invariant($"namespace = \"ZDoom\"; sector {{ gravity = {gravity}; }}");
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        Assert.Equal(gravity, Assert.Single(map.Sectors).Gravity);
        Assert.Equal(gravity, Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Sectors).Gravity);
    }

    [Fact]
    public void MissingGravityDefaultsToOne()
    {
        Assert.True(UdmfTextMapParser.TryParse("namespace = \"ZDoom\"; sector {}", out var map, out var error), error);
        Assert.Equal(1, Assert.Single(map.Sectors).Gravity);
        Assert.Equal(1, Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Sectors).Gravity);
    }
}