namespace HCDE.Playsim.Tests;

public class VmSoundEngineLimitStopTests
{
    [Fact]
    public void WithinLimitDistance_ZeroAttnAlwaysInside() =>
        Assert.True(VmSoundEngineLimitStop.WithinLimitDistance(9999, 100, 0, 1));

    [Fact]
    public void ShouldStopUnpositioned_MatchesAutoChannel() =>
        Assert.True(VmSoundEngineLimitStop.ShouldStopUnpositioned(0, -1, 3, -1, 3));

    [Fact]
    public void CheckSoundLimitNative_HitsCeiling() =>
        Assert.True(VmSoundEngineLimitStop.CheckSoundLimitNative(2, 2, limitReachedForNew: true));

    [Fact]
    public void IsRestartingSameSource_AllowsReplay()
    {
        var actor = new object();
        Assert.True(VmSoundEngineLimitStop.IsRestartingSameSource(
            actor, channel: 1, chanEntChannel: 1, chanSourceType: 2, chanSource: actor, sourcetype: 2));
    }

    [Fact]
    public void ShouldStopActorSound_ByOrgId()
    {
        var actor = new object();
        Assert.True(VmSoundEngineLimitStop.ShouldStopActorSound(
            chanSourceType: 1, sourcetype: 1, chanSource: actor, actor: actor,
            soundId: 7, chanOrgId: 7, channel: 0, chanEntChannel: 0));
    }

    [Fact]
    public void ShouldStopActorChannel_RangeOrAll()
    {
        var actor = new object();
        Assert.True(VmSoundEngineLimitStop.ShouldStopActorChannel(
            1, 1, actor, actor, allChannels: false, chanEntChannel: 3, chanMin: 2, chanMax: 4));
    }

    [Fact]
    public void ShouldRelinkSource_MatchesFromPointer()
    {
        var from = new object();
        Assert.True(VmSoundEngineLimitStop.ShouldRelinkSource(1, 1, from, from));
    }
}
