namespace HCDE.Client.Tests;

public class VmMusicExtraCommandsTests
{
    [Fact]
    public void PlaylistUsageAndLoadRouteWithoutCheckingMusicEnablement()
    {
        var playlist = new VmMusicPlaylist([]);
        var messages = new List<string>();
        var activations = new List<bool>();
        var console = new VmMusicConsole<object>(new("", null), playlist,
            () => throw new InvalidOperationException(), (_, _) => false, _ => { }, activations.Add, messages.Add,
            _ => System.Text.Encoding.ASCII.GetBytes("one\ntwo\n"));
        Assert.True(console.TryExecute(["playlist"]));
        Assert.True(console.TryExecute(["playlist", "list.m3u", "1"]));
        Assert.Equal(1, playlist.Position);
        Assert.Equal(new[] { false }, activations);
        Assert.Equal(new[] { "playlist <playlist.m3u> [<position>|shuffle]\n" }, messages);
    }

    [Theory]
    [InlineData("20tail", 10)]
    [InlineData(" -3.5dB", -3.5f)]
    [InlineData("invalid", 0)]
    public void ReplayGainDispatchStoresSavesThenAppliesParsedGain(string argument, float expected)
    {
        var calls = new List<string>();
        var console = new VmMusicConsole<object>(new("song", new()), new([]), () => false,
            (_, _) => false, _ => { }, _ => { }, _ => { }, setReplayGain: args =>
                VmReplayGainCommand.Execute(true, "hash", 1, args,
                    (hash, gain) => { Assert.Equal("hash", hash); Assert.Equal(expected, gain); calls.Add("store"); },
                    () => calls.Add("save"), volume =>
                    { Assert.Equal(VmReplayGain.DecibelsToAmplitude(expected), volume); calls.Add("volume"); },
                    _ => throw new InvalidOperationException()));
        Assert.True(console.TryExecute(["setreplaygain", argument]));
        Assert.Equal(new[] { "store", "save", "volume" }, calls);
    }

    [Fact]
    public void ReplayGainRequiresSongAndHashBeforeShowingUsage()
    {
        var messages = new List<string>();
        void Execute(bool hasSong, string hash) => VmReplayGainCommand.Execute(hasSong, hash, 1,
            ["setreplaygain"], (_, _) => throw new InvalidOperationException(),
            () => throw new InvalidOperationException(), _ => throw new InvalidOperationException(), messages.Add);
        Execute(false, "hash");
        Execute(true, "");
        Execute(true, "hash");
        Assert.Equal(new[] { "setreplaygain needs some music playing\n", "setreplaygain needs some music playing\n",
            "Usage: setreplaygain {dB}\n", "Current replay gain is 0.000000 dB\n" }, messages);
    }
}
