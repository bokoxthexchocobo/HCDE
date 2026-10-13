namespace HCDE.Client.Tests;

public class VmMusicVolumeCallbackTests
{
    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void GeneralCallbackDoesNotSendRelativeSettingAndUsesHandlePresence(bool hasSong, bool hasOutput)
    {
        var volume = new VmMusicVolume();
        volume.SetMusicLevel(.5f);
        volume.SetRelativeLevel(.25f);
        var calls = new List<string>();
        volume.NotifyMusicVolume(value => { Assert.Equal(.5f, value); calls.Add("music"); },
            hasOutput ? value => { Assert.Equal(.125f, value); calls.Add("digital"); } : null,
            hasSong, () => calls.Add("changed"), () => calls.Add("restart"));
        Assert.Equal(hasOutput ? new[] { "music", "digital", hasSong ? "changed" : "restart" }
            : new[] { "music", hasSong ? "changed" : "restart" }, calls);
    }

    [Theory]
    [InlineData(-3, 0)]
    [InlineData(3, 2)]
    public void GameVolumeClampsBeforeRelativeSettingAndGeneralCallback(double input, float expected)
    {
        var volume = new VmMusicVolume();
        var calls = new List<string>();
        volume.ChangeGameVolume(input, value =>
        { Assert.Equal(expected, value); Assert.Equal(expected, volume.RelativeLevel); calls.Add("relative"); },
            () => calls.Add("callback"));
        Assert.Equal(new[] { "relative", "callback" }, calls);
    }

    [Fact]
    public void RelativeVolumeDoesNotClampAndBackendFailureSkipsGeneralCallback()
    {
        var volume = new VmMusicVolume();
        Assert.Throws<InvalidOperationException>(() => volume.ChangeRelativeVolume(-2,
            value => { Assert.Equal(-2, value); throw new InvalidOperationException("setting failed"); },
            () => throw new Exception("unexpected callback")));
        Assert.Equal(-2, volume.RelativeLevel);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RuntimeEnableCallbackCommitsStateThenRestartsOrForceStops(bool enabled)
    {
        var state = new VmMusicEnableState { NoMusic = true };
        var calls = new List<string>();
        state.ApplyRuntime(enabled,
            () => { Assert.True(state.RuntimeEnabled); calls.Add("restart"); },
            force => { Assert.True(force); Assert.False(state.RuntimeEnabled); calls.Add("stop"); });
        Assert.False(state.Enabled);
        Assert.Equal(new[] { enabled ? "restart" : "stop" }, calls);
    }
}
