using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorProximityTargetTests
{
    [Theory]
    [InlineData(64, 80)]
    [InlineData(1088, 10)]
    [InlineData(576, 90)]
    public void TargetSelectionHonorsFirstClosestAndFarthest(int flags, int expectedX)
    {
        var sim = Room(); var caller = sim.AddBot(0, 0, 3004);
        sim.AddBot(80, 0, 3001); sim.AddBot(20, 0, 3001); sim.AddBot(90, 0, 3001); sim.AddBot(10, 0, 3001);
        Assert.True(ActorProximity.Check(caller, "DoomImp", 100, flags: flags));
        Assert.Equal(sim.Actors.Single(a => a.X.ToDouble() == expectedX).Id, caller.Brain!.TargetId);
    }

    [Fact]
    public void FailedExactCheckStillChangesTargetAndCountsAllForPreference()
    {
        var sim = Room(); var caller = sim.AddBot(0, 0); sim.AddBot(80, 0, 3001);
        sim.AddBot(20, 0, 3001); var nearest = sim.AddBot(10, 0, 3001);
        Assert.False(ActorProximity.Check(caller, "DoomImp", 100, flags: 64 | 32 | 1024));
        Assert.Equal(nearest.Id, caller.Brain!.TargetId);
    }

    [Fact]
    public void SetOnPointerChangesReferenceInsteadOfCaller()
    {
        var sim = Room(); var caller = sim.AddBot(200, 0); var reference = sim.AddBot(0, 0, 3001);
        var nearby = sim.AddBot(20, 0, 3001); caller.Brain!.SetSpecialTarget(reference);
        Assert.True(ActorProximity.Check(caller, "DoomImp", 100, flags: 64 | 2048, pointerSelector: 2));
        Assert.Equal(reference.Id, caller.Brain.TargetId); Assert.Equal(nearby.Id, reference.Brain!.TargetId);
    }

    [Fact]
    public void NullJumpLabelCanChangeTargetButNoMatchPreservesPreviousTarget()
    {
        var sim = Room(); var caller = sim.AddBot(0, 0); var imp = sim.AddBot(20, 0, 3001);
        Assert.Null(ActorJumpActions.CheckProximity(caller, null, "DoomImp", 100, flags: 64));
        Assert.Equal(imp.Id, caller.Brain!.TargetId);
        Assert.Null(ActorJumpActions.CheckProximity(caller, null, "DoomImp", 1, flags: 64));
        Assert.Equal(imp.Id, caller.Brain.TargetId);
        Assert.Null(ActorJumpActions.CheckProximity(caller, null, "DoomImp", 100, flags: 1));
    }

    [Fact]
    public void CountingCanAlsoSelectTargetUsingHorizontalPreference()
    {
        var sim = Room(); var caller = sim.AddBot(0, 0); var nearest = sim.AddBot(10, 0, 3001);
        nearest.Z = Fixed.FromInt(200); sim.AddBot(20, 0, 3001);
        Assert.Equal(2, ActorProximity.CountProximity(caller, "DoomImp", 100, flags: 64 | 1024 | 4));
        Assert.Equal(nearest.Id, caller.Brain!.TargetId);
    }

    [Fact]
    public void AcsOptionalFlagsChangeActivatorTarget()
    {
        var sim = Room(); var caller = sim.AddBot(0, 0); var imp = sim.AddBot(20, 0, 3001);
        var stack = new List<int> { 0, 0, 100 << 16, 1, 64 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding { Value = caller },
            ["DoomImp"], new AcsGlobalStrings(), AcsCallFunctions.CheckProximity, 5, out var result));
        Assert.Equal(1, result); Assert.Equal(imp.Id, caller.Brain!.TargetId); Assert.Empty(stack);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 512 }] });
}
