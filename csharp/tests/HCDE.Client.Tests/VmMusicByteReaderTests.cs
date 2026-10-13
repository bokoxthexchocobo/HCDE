namespace HCDE.Client.Tests;

public class VmMusicByteReaderTests
{
    [Fact]
    public void GetsSkipsCarriageReturnsIncludesLinefeedAndConsumesNulSeparators()
    {
        using var reader = new VmMusicByteReader("a\r\nb\0c\nend"u8);
        var output = new byte[8];
        Assert.Equal(2, reader.Gets(output));
        Assert.Equal(new byte[] { (byte)'a', 10, 0 }, output.Take(3));
        Assert.Equal(3, reader.Tell());
        Assert.Equal(1, reader.Gets(output));
        Assert.Equal(0, output[1]);
        Assert.Equal(5, reader.Tell());
        Assert.Equal(2, reader.Gets(output));
        Assert.Equal((byte)'c', output[0]);
    }

    [Fact]
    public void GetsPreservesNativeBudgetAndSafelyStopsAtEndAfterCarriageReturns()
    {
        using var reader = new VmMusicByteReader("abc"u8);
        var output = new byte[8];
        Assert.Equal(2, reader.Gets(output));
        Assert.Equal(2, reader.Tell());
        Assert.Null(reader.Gets(output));
        Assert.Equal(2, reader.Tell());
        Assert.Equal(1, reader.Read(output));
        Assert.Null(reader.Gets(output));
        using var carriageReturns = new VmMusicByteReader("\r\r"u8);
        Assert.Null(carriageReturns.Gets(output));
        Assert.Equal(2, carriageReturns.Tell());
        reader.Dispose();
        Assert.Throws<ObjectDisposedException>(() => reader.Gets(output));
    }

    [Fact]
    public void GetsSmallBuffersDoNotAdvanceAndTruncatedLinesCanResume()
    {
        using var reader = new VmMusicByteReader("abcd\n"u8);
        Assert.Null(reader.Gets(new byte[1]));
        Assert.Equal(0, reader.Tell());
        var output = new byte[3];
        Assert.Equal(2, reader.Gets(output));
        Assert.Equal(new byte[] { (byte)'a', (byte)'b', 0 }, output);
        Assert.Equal(2, reader.Gets(output));
        Assert.Equal(new byte[] { (byte)'c', (byte)'d', 0 }, output);
    }

    [Fact]
    public void BinaryReadsAndSeeksPreserveEofAndFailedSeekPosition()
    {
        byte[] source = [0, 255, 17];
        using var reader = new VmMusicByteReader(source);
        source[0] = 42;
        var output = new byte[4];
        Assert.Equal(3, reader.Read(output));
        Assert.Equal(new byte[] { 0, 255, 17, 0 }, output);
        Assert.Equal(0, reader.Read(output));
        Assert.Equal(3, reader.Tell());
        Assert.Equal(-1, reader.Seek(long.MaxValue, SeekOrigin.Current));
        Assert.Equal(3, reader.Tell());
        Assert.Equal(0, reader.Seek(-2, SeekOrigin.End));
        Assert.Equal(1, reader.Tell());
        Assert.Equal(0, reader.Seek(-1, SeekOrigin.Current));
        Assert.Equal(0, reader.Tell());
        Assert.Equal(-1, reader.Seek(-1, SeekOrigin.Begin));
        Assert.Equal(0, reader.Tell());
    }

    [Fact]
    public void CallbackReaderOwnsIndependentLifetimeAndRejectsClosedAccess()
    {
        var callbacks = new VmMusicCallbacks([], _ => 0, _ => [17]);
        using var handle = callbacks.OpenBank("bank.cfg", VmMusicBankFormat.Gus)!;
        using var reader = handle.OpenByteReader(null)!;
        handle.Dispose();
        Assert.Equal(1, reader.Read(new byte[1]));
        reader.Dispose();
        Assert.Throws<ObjectDisposedException>(() => reader.Tell());
        Assert.Throws<ObjectDisposedException>(() => reader.Seek(0, SeekOrigin.Begin));
        Assert.Throws<ObjectDisposedException>(() => reader.Read(new byte[1]));
    }
}
