namespace HCDE.Client.Tests;

public class VmMusicFmConfiguredDeviceTests
{
    private sealed class HostOutput(VmMusicFloatFill fill) : IVmMusicOutputStream
    {
        public bool Fill(float[] samples) => fill(samples);
        public void Play(bool loop, float volume) { }
        public void SetPaused(bool paused) { }
        public void Stop() { }
        public void Dispose() { }
    }

    [Theory]
    [InlineData(7, "libadl.")]
    [InlineData(8, "libopn.")]
    public void StreamHostForwardsRuntimeGainAndCustomBankChanges(int kind, string prefix)
    {
        var renderer = new Renderer();
        using var source = Source();
        HostOutput? output = null;
        using var playback = VmMusicSynthStreamPlayback.Start(source, (VmMusicMidiDeviceKind)kind,
            -kind, false, _ => kind == 7
                ? VmMusicFmDeviceFactory.CreateAdl(_ => renderer, new(), Resources())
                : VmMusicFmDeviceFactory.CreateOpn(_ => renderer, new(), Resources()),
            new(600000), (_, fill) => output = new(fill));
        Assert.True(playback.ChangeNumber(prefix + "gain", 0.5));
        Assert.Equal("This MIDI device does not have any stats.", playback.GetStatistics());
        var before = new float[128];
        Assert.True(output!.Fill(before));
        Assert.Contains(before, value => value != 0);
        Assert.True(playback.ChangeNumber(prefix + "gain", 0.25));
        var after = new float[128];
        Assert.True(output.Fill(after));
        Assert.Equal(before.Select(value => value * 0.5f), after);
        Assert.Equal(1, playback.OutputVolume);
        Assert.True(playback.ChangeString(prefix + "custombank", "runtime-bank"));
        Assert.True(playback.ChangeInteger(prefix + "usecustombank", 1));
        Assert.Equal("runtime-bank", renderer.File);
        Assert.True(playback.ChangeString(prefix + "custombank", "replacement-bank"));
        Assert.Equal("replacement-bank", renderer.File);
        Assert.False(playback.ChangeNumber("unknown", 1));
        Assert.False(playback.ChangeString("unknown", "bank"));
        Assert.Throws<ArgumentNullException>(() => playback.ChangeNumber(null!, 1));
        Assert.Throws<ArgumentNullException>(() => playback.ChangeString(prefix + "custombank", null!));
        playback.Dispose();
        Assert.Equal(1, renderer.Disposals);
        Assert.Throws<ObjectDisposedException>(() => playback.ChangeNumber(prefix + "gain", 1));
        Assert.Throws<ObjectDisposedException>(() => playback.ChangeString(prefix + "custombank", "bank"));
    }

    [Theory]
    [InlineData(7)]
    [InlineData(8)]
    public void ConfiguredSynthRouterForwardsSongArgumentsToFmWithoutLoadingWildMidi(int kind)
    {
        using var cache = new VmMusicWildMidiResourceCache();
        var renderer = new Renderer();
        var router = VmMusicSynthDeviceFactory.CreateConfiguredRouter((selected, argument) =>
        {
            Assert.Equal((VmMusicMidiDeviceKind)kind, selected);
            Assert.Equal("song-bank", argument);
            return selected == VmMusicMidiDeviceKind.Adl
                ? VmMusicFmDeviceFactory.CreateAdl(_ => renderer, new(), Resources(), argument)
                : VmMusicFmDeviceFactory.CreateOpn(_ => renderer, new(), Resources(), argument);
        }, cache, "missing-default.cfg", "song-bank", 22050, false, false);
        using (var device = Assert.IsType<VmMusicFmSessionDevice>(router((VmMusicMidiDeviceKind)kind)))
        {
            Assert.Equal("song-bank", renderer.File);
            Assert.Equal("", cache.LoadedConfiguration);
        }
        Assert.Equal(1, renderer.Disposals);
    }

    [Theory]
    [InlineData(7)]
    [InlineData(8)]
    public void PlaybackRouterResolvesPerSongBankBeforeAllocation(int kind)
    {
        var renderer = new Renderer();
        var resolved = false;
        Renderer Allocate(int rate) { Assert.True(resolved); Assert.Equal(44100, rate); return renderer; }
        var adl = new VmMusicAdlConfiguration();
        var opn = new VmMusicOpnConfiguration();
        var router = VmMusicFmDeviceFactory.CreateRouter<Renderer, Renderer>(Allocate, Allocate,
            adl, opn, Resources(), bankArgument: "song-bank", resolveBank: (name, format) =>
            {
                Assert.Equal("song-bank", name);
                Assert.Equal(kind == 7 ? VmMusicBankFormat.Wopl : VmMusicBankFormat.Wopn, format);
                resolved = true;
                return "resolved-bank";
            });
        using var source = Source();
        var playing = VmMusicMidiPlay.Start(source, (VmMusicMidiDeviceKind)kind, -kind, false, router, new(600000));
        using (playing.Session)
        {
            Assert.True(playing.Started);
            Assert.Equal((VmMusicMidiDeviceKind)kind, playing.Selected);
            Assert.Equal("resolved-bank", renderer.File);
            Assert.False(adl.Bank.UseCustomBank);
            Assert.False(opn.Bank.UseCustomBank);
        }
        Assert.Equal(1, renderer.Disposals);
    }

    [Fact]
    public void PlaybackRouterSupportsAdlNumericBankWithoutPathResolution()
    {
        var renderer = new Renderer();
        var router = VmMusicFmDeviceFactory.CreateRouter<Renderer, Renderer>(_ => renderer, null,
            new(), new(), Resources(), bankArgument: "8suffix",
            resolveBank: (_, _) => throw new InvalidOperationException("unexpected resolution"));
        using var device = Assert.IsType<VmMusicFmSessionDevice>(router(VmMusicMidiDeviceKind.Adl));
        Assert.Equal(8, renderer.Bank);
    }

    [Theory]
    [InlineData(7)]
    [InlineData(8)]
    public void PlaybackBankResolutionFailureDoesNotAllocateRenderer(int kind)
    {
        var allocated = false;
        Renderer Allocate(int _) { allocated = true; return new(); }
        var error = new FileNotFoundException("bank resolution");
        var router = VmMusicFmDeviceFactory.CreateRouter<Renderer, Renderer>(Allocate, Allocate,
            new(), new(), Resources(), bankArgument: "missing",
            resolveBank: (_, _) => throw error);
        Assert.Same(error, Assert.Throws<FileNotFoundException>(() => router((VmMusicMidiDeviceKind)kind)));
        Assert.False(allocated);
    }

    [Fact]
    public void ResourceWaveExportResolvesCurrentSongBeforeStopClearsPlaybackState()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".wav");
        var state = new VmMusicPlaybackState<object>("current", new(), 2, true);
        var renderer = new Renderer();
        var calls = new List<string>();
        var bytes = VmMusicSmf.Serialize(120, 500000,
            Enumerable.Range(0, 300).Select(_ => new VmMusicSmfEvent(1, 0x643c90)));
        var console = new VmMusicConsole<object>(state, new([]), () => true, (_, _) => false,
            force =>
            {
                Assert.True(force);
                calls.Add("stop");
                state.Stop(true, 0, () => { }, () => { }, _ => { }, _ => { });
                Assert.Equal("", state.Name);
            }, _ => { }, _ => Assert.Fail("unexpected report"));
        console.ConfigureFmWaveExport(-1,
            name => { Assert.Equal("current", name); calls.Add("namespace"); return -1; },
            name => { Assert.Equal("current", name); calls.Add("full"); return 3; },
            index => { Assert.Equal(3, index); calls.Add("read"); return bytes; },
            VmMusicFmDeviceFactory.CreateWaveFactory<Renderer, Renderer>(_ => renderer, null,
                new(), new(), Resources(), VmMusicMidiDeviceKind.Adl),
            (name, order, loop, force) =>
            {
                Assert.Equal("current", name); Assert.Equal(2, order); Assert.True(loop); Assert.True(force);
                calls.Add("restore"); return true;
            });
        try
        {
            Assert.True(console.TryExecute(["writewave", "*", path]));
            Assert.Equal(new[] { "namespace", "full", "read", "stop", "restore" }, calls);
            Assert.True(new FileInfo(path).Length > 68);
            Assert.Equal(1, renderer.Disposals);
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData(false, "Cannot find MIDI lump song.\n")]
    [InlineData(true, "song is not MIDI-based.\n")]
    public void ResourceWaveExportRejectsMissingOrNonMidiSongBeforeStoppingOrAllocating(bool found, string message)
    {
        var reports = new List<string>();
        var console = new VmMusicConsole<object>(new("current", null), new([]), () => true,
            (_, _) => false, _ => Assert.Fail("unexpected stop"), _ => { }, reports.Add);
        console.ConfigureFmWaveExport(-1, _ => found ? 0 : -1, _ => -1, _ => new byte[40],
            _ => throw new InvalidOperationException("unexpected allocation"),
            (_, _, _, _) => throw new InvalidOperationException("unexpected restoration"));
        Assert.True(console.TryExecute(["writewave", "song", "unused.wav"]));
        Assert.Equal(message, Assert.Single(reports));
    }

    [Fact]
    public void RegisteredConsoleCommandResolvesBankAndWritesWaveThroughOwnedFactory()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".wav");
        var renderer = new Renderer();
        var restored = false;
        var stopped = false;
        try
        {
            var console = new VmMusicConsole<object>(new("old", null), new([]), () => true,
                (_, _) => true, force => { Assert.True(force); stopped = true; }, _ => { }, _ => Assert.Fail("unexpected report"));
            var factory = VmMusicFmDeviceFactory.CreateWaveFactory<Renderer, Renderer>(_ => renderer, null,
                new(), new(), Resources(), VmMusicMidiDeviceKind.Adl);
            console.ConfigureFmWaveExport(-1, _ => Source(), factory,
                (name, _, _, force) => { Assert.Equal("old", name); Assert.True(force); restored = true; return true; }, () => "error");
            Assert.True(console.TryExecute(new[] { "WRITEWAVE", "song", path, "0", "0", "adl", "8" }));
            Assert.True(stopped);
            Assert.True(restored);
            Assert.Equal(8, renderer.Bank);
            Assert.Equal(1, renderer.Disposals);
            Assert.True(new FileInfo(path).Length > 68);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void RegisteredConsoleRetainsUsageValidationWithoutAllocatingRenderer()
    {
        var reports = new List<string>();
        var console = new VmMusicConsole<object>(new("", null), new([]), () => true,
            (_, _) => true, _ => Assert.Fail("unexpected stop"), _ => { }, reports.Add);
        console.ConfigureFmWaveExport(-1, _ => throw new InvalidOperationException("unexpected source"),
            _ => throw new InvalidOperationException("unexpected renderer"), (_, _, _, _) => true, () => "error");
        Assert.True(console.TryExecute(new[] { "writewave" }));
        Assert.Contains("Usage: writewave", Assert.Single(reports));
        Assert.False(console.TryExecute(new[] { "unknown" }));
    }

    [Theory]
    [InlineData(7)]
    [InlineData(8)]
    public void WaveFactoryResolvesNamedBankBeforeRendererAllocation(int kind)
    {
        var renderer = new Renderer();
        var resolved = false;
        Renderer Allocate(int rate) { Assert.True(resolved); Assert.Equal(44100, rate); return renderer; }
        var factory = VmMusicFmDeviceFactory.CreateWaveFactory<Renderer, Renderer>(Allocate, Allocate,
            new(), new(), Resources(), VmMusicMidiDeviceKind.Adl, (name, format) =>
            {
                Assert.Equal("named", name);
                Assert.Equal(kind == 7 ? VmMusicBankFormat.Wopl : VmMusicBankFormat.Wopn, format);
                resolved = true;
                return "resolved-bank";
            });
        using var device = factory(new(kind, "named", "unused", 0, 48000));
        Assert.Equal("resolved-bank", renderer.File);
        Assert.Equal(44100, device.GetStreamInfo().SampleRate);
    }

    [Fact]
    public void WaveFactoryDefaultAdlUsesNumericBankArgumentAndRejectsOtherDevices()
    {
        var renderer = new Renderer();
        var factory = VmMusicFmDeviceFactory.CreateWaveFactory<Renderer, Renderer>(_ => renderer, null,
            new(), new(), Resources(), VmMusicMidiDeviceKind.Adl);
        using var device = factory(new(-1, "8tail", "unused", 0, 0));
        Assert.Equal(8, renderer.Bank);
        Assert.Null(renderer.File);
        Assert.Throws<NotSupportedException>(() => factory(new(0, null, "unused", 0, 0)));
    }

    [Fact]
    public void WriteWaveCommandCreatesFileAndRestoresSavedPlayback()
    {
        var renderer = new Renderer();
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".wav");
        var calls = new List<string>();
        try
        {
            var state = new VmMusicPlaybackState<object>("old", null, 2, true);
            VmMusicWaveExport.WriteFmWave(state, ["writewave", "song", path, "0", "0", "adl", "custom"], -1,
                name => { Assert.Equal("song", name); return Source(); },
                force => { Assert.True(force); calls.Add("stop"); },
                request =>
                {
                    Assert.Equal(new VmMusicWaveRequest(7, "custom", path, 0, 0), request);
                    calls.Add("create");
                    return VmMusicFmDeviceFactory.CreateAdl(_ => renderer, new(), Resources());
                },
                (name, order, loop, force) =>
                {
                    Assert.Equal("old", name); Assert.Equal(2, order); Assert.True(loop); Assert.True(force);
                    calls.Add("restore"); return true;
                }, () => "unexpected error", _ => Assert.Fail("unexpected report"));
            Assert.Equal(new[] { "stop", "create", "restore" }, calls);
            Assert.True(new FileInfo(path).Length > 68);
            Assert.Equal(1, renderer.Disposals);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void WriteWaveCommandRestoresPlaybackAfterRendererStartupFailure()
    {
        var failure = new InvalidOperationException("reset failed");
        var renderer = new Renderer { ResetFailure = failure };
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".wav");
        var restored = false;
        try
        {
            Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => VmMusicWaveExport.WriteFmWave(
                new VmMusicPlaybackState<object>("old", null), ["writewave", "song", path, "0", "0", "adl"], -1,
                _ => Source(), _ => { }, _ => VmMusicFmDeviceFactory.CreateAdl(_ => renderer, new(), Resources()),
                (_, _, _, _) => { restored = true; return true; }, () => "error", _ => { })));
            Assert.True(restored);
            Assert.Equal(1, renderer.Disposals);
            Assert.Equal(0u, System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(File.ReadAllBytes(path).AsSpan(4)));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void InvalidSubsongPreventsAllocationAndOutputOverwrite()
    {
        using var source = Source();
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".wav");
        try
        {
            File.WriteAllBytes(path, new byte[] { 17, 42 });
            var allocated = false;
            Assert.Throws<ArgumentOutOfRangeException>(() => VmMusicFmWaveDump.Write(source, new(7, null, path, 99, 0),
                _ => { allocated = true; throw new InvalidOperationException(); }));
            Assert.False(allocated);
            Assert.Equal(new byte[] { 17, 42 }, File.ReadAllBytes(path));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void WaveDumpRequestCreatesRealFileAndReleasesDeviceWhileBorrowingSource()
    {
        using var source = Source();
        var renderer = new Renderer();
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".wav");
        try
        {
            var request = new VmMusicWaveRequest(7, "bank-argument", path, 0, 0);
            Assert.True(VmMusicFmWaveDump.Write(source, request, received =>
            {
                Assert.Equal(request, received);
                return VmMusicFmDeviceFactory.CreateAdl(_ => renderer, new(), Resources());
            }));
            var bytes = File.ReadAllBytes(path);
            Assert.Equal("RIFF"u8.ToArray(), bytes[..4]);
            Assert.Equal((uint)bytes.Length - 8, System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(4)));
            Assert.Equal((uint)bytes.Length - 68, System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(64)));
            Assert.Equal(1, renderer.Disposals);
            source.Restart();
            Assert.False(source.CheckDone());
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void FailedWaveDumpLeavesProvisionalHeaderAndReleasesDevice()
    {
        using var source = Source();
        var failure = new InvalidOperationException("render failed");
        var renderer = new Renderer { RenderFailure = failure };
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".wav");
        try
        {
            Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => VmMusicFmWaveDump.Write(source,
                new(7, null, path, 0, 0), _ => VmMusicFmDeviceFactory.CreateAdl(_ => renderer, new(), Resources()))));
            Assert.Equal(1, renderer.Disposals);
            Assert.Equal(0u, System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(File.ReadAllBytes(path).AsSpan(4)));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void SourceSessionRendersFinalizedWaveAndReleasesRenderer()
    {
        using var source = Source();
        var renderer = new Renderer();
        var device = VmMusicFmDeviceFactory.CreateAdl(_ => renderer, new(), Resources());
        using var session = new VmMusicMidiSession(source, device);
        Assert.True(session.Start(600000));
        using var stream = new MemoryStream();
        using var writer = new VmMusicWaveWriter(stream, device.GetStreamInfo().SampleRate);
        writer.RenderUntilEnd(device.ServiceStream);
        Assert.True(writer.DataBytes > 0);
        Assert.Equal(0u, writer.DataBytes % 16384);
        Assert.Equal(writer.DataBytes + 68L, stream.Length);
        Assert.Equal(writer.DataBytes, System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(stream.ToArray().AsSpan(64)));
        session.Update();
        Assert.False(device.IsOpen);
        Assert.Equal(1, renderer.Disposals);
    }

    [Fact]
    public void RouterSelectsRequestedFmBackendWithoutAllocatingOtherRenderer()
    {
        var renderer = new Renderer();
        var adlAllocations = 0;
        var opnAllocations = 0;
        var factory = VmMusicFmDeviceFactory.CreateRouter<Renderer, Renderer>(
            _ => { adlAllocations++; return renderer; }, _ => { opnAllocations++; return new Renderer(); },
            new(), new(), Resources());
        var created = VmMusicMidiDeviceCreation.Create(VmMusicMidiDeviceKind.Adl, false, factory);
        Assert.Equal(VmMusicMidiDeviceKind.Adl, created.Selected);
        Assert.Equal(1, adlAllocations);
        Assert.Equal(0, opnAllocations);
        ((IDisposable)created.Device).Dispose();
        Assert.Equal(1, renderer.Disposals);
    }

    [Fact]
    public void MissingAdlFallsThroughExistingFallbackOrderToOwnedOpn()
    {
        using var source = Source();
        var renderer = new Renderer();
        var attempts = new List<VmMusicMidiDeviceKind>();
        var factory = VmMusicFmDeviceFactory.CreateRouter<Renderer, Renderer>(null,
            rate => { Assert.Equal(44100, rate); return renderer; }, new(), new(), Resources(),
            kind => { attempts.Add(kind); throw new VmMusicMidiDeviceUnavailableException("not installed"); });
        var playing = VmMusicMidiPlay.Start(source, VmMusicMidiDeviceKind.Default, -7, false, factory, new(600000));
        Assert.True(playing.Started);
        Assert.Equal(VmMusicMidiDeviceKind.Adl, playing.Requested);
        Assert.Equal(VmMusicMidiDeviceKind.Opn, playing.Selected);
        Assert.Equal(new[] { VmMusicMidiDeviceKind.FluidSynth, VmMusicMidiDeviceKind.Timidity,
            VmMusicMidiDeviceKind.WildMidi, VmMusicMidiDeviceKind.Gus }, attempts);
        playing.Session.Dispose();
        Assert.Equal(1, renderer.Disposals);
    }

    [Fact]
    public void RouterDoesNotHideInitializationErrorsWithFallback()
    {
        var failure = new InvalidOperationException("bad configuration");
        var renderer = new Renderer { InitializationFailure = failure };
        var opnAllocations = 0;
        var factory = VmMusicFmDeviceFactory.CreateRouter<Renderer, Renderer>(_ => renderer,
            _ => { opnAllocations++; return new Renderer(); }, new(), new(), Resources());
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() =>
            VmMusicMidiDeviceCreation.Create(VmMusicMidiDeviceKind.Adl, false, factory)));
        Assert.Equal(0, opnAllocations);
        Assert.Equal(1, renderer.Disposals);
        Assert.Throws<ArgumentOutOfRangeException>(() => factory(VmMusicMidiDeviceKind.Default));
    }

    [Fact]
    public void RenderFailureStopsSessionAndUpdateReleasesOwnedRenderer()
    {
        using var source = Source();
        var failure = new InvalidOperationException("audio generation failed");
        var renderer = new Renderer { RenderFailure = failure };
        var device = VmMusicFmDeviceFactory.CreateAdl(_ => renderer, new(), Resources());
        using var session = new VmMusicMidiSession(source, device);
        Assert.True(session.Start(600000));
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => device.ServiceStreamBytes(new byte[8192])));
        Assert.False(device.Update());
        session.Update();
        Assert.False(device.IsOpen);
        Assert.Equal(1, renderer.Disposals);
        Assert.False(session.IsPlaying());
    }

    private static VmMusicSmfPlaybackSource Source(int count = 300) => new(VmMusicSmf.Serialize(120, 500000,
        Enumerable.Range(0, count).Select(_ => new VmMusicSmfEvent(1, 0x643c90))));

    [Fact]
    public void SessionDisposesOwnedRendererWithoutStartup()
    {
        using var source = Source();
        var renderer = new Renderer();
        var device = VmMusicFmDeviceFactory.CreateAdl(_ => renderer, new(), Resources());
        var session = new VmMusicMidiSession(source, device);
        session.Dispose();
        session.Dispose();
        Assert.Equal(1, renderer.Disposals);
        Assert.Throws<ObjectDisposedException>(() => device.Open());
    }

    [Fact]
    public void StartupResetFailureDisposesOwnedRenderer()
    {
        using var source = Source();
        var failure = new InvalidOperationException("reset failed");
        var renderer = new Renderer { ResetFailure = failure };
        var device = VmMusicFmDeviceFactory.CreateAdl(_ => renderer, new(), Resources());
        using var session = new VmMusicMidiSession(source, device);
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => session.Start(600000)));
        Assert.Equal(1, renderer.Disposals);
        Assert.False(device.IsOpen);
    }

    [Fact]
    public void EndOfSongAndShortSongBothReleaseOwnedRenderer()
    {
        foreach (var count in new[] { 1, 300 })
        {
            using var source = Source(count);
            var renderer = new Renderer();
            var device = VmMusicFmDeviceFactory.CreateOpn(_ => renderer, new(), Resources());
            using var session = new VmMusicMidiSession(source, device);
            Assert.True(session.Start(600000));
            for (var i = 0; i < 200 && device.IsOpen; i++)
            {
                device.ServiceStream(new float[2048]);
                session.Update();
            }
            Assert.False(device.IsOpen);
            Assert.Equal(1, renderer.Disposals);
        }
    }

    [Fact]
    public void PlayFactoryTransfersOwnedDeviceToSession()
    {
        using var source = Source();
        var renderer = new Renderer();
        var playing = VmMusicMidiPlay.Start(source, VmMusicMidiDeviceKind.Adl, -7, false,
            _ => VmMusicFmDeviceFactory.CreateAdl(_ => renderer, new(), Resources()), new(600000));
        Assert.True(playing.Started);
        playing.Session.Dispose();
        Assert.Equal(1, renderer.Disposals);
        source.Restart();
        Assert.False(source.CheckDone());
    }

    [Fact]
    public void CleanupFailureIsReportedAndDisposalIsNotRepeated()
    {
        using var source = Source();
        var failure = new InvalidOperationException("dispose failed");
        var renderer = new Renderer { DisposalFailure = failure };
        var device = VmMusicFmDeviceFactory.CreateAdl(_ => renderer, new(), Resources());
        var session = new VmMusicMidiSession(source, device);
        Assert.True(session.Start(600000));
        var error = Assert.Throws<AggregateException>(() => session.Dispose());
        Assert.Contains(failure, error.InnerExceptions);
        Assert.False(device.IsOpen);
        session.Dispose();
        Assert.Equal(1, renderer.Disposals);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OwnedFactoryUsesNativeRateAndReleasesRendererOnce(bool adl)
    {
        var renderer = new Renderer();
        Renderer Allocate(int rate) { Assert.Equal(44100, rate); return renderer; }
        var device = adl ? VmMusicFmDeviceFactory.CreateAdl(Allocate, new(), Resources()) :
            VmMusicFmDeviceFactory.CreateOpn(Allocate, new(), Resources());
        device.Open();
        device.Close();
        Assert.Equal(0, renderer.Disposals);
        device.Open();
        device.Dispose();
        device.Dispose();
        Assert.Equal(1, renderer.Disposals);
        Assert.False(device.IsOpen);
        Assert.Throws<ObjectDisposedException>(() => device.Open());
    }

    [Fact]
    public void FailedInitializationDisposesRendererAndPreservesFailure()
    {
        var failure = new InvalidOperationException("chips rejected");
        var renderer = new Renderer { InitializationFailure = failure };
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() =>
            VmMusicFmDeviceFactory.CreateAdl(_ => renderer, new(), Resources())));
        Assert.Equal(1, renderer.Disposals);
    }

    [Fact]
    public void MissingRendererIsExplicitlyUnavailable()
    {
        Assert.Throws<VmMusicMidiDeviceUnavailableException>(() =>
            VmMusicFmDeviceFactory.CreateOpn<Renderer>(_ => null, new(), Resources()));
    }

    private static VmMusicStartupData Resources() => VmMusicStartupData.Load(_ => null, _ => [17], _ => { });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ConfiguredDevicesApplyRuntimeGainToAudio(bool adl)
    {
        var renderer = new Renderer();
        var device = adl ? VmMusicFmSessionDevice.CreateAdl(renderer, 1000, new(), Resources()) :
            VmMusicFmSessionDevice.CreateOpn(renderer, 1000, new(), Resources());
        Assert.Equal(adl ? 2f : 4f, device.OutputGain);
        device.Open();
        device.Prepare(0, new uint[] { 100, 0, 0x003c90 });
        device.StreamOut(0);
        device.StartStream();
        Assert.True(device.ChangeNumber(adl ? "libadl.gain" : "libopn.gain", 0.5));
        var samples = new float[2];
        device.ServiceStream(samples);
        Assert.Equal(adl ? new float[] { 1, 1 } : new float[] { 2, 2 }, samples);
        Assert.False(device.ChangeInteger("other.numchips", 9));
        Assert.False(device.ChangeString("other.custombank", "bank"));
        device.Close();
    }

    [Fact]
    public void AdlBankAndVolumeChangesUpdateDeviceGainAutomatically()
    {
        var renderer = new Renderer();
        var device = VmMusicFmSessionDevice.CreateAdl(renderer, 1000, new(), Resources());
        device.Open();
        renderer.Model = 10;
        Assert.True(device.ChangeInteger("libadl.banknum", 8));
        Assert.Equal(8, renderer.Bank);
        Assert.Equal(2.5f, device.OutputGain);
        Assert.True(device.ChangeInteger("libadl.volumemodel", 2));
        Assert.Equal(3.8f, device.OutputGain);
        device.Close();
    }

    [Fact]
    public void OpnBankChangesReachConfiguredRenderer()
    {
        var renderer = new Renderer();
        var device = VmMusicFmSessionDevice.CreateOpn(renderer, 1000, new(), Resources());
        device.Open();
        device.ChangeString("libopn.custombank", "custom");
        Assert.Null(renderer.File);
        device.ChangeInteger("libopn.usecustombank", 1);
        Assert.Equal("custom", renderer.File);
        Assert.Equal(4, device.OutputGain);
        device.Close();
    }

    private sealed class Renderer : IVmMusicAdlRenderer, IVmMusicOpnRenderer, IDisposable
    {
        public int Disposals { get; private set; }
        public Exception? InitializationFailure { get; init; }
        public Exception? ResetFailure { get; init; }
        public Exception? DisposalFailure { get; init; }
        public Exception? RenderFailure { get; init; }
        public void Dispose() { Disposals++; if (DisposalFailure is not null) throw DisposalFailure; }
        public int Model { get; set; } = 1;
        public int Bank { get; private set; }
        public string? File { get; private set; }
        public void ResetState() { if (ResetFailure is not null) throw ResetFailure; }
        public int Generate(Span<float> samples) { if (RenderFailure is not null) throw RenderFailure; samples.Fill(1); return samples.Length; }
        public int GetVolumeRangeModel() => Model;
        public int OpenBankData(ReadOnlyMemory<byte> bank) => 0;
        public int OpenBankFile(string name) { File = name; return 0; }
        public void SetBuiltinBank(int bank) => Bank = bank;
        public void SwitchEmulator(int emulator) { }
        public void SetRunAtPcmRate(bool enabled) { }
        public void SetNumChips(int chips) { if (InitializationFailure is not null) throw InitializationFailure; }
        public void SetVolumeRangeModel(int model) { if (model != 0) Model = model; }
        public void SetChannelAllocation(int mode) { }
        public void SetSoftPan(bool enabled) { }
        public void SetAutoArpeggio(bool enabled) { }
        public void NoteOn(int channel, int note, int velocity) { }
        public void NoteOff(int channel, int note) { }
        public void NoteAfterTouch(int channel, int note, int pressure) { }
        public void ControllerChange(int channel, int controller, int value) { }
        public void PatchChange(int channel, int program) { }
        public void ChannelAfterTouch(int channel, int pressure) { }
        public void PitchBend(int channel, int mostSignificant, int leastSignificant) { }
        public void SystemExclusive(ReadOnlySpan<byte> data) { }
    }
}
