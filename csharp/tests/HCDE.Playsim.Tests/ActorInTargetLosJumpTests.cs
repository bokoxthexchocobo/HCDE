using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorInTargetLosJumpTests
{
    [Theory]
    [InlineData(0, 90, true)]
    [InlineData(45, 90, true)]
    [InlineData(46, 90, false)]
    [InlineData(359, 10, true)]
    [InlineData(180, 0, true)]
    [InlineData(180, 360, true)]
    public void FieldOfViewUsesTargetsFacingAndInclusiveHalfAngle(int facing, int fov, bool jumps)
    {
        var sim = Room(); var caller = sim.AddBot(100, 0); var target = sim.Players.Single();
        caller.Brain!.SetSpecialTarget(target); target.Angle = BamAngle.FromDegrees(facing);
        caller.Angle = BamAngle.FromDegrees(180);
        Assert.Equal(jumps ? 2 : (int?)null, ActorJumpActions.JumpIfInTargetLOS(caller, 2, fov));
    }

    [Theory]
    [InlineData(0, 101, false)]
    [InlineData(2, 101, false)]
    [InlineData(4, 101, false)]
    [InlineData(12, 101, true)]
    [InlineData(12, 100, false)]
    [InlineData(28, 101, false)]
    public void CloseRangeFlagsBypassOnlyRequestedChecksAndUseStrictBoundary(int flags, int close, bool jumps)
    {
        var sim = Room(blocked: true); var caller = sim.AddBot(100, 0); var target = sim.Players.Single();
        caller.Brain!.SetSpecialTarget(target); target.Angle = BamAngle.FromDegrees(180);
        Assert.Equal(jumps ? 2 : (int?)null, ActorJumpActions.JumpIfInTargetLOS(caller, 2, 90, flags, closeDistance: close));
    }

    [Fact]
    public void MaximumRangeUsesPositionDistanceIncludingZ()
    {
        var sim = Room(); var caller = sim.AddBot(60, 0); caller.Z = Fixed.FromInt(80);
        caller.Brain!.SetSpecialTarget(sim.Players.Single());
        Assert.Equal(2, ActorJumpActions.JumpIfInTargetLOS(caller, 2, flags: 2, maxDistance: 100));
        Assert.Null(ActorJumpActions.JumpIfInTargetLOS(caller, 2, flags: 2, maxDistance: 99));
    }

    [Fact]
    public void DeadTargetIsAllowedUnlessFlaggedButRemovedTargetIsNot()
    {
        var sim = Room(); var caller = sim.AddBot(100, 0); var target = sim.Players.Single();
        caller.Brain!.SetSpecialTarget(target); target.Health = 0;
        Assert.Equal(2, ActorJumpActions.JumpIfInTargetLOS(caller, 2));
        Assert.Null(ActorJumpActions.JumpIfInTargetLOS(caller, 2, flags: 32));
        target.Destroy();
        Assert.Null(ActorJumpActions.JumpIfInTargetLOS(caller, 2));
    }

    [Fact]
    public void NoSightBypassesWallAndReturnedStateChangesCaller()
    {
        var sim = Room(blocked: true); var caller = sim.AddBot(100, 0);
        caller.Brain!.SetSpecialTarget(sim.Players.Single());
        Assert.Null(ActorJumpActions.JumpIfInTargetLOS(caller, 2));
        caller.States.Configure(caller, [new(-1, 0), new(5, 0,
            StateAction: self => ActorJumpActions.JumpIfInTargetLOS(self, 2, flags: 2)), new(-1, 2)], 0);
        caller.States.Enter(caller, 1);
        Assert.Equal(2, caller.States.Current);
        Assert.Null(ActorJumpActions.JumpIfInTargetLOS(caller, 2, flags: 64));
    }

    [Fact]
    public void ProjectileFlagSelectsSeekerTracerAndMasterTakesPrecedence()
    {
        var sim = Room(); var owner = sim.Players.Single(); var target = sim.AddBot(100, 0);
        var seeker = sim.SpawnProjectile(owner, ProjectileKind.RevenantTracer, target);
        Assert.Equal(2, ActorJumpActions.JumpIfInTargetLOS(seeker, 2, flags: 3));
        Assert.Null(ActorJumpActions.JumpIfInTargetLOS(seeker, 2, flags: 67));
        var rocket = sim.SpawnProjectile(owner, ProjectileKind.Rocket, target);
        rocket.RestoreTracerTarget(target.Id);
        Assert.Null(ActorJumpActions.JumpIfInTargetLOS(rocket, 2, flags: 3));
        target.Destroy();
        Assert.Null(ActorJumpActions.JumpIfInTargetLOS(seeker, 2, flags: 3));
    }

    private static AuthoritySimulation Room(bool blocked = false) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }], Sides = [new LevelSide { Sector = 0 }],
        Lines = blocked ? [new LevelLine { X1 = 50, X2 = 50, Y1 = -128, Y2 = 128,
            SideFront = 0, SideBack = 0, Flags = LevelLine.BlockSightFlag }] : [],
        Things = [new LevelThing { Type = 1 }],
    });
}
