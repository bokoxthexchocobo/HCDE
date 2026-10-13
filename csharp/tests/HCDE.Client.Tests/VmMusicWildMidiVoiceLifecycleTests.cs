using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiVoiceLifecycleTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void CompletionRemovesVoiceAndReturnsSuccessor(int index)
    {
        var original = new[] { new VmMusicWildMidiNoteReleaseState { Active = true }, new() { Active = true }, new() { Active = true } };
        var voices = original.ToList();
        var next = VmMusicWildMidiVoiceLifecycle.Complete(voices, index);
        Assert.False(original[index].Active);
        Assert.Equal(original.Where((_, i) => i != index), voices);
        Assert.Same(index < 2 ? original[index + 1] : null, next);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ReplayReplacesVoiceInPlaceAndRetainsState(int index)
    {
        var replay = new VmMusicWildMidiNoteReleaseState { EnvelopeLevel = 123, SampleIncrement = 456 };
        var voices = new List<VmMusicWildMidiNoteReleaseState> { new() { Active = true }, new() { Active = true }, new() { Active = true } };
        var old = voices[index];
        old.Replay = replay;
        Assert.Same(replay, VmMusicWildMidiVoiceLifecycle.Complete(voices, index));
        Assert.False(old.Active);
        Assert.True(replay.Active);
        Assert.Same(replay, voices[index]);
        Assert.Same(replay, old.Replay);
        Assert.Equal(3, voices.Count);
        Assert.Equal(123, replay.EnvelopeLevel);
        Assert.Equal(456UL, replay.SampleIncrement);
    }

    [Fact]
    public void EnvelopeCompletionProcessesReplayAndSuccessorInSameFrame()
    {
        var replay = new VmMusicWildMidiNoteReleaseState { EnvelopeIncrement = 0 };
        var voice = new VmMusicWildMidiNoteReleaseState
        {
            Active = true, Envelope = 5, EnvelopeLevel = 1, EnvelopeIncrement = -2,
            EnvelopeTargets = new int[7], EnvelopeRates = new int[7], Replay = replay
        };
        var successor = new VmMusicWildMidiNoteReleaseState();
        var voices = new List<VmMusicWildMidiNoteReleaseState> { voice, successor };
        Assert.Equal(0, VmMusicWildMidiVoiceLifecycle.AdvanceEnvelope(voices, 0));
        Assert.Same(replay, voices[0]);
        Assert.Equal(1, VmMusicWildMidiVoiceLifecycle.AdvanceEnvelope(voices, 0));
        Assert.Same(successor, VmMusicWildMidiVoiceLifecycle.Complete(voices, 0));
        Assert.Equal(1, VmMusicWildMidiVoiceLifecycle.AdvanceEnvelope(voices, 0));
    }

    [Fact]
    public void InvalidOrReadOnlyListsFailBeforeChangingActiveState()
    {
        var voice = new VmMusicWildMidiNoteReleaseState { Active = true };
        var voices = new List<VmMusicWildMidiNoteReleaseState> { voice };
        Assert.Throws<ArgumentOutOfRangeException>(() => VmMusicWildMidiVoiceLifecycle.Complete(voices, 1));
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiVoiceLifecycle.Complete(voices.AsReadOnly(), 0));
        Assert.True(voice.Active);
        Assert.Same(voice, Assert.Single(voices));
    }
}
