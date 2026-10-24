namespace HCDE.Client.Tests;

public class VmKeyBindConsoleTests
{
    [Fact]
    public void Stacks_AreIndependent()
    {
        var console = new VmKeyBindConsole();
        console.Bindings.DoBind("f1", "a");
        console.DoubleBindings.DoBind("f1", "b");
        console.AutomapBindings.DoBind("f1", "c");
        Assert.Equal("a", console.Bindings.GetBinding(VmKeyNames.GetKeyFromName("f1")));
        Assert.Equal("b", console.DoubleBindings.GetBinding(VmKeyNames.GetKeyFromName("f1")));
        console.PerformMapBind(["mapbind", "f1"], _ => { });
        Assert.Equal("c", console.AutomapBindings.GetBinding(VmKeyNames.GetKeyFromName("f1")));
    }

    [Fact]
    public void UnbindAllStacks_ClearsEach()
    {
        var console = new VmKeyBindConsole();
        console.Bindings.DoBind("f2", "x");
        console.DoubleBindings.DoBind("f2", "y");
        console.UnbindAllStacks();
        Assert.Equal("", console.Bindings.GetBinding(VmKeyNames.GetKeyFromName("f2")));
        Assert.Equal("", console.DoubleBindings.GetBinding(VmKeyNames.GetKeyFromName("f2")));
    }
}
