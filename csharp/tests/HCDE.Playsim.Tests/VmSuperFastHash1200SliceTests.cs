namespace HCDE.Playsim.Tests;

public class VmSuperFastHash1200SliceTests
{
    [Fact]
    public void Hash_MatchesNativeLengthsAndSignedTail()
    {
        Assert.Equal(0u, VmSuperFastHash1200Slice.Hash(null, 4));
        Assert.Equal(0u, VmSuperFastHash1200Slice.Hash(Array.Empty<byte>(), 0));
        Assert.Equal(2472816263u, VmSuperFastHash1200Slice.Hash("a"u8.ToArray(), 1));
        Assert.Equal(1535905475u, VmSuperFastHash1200Slice.Hash("ab"u8.ToArray(), 2));
        Assert.Equal(3843582778u, VmSuperFastHash1200Slice.Hash("abc"u8.ToArray(), 3));
        Assert.Equal(984896216u, VmSuperFastHash1200Slice.Hash("abcd"u8.ToArray(), 4));
        Assert.Equal(2222483234u, VmSuperFastHash1200Slice.Hash("abcde"u8.ToArray(), 5));
        var sample = "abcdefghijXYZ"u8.ToArray();
        uint[] expect =
        {
            3195826148, 1034764719, 4286381702, 2805871937, 763103107, 3300651084, 546122573, 904173424,
        };
        for (int n = 6; n <= 13; n++)
            Assert.Equal(expect[n - 6], VmSuperFastHash1200Slice.Hash(sample, n));
        Assert.Equal(2147133753u, VmSuperFastHash1200Slice.Hash(new byte[] { 0xFF }, 1));
        Assert.Equal(84345140u, VmSuperFastHash1200Slice.Hash(new byte[] { 0x01, 0x80, 0xFF }, 3));
        Assert.Equal(1863916929u, VmSuperFastHash1200Slice.Hash(new byte[] { 0x80, 0x7F, (byte)'A', 0xFF, (byte)'Z', 1 }, 6));
    }

    [Fact]
    public void HashI_FoldsAsciiAndKeepsHighBytes()
    {
        Assert.Equal(0u, VmSuperFastHash1200Slice.HashI(null, 1));
        Assert.Equal(0u, VmSuperFastHash1200Slice.HashI("abc"u8.ToArray(), 0));
        Assert.Equal(0u, VmSuperFastHash1200Slice.HashI("abc"u8.ToArray(), -1));
        Assert.Equal(1034764719u, VmSuperFastHash1200Slice.HashI("ABCDEFG"u8.ToArray(), 7));
        Assert.Equal(
            VmSuperFastHash1200Slice.Hash("abcdefg"u8.ToArray(), 7),
            VmSuperFastHash1200Slice.HashI("ABCDEFG"u8.ToArray(), 7));
        Assert.Equal(1672309298u, VmSuperFastHash1200Slice.Hash("ABCDEFG"u8.ToArray(), 7));
        var sample = "abcdefghijXYZ"u8.ToArray();
        Assert.Equal(3136509622u, VmSuperFastHash1200Slice.HashI(sample, 11));
        Assert.Equal(1563622466u, VmSuperFastHash1200Slice.HashI(sample, 12));
        Assert.Equal(335007023u, VmSuperFastHash1200Slice.HashI(sample, 13));
        Assert.Equal(2989192853u, VmSuperFastHash1200Slice.HashI(new byte[] { 0x80, 0x7F, (byte)'A', 0xFF, (byte)'Z', 1 }, 6));
        Assert.Equal(23152732u, VmSuperFastHash1200Slice.HashI("Message"u8.ToArray(), 7));
        Assert.Equal(3861256771u, VmSuperFastHash1200Slice.Hash("Message"u8.ToArray(), 7));
    }
}
