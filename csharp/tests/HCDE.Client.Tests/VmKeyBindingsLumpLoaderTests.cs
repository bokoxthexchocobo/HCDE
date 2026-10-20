namespace HCDE.Client.Tests;

public class VmKeyBindingsLumpLoaderTests
{
    [Fact]
    public void Load_ParsesDoubleAndMapBind()
    {
        var console = new VmKeyBindConsole();
        VmKeyBindingsLumpLoader.Load(console, """
            doublebind f4 "dbl"
            mapbind f5 "map"
            bind f6 "norm"
            """, true);
        var f4 = VmKeyNames.GetKeyFromName("f4");
        Assert.Equal("dbl", console.DoubleBindings.GetBinding(f4));
        Assert.Equal("map", console.AutomapBindings.GetBinding(VmKeyNames.GetKeyFromName("f5")));
        Assert.Equal("norm", console.Bindings.GetBinding(VmKeyNames.GetKeyFromName("f6")));
    }
}
