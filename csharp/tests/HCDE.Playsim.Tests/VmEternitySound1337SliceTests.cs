namespace HCDE.Playsim.Tests;

public class VmEternitySound1337SliceTests
{
    [Fact]
    public void RendererRecordsSpatialAndStaysSilent()
    {
        var host = new VmEternitySound1337();
        var renderer = host.Create();
        Assert.True(host.RegistrationAttempted);
        Assert.True(host.RendererConstructed);
        Assert.Same(renderer, host.Current);
        Assert.Equal(
            "snd_backend=eternity selected: using silent Eternity spatial facade (mixer not vendored yet).\n",
            host.Printed[0]);
        Assert.False(renderer.IsNull());
        Assert.True(renderer.IsValid());
        Assert.Null(renderer.LoadSound());
        Assert.Null(renderer.LoadSoundRaw());
        Assert.Equal(2, renderer.LoadedSounds);
        Assert.Equal(250u, renderer.GetMsLength());
        Assert.Equal(0u, renderer.GetSampleLength());
        Assert.Equal(44100f, renderer.GetOutputRate());
        Assert.Null(renderer.CreateStream());
        Assert.Equal(1, renderer.StreamRequests);
        Assert.Null(renderer.StartSound(0.5f, 1.5f, 3));
        Assert.Equal(1, renderer.Count2D);
        Assert.Equal(0.5f, renderer.LastVolume);
        Assert.Equal(1.5f, renderer.LastPitch);
        Assert.Equal(3, renderer.LastChannelFlags);
        Assert.Null(renderer.StartSound3D(0.25f, 2f, 0.75f, 9, 1.5, 2.5, 3.5, -1.25, 0.5, 4, 6, 11));
        Assert.Equal(1, renderer.Count3D);
        Assert.Equal(2f, renderer.LastDistanceScale);
        Assert.Equal(1.5f, renderer.LastPosX);
        Assert.Equal(2.5f, renderer.LastPosY);
        Assert.Equal(3.5f, renderer.LastPosZ);
        Assert.Equal(-1.25f, renderer.LastVelX);
        Assert.Equal(0.5f, renderer.LastVelY);
        Assert.Equal(4f, renderer.LastVelZ);
        Assert.Equal(6, renderer.LastChannel);
        Assert.Equal(11, renderer.LastChannelFlags);
        Assert.Equal(9, renderer.LastPriority);
        Assert.False(renderer.LastAreaSound);
        renderer.ChannelVolume(0.8f);
        Assert.Equal(0.8f, renderer.LastVolume);
        Assert.Equal(0.75f, renderer.LastPitch);
        renderer.ChannelPitch(1.25f);
        Assert.Equal(1.25f, renderer.LastPitch);
        renderer.UpdateSoundParams3D(true, 8, 9, 10, 0.25, 0.5, 0.75);
        Assert.Equal(1, renderer.ParamUpdates3D);
        Assert.True(renderer.LastAreaSound);
        Assert.Equal(8f, renderer.LastPosX);
        Assert.Equal(9f, renderer.LastPosY);
        Assert.Equal(10f, renderer.LastPosZ);
        Assert.Equal(0.25f, renderer.LastVelX);
        Assert.Equal(0.5f, renderer.LastVelY);
        Assert.Equal(0.75f, renderer.LastVelZ);
        Assert.Equal(6, renderer.LastChannel);
        renderer.UpdateListener();
        Assert.Equal(1, renderer.ListenerUpdates);
        renderer.UpdateSounds();
        renderer.UpdateSounds();
        Assert.Equal(2, renderer.MixTicks);
        renderer.Sync(true);
        Assert.True(renderer.SyncActive);
        renderer.Sync(false);
        Assert.False(renderer.SyncActive);
        renderer.SetSfxPaused(true);
        Assert.True(renderer.SfxPaused);
        renderer.SetInactive(VmEternitySound1337.InactiveState.Mute);
        Assert.Equal(VmEternitySound1337.InactiveState.Mute, renderer.Inactive);
        renderer.SetSfxVolume(0.4f);
        renderer.SetMusicVolume(0.2f);
        renderer.UpdateMusicParams();
        renderer.UnloadSound();
        renderer.StopChannel();
        renderer.MarkStartTime();
        Assert.Equal(2, renderer.LoadedSounds);
        Assert.Equal(1, renderer.Count2D);
        Assert.Equal(0.4f, renderer.SfxVolume);
        Assert.Equal(0.2f, renderer.MusicVolume);
        Assert.Equal(0u, renderer.GetPosition());
        Assert.Equal(0f, renderer.GetAudibility());
        renderer.PrintDriversList();
        renderer.PrintStatus();
        Assert.Equal("Eternity spatial audio facade has no platform drivers yet.\n", host.Printed[1]);
        Assert.Equal("Eternity spatial audio facade active (silent mixer placeholder).\n", host.Printed[2]);
        Assert.Equal(
            "  loaded=2 2d-starts=1 3d-starts=1 listener-updates=1 param-updates=1 mix-ticks=2\n",
            host.Printed[3]);
        Assert.Equal(
            "Eternity facade: loaded=2 2d=1 3d=1 listener=1 params=1 streams=1 mix=2",
            renderer.GatherStats());
    }

    [Fact]
    public void ProbeStatusAndStubReplaceTheRenderer()
    {
        var host = new VmEternitySound1337();
        host.Probe();
        Assert.Equal(1, host.DiagnosticTriggers);
        Assert.Null(host.Current);
        Assert.False(host.RegistrationAttempted);
        Assert.False(host.RendererConstructed);
        host.Status(null);
        Assert.Equal(
            new[]
            {
                "Eternity spatial audio diagnostic trigger submitted.\n",
                "Eternity spatial audio backend: not selected\n",
                "  snd_backend = <empty>\n",
                "  registration attempted = no\n",
                "  renderer constructed = no\n",
                "  implementation = silent facade (Eternity mixer not vendored yet)\n",
                "  diagnostic triggers = 1\n",
                "  boundary = client-local audio backend; no snapshots, saves, demos, or authority state\n",
                "  note: Eternity facade is silent; use snd_backend=openal for full audio with snd_env_reverb support\n",
            },
            host.Printed);

        host.Printed.Clear();
        host.Status("");
        Assert.Equal("  snd_backend = <empty>\n", host.Printed[1]);
        host.Printed.Clear();
        host.Status("OpenAL");
        Assert.Equal("Eternity spatial audio backend: not selected\n", host.Printed[0]);
        Assert.Equal("  snd_backend = OpenAL\n", host.Printed[1]);

        var first = host.Create();
        var stub = host.CreateStub();
        Assert.NotSame(first, stub);
        Assert.Same(stub, host.Current);
        Assert.True(host.RendererConstructed);
        Assert.Equal(0, first.Count2D);
        host.Probe();
        Assert.Equal(2, host.DiagnosticTriggers);
        Assert.Equal(1, stub.Count2D);
        Assert.Equal(1f, stub.LastVolume);
        Assert.Equal(1f, stub.LastPitch);
        Assert.Equal(0, stub.LastChannelFlags);
        Assert.Equal(-1, stub.LastChannel);
        Assert.Equal(0, first.Count2D);

        host.Printed.Clear();
        host.Status("ETERNITY");
        Assert.Equal(
            new[]
            {
                "Eternity spatial audio backend: selected\n",
                "  snd_backend = ETERNITY\n",
                "  registration attempted = yes\n",
                "  renderer constructed = yes\n",
                "  implementation = silent facade (Eternity mixer not vendored yet)\n",
                "  diagnostic triggers = 2\n",
                "  loaded sounds = 0\n",
                "  2D starts = 1, 3D starts = 0\n",
                "  listener updates = 0, 3D param updates = 0, mix ticks = 0\n",
                "  last spatial submit: pos=(0.00 0.00 0.00) vel=(0.00 0.00 0.00) vol=1.000 pitch=1.000 channel=-1 flags=0 priority=0 area=0\n",
                "  boundary = client-local audio backend; no snapshots, saves, demos, or authority state\n",
                "  note: Eternity facade is silent; use snd_backend=openal for full audio with snd_env_reverb support\n",
            },
            host.Printed);
    }
}
