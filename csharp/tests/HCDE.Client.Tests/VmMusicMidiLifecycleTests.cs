namespace HCDE.Client.Tests;

public class VmMusicMidiLifecycleTests
{
    private sealed class Device : IVmMusicMidiPauseDevice, IVmMusicMidiBufferDevice
    {
        public bool FakeVolume { get; set; } = true;
        public bool HandlesPause { get; set; }
        public List<bool> Calls { get; } = [];
        public List<uint[]> Packets { get; } = [];
        public bool Pause(bool paused) { Calls.Add(paused); return HandlesPause; }
        public int Unprepare(int number) => 0;
        public int Prepare(int number, ReadOnlyMemory<uint> events) { Packets.Add(events.ToArray()); return 0; }
        public int StreamOut(int number) => 0;
        public int StreamOutSync(int number) => 0;
    }
    private static VmMusicSmfPlaybackSource Source() => new(VmMusicSmf.Serialize(120, 500000, [new(0, 0x01c0)]));

    [Fact]
    public void UnsupportedPauseQueuesMuteAndResumeRestoresLatestMaster()
    {
        using var source = Source();
        var playback = new VmMusicMidiPlaybackBuffer(source);
        var device = new Device();
        var lifecycle = new VmMusicMidiLifecycle(playback, device);
        lifecycle.MarkPlaying(); lifecycle.Pause();
        Assert.Equal(VmMusicMidiPlaybackState.Paused, lifecycle.State);
        uint[] buffer = new uint[48];
        Assert.Equal(48, playback.VolumeUpdates.WritePending(buffer, true));
        Assert.Equal(0x07b0u, buffer[2]);
        lifecycle.MusicVolumeChanged(0.5f, 1, 1);
        Assert.False(playback.VolumeUpdates.HasPendingUpdate);
        lifecycle.Resume();
        Assert.Equal(VmMusicMidiPlaybackState.Playing, lifecycle.State);
        Assert.Equal(48, playback.VolumeUpdates.WritePending(buffer, false));
        Assert.Equal(0x3207b0u, buffer[2]);
        Assert.Equal(new[] { true, false }, device.Calls);
    }

    [Fact]
    public void SupportedDevicePauseDoesNotInventVolumeUpdates()
    {
        using var source = Source();
        var playback = new VmMusicMidiPlaybackBuffer(source);
        var lifecycle = new VmMusicMidiLifecycle(playback, new Device { HandlesPause = true });
        lifecycle.MarkPlaying(); lifecycle.Pause(); lifecycle.Resume();
        Assert.False(playback.VolumeUpdates.HasPendingUpdate);
    }

    [Fact]
    public void StoppedAndRepeatedTransitionsAreIgnoredAndStopClearsPending()
    {
        using var source = Source();
        var playback = new VmMusicMidiPlaybackBuffer(source);
        var device = new Device();
        var lifecycle = new VmMusicMidiLifecycle(playback, device);
        lifecycle.Pause(); lifecycle.Resume();
        Assert.Empty(device.Calls);
        lifecycle.MarkPlaying(); lifecycle.Pause(); lifecycle.Pause(); lifecycle.Resume(); lifecycle.Resume();
        Assert.Equal(new[] { true, false }, device.Calls);
        lifecycle.MarkStopped();
        Assert.False(playback.VolumeUpdates.HasPendingUpdate);
    }

    [Theory]
    [InlineData(0f, 1f, 1f, 0)]
    [InlineData(0.5f, 0.5f, 1f, 16383)]
    [InlineData(-1f, 1f, 1f, 65535)]
    [InlineData(2f, 1f, 1f, 65535)]
    public void MusicVolumeUsesNativeProductRangeFallbackAndTruncation(float music, float relative, float master, int expected)
    {
        using var source = Source();
        var playback = new VmMusicMidiPlaybackBuffer(source);
        var lifecycle = new VmMusicMidiLifecycle(playback, new Device());
        lifecycle.MusicVolumeChanged(music, relative, master);
        Assert.Equal(expected, playback.Channels.MasterVolume);
        Assert.False(playback.VolumeUpdates.HasPendingUpdate);
        lifecycle.MarkPlaying(); lifecycle.MusicVolumeChanged(music, relative, master);
        Assert.True(playback.VolumeUpdates.HasPendingUpdate);
    }

    [Fact]
    public void NonFakeDeviceKeepsFullSourceVolumeAndDoesNotQueueMute()
    {
        using var source = Source();
        var playback = new VmMusicMidiPlaybackBuffer(source);
        var lifecycle = new VmMusicMidiLifecycle(playback, new Device { FakeVolume = false });
        lifecycle.MarkPlaying(); lifecycle.MusicVolumeChanged(0, 1, 1); lifecycle.Pause(); lifecycle.Resume();
        Assert.Equal(65535, playback.Channels.MasterVolume);
        Assert.False(playback.VolumeUpdates.HasPendingUpdate);
    }

    [Fact]
    public void LifecycleServiceSuppressesStoppedWorkAndRoutesPausedBuffers()
    {
        using var source = Source();
        var playback = new VmMusicMidiPlaybackBuffer(source);
        var device = new Device();
        var service = new VmMusicMidiBufferService(playback, device);
        var lifecycle = new VmMusicMidiLifecycle(playback, device);
        Assert.Equal(0, lifecycle.Service(service, 600000, false, false));
        Assert.Empty(device.Packets);
        lifecycle.MarkPlaying(); lifecycle.Pause();
        Assert.Equal(0, lifecycle.Service(service, 600000, false, false));
        Assert.False(source.CheckDone());
        Assert.Equal(61, device.Packets[0].Length);
        lifecycle.Resume();
        Assert.Equal(0, lifecycle.Service(service, 600000, false, false));
        Assert.True(source.CheckDone());
        Assert.Equal(0x01c0u, device.Packets[1][^1]);
    }

    [Fact]
    public void NonfiniteVolumeFailsWithoutChangingState()
    {
        using var source = Source();
        var playback = new VmMusicMidiPlaybackBuffer(source);
        var lifecycle = new VmMusicMidiLifecycle(playback, new Device());
        Assert.Throws<ArgumentOutOfRangeException>(() => lifecycle.MusicVolumeChanged(float.NaN, 1, 1));
        Assert.Equal(65535, playback.Channels.MasterVolume);
        Assert.False(playback.VolumeUpdates.HasPendingUpdate);
    }
}
