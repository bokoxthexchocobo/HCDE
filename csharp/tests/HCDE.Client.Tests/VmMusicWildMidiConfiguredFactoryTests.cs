using System.Buffers.Binary;
using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiConfiguredFactoryTests
{
    private sealed class OutputProbe : IVmMusicOutputStream
    {
        public void Play(bool loop, float volume) { }
        public void SetPaused(bool paused) { }
        public void Stop() { }
        public void Dispose() { }
    }

    [Fact]
    public void HostOutputGainChangesRenderedPatchWithoutChangingMidiVolume()
    {
        var catalog = new VmMusicWildMidiInstrumentCatalog(_ => Bytes());
        catalog.Register(0, "piano.pat");
        using var renderer = VmMusicWildMidiFactory.CreateConfigured(catalog, 44100, false, false);
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000,
            [new(0, 0x643c90), new(120, 0x3c80)]));
        VmMusicFloatFill? fill = null;
        using var playback = VmMusicSynthStreamPlayback.Start(source, VmMusicMidiDeviceKind.WildMidi, -6, false,
            _ => new VmMusicWildMidiSessionDevice(renderer), new(600000),
            (_, callback) => { fill = callback; return new OutputProbe(); });
        var sample = new float[16];
        Assert.True(fill!(sample));
        Assert.Contains(sample, value => value != 0);
        playback.OutputVolume = 0;
        Assert.True(fill(sample));
        Assert.Equal(new float[16], sample);
        playback.OutputVolume = 1;
        Assert.True(fill(sample));
        Assert.Contains(sample, value => value != 0);
    }

    private static byte[] Bytes()
    {
        var bytes = new byte[399];
        "GF1PATCH110\0ID#000002\0"u8.CopyTo(bytes);
        bytes[198] = 1;
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(247), 64);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(259), 44100);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(269), 262000);
        bytes.AsSpan(335).Fill(64);
        return bytes;
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("override")]
    public void WaveFactoryLoadsDefaultOrRequestedConfigAndProducesAudio(string? argument)
    {
        var directory = Path.Combine(Path.GetTempPath(), "hcde-wild-wave-" + Guid.NewGuid());
        Directory.CreateDirectory(directory);
        var outputPath = Path.Combine(directory, "output.wav");
        try
        {
            var config = Path.Combine(directory, "main.cfg");
            var alternate = Path.Combine(directory, "alternate.cfg");
            File.WriteAllText(config, "0 piano");
            File.WriteAllText(alternate, "0 piano");
            File.WriteAllBytes(Path.Combine(directory, "piano.pat"), Bytes());
            var overridePath = argument == "override" ? alternate : argument;
            // A valid override must not require the configured default file to exist.
            if (argument == "override") File.Delete(config);
            var factory = VmMusicWildMidiFactory.CreateWaveFactory(config, true, true);
            var router = VmMusicSynthWaveDump.CreateRouter(VmMusicMidiDeviceKind.WildMidi, null, factory);
            using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000,
                [new(0, 0x643c90), new(120, 0x3c80)]));
            Assert.True(router(source, new(-1, overridePath, outputPath, 0, 0)));
            var wave = File.ReadAllBytes(outputPath);
            Assert.Equal(44100u, BinaryPrimitives.ReadUInt32LittleEndian(wave.AsSpan(24)));
            Assert.Contains(wave.AsSpan(68).ToArray(), value => value != 0);
            using var exclusive = new FileStream(Path.Combine(directory, "piano.pat"),
                FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }
        finally
        {
            foreach (var file in Directory.GetFiles(directory)) File.Delete(file);
            Directory.Delete(directory);
        }
    }

    [Fact]
    public void WaveFactoryRejectsOtherDevicesAndDoesNotHideMissingOverride()
    {
        var factory = VmMusicWildMidiFactory.CreateWaveFactory("missing-default.cfg", false, false);
        Assert.Throws<NotSupportedException>(() => factory(new(7, null, "unused.wav", 0, 0)));
        Assert.Throws<FileNotFoundException>(() => factory(new(6,
            Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".cfg"), "unused.wav", 0, 0)));
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiFactory.CreateWaveFactory("", false, false));
    }

    [Fact]
    public void FileConfigurationIncludesLoadPlayablePatchAndCloseResources()
    {
        var directory = Path.Combine(Path.GetTempPath(), "hcde-wildmidi-" + Guid.NewGuid());
        Directory.CreateDirectory(directory);
        try
        {
            var config = Path.Combine(directory, "main.cfg");
            File.WriteAllText(config, "source instruments.cfg\nreverb_room_width 1\nreverb_room_length 1");
            File.WriteAllText(Path.Combine(directory, "instruments.cfg"), "0 piano");
            var patchPath = Path.Combine(directory, "piano.pat");
            File.WriteAllBytes(patchPath, Bytes());
            using (var renderer = VmMusicWildMidiFactory.CreateFromFile(config, 44100, false, true))
            {
                renderer.ShortEvent(0x90, 60, 100);
                Assert.Equal(1, renderer.GetVoiceCount());
                var output = new float[4000];
                renderer.ComputeOutput(output, 2000);
                Assert.Contains(output, value => value != 0f);
                using var exclusive = new FileStream(patchPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
        }
        finally
        {
            foreach (var file in Directory.GetFiles(directory)) File.Delete(file);
            Directory.Delete(directory);
        }
    }

    [Fact]
    public void ConfiguredFactoryOwnsLoadedPatchAndNormalizesInvalidRate()
    {
        var catalog = new VmMusicWildMidiInstrumentCatalog(_ => Bytes());
        new VmMusicWildMidiConfigReader(catalog, "config.cfg", true).Read("0 piano");
        var patch = catalog.FindExact(0)!;
        using (var renderer = VmMusicWildMidiFactory.CreateConfigured(catalog, 0, true, true))
        {
            Assert.True(patch.Loaded);
            Assert.Equal(1, patch.UseCount);
            renderer.ShortEvent(0x90, 60, 100);
            renderer.ComputeOutput(new float[200], 100);
        }
        Assert.False(patch.Loaded);
        Assert.Equal(0, patch.UseCount);
    }

    [Fact]
    public void MissingConfigurationAndPatchFailExplicitly()
    {
        Assert.Throws<FileNotFoundException>(() => VmMusicWildMidiFactory.CreateFromFile(
            Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".cfg"), 44100, false, false));
        var catalog = new VmMusicWildMidiInstrumentCatalog(_ => null);
        catalog.Register(0, "missing.pat");
        Assert.Throws<FileNotFoundException>(() => VmMusicWildMidiFactory.CreateConfigured(catalog, 44100, false, false));
        Assert.Equal(0, catalog.FindExact(0)!.UseCount);
    }

    [Fact]
    public void SharedCatalogSamplesRemainPlayableUntilLastRendererDisposes()
    {
        var cache = new VmMusicWildMidiCatalogCache();
        var catalog = cache.Load("bank.cfg", _ =>
        {
            var loaded = new VmMusicWildMidiInstrumentCatalog(_ => Bytes());
            new VmMusicWildMidiConfigReader(loaded, "bank.cfg", false).Read("0 piano");
            return loaded;
        });
        var patch = catalog.FindExact(0)!;
        using var first = VmMusicWildMidiFactory.CreateConfigured(catalog, 44100, false, false);
        using (var second = VmMusicWildMidiFactory.CreateConfigured(catalog, 22050, false, false))
        {
            Assert.Equal(2, patch.UseCount);
            first.Dispose();
            Assert.Equal(1, patch.UseCount);
            Assert.True(patch.Loaded);
            second.ShortEvent(0x90, 60, 100);
            var output = new float[400];
            second.ComputeOutput(output, 200);
            Assert.Contains(output, value => value != 0);
        }
        Assert.Equal(0, patch.UseCount);
        Assert.False(patch.Loaded);
    }
}
