namespace HCDE.Playsim.Tests;

public class VmPMapPitCheckLineTests
{
    [Fact]
    public void SkipWhenBoxMissesLine() =>
        Assert.True(VmPMapPitCheckLine.SkipWhenBoxMissesLine(false, -1));

    [Fact]
    public void CanStepCeilingPortal_WhenBelowPortalTop() =>
        Assert.True(VmPMapPitCheckLine.CanStepCeilingPortal(false, false, 10, 24, 32, 8));

    [Fact]
    public void TreatAsProjectile_MissileFlag() =>
        Assert.True(VmPMapPitCheckLine.TreatAsProjectile(VmPMapPitCheckLine.MfMissile, 0));

    [Fact]
    public void SteepSlopeDropoffBlocked_OnBoundary() =>
        Assert.True(VmPMapPitCheckLine.SteepSlopeDropoffBlocked(0, 0.1, 0.9, 0.5));

    [Fact]
    public void RailingFloorBump_Adds32() =>
        Assert.Equal(64, VmPMapPitCheckLine.RailingFloorBump(32));

    [Fact]
    public void LowerCeilingToPortal_TakesMinimum() =>
        Assert.Equal(16, VmPMapPitCheckLine.LowerCeilingToPortal(16, 32));

    [Fact]
    public void BlastedDamageAmount_ShiftsMass() =>
        Assert.Equal(4, VmPMapPitCheckLine.BlastedDamageAmount(128));

    [Fact]
    public void CrossLineCheckFails_WhenCannotCross() =>
        Assert.True(VmPMapPitCheckLine.CrossLineCheckFails(VmPMapPitCheckLine.Mf8Crosslinecheck, false));
}
