namespace HCDE.Client.Tests;

public class VmReplayGainHashTests
{
    [Fact]
    public void WaveformHashUsesReadBytesAndResetsReaderBeforePaddedProbe()
    {
        using var reader = new VmMusicByteReader("abc"u8);
        var key = VmReplayGainHash.Create(reader, 3, -1, "", 0, header =>
        {
            Assert.Equal(0, reader.Tell());
            Assert.Equal(32, header.Length);
            Assert.Equal((byte)'a', header[0]);
            Assert.Equal(0, header[3]);
            return false;
        }, _ => throw new InvalidOperationException());
        Assert.Equal("3:900150983CD24FB0D6963F7D28E17F72", key);
    }

    [Theory]
    [InlineData(-1, 4)]
    [InlineData(-2, 3)]
    [InlineData(-3, 1)]
    [InlineData(-4, 5)]
    [InlineData(-5, 4)]
    [InlineData(-6, 6)]
    [InlineData(-7, 7)]
    [InlineData(-8, 8)]
    public void MidiDefaultsMapToNativeDevicesAndUppercaseParameter(int configured, int expected)
    {
        using var reader = new VmMusicByteReader([]);
        var key = VmReplayGainHash.Create(reader, 0, -1, "", configured, _ => true,
            device => { Assert.Equal(expected, device); return "bank.sf2"; });
        Assert.Equal($"0:D41D8CD98F00B204E9800998ECF8427E:{expected}:BANK.SF2", key);
    }

    [Fact]
    public void SystemSynthAndUnknownDefaultHaveNoKey()
    {
        using var reader = new VmMusicByteReader([]);
        Assert.Equal("", VmReplayGainHash.Create(reader, 0, 2, "custom", 0, _ => true, _ => throw new InvalidOperationException()));
        Assert.Equal("", VmReplayGainHash.Create(reader, 0, -1, "", 0, _ => true, _ => throw new InvalidOperationException()));
    }

    [Fact]
    public void HashIsLimitedToFirstFiftyThousandBytesAndExplicitParameterOverridesDefaults()
    {
        var data = new byte[50001];
        using var first = new VmMusicByteReader(data);
        data[^1] = 1;
        using var second = new VmMusicByteReader(data);
        string Key(VmMusicByteReader reader) => VmReplayGainHash.Create(reader, data.Length, 4, "custom", 0,
            _ => true, _ => throw new InvalidOperationException());
        Assert.Equal(Key(first), Key(second));
        Assert.EndsWith(":4:CUSTOM", Key(first));
    }
}
