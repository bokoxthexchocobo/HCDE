namespace HCDE.Client.Tests;

public class VmKeyBindingsTests
{
    [Fact]
    public void DoBindAndArchive_RoundTrip()
    {
        var config = new VmConfigFile();
        config.SetSection("Doom.Bindings", allowCreate: true);
        var binds = new VmKeyBindings();
        binds.DoBind("F11", "jump");
        binds.ArchiveBindings(config);
        Assert.Equal("jump", config.GetValueForKey("F11"));
    }

    [Fact]
    public void DefaultBind_SkipsWhenKeyUsed()
    {
        var binds = new VmKeyBindings();
        binds.DoBind("F11", "a");
        binds.DefaultBind("F11", "b");
        Assert.Equal("a", binds.GetBind(0x57));
    }
}
