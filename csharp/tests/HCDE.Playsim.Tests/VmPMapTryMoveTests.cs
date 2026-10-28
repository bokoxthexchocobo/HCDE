namespace HCDE.Playsim.Tests;

public class VmPMapTryMoveTests
{
    [Fact]
    public void TryMove_RejectsHeightCrush()
    {
        var tm = new VmPMapTryMove.CheckPositionTm { FloorZ = 0, CeilingZ = 32 };
        var actor = new VmPMapTryMove.TryMoveActor { Z = 0, Height = 56 };
        var st = new VmPMapTryMove.TryMoveState
        {
            Actor = actor,
            Tm = tm,
            PosX = 64,
            PosY = 0,
            CheckPosition = (_, _, _, t) =>
            {
                t.FloorZ = 0;
                t.CeilingZ = 32;
                return true;
            },
        };
        Assert.False(VmPMapTryMove.TryMove(st));
    }

    [Fact]
    public void TryMove_AllowsFloatOkWhenOpen()
    {
        var tm = new VmPMapTryMove.CheckPositionTm();
        var actor = new VmPMapTryMove.TryMoveActor { Z = 0, Height = 40, X = 0, Y = 0 };
        var st = new VmPMapTryMove.TryMoveState
        {
            Actor = actor,
            Tm = tm,
            PosX = 32,
            PosY = 0,
            CheckPosition = (_, _, _, t) =>
            {
                t.FloorZ = 0;
                t.CeilingZ = 128;
                return true;
            },
            CommitMove = a => a.X = 32,
        };
        Assert.True(VmPMapTryMove.TryMove(st));
        Assert.True(tm.FloatOk);
    }

    [Fact]
    public void TryMove_BlocksPassMobjWhenCompat()
    {
        var tm = new VmPMapTryMove.CheckPositionTm { ThingFlags2 = 0 };
        var actor = new VmPMapTryMove.TryMoveActor { Z = 0, Height = 40, BlockingMobjPresent = true };
        var st = new VmPMapTryMove.TryMoveState
        {
            Actor = actor,
            Tm = tm,
            PosX = 32,
            PosY = 0,
            LevelCompatFlags = VmPMapTryMove.CompatfNoPassmobj,
            CheckPosition = (_, _, _, _) => false,
        };
        Assert.False(VmPMapTryMove.TryMove(st));
    }

    [Fact]
    public void TryMove_RunsSpechitCrossAfterPortalOk()
    {
        var tm = new VmPMapTryMove.CheckPositionTm { FloorZ = 0, CeilingZ = 128 };
        var actor = new VmPMapTryMove.TryMoveActor { Z = 0, Height = 40 };
        var activated = 0;
        var st = new VmPMapTryMove.TryMoveState
        {
            Actor = actor,
            Tm = tm,
            PosX = 0,
            PosY = 0,
            CheckPosition = (_, _, _, t) =>
            {
                t.FloorZ = 0;
                t.CeilingZ = 128;
                return true;
            },
            ResolvePortalCrossing = _ => true,
            Spechit = { new VmPMapTryMove.SpechitEntry { LineSpecial = 1 } },
            ActivateCrossLine = (_, _, _, _) => activated++,
            CommitMove = _ => { },
        };
        Assert.True(VmPMapTryMove.TryMove(st));
        Assert.Equal(1, activated);
    }

    [Fact]
    public void TryMove_BlocksDropoff()
    {
        var tm = new VmPMapTryMove.CheckPositionTm { FloorZ = 64, DropoffZ = 0, CeilingZ = 128 };
        var actor = new VmPMapTryMove.TryMoveActor
        {
            Z = 64,
            Height = 40,
            MaxDropOffHeight = 24,
            Flags = 0,
            Flags5 = 0,
        };
        var st = new VmPMapTryMove.TryMoveState
        {
            Actor = actor,
            Tm = tm,
            PosX = 32,
            PosY = 0,
            Dropoff = 0,
            CheckPosition = (_, _, _, t) =>
            {
                t.FloorZ = 64;
                t.DropoffZ = 0;
                t.CeilingZ = 128;
                return true;
            },
        };
        Assert.False(VmPMapTryMove.TryMove(st));
    }
}
