using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiNoteSlotsTests
{
    [Fact]
    public void InactivePairAppendsPrimaryAndActivatesIt()
    {
        var existing = new VmMusicWildMidiNoteReleaseState();
        var primary = new VmMusicWildMidiNoteReleaseState();
        var voices = new List<VmMusicWildMidiNoteReleaseState> { existing };
        Assert.Same(primary, VmMusicWildMidiNoteSlots.Select(voices, primary, new()));
        Assert.Equal(new[] { existing, primary }, voices);
        Assert.True(primary.Active);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ActiveSlotSchedulesOtherSlotWithoutAppending(bool primaryActive)
    {
        var primary = new VmMusicWildMidiNoteReleaseState { Active = primaryActive, EnvelopeRates = [0, 0, 0, 0, 0, 0, 10] };
        var secondary = new VmMusicWildMidiNoteReleaseState { Active = !primaryActive, EnvelopeRates = [0, 0, 0, 0, 0, 0, 20] };
        var current = primaryActive ? primary : secondary;
        var destination = primaryActive ? secondary : primary;
        var voices = new List<VmMusicWildMidiNoteReleaseState> { current };
        Assert.Same(destination, VmMusicWildMidiNoteSlots.Select(voices, primary, secondary));
        Assert.Same(destination, current.Replay);
        Assert.Equal(6, current.Envelope);
        Assert.Equal(primaryActive ? -10 : -20, current.EnvelopeIncrement);
        Assert.Same(current, Assert.Single(voices));
        Assert.False(destination.Active);
        Assert.Same(destination, VmMusicWildMidiVoiceLifecycle.Complete(voices, 0));
        Assert.True(destination.Active);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EarlyEnvelopeSuppressionUsesPrimaryMetadata(bool primaryActive)
    {
        var primary = new VmMusicWildMidiNoteReleaseState { Active = primaryActive, Modes = 64, Envelope = 2 };
        var secondary = new VmMusicWildMidiNoteReleaseState { Active = !primaryActive, Envelope = 5 };
        var voices = new List<VmMusicWildMidiNoteReleaseState>();
        Assert.Null(VmMusicWildMidiNoteSlots.Select(voices, primary, secondary));
        Assert.Null(primary.Replay);
        Assert.Null(secondary.Replay);
        Assert.Equal(5, secondary.Envelope);
    }

    [Fact]
    public void HeldOffBitAllowsRetriggerDuringAttack()
    {
        var primary = new VmMusicWildMidiNoteReleaseState { Active = true, Modes = 64, Envelope = 0, Hold = 3, EnvelopeRates = new int[7] };
        var secondary = new VmMusicWildMidiNoteReleaseState();
        Assert.Same(secondary, VmMusicWildMidiNoteSlots.Select(new List<VmMusicWildMidiNoteReleaseState> { primary }, primary, secondary));
        Assert.Equal(6, primary.Envelope);
    }

    [Fact]
    public void MissingTerminalRateFailsBeforeReplayMutation()
    {
        var primary = new VmMusicWildMidiNoteReleaseState { Active = true, Envelope = 4 };
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiNoteSlots.Select(new List<VmMusicWildMidiNoteReleaseState> { primary }, primary, new()));
        Assert.Null(primary.Replay);
        Assert.Equal(4, primary.Envelope);
    }
}
