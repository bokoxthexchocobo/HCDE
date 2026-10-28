namespace HCDE.Client.Tests;

public class VmGameConfigArchiveRegistryTests
{
    [Fact]
    public void ArchiveGameDataFromRegistry_WritesFilteredCvars()
    {
        var reg = new VmCvarRegistry();
        reg.Register(new VmCvarEntry("crosshair", VmCvarFlags.Archive | VmCvarFlags.UserInfo, "1"));
        var config = new VmConfigFile();
        VmGameConfigArchive.ArchiveGameDataFromRegistry(
            config, "Doom", false, false, true, reg,
            new VmKeyBindings(), new VmKeyBindings(), new VmKeyBindings());
        Assert.True(config.SetSection("Doom.Player"));
        Assert.Equal("1", config.GetValueForKey("crosshair"));
    }
}
