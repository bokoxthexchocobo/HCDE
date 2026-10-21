namespace HCDE.Playsim.Tests;

public class VmActionCheckLofTests
{
    [Fact]
    public void ScaleOffsetHeight_PlayerCrouch() =>
        Assert.Equal(40, VmActionCheckLof.ScaleOffsetHeight(VmActionCheckLof.CloffMulHeight, true, 2, 40, 0.5, 56));

    [Fact]
    public void AddHitscanOriginZ_UsesHeightWhenNotFromBase() =>
        Assert.Equal(30, VmActionCheckLof.AddHitscanOriginZ(10, 0, 40, 0));

    [Fact]
    public void TargetBeyondRange_WhenTooFar() =>
        Assert.True(VmActionCheckLof.TargetBeyondRange(true, 100, 0, 200));

    [Fact]
    public void LofTraceUnitVector_MatchesComponents()
    {
        var (vx, vy, vz) = VmActionCheckLof.LofTraceUnitVector(1, 0.6, 0.8, 0.5);
        Assert.Equal(0.6, vx);
        Assert.Equal(0.8, vy);
        Assert.Equal(-0.5, vz);
    }

    [Fact]
    public void DefaultLofRange_UsesPlayerRange() =>
        Assert.Equal(VmActionCheckLof.PlayerMissileRange, VmActionCheckLof.DefaultLofRange(0, true));

    [Fact]
    public void LofHitAccepted_JumpOnMissWhenNoHit() =>
        Assert.True(VmActionCheckLof.LofHitAccepted(1, 0, VmActionCheckLof.CloffJumpOnMiss, false, 2));

    [Fact]
    public void RejectBelowMinRange_WhenTooClose() =>
        Assert.True(VmActionCheckLof.RejectBelowMinRange(64, 32));

    [Fact]
    public void SelectLofPointerOp_PrefersTarget() =>
        Assert.Equal(1, VmActionCheckLof.SelectLofPointerOp(VmActionCheckLof.CloffSetTarget));
}
