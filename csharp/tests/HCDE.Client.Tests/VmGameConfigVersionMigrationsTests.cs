namespace HCDE.Client.Tests;

public class VmGameConfigVersionMigrationsTests
{
    [Fact]
    public void MapLegacyHqResize_MapsKnownValues()
    {
        Assert.Equal((4, 5), VmGameConfigVersionMigrations.MapLegacyHqResize(18));
        Assert.Equal((0, 1), VmGameConfigVersionMigrations.MapLegacyHqResize(99));
    }

    [Fact]
    public void Apply_MigratesChannelsAndHqResize()
    {
        var cvars = new Dictionary<string, string>
        {
            ["snd_channels"] = "32",
            ["gl_texture_hqresize"] = "5"
        };
        VmGameConfigVersionMigrations.Apply(212, cvars);
        Assert.False(cvars.ContainsKey("snd_channels"));
        Assert.Equal("2", cvars["gl_texture_hqresizemode"]);
        Assert.Equal("3", cvars["gl_texture_hqresizemult"]);
    }

    [Fact]
    public void Apply_FlipsPauseWhenSoundInBackground()
    {
        var cvars = new Dictionary<string, string> { ["i_soundinbackground"] = "true" };
        VmGameConfigVersionMigrations.Apply(222, cvars);
        Assert.Equal("False", cvars["i_pauseinbackground"]);
    }
}
