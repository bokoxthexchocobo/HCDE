namespace HCDE.Client.Tests;

public class VmGameConfigKeySetupTests
{
    [Fact]
    public void DoKeySetup_LoadsBindingsAndAppliesGammaReset()
    {
        var config = new VmGameConfigFile([]);
        config.LoadFromText("[Doom.Bindings]\nF11=bumpgamma\n");
        var flags = VmGameConfigResetBindFlags.V230Gamma;
        var binds = new VmKeyBindings();
        VmGameConfigKeySetup.DoKeySetup(config, "Doom", binds, new VmKeyBindings(), new VmKeyBindings(), ref flags);
        Assert.Equal(0, flags);
        Assert.Equal("bumplight", binds.GetBind(0x57));
    }
}
