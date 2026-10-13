namespace HCDE.Client.Tests;

public class VmMusicWildMidiSystemExclusiveTests
{
    private static VmMusicWildMidiChannelState[] Channels() => Enumerable.Range(0, 16)
        .Select(_ => new VmMusicWildMidiChannelState()).ToArray();

    [Theory]
    [InlineData("F07E7F0901F7")]
    [InlineData("F043104C00007E00F7")]
    [InlineData("F04110421240007F0041F7")]
    public void RecognizedResetsReachSharedChannelReset(string hex)
    {
        var channels = Channels();
        var adjusted = 0;
        Assert.True(VmMusicWildMidiSystemExclusive.Handle(Convert.FromHexString(hex), channels, _ => null,
            channel => { Assert.Equal(16, channel); adjusted++; }));
        Assert.Equal(1, adjusted); Assert.True(channels[9].IsDrum); Assert.Equal(100, channels[0].Volume);
    }

    [Theory]
    [InlineData(0, 9)]
    [InlineData(1, 0)]
    [InlineData(9, 8)]
    [InlineData(10, 10)]
    public void RolandDrumTrackUsesNativeChannelMapping(int part, int channel)
    {
        byte[] data = [0xf0, 0x41, 0x10, 0x42, 0x12, 0x40, (byte)(0x10 | part), 0x15, 1, 0, 0xf7];
        data[9] = (byte)(128 - ((data[5] + data[6] + data[7] + data[8]) & 127));
        var channels = Channels();
        Assert.True(VmMusicWildMidiSystemExclusive.Handle(data, channels, _ => throw new Exception(), _ => throw new Exception()));
        Assert.True(channels[channel].IsDrum); Assert.Single(channels, c => c.IsDrum);
    }

    [Fact]
    public void BadChecksumTruncatedAndUnknownMessagesDoNotMutateChannels()
    {
        var channels = Channels();
        foreach (var hex in new[] { "F04110421240007F0040F7", "F04110421240", "F07E7F0902F7" })
            Assert.False(VmMusicWildMidiSystemExclusive.Handle(Convert.FromHexString(hex), channels,
                _ => throw new Exception(), _ => throw new Exception()));
        Assert.All(channels, c => Assert.Equal(0, c.Volume));
    }
}
