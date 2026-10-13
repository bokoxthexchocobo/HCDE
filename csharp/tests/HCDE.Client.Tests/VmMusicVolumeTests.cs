namespace HCDE.Client.Tests;

public class VmMusicVolumeTests
{
    [Fact]
    public void SongChangesPreserveRelativeModifierAcrossSavedSongLevels()
    {
        var volume = new VmMusicVolume();
        Assert.True(volume.StartSongLevel(.5f));
        Assert.Equal(.5f, volume.RelativeLevel);
        volume.SetGameLevel(.25);
        Assert.True(volume.StartSongLevel(.8f));
        Assert.Equal(.8f, volume.SavedSongLevel);
        Assert.Equal(.4f, volume.RelativeLevel);
        Assert.True(volume.StartSongLevel(.2f));
        Assert.Equal(.1f, volume.RelativeLevel);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(.5f, true)]
    public void InvalidOrReplayGainSongLevelsLeaveSavedStateUnchanged(float level, bool replayGain)
    {
        var volume = new VmMusicVolume();
        volume.SetRelativeLevel(.25f);
        Assert.False(volume.StartSongLevel(level, replayGain));
        Assert.Equal(1, volume.SavedSongLevel);
        Assert.Equal(.25f, volume.RelativeLevel);
    }

    [Fact]
    public void LevelsClampAndHostUsesAcsVolumeFactor()
    {
        using var host = ClientHost.DemoRoom();
        host.MusicVolume.SetMusicLevel(.5f);
        host.MusicVolume.SetMasterLevel(.5f);
        host.Simulation.MusicVolume = 2;
        Assert.Equal(.5f, host.EffectiveMusicVolume);
        host.Simulation.MusicVolume = 10;
        Assert.Equal(.5f, host.EffectiveMusicVolume);
        host.Simulation.MusicVolume = -1;
        Assert.Equal(0, host.EffectiveMusicVolume);
        host.MusicVolume.SetMusicLevel(2);
        host.MusicVolume.SetMasterLevel(-1);
        Assert.Equal(1, host.MusicVolume.MusicLevel);
        Assert.Equal(0, host.MusicVolume.MasterLevel);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RelativeVolumeAppliesSettingsAndSelectsPlayingOrRestartNotification(bool playing)
    {
        var volume = new VmMusicVolume();
        volume.SetGameLevel(3);
        Assert.Equal(2, volume.RelativeLevel);
        volume.SetMusicLevel(.25f);
        var order = new List<string>();
        volume.Apply(value => { Assert.Equal(.25f, value); order.Add("music"); },
            value => { Assert.Equal(2, value); order.Add("relative"); },
            value => { Assert.Equal(.5f, value); order.Add("digital"); }, playing,
            () => order.Add("changed"), () => order.Add("restart"));
        Assert.Equal(new[] { "relative", "music", "digital", playing ? "changed" : "restart" }, order);
    }
}
