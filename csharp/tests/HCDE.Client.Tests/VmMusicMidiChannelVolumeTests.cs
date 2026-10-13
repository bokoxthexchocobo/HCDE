namespace HCDE.Client.Tests;

public class VmMusicMidiChannelVolumeTests
{
    [Theory]
    [InlineData(0, 127, 0)]
    [InlineData(65535, 0, 0)]
    [InlineData(65535, 100, 100)]
    [InlineData(65535, 127, 127)]
    [InlineData(32768, 100, 50)]
    [InlineData(32768, 127, 64)]
    [InlineData(1, 127, 0)]
    public void ControllerScalingUsesNativeFixedPointRounding(ushort master, byte raw, byte expected)
    {
        var state = new VmMusicMidiChannelVolume { MasterVolume = master };
        Assert.Equal(expected, state.ControllerChange(9, raw));
        Assert.Equal(raw, state.GetChannelVolume(9));
    }

    [Fact]
    public void ExportPreservesRawValueAndStillUpdatesChannelState()
    {
        var state = new VmMusicMidiChannelVolume { MasterVolume = 0 };
        Assert.Equal(99, state.ControllerChange(2, 99, exporting: true));
        Assert.Equal(99, state.GetChannelVolume(2));
        var item = new VmMusicSmfEvent(8, 0x6307b2);
        Assert.Equal(item, state.Apply(item, exporting: true));
        Assert.Equal(new VmMusicSmfEvent(8, 0x0007b2), state.Apply(item));
    }

    [Fact]
    public void InitialSetupRestoresChannelsWithoutChangingMasterVolume()
    {
        var state = new VmMusicMidiChannelVolume { MasterVolume = 20000 };
        state.ControllerChange(0, 5);
        state.ControllerChange(15, 127);
        state.Initialize();
        Assert.Equal(20000, state.MasterVolume);
        Assert.All(Enumerable.Range(0, 16), channel => Assert.Equal(100, state.GetChannelVolume(channel)));
    }

    [Fact]
    public void MasterChangeBurstUsesStoredRawVolumesAndOwnsItsSnapshot()
    {
        var state = new VmMusicMidiChannelVolume { MasterVolume = 32768 };
        state.ControllerChange(0, 0);
        state.ControllerChange(15, 127);
        var events = state.CreateVolumeChanges();
        Assert.Equal(16, events.Count);
        Assert.Equal(0x0007b0u, events[0].Event);
        Assert.Equal(0x4007bfu, events[15].Event);
        Assert.All(events, item => Assert.Equal(0u, item.Delay));
        state.MasterVolume = 0;
        Assert.Equal(0x4007bfu, events[15].Event);
        Assert.All(state.CreateVolumeChanges(), item => Assert.Equal(0u, item.Event >> 16));
        Assert.Equal(127, state.GetChannelVolume(15));
    }

    [Fact]
    public void OtherControllersNotesTempoAndLongMessagesPassThrough()
    {
        var state = new VmMusicMidiChannelVolume { MasterVolume = 0 };
        VmMusicSmfEvent[] events = [new(1, 0x643cb0), new(2, 0x643c90), new(3, 0x0107a120),
            new(4, 0x80000003, new byte[] { 0xf0, 7, 0xf7 }), new(5, 0x02c0)];
        foreach (var item in events) Assert.Equal(item, state.Apply(item));
        Assert.Equal(100, state.GetChannelVolume(0));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(16)]
    public void InvalidChannelsAndVolumesFailBeforeUpdatingState(int channel)
    {
        var state = new VmMusicMidiChannelVolume();
        Assert.Throws<ArgumentOutOfRangeException>(() => state.GetChannelVolume(channel));
        Assert.Throws<ArgumentOutOfRangeException>(() => state.ControllerChange(channel, 100));
        Assert.Throws<ArgumentOutOfRangeException>(() => state.ControllerChange(0, 128));
        Assert.Equal(100, state.GetChannelVolume(0));
    }
}
