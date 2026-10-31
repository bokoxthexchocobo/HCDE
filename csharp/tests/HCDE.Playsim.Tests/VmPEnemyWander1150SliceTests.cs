namespace HCDE.Playsim.Tests;

public class VmPEnemyWander1150SliceTests
{
    [Fact]
    public void WanderStep_ClearsCombatAndTurnsBeforeANewDirection()
    {
        Assert.False(VmPEnemyWander1150Slice.ClearLastHeard(true));
        Assert.Equal(-180d, VmPEnemyWander1150Slice.Normalize180(180));
        Assert.Equal(640, VmPEnemyWander1150Slice.BossStrafeRange);

        int moves = 0;
        var talking = Step(flags5: VmPEnemyWander1150Slice.InConversation, flags4: VmPEnemyWander1150Slice.InCombat, reactionTime: 4, smart: () =>
        {
            moves++;
            return false;
        });
        Assert.Equal(0, talking.Flags4);
        Assert.Equal(4, talking.ReactionTime);
        Assert.Equal(0, moves);

        var waiting = Step(reactionTime: 2, yaw: 10);
        Assert.Equal(1, waiting.ReactionTime);
        Assert.Equal(10, waiting.Yaw);

        var west = Step(moveDir: 4, yaw: 10, moveCount: 1);
        Assert.Equal(-45d, west.Yaw);
        Assert.False(west.ChoseNewDirection);
        Assert.Equal(0, west.MoveCount);

        moves = 0;
        var expired = Step(moveCount: 0, smart: () =>
        {
            moves++;
            return false;
        });
        Assert.Equal(0, moves);
        Assert.True(expired.ChoseNewDirection);
        Assert.Equal(4, expired.MoveCount);

        var blocked = Step(moveCount: 3, chaseFlags: VmPEnemyWander1150Slice.StopIfBlocked, smart: () => false);
        Assert.False(blocked.ChoseNewDirection);
        Assert.Equal(2, blocked.MoveCount);
    }

    [Fact]
    public void Look2_SeesAHostileAndFidgetsWhenTheSearchFails()
    {
        var quiet = VmPEnemyWander1150Slice.Look(
            0, 0, VmPEnemyWander1150Slice.InConversation, 0, false, 0, 0, false, false, Array.Empty<int>());
        Assert.True(quiet.Stopped);
        Assert.False(quiet.ThresholdCleared);

        var hostile = VmPEnemyWander1150Slice.Look(
            0, 0, 0, 0, true, 100, VmPEnemyWander1150Slice.Shootable | VmPEnemyWander1150Slice.Friendly, true, false, Array.Empty<int>());
        Assert.True(hostile.Saw);
        Assert.Equal(10, hostile.Threshold);
        Assert.False(hostile.InCombat);

        var allies = VmPEnemyWander1150Slice.Look(
            VmPEnemyWander1150Slice.Friendly, 0, 0, VmPEnemyWander1150Slice.NoAllies, true, 100,
            VmPEnemyWander1150Slice.Shootable | VmPEnemyWander1150Slice.Friendly, false, false, Array.Empty<int>());
        Assert.True(allies.Saw);
        Assert.Equal(10, allies.Threshold);

        var friend = VmPEnemyWander1150Slice.Look(
            VmPEnemyWander1150Slice.Friendly, 0, 0, 0, true, 100,
            VmPEnemyWander1150Slice.Shootable | VmPEnemyWander1150Slice.Friendly, false, true, Array.Empty<int>());
        Assert.True(friend.Saw);
        Assert.True(friend.InCombat);
        Assert.Equal(0, friend.Threshold);

        var deaf = VmPEnemyWander1150Slice.Look(
            VmPEnemyWander1150Slice.Ambush, 0, 0, 0, true, 100,
            VmPEnemyWander1150Slice.Shootable | VmPEnemyWander1150Slice.Friendly, false, false, new[] { 10, 1, 40 });
        Assert.False(deaf.Saw);
        Assert.Equal(2, deaf.SpawnFrame);
        Assert.Null(deaf.IdleFrame);

        var idle = VmPEnemyWander1150Slice.Look(0, 0, 0, 0, false, 0, 0, false, false, new[] { 40, 39 });
        Assert.Null(idle.SpawnFrame);
        Assert.Equal(3, idle.IdleFrame);

        var still = VmPEnemyWander1150Slice.Look(
            0, VmPEnemyWander1150Slice.StandStill, 0, 0, false, 0, 0, false, false, new[] { 30 });
        Assert.Null(still.SpawnFrame);
        Assert.Null(still.IdleFrame);
    }

    private static VmPEnemyWander1150Slice.Wander Step(
        int flags4 = VmPEnemyWander1150Slice.InCombat,
        int flags5 = 0,
        int reactionTime = 0,
        int moveDir = 8,
        double yaw = 0,
        int moveCount = 3,
        int chaseFlags = 0,
        Func<bool>? smart = null)
    {
        return VmPEnemyWander1150Slice.WanderStep(
            flags4, flags5, reactionTime, moveDir, yaw, moveCount, chaseFlags, smart ?? (() => true));
    }
}
