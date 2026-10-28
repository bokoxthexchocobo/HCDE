namespace HCDE.Client.Tests;

public class VmKeyBindingsExtendedTests
{
    [Fact]
    public void GetKeysForCommand_FindsTwoKeys()
    {
        var binds = new VmKeyBindings();
        binds.DoBind("F11", "jump");
        binds.DoBind("F12", "jump");
        var count = binds.GetKeysForCommand("jump", out var first, out var second);
        Assert.Equal(2, count);
        Assert.Equal(VmKeyNames.GetKeyFromName("F11"), first);
        Assert.Equal(VmKeyNames.GetKeyFromName("F12"), second);
    }
}
