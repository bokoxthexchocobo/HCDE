namespace HCDE.Client.Tests;

public class VmGameConfigExtendedTests
{
    [Fact]
    public void ReadCvars_LoadsGlobalSettings()
    {
        var config = new VmGameConfigFile([]);
        config.LoadFromText("[GlobalSettings]\nfoo=bar\n");
        var sink = new Dictionary<string, string>();
        config.DoGlobalSetup(sink, "5.0");
        Assert.Equal("bar", sink["foo"]);
    }

    [Fact]
    public void PurgeWeaponSlots_RemovesLegacySections()
    {
        var config = new VmGameConfigFile([]);
        config.LoadFromText("[Doom.WeaponSlots]\na=1\n[General]\nb=2\n");
        VmGameConfigGlobalSetup.PurgeWeaponSlotSections(config, 207);
        Assert.False(config.SetSection("Doom.WeaponSlots"));
        Assert.True(config.SetSection("General"));
    }

    [Fact]
    public void DoAutoloadSetup_RenamesLegacySection()
    {
        var config = new VmGameConfigFile([]);
        config.LoadFromText("[LastRun]\nVersion=210\n[Doom2.Autoload]\n");
        config.DoAutoloadSetup([]);
        Assert.True(config.SetSection("doom.id.doom2.commercial.Autoload"));
    }

    [Fact]
    public void AddAutoexec_CollectsExistingPaths()
    {
        var config = new VmGameConfigFile([]);
        config.SetSection("Doom.AutoExec", allowCreate: true);
        config.SetValueForKey("Path", "autoexec.cfg");
        var paths = config.AddAutoexec("Doom", path => path == "autoexec.cfg");
        Assert.Single(paths);
    }
}
