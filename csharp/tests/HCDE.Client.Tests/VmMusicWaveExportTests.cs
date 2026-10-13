namespace HCDE.Client.Tests;

public class VmMusicWaveExportTests
{
    private sealed class Source : IDisposable
    {
        public bool Disposed { get; private set; }
        public void Dispose() => Disposed = true;
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
