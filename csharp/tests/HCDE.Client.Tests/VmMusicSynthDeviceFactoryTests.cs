using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicSynthDeviceFactoryTests
{
    [Fact]
    public void PlaybackFallbackTransfersWildMidiRendererToSession()
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000,
            [new(0, 0x643c90), new(120, 0x3c80)]));
        var renderer = new VmMusicWildMidiManagedRenderer(22050, _ => null, _ => 0);
        var router = VmMusicSynthDeviceFactory.CreateRouter(null, () => renderer);
        var selected = VmMusicMidiDeviceCreation.Create(VmMusicMidiDeviceKind.FluidSynth, false, router);
        Assert.Equal(VmMusicMidiDeviceKind.WildMidi, selected.Selected);
        var device = Assert.IsType<VmMusicWildMidiSessionDevice>(selected.Device);
        using (var session = new VmMusicMidiSession(source, device))
        {
            Assert.True(session.Start(600000));
            Assert.True(device.ChangeInteger("wildmidi.reverb", 1));
            for (var i = 0; i < 100 && device.IsOpen; i++)
            {
                device.ServiceStream(new float[2048]);
                session.Update();
            }
            Assert.False(device.IsOpen);
        }
        Assert.Throws<ObjectDisposedException>(() => renderer.GetVoiceCount());
    }

    [Fact]
    public void PlayEntryPointOwnsSelectedWildMidiAndPropagatesConfigurationFailure()
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000,
            [new(0, 0x643c90), new(120, 0x3c80)]));
        var renderer = new VmMusicWildMidiManagedRenderer(44100, _ => null, _ => 0);
        var router = VmMusicSynthDeviceFactory.CreateRouter(null, () => renderer);
        var playing = VmMusicMidiPlay.Start(source, VmMusicMidiDeviceKind.Default, -6, false, router, new(600000));
        Assert.True(playing.Started);
        Assert.Equal(VmMusicMidiDeviceKind.WildMidi, playing.Selected);
        playing.Session.Dispose();
        Assert.Throws<ObjectDisposedException>(() => renderer.GetVoiceCount());
        var failure = new FileNotFoundException("configuration");
        var broken = VmMusicSynthDeviceFactory.CreateRouter(null, () => throw failure);
        Assert.Same(failure, Assert.Throws<FileNotFoundException>(() =>
            VmMusicMidiDeviceCreation.Create(VmMusicMidiDeviceKind.WildMidi, false, broken)));
    }
}
