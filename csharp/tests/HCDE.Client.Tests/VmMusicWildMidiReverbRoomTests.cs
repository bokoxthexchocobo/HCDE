using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiReverbRoomTests
{
    [Fact]
    public void DefaultRoomProducesDelayedImpulseAndIsChunkIndependent()
    {
        var whole = new int[12000];
        whole[0] = 4096;
        var chunks = (int[])whole.Clone();
        VmMusicWildMidiReverbRoom.Create(44100, new()).Process(whole);
        var room = VmMusicWildMidiReverbRoom.Create(44100, new());
        room.Process(chunks.AsSpan(0, 200));
        room.Process(chunks.AsSpan(200));
        Assert.Equal(whole, chunks);
        Assert.Equal(4096, whole[0]);
        Assert.Equal(0, whole[1]);
        Assert.Contains(whole.Skip(2), value => value != 0);
    }

    [Fact]
    public void ExtremeSettingsAndAllSupportedRatesInitializeSafely()
    {
        foreach (var rate in new[] { 11025, 44100, 65535 })
            foreach (var dimension in new[] { 1, 100 })
            {
                var settings = new VmMusicWildMidiReverbSettings();
                settings.Apply($"reverb_room_width {dimension}");
                settings.Apply($"reverb_room_length {dimension}");
                settings.Apply("reverb_listener_posx 0");
                settings.Apply("reverb_listener_posy 0");
                var room = VmMusicWildMidiReverbRoom.Create(rate, settings);
                int[] silence = new int[200];
                room.Process(silence);
                Assert.All(silence, value => Assert.Equal(0, value));
            }
        Assert.Throws<ArgumentOutOfRangeException>(() => VmMusicWildMidiReverbRoom.Create(0, new()));
    }
}
