namespace HCDE.Client.Tests;

public class VmMusicFmConfiguredDeviceTests
{
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
            Assert.Equal(count > 1, session.Start(600000));
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
