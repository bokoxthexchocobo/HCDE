namespace HCDE.Client.Tests;

public class VmGameConfigGameSetupTests
{
    [Fact]
    public void ReadConsoleAliases_PairsNameAndCommand()
    {
        var config = new VmGameConfigFile([]);
        config.LoadFromText("""
            [Doom.ConsoleAliases]
            Name=foo
            Command=bar
            Name=baz
            Command=qux
            """);
        var aliases = VmGameConfigGameSetup.ReadConsoleAliases(config, "Doom");
        Assert.Equal("bar", aliases["foo"]);
        Assert.Equal("qux", aliases["baz"]);
    }

    [Fact]
    public void DoGameSetup_LoadsPlayerSection()
    {
        var config = new VmGameConfigFile([]);
        config.LoadFromText("[LastRun]\nVersion=232\n[Doom.Player]\ncrosshair=1\n");
        var sink = new Dictionary<string, string>();
        config.DoGameSetup("Doom", sink);
        Assert.Equal("1", sink["crosshair"]);
        Assert.False(sink.ContainsKey("uiscale"));
    }

    [Fact]
    public void ApplyHcdeUiBaseline_RunsBelow232()
    {
        var sink = new Dictionary<string, string>();
        VmGameConfigGameSetup.ApplyHcdeUiBaseline(231, sink);
        Assert.Equal("1", sink["uiscale"]);
    }
}
