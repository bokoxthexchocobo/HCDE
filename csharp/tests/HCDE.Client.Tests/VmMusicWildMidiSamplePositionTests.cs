using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiSamplePositionTests
{
    [Theory]
    [InlineData(9, 1, 10)]
    [InlineData(10, 1, 3)]
    [InlineData(10, 25, 3)]
    public void LoopEndIsInclusiveAndOvershootWrapsModuloSize(uint start, ulong increment, uint expected)
    {
        var voice = new VmMusicWildMidiNoteReleaseState { Modes = 4, SamplePosition = start, SampleIncrement = increment };
        Assert.False(VmMusicWildMidiSamplePosition.Advance(voice, new(20, 2, 10, 8), false));
        Assert.Equal(expected, voice.SamplePosition);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void NativeResamplersHaveDifferentExhaustionGates(bool enhanced, bool exhausted)
    {
        var voice = new VmMusicWildMidiNoteReleaseState { SamplePosition = 9, SampleIncrement = 1 };
        Assert.Equal(exhausted, VmMusicWildMidiSamplePosition.Advance(voice, new(10, 0, 10, 0), enhanced));
        Assert.Equal(10U, voice.SamplePosition);
    }

    [Fact]
    public void PositionAdditionUsesNativeUnsignedIntWidth()
    {
        var voice = new VmMusicWildMidiNoteReleaseState { SamplePosition = uint.MaxValue, SampleIncrement = 2 };
        Assert.False(VmMusicWildMidiSamplePosition.Advance(voice, new(20, 0, 0, 0), false));
        Assert.Equal(1U, voice.SamplePosition);
    }

    [Fact]
    public void ExhaustionReplacesVoiceBeforeEnvelopeCanAdvance()
    {
        var replay = new VmMusicWildMidiNoteReleaseState();
        var voice = new VmMusicWildMidiNoteReleaseState
        {
            Active = true, SamplePosition = 9, SampleIncrement = 1,
            EnvelopeLevel = 42, EnvelopeIncrement = 1, Replay = replay
        };
        var voices = new List<VmMusicWildMidiNoteReleaseState> { voice };
        Assert.Equal(0, VmMusicWildMidiVoiceLifecycle.AdvanceSample(voices, 0, new(10, 0, 0, 0), false));
        Assert.Equal(42, voice.EnvelopeLevel);
        Assert.False(voice.Active);
        Assert.True(replay.Active);
        Assert.Same(replay, Assert.Single(voices));
    }

    [Fact]
    public void ContinuingSampleAdvancesEnvelopeAfterPosition()
    {
        var voice = new VmMusicWildMidiNoteReleaseState
        {
            SampleIncrement = 1, EnvelopeLevel = 1, EnvelopeIncrement = 1,
            EnvelopeTargets = [10, 0, 0, 0, 0, 0, 0], EnvelopeRates = new int[7]
        };
        var voices = new List<VmMusicWildMidiNoteReleaseState> { voice };
        Assert.Equal(1, VmMusicWildMidiVoiceLifecycle.AdvanceSample(voices, 0, new(10, 0, 0, 0), false));
        Assert.Equal(1U, voice.SamplePosition);
        Assert.Equal(2, voice.EnvelopeLevel);
    }

    [Fact]
    public void InvalidLoopSizeFailsBeforePositionMutation()
    {
        var voice = new VmMusicWildMidiNoteReleaseState { Modes = 4, SamplePosition = 9, SampleIncrement = 2 };
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiSamplePosition.Advance(voice, new(20, 2, 10, 0), false));
        Assert.Equal(9U, voice.SamplePosition);
    }
}
