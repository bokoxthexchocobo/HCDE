namespace HCDE.Client.Tests;

public class VmGameConfigModArchiveTests
{
    [Fact]
    public void DoModSetup_ReadsModSections()
    {
        var config = new VmGameConfigFile([]);
        config.LoadFromText("[Doom.Player.Mod]\nfoo=1\n");
        var sink = new Dictionary<string, string>();
        Assert.True(config.DoModSetup("Doom", sink));
        Assert.Equal("1", sink["foo"]);
    }

    [Fact]
    public void ArchiveGameData_WritesBindings()
    {
        var config = new VmConfigFile();
        var binds = new VmKeyBindings();
        binds.DoBind("F11", "jump");
        VmGameConfigArchive.ArchiveGameData(
            config, "Doom", modSetup: false, netgame: false, isHost: true,
            new Dictionary<string, string> { ["crosshair"] = "1" },
            binds, new VmKeyBindings(), new VmKeyBindings());
        Assert.True(config.SetSection("Doom.Bindings"));
        Assert.Equal("jump", config.GetValueForKey("F11"));
    }
}
