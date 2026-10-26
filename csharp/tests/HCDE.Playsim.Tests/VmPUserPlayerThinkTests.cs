namespace HCDE.Playsim.Tests;

public class VmPUserPlayerThinkTests
{
    [Fact]
    public void PlayerThink_SkipsBobWhenPredicting() =>
        Assert.False(VmPUserPlayerThink.SkipBobTimerWhenPredicting(
            VmPUserPlayerThink.CfPredicting, VmPUserPlayerThink.CfPredicting));

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
    public void CheckUse_FiresOnEdge() =>
        Assert.True(VmPUserPlayerThink.CheckUseEdge(VmPUserPlayerThink.BtUse, VmPUserPlayerThink.BtUse, usedown: false));

    [Fact]
    public void Environment_TriggersFallingBand() =>
        Assert.True(VmPUserPlayerThink.EnvironmentFallingScream(
            new VmPUserPlayerThink.EnvironmentSoundContext(-30, 20, 40, false, false)));

    [Fact]
    public void MusicInfo_UsesLookupWhenArgNonZero() =>
        Assert.True(VmPUserPlayerThink.MusicInfoUsesLookup(
            new VmPUserPlayerThink.MusicInfoContext(0, true, true, 3, 0)));

    [Fact]
    public void MusicInfo_FiresWhenTicsExpire() =>
        Assert.True(VmPUserPlayerThink.MusicInfoFiredAfterDecrement(0));

    [Fact]
    public void SafePos_ValidOnFloor() =>
        Assert.True(VmPUserPlayerThink.SafePosValidOnFloor(
            new VmPUserPlayerThink.SafePosSample(new object(), 0, 0, 10, 10, 0, 24, 0, 41, false, 0, true)));

    [Fact]
    public void Velocity_SelectsXyWhenGravity() =>
        Assert.Equal(5, VmPUserPlayerThink.SelectVelocitySpeed(
            new VmPUserPlayerThink.VelocityRecordGate(false, true, 0, VmPUserPlayerThink.MfNogravity),
            velLength: 9,
            velXyLength: 5));

    [Fact]
    public void StripPresentationCheats_ClearsInterpFlags()
    {
        var cleared = VmPUserPlayerThink.StripPresentationCheats(
            new VmPUserPlayerThink.ThinkCheatsGate(
                VmPUserPlayerThink.CfInterpview | VmPUserPlayerThink.CfNofovinterp,
                VmPUserPlayerThink.CfPredicting,
                VmPUserPlayerThink.CfInterpview));
        Assert.Equal(0, cleared);
    }
}
