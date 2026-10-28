namespace HCDE.Playsim.Tests;

public class VmPLnspecCrusherPlatExitTeleSliceTests
{
    [Fact]
    public void GenericCrusher_SlowSpeedUsesSlowdownMode()
    {
        VmPLnspecCrusherPlatExitTeleSlice.CrushMode? mode = null;
        VmPLnspecCrusherPlatExitTeleSlice.GenericCrusher(1, 16, 16, 0, 10, new VmPLnspecCrusherPlatExitTeleSlice.LineContext
        {
            EvGenericCrusher = c => { mode = c.Mode; return true; },
        });
        Assert.Equal(VmPLnspecCrusherPlatExitTeleSlice.CrushMode.Slowdown, mode);
    }

    [Fact]
    public void PlatStop_HexenDefaultRemoves()
    {
        Assert.True(VmPLnspecCrusherPlatExitTeleSlice.PlatStop(1, 0, new VmPLnspecCrusherPlatExitTeleSlice.LineContext
        {
            GameType = VmPLnspecCrusherPlatExitTeleSlice.GameHexen,
            EvStopPlat = (_, remove) => remove,
        }));
    }

    [Fact]
    public void GenericLift_TargetTwoUsesDownToNearest()
    {
        var kind = (VmPLnspecCrusherPlatExitTeleSlice.PlatKind)(-1);
        VmPLnspecCrusherPlatExitTeleSlice.GenericLift(1, 8, 0, 2, 4, new VmPLnspecCrusherPlatExitTeleSlice.LineContext
        {
            EvDoPlat = p => { kind = p.Kind; return true; },
        });
        Assert.Equal(VmPLnspecCrusherPlatExitTeleSlice.PlatKind.DownToNearestFloor, kind);
    }

    [Fact]
    public void TeleportNewMap_BlocksBacksideUnlessStrife()
    {
        Assert.False(VmPLnspecCrusherPlatExitTeleSlice.TeleportNewMap(1, 0, 0, true, new VmPLnspecCrusherPlatExitTeleSlice.LineContext
        {
            FindLevelByNum = _ => true,
            CheckExitGood = () => true,
            EvChangeLevel = (_, _) => true,
        }));
    }

    [Fact]
    public void TeleportNoFog_UseAngTwoAddsRotateBoom()
    {
        var flags = 0;
        VmPLnspecCrusherPlatExitTeleSlice.TeleportNoFog(0, 2, 1, false, true, new VmPLnspecCrusherPlatExitTeleSlice.LineContext
        {
            EvTeleport = (_, _, f) => { flags = f; return true; },
        });
        Assert.Equal(
            VmPLnspecCrusherPlatExitTeleSlice.TelfKeepOrientation | VmPLnspecCrusherPlatExitTeleSlice.TelfRotateBoom,
            flags);
    }

    [Fact]
    public void ExitNormal_RequiresGoodExit()
    {
        Assert.False(VmPLnspecCrusherPlatExitTeleSlice.ExitNormal(0, new VmPLnspecCrusherPlatExitTeleSlice.LineContext
        {
            CheckExitGood = () => false,
            EvExitLevel = (_, _) => true,
        }));
    }
}
