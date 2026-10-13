using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiBoundVoiceTests
{
    private static VmMusicWildMidiInitializedVoice Prepared(short value) => new(new()
    {
        NoteId = 60, Velocity = 100, SampleIncrement = 1024, Modes = 64,
        EnvelopeTargets = [4194303, 0, 0, 0, 0, 0, 0], EnvelopeRates = [100, 1, 1, 1, 1, 1, 10],
        MixVolume = new(1024, 1024)
    }, new(new short[] { value, value, 0 }, new(2048, 0, 0, 0)));

    [Fact]
    public void StartBindsSampleAndResetsReusableStateWithoutClearingActivation()
    {
        var primary = new VmMusicWildMidiNoteReleaseState { SamplePosition = 99, IsOff = true, Replay = new(), EnvelopeLevel = 99 };
        var voices = new List<VmMusicWildMidiNoteReleaseState>();
        Assert.Same(primary, VmMusicWildMidiVoiceInitialization.Start(voices, primary, new(), Prepared(100)));
        Assert.True(primary.Active);
        Assert.Equal(0U, primary.SamplePosition);
        Assert.Equal(0, primary.EnvelopeLevel);
        Assert.False(primary.IsOff);
        Assert.Null(primary.Replay);
        VmMusicWildMidiLinearMixer.Mix(voices, new int[2]);
        Assert.Equal(100, primary.EnvelopeLevel);
    }

    [Fact]
    public void RetriggerKeepsOldSampleUntilReplayActivation()
    {
        var primary = new VmMusicWildMidiNoteReleaseState();
        var secondary = new VmMusicWildMidiNoteReleaseState();
        var voices = new List<VmMusicWildMidiNoteReleaseState>();
        VmMusicWildMidiVoiceInitialization.Start(voices, primary, secondary, Prepared(100));
        primary.Envelope = 4;
        primary.EnvelopeLevel = 4096;
        Assert.Same(secondary, VmMusicWildMidiVoiceInitialization.Start(voices, primary, secondary, Prepared(200)));
        Assert.Equal(100, primary.Sample!.Value.Data.Span[0]);
        Assert.Equal(200, secondary.Sample!.Value.Data.Span[0]);
        Assert.False(secondary.Active);
        VmMusicWildMidiVoiceLifecycle.Complete(voices, 0);
        Assert.True(secondary.Active);
        Assert.Same(secondary, Assert.Single(voices));
        VmMusicWildMidiLinearMixer.Mix(voices, new int[2]);
        Assert.Equal(100, secondary.EnvelopeLevel);
    }

    [Fact]
    public void SuppressedDuplicateDoesNotOverwriteSampleBinding()
    {
        var primary = new VmMusicWildMidiNoteReleaseState();
        var voices = new List<VmMusicWildMidiNoteReleaseState>();
        VmMusicWildMidiVoiceInitialization.Start(voices, primary, new(), Prepared(100));
        Assert.Null(VmMusicWildMidiVoiceInitialization.Start(voices, primary, new(), Prepared(200)));
        Assert.Equal(100, primary.Sample!.Value.Data.Span[0]);
    }

    [Fact]
    public void InvalidPreparedStateFailsBeforeSlotInsertion()
    {
        var primary = new VmMusicWildMidiNoteReleaseState();
        var voices = new List<VmMusicWildMidiNoteReleaseState>();
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiVoiceInitialization.Start(voices, primary, new(), new(new(), default)));
        Assert.Empty(voices);
        Assert.False(primary.Active);
    }

    [Fact]
    public void MixingUnboundVoiceFailsExplicitly()
    {
        Assert.Throws<InvalidOperationException>(() => VmMusicWildMidiLinearMixer.Mix(
            new List<VmMusicWildMidiNoteReleaseState> { new() }, new int[2]));
    }
}
