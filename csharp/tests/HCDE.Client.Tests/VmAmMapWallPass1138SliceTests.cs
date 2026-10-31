namespace HCDE.Client.Tests;

public class VmAmMapWallPass1138SliceTests
{
    [Fact]
    public void Triggers_HideUnknownSpecialsAndPlainDoorsInModeOne()
    {
        Assert.True(VmAmMapWallPass1138Slice.ExitSecretArm(VmAmMapWallPass1138Slice.ExitSecret));
        Assert.False(VmAmMapWallPass1138Slice.ExitSecretArm(VmAmMapWallPass1138Slice.DoorOpen));
        var known = new VmAmMapWallPass1138Slice.SpecialInfo(true, 3);
        Assert.False(VmAmMapWallPass1138Slice.IsShownTrigger(1, VmAmMapWallPass1138Slice.DoorOpen, known));
        Assert.False(VmAmMapWallPass1138Slice.IsShownTrigger(1, VmAmMapWallPass1138Slice.DoorClose, known));
        Assert.False(VmAmMapWallPass1138Slice.IsShownTrigger(1, VmAmMapWallPass1138Slice.GenericDoor, known));
        Assert.True(VmAmMapWallPass1138Slice.IsShownTrigger(1, 70, known));
        Assert.True(VmAmMapWallPass1138Slice.IsShownTrigger(0, VmAmMapWallPass1138Slice.DoorRaise, known));
        Assert.True(VmAmMapWallPass1138Slice.IsShownTrigger(2, VmAmMapWallPass1138Slice.DoorCloseWaitOpen, known));
        Assert.False(VmAmMapWallPass1138Slice.IsShownTrigger(0, 70, new VmAmMapWallPass1138Slice.SpecialInfo(false, 3)));
        Assert.False(VmAmMapWallPass1138Slice.IsShownTrigger(0, 70, new VmAmMapWallPass1138Slice.SpecialInfo(true, -1)));
    }

    [Fact]
    public void LockBoundary_UsesTheLineLockOrTheSpecialArgument()
    {
        var none = System.Array.Empty<VmAmMapFloorBoundary1137Slice.SectorAction>();
        var numbered = VmAmMapWallPass1138Slice.LockBoundary(7, 0, 0, System.Array.Empty<int>(), false, none, none);
        Assert.True(numbered.Found);
        Assert.Equal(7, numbered.Lock);

        var fs = VmAmMapWallPass1138Slice.LockBoundary(
            0, VmAmMapFloorBoundary1137Slice.PlayerActivate, VmAmMapWallPass1138Slice.FsExecute,
            new[] { 0, 0, 5, 0, 0 }, false, none, none);
        Assert.Equal(5, fs.Lock);

        var raised = VmAmMapWallPass1138Slice.LockBoundary(
            0, VmAmMapFloorBoundary1137Slice.PlayerActivate, VmAmMapWallPass1138Slice.DoorLockedRaise,
            new[] { 0, 0, 0, 9, 1 }, false, none, none);
        Assert.Equal(9, raised.Lock);

        var acs = VmAmMapWallPass1138Slice.LockBoundary(
            0, VmAmMapFloorBoundary1137Slice.PlayerActivate, VmAmMapWallPass1138Slice.AcsLockedExecuteDoor,
            new[] { 0, 0, 0, 0, 4 }, false, none, none);
        Assert.Equal(4, acs.Lock);

        Assert.False(VmAmMapWallPass1138Slice.IsLockSpecial(VmAmMapWallPass1138Slice.DoorAnimated, new[] { 0, 0, 0, 0, 0 }));
        Assert.False(VmAmMapWallPass1138Slice.IsLockSpecial(VmAmMapWallPass1138Slice.GenericDoor, new[] { 0, 0, 0, 0, 0 }));
        Assert.True(VmAmMapWallPass1138Slice.IsLockSpecial(VmAmMapWallPass1138Slice.AcsLockedExecute, new[] { 0, 0, 0, 0, 1 }));
        var generic = VmAmMapWallPass1138Slice.LockBoundary(
            0, VmAmMapFloorBoundary1137Slice.PlayerActivate, VmAmMapWallPass1138Slice.GenericDoor,
            new[] { 0, 0, 0, 0, 3 }, false, none, none);
        Assert.Equal(3, generic.Lock);
    }

    [Fact]
    public void PortalPasses_SkipTheMapGroupAndPlaceTheOffset()
    {
        Assert.Equal(0, VmAmMapWallPass1138Slice.PortalGroupCount(false, 4));
        Assert.Equal(4, VmAmMapWallPass1138Slice.PortalGroupCount(true, 4));
        Assert.Equal(new[] { 2, 0, -1 }, VmAmMapWallPass1138Slice.PortalPasses(3, 1));
        Assert.True(VmAmMapWallPass1138Slice.IsPolyObjectWall(VmAmMapWallPass1138Slice.PolyObjectWall));
        Assert.Equal(8, VmAmMapWallPass1138Slice.LinePortalGroup(true, 8, 1));
        Assert.Equal(1, VmAmMapWallPass1138Slice.LinePortalGroup(false, 8, 1));

        var shifted = VmAmMapWallPass1138Slice.WallOffset(2, 2, 0, true, 3, 10, -4);
        Assert.True(shifted.Draw);
        Assert.Equal(10d, shifted.X);
        Assert.True(shifted.PortalMode);
        var basePass = VmAmMapWallPass1138Slice.WallOffset(-1, 2, 0, true, 3, 10, -4);
        Assert.False(basePass.Draw);
        var local = VmAmMapWallPass1138Slice.WallOffset(-1, 2, 0, false, 0, 10, -4);
        Assert.True(local.Draw);
        Assert.Equal(0d, local.X);
        Assert.False(local.PortalMode);

        var placed = VmAmMapWallPass1138Slice.PlacedWall(1, 2, 3, 4, 10, 20, true, (x, y) => (x + 1, y));
        Assert.Equal((12d, 22d, 14d, 24d), placed);
    }

    [Fact]
    public void ChooseWall_StopsAtStylePortalOrBoomSecret()
    {
        Assert.True(VmAmMapWallPass1138Slice.WallIsMapped(1, 0));
        Assert.True(VmAmMapWallPass1138Slice.WallIsMapped(0, VmAmMapWallPass1138Slice.Mapped));
        Assert.False(VmAmMapWallPass1138Slice.WallIsMapped(0, 0));
        Assert.True(VmAmMapWallPass1138Slice.SkipDontDraw(VmAmMapWallPass1138Slice.DontDraw, 0, false, false));
        Assert.False(VmAmMapWallPass1138Slice.SkipDontDraw(VmAmMapWallPass1138Slice.DontDraw, 2, false, false));
        Assert.False(VmAmMapWallPass1138Slice.SkipDontDraw(VmAmMapWallPass1138Slice.DontDraw, 4, true, false));
        Assert.True(VmAmMapWallPass1138Slice.SkipDontDraw(VmAmMapWallPass1138Slice.DontDraw, 4, true, true));
        Assert.True(VmAmMapWallPass1138Slice.UseAutomapStyle(2, 0));
        Assert.False(VmAmMapWallPass1138Slice.UseAutomapStyle(2, 1));
        Assert.False(VmAmMapWallPass1138Slice.UseAutomapStyle(14, 0));

        Assert.Equal(VmAmMapWallPass1138Slice.WallChoice.Hidden, VmAmMapWallPass1138Slice.ChooseWall(false, false, false, false, 0));
        Assert.Equal(VmAmMapWallPass1138Slice.WallChoice.Style, VmAmMapWallPass1138Slice.ChooseWall(true, false, true, true, 1));
        Assert.Equal(VmAmMapWallPass1138Slice.WallChoice.Portal, VmAmMapWallPass1138Slice.ChooseWall(true, false, false, true, 1));
        Assert.Equal(VmAmMapWallPass1138Slice.WallChoice.SecretSector, VmAmMapWallPass1138Slice.ChooseWall(true, false, false, false, 1));
        Assert.Equal(VmAmMapWallPass1138Slice.WallChoice.Continue, VmAmMapWallPass1138Slice.ChooseWall(true, false, false, false, 2));
    }
}
