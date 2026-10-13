namespace HCDE.Client.Tests;

public class VmMusicWildMidiNoteVolumeTests
{
    [Fact]
    public void ControllerChangeUpdatesVoiceAndReplayThroughConvertedVolumePath()
    {
        var channels = Enumerable.Range(0, 16).Select(_ => new VmMusicWildMidiChannelState
            { Volume = 127, Expression = 127, Pan = 64, Balance = 64 }).ToArray();
        var replay = new VmMusicWildMidiNoteReleaseState { Velocity = 64 };
        var voice = new VmMusicWildMidiNoteReleaseState { NoteId = 0x13c, Velocity = 127, Replay = replay };
        VmMusicWildMidiChannelControls.Apply(channels[1], [voice], 1, 7, 100,
            () => VmMusicWildMidiChannelVolume.Adjust([voice], 1, (channel, note) =>
                VmMusicWildMidiNoteVolume.Adjust(channels, channel, note, 1024, false)));
        Assert.Equal(VmMusicWildMidiNoteVolume.Compute(channels[1], 127, 1024, false), voice.MixVolume);
        Assert.Equal(VmMusicWildMidiNoteVolume.Compute(channels[1], 64, 1024, false), replay.MixVolume);
        Assert.True(voice.MixVolume.Left > replay.MixVolume.Left);
    }

    [Fact]
    public void AllChannelSentinelUsesNativeLastChannelClampAndMasterVolumeTable()
    {
        var channels = Enumerable.Range(0, 16).Select(_ => new VmMusicWildMidiChannelState()).ToArray();
        channels[15].Volume = 127; channels[15].Expression = 127;
        channels[15].Pan = 0; channels[15].Balance = 64;
        var voice = new VmMusicWildMidiNoteReleaseState { NoteId = 0x13c, Velocity = 127 };
        VmMusicWildMidiNoteVolume.Adjust(channels, 16, voice, VmMusicWildMidiNoteVolume.MasterVolume(255), false);
        Assert.Equal(new(256, 0), voice.MixVolume);
        Assert.Equal(0, VmMusicWildMidiNoteVolume.MasterVolume(0));
        Assert.Equal(1024, VmMusicWildMidiNoteVolume.MasterVolume(127));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CenterIsSymmetricAndZeroVelocityOrMasterIsSilent(bool logarithmic)
    {
        var channel = new VmMusicWildMidiChannelState { Volume = 127, Expression = 127, Pan = 64, Balance = 64 };
        var full = VmMusicWildMidiNoteVolume.Compute(channel, 127, 1024, logarithmic);
        Assert.Equal(full.Left, full.Right); Assert.True(full.Left > 0);
        Assert.Equal(new(0, 0), VmMusicWildMidiNoteVolume.Compute(channel, 0, 1024, logarithmic));
        Assert.Equal(new(0, 0), VmMusicWildMidiNoteVolume.Compute(channel, 127, 0, logarithmic));
    }

    [Fact]
    public void NativeHardPanAndIntegerVolumeIndexArePreserved()
    {
        var channel = new VmMusicWildMidiChannelState { Volume = 127, Expression = 127, Pan = 0, Balance = 64 };
        var left = VmMusicWildMidiNoteVolume.Compute(channel, 127, 1024, false);
        Assert.Equal(new(256, 0), left);
        channel.Pan = 1;
        Assert.Equal(left, VmMusicWildMidiNoteVolume.Compute(channel, 127, 1024, false));
        channel.Pan = 127;
        Assert.Equal(new(0, 256), VmMusicWildMidiNoteVolume.Compute(channel, 127, 1024, false));
    }
}
