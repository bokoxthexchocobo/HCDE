namespace HCDE.Client.Tests;

public class VmMusicMidiSysexTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SuppressionPreservesDelayAndFollowingEvents(bool suppress)
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(120, 500000,
            [new(7, 0x80000003, new byte[] { 0xf0, 1, 0xf7 }), new(3, 0x643c90)]));
        source.SkipSysex = suppress;
        uint[] buffer = new uint[32];
        var count = source.MakeEvents(buffer, 1000000);
        Assert.Equal(suppress ? 6 : 7, count);
        Assert.Equal(7u, buffer[0]);
        Assert.Equal(suppress ? 0x02000000u : 0x80000003u, buffer[2]);
        var offset = suppress ? 3 : 4;
        Assert.Equal(3u, buffer[offset]);
        Assert.Equal(0x643c90u, buffer[offset + 2]);
    }

    [Fact]
    public void SuppressedOversizedPacketFitsSingleEventSlotAndPolicySurvivesRestart()
    {
        var data = new byte[2000];
        data[0] = 0xf0; data[^1] = 0xf7;
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(120, 500000,
            [new(9, 0x80000000u | (uint)data.Length, data)]));
        source.SkipSysex = true;
        uint[] buffer = new uint[3];
        Assert.Equal(3, source.MakeEvents(buffer, 1000000));
        Assert.Equal(new uint[] { 9, 0, 0x02000000 }, buffer);
        source.Restart();
        Assert.Equal(3, source.MakeEvents(buffer, 1000000));
        source.SkipSysex = false;
        source.Restart();
        Assert.Throws<InvalidDataException>(() => source.MakeEvents(buffer, 1000000));
    }

    [Fact]
    public void EscapedSystemPacketSuppressionPreservesTiming()
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(120, 500000,
            [new(4, 0x80000002, new byte[] { 0xf1, 3 })]));
        source.SkipSysex = true;
        uint[] buffer = new uint[3];
        Assert.Equal(3, source.MakeEvents(buffer, 1000000));
        Assert.Equal(new uint[] { 4, 0, 0x02000000 }, buffer);
    }
}
