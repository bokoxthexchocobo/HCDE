using System.Buffers.Binary;

namespace HCDE.Client.Tests;

public class VmMusicMusPrecacheTests
{
    private static byte[] Mus(ushort count, byte[] table)
    {
        var bytes = new byte[17 + table.Length];
        "MUS\u001a"u8.CopyTo(bytes);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(4), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(6), (ushort)(16 + table.Length));
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(12), count);
        table.CopyTo(bytes, 16); bytes[^1] = 0x60;
        return bytes;
    }

    [Fact]
    public void NativeTableMapsMelodicBanksPercussionAndPreservesDuplicates()
    {
        using var source = new VmMusicMusSource(Mus(5, [5, 0, 6, 2, 1, 2, 135, 0, 188, 1, 3, 5, 0]));
        Assert.Equal(new ushort[] { 5, 134, 262, 16419, 16856, 5 }, source.PrecacheData());
    }

    [Theory]
    [InlineData(128)]
    [InlineData(134)]
    [InlineData(189)]
    [InlineData(255)]
    public void UnsupportedInstrumentsSkipTheirBanksAndContinue(byte instrument)
    {
        using var source = new VmMusicMusSource(Mus(2, [instrument, 2, 7, 8, 10, 0]));
        Assert.Equal(new ushort[] { 10 }, source.PrecacheData());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void TruncatedTableCannotReadIntoScoreBytes(int available)
    {
        var table = new byte[] { 5, 2, 1 }[..available];
        using var source = new VmMusicMusSource(Mus(1, table));
        Assert.Throws<InvalidDataException>(() => source.PrecacheData());
    }

    [Fact]
    public void PrecacheOwnsSnapshotAndDoesNotAdvancePlayback()
    {
        var bytes = Mus(1, [7, 0]);
        using var source = new VmMusicMusSource(bytes);
        Array.Clear(bytes);
        Assert.False(source.CheckDone());
        Assert.Equal(source.PrecacheData(), source.PrecacheData());
        Assert.Equal(new ushort[] { 7 }, source.PrecacheData());
        Assert.Equal(VmMusicSmf.Serialize(140, 1000000, []), VmMusicSmfSource.Create(source, 1));
    }

    [Fact]
    public void EmptyTableAndDisposedAccessAreExplicit()
    {
        var source = new VmMusicMusSource(Mus(0, []));
        Assert.Empty(source.PrecacheData());
        source.Dispose();
        Assert.Throws<ObjectDisposedException>(() => source.PrecacheData());
    }
}
