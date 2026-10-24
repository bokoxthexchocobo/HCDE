namespace HCDE.Client.Tests;

public class VmDNetTrySkipCommandTests
{
    [Fact]
    public void TryReadCStringBytes_FindsTerminator()
    {
        var data = new byte[] { (byte)'a', 0, 9 };
        Assert.True(VmDNetTrySkipCommand.TryReadCStringBytes(3, 0, data, out var n));
        Assert.Equal(2, n);
    }

    [Fact]
    public void TrySkipZscCmd_AddsBigEndianLength()
    {
        var stream = new byte[] { (byte)'x', 0, 0, 2, 0, 9, 9 };
        Assert.True(VmDNetTrySkipCommand.TrySkipZscCmd(7, 2, stream, out var skip));
        Assert.Equal(6, skip);
    }

    [Fact]
    public void CanAdvance_RejectsOverflow() =>
        Assert.False(VmDNetTrySkipCommand.CanAdvance(5, 4));
}
