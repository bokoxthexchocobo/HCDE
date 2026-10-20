namespace HCDE.Playsim.Tests;

public class VmPMobjSpawnMissileVariantsTests
{
    [Fact]
    public void XyzTail_FailsWhenCheckSpawnBlocks() =>
        Assert.True(VmPMobjSpawnMissileVariants.FailCheckMissileSpawn(true, false));

    [Fact]
    public void OldSpawn_RejectsNullType() =>
        Assert.True(VmPMobjSpawnMissileVariants.OldSpawn_NullArgs(false, true));

    [Fact]
    public void Aimed_ComputesVerticalSpeed() =>
        Assert.Equal(5, VmPMobjSpawnMissileVariants.AimedVerticalSpeed(
            new VmPMobjSpawnMissileVariants.AimedMissileContext(10, 5, 20, 10)));

    [Fact]
    public void SubMissile_InheritsSpectralFromMissileSource() =>
        Assert.True(VmPMobjSpawnMissileVariants.SubMissileSpectralInherit(
            VmPMobjSpawnMissileVariants.Mf4Spectral,
            VmPMobjSpawnMissileVariants.MfMissile,
            VmPMobjSpawnMissileVariants.Mf4Spectral));

    [Fact]
    public void AngleSpawn_AdjustsCustomZ() =>
        Assert.True(VmPMobjSpawnMissileVariants.AdjustAngleSpawnZ(10));

    [Fact]
    public void Tail_AbortsBlockedSpawn() =>
        Assert.True(VmPMobjSpawnMissileVariants.AbortWhenSpawnBlocked(
            new VmPMobjSpawnMissileVariants.MissileSpawnTailContext(true, false, 0, false)));

    [Fact]
    public void OldSpawnVelZ_FromContext() =>
        Assert.Equal(2, VmPMobjSpawnMissileVariants.ComputeOldSpawnVelZ(
            new VmPMobjSpawnMissileVariants.OldSpawnVelocityContext(20, 10, 5)));
}
