using System.Buffers.Binary;

namespace HCDE.Client.Tests;

public class VmMusicSmfPlaybackSourceTests
{
    [Fact]
    public void FormatZeroRoundTripsThroughFactorySourceAndBinaryCommand()
    {
        var bytes = VmMusicSmf.Serialize(480, 1000000,
            [new(0, 0x403c90), new(128, 0x403d90), new(3, 0x0107a120),
                new(0, 0x80000004, new byte[] { 240, 1, 2, 247 }), new(7, 0x02000000)]);
        var path = Path.Combine(Path.GetTempPath(), "hcde-smf-source-" + Guid.NewGuid().ToString("N") + ".mid");
        try
        {
            VmMusicMidiExport.WriteMidiFile(["writemidi", "song", path], name =>
                VmMusicMidiSource.OpenManaged(name, "", _ => 0, _ => -1, _ => bytes,
                    _ => throw new Exception("unexpected loader report")), () => "unused", _ => throw new Exception("unexpected report"));
            Assert.Equal(bytes, File.ReadAllBytes(path));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void LongEventWaitsForNextBatchWithoutLosingDataOrPadding()
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000,
            [new(0, 0x403c90), new(2, 0x80000005, new byte[] { 240, 1, 2, 3, 247 })]));
        var buffer = new uint[5];
        Assert.Equal(3, source.MakeEvents(buffer, uint.MaxValue));
        Assert.Equal(5, source.MakeEvents(buffer, uint.MaxValue));
        Assert.Equal(new uint[] { 2, 0, 0x80000005, 0x030201f0, 247 }, buffer);
        Assert.True(source.CheckDone());
        source.Restart();
        Assert.False(source.CheckDone());
    }

    [Fact]
    public void OversizedEventFailsInsteadOfStallingOrSilentlyDropping()
    {
        var payload = new byte[1600]; payload[0] = 240;
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000, [new(0, 0x80000640, payload)]));
        Assert.Throws<InvalidDataException>(() => VmMusicSmfSource.Create(source, 1));
        Assert.Equal(0, source.LoopLimit);
    }

    [Fact]
    public void TimeBudgetUsesTempoBeforeEventAndUpdatesForFollowingEvents()
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(100, 1000000,
            [new(1, 0x0107a120), new(1, 0x403c90), new(1, 0x403d90)]));
        var buffer = new uint[12];
        Assert.Equal(3, source.MakeEvents(buffer, 6000));
        Assert.Equal(6, source.MakeEvents(buffer, 6000));
        Assert.True(source.CheckDone());
    }

    [Theory]
    [InlineData(0, 0xe728)]
    public void UnconvertedFormatsAndTimingAreExplicitlyRejected(ushort format, ushort division)
    {
        var bytes = VmMusicSmf.Serialize(division, 500000, []);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(8), format);
        Assert.Throws<InvalidDataException>(() => new VmMusicSmfPlaybackSource(bytes));
    }

    [Theory]
    [InlineData(119)]
    public void RepeatedEmidiLoopsAreRejectedUntilTheirSemanticsAreConverted(byte controller)
    {
        var track = new VmMusicSmfTrack(500000, [new(0, (uint)(0xb0 | (controller << 8)))]);
        Assert.Throws<InvalidDataException>(() => VmMusicEmidi.Apply(track, 480, loopLimit: 2));
    }
}
