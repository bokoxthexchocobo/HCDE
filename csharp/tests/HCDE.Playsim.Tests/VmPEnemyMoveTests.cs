namespace HCDE.Playsim.Tests;

public class VmPEnemyMoveTests
{
    [Fact]
    public void Move_BlastedActorAlwaysSucceeds()
    {
        var s = new VmPEnemyMove.MoveState { Flags2 = VmPEnemyMove.Mf2Blasted };
        Assert.True(VmPEnemyMove.Move(s));
    }

    [Fact]
    public void Move_RejectsInvalidMovedir()
    {
        var s = new VmPEnemyMove.MoveState { Movedir = VmPEnemyMove.DiNodir };
        Assert.False(VmPEnemyMove.Move(s));
        Assert.Equal(VmPEnemyMove.DiNodir, s.Movedir);
    }

    [Fact]
    public void Move_BlocksWhenFloatingAboveFloor()
    {
        var s = new VmPEnemyMove.MoveState { Z = 64, FloorZ = 0, CanJump = () => false };
        Assert.False(VmPEnemyMove.Move(s));
    }

    [Fact]
    public void Move_ActivatesSpechitWhenBlocked()
    {
        var blocking = new object();
        var other = new object();
        var s = new VmPEnemyMove.MoveState
        {
            Movedir = 0,
            Speed = 8,
            Radius = 16,
            Flags4 = VmPEnemyMove.Mf4Canusewalls,
            BlockingLine = blocking,
            TryMove = (_, _, _) => false,
            PrOpendoor = () => 250,
            ActivateUseLine = _ => true,
        };
        s.Spechit.Push(new VmPEnemyMove.SpechitEntry { Line = other });
        Assert.True(VmPEnemyMove.Move(s));
    }
}
