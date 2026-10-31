namespace HCDE.Client.Tests;

public class VmAmMapWallColor1139SliceTests
{
    [Fact]
    public void ChooseWall_FollowsTheSecretTeleportAndLockOrder()
    {
        var secret = Choose(secretResult: 1);
        Assert.Equal(VmAmMapWallColor1139Slice.WallColor.SecretSector, secret.Color);
        Assert.Equal(VmAmMapWallColor1139Slice.WallColor.UnexploredSecret, Choose(secretResult: 2).Color);
        Assert.Equal(VmAmMapWallColor1139Slice.WallColor.SecretWall, Choose(lineFlags: VmAmMapWallColor1139Slice.LineSecret, cheat: 2, hasBack: true).Color);
        Assert.Equal(VmAmMapWallColor1139Slice.WallColor.Wall, Choose(lineFlags: VmAmMapWallColor1139Slice.LineSecret, cheat: 0, hasBack: true).Color);
        Assert.Equal(VmAmMapWallColor1139Slice.WallColor.IntraTeleport, Choose(teleportBoundary: true, intraColorValid: true).Color);
        Assert.Equal(VmAmMapWallColor1139Slice.WallColor.InterTeleport, Choose(teleportBoundary: true, intraColorValid: false, exitBoundary: true, interColorValid: true).Color);
        var mapLock = Choose(lockBoundary: true, displayLocks: true, lockMapColor: 0x010203);
        Assert.Equal(VmAmMapWallColor1139Slice.WallColor.LockFromMap, mapLock.Color);
        Assert.Equal((byte)1, mapLock.R);
        Assert.Equal((byte)2, mapLock.G);
        Assert.Equal((byte)3, mapLock.B);
        Assert.Equal(VmAmMapWallColor1139Slice.WallColor.LockFromMap, Choose(lockBoundary: true, displayLocks: true, lockMapColor: 0).Color);
        Assert.Equal(VmAmMapWallColor1139Slice.WallColor.Locked, Choose(lockBoundary: true, displayLocks: true, lockMapColor: -1).Color);
        Assert.Equal(VmAmMapWallColor1139Slice.WallColor.Locked, Choose(lockBoundary: true, displayLocks: false, lockMapColor: 0x010203).Color);
    }

    [Fact]
    public void ChooseWall_UsesTriggerThenPlaneChangesThenTheUnseenColor()
    {
        Assert.Equal(VmAmMapWallColor1139Slice.WallColor.Trigger, Choose(showTriggerLines: 2, specialColorValid: true, triggerBoundary: true).Color);
        Assert.Equal(VmAmMapWallColor1139Slice.WallColor.OneSided, Choose(hasBack: false).Color);
        Assert.Equal(VmAmMapWallColor1139Slice.WallColor.FloorDiff, Choose(hasBack: true, floorsDiffer: true, ceilingsDiffer: true).Color);
        Assert.Equal(VmAmMapWallColor1139Slice.WallColor.CeilingDiff, Choose(hasBack: true, ceilingsDiffer: true, extraFloor: true).Color);
        Assert.Equal(VmAmMapWallColor1139Slice.WallColor.ExtraFloor, Choose(hasBack: true, extraFloor: true, cheat: 2).Color);
        Assert.Equal(VmAmMapWallColor1139Slice.WallColor.TwoSided, Choose(hasBack: true, cheat: 2).Color);
        Assert.Equal(VmAmMapWallColor1139Slice.WallColor.None, Choose(hasBack: true, cheat: 0).Color);
        Assert.Equal(VmAmMapWallColor1139Slice.WallColor.NotSeen, Choose(mapped: false, allMap: true).Color);
        Assert.Equal(
            VmAmMapWallColor1139Slice.WallColor.None,
            Choose(mapped: false, lineFlags: VmAmMapWallColor1139Slice.Revealed | VmAmMapWallColor1139Slice.DontDraw, cheat: 0, showAllEnabled: false).Color);
        Assert.Equal(VmAmMapWallColor1139Slice.WallColor.NotSeen, Choose(mapped: false, lineFlags: VmAmMapWallColor1139Slice.Revealed, cheat: 4, showAllEnabled: true).Color);
        Assert.Equal(VmAmMapWallColor1139Slice.WallColor.None, Choose(mapped: false).Color);
    }

    [Fact]
    public void RotatePoint_SpinsAroundTheMapCenter()
    {
        var kept = VmAmMapWallColor1139Slice.UpdateSpin(false, new VmAmMapWallColor1139Slice.Spin(0.5, 0.5), 1, 0);
        Assert.Equal(0.5, kept.Sin);
        var replaced = VmAmMapWallColor1139Slice.UpdateSpin(true, kept, 1, 0);
        Assert.Equal(0d, replaced.Cos);
        Assert.Equal((-4d, 3d), VmAmMapWallColor1139Slice.ApplySpin(3, 4, new VmAmMapWallColor1139Slice.Spin(1, 0)));
        Assert.Equal((3d, 4d), VmAmMapWallColor1139Slice.ApplySpin(3, 4, new VmAmMapWallColor1139Slice.Spin(0, 1)));
        var turned = VmAmMapWallColor1139Slice.RotatePoint(6, 5, 0, 0, 10, 10, new VmAmMapWallColor1139Slice.Spin(1, 0));
        Assert.Equal((5d, 6d), turned);
        Assert.Equal((6d, 8d), VmAmMapWallColor1139Slice.ScaleGlyphPoint(3, 4, 2));
        Assert.Equal((3d, 4d), VmAmMapWallColor1139Slice.ScaleGlyphPoint(3, 4, 0));
    }

    private static VmAmMapWallColor1139Slice.WallPaint Choose(
        bool mapped = true,
        int secretResult = 0,
        int lineFlags = 0,
        int cheat = 0,
        bool hasBack = true,
        bool teleportBoundary = false,
        bool intraColorValid = false,
        bool exitBoundary = false,
        bool interColorValid = false,
        bool lockBoundary = false,
        bool displayLocks = false,
        int lockMapColor = -1,
        int showTriggerLines = 0,
        bool specialColorValid = false,
        bool triggerBoundary = false,
        bool floorsDiffer = false,
        bool ceilingsDiffer = false,
        bool extraFloor = false,
        bool allMap = false,
        bool showAllEnabled = false,
        bool cheatModeBlocks = false)
    {
        return VmAmMapWallColor1139Slice.ChooseWall(
            mapped, secretResult, lineFlags, cheat, hasBack,
            teleportBoundary, intraColorValid, exitBoundary, interColorValid,
            lockBoundary, displayLocks, lockMapColor,
            showTriggerLines, specialColorValid, triggerBoundary,
            floorsDiffer, ceilingsDiffer, extraFloor,
            allMap, showAllEnabled, cheatModeBlocks);
    }
}
