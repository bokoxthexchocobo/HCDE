using System.Text;

namespace HCDE.Playsim.Tests;

public class VmMd51209SliceTests
{
    static string Hex(byte[] data)
    {
        var built = new StringBuilder(data.Length * 2);
        foreach (byte b in data)
            built.Append(b.ToString("x2"));
        return built.ToString();
    }

    static string Digest(byte[] data)
    {
        var ctx = new VmMd51209Slice();
        ctx.Update(data);
        return Hex(ctx.Final());
    }

    [Fact]
    public void Final_MatchesTheRfcVectorsAndClearsTheContext()
    {
        Assert.Equal("d41d8cd98f00b204e9800998ecf8427e", Digest(Array.Empty<byte>()));
        Assert.Equal("0cc175b9c0f1b6a831c399e269772661", Digest(Encoding.ASCII.GetBytes("a")));
        Assert.Equal("900150983cd24fb0d6963f7d28e17f72", Digest(Encoding.ASCII.GetBytes("abc")));
        Assert.Equal("f96b697d7cb7938d525a2f31aaf161d0", Digest(Encoding.ASCII.GetBytes("message digest")));
        Assert.Equal("c3fcd3d76192e4007dfb496cca67e13b", Digest(Encoding.ASCII.GetBytes("abcdefghijklmnopqrstuvwxyz")));
        Assert.Equal("668a72d5ba17f08e62dabcafad6db14b", Digest(Encoding.ASCII.GetBytes(new string('x', 56))));
        Assert.Equal("b0c4864f4e28d2d76929c38d87db7ed5", Digest(Encoding.ASCII.GetBytes(new string('y', 64))));
        Assert.Equal("6ae46a90f7abe9dfd5f132c2fb17979d", Digest(Encoding.ASCII.GetBytes(new string('z', 100))));

        var wiped = new VmMd51209Slice();
        Assert.Equal("d41d8cd98f00b204e9800998ecf8427e", Hex(wiped.Final()));
        Assert.Equal("b0e641c998cc3eae6fa2f8726d98cddd", Hex(wiped.Final()));
        Assert.Throws<NullReferenceException>(() => wiped.Update(null, 1));
    }

    [Fact]
    public void Update_ConcatenatesChunksAndAZeroLengthCall()
    {
        var text = Encoding.ASCII.GetBytes("message digest");
        var ctx = new VmMd51209Slice();
        ctx.Update(null, 0);
        ctx.Update(text, 4);
        ctx.Update(text.AsSpan(4).ToArray());
        Assert.Equal("f96b697d7cb7938d525a2f31aaf161d0", Hex(ctx.Final()));
    }
}
