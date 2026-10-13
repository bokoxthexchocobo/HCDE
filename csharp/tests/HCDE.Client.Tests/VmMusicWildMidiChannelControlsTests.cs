namespace HCDE.Client.Tests;

public class VmMusicWildMidiChannelControlsTests
{
    [Fact]
    public void VoiceControllerDispatchConnectsHoldNotesOffAndPedalRelease()
    {
        var channel = new VmMusicWildMidiChannelState();
        var voice = new VmMusicWildMidiNoteReleaseState
        {
            NoteId = 0x13c, Active = true, Modes = 64,
            EnvelopeTargets = [0, 0, 0, 0, 10, 10], EnvelopeRates = [0, 0, 0, 0, 4, 5]
        };
        void Apply(int controller, int value) => Assert.True(VmMusicWildMidiChannelControls.Apply(channel,
            [voice], 1, controller, value, () => Assert.Fail("unexpected adjustment")));
        Apply(64, 127);
        Assert.Equal(1, channel.Hold);
        voice.Hold = channel.Hold;
        Apply(123, 0);
        Assert.Equal(3, voice.Hold);
        Apply(64, 0);
        Assert.Equal(0, voice.Hold); Assert.Equal(4, voice.Envelope);
        Apply(120, 0);
        Assert.False(voice.Active);
    }

    [Fact]
    public void DrumNotesOffIsHandledWithoutChangingVoiceAndBasicControlsUseSharedDispatcher()
    {
        var channel = new VmMusicWildMidiChannelState { IsDrum = true };
        var voice = new VmMusicWildMidiNoteReleaseState { NoteId = 0x93c, Active = true, Hold = 1 };
        var adjustments = 0;
        Assert.True(VmMusicWildMidiChannelControls.Apply(channel, [voice], 9, 123, 0, () => adjustments++));
        Assert.Equal(1, voice.Hold);
        Assert.True(VmMusicWildMidiChannelControls.Apply(channel, [voice], 9, 7, 100, () => adjustments++));
        Assert.Equal(100, channel.Volume); Assert.Equal(1, adjustments);
        Assert.False(VmMusicWildMidiChannelControls.Apply(channel, [voice], 9, 127, 0, () => adjustments++));
        Assert.Equal(1, adjustments);
    }

    [Fact]
    public void ControllerResetChangesNativeFieldsBeforeVolumeAdjustmentAndPreservesOthers()
    {
        var channel = new VmMusicWildMidiChannelState
        {
            Bank = 5, Volume = 6, Balance = 7, Pan = 8, NonRegistered = 1,
            Expression = 9, Pressure = 10, RegisteredParameter = 11, PitchRange = 12,
            Pitch = 13, PitchAdjust = 14, Hold = 1
        };
        var adjustments = 0;
        Assert.True(VmMusicWildMidiChannelControls.Apply(channel, 121, 99, () =>
        {
            Assert.Equal(127, channel.Expression); Assert.Equal(127, channel.Pressure);
            Assert.Equal(0xffff, channel.RegisteredParameter); Assert.Equal(200, channel.PitchRange);
            Assert.Equal(0, channel.Pitch); Assert.Equal(0, channel.PitchAdjust); Assert.Equal(0, channel.Hold);
            adjustments++;
        }));
        Assert.Equal(1, adjustments);
        Assert.Equal(5, channel.Bank); Assert.Equal(6, channel.Volume);
        Assert.Equal(7, channel.Balance); Assert.Equal(8, channel.Pan); Assert.Equal(1, channel.NonRegistered);
    }

    [Theory]
    [InlineData(98, 1)]
    [InlineData(100, 0)]
    public void FineParameterSelectionPreservesNativeMask(int controller, byte mode)
    {
        var channel = new VmMusicWildMidiChannelState { RegisteredParameter = 0xffff, NonRegistered = 1 };
        VmMusicWildMidiChannelControls.Apply(channel, controller, 5, () => Assert.Fail("unexpected adjustment"));
        Assert.Equal(0x3f85, channel.RegisteredParameter);
        Assert.Equal(mode, channel.NonRegistered);
    }

    [Theory]
    [InlineData(99, 1)]
    [InlineData(101, 0)]
    public void CoarseParameterSelectionPreservesLowSevenBits(int controller, byte mode)
    {
        var channel = new VmMusicWildMidiChannelState { RegisteredParameter = 0xffff };
        VmMusicWildMidiChannelControls.Apply(channel, controller, 2, () => Assert.Fail("unexpected adjustment"));
        Assert.Equal(0x17f, channel.RegisteredParameter);
        Assert.Equal(mode, channel.NonRegistered);
    }

    [Fact]
    public void SelectingPitchRangeEnablesDataEntryAndIncrementDecrementBounds()
    {
        var channel = new VmMusicWildMidiChannelState { RegisteredParameter = 0xffff, NonRegistered = 1 };
        void Apply(int controller, int value) => VmMusicWildMidiChannelControls.Apply(channel, controller, value,
            () => Assert.Fail("unexpected adjustment"));
        Apply(101, 0); Apply(100, 0); Apply(6, 2); Apply(38, 50);
        Assert.Equal(250, channel.PitchRange);
        Apply(96, 127); Assert.Equal(251, channel.PitchRange);
        Apply(97, 0); Assert.Equal(250, channel.PitchRange);
        channel.PitchRange = 0x3fff; Apply(96, 0); Assert.Equal(0x3fff, channel.PitchRange);
        channel.PitchRange = 0; Apply(97, 0); Assert.Equal(0, channel.PitchRange);
        Apply(99, 0); Apply(96, 0); Assert.Equal(0, channel.PitchRange);
    }

    [Theory]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(10)]
    [InlineData(11)]
    public void VolumeControlsNarrowThenAdjustExactlyOnce(int control)
    {
        var channel = new VmMusicWildMidiChannelState();
        var adjustments = 0;
        Assert.True(VmMusicWildMidiChannelControls.Apply(channel, control, 255, () =>
        {
            Assert.Equal(control == 7 ? 255 : 0, channel.Volume);
            Assert.Equal(control == 11 ? 255 : 0, channel.Expression);
            Assert.Equal(control == 8 ? -1 : 0, channel.Balance);
            Assert.Equal(control == 10 ? -1 : 0, channel.Pan);
            adjustments++;
        }));
        Assert.Equal(1, adjustments);
    }

    [Fact]
    public void BankAndUnknownControlsDoNotAdjustVolume()
    {
        var channel = new VmMusicWildMidiChannelState();
        Assert.True(VmMusicWildMidiChannelControls.Apply(channel, 0, 300, () => Assert.Fail("unexpected adjustment")));
        Assert.Equal(44, channel.Bank);
        Assert.False(VmMusicWildMidiChannelControls.Apply(channel, 127, 0, () => Assert.Fail("unexpected adjustment")));
    }

    [Fact]
    public void PitchRangeCoarseAndFinePreserveOtherComponentAndNarrowToShort()
    {
        var channel = new VmMusicWildMidiChannelState { PitchRange = 250 };
        VmMusicWildMidiChannelControls.Apply(channel, 6, 12, () => Assert.Fail("unexpected adjustment"));
        Assert.Equal(1250, channel.PitchRange);
        VmMusicWildMidiChannelControls.Apply(channel, 38, 75, () => Assert.Fail("unexpected adjustment"));
        Assert.Equal(1275, channel.PitchRange);
        VmMusicWildMidiChannelControls.Apply(channel, 6, 400, () => Assert.Fail("unexpected adjustment"));
        Assert.Equal(unchecked((short)40075), channel.PitchRange);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(0, 1)]
    public void UnselectedPitchRangeParameterIsIgnored(byte nonRegistered, ushort parameter)
    {
        var channel = new VmMusicWildMidiChannelState
            { NonRegistered = nonRegistered, RegisteredParameter = parameter, PitchRange = 250 };
        VmMusicWildMidiChannelControls.Apply(channel, 6, 12, () => Assert.Fail("unexpected adjustment"));
        VmMusicWildMidiChannelControls.Apply(channel, 38, 75, () => Assert.Fail("unexpected adjustment"));
        Assert.Equal(250, channel.PitchRange);
    }
}
