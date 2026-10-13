namespace HCDE.Client.Tests;

public class VmMusicMidiPlaybackBufferTests
{
    private static VmMusicSmfPlaybackSource Source() => new(VmMusicSmf.Serialize(120, 500000,
        [new(0, 0x6407b0), new(2, 0x643c90)]));

    [Fact]
    public void InitialControlsPrecedeScaledSongAndAreEmittedOnce()
    {
        using var source = Source();
        var playback = new VmMusicMidiPlaybackBuffer(source);
        playback.Channels.MasterVolume = 32768;
        uint[] buffer = new uint[64];
        Assert.Equal(16, playback.Fill(buffer, 1000000, false, false));
        Assert.Equal(0x80000006u, buffer[2]);
        Assert.Equal(0x3207b0u, buffer[12]);
        Assert.Equal(0x643c90u, buffer[15]);
        Assert.Equal(100, playback.Channels.GetChannelVolume(0));
        Assert.Equal(0, playback.Fill(buffer, 1000000, false, false));
    }

    [Fact]
    public void PausedMuteAndRestartDeferralDoNotAdvanceSong()
    {
        using var source = Source();
        var playback = new VmMusicMidiPlaybackBuffer(source);
        playback.RequestRestart();
        playback.VolumeUpdates.OutputVolume(0, true);
        uint[] buffer = new uint[256];
        Assert.Equal(61, playback.Fill(buffer, 600000, true, false));
        Assert.False(source.CheckDone());
        Assert.Equal(0x07b0u, buffer[12]);
        Assert.Equal(48u, buffer[58]);
        Assert.Equal(105, playback.Fill(buffer, 600000, false, false));
        Assert.Equal(0x0107a120u, buffer[2]);
        Assert.Equal(0x7bb0u, buffer[5]);
        Assert.True(source.CheckDone());
    }

    [Fact]
    public void ShortBufferFailsBeforeInitialAndPendingStateAreConsumed()
    {
        using var source = Source();
        var playback = new VmMusicMidiPlaybackBuffer(source);
        playback.VolumeUpdates.OutputVolume(0, true);
        Assert.Throws<ArgumentException>(() => playback.Fill(new uint[60], 600000, true, false));
        Assert.True(playback.VolumeUpdates.HasPendingUpdate);
        uint[] buffer = new uint[61];
        Assert.Equal(61, playback.Fill(buffer, 600000, true, false));
        Assert.Equal(0x80000006u, buffer[2]);
    }

    [Fact]
    public void TempoEventsControlSubsequentPausedNopDelay()
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(120, 500000,
            [new(1, 0x0103d090), new(0, 0x80000003, new byte[] { 0xf0, 1, 0xf7 })]));
        var playback = new VmMusicMidiPlaybackBuffer(source);
        uint[] buffer = new uint[64];
        Assert.Equal(17, playback.Fill(buffer, 1000000, false, false));
        Assert.Equal(250000u, playback.Tempo);
        Assert.Equal(0x00f701f0u, buffer[16]);
        Assert.Equal(3, playback.Fill(buffer, 600000, true, false));
        Assert.Equal(96u, buffer[0]);
    }

    [Fact]
    public void PlaybackScalingDoesNotChangeSubsequentExport()
    {
        using var source = Source();
        var playback = new VmMusicMidiPlaybackBuffer(source);
        playback.Channels.MasterVolume = 0;
        playback.Fill(new uint[64], 1000000, false, false);
        Assert.Equal(VmMusicSmf.Serialize(120, 500000, [new(0, 0x6407b0), new(2, 0x643c90)]), VmMusicSmfSource.Create(source, 1));
    }
}
