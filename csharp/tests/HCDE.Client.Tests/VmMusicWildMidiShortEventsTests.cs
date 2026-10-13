using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiShortEventsTests
{
    private static VmMusicWildMidiChannelState[] Channels() => Enumerable.Range(0, 16).Select(_ => new VmMusicWildMidiChannelState
    { Volume = 100, Expression = 127, Balance = 64, Pan = 64, PitchRange = 200 }).ToArray();

    [Fact]
    public void RawProgramNotePressurePitchAndReleaseReachConvertedPaths()
    {
        var bytes = new byte[337];
        "GF1PATCH110\0ID#000002\0"u8.CopyTo(bytes);
        bytes[198] = 1; bytes[247] = 2; bytes[259] = 1; bytes[269] = 1;
        var patch = new VmMusicWildMidiInstrumentPatch(7);
        patch.Load(bytes);
        var channels = Channels();
        var notes = new VmMusicWildMidiNoteTable();
        bool Send(int status, int a, int b) => VmMusicWildMidiShortEvents.Dispatch(notes, channels, id => id == 7 ? patch : null, status, a, b, 44100, 1024, false);
        Assert.True(Send(0xc2, 7, 999));
        Assert.True(Send(0x92, 60, 100));
        var voice = Assert.Single(notes.Voices);
        Assert.Equal(0x23c, voice.NoteId);
        Assert.True(Send(0xa2, 60, 50));
        Assert.Equal(50, voice.Velocity);
        Assert.True(Send(0xe2, 127, 127));
        Assert.Equal(200, channels[2].PitchAdjust);
        Assert.True(Send(0x92, 60, 0));
        Assert.Equal(0, voice.Modes & 4);
    }

    [Fact]
    public void ControllersAndOneByteEventsIgnoreUnusedParameter()
    {
        var channels = Channels();
        var notes = new VmMusicWildMidiNoteTable();
        Assert.True(VmMusicWildMidiShortEvents.Dispatch(notes, channels, _ => null, 0xb1, 7, 80, 44100, 1024, false));
        Assert.Equal(80, channels[1].Volume);
        Assert.True(VmMusicWildMidiShortEvents.Dispatch(notes, channels, _ => null, 0xd1, 50, -1, 44100, 1024, false));
    }

    [Fact]
    public void UnsupportedCommandsAndMalformedDataDoNotInsertVoices()
    {
        var channels = Channels();
        var notes = new VmMusicWildMidiNoteTable();
        Assert.False(VmMusicWildMidiShortEvents.Dispatch(notes, channels, _ => null, 0xf8, -1, -1, 44100, 1024, false));
        Assert.Throws<ArgumentOutOfRangeException>(() => VmMusicWildMidiShortEvents.Dispatch(notes, channels, _ => null, 0x90, 60, 128, 44100, 1024, false));
        Assert.Empty(notes.Voices);
    }
}
