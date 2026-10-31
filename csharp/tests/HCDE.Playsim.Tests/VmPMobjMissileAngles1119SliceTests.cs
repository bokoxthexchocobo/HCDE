namespace HCDE.Playsim.Tests;

public class VmPMobjMissileAngles1119SliceTests
{
    [Fact]
    public void AimedVerticalSpeed_UsesTravelTimeWhenDistancePositive()
    {
        Assert.Equal(2, VmPMobjMissileAngles1119Slice.AimedVerticalSpeed(40, 10, 28, 20));
        Assert.Equal(8, VmPMobjMissileAngles1119Slice.AimedVerticalSpeed(0, 8, 10, 0));
    }

    [Fact]
    public void RunOldSpawnMissile_PlacesThirtyTwoAboveSource()
    {
        var ctx = new VmPMobjMissileAngles1119Slice.AngleSpeedContext
        {
            HasSource = true,
            HasType = true,
            Flags4 = VmPMobjMissileAngles1119Slice.Mf4Spectral
        };
        Assert.True(VmPMobjMissileAngles1119Slice.RunOldSpawnMissile(ctx, 80, 10, 20));
        Assert.Equal(42, ctx.Z);
        Assert.Equal(3.5, ctx.VerticalSpeed);
        Assert.True(ctx.SpectralFriend);
    }

    [Fact]
    public void RunSpawnMissileAngleZSpeed_AppliesFloorclipAndFailsCheck()
    {
        var ctx = new VmPMobjMissileAngles1119Slice.AngleSpeedContext
        {
            HasSource = true,
            HasType = true,
            Z = 40,
            FloorClip = 8,
            AimAngle = 90,
            IncomingVz = 4,
            CheckSpawn = true,
            SpawnCheckPassed = false
        };
        Assert.False(VmPMobjMissileAngles1119Slice.RunSpawnMissileAngleZSpeed(ctx));
        Assert.Equal(32, ctx.Z);
        Assert.Equal(90, ctx.Yaw);
        Assert.True(ctx.Aborted);
    }

    [Fact]
    public void ApplySubMissileFriend_CopiesSpectralMissileOwner()
    {
        var ctx = new VmPMobjMissileAngles1119Slice.SubMissileContext
        {
            MissileSpectral = true,
            SourceIsMissile = true,
            SourceSpectral = true,
            SourceFriendPlayer = 3,
            TargetFriendPlayer = 1
        };
        VmPMobjMissileAngles1119Slice.ApplySubMissileFriend(ctx);
        Assert.Equal(3, ctx.FriendPlayer);
        Assert.True(ctx.CopiedSourceFriend);
    }
}
