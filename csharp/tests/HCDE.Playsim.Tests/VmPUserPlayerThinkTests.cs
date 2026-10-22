namespace HCDE.Playsim.Tests;

public class VmPUserPlayerThinkTests
{
    [Fact]
    public void PlayerThink_SkipsBobWhenPredicting() =>
        Assert.False(VmPUserPlayerThink.SkipBobTimerWhenPredicting(VmPUserPlayerThink.CfPredicting, VmPUserPlayerThink.CfPredicting));

    [Fact]
    public void PlayerThink_BotFrozenReturnsEarly() =>
        Assert.True(VmPUserPlayerThink.BotFrozenSkip(true, true));

    [Fact]
    public void PlayerThink_RecordsVelocityOffline() =>
        Assert.True(VmPUserPlayerThink.RecordVelocityWhenNotPredicting(false, true));

    [Fact]
    public void PlayerThink_SubtitleDecrements() =>
        Assert.Equal(2, VmPUserPlayerThink.DecrementSubtitle(3));

    [Fact]
    public void PlayerThink_AppliesAngleOffset() =>
        Assert.Equal(45, VmPUserPlayerThink.ApplyAngleOffsetValue(
            new VmPUserPlayerThink.AngleOffsetApply(5, 40)));

    [Fact]
    public void CheckUse_SkipsDuringPrediction() =>
        Assert.True(VmPUserPlayerThink.CheckUseSkipsWhenPredicting(VmPUserPlayerThink.CfPredicting));

    [Fact]
    public void Environment_TriggersFallingBand() =>
        Assert.True(VmPUserPlayerThink.EnvironmentFallingScream(
            new VmPUserPlayerThink.EnvironmentSoundContext(-30, 20, 40, false, false)));

    [Fact]
    public void MusicInfo_UsesLookupWhenArgNonZero() =>
        Assert.True(VmPUserPlayerThink.MusicInfoUsesLookup(
            new VmPUserPlayerThink.MusicInfoContext(0, true, true, 3)));
}
