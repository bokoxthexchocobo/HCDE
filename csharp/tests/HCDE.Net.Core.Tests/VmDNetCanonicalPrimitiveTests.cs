namespace HCDE.Net.Core.Tests;

public class VmDNetCanonicalPrimitiveTests
{
    [Fact]
    public void TryAppendCanonicalString_RoundTrip()
    {
        Span<byte> output = stackalloc byte[16];
        var cursor = 0;
        Assert.True(VmDNetCanonicalPrimitive.TryAppendCanonicalString(output, ref cursor, "hi"u8));
        Assert.Equal(4, cursor);
    }

    [Fact]
    public void TryAppendCanonicalNullString_ReadsTerminator()
    {
        var data = new byte[] { (byte)'x', 0, 99 };
        var inputCursor = 0;
        Span<byte> output = stackalloc byte[16];
        var outputCursor = 0;
        Assert.True(VmDNetCanonicalPrimitive.TryAppendCanonicalNullString(data, ref inputCursor, output, ref outputCursor));
        Assert.Equal(2, inputCursor);
    }

    [Fact]
    public void TryAppendCanonicalWeaponIndex_TwoByteIndex()
    {
        var data = new byte[] { 0x81, 0x05 };
        var inputCursor = 0;
        Span<byte> output = stackalloc byte[8];
        var outputCursor = 0;
        Assert.True(VmDNetCanonicalPrimitive.TryAppendCanonicalWeaponIndex(data, ref inputCursor, output, ref outputCursor));
        Assert.Equal(2, outputCursor);
    }
}
