namespace HCDE.Client.Tests;

public class VmMusicMidiVolumeUpdatesTests
{
    [Fact]
    public void NoPendingUpdateDoesNotRequireSpaceOrModifyBuffer()
    {
        var updates = new VmMusicMidiVolumeUpdates(new());
        uint[] buffer = [99];
        Assert.Equal(0, updates.WritePending(buffer, false));
        Assert.Equal(99u, buffer[0]);
    }

    [Fact]
    public void PlayingEmitsAllChannelsOnceUsingPendingVolume()
    {
        var channels = new VmMusicMidiChannelVolume { MasterVolume = ushort.MaxValue };
        channels.ControllerChange(15, 127);
        var updates = new VmMusicMidiVolumeUpdates(channels);
        updates.OutputVolume(32768, true);
        uint[] buffer = new uint[51]; buffer[^1] = 99;
        Assert.Equal(48, updates.WritePending(buffer, false));
        Assert.Equal(0x3207b0u, buffer[2]);
        Assert.Equal(0x4007bfu, buffer[47]);
        for (var i = 0; i < 16; i++)
        {
            Assert.Equal(0u, buffer[i * 3]);
            Assert.Equal(0u, buffer[i * 3 + 1]);
        }
        Assert.Equal(99u, buffer[^1]);
        Assert.Equal(ushort.MaxValue, channels.MasterVolume);
        Assert.Equal(127, channels.GetChannelVolume(15));
        Assert.False(updates.HasPendingUpdate);
        Assert.Equal(0, updates.WritePending([], false));
    }

    [Fact]
    public void PausedNonzeroIsDeferredUntilResume()
    {
        var updates = new VmMusicMidiVolumeUpdates(new());
        updates.OutputVolume(65535, true);
        Assert.Equal(0, updates.WritePending([], true));
        Assert.True(updates.HasPendingUpdate);
        Assert.Equal(48, updates.WritePending(new uint[48], false));
    }

    [Fact]
    public void PausedMuteIsEmittedImmediately()
    {
        var updates = new VmMusicMidiVolumeUpdates(new());
        updates.OutputVolume(0, true);
        uint[] buffer = new uint[48];
        Assert.Equal(48, updates.WritePending(buffer, true));
        for (var i = 0; i < 16; i++) Assert.Equal((uint)(0x07b0 | i), buffer[i * 3 + 2]);
    }

    [Fact]
    public void LatestPendingChangeWinsAndReadsCurrentRawChannelValues()
    {
        var channels = new VmMusicMidiChannelVolume();
        var updates = new VmMusicMidiVolumeUpdates(channels);
        updates.OutputVolume(0, true);
        updates.OutputVolume(65535, true);
        channels.ControllerChange(0, 25);
        uint[] buffer = new uint[48];
        Assert.Equal(48, updates.WritePending(buffer, false));
        Assert.Equal(0x1907b0u, buffer[2]);
    }

    [Fact]
    public void DeviceWithoutFakeVolumeDoesNotQueueOrReplaceUpdate()
    {
        var updates = new VmMusicMidiVolumeUpdates(new());
        updates.OutputVolume(0, false);
        Assert.False(updates.HasPendingUpdate);
        updates.OutputVolume(65535, true);
        updates.OutputVolume(0, false);
        Assert.Equal(0, updates.WritePending([], true));
        Assert.True(updates.HasPendingUpdate);
    }

    [Fact]
    public void InsufficientBufferRetainsPendingUpdateWithoutWriting()
    {
        var updates = new VmMusicMidiVolumeUpdates(new());
        updates.OutputVolume(0, true);
        var buffer = Enumerable.Repeat(99u, 47).ToArray();
        Assert.Throws<ArgumentException>(() => updates.WritePending(buffer, true));
        Assert.All(buffer, value => Assert.Equal(99u, value));
        Assert.True(updates.HasPendingUpdate);
        Assert.Equal(48, updates.WritePending(new uint[48], true));
    }

    [Fact]
    public void NewSessionClearDropsPendingUpdate()
    {
        var updates = new VmMusicMidiVolumeUpdates(new());
        updates.OutputVolume(123, true);
        updates.Clear();
        Assert.False(updates.HasPendingUpdate);
        Assert.Equal(0, updates.WritePending([], false));
    }
}
