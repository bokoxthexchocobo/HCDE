namespace HCDE.Client.Tests;

public class VmMusicMidiSessionTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void InlineStartupCompletionsAreServicedAfterStreamStartInQueueOrder(int completions)
    {
        using var source = Source(1500);
        var device = new Device { InlineStartupCompletions = completions };
        using var session = new VmMusicMidiSession(source, device);
        Assert.True(session.Start(600000));
        Assert.All(device.InlineResults, result => Assert.Equal(0, result));
        var start = device.Calls.IndexOf("start");
        var returned = device.Calls.IndexOf("start:return");
        Assert.DoesNotContain(device.Calls.Skip(start).Take(returned - start), call => call.StartsWith("unprepare:"));
        var serviced = device.Calls.Skip(returned + 1).ToArray();
        var expected = new[] { "unprepare:0", "prepare:0", "stream:0", "unprepare:1", "prepare:1", "stream:1" };
        Assert.Equal(expected.Take(completions * 3), serviced);
        Assert.True(session.IsPlaying());
        Assert.Equal(0, session.LastServiceError);
    }

    [Fact]
    public void FailedStreamStartDiscardsInlineCompletionsAndCloses()
    {
        using var source = Source(1500);
        var device = new Device { InlineStartupCompletions = 2, Failure = "start" };
        using var session = new VmMusicMidiSession(source, device);
        Assert.Throws<InvalidOperationException>(() => session.Start(600000));
        Assert.DoesNotContain("stream:0", device.Calls);
        Assert.DoesNotContain("stream:1", device.Calls);
        Assert.False(device.IsOpen);
        Assert.Null(device.CompletionCallback);
        Assert.Equal(0, session.ServiceCompletion());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InlineStartupCompletionsHonorEndOfSongAndRepeat(bool repeat)
    {
        using var source = Source(180);
        var device = new Device { InlineStartupCompletions = 2 };
        using var session = new VmMusicMidiSession(source, device);
        Assert.True(session.Start(600000, repeatSong: repeat));
        Assert.Equal(repeat ? VmMusicMidiEndState.Playing : VmMusicMidiEndState.StopQueued, session.Startup.Buffers.EndState);
        Assert.Equal(repeat, session.IsPlaying());
        Assert.Equal(repeat, device.IsOpen);
        Assert.Equal(2, device.InlineResults.Count);
    }

    [Fact]
    public void ShortSongNeverArmsInlineStartupCompletionCapture()
    {
        using var source = Source(1);
        var device = new Device { InlineStartupCompletions = 2 };
        using var session = new VmMusicMidiSession(source, device);
        Assert.False(session.Start(600000));
        Assert.Empty(device.InlineResults);
        Assert.DoesNotContain("start", device.Calls);
        Assert.False(device.IsOpen);
        Assert.Null(device.CompletionCallback);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InlineCompletionServiceFailurePropagatesAndCleansUp(bool throws)
    {
        using var source = Source(1500);
        var device = new Device { InlineStartupCompletions = 1 };
        if (throws) device.ThrowOnCall = "unprepare:0";
        else device.Failure = "unprepare:0";
        using var session = new VmMusicMidiSession(source, device);
        var error = Assert.Throws<AggregateException>(() => session.Start(600000));
        Assert.Contains(error.InnerExceptions, failure => failure.Message.Contains(throws ? "unprepare:0 threw" : "Initial MIDI completion failed"));
        Assert.Equal(throws ? -1 : 17, session.LastServiceError);
        Assert.Equal(VmMusicMidiPlaybackState.Stopped, session.Startup.Lifecycle.State);
        Assert.False(device.IsOpen);
        Assert.Null(device.CompletionCallback);
        Assert.DoesNotContain("stream:0", device.Calls);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void MusAndMidsLiveStartupLeavesWholeSongRepeatToSession(bool mids, bool repeat)
    {
        using var source = LegacySource(mids);
        var device = new Device();
        using var session = new VmMusicMidiSession(source, device);
        Assert.True(session.Start(600000, repeatSong: repeat, liveLoops: true));
        Assert.Equal(0, source.LoopLimit);
        if (repeat)
        {
            for (var i = 0; i < 10; i++) Assert.Equal(0, session.ServiceCompletion());
            Assert.True(session.IsPlaying());
            Assert.Equal(VmMusicMidiEndState.Playing, session.Startup.Buffers.EndState);
        }
        else
        {
            for (var i = 0; i < 10 && session.Startup.Buffers.EndState == VmMusicMidiEndState.Playing; i++)
                Assert.Equal(0, session.ServiceCompletion());
            Assert.Equal(VmMusicMidiEndState.StopPending, session.Startup.Buffers.EndState);
            Assert.False(session.IsPlaying());
            Assert.False(device.IsOpen);
        }
        Assert.Contains(device.Packets, packet => packet.Contains(mids ? 0x403c90u : 0x7f3c90u));
    }

    private static IVmMusicSmfSource LegacySource(bool mids)
    {
        const int count = 300;
        if (!mids)
        {
            var bytes = new byte[16 + count * 2 + 1];
            "MUS\u001a"u8.CopyTo(bytes);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(4), count * 2 + 1);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(6), 16);
            for (var i = 0; i < count; i++) { bytes[16 + i * 2] = 0x10; bytes[17 + i * 2] = 60; }
            bytes[^1] = 0x60;
            return new VmMusicMusSource(bytes);
        }
        var data = new byte[52 + count * 8];
        "RIFF"u8.CopyTo(data); "MIDSfmt "u8.CopyTo(data.AsSpan(8)); "data"u8.CopyTo(data.AsSpan(32));
        void Put(int offset, uint value) => System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(offset), value);
        Put(4, (uint)data.Length - 8); Put(16, 12); Put(20, 480); Put(28, 1);
        Put(36, (uint)data.Length - 40); Put(40, 1); Put(48, count * 8);
        for (var i = 0; i < count; i++) Put(56 + i * 8, 0x403c90);
        return new VmMusicMidsSource(data);
    }

    [Fact]
    public void LiveSingleTrackEmidiLoopsPrimeAndRefillThroughSession()
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000,
            [new(0, 0x0074b0), new(2, 0x403c90), new(3, 0x7f75b0)]));
        var device = new Device();
        using var session = new VmMusicMidiSession(source, device);
        Assert.True(session.Start(600000, liveLoops: true));
        for (var i = 0; i < 5; i++) Assert.Equal(0, session.ServiceCompletion());
        Assert.True(session.IsPlaying());
        Assert.False(source.CheckDone());
    }
    [Theory]
    [InlineData(1, false)]
    [InlineData(4, true)]
    public void SessionCapabilitiesControlEmidiPrecacheAndPlayback(int technology, bool included)
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000,
            new VmMusicSmfEvent[] { new(0, 0x076eb0), new(0, 0x05c0) }.Concat(
                Enumerable.Range(0, 300).Select(_ => new VmMusicSmfEvent(1, 0x643c90)))));
        var device = new Device { Technology = technology };
        using var session = new VmMusicMidiSession(source, device);
        Assert.True(session.Start(600000));
        Assert.Equal(included ? new ushort[] { 5 } : Array.Empty<ushort>(), device.Instruments);
        if (included) Assert.Contains(0x643c90u, device.Packets[0]);
        else Assert.DoesNotContain(0x643c90u, device.Packets[0]);
    }
    [Fact]
    public void PlayConnectsSelectionFallbackStartupAndSessionOwnership()
    {
        using var source = Source(1000);
        var device = new Device();
        List<VmMusicMidiDeviceKind> attempts = [];
        var playing = VmMusicMidiPlay.Start(source, VmMusicMidiDeviceKind.Default, -3, true, kind =>
        {
            attempts.Add(kind);
            if (kind == VmMusicMidiDeviceKind.Opl) throw new VmMusicMidiDeviceUnavailableException("OPL missing");
            return device;
        }, new(600000, RepeatSong: true, MusicVolume: 0.5f));
        using var session = playing.Session;
        Assert.True(playing.Started);
        Assert.Equal(new[] { VmMusicMidiDeviceKind.Opl, VmMusicMidiDeviceKind.FluidSynth }, attempts);
        Assert.Equal(VmMusicMidiDeviceKind.Opl, playing.Requested);
        Assert.Equal(VmMusicMidiDeviceKind.FluidSynth, playing.Selected);
        Assert.Contains(0x3207b0u, device.Packets[0]);
        Assert.True(session.IsPlaying());
        session.Dispose();
        Assert.False(device.IsOpen);
        source.Restart();
    }

    [Fact]
    public void PlayValidatesSubsongBeforeCreatingDevice()
    {
        using var source = Source();
        var creations = 0;
        Assert.Throws<ArgumentOutOfRangeException>(() => VmMusicMidiPlay.Start(source,
            VmMusicMidiDeviceKind.Default, -5, false, _ => { creations++; return new Device(); }, new(600000, Subsong: 1)));
        Assert.Equal(0, creations);
    }

    [Fact]
    public void PlayDoesNotRetryCreationAfterDeviceOpenFails()
    {
        using var source = Source();
        var device = new Device { Failure = "open" };
        var creations = 0;
        Assert.Throws<InvalidOperationException>(() => VmMusicMidiPlay.Start(source,
            VmMusicMidiDeviceKind.Default, -5, true, _ => { creations++; return device; }, new(600000)));
        Assert.Equal(1, creations);
        Assert.False(device.IsOpen);
        Assert.Null(device.CompletionCallback);
        Assert.Single(device.Calls, call => call == "close");
    }

    [Fact]
    public void PlayReturnsClosedSessionForShortSong()
    {
        using var source = Source(1);
        var device = new Device();
        var playing = VmMusicMidiPlay.Start(source, VmMusicMidiDeviceKind.FluidSynth, 0, true,
            _ => device, new(600000));
        using var session = playing.Session;
        Assert.False(playing.Started);
        Assert.False(device.IsOpen);
        Assert.False(session.IsPlaying());
    }

    [Fact]
    public void PlayForwardsSelectedXmiSubsongAndLiveLoopMode()
    {
        using var source = new VmMusicXmiSource(XmiDocument([0xc0, 1],
            [0xb0, 116, 0, 1, 0xc0, 7, 0xb0, 117, 127]));
        var device = new Device();
        var playing = VmMusicMidiPlay.Start(source, VmMusicMidiDeviceKind.FluidSynth, 0, true,
            _ => device, new(600000, Subsong: 1, LiveLoops: true));
        using var session = playing.Session;
        Assert.True(playing.Started);
        Assert.Equal(1, source.CurrentSong);
        Assert.Equal(new ushort[] { 7 }, device.Instruments);
        Assert.Equal(0, session.ServiceCompletion());
        Assert.False(source.CheckDone());
    }
    [Theory]
    [InlineData(-1, 2)]
    [InlineData(-2, 3)]
    [InlineData(-3, 1)]
    [InlineData(-4, 5)]
    [InlineData(-5, 4)]
    [InlineData(-6, 6)]
    [InlineData(-7, 7)]
    [InlineData(-8, 8)]
    [InlineData(0, 0)]
    [InlineData(42, 0)]
    public void NativeMidiDeviceSettingSelectsExpectedBackend(int setting, int expected)
    {
        Assert.Equal((VmMusicMidiDeviceKind)expected,
            VmMusicMidiDeviceCreation.Select(VmMusicMidiDeviceKind.Default, setting, true));
        Assert.Equal(VmMusicMidiDeviceKind.Opl,
            VmMusicMidiDeviceCreation.Select(VmMusicMidiDeviceKind.Opl, setting, true));
    }

    [Fact]
    public void FactoryFallbackUsesNativeOrderWithoutRetryingRequestedDevice()
    {
        List<VmMusicMidiDeviceKind> calls = [];
        var device = new Device();
        var result = VmMusicMidiDeviceCreation.Create(VmMusicMidiDeviceKind.Gus, true, kind =>
        {
            calls.Add(kind);
            if (kind != VmMusicMidiDeviceKind.Adl) throw new VmMusicMidiDeviceUnavailableException(kind.ToString());
            return device;
        });
        Assert.Equal(new[] { VmMusicMidiDeviceKind.Gus, VmMusicMidiDeviceKind.FluidSynth,
            VmMusicMidiDeviceKind.Timidity, VmMusicMidiDeviceKind.WildMidi, VmMusicMidiDeviceKind.Standard,
            VmMusicMidiDeviceKind.Adl }, calls);
        Assert.Same(device, result.Device);
        Assert.Equal(VmMusicMidiDeviceKind.Gus, result.Requested);
        Assert.Equal(VmMusicMidiDeviceKind.Adl, result.Selected);
        Assert.Empty(device.Calls);
    }

    [Fact]
    public void MissingSystemMidiMapsToFluidSynthAndSoundSystemNormalizes()
    {
        var result = VmMusicMidiDeviceCreation.Create(VmMusicMidiDeviceKind.Standard, false,
            kind => { Assert.Equal(VmMusicMidiDeviceKind.FluidSynth, kind); return new Device(); });
        Assert.Equal(VmMusicMidiDeviceKind.FluidSynth, result.Selected);
        Assert.Equal(VmMusicMidiDeviceKind.SoundSystem,
            VmMusicMidiDeviceCreation.Select(VmMusicMidiDeviceKind.Default, 42, false));
        result = VmMusicMidiDeviceCreation.Create(VmMusicMidiDeviceKind.SoundSystem, false, _ => new Device());
        Assert.Equal(VmMusicMidiDeviceKind.FluidSynth, result.Requested);
    }

    [Fact]
    public void ExhaustedFactoriesReportEveryFailureAndTerminate()
    {
        var failure = Assert.Throws<AggregateException>(() => VmMusicMidiDeviceCreation.Create(
            VmMusicMidiDeviceKind.FluidSynth, false, kind => throw new VmMusicMidiDeviceUnavailableException(kind.ToString())));
        Assert.Equal(7, failure.InnerExceptions.Count);
        Assert.All(failure.InnerExceptions, error => Assert.IsType<VmMusicMidiDeviceUnavailableException>(error));
    }

    [Fact]
    public void FactoryProgrammingErrorsAndNullResultsDoNotFallBack()
    {
        var calls = 0;
        Assert.Throws<ArgumentException>(() => VmMusicMidiDeviceCreation.Create(VmMusicMidiDeviceKind.Opl, true,
            _ => { calls++; throw new ArgumentException("bad factory"); }));
        Assert.Equal(1, calls);
        Assert.Throws<InvalidOperationException>(() => VmMusicMidiDeviceCreation.Create(VmMusicMidiDeviceKind.Opl,
            true, _ => null!));
    }
    [Fact]
    public void LiveXmiLoopStartsAndRefillsWithoutSongEnd()
    {
        using var source = new VmMusicXmiSource(XmiDocument([0xb0, 116, 0, 1, 0xc0, 7, 0xb0, 117, 127]));
        var device = new Device();
        using var session = new VmMusicMidiSession(source, device);
        Assert.True(session.Start(600000, liveLoops: true));
        for (var i = 0; i < 5; i++) Assert.Equal(0, session.ServiceCompletion());
        Assert.True(session.IsPlaying());
        Assert.False(source.CheckDone());
    }
    private sealed class Device : IVmMusicMidiSessionDevice
    {
        public bool IsOpen { get; set; }
        public int Technology { get; set; } = 1;
        public bool CanHandleSysex { get; set; } = true;
        public bool FakeVolume => true;
        public List<uint[]> Packets { get; } = [];
        public IReadOnlyList<ushort> Instruments { get; private set; } = [];
        public string? Failure { get; set; }
        public string? ThrowOnCall { get; set; }
        public bool FailCleanup { get; set; }
        public bool Alive { get; set; } = true;
        public bool ThrowOnUpdate { get; set; }
        public int InlineStartupCompletions { get; set; }
        public List<int> InlineResults { get; } = [];
        public bool Update()
        {
            Calls.Add("update");
            if (ThrowOnUpdate) throw new InvalidOperationException("update failed");
            return Alive;
        }
        public List<string> Calls { get; } = [];
        public Func<int>? CompletionCallback { get; private set; }
        public void SetCompletionCallback(Func<int>? callback) => CompletionCallback = callback;
        private int Call(string name)
        {
            Calls.Add(name);
            if (ThrowOnCall == name) throw new InvalidOperationException($"{name} threw");
            return Failure == name ? 17 : 0;
        }
        public int Open() { IsOpen = true; return Call("open"); }
        public void Stop()
        {
            Calls.Add("stop");
            if (FailCleanup) throw new InvalidOperationException("stop failed");
        }
        public void Close() { Calls.Add("close"); IsOpen = false; }
        public int Unprepare(int number) { var error = Call($"unprepare:{number}"); return FailCleanup ? 8 : error; }
        public void PrecacheInstruments(IReadOnlyList<ushort> instruments)
        { Instruments = instruments; Calls.Add("precache"); }
        public int SetTimeDivision(ushort division) => Call("division");
        public int SetTempo(uint tempo) => Call("tempo");
        public void InitializePlayback() => Calls.Add("initialize");
        public int StartStream()
        {
            var result = Call("start");
            for (var i = 0; i < InlineStartupCompletions; i++) InlineResults.Add(CompletionCallback!());
            Calls.Add("start:return");
            return result;
        }
        public bool Pause(bool paused) => false;
        public int Prepare(int number, ReadOnlyMemory<uint> events)
        { Packets.Add(events.ToArray()); return Call($"prepare:{number}"); }
        public int StreamOut(int number) => Call($"stream:{number}");
        public int StreamOutSync(int number) => Call($"sync:{number}");
    }

    private static VmMusicSmfPlaybackSource Source(int count = 300) => new(
        VmMusicSmf.Serialize(120, 500000, Enumerable.Range(0, count).Select(_ => new VmMusicSmfEvent(1, 0x643c90))));

    [Fact]
    public void SuccessfulSessionClosesInNativeOrderAndBorrowsSource()
    {
        using var source = Source();
        var device = new Device();
        var session = new VmMusicMidiSession(source, device);
        Assert.True(session.Start(600000));
        Assert.Equal("open", device.Calls[0]);
        Assert.True(device.IsOpen);
        session.Dispose();
        Assert.Equal(new[] { "stop", "unprepare:0", "unprepare:1", "close" }, device.Calls.TakeLast(4));
        Assert.Equal(VmMusicMidiPlaybackState.Stopped, session.Startup.Lifecycle.State);
        Assert.False(device.IsOpen);
        var calls = device.Calls.Count;
        session.Dispose();
        Assert.Equal(calls, device.Calls.Count);
        source.Restart();
        Assert.Throws<ObjectDisposedException>(() => session.Start(600000));
    }

    [Theory]
    [InlineData("open")]
    [InlineData("division")]
    [InlineData("prepare:1")]
    [InlineData("start")]
    public void FailedStartupReleasesEvenPartiallyOpenedDevices(string failure)
    {
        using var source = Source();
        var device = new Device { Failure = failure };
        using var session = new VmMusicMidiSession(source, device);
        Assert.Contains("17", Assert.Throws<InvalidOperationException>(() => session.Start(600000)).Message);
        Assert.False(device.IsOpen);
        Assert.Equal("close", device.Calls[^1]);
        Assert.Equal(VmMusicMidiPlaybackState.Stopped, session.Startup.Lifecycle.State);
    }

    [Fact]
    public void CleanupReportsAllErrorsWithoutLosingStartupFailure()
    {
        using var source = Source();
        var device = new Device { Failure = "start", FailCleanup = true };
        using var session = new VmMusicMidiSession(source, device);
        var error = Assert.Throws<AggregateException>(() => session.Start(600000));
        Assert.Equal(4, error.InnerExceptions.Count);
        Assert.Contains("17", error.InnerExceptions[0].Message);
        Assert.Equal(new[] { "stop", "unprepare:0", "unprepare:1", "close" }, device.Calls.TakeLast(4));
        Assert.False(device.IsOpen);
    }

    [Fact]
    public void ShortSongClosesWithoutStartingStream()
    {
        using var source = Source(1);
        var device = new Device();
        using var session = new VmMusicMidiSession(source, device);
        Assert.False(session.Start(600000));
        Assert.False(device.IsOpen);
        Assert.DoesNotContain("start", device.Calls);
        Assert.Equal("close", device.Calls[^1]);
    }

    [Fact]
    public void DeviceWithoutSysexSupportStartsWithSuppressionAndRestoresBorrowedSource()
    {
        using var source = Source();
        var device = new Device { CanHandleSysex = false };
        using var session = new VmMusicMidiSession(source, device);
        Assert.True(session.Start(600000));
        Assert.True(source.SkipSysex);
        session.Dispose();
        Assert.False(source.SkipSysex);
        Assert.False(device.IsOpen);
    }

    [Fact]
    public void PreopenedDeviceIsRejectedWithoutTakingOwnership()
    {
        using var source = Source();
        var device = new Device { IsOpen = true };
        var session = new VmMusicMidiSession(source, device);
        Assert.Throws<InvalidOperationException>(() => session.Start(600000));
        session.Dispose();
        Assert.Empty(device.Calls);
        Assert.True(device.IsOpen);
    }

    [Fact]
    public void CompletionRefillsUntilEndAndHostPollClosesSession()
    {
        using var source = Source();
        var device = new Device();
        using var session = new VmMusicMidiSession(source, device);
        Assert.True(session.Start(600000));
        var callback = Assert.IsType<Func<int>>(device.CompletionCallback);
        Assert.True(session.IsPlaying());
        for (var i = 0; i < 10 && session.Startup.Buffers.EndState == VmMusicMidiEndState.Playing; i++)
            Assert.Equal(0, callback());
        Assert.Equal(VmMusicMidiEndState.StopPending, session.Startup.Buffers.EndState);
        Assert.False(session.IsPlaying());
        Assert.Null(device.CompletionCallback);
        var calls = device.Calls.Count;
        Assert.Equal(0, callback());
        Assert.Equal(calls, device.Calls.Count);
    }

    [Fact]
    public void CallbackDeviceErrorIsRetainedAndFurtherRefillsAreGated()
    {
        using var source = Source(1000);
        var device = new Device();
        using var session = new VmMusicMidiSession(source, device);
        Assert.True(session.Start(600000));
        device.Failure = "prepare:0";
        Assert.Equal(17, device.CompletionCallback!());
        var calls = device.Calls.Count;
        Assert.Equal(17, session.ServiceCompletion());
        Assert.Equal(calls, device.Calls.Count);
        Assert.False(session.IsPlaying());
        Assert.Equal(17, session.LastServiceError);
        Assert.Null(device.CompletionCallback);
    }

    [Fact]
    public void PausedCompletionRefillsWithoutAdvancingSourceAndExternalCloseIsDetected()
    {
        using var source = Source(1000);
        var device = new Device();
        using var session = new VmMusicMidiSession(source, device);
        Assert.True(session.Start(600000));
        session.Startup.Lifecycle.Pause();
        Assert.Equal(0, device.CompletionCallback!());
        Assert.False(source.CheckDone());
        Assert.True(session.IsPlaying());
        device.IsOpen = false;
        Assert.False(session.IsPlaying());
        Assert.Null(device.CompletionCallback);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UpdateKeepsLivePlaybackAndClosesDeadBackendEvenWhenPaused(bool paused)
    {
        using var source = Source(1000);
        var device = new Device { CanHandleSysex = false };
        using var session = new VmMusicMidiSession(source, device);
        Assert.True(session.Start(600000));
        if (paused) session.Startup.Lifecycle.Pause();
        session.Update();
        Assert.True(session.IsPlaying());
        var callback = device.CompletionCallback!;
        device.Alive = false;
        session.Update();
        Assert.Equal(new[] { "update", "stop", "unprepare:0", "unprepare:1", "close" }, device.Calls.TakeLast(5));
        Assert.False(session.IsPlaying());
        Assert.False(source.SkipSysex);
        Assert.Null(device.CompletionCallback);
        var calls = device.Calls.Count;
        Assert.Equal(0, callback());
        session.Update();
        Assert.Equal(calls, device.Calls.Count);
    }

    [Fact]
    public void UpdateBeforeStartupDoesNotTouchDevice()
    {
        using var source = Source();
        var device = new Device();
        using var session = new VmMusicMidiSession(source, device);
        session.Update();
        Assert.Empty(device.Calls);
        Assert.False(session.IsPlaying());
    }

    [Fact]
    public void UpdateExceptionIsPropagatedAfterClosingDevice()
    {
        using var source = Source();
        var device = new Device { ThrowOnUpdate = true };
        using var session = new VmMusicMidiSession(source, device);
        Assert.True(session.Start(600000));
        Assert.Equal("update failed", Assert.Throws<InvalidOperationException>(() => session.Update()).Message);
        Assert.False(device.IsOpen);
        Assert.False(session.IsPlaying());
        Assert.Null(device.CompletionCallback);
    }

    [Fact]
    public void UpdateExceptionAndCleanupErrorsAreReportedTogether()
    {
        using var source = Source();
        var device = new Device { ThrowOnUpdate = true };
        using var session = new VmMusicMidiSession(source, device);
        Assert.True(session.Start(600000));
        device.FailCleanup = true;
        var error = Assert.Throws<AggregateException>(() => session.Update());
        Assert.Equal(4, error.InnerExceptions.Count);
        Assert.Equal("update failed", error.InnerExceptions[0].Message);
        Assert.False(device.IsOpen);
        Assert.Equal("close", device.Calls[^1]);
    }

    private static byte[] XmiDocument(params byte[][] songs)
    {
        static byte[] Chunk(string id, byte[] payload)
        {
            var result = new byte[8 + payload.Length + (payload.Length & 1)];
            System.Text.Encoding.ASCII.GetBytes(id).CopyTo(result, 0);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(result.AsSpan(4), (uint)payload.Length);
            payload.CopyTo(result, 8);
            return result;
        }
        return Chunk("CAT ", "XMID"u8.ToArray().Concat(songs.SelectMany(song =>
            Chunk("FORM", "XMID"u8.ToArray().Concat(Chunk("EVNT", song)).ToArray()))).ToArray());
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    public void SelectedXmiSubsongControlsPrecacheAndQueuedEvents(int selected, int program)
    {
        var song1 = Enumerable.Range(0, 300).SelectMany(_ => new byte[] { 1, 0xc0, 1 }).ToArray();
        var song2 = Enumerable.Range(0, 300).SelectMany(_ => new byte[] { 1, 0xc0, 2 }).ToArray();
        using var source = new VmMusicXmiSource(XmiDocument(song1, song2));
        Assert.True(source.SetSubsong(1 - selected));
        var device = new Device();
        using var session = new VmMusicMidiSession(source, device);
        Assert.True(session.Start(600000, subsong: selected));
        Assert.Equal(selected, source.CurrentSong);
        Assert.Equal(new ushort[] { (ushort)program }, device.Instruments);
        Assert.Contains((uint)(0xc0 | program << 8), device.Packets[0]);
        Assert.DoesNotContain((uint)(0xc0 | (3 - program) << 8), device.Packets[0]);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    public void InvalidSingleSongSelectionDoesNotOpenDeviceAndAllowsCorrectedStart(int selected)
    {
        using var source = Source();
        var device = new Device();
        using var session = new VmMusicMidiSession(source, device);
        Assert.Throws<ArgumentOutOfRangeException>(() => session.Start(600000, subsong: selected));
        Assert.Empty(device.Calls);
        Assert.True(session.Start(600000));
    }

    [Fact]
    public void InvalidXmiSelectionPreservesCurrentSongAndAvoidsDeviceAccess()
    {
        using var source = new VmMusicXmiSource(XmiDocument([0xc0, 1], [0xc0, 2]));
        Assert.True(source.SetSubsong(1));
        var device = new Device();
        using var session = new VmMusicMidiSession(source, device);
        Assert.Throws<ArgumentOutOfRangeException>(() => session.Start(600000, subsong: 2));
        Assert.Equal(1, source.CurrentSong);
        Assert.Empty(device.Calls);
    }

    [Fact]
    public void RepeatRestartsWholeSongWithoutStoppingOrReopeningDevice()
    {
        using var source = Source();
        var device = new Device();
        using var session = new VmMusicMidiSession(source, device);
        Assert.True(session.Start(600000, repeatSong: true));
        for (var i = 0; i < 12; i++)
        {
            Assert.Equal(0, device.CompletionCallback!());
            Assert.True(session.IsPlaying());
        }
        Assert.Equal(VmMusicMidiEndState.Playing, session.Startup.Buffers.EndState);
        Assert.Single(device.Calls, call => call == "open");
        Assert.Single(device.Calls, call => call == "start");
        Assert.DoesNotContain("stop", device.Calls);
        Assert.Contains(device.Packets.Skip(2), packet => packet[2] == 0x0107a120u);
    }

    [Fact]
    public void RepeatAtPausedSongBoundaryDefersRestartUntilResume()
    {
        using var source = Source();
        var device = new Device();
        using var session = new VmMusicMidiSession(source, device);
        Assert.True(session.Start(600000, repeatSong: true));
        for (var i = 0; i < 10 && !source.CheckDone(); i++) Assert.Equal(0, session.ServiceCompletion());
        Assert.True(source.CheckDone());
        session.Startup.Lifecycle.Pause();
        Assert.Equal(0, session.ServiceCompletion());
        Assert.True(source.CheckDone());
        Assert.True(session.IsPlaying());
        session.Startup.Lifecycle.Resume();
        Assert.Equal(0, session.ServiceCompletion());
        Assert.False(source.CheckDone());
        Assert.True(session.IsPlaying());
    }

    [Fact]
    public void RepeatStillReportsRefillFailuresAndRejectsNativeShortSongs()
    {
        using var source = Source(1000);
        var device = new Device();
        using var session = new VmMusicMidiSession(source, device);
        Assert.True(session.Start(600000, repeatSong: true));
        device.Failure = "prepare:0";
        Assert.Equal(17, session.ServiceCompletion());
        Assert.False(session.IsPlaying());
        using var shortSource = Source(1);
        var shortDevice = new Device();
        using var shortSession = new VmMusicMidiSession(shortSource, shortDevice);
        Assert.False(shortSession.Start(600000, repeatSong: true));
        Assert.False(shortDevice.IsOpen);
    }

    [Theory]
    [InlineData("unprepare:0")]
    [InlineData("prepare:0")]
    [InlineData("stream:0")]
    public void CallbackExceptionIsRetainedRethrownAndGatesFurtherRefills(string operation)
    {
        using var source = Source(1000);
        var device = new Device();
        using var session = new VmMusicMidiSession(source, device);
        Assert.True(session.Start(600000));
        var callback = device.CompletionCallback!;
        device.ThrowOnCall = operation;
        var error = Assert.Throws<InvalidOperationException>(() => callback());
        Assert.Same(error, session.LastServiceException);
        Assert.Equal(-1, session.LastServiceError);
        Assert.Equal(VmMusicMidiPlaybackState.Stopped, session.Startup.Lifecycle.State);
        var calls = device.Calls.Count;
        Assert.Equal(-1, callback());
        Assert.Equal(calls, device.Calls.Count);
        device.ThrowOnCall = null;
        Assert.False(session.IsPlaying());
        Assert.False(device.IsOpen);
        Assert.Null(device.CompletionCallback);
    }

    [Fact]
    public void DecoderExceptionCannotLeaveSessionReportingActivePlayback()
    {
        var source = Source(1000);
        var device = new Device();
        using var session = new VmMusicMidiSession(source, device);
        Assert.True(session.Start(600000));
        source.Dispose();
        Assert.Throws<ObjectDisposedException>(() => session.ServiceCompletion());
        Assert.False(session.IsPlaying());
        Assert.False(device.IsOpen);
    }

    [Fact]
    public void CallbackExceptionAndDeferredCleanupFailuresRetainOriginalDiagnostic()
    {
        using var source = Source(1000);
        var device = new Device();
        using var session = new VmMusicMidiSession(source, device);
        Assert.True(session.Start(600000));
        device.ThrowOnCall = "prepare:0";
        var original = Assert.Throws<InvalidOperationException>(() => session.ServiceCompletion());
        device.ThrowOnCall = null;
        device.FailCleanup = true;
        var cleanup = Assert.Throws<AggregateException>(() => session.IsPlaying());
        Assert.Equal(4, cleanup.InnerExceptions.Count);
        Assert.Same(original, cleanup.InnerExceptions[0]);
        Assert.False(device.IsOpen);
        Assert.False(session.IsPlaying());
    }
}
