using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorCheckBlockTests
{
    [Theory]
    [InlineData(true, 4, true)]
    [InlineData(false, 4, false)]
    [InlineData(true, 68, false)]
    [InlineData(false, 68, false)]
    public void OrdinaryMoverRequiresSolidityForActorBlock(bool solid, int flags, bool blocked)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var blocker = sim.AddBot(0, 0);
        var previous = sim.AddBot(200, 0); actor.MasterId = previous.Id; actor.Solid = solid;
        Assert.Equal(blocked ? 2 : (int?)null, ActorJumpActions.CheckBlock(actor, 2, flags));
        Assert.Equal(solid ? blocker.Id : previous.Id, actor.MasterId);
        Assert.Equal(default, actor.X); Assert.Equal(default, actor.Y);
    }

    [Fact]
    public void NonsolidMoverStillChecksWalls()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 1, X = 500 }],
            Sectors = [new LevelSector { CeilingHeight = 128 }], Sides = [new LevelSide { Sector = 0 }],
            Lines = [new LevelLine { X1 = 100, Y1 = -100, X2 = 100, Y2 = 100, SideFront = 0, SideBack = -1 }]
        });
        var actor = sim.AddBot(0, 0); actor.Solid = false;
        Assert.Equal(2, ActorJumpActions.CheckBlock(actor, 2, xOffset: 100));
        Assert.Equal(default, actor.X);
    }

    [Theory]
    [InlineData(30, 30, true)]
    [InlineData(-30, -30, true)]
    [InlineData(39, 39, true)]
    [InlineData(40, 0, false)]
    [InlineData(0, -40, false)]
    [InlineData(40, 40, false)]
    public void InitialActorProbeUsesNativeSquareBounds(int x, int y, bool blocked)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var other = sim.AddBot(x, y);
        actor.Radius = other.Radius = Fixed.FromInt(20);
        var before = SimSavegame.Write(sim);
        Assert.Equal(blocked ? 2 : (int?)null, ActorJumpActions.CheckBlock(actor, 2));
        Assert.Equal(before, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(4, true)]
    [InlineData(68, false)]
    public void DiagonalBlockerSetsPointerEvenWhenActorResultIsFiltered(int flags, bool blocked)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var other = sim.AddBot(30, 30);
        actor.Radius = other.Radius = Fixed.FromInt(20);
        Assert.Equal(blocked ? 2 : (int?)null, ActorJumpActions.CheckBlock(actor, 2, flags));
        Assert.Equal(other.Id, actor.MasterId);
    }

    [Theory]
    [InlineData(0, 100, 0, 0, true)]
    [InlineData(0, 0, 100, 0, false)]
    [InlineData(0, 100, 0, 90, false)]
    [InlineData(128, 100, 0, 90, true)]
    public void ProbeOffsetsUseNativeRotationAndRestorePosition(int flags, double x, double y, double angle, bool expected)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); sim.AddBot(100, 0);
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(expected ? 2 : (int?)null, ActorJumpActions.CheckBlock(actor, 2, flags, xOffset: x, yOffset: y, angle: angle));
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void CallerAngleControlsMasterProbeAndAbsoluteAngleOverridesIt()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var master = sim.AddBot(200, 0); sim.AddBot(200, 100);
        actor.MasterId = master.Id; actor.Angle = BamAngle.FromDegrees(90);
        Assert.Equal(2, ActorJumpActions.CheckBlock(actor, 2, pointerSelector: AcsActorPointer.Master, xOffset: 100));
        Assert.Null(ActorJumpActions.CheckBlock(actor, 2, flags: 256, pointerSelector: AcsActorPointer.Master, xOffset: 100));
        Assert.Equal(Fixed.FromInt(200), master.X); Assert.Equal(default, master.Y);
    }

    [Fact]
    public void FloorOnlyFailureDoesNotCountAsActorOrLineBlock()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        Assert.Null(ActorJumpActions.CheckBlock(actor, 2, zOffset: -100));
        Assert.Null(ActorJumpActions.CheckBlock(actor, 2, pointerSelector: AcsActorPointer.Null));
    }

    [Fact]
    public void UnsupportedFlagsFailBeforeMovement()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        Assert.Throws<NotSupportedException>(() => ActorJumpActions.CheckBlock(actor, 2, flags: 512, xOffset: 100));
        Assert.Equal(default, actor.X);
    }

    [Fact]
    public void WallProbeCanReturnAStateWithoutMovingActor()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 1, X = 500 }],
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Sides = [new LevelSide { Sector = 0 }],
            Lines = [new LevelLine { X1 = 100, Y1 = -100, X2 = 100, Y2 = 100, SideFront = 0, SideBack = -1, Flags = 1 }]
        });
        var actor = sim.AddBot(0, 0);
        actor.States.Configure(actor, [new(-1, 0), new(5, 0,
            StateAction: self => ActorJumpActions.CheckBlock(self, 2, xOffset: 100)), new(-1, 2)], 0);
        actor.States.Enter(actor, 1);
        Assert.Equal(2, actor.States.Current); Assert.Equal(default, actor.X);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(64, false)]
    [InlineData(65, false)]
    public void ActorFilterControlsReportedBlockWithoutMoving(int flags, bool expected)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); sim.AddBot(100, 0);
        Assert.Equal(expected ? 2 : (int?)null, ActorJumpActions.CheckBlock(actor, 2, flags, xOffset: 100));
        Assert.Equal(default, actor.X);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(64, true)]
    [InlineData(65, false)]
    public void WallFilterControlsReportedBlock(int flags, bool expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 1, X = 500 }], Sectors = [new LevelSector { CeilingHeight = 128 }],
            Sides = [new LevelSide { Sector = 0 }],
            Lines = [new LevelLine { X1 = 100, Y1 = -100, X2 = 100, Y2 = 100, SideFront = 0, SideBack = -1, Flags = 1 }]
        });
        var actor = sim.AddBot(0, 0);
        Assert.Equal(expected ? 2 : (int?)null, ActorJumpActions.CheckBlock(actor, 2, flags, xOffset: 100));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void BlockerPointersUseSetterSelectionEvenWhenActorsAreFiltered(bool onPointer, bool filtered)
    {
        var sim = Room(); var caller = sim.AddBot(0, 0); var subject = sim.AddBot(200, 0); var blocker = sim.AddBot(300, 0);
        caller.MasterId = subject.Id; blocker.MasterId = onPointer ? subject.Id : caller.Id;
        var flags = 2 | 4 | (onPointer ? 16 : 0) | (filtered ? 64 : 0);
        Assert.Equal(filtered ? (int?)null : 2, ActorJumpActions.CheckBlock(caller, 2, flags,
            AcsActorPointer.Master, xOffset: 100));
        var setter = onPointer ? subject : caller;
        Assert.Equal(blocker.Id, setter.MasterId); Assert.Equal(blocker.Id, setter.Brain!.TargetId);
        Assert.Equal(Fixed.FromInt(200), subject.X);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        setter.MasterId = null; setter.Brain.SetSpecialTarget(null); sim.RestoreState(state);
        Assert.Equal(blocker.Id, setter.MasterId); Assert.Equal(blocker.Id, setter.Brain.TargetId);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void ProjectileSetterCanReceiveTracer()
    {
        var sim = Room(); var caller = sim.AddBot(0, 0); var blocker = sim.AddBot(300, 0);
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Rocket);
        missile.X = Fixed.FromInt(200); missile.Y = missile.Z = default; caller.MasterId = missile.Id;
        Assert.Equal(2, ActorJumpActions.CheckBlock(caller, 2, 8 | 16, AcsActorPointer.Master, xOffset: 100));
        Assert.Equal(blocker.Id, missile.TracerTargetId); Assert.Equal(Fixed.FromInt(200), missile.X);
    }

    [Fact]
    public void UnsupportedTracerPreventsEarlierPointerWritesAndRestoresProbe()
    {
        var sim = Room(); var caller = sim.AddBot(0, 0); sim.AddBot(100, 0);
        Assert.Throws<NotSupportedException>(() => ActorJumpActions.CheckBlock(caller, 2, 2 | 4 | 8, xOffset: 100));
        Assert.Null(caller.MasterId); Assert.Null(caller.Brain!.TargetId); Assert.Equal(default, caller.X);
    }

    [Theory]
    [InlineData(-24, false)]
    [InlineData(-25, true)]
    [InlineData(24, false)]
    [InlineData(25, true)]
    [InlineData(100, true)]
    public void DropoffProbeUsesRequestedZForStepDropAndHeadroom(double z, bool blocked)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(blocked ? 2 : (int?)null, ActorJumpActions.CheckBlock(actor, 2, flags: 32, zOffset: z));
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FloatingAndDropoffFlagsAllowProbeAboveFloor(bool floating)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Floating = floating; actor.AllowDropOff = !floating;
        Assert.Null(ActorJumpActions.CheckBlock(actor, 2, flags: 32, zOffset: 30));
    }

    [Fact]
    public void DropoffFilteredActorDoesNotAssignPointerWhenMoveFits()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); sim.AddBot(100, 0);
        Assert.Null(ActorJumpActions.CheckBlock(actor, 2, flags: 32 | 64 | 4, xOffset: 100));
        Assert.Null(actor.MasterId);
    }

    [Theory]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    public void LegalStepRechecksOverlapAtFloorEvenWithNoActors(bool filtered, bool thruActors, bool blocked)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, 3004); var overhead = sim.AddBot(100, 0, 3004);
        overhead.Z = Fixed.FromInt(50); actor.ThruActors = thruActors;
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(blocked ? 2 : (int?)null, ActorJumpActions.CheckBlock(actor, 2, 32 | (filtered ? 64 : 0),
            xOffset: 100, zOffset: -10));
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void StepRecheckDoesNotSetInitialBlockerPointer()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, 3004); var overhead = sim.AddBot(100, 0, 3004);
        overhead.Z = Fixed.FromInt(50);
        Assert.Equal(2, ActorJumpActions.CheckBlock(actor, 2, 32 | 4, xOffset: 100, zOffset: -10));
        Assert.Null(actor.MasterId); Assert.Equal(default, actor.Z);
    }

    [Theory]
    [InlineData(30, true)]
    [InlineData(40, false)]
    public void PostStepUsesSquareBoundsWithStrictHorizontalEdges(int diagonal, bool blocked)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, 3004); var overhead = sim.AddBot(100 + diagonal, diagonal, 3004);
        overhead.Z = Fixed.FromInt(50); actor.Radius = overhead.Radius = Fixed.FromInt(20);
        Assert.Equal(blocked ? 2 : (int?)null, ActorJumpActions.CheckBlock(actor, 2, flags: 32, xOffset: 100, zOffset: -10));
        Assert.Equal(default, actor.Z);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void PostStepOnlyIceCorpseCollidesWithSolidCorpse(bool ice, bool blocked)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, 3004); var corpse = sim.AddBot(100, 0, 3004);
        corpse.Health = 0; corpse.Solid = true; corpse.Z = Fixed.FromInt(50); actor.IceCorpse = ice;
        Assert.Equal(blocked ? 2 : (int?)null, ActorJumpActions.CheckBlock(actor, 2, flags: 32, xOffset: 100, zOffset: -10));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void SpecialPickupProbeIsBlockedOnlyByBridge(bool bridge, bool blocked)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, 3004); var overhead = sim.AddBot(100, 0, 3004);
        actor.SpecialPickup = true; overhead.ActsLikeBridge = bridge; overhead.Z = Fixed.FromInt(50);
        Assert.Equal(blocked ? 2 : (int?)null, ActorJumpActions.CheckBlock(actor, 2, flags: 32, xOffset: 100, zOffset: -10));
    }

    [Fact]
    public void SolidPickupDoesNotBlockPostStep()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, 3004); var overhead = sim.AddBot(100, 0, 3004);
        overhead.SpecialPickup = true; overhead.Z = Fixed.FromInt(50);
        Assert.Null(ActorJumpActions.CheckBlock(actor, 2, flags: 32, xOffset: 100, zOffset: -10));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Things = [new LevelThing { Type = 1, X = 500 }], Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
