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
    public void UncheckedAdvanceStreamNative_ThrowsPastEnd()
    {
        var off = 0;
        var len = 2;
        Assert.Throws<InvalidOperationException>(() =>
            VmIProtocolUnchecked.AdvanceStreamNative(ref off, ref len, 3));
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
        var buf = System.Text.Encoding.UTF8.GetBytes("ping\0tail");
        var stream = new ProtocolStream { Buffer = buf, Offset = 0, Length = buf.Length };
        Assert.Equal("ping", VmIProtocolChecked.ReadStringNative(ref stream));
        Assert.Equal(5, stream.Offset);
    }

    [Fact]
    public void ReadBytesNative_Checked()
    {
        var buf = new byte[] { 1, 2, 3, 9 };
        var stream = new ProtocolStream { Buffer = buf, Offset = 0, Length = buf.Length };
        var dst = new byte[3];
        VmIProtocolChecked.ReadBytesNative(dst, ref stream);
        Assert.Equal(new byte[] { 1, 2, 3 }, dst);
        Assert.Equal(3, stream.Offset);
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

    [Fact]
    public void WriteFStringNative_WritesPayload()
    {
        var buf = new byte[8];
        var stream = new ProtocolStream { Buffer = buf, Offset = 0, Length = buf.Length };
        VmIProtocolChecked.WriteFStringNative(new byte[] { 9, 8, 7 }, ref stream);
        Assert.Equal(new byte[] { 9, 8, 7, 0, 0, 0, 0, 0 }, buf);
        Assert.Equal(3, stream.Offset);
    }

    [Fact]
    public void FloatRoundTrip_Unchecked()
    {
        var buf = new byte[8];
        var off = 0;
        VmIProtocolUnchecked.UncheckedWriteFloatNative(3.5f, ref off, buf);
        off = 0;
        Assert.Equal(3.5f, VmIProtocolUnchecked.UncheckedReadFloatNative(ref off, buf));
    }
}
