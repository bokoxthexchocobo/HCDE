using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiEnvelopeTests
{
    private static VmMusicWildMidiNoteReleaseState Voice(int stage = 0) => new()
    {
        Active = true, Modes = VmMusicWildMidiNoteRelease.Envelope | VmMusicWildMidiNoteRelease.Loop,
        Envelope = stage, EnvelopeLevel = 8, EnvelopeIncrement = 3,
        EnvelopeTargets = [10, 20, 10, 5, 0, 0, 0], EnvelopeRates = [3, 2, 2, 1, 2, 4, 8]
    };

    [Fact]
    public void AttackClampsAndConsumesDeferredNoteOff()
    {
        var voice = Voice();
        Assert.True(VmMusicWildMidiNoteRelease.NoteOff(voice, new(), false));
        Assert.False(VmMusicWildMidiEnvelope.Advance(voice));
        Assert.Equal(10, voice.EnvelopeLevel);
        Assert.Equal(4, voice.Envelope);
        Assert.Equal(-2, voice.EnvelopeIncrement);
        Assert.False(voice.IsOff);
    }

    [Fact]
    public void HeldDeferredReleaseContinuesAttackAndMarksPedalRelease()
    {
        var voice = Voice();
        voice.Hold = 1;
        voice.IsOff = true;
        Assert.False(VmMusicWildMidiEnvelope.Advance(voice));
        Assert.Equal(1, voice.Envelope);
        Assert.Equal(2, voice.EnvelopeIncrement);
        Assert.Equal(3, voice.Hold);
        Assert.False(voice.IsOff);
    }

    [Theory]
    [InlineData(32, 2, 0)]
    [InlineData(128, 5, -4)]
    [InlineData(160, 2, 0)]
    public void SustainAndClampedStageTransitionsMatchNative(int modes, int stage, int increment)
    {
        var voice = Voice(2);
        voice.Modes |= modes;
        Assert.False(VmMusicWildMidiEnvelope.Advance(voice));
        Assert.Equal(stage, voice.Envelope);
        Assert.Equal(increment, voice.EnvelopeIncrement);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(6)]
    public void TerminalStagesSignalMixerWithoutMutatingVoiceOwnership(int stage)
    {
        var voice = Voice(stage);
        voice.EnvelopeIncrement = -10;
        voice.Replay = new();
        Assert.True(VmMusicWildMidiEnvelope.Advance(voice));
        Assert.Equal(0, voice.EnvelopeLevel);
        Assert.True(voice.Active);
        Assert.NotNull(voice.Replay);
    }

    [Fact]
    public void NonzeroReleaseTargetStopsLooping()
    {
        var voice = Voice(5);
        voice.EnvelopeTargets[5] = 10;
        Assert.False(VmMusicWildMidiEnvelope.Advance(voice));
        Assert.Equal(0, voice.Modes & VmMusicWildMidiNoteRelease.Loop);
        Assert.Equal(0, voice.EnvelopeIncrement);
    }

    [Fact]
    public void ProgressAndStoppedEnvelopesDoNotAdvanceStage()
    {
        var voice = Voice();
        voice.EnvelopeIncrement = 1;
        Assert.False(VmMusicWildMidiEnvelope.Advance(voice));
        Assert.Equal(9, voice.EnvelopeLevel);
        Assert.Equal(0, voice.Envelope);
        voice.EnvelopeIncrement = 0;
        Assert.False(VmMusicWildMidiEnvelope.Advance(voice));
        Assert.Equal(9, voice.EnvelopeLevel);
    }
}
