namespace HCDE.Client.Tests;

public class VmMusicEnableStateTests
{
    [Theory]
    [InlineData(true, false, 1)]
    [InlineData(false, false, 0)]
    [InlineData(true, true, 0)]
    [InlineData(false, true, 0)]
    public void NativeGlobalEnablementUsesBothGates(bool runtime, bool noMusic, int expected)
    {
        var state = new VmMusicEnableState { RuntimeEnabled = runtime, NoMusic = noMusic };
        Assert.Equal(expected != 0, state.Enabled);
        Assert.Equal(expected, state.ScriptValue);
    }

    [Theory]
    [InlineData("--no-music")]
    [InlineData("-nomusic")]
    public void LauncherParsesRoundTripsAndForwardsNoMusicToHost(string option)
    {
        var path = Path.Combine(Path.GetTempPath(), "hcde-nomusic-" + Guid.NewGuid().ToString("N") + ".wad");
        File.WriteAllBytes(path, [80, 87, 65, 68, 0, 0, 0, 0, 12, 0, 0, 0]);
        try
        {
            Assert.True(LauncherPlan.TryParse(["--iwad", path, option], out var selection, out _));
            Assert.True(selection.NoMusic);
            Assert.True(LauncherPlan.TryParse(LauncherPlan.BuildArgs(selection), out var copy, out _));
            Assert.True(copy.NoMusic);
            using var demo = ClientHost.DemoRoom();
            using var host = LauncherPlan.CreateHost(copy, demo.Simulation);
            Assert.False(host.MusicEnablement.Enabled);
            host.MusicEnablement.RuntimeEnabled = true;
            Assert.False(host.MusicEnablement.Enabled);
            host.MusicEnablement.NoMusic = false;
            Assert.True(host.MusicEnablement.Enabled);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void HostEnablementFeedsLiveConsoleGuard()
    {
        using var host = ClientHost.DemoRoom();
        var changes = 0;
        var console = new VmMusicConsole<object>(new("", null), new([]), () => host.MusicEnablement.Enabled,
            (_, _) => { changes++; return true; }, _ => { }, _ => { }, _ => { });
        console.TryExecute(["changemus", "song"]);
        host.MusicEnablement.NoMusic = true;
        console.TryExecute(["changemus", "song"]);
        host.MusicEnablement.NoMusic = false;
        host.MusicEnablement.RuntimeEnabled = false;
        console.TryExecute(["changemus", "song"]);
        Assert.Equal(1, changes);
    }
}
