namespace HCDE.Client.Tests;

public class VmMusicWildMidiChannelVolumeTests
{
    [Theory]
    [InlineData(1, 2)]
    [InlineData(16, 3)]
    [InlineData(255, 3)]
    public void AdjustmentPreservesVoiceReplayOrderAndNativeAllChannelSentinel(int channel, int count)
    {
        var replay = new VmMusicWildMidiNoteReleaseState();
        var first = new VmMusicWildMidiNoteReleaseState { NoteId = 0x13c, Replay = replay };
        var other = new VmMusicWildMidiNoteReleaseState { NoteId = 0x23c };
        var calls = new List<VmMusicWildMidiNoteReleaseState>();
        VmMusicWildMidiChannelVolume.Adjust([first, other], channel, (inputChannel, voice) =>
            { Assert.Equal(channel, inputChannel); calls.Add(voice); });
        Assert.Equal(count, calls.Count);
        Assert.Same(first, calls[0]); Assert.Same(replay, calls[1]);
        if (count == 3) Assert.Same(other, calls[2]);
    }

    [Fact]
    public void ControllerVolumeChangeCanAdjustMatchingVoicesThroughConvertedTraversal()
    {
        var channel = new VmMusicWildMidiChannelState();
        var voice = new VmMusicWildMidiNoteReleaseState { NoteId = 0x13c };
        var adjusted = 0;
        Assert.True(VmMusicWildMidiChannelControls.Apply(channel, [voice], 1, 7, 100,
            () => VmMusicWildMidiChannelVolume.Adjust([voice], 1, (_, input) =>
            { Assert.Same(voice, input); Assert.Equal(100, channel.Volume); adjusted++; })));
        Assert.Equal(1, adjusted);
    }

    [Fact]
    public void AdjustmentFailurePropagatesBeforeReplayProcessing()
    {
        var voice = new VmMusicWildMidiNoteReleaseState { Replay = new() };
        var error = new IOException("adjustment failed");
        var calls = 0;
        Assert.Same(error, Assert.Throws<IOException>(() => VmMusicWildMidiChannelVolume.Adjust([voice], 16,
            (_, _) => { calls++; throw error; })));
        Assert.Equal(1, calls);
    }
}
