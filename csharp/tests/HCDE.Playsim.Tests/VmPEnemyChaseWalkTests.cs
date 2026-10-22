namespace HCDE.Playsim.Tests;

public class VmPEnemyChaseWalkTests
{
    [Fact]
    public void SmartMove_ClearsMovedirWhenLiftLost()
    {
        var s = new VmPEnemyChaseWalk.SmartMoveState
        {
            CompatFlags2 = VmPEnemyChaseWalk.CompatAvoidHazards,
            UnderDamageAfter = true,
            PrAvoidCrush = () => 100,
            Movedir = VmPEnemyChaseWalk.DiEast,
            Move = () => true,
        };
        Assert.True(VmPEnemyChaseWalk.SmartMove(s));
        Assert.Equal(VmPEnemyChaseWalk.DiNodir, s.Movedir);
    }

    [Fact]
    public void TryWalk_SetsMoveCountFromRng()
    {
        var s = new VmPEnemyChaseWalk.TryWalkState { PrTrywalk = () => 19 };
        s.Smart.Move = () => true;
        Assert.True(VmPEnemyChaseWalk.TryWalk(s));
        Assert.Equal(3, s.MoveCount);
    }

    [Fact]
    public void DoNewChaseDir_PicksDiagonalWhenBothAxesLarge()
    {
        var walked = false;
        var st = new VmPEnemyChaseWalk.DoNewChaseDirState
        {
            Movedir = VmPEnemyChaseWalk.DiNodir,
            TryWalk = () => { walked = true; return true; },
        };
        VmPEnemyChaseWalk.DoNewChaseDir(st, -20, -20);
        Assert.True(walked);
        Assert.Equal(VmPEnemyChaseWalk.DiSouthwest, st.Movedir);
    }

    [Fact]
    public void NewChaseDir_InvertsDeltaWhenFrightened()
    {
        double? dx = null;
        var s = new VmPEnemyChaseWalk.NewChaseDirState
        {
            DeltaX = 100,
            DeltaY = 0,
            TargetHasPlayer = true,
            TargetPlayerCheats = VmPEnemyChaseWalk.CfFrightening,
            DoNewChaseDir = (x, _) => { dx = x; },
        };
        VmPEnemyChaseWalk.NewChaseDir(s);
        Assert.Equal(-100, dx);
    }
}
