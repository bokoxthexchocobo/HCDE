namespace HCDE.Playsim.Tests;

public class VmPEnemyLookCoreTests
{
    [Fact]
    public void IsVisible_RejectsBeyondMaxDist()
    {
        var s = new VmPEnemyLookCore.IsVisibleState { Distance2D = 500, MaxDist = 256, CheckSight = () => true };
        Assert.False(VmPEnemyLookCore.IsVisible(s));
    }

    [Fact]
    public void ValidEnemyInBlock_RejectsNeverTarget()
    {
        var s = new VmPEnemyLookCore.ValidEnemyBlockState
        {
            Flags = VmPEnemyLookCore.MfShootable,
            Health = 50,
            Flags3 = VmPEnemyLookCore.Mf3Ismonster,
            Flags7 = VmPEnemyLookCore.Mf7Nevertarget,
        };
        Assert.False(VmPEnemyLookCore.ValidEnemyInBlock(s));
    }

    [Fact]
    public void IsTargetablePlayer_RejectsFriend()
    {
        var s = new VmPEnemyLookCore.TargetablePlayerState { IsFriend = true, MoNull = false, MoFlags = VmPEnemyLookCore.MfShootable, Health = 50 };
        s.Visible.CheckSight = () => true;
        Assert.False(VmPEnemyLookCore.IsTargetablePlayer(s));
    }

    [Fact]
    public void ValidEnemyInBlock_AllowsNonFriendlyKeepChecking()
    {
        var s = new VmPEnemyLookCore.ValidEnemyBlockState
        {
            Flags = VmPEnemyLookCore.MfShootable,
            Health = 50,
            Flags3 = VmPEnemyLookCore.Mf3Ismonster,
            LookeeFriendly = true,
            OtherFriendly = false,
            Visible = true,
        };
        Assert.True(VmPEnemyLookCore.ValidEnemyInBlock(s));
    }

    [Fact]
    public void LookForPlayers_SetsTargetWhenPlayerTargetable()
    {
        var s = new VmPEnemyLookCore.LookForPlayersState
        {
            IsTargetable = (_, t) =>
            {
                t.MoNull = false;
                t.MoFlags = VmPEnemyLookCore.MfShootable;
                t.Health = 50;
                t.IsFriend = false;
                t.Visible.CheckSight = () => true;
                return VmPEnemyLookCore.IsTargetablePlayer(t);
            },
        };
        s.PlayerInGame[1] = true;
        Assert.True(VmPEnemyLookCore.LookForPlayers(s, false, null));
        Assert.Equal(1, s.Target);
    }

    [Fact]
    public void LookForPlayers_PicksGoalWhenScanExhausted()
    {
        var goal = new object();
        var s = new VmPEnemyLookCore.LookForPlayersState
        {
            Goal = goal,
            Target = null,
            IsTargetable = (_, _) => false,
        };
        s.PlayerInGame[0] = true;
        Assert.True(VmPEnemyLookCore.LookForPlayers(s, false, null));
        Assert.Same(goal, s.Target);
    }
}
