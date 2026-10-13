using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;
using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiResourceCacheTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SourceDiscoveryPreloadsCallbackPatchesAndPreservesCacheOwnership(bool missingPatch)
    {
        var reads = new List<string>();
        var resources = new Dictionary<string, byte[]>
        {
            ["bank.cfg"] = Encoding.ASCII.GetBytes("0 piano\n1 violin"),
            ["piano.pat"] = Patch()
        };
        if (!missingPatch) resources["violin.pat"] = Patch();
        var reader = new OwnedReader(new VmGusResourceMusicReader("bank.cfg", name =>
        {
            reads.Add(name);
            return resources.GetValueOrDefault(name);
        }));
        using var cache = new VmMusicWildMidiResourceCache(_ => reader);
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000,
            [new(0, 0x01c0), new(0, 0x643c90)]));
        if (missingPatch)
        {
            Assert.Throws<FileNotFoundException>(() => cache.CreateRendererForSource("bank.cfg", source, 44100, false, false));
            Assert.Contains("violin.pat", reads);
            Assert.Equal(0, reader.Disposals);
            cache.Clear();
            Assert.Equal(1, reader.Disposals);
        }
        else
        {
            using var renderer = cache.CreateRendererForSource("bank.cfg", source, 44100, false, false);
            Assert.Contains("violin.pat", reads);
            cache.Clear();
            Assert.Equal(0, reader.Disposals);
            renderer.ShortEvent(0xc0, 1, 0);
            AssertAudio(renderer);
            renderer.Dispose();
            Assert.Equal(1, reader.Disposals);
        }
        Assert.False(source.CheckDone());
        Assert.Equal(1, source.LoopLimit);
        Assert.Equal("", cache.LoadedConfiguration);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FileBankCallbacksResolveNestedIncludesAndPatchesForRelativeOrAbsoluteConfiguration(bool absolute)
    {
        var root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))).FullName;
        try
        {
            var config = Path.Combine(root, "song.cfg");
            var sets = Directory.CreateDirectory(Path.Combine(root, "sets")).FullName;
            File.WriteAllText(config, "source sets/tones.cfg");
            File.WriteAllText(Path.Combine(sets, "tones.cfg"), "1 piano");
            File.WriteAllBytes(Path.Combine(sets, "piano.pat"), Patch());
            var requested = absolute ? config : Path.GetRelativePath(Environment.CurrentDirectory, config);
            using var callbacks = new VmMusicCallbacks([]);
            using var cache = callbacks.CreateWildMidiCache();
            using var renderer = cache.CreateRenderer(requested, 44100, false, false);
            Assert.Equal(requested, cache.LoadedConfiguration);
            cache.Clear();
            renderer.LoadInstrument(0, 0, 1);
            renderer.ShortEvent(0xc0, 1, 0);
            AssertAudio(renderer);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Theory]
    [InlineData("music/song.cfg", "music/")]
    [InlineData("music\\song.cfg", "music/")]
    [InlineData("deep/music/song.cfg", "deep/music/")]
    public void PackagedConfigurationBaseDirectoryIsAppliedOnceToIncludesAndPatches(string name, string basePath)
    {
        var lookedUp = new List<string>();
        using var callbacks = new VmMusicCallbacks([], candidate =>
        {
            lookedUp.Add(candidate);
            if (candidate == name) return 0;
            if (candidate == basePath + "sets/tones.cfg") return 1;
            if (candidate == basePath + "sets/piano.pat") return 2;
            return -1;
        }, index => index switch
        {
            0 => Encoding.ASCII.GetBytes("source sets/tones.cfg"),
            1 => Encoding.ASCII.GetBytes("1 piano"),
            2 => Patch(),
            _ => null
        });
        using var cache = callbacks.CreateWildMidiCache();
        using var renderer = cache.CreateRenderer(name, 44100, false, false);
        Assert.Contains(basePath + "sets/tones.cfg", lookedUp);
        Assert.DoesNotContain(basePath + "sets/piano.pat", lookedUp);
        cache.Clear();
        renderer.LoadInstrument(0, 0, 1);
        renderer.ShortEvent(0xc0, 1, 0);
        AssertAudio(renderer);
        Assert.Contains(basePath + "sets/piano.pat", lookedUp);
        Assert.DoesNotContain(lookedUp, candidate => candidate.StartsWith(basePath + basePath, StringComparison.Ordinal));
    }

    [Fact]
    public void ApplicationCallbacksConnectIndexedResourcesToCachedWaveRenderer()
    {
        var configLookups = 0;
        using var callbacks = new VmMusicCallbacks([], name => name switch
        {
            "song.cfg" => ++configLookups == 1 ? 0 : -1,
            "tones.cfg" => 1,
            "piano.pat" => 2,
            _ => -1
        }, index => index switch
        {
            0 => Encoding.ASCII.GetBytes("source tones.cfg"),
            1 => Encoding.ASCII.GetBytes("0 piano"),
            2 => Patch(),
            _ => null
        });
        using var cache = callbacks.CreateWildMidiCache();
        var factory = VmMusicWildMidiFactory.CreateCachedResourceWaveFactory(cache, "song.cfg", false, false);
        using var first = factory(new(6, null, "unused.wav", 0, 22050));
        using var second = factory(new(6, "SONG.CFG", "unused.wav", 0, 44100));
        Assert.Equal(1, configLookups);
        cache.Clear();
        AssertAudio(first);
        first.Dispose();
        AssertAudio(second);
        second.Dispose();
        callbacks.Dispose();
        Assert.Throws<ObjectDisposedException>(() => cache.CreateRenderer("song.cfg", 44100, false, false));
    }

    private sealed class OwnedReader(IVmMusicBankReader reader) : IVmMusicBankReader, IDisposable
    {
        public int Disposals;
        public VmMusicPatchPaths Paths => reader.Paths;
        public (byte[]? Data, string Name) Open(string? name)
        {
            ObjectDisposedException.ThrowIf(Disposals != 0, this);
            return reader.Open(name);
        }
        public void Dispose() => Disposals++;
    }

    [Fact]
    public void ResourceCallbackLoadsIncludesAndLazyPatchesWithoutDiskAndRetainsReaderAcrossClear()
    {
        var resources = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["bank.cfg"] = Encoding.ASCII.GetBytes("source tones.cfg"),
            ["tones.cfg"] = Encoding.ASCII.GetBytes("0 piano"),
            ["piano.pat"] = Patch()
        };
        var reader = new OwnedReader(new VmGusResourceMusicReader("bank.cfg",
            name => resources.GetValueOrDefault(name)));
        var calls = 0;
        using var cache = new VmMusicWildMidiResourceCache(name =>
        {
            Assert.Equal("bank.cfg", name);
            calls++;
            return reader;
        });
        using var first = cache.CreateRenderer("bank.cfg", 22050, false, false);
        using var second = cache.CreateRenderer("BANK.CFG", 44100, false, false);
        Assert.Equal(1, calls);
        cache.Clear();
        Assert.Equal(0, reader.Disposals);
        AssertAudio(first);
        first.Dispose();
        AssertAudio(second);
        second.Dispose();
        Assert.Equal(1, reader.Disposals);
    }

    [Fact]
    public void FailedSuppliedConfigurationClosesReaderAndPreservesPreviousCacheEntry()
    {
        var valid = new OwnedReader(new VmGusResourceMusicReader("valid.cfg", _ => []));
        var invalid = new OwnedReader(new VmGusResourceMusicReader("invalid.cfg",
            name => name == "invalid.cfg" ? Encoding.ASCII.GetBytes("source missing.cfg") : null));
        using var cache = new VmMusicWildMidiResourceCache(name => name == "valid.cfg" ? valid : invalid);
        using var first = cache.CreateRenderer("valid.cfg", 44100, false, false);
        Assert.Throws<FileNotFoundException>(() => cache.CreateRenderer("invalid.cfg", 44100, false, false));
        Assert.Equal(1, invalid.Disposals);
        Assert.Equal("valid.cfg", cache.LoadedConfiguration);
        using var reused = cache.CreateRenderer("VALID.CFG", 22050, false, false);
        cache.Dispose();
        Assert.Equal(0, valid.Disposals);
        first.Dispose();
        reused.Dispose();
        Assert.Equal(1, valid.Disposals);
    }

    [Fact]
    public void NullBankCallbackFallsBackToDiskArchive()
    {
        var path = Archive();
        try
        {
            var calls = 0;
            using var cache = new VmMusicWildMidiResourceCache(name =>
                { Assert.Equal(path, name); calls++; return null; });
            using var renderer = cache.CreateRenderer(path, 44100, false, false);
            AssertAudio(renderer);
            Assert.Equal(1, calls);
            renderer.Dispose();
            cache.Clear();
            AssertReleased(path);
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ConfiguredPlaybackRouterUsesDefaultOrSongArchiveAndSurvivesCacheDisposal(bool overrideDefault)
    {
        var path = Archive();
        try
        {
            using var cache = new VmMusicWildMidiResourceCache();
            var router = VmMusicSynthDeviceFactory.CreateConfiguredRouter(null, cache,
                overrideDefault ? "missing-default.cfg" : path, overrideDefault ? path : "",
                22050, false, false);
            using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000,
                [new(0, 0x643c90), new(120, 0x3c80)]));
            IVmMusicMidiSessionDevice? selected = null;
            var playing = VmMusicMidiPlay.Start(source, VmMusicMidiDeviceKind.FluidSynth,
                -6, false, kind => selected = router(kind), new(600000));
            using (playing.Session)
            {
                Assert.True(playing.Started);
                Assert.Equal(VmMusicMidiDeviceKind.WildMidi, playing.Selected);
                Assert.Equal(path, cache.LoadedConfiguration);
                var device = Assert.IsType<VmMusicWildMidiSessionDevice>(selected);
                Assert.Equal(22050, device.GetStreamInfo().SampleRate);
                cache.Dispose();
                var samples = new float[400];
                Assert.True(device.ServiceStream(samples));
                Assert.Contains(samples, value => value != 0);
            }
            AssertReleased(path);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void ConfiguredPlaybackFailureDoesNotFallBackToAnUnrelatedFmBank()
    {
        using var cache = new VmMusicWildMidiResourceCache();
        var fmAllocated = false;
        var missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".cfg");
        var router = VmMusicSynthDeviceFactory.CreateConfiguredRouter((_, _) =>
            { fmAllocated = true; throw new InvalidOperationException("unexpected FM allocation"); },
            cache, "default.cfg", missing, 44100, false, false);
        Assert.Throws<FileNotFoundException>(() =>
            VmMusicMidiDeviceCreation.Create(VmMusicMidiDeviceKind.WildMidi, false, router));
        Assert.False(fmAllocated);
    }

    private static string Archive(string configuration = "0 piano")
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".bank");
        using var file = File.Create(path);
        using var zip = new ZipArchive(file, ZipArchiveMode.Create);
        using (var output = zip.CreateEntry("timidity.cfg").Open())
            output.Write(Encoding.ASCII.GetBytes(configuration));
        using (var output = zip.CreateEntry("piano.pat").Open()) output.Write(Patch());
        return path;
    }

    private static byte[] Patch()
    {
        var patch = new byte[399];
        "GF1PATCH110\0ID#000002\0"u8.CopyTo(patch);
        patch[198] = 1;
        BinaryPrimitives.WriteUInt32LittleEndian(patch.AsSpan(247), 64);
        BinaryPrimitives.WriteUInt16LittleEndian(patch.AsSpan(259), 44100);
        BinaryPrimitives.WriteUInt32LittleEndian(patch.AsSpan(269), 262000);
        patch.AsSpan(335).Fill(64);
        return patch;
    }

    private static void AssertAudio(VmMusicWildMidiManagedRenderer renderer)
    {
        renderer.ShortEvent(0x90, 60, 100);
        var samples = new float[400];
        renderer.ComputeOutput(samples, 200);
        Assert.Contains(samples, value => value != 0);
    }

    private static void AssertReleased(string path)
    {
        using var exclusive = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ActiveRenderersRetainLazyArchiveResourcesAfterCacheRelease(bool dispose)
    {
        var path = Archive();
        try
        {
            using var cache = new VmMusicWildMidiResourceCache();
            using var first = cache.CreateRenderer(path, 22050, false, false);
            using var second = cache.CreateRenderer(path.ToUpperInvariant(), 44100, false, false);
            if (dispose) cache.Dispose(); else cache.Clear();
            Assert.Equal("", cache.LoadedConfiguration);
            AssertAudio(first);
            first.Dispose();
            // The second renderer has not loaded a patch yet, even after cache release.
            AssertAudio(second);
            second.Dispose();
            AssertReleased(path);
            if (dispose)
                Assert.Throws<ObjectDisposedException>(() => cache.CreateRenderer(path, 44100, false, false));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void CacheRetainsArchiveBetweenRendererLifetimesUntilClear()
    {
        var path = Archive();
        try
        {
            using var cache = new VmMusicWildMidiResourceCache();
            using (var first = cache.CreateRenderer(path, 44100, false, false)) AssertAudio(first);
            Assert.Throws<IOException>(() => AssertReleased(path));
            using (var second = cache.CreateRenderer(path.ToUpperInvariant(), 22050, false, false)) AssertAudio(second);
            cache.Clear();
            AssertReleased(path);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void FailedReplacementPreservesCachedArchiveAndSuccessfulReplacementRetainsActiveOwner()
    {
        var path = Archive();
        var invalid = Archive("source missing.cfg");
        var next = Archive();
        try
        {
            using var cache = new VmMusicWildMidiResourceCache();
            using var renderer = cache.CreateRenderer(path, 44100, false, false);
            Assert.Throws<FileNotFoundException>(() => cache.CreateRenderer(invalid, 44100, false, false));
            Assert.Equal(path, cache.LoadedConfiguration);
            AssertReleased(invalid);
            using var reused = cache.CreateRenderer(path.ToUpperInvariant(), 22050, false, false);
            using var replacement = cache.CreateRenderer(next, 44100, false, false);
            Assert.Equal(next, cache.LoadedConfiguration);
            cache.Clear();
            AssertAudio(renderer);
            renderer.Dispose();
            AssertAudio(reused);
            reused.Dispose();
            AssertReleased(path);
            AssertAudio(replacement);
            replacement.Dispose();
            AssertReleased(next);
        }
        finally { File.Delete(path); File.Delete(invalid); File.Delete(next); }
    }

    [Fact]
    public void FailedPrecacheReleasesRendererLeaseWhenCacheIsDisposed()
    {
        var path = Archive("0 missing");
        try
        {
            using var cache = new VmMusicWildMidiResourceCache();
            Assert.Throws<FileNotFoundException>(() => cache.CreateRenderer(path, 44100, false, false, [0]));
            cache.Dispose();
            AssertReleased(path);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void CachedWaveFactorySupportsArchivesAndFileConfigurationOverrides()
    {
        var path = Archive();
        var config = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".cfg");
        try
        {
            File.WriteAllText(config, "");
            using var cache = new VmMusicWildMidiResourceCache();
            var factory = VmMusicWildMidiFactory.CreateCachedResourceWaveFactory(cache, path, false, false);
            using (var renderer = factory(new(6, null, "unused.wav", 0, 22050))) AssertAudio(renderer);
            using (var renderer = factory(new(-1, config, "unused.wav", 0, 0))) Assert.Equal(44100, renderer.SampleRate);
            AssertReleased(path);
            File.WriteAllText(config, "source missing.cfg");
            using (var renderer = factory(new(6, config, "unused.wav", 0, 0))) Assert.Equal(44100, renderer.SampleRate);
            cache.Clear();
            Assert.Throws<FileNotFoundException>(() => factory(new(6, config, "unused.wav", 0, 0)));
            Assert.Throws<NotSupportedException>(() => factory(new(4, null, "unused.wav", 0, 0)));
        }
        finally { File.Delete(path); File.Delete(config); }
    }
}
