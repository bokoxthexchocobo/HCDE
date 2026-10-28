namespace HCDE.Net.Core.Tests;

public class VmHcdeServerModeTests
{
    [Fact]
    public void HcdeServerModeInitFromArgsNative_ParsesServerFlag()
    {
        var server = false;
        VmHcdeServerMode.HcdeServerModeInitFromArgsNative(new HcdeServerModeStateContext
        {
            Runtime = new HcdeServerModeRuntimeContext { NumArgs = () => 2 },
            CheckParm = s => s == "-server" ? 1 : 0,
            SetInitialized = _ => { },
            SetDedicatedServer = v => server = v,
            SetDedicatedExecutable = _ => { },
        });
        Assert.True(server);
    }

    [Fact]
    public void HcdeServerModeShouldSuppressRoomUiNative_DedicatedServer()
    {
        var ctx = new HcdeServerModeStateContext { DedicatedServer = true };
        Assert.True(VmHcdeServerModeAccessors.HcdeServerModeShouldSuppressRoomUiNative(ctx));
    }
}
