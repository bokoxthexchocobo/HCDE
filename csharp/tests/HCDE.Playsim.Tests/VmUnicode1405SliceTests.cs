using S = HCDE.Playsim.VmUnicode1405Slice;

namespace HCDE.Playsim.Tests;

public class VmUnicode1405SliceTests
{
    [Fact]
    public void Utf16AndIbm437AppendATerminatorAndKeepAnUnpairedTail()
    {
        var buffer = new List<byte> { 9 };
        S.Utf16ToUtf8(new ushort[] { 0 }, buffer);
        Assert.Empty(buffer);
        S.Utf16ToUtf8(new ushort[] { 0x41, 0xD800, 0xDC00, 0xDC00 }, buffer);
        Assert.Equal(new byte[] { 0x41, 0xF0, 0x90, 0x80, 0x80, 0xEF, 0xBF, 0xBD, 0 }, buffer);
        S.Utf16ToUtf8(new ushort[] { 0xD800, 0x41 }, buffer);
        Assert.Equal(new byte[] { 0xEF, 0xBF, 0xBD, 0x41, 0 }, buffer);
        S.Ibm437ToUtf8(new byte[] { 0 }, buffer);
        Assert.Empty(buffer);
        S.Ibm437ToUtf8(new byte[] { 0x41, 0x80, 0xFF }, buffer);
        Assert.Equal(new byte[] { 0x41, 0xC3, 0x87, 0xC2, 0xA0, 0 }, buffer);
        var encoded = new List<byte>();
        S.Encode(0xDFFF, encoded);
        Assert.Equal(new byte[] { 0xED, 0xBF, 0xBF }, encoded);
        encoded.Clear();
        S.Encode(0xD800, encoded);
        Assert.Equal(new byte[] { 0xEF, 0xBF, 0xBD }, encoded);
        encoded.Clear();
        S.Encode(-1, encoded);
        Assert.Equal(new byte[] { 0xEF, 0xBF, 0xBD }, encoded);
        Assert.Equal(128, S.Ibm437.Length);
    }

    [Fact]
    public void FoldPassesTheCaseFoldFlagsAndValidateRepeatsALiveByte()
    {
        var source = new byte[] { (byte)'A', 0 };
        int options = 0;
        byte[] folded = S.Fold(source, (text, flags) =>
        {
            Assert.Same(source, text);
            options = flags;
            return new byte[] { (byte)'a', 0 };
        });
        Assert.Equal(S.FoldOptions, options);
        Assert.Equal(S.NullTerm | S.Stable | S.Compose | S.CaseFold, options);
        Assert.Equal(new byte[] { (byte)'a', 0 }, folded);
        Assert.True(S.Validate(new byte[] { 0 }, (_, _) => throw new InvalidOperationException("empty")));
        int calls = 0;
        Assert.False(S.Validate(new byte[] { 0x41 }, (text, index) =>
        {
            Assert.Equal(0, index);
            Assert.Equal(0x41, text[0]);
            calls++;
            return -1;
        }));
        Assert.Equal(1, calls);
        calls = 0;
        Assert.Throws<InvalidOperationException>(() => S.Validate(new byte[] { 0x41 }, (_, index) =>
        {
            Assert.Equal(0, index);
            calls++;
            if (calls == 2)
                throw new InvalidOperationException("repeat");
            return 1;
        }));
        Assert.Equal(2, calls);
        Assert.Throws<ArgumentNullException>(() => S.Fold(null!, (_, _) => Array.Empty<byte>()));
        Assert.Throws<ArgumentNullException>(() => S.Utf16ToUtf8(new ushort[] { 1 }, null!));
    }
}
