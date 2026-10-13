namespace HCDE.Client.Tests;

public class VmMusicWaveExportTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SourceCleanupFailurePreservesDumpAndOptionalRestorationErrors(bool restoreFails)
    {
        var dumpError = new IOException("dump failed");
        var restoreError = new IOException("restore failed");
        var cleanupError = new IOException("source cleanup failed");
        var source = new Source { DisposeError = cleanupError };
        var restored = false;
        var error = Assert.Throws<AggregateException>(() => VmMusicWaveExport.WriteWave(
            new VmMusicPlaybackState<object>("old", null), ["writewave", "song", "out"], -1,
            _ => source, _ => { }, (_, _) => throw dumpError,
            (_, _, _, _) => { restored = true; if (restoreFails) throw restoreError; return true; },
            () => "error", _ => { }));
        Assert.Equal(restoreFails ? new Exception[] { dumpError, restoreError, cleanupError }
            : new Exception[] { dumpError, cleanupError }, error.Flatten().InnerExceptions.OrderBy(
                failure => ReferenceEquals(failure, dumpError) ? 0 : ReferenceEquals(failure, restoreError) ? 1 : 2));
        Assert.True(restored);
        Assert.Equal(1, source.Disposals);
    }

    [Fact]
    public void SourceCleanupOnlyFailureRemainsUnwrappedAfterRestoration()
    {
        var cleanupError = new IOException("source cleanup failed");
        var source = new Source { DisposeError = cleanupError };
        var restored = false;
        Assert.Same(cleanupError, Assert.Throws<IOException>(() => VmMusicWaveExport.WriteWave(
            new VmMusicPlaybackState<object>("old", null), ["writewave", "song", "out"], -1,
            _ => source, _ => { }, (_, _) => true,
            (_, _, _, _) => { restored = true; return true; }, () => "error", _ => { })));
        Assert.True(restored);
        Assert.Equal(1, source.Disposals);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void StopFailureAttemptsRestorationAndPreservesBothErrorsWhenRestorationFails(bool restoreFails)
    {
        var source = new Source();
        var stopError = new InvalidOperationException("stop failed");
        var restoreError = new IOException("restore failed");
        var restored = false;
        var error = Record.Exception(() => VmMusicWaveExport.WriteWave(
            new VmMusicPlaybackState<object>("old", null, 2, true), ["writewave", "song", "out"], -1,
            _ => source, _ => throw stopError, (_, _) => throw new Exception("unexpected dump"),
            (name, order, loop, force) =>
            {
                Assert.Equal("old", name); Assert.Equal(2, order); Assert.True(loop); Assert.True(force);
                restored = true;
                if (restoreFails) throw restoreError;
                return true;
            }, () => "error", _ => { }));
        Assert.True(restored);
        Assert.True(source.Disposed);
        if (restoreFails)
            Assert.Equal(new Exception[] { stopError, restoreError }, Assert.IsType<AggregateException>(error).InnerExceptions);
        else Assert.Same(stopError, error);
    }

    [Fact]
    public void DumpAndRestorationFailuresAreBothSurfacedAndSourceIsDisposed()
    {
        var source = new Source();
        var dumpError = new IOException("dump failed");
        var restoreError = new InvalidOperationException("restore failed");
        var error = Assert.Throws<AggregateException>(() => VmMusicWaveExport.WriteWave(
            new VmMusicPlaybackState<object>("old", null), ["writewave", "song", "out"], -1,
            _ => source, _ => { }, (_, _) => throw dumpError,
            (_, _, _, _) => throw restoreError, () => "error", _ => { }));
        Assert.Equal(new Exception[] { dumpError, restoreError }, error.InnerExceptions);
        Assert.True(source.Disposed);
    }

    [Fact]
    public void SuccessfulDumpPropagatesRestorationFailureUnwrapped()
    {
        var source = new Source();
        var restoreError = new IOException("restore failed");
        Assert.Same(restoreError, Assert.Throws<IOException>(() => VmMusicWaveExport.WriteWave(
            new VmMusicPlaybackState<object>("old", null), ["writewave", "song", "out"], -1,
            _ => source, _ => { }, (_, _) => true,
            (_, _, _, _) => throw restoreError, () => "error", _ => { })));
        Assert.True(source.Disposed);
    }

    private sealed class Source : IDisposable
    {
        public bool Disposed { get; private set; }
        public int Disposals { get; private set; }
        public Exception? DisposeError { get; init; }
        public void Dispose()
        {
            Disposed = true;
            Disposals++;
            if (DisposeError is { } error) throw error;
        }
    }

    [Theory]
    [InlineData("WildMidi", 6)]
    [InlineData("GUS", 5)]
    [InlineData("Timidity", 3)]
    [InlineData("Timidity++", 3)]
    [InlineData("FluidSynth", 4)]
    [InlineData("OPL", 1)]
    [InlineData("OPN", 8)]
    [InlineData("ADL", 7)]
    public void SynthOnlyArgumentHasNoSoundfontAndRestoresSavedPlayback(string synth, int expected)
    {
        var state = new VmMusicPlaybackState<object>("old", null, 2, true);
        var source = new Source();
        var calls = new List<string>();
        VmMusicWaveExport.WriteWave(state, ["writewave", "*", "out.wav", "3tail", "44100", synth], 0,
            _ => source, force => { Assert.True(force); calls.Add("stop"); state.Stop(true, 0, () => { }, () => { }, _ => { }, _ => { }); },
            (_, request) => { Assert.Equal(new(expected, null, "out.wav", 3, 44100), request); calls.Add("dump"); return true; },
            (name, order, loop, force) => { Assert.Equal("old", name); Assert.Equal(2, order); Assert.True(loop); Assert.True(force); calls.Add("restore"); return false; },
            () => throw new InvalidOperationException(), _ => throw new InvalidOperationException());
        Assert.Equal(new[] { "stop", "dump", "restore" }, calls);
        Assert.True(source.Disposed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DefaultDeviceAndLiteModeUseFluidFallback(bool lite)
    {
        var args = lite ? new[] { "writewave", "song", "out", "0", "0", "ignored", "bank" }
            : new[] { "writewave", "song", "out" };
        VmMusicWaveExport.WriteWave(new VmMusicPlaybackState<object>("", null), args, 0,
            _ => new Source(), _ => { }, (_, request) =>
            { Assert.Equal(4, request.Device); Assert.Equal(lite ? "bank" : null, request.Soundfont); return true; },
            (_, _, _, _) => true, () => "error", _ => { }, lite);
    }

    [Fact]
    public void UnknownSynthDisposesSourceWithoutStopping()
    {
        var source = new Source();
        var reports = new List<string>();
        VmMusicWaveExport.WriteWave(new VmMusicPlaybackState<object>("old", null),
            ["writewave", "song", "out", "0", "0", "unknown"], -1, _ => source,
            _ => throw new InvalidOperationException(), (_, _) => throw new InvalidOperationException(),
            (_, _, _, _) => throw new InvalidOperationException(), () => "error", reports.Add);
        Assert.Equal(new[] { "unknown: Unknown MIDI device\n" }, reports);
        Assert.True(source.Disposed);
    }

    [Fact]
    public void ThrowingDumperStillRestoresAndDisposes()
    {
        var source = new Source();
        var restored = false;
        Assert.Throws<InvalidOperationException>(() => VmMusicWaveExport.WriteWave(
            new VmMusicPlaybackState<object>("old", null), ["writewave", "song", "out"], -1,
            _ => source, _ => { }, (_, _) => throw new InvalidOperationException("dump failed"),
            (_, _, _, _) => { restored = true; return true; }, () => "error", _ => { }));
        Assert.True(restored);
        Assert.True(source.Disposed);
    }
}
