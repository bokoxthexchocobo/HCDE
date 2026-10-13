using System.Buffers.Binary;

namespace HCDE.Client.Tests;

public class VmMusicMusSourceTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PlaybackModeAcceptsLiveFlagAndLeavesWholeSongRepeatToPlayer(bool live)
    {
        using var source = new VmMusicMusSource(Mus([0x10, 60, 0x10, 61, 0x60]));
        source.StartPlayback(live, 3);
        Assert.Equal(3, source.LoopLimit);
        uint[] first = new uint[12], restarted = new uint[12];
        Assert.Equal(6, source.MakeEvents(first, uint.MaxValue));
        Assert.True(source.CheckDone());
        Assert.Equal(0, source.MakeEvents(restarted, uint.MaxValue));
        source.Restart();
        Assert.Equal(6, source.MakeEvents(restarted, uint.MaxValue));
        Assert.Equal(first, restarted);
        Assert.Throws<ArgumentOutOfRangeException>(() => source.StartPlayback(live, -1));
        Assert.Equal(3, source.LoopLimit);
    }

    private static byte[] Mus(byte[] score, int prefix = 0)
    {
        var bytes = new byte[prefix + 16 + score.Length];
        "MUS\u001a"u8.CopyTo(bytes.AsSpan(prefix));
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(prefix + 4), (ushort)score.Length);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(prefix + 6), 16);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(prefix + 8), 2);
        score.CopyTo(bytes, prefix + 16);
        return bytes;
    }

    [Theory]
    [InlineData(0)]
    [InlineData(28)]
    public void RealMusNoteEventsConvertToExactMidiBytes(int prefix)
    {
        using var source = new VmMusicMusSource(Mus([0x90, 0xbc, 100, 10, 0x80, 60, 5, 0x60], prefix));
        Assert.Equal(Convert.FromHexString("4D5468640000000600000001008C4D54726B0000001200FF51030F424000903C640A3C0005FF2F00"),
            VmMusicSmfSource.Create(source, 1));
    }

    [Fact]
    public void VelocityMemoryAndChannelMappingMatchMusRules()
    {
        using var source = new VmMusicMusSource(Mus([0x19, 0xbc, 70, 0x19, 61, 0x1f, 35, 0x60]));
        Assert.Equal(VmMusicSmf.Serialize(140, 1000000, [new(0, 0x463c9a), new(0, 0x463d9a), new(0, 0x7f2399)]),
            VmMusicSmfSource.Create(source, 1));
        Assert.Equal(VmMusicSmfSource.Create(source, 1), VmMusicSmfSource.Create(source, 1));
    }

    [Fact]
    public void PitchProgramVolumeAndMonoControllersTranslate()
    {
        using var source = new VmMusicMusSource(Mus([0x20, 255, 0x40, 0, 12, 0x40, 3, 255, 0x30, 12, 0x60]));
        Assert.Equal(VmMusicSmf.Serialize(140, 1000000,
            [new(0, 0x7f40e0), new(0, 0x0cc0), new(0, 0x7f07b0), new(0, 0x027eb0)]), VmMusicSmfSource.Create(source, 1));
    }

    [Fact]
    public void SmallBuffersReserveTrailingDelayAndPreserveTimingAcrossBatches()
    {
        using var source = new VmMusicMusSource(Mus([0x90, 60, 0x81, 0, 0x10, 61, 0x60]));
        var buffer = new uint[6];
        Assert.Equal(6, source.MakeEvents(buffer, uint.MaxValue));
        Assert.Equal(new uint[] { 0, 0, 0x7f3c90, 128, 0, 0x02000000 }, buffer);
        Assert.Equal(3, source.MakeEvents(buffer, uint.MaxValue));
        Assert.Equal(0x7f3d90u, buffer[2]);
        Assert.Equal(0, source.MakeEvents(buffer, uint.MaxValue));
        Assert.True(source.CheckDone());
    }

    [Theory]
    [InlineData(0x10, 128)]
    [InlineData(0x90, 60)]
    [InlineData(0x40, 3)]
    [InlineData(0x30, 15)]
    public void TruncatedOrInvalidEventsFailExplicitly(byte descriptor, byte value)
    {
        using var source = new VmMusicMusSource(Mus([descriptor, value]));
        Assert.Throws<InvalidDataException>(() => VmMusicSmfSource.Create(source, 1));
        Assert.Equal(0, source.LoopLimit);
    }

    [Fact]
    public void CommandExportsMusBytesToRealMidiFile()
    {
        var path = Path.Combine(Path.GetTempPath(), "hcde-mus-" + Guid.NewGuid().ToString("N") + ".mid");
        var input = Mus([0x10, 60, 0x60]);
        try
        {
            VmMusicMidiExport.WriteMidiFile(["writemidi", "song.mus", path],
                _ => new VmMusicMusSource(input), () => "unused", _ => throw new Exception("unexpected report"));
            Assert.Equal(VmMusicSmf.Serialize(140, 1000000, [new(0, 0x7f3c90)]), File.ReadAllBytes(path));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void InvalidHeaderOffsetsAndChannelCountsAreRejected()
    {
        var bytes = Mus([0x60]);
        bytes[6] = 255;
        Assert.Throws<InvalidDataException>(() => new VmMusicMusSource(bytes));
        bytes = Mus([0x60]); bytes[8] = 16;
        Assert.Throws<InvalidDataException>(() => new VmMusicMusSource(bytes));
        Assert.Throws<InvalidDataException>(() => new VmMusicMusSource(new byte[3]));
    }
}
