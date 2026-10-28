namespace HCDE.Client.Tests;

public class VmKeyBindQueueTests
{
    [Fact]
    public void DoubleClick_UsesDoubleBinding()
    {
        var time = 1000u;
        var queue = new VmKeyBindQueue(VmKeyNames.NumKeys, () => time) { DoubleClickThresholdMs = 200 };
        var main = new VmKeyBindings();
        var dbl = new VmKeyBindings();
        var key = VmKeyNames.GetKeyFromName("f3");
        main.SetBind(key, "+move");
        dbl.SetBind(key, "jump");
        Assert.True(queue.TryHandleKey(key, false, main, dbl, out var first));
        Assert.Null(first);
        time = 1100;
        Assert.True(queue.TryHandleKey(key, false, main, dbl, out var second));
        Assert.NotNull(second);
        Assert.True(second!.Value.IsDoubleClick);
        Assert.Equal("jump", second.Value.Binding);
    }
}
