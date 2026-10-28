namespace HCDE.Client.Tests;

public class VmKeySectionArchiveTests
{
    [Fact]
    public void SaveCustomKeys_ArchivesActionBindings()
    {
        var config = new VmConfigFile();
        var binds = new VmKeyBindings();
        binds.DoBind("F11", "invuse");
        var sections = new[] { new VmKeySectionArchive.KeySection("weapons", ["invuse"]) };
        VmKeySectionArchive.SaveCustomKeys(config, "Doom", sections, binds, new VmKeyBindings());
        Assert.True(config.SetSection("Doom.weapons.Bindings"));
        Assert.Equal("invuse", config.GetValueForKey("F11"));
    }
}
