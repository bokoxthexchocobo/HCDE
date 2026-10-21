namespace HCDE.Net.Core.Tests;

public class VmIProtocolStreamTests
{
    [Fact]
    public void UncheckedReadInt16Native_BigEndian()
    {
        var buf = new byte[] { 0, 0x2A, 0xFF, 0 };
        var off = 0;
        Assert.Equal(42, VmIProtocolUnchecked.UncheckedReadInt16Native(ref off, buf));
    }

    [Fact]
    public void ReadInt32Native_Checked()
    {
        var buf = new byte[] { 0, 0, 0, 7, 0 };
        var stream = new ProtocolStream { Buffer = buf, Offset = 0, Length = buf.Length };
        Assert.Equal(7, VmIProtocolChecked.ReadInt32Native(ref stream));
    }
}
