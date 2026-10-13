using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicSynthWaveDumpTests
{
    [Theory]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    public void ExplicitSelectionReachesOnlyChosenFactory(int device)
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000, []));
        var failure = new InvalidOperationException("selected backend");
        var router = VmMusicSynthWaveDump.CreateRouter(VmMusicMidiDeviceKind.WildMidi,
            request => { Assert.NotEqual(6, device); Assert.Equal(device, request.Device); throw failure; },
            request => { Assert.Equal(6, device); Assert.Equal(device, request.Device); throw failure; });
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => router(source,
            new(device, "bank", "unused.wav", 0, 22050))));
    }

    [Fact]
    public void UnavailableAndUnsupportedDevicesFailWithoutAllocating()
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000, []));
        var router = VmMusicSynthWaveDump.CreateRouter(VmMusicMidiDeviceKind.WildMidi, null, null);
        Assert.Throws<VmMusicMidiDeviceUnavailableException>(() => router(source, new(-1, null, "unused.wav", 0, 0)));
        Assert.Throws<NotSupportedException>(() => router(source, new(4, null, "unused.wav", 0, 0)));
    }

    [Fact]
    public void ConsoleDefaultRoutesToWildMidiAndRestoresPlayback()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
        var bytes = VmMusicSmf.Serialize(480, 500000, [new(0, 0x643c90), new(120, 0x3c80)]);
        var restored = false;
        var console = new VmMusicConsole<object>(new("old", null), new([]), () => true,
            (_, _) => true, _ => { }, _ => { }, _ => Assert.Fail("unexpected report"));
        console.ConfigureSynthWaveExport(-1, _ => 0, _ => -1, _ => bytes,
            VmMusicMidiDeviceKind.WildMidi, _ => throw new InvalidOperationException("unexpected FM"),
            request => { Assert.Equal(6, request.Device); return new(44100, _ => null, _ => 0); },
            (name, _, _, _) => { Assert.Equal("old", name); restored = true; return true; });
        try
        {
            Assert.True(console.TryExecute(["writewave", "song", path]));
            Assert.True(restored);
            Assert.True(new FileInfo(path).Length > 68);
        }
        finally { File.Delete(path); }
    }
}
