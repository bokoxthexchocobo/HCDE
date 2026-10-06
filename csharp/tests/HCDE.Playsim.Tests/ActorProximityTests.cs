using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorProximityTests
{
    [Theory]
    [InlineData(99, 0, 0, true)]
    [InlineData(100, 0, 0, false)]
    [InlineData(0, 155, 0, true)]
    [InlineData(0, 156, 0, false)]
    [InlineData(0, 300, 4, true)]
    public void GeometryUsesStrictHorizontalAndVerticalBodyGap(int x, int z, int flags, bool expected)
    {
        var sim = Room(); var target = sim.AddBot(x, 0, 3001); target.Z = Fixed.FromInt(z);
        Assert.Equal(expected, ActorProximity.Check(sim.Players.Single(), "DoomImp", 100, flags: flags));
    }

    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(0, 32, false)]
    [InlineData(2, 0, false)]
    [InlineData(2, 2, true)]
    [InlineData(2, 34, false)]
    [InlineData(1, 32, true)]
    public void CountModesHandleZeroExactAndLessOrEqual(int required, int flags, bool expected)
    {
        var sim = Room(); sim.AddBot(20, 0, 3001);
        Assert.Equal(expected, ActorProximity.Check(sim.Players.Single(), "DoomImp", 100, required, flags));
    }

    [Fact]
    public void DeathFlagsAndRemovedActorsControlCounting()
    {
        var sim = Room(); var target = sim.AddBot(20, 0, 3001); var player = sim.Players.Single();
        Assert.False(ActorProximity.Check(player, "DoomImp", 100, flags: 16));
        target.Health = 0;
        target.Killed = true;
        Assert.False(ActorProximity.Check(player, "DoomImp", 100));
        Assert.True(ActorProximity.Check(player, "DoomImp", 100, flags: 8));
        Assert.True(ActorProximity.Check(player, "DoomImp", 100, flags: 16));
        target.Destroy(); Assert.False(ActorProximity.Check(player, "DoomImp", 100, flags: 8));
    }

    [Fact]
    public void ForwardedReferenceIsExcludedAndFrameJumpRunsOnCaller()
    {
        var sim = Room(); var caller = sim.AddBot(200, 0, 3004); var reference = sim.AddBot(0, 0, 3001);
        caller.Brain!.SetSpecialTarget(reference);
        Assert.False(ActorProximity.Check(caller, "DoomImp", 100, pointerSelector: 2));
        sim.AddBot(20, 0, 3001);
        caller.States.Configure(caller, [new(-1, 0), new(5, 0,
            StateAction: self => ActorJumpActions.CheckProximity(self, 2, "DoomImp", 100, pointerSelector: 2)), new(-1, 2)], 0);
        caller.States.Enter(caller, 1); Assert.Equal(2, caller.States.Current);
        Assert.False(ActorProximity.Check(caller, "DoomImp", 100, pointerSelector: 1));
    }

    [Fact]
    public void SightFlagChecksOcclusionAndUnsupportedFlagsFailExplicitly()
    {
        var sim = Room(blocked: true); sim.AddBot(80, 0, 3001); var player = sim.Players.Single();
        Assert.True(ActorProximity.Check(player, "DoomImp", 100));
        Assert.False(ActorProximity.Check(player, "DoomImp", 100, flags: 4096));
        Assert.Throws<NotSupportedException>(() => ActorProximity.Check(player, "DoomImp", 100, flags: 64));
        Assert.False(ActorProximity.Check(player, "Unknown", 100, count: 0));
        Assert.False(ActorProximity.Check(player, "DoomImp", 0, count: 0));
    }

    [Theory]
    [InlineData(100, 0, 0, 0)]
    [InlineData(20, 300, 0, 0)]
    [InlineData(20, 300, 4, 1)]
    public void AcsUsesSameStrictGeometryAndReadsOptionalFlags(int x, int z, int flags, int expected)
    {
        var sim = Room(); var target = sim.AddBot(x, 0, 3001); target.Z = Fixed.FromInt(z);
        var stack = new List<int> { 0, 0, 100 << 16, 1, flags, 0 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding { Value = sim.Players.Single() },
            ["DoomImp"], new AcsGlobalStrings(), AcsCallFunctions.CheckProximity, 6, out var result));
        Assert.Equal(expected, result); Assert.Empty(stack);
    }

    private static AuthoritySimulation Room(bool blocked = false) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }], Sides = [new LevelSide { Sector = 0 }],
        Lines = blocked ? [new LevelLine { X1 = 50, X2 = 50, Y1 = -128, Y2 = 128, SideFront = 0, SideBack = -1 }] : [],
        Things = [new LevelThing { Type = 1 }],
    });
}
