namespace HCDE.Gamedata.Tests;

public class VmTerrainSplashParserTests
{
    [Fact]
    public void ParseSplash_ModifyResetsDefaults()
    {
        var db = new VmTerrainDatabase();
        var errors = new List<string>();
        Assert.True(VmTerrainSplashParser.TryParseSplash("Water", modify: false, "{ sound splash1 }", db, errors), string.Join("; ", errors));
        Assert.True(VmTerrainSplashParser.TryParseSplash("Water", modify: true, "{ smallsound small1 noalert 1 }", db, errors));
        var splash = Assert.Single(db.Splashes);
        Assert.Equal("small1", splash.SmallSplashSound);
        Assert.True(splash.NoAlert);
        Assert.Equal("", splash.NormalSplashSound);
    }

    [Fact]
    public void Friction_MatchesNativeFormula()
    {
        VmTerrainFrictionParser.ApplyFrictionInput(1.0, out var friction, out var move);
        Assert.InRange(friction, 0.5, 1.0);
        Assert.True(move >= 32 / 65536.0);
    }
}
