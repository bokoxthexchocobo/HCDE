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
}
