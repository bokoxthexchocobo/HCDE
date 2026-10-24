namespace HCDE.Net.Core.Tests;

public class VmIProtocolStreamTests
{
    [Fact]
    public void UncheckedReadStringNative_NullTerminated()
    {
        var buf = System.Text.Encoding.UTF8.GetBytes("ok\0rest");
        var off = 0;
        Assert.Equal("ok", VmIProtocolUnchecked.UncheckedReadStringNative(ref off, buf));
        Assert.Equal(3, off);
    }

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

    [Fact]
    public void ReadStringNative_Checked()
    {
        var buf = System.Text.Encoding.UTF8.GetBytes("ping\0");
        var stream = new ProtocolStream { Buffer = buf, Offset = 0, Length = buf.Length };
        Assert.Equal("ping", VmIProtocolChecked.ReadStringNative(stream));
    }

    [Fact]
    public void WriteInt16Native_Checked()
    {
        var buf = new byte[4];
        var stream = new ProtocolStream { Buffer = buf, Offset = 0, Length = buf.Length };
        VmIProtocolChecked.WriteInt16Native(0x102, ref stream);
        Assert.Equal(0x01, buf[0]);
        Assert.Equal(0x02, buf[1]);
    }
}
