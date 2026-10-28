using HCDE.Gamedata;

namespace HCDE.Client.Tests;

public class VmKeyConfIniLoaderTests
{
    [Fact]
    public void LoadKeys_BindsFromGameConfigSection()
    {
        var config = new VmConfigFile();
        config.SetSection("Doom.weapons.Bindings", allowCreate: true);
        config.SetValueForKey("a", "invuse");
        var binds = new VmKeyBindings();
        VmKeyConfIniLoader.LoadKeys("Doom", "weapons", false, config, binds, new VmKeyBindings());
        Assert.Equal("invuse", binds.GetBind("a"));
    }

    [Fact]
    public void LoadKeysForSection_LoadsSingleAndDouble()
    {
        var config = new VmConfigFile();
        config.SetSection("Doom.keys.Bindings", allowCreate: true);
        config.SetValueForKey("b", "cmd1");
        config.SetSection("Doom.keys.DoubleBindings", allowCreate: true);
        config.SetValueForKey("c", "cmd2");
        var single = new VmKeyBindings();
        var dbl = new VmKeyBindings();
        var section = new VmKeyConfSection { IniName = "keys" };
        VmKeyConfIniLoader.LoadKeysForSection("Doom", section, config, single, dbl);
        Assert.Equal("cmd1", single.GetBind("b"));
        Assert.Equal("cmd2", dbl.GetBind("c"));
    }
}
