namespace HCDE.Client.Tests;

public class VmMusicCallbacksTests
{
    [Fact]
    public void CallbackShutdownClosesAllOutstandingHandlesAndToleratesAlreadyClosedHandles()
    {
        var callbacks = new VmMusicCallbacks([], _ => 0, _ => [17]);
        var first = callbacks.OpenBank("first.cfg", VmMusicBankFormat.Gus)!;
        var second = callbacks.OpenBank("second.cfg", VmMusicBankFormat.Gus)!;
        var closed = callbacks.OpenBank("closed.cfg", VmMusicBankFormat.Gus)!;
        closed.Dispose();
        callbacks.Dispose();
        callbacks.Dispose();
        Assert.Throws<ObjectDisposedException>(() => first.OpenFile(null));
        Assert.Throws<ObjectDisposedException>(() => second.AddPath("patches"));
        Assert.Throws<ObjectDisposedException>(() => callbacks.OpenBank(null, VmMusicBankFormat.Gus));
        Assert.Throws<ObjectDisposedException>(() => callbacks.CreateWildMidiCache());
    }

    [Fact]
    public void FmPathCallbackSelectsCompatibleNameOrFallbackFromSnapshot()
    {
        VmMusicBank[] banks = [new("opl", "opl.wopl", "opl-path", VmMusicBankFormat.Wopl),
            new("opn", "opn.wopn", "opn-path", VmMusicBankFormat.Wopn)];
        var callbacks = new VmMusicCallbacks(banks);
        banks[0] = banks[1];
        Assert.Equal("opl-path", callbacks.PathToBank("OPL.WOPL", VmMusicBankFormat.Wopl));
        Assert.Equal("opn-path", callbacks.PathToBank("missing", VmMusicBankFormat.Wopn));
        Assert.Null(callbacks.PathToBank(null, VmMusicBankFormat.Sf2));
        Assert.Equal(AppContext.BaseDirectory, callbacks.ExpandPath("$PROGDIR"));
    }

    [Fact]
    public void HandleCallbacksOpenConfigAndPatchPathsAndEnforceClosedState()
    {
        var callbacks = new VmMusicCallbacks([], name => name switch
            { "bank.cfg" => 0, "patches/piano.pat" => 1, _ => -1 }, index => [(byte)index]);
        using var handle = callbacks.OpenBank("bank.cfg", VmMusicBankFormat.None)!;
        Assert.Equal(new byte[] { 0 }, handle.OpenFile(null).Data);
        var bankReader = Assert.IsAssignableFrom<IVmMusicBankReader>(handle);
        Assert.Equal(new byte[] { 0 }, bankReader.Open(null).Data);
        handle.AddPath("patches");
        Assert.Equal(new byte[] { 1 }, handle.OpenFile("piano.pat").Data);
        handle.Dispose();
        handle.Dispose();
        Assert.Throws<ObjectDisposedException>(() => handle.OpenFile(null));
        Assert.Throws<ObjectDisposedException>(() => handle.AddPath("other"));
        Assert.Throws<ObjectDisposedException>(() => bankReader.Open(null));
        Assert.Throws<ObjectDisposedException>(() => bankReader.Paths);
        Assert.Null(callbacks.OpenBank(null, VmMusicBankFormat.Gus));
    }
}
