using System.Buffers.Binary;
using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiRendererVolumeTests
{
    private sealed class Output : IVmMusicOutputStream
    {
        public void Play(bool loop, float volume) { }
        public void SetPaused(bool paused) { }
        public void Stop() { }
        public void Dispose() { }
    }

    [Fact]
    public void HostStatisticsReportLiveVoicesThroughPlaybackState()
    {
        var patch = Patch();
        var renderer = new VmMusicWildMidiManagedRenderer(44100, _ => patch, _ => 0);
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000,
            [new(0, 0x643c90), new(120, 0x3c80)]));
        var router = VmMusicSynthDeviceFactory.CreateRouter(null, () => renderer);
        VmMusicFloatFill? callback = null;
        using var playback = VmMusicSynthStreamPlayback.Start(source, VmMusicMidiDeviceKind.WildMidi,
            -6, false, kind => Assert.IsAssignableFrom<IVmMusicAudioSessionDevice>(router(kind)), new(600000),
            (_, fill) => { callback = fill; return new Output(); });
        var state = new VmMusicPlaybackState<VmMusicSynthStreamPlayback>("song", playback);
        Assert.Equal("  0 voices", state.GetStatistics(song => song.GetStatistics()));
        Assert.True(callback!(new float[2]));
        Assert.Equal("  1 voices", state.GetStatistics(song => song.GetStatistics()));
        playback.Pause(true);
        Assert.True(callback(new float[128]));
        Assert.Equal("  1 voices", playback.GetStatistics());
        playback.Dispose();
        Assert.Throws<ObjectDisposedException>(() => playback.GetStatistics());
    }

    [Fact]
    public void InitialHostGainAppliesBeforeOutputCreationCallback()
    {
        static float[] Render(float gain)
        {
            var patch = Patch();
            using var renderer = new VmMusicWildMidiManagedRenderer(44100, _ => patch, _ => 0);
            using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000,
                [new(0, 0x643c90), new(120, 0x3c80)]));
            var samples = new float[128];
            using var playback = VmMusicSynthStreamPlayback.Start(source, VmMusicMidiDeviceKind.WildMidi,
                -6, false, _ => new VmMusicWildMidiSessionDevice(renderer), new(600000),
                (_, fill) => { Assert.True(fill(samples)); return new Output(); }, gain);
            Assert.Equal(gain, playback.OutputVolume);
            return samples;
        }
        var unity = Render(1);
        Assert.Contains(unity, value => value != 0);
        Assert.Equal(unity.Select(value => value * 0.5f), Render(0.5f));
        Assert.Equal(unity.Select(value => value * 2), Render(2));
        Assert.All(Render(0), value => Assert.Equal(0, value));
    }

    [Theory]
    [InlineData(null, 0.25f)]
    [InlineData(0f, 0f)]
    [InlineData(0.5f, 0.5f)]
    [InlineData(2f, 2f)]
    public void ResolvedSongSettingsSelectInitialGainBeforeDeviceAndOutputCreation(float? volume, float expected)
    {
        float[] Render(float? configuredVolume, float expectedGain, out int replayCalls)
        {
            var patch = Patch();
            using var renderer = new VmMusicWildMidiManagedRenderer(44100, _ => patch, _ => 0);
            using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000,
                [new(0, 0x643c90), new(120, 0x3c80)]));
            var calls = 0;
            var samples = new float[128];
            var settings = VmMusicResourceSettings.Resolve(42, 0,
                new Dictionary<int, VmMusicDeviceSetting> { [42] = new(6, "song-bank") },
                new Dictionary<int, int>(), configuredVolume is { } value
                    ? new Dictionary<int, float> { [42] = value } : new Dictionary<int, float>());
            using var playback = VmMusicSynthStreamPlayback.StartConfigured(source,
                settings, -7, false,
                (kind, arguments) =>
                {
                    Assert.Equal(VmMusicMidiDeviceKind.WildMidi, kind);
                    Assert.Equal("song-bank", arguments);
                    Assert.Equal(configuredVolume is null ? 1 : 0, calls);
                    return new VmMusicWildMidiSessionDevice(renderer);
                }, new(600000), (_, fill) => { Assert.True(fill(samples)); return new Output(); },
                (device, arguments) =>
                {
                    Assert.Equal(6, device);
                    Assert.Equal("song-bank", arguments);
                    calls++;
                    return expectedGain;
                });
            replayCalls = calls;
            Assert.Equal(expectedGain, playback.OutputVolume);
            return samples;
        }
        var unity = Render(1, 1, out _);
        Assert.Contains(unity, value => value != 0);
        var actual = Render(volume, expected, out var replayCalls);
        Assert.Equal(volume is null ? 1 : 0, replayCalls);
        Assert.Equal(unity.Select(value => value * expected), actual);
    }

    private static VmMusicWildMidiInstrumentPatch Patch()
    {
        var bytes = new byte[239 + 96 + 512];
        "GF1PATCH110\0ID#000002\0"u8.CopyTo(bytes);
        bytes[198] = 1;
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(247), 512);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(259), 22050);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(269), 440000);
        Array.Fill(bytes, (byte)64, 335, 512);
        var patch = new VmMusicWildMidiInstrumentPatch(0);
        patch.Load(bytes);
        return patch;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ConcreteRendererProducesNonzeroDecodedPatchAudio(bool logarithmic)
    {
        var patch = Patch();
        using var renderer = new VmMusicWildMidiManagedRenderer(44100, _ => patch, _ => 0);
        renderer.LoadInstrument(0, 0, 0);
        renderer.SetOption(1, logarithmic ? 1 : 0);
        renderer.ShortEvent(0x90, 60, 100);
        var output = new float[128];
        renderer.ComputeOutput(output, 64);
        Assert.Contains(output, value => value > 0);
        Assert.All(output, value => Assert.True(float.IsFinite(value)));
        Assert.Equal(1, renderer.GetVoiceCount());
    }

    [Fact]
    public void MasterVolumeAffectsNewVoicesAndOptionValidationIsExplicit()
    {
        var patch = Patch();
        using var renderer = new VmMusicWildMidiManagedRenderer(44100, _ => patch, _ => 0);
        renderer.SetMasterVolume(0);
        renderer.ShortEvent(0x90, 60, 100);
        var output = new float[128];
        renderer.ComputeOutput(output, 64);
        Assert.All(output, value => Assert.Equal(0f, value));
        Assert.Throws<ArgumentOutOfRangeException>(() => renderer.SetOption(0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => renderer.SetOption(1, 8));
    }
}
