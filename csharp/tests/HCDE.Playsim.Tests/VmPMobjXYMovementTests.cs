namespace HCDE.Playsim.Tests;

public class VmPMobjXYMovementTests
{
    [Fact]
    public void XYMovement_ZeroMove_ClearsSkullfly()
    {
        var mo = new VmPMobjXYMovement.XYActor
        {
            Flags = VmPMobjXYMovement.MfSkullfly,
            VelX = 0,
            VelY = 0,
        };
        var idle = false;
        var st = new VmPMobjXYMovement.XYMovementState
        {
            Actor = mo,
            SetIdle = _ => idle = true,
        };
        VmPMobjXYMovement.XYMovement(st);
        Assert.Equal(0, mo.Flags & VmPMobjXYMovement.MfSkullfly);
        Assert.True(idle);
    }

    [Fact]
    public void XYMovement_MissileBlocked_Explodes()
    {
        var exploded = false;
        var mo = new VmPMobjXYMovement.XYActor
        {
            Flags = VmPMobjXYMovement.MfMissile,
            Radius = 16,
            VelX = 10,
            VelY = 0,
            X = 0,
            Y = 0,
        };
        var st = new VmPMobjXYMovement.XYMovementState
        {
            Actor = mo,
            TryMove = (_, _, _, _, _) => false,
            ExplodeMissile = (_, _, _, _) => exploded = true,
        };
        VmPMobjXYMovement.XYMovement(st);
        Assert.True(exploded);
    }

    [Fact]
    public void XYMovement_ScrollAboveCarryStop_AddsScrollMoveFlag()
    {
        var mo = new VmPMobjXYMovement.XYActor
        {
            Radius = 16,
            VelX = 5,
            VelY = 0,
            X = 0,
            Y = 0,
        };
        var st = new VmPMobjXYMovement.XYMovementState
        {
            Actor = mo,
            ScrollX = 1,
            TryMove = (a, x, _, _, _tm) =>
            {
                a.X = x;
                return true;
            },
        };
        VmPMobjXYMovement.XYMovement(st);
        Assert.NotEqual(0, mo.Flags4 & VmPMobjXYMovement.Mf4Scrollmove);
    }

    [Fact]
    public void SubstepsForMove_SplitsFastMotion()
    {
        Assert.True(VmPMobjXYMovement.SubstepsForMove(100, 0, 20) > 1);
    }
}
