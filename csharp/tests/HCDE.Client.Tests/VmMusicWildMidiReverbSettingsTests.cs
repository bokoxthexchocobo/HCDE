using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiReverbSettingsTests
{
    [Fact]
    public void DefaultsAndRoomClampsMatchNativeValues()
    {
        var settings = new VmMusicWildMidiReverbSettings();
        Assert.Equal(16.875f, settings.RoomWidth);
        Assert.Equal(22.5f, settings.RoomLength);
        Assert.Equal(8.4375f, settings.ListenerX);
        Assert.Equal(16.875f, settings.ListenerY);
        settings.Apply("REVERB_ROOM_WIDTH 0tail");
        settings.Apply("reverb_room_length 1e999");
        Assert.Equal(1f, settings.RoomWidth);
        Assert.Equal(100f, settings.RoomLength);
        settings.Apply("reverb_room_width 12.5");
        Assert.Equal(12.5f, settings.RoomWidth);
    }

    [Fact]
    public void ListenerYUsesNativeWidthComparisonAndLengthFallback()
    {
        var settings = new VmMusicWildMidiReverbSettings();
        settings.Apply("reverb_room_width 10");
        settings.Apply("reverb_room_length 40");
        settings.Apply("reverb_listener_posx 11");
        settings.Apply("reverb_listener_posy 11");
        Assert.Equal(5f, settings.ListenerX);
        Assert.Equal(30f, settings.ListenerY);
        settings.Apply("reverb_listener_posy 10");
        settings.Apply("reverb_room_width 1");
        Assert.Equal(10f, settings.ListenerY);
    }

    [Fact]
    public void IncludedSettingsAreSharedAndInvalidValuesLeaveStateUnchanged()
    {
        var catalog = new VmMusicWildMidiInstrumentCatalog(_ => null);
        new VmMusicWildMidiConfigReader(catalog, "config.cfg", true, _ => "reverb_room_width 25\nreverb_listener_posx 20")
            .Read("source child.cfg\nreverb_room_length 50");
        Assert.Equal(25f, catalog.Reverb.RoomWidth);
        Assert.Equal(20f, catalog.Reverb.ListenerX);
        Assert.Equal(50f, catalog.Reverb.RoomLength);
        foreach (var line in new[] { "reverb_room_width", "reverb_room_width -1", "reverb_room_width .5", "reverb_room_width x" })
            Assert.Throws<InvalidDataException>(() => catalog.Reverb.Apply(line));
        Assert.Equal(25f, catalog.Reverb.RoomWidth);
        Assert.False(catalog.Reverb.Apply("bank 1"));
    }
}
