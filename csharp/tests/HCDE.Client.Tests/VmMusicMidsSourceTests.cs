using System.Buffers.Binary;

namespace HCDE.Client.Tests;

public class VmMusicMidsSourceTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PlaybackModeAcceptsLiveFlagAndPreservesRecordsAndRestart(bool live)
    {
        using var source = new VmMusicMidsSource(Mids(0, [0, 42, 0x403c90, 4, 43, 0x403d90]));
        source.StartPlayback(live, 3);
        Assert.Equal(3, source.LoopLimit);
        uint[] first = new uint[6], restarted = new uint[6];
        Assert.Equal(6, source.MakeEvents(first, uint.MaxValue));
        Assert.Equal(new uint[] { 0, 42, 0x403c90, 4, 43, 0x403d90 }, first);
        Assert.True(source.CheckDone());
        Assert.Equal(0, source.MakeEvents(restarted, uint.MaxValue));
        source.Restart();
        Assert.Equal(6, source.MakeEvents(restarted, uint.MaxValue));
        Assert.Equal(first, restarted);
        Assert.Throws<ArgumentOutOfRangeException>(() => source.StartPlayback(live, -1));
        Assert.Equal(3, source.LoopLimit);
    }

    private static byte[] Mids(uint flags, params uint[][] blocks)
    {
        var size = 44 + blocks.Sum(x => 8 + x.Length * 4);
        var bytes = new byte[Math.Max(56, size)];
        "RIFF"u8.CopyTo(bytes); "MIDSfmt "u8.CopyTo(bytes.AsSpan(8));
        void Put(int offset, uint value) => BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset), value);
        Put(4, (uint)(bytes.Length - 8)); Put(16, 12); Put(20, 480); Put(28, flags);
        "data"u8.CopyTo(bytes.AsSpan(32)); Put(36, (uint)(bytes.Length - 40)); Put(40, (uint)blocks.Length);
        var position = 44;
        foreach (var block in blocks)
        {
            Put(position, 999); Put(position + 4, (uint)(block.Length * 4)); position += 8;
            foreach (var word in block) { Put(position, word); position += 4; }
        }
        return bytes;
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(1u)]
    [InlineData(7u)]
    public void BothRecordLayoutsAndMultipleBlocksExportInOrder(uint flags)
    {
        uint[] Record(uint delay, uint word) => flags == 0 ? [delay, 42, word] : [delay, word];
        var bytes = Mids(flags, Record(0, 0x010f4240), Record(3, 0x403c90), Record(5, 0x403d90));
        using var source = new VmMusicMidsSource(bytes);
        Array.Clear(bytes);
        var expected = VmMusicSmf.Serialize(480, 1000000,
            [new(0, 0x010f4240), new(3, 0x403c90), new(5, 0x403d90)]);
        Assert.Equal(expected, VmMusicSmfSource.Create(source, 1));
        Assert.Equal(expected, VmMusicSmfSource.Create(source, 1));
    }

    [Fact]
    public void EmptyBlocksExportSafelyAndDefaultTempoIsUsed()
    {
        using var source = new VmMusicMidsSource(Mids(0, []));
        Assert.Equal(VmMusicSmf.Serialize(480, 500000, []), VmMusicSmfSource.Create(source, 1));
    }

    [Fact]
    public void SmallOutputBufferDoesNotLoseEventsOrStreamIds()
    {
        using var source = new VmMusicMidsSource(Mids(0, [0, 42, 0x403c90, 4, 43, 0x403d90]));
        source.Restart();
        var words = new uint[4];
        Assert.Equal(3, source.MakeEvents(words, uint.MaxValue));
        Assert.Equal(new uint[] { 0, 42, 0x403c90 }, words[..3]);
        Assert.Equal(3, source.MakeEvents(words, uint.MaxValue));
        Assert.Equal(new uint[] { 4, 43, 0x403d90 }, words[..3]);
        Assert.True(source.CheckDone());
    }

    [Theory]
    [InlineData(16, 8u)]
    [InlineData(20, 0u)]
    [InlineData(36, 10000u)]
    [InlineData(40, 100u)]
    [InlineData(48, 9u)]
    [InlineData(48, 1000u)]
    public void MalformedHeaderAndBlockLengthsAreRejected(int offset, uint value)
    {
        var bytes = Mids(0, [0, 0, 0x403c90]);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset), value);
        Assert.Throws<InvalidDataException>(() => new VmMusicMidsSource(bytes));
    }

    [Fact]
    public void LongRecordsAndZeroTempoFailExplicitly()
    {
        Assert.Throws<InvalidDataException>(() => new VmMusicMidsSource(Mids(0, [0, 0, 0x80000001])));
        using var source = new VmMusicMidsSource(Mids(0, [0, 0, 0x01000000]));
        Assert.Throws<InvalidDataException>(() => VmMusicSmfSource.Create(source, 1));
        Assert.Equal(0, source.LoopLimit);
    }

    [Fact]
    public void ManagedResourceLoaderCreatesConcreteMidsSource()
    {
        using var source = VmMusicMidiSource.OpenManaged("song", "", _ => 0, _ => -1,
            _ => Mids(1, [0, 0x403c90]), _ => throw new Exception("unexpected report"));
        Assert.IsType<VmMusicMidsSource>(source);
        Assert.Equal(VmMusicSmf.Serialize(480, 500000, [new(0, 0x403c90)]), VmMusicSmfSource.Create(source!, 1));
    }
}
