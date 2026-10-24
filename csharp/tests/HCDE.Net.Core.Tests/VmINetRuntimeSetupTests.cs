namespace HCDE.Net.Core.Tests;

public class VmINetRuntimeSetupTests
{
    [Fact]
    public void DropClientForHcdeTimeoutNative_SkipsReadyClient()
    {
        var players = 2;
        int? connected = players;
        var dropped = VmINetRuntimeSetup.DropClientForHcdeTimeoutNative(
            1, 8,
            _ => 3,
            0,
            _ => true,
            _ => true,
            _ => (0, 0, 0, 0, 0ul),
            _ => 0,
            1000,
            5000,
            60000,
            null,
            null,
            null,
            ref connected,
            null,
            8);
        Assert.False(dropped);
    }
}
