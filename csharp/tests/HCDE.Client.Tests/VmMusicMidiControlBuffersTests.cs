namespace HCDE.Client.Tests;

public class VmMusicMidiControlBuffersTests
{
    [Theory]
    [InlineData(0u, 120, 500000u, 1u)]
    [InlineData(600000u, 120, 500000u, 48u)]
    [InlineData(10u, 120, 1u, 360u)]
    [InlineData(uint.MaxValue, 65535, 4000000u, 23455890u)]
    public void PausedNopUsesOneThirdTimeAndWideArithmetic(uint maximumTime, ushort division, uint tempo, uint expected)
    {
        uint[] buffer = [99, 99, 99, 99];
        Assert.Equal(3, VmMusicMidiControlBuffers.WritePaused(buffer, maximumTime, division, tempo, new(new())));
        Assert.Equal(new uint[] { expected, 0, 0x02000000, 99 }, buffer);
    }

    [Fact]
    public void PausedMutePrecedesNopAndNonzeroRequestRemainsPending()
    {
        var updates = new VmMusicMidiVolumeUpdates(new());
        updates.OutputVolume(50000, true);
        Assert.Equal(3, VmMusicMidiControlBuffers.WritePaused(new uint[3], 600000, 120, 500000, updates));
        Assert.True(updates.HasPendingUpdate);
        updates.OutputVolume(0, true);
        uint[] buffer = new uint[51];
        Assert.Equal(51, VmMusicMidiControlBuffers.WritePaused(buffer, 600000, 120, 500000, updates));
        Assert.Equal(0x07b0u, buffer[2]);
        Assert.Equal(48u, buffer[48]);
        Assert.False(updates.HasPendingUpdate);
    }

    [Fact]
    public void ShortPausedBufferRetainsMuteAndWritesNothing()
    {
        var updates = new VmMusicMidiVolumeUpdates(new());
        updates.OutputVolume(0, true);
        var buffer = Enumerable.Repeat(99u, 50).ToArray();
        Assert.Throws<ArgumentException>(() => VmMusicMidiControlBuffers.WritePaused(buffer, 1, 120, 500000, updates));
        Assert.All(buffer, word => Assert.Equal(99u, word));
        Assert.True(updates.HasPendingUpdate);
    }

    [Fact]
    public void StopResetsEveryChannelAndHoldsFiveHundredTicks()
    {
        uint[] buffer = new uint[100]; buffer[^1] = 99;
        Assert.Equal(99, VmMusicMidiControlBuffers.WriteStop(buffer));
        for (var channel = 0; channel < 16; channel++)
        {
            Assert.Equal((uint)(0x7bb0 | channel), buffer[channel * 6 + 2]);
            Assert.Equal((uint)(0x79b0 | channel), buffer[channel * 6 + 5]);
        }
        Assert.Equal(new uint[] { 500, 0, 0x02000000, 99 }, buffer[96..]);
    }

    [Theory]
    [InlineData(false, 10)]
    [InlineData(true, 16)]
    public void InitialPacketsMatchNativeBytesAndResetChannelState(bool sysex, int count)
    {
        var channels = new VmMusicMidiChannelVolume { MasterVolume = 123 };
        channels.ControllerChange(0, 5);
        uint[] buffer = new uint[count];
        Assert.Equal(count, VmMusicMidiControlBuffers.WriteInitial(buffer, sysex, channels));
        Assert.Equal(new uint[] { 0, 0, 0x80000006, 0x097f7ef0, 0x0000f701 }, buffer[..5]);
        if (sysex) Assert.Equal(new uint[] { 0, 0, 0x8000000b, 0x427f41f0, 0x7f004012, 0x00f74100 }, buffer[5..11]);
        Assert.Equal(new uint[] { 0, 0, 0x80000008, 0x047f7ff0, 0xf77f7f01 }, buffer[^5..]);
        Assert.Equal(100, channels.GetChannelVolume(0));
        Assert.Equal(123, channels.MasterVolume);
    }

    [Fact]
    public void InvalidTimingAndShortControlBuffersFailBeforeMutation()
    {
        var channels = new VmMusicMidiChannelVolume(); channels.ControllerChange(0, 5);
        var buffer = Enumerable.Repeat(99u, 9).ToArray();
        Assert.Throws<ArgumentException>(() => VmMusicMidiControlBuffers.WriteInitial(buffer, false, channels));
        Assert.Equal(5, channels.GetChannelVolume(0));
        Assert.All(buffer, word => Assert.Equal(99u, word));
        Assert.Throws<ArgumentException>(() => VmMusicMidiControlBuffers.WriteStop(new uint[98]));
        Assert.Throws<ArgumentOutOfRangeException>(() => VmMusicMidiControlBuffers.WritePaused(buffer, 1, 0, 1, new(channels)));
        Assert.Throws<ArgumentOutOfRangeException>(() => VmMusicMidiControlBuffers.WritePaused(buffer, 1, 1, 0, new(channels)));
    }
}
