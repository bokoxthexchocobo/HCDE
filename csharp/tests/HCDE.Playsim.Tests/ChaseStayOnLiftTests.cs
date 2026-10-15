using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseStayOnLiftTests
{
    [Theory]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(false, false, false)]
    public void LeavingMatchingTaggedLiftUsesFlagOrCompatibility(bool flag, bool compatibility, bool draws)
    {
        var (sim, actor, _) = Room(compatibility); actor.StayOnLift = flag;
        var state = sim.CaptureState(); state.StayOnLiftRandomState = 0; sim.RestoreState(state);
        Walk(actor, 80); Assert.Equal(Fixed.FromInt(40), actor.X); Assert.Equal(draws ? 8 : 0, actor.MoveDirection);
        Assert.Equal(draws, sim.CaptureState().StayOnLiftRandomState != 0);
    }

    [Fact]
    public void RemainingOnLiftStillConsumesNativeDraw()
    {
        var (sim, actor, _) = Room(); Walk(actor, 8);
        Assert.Equal(0, actor.MoveDirection); Assert.NotNull(sim.CaptureState().StayOnLiftRandomState);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void MissingDeadOrDifferentlyTaggedTargetSuppressesDraw(int exclusion)
    {
        var (sim, actor, target) = Room(secondTag: exclusion == 2 ? 9 : 7);
        if (exclusion == 0) actor.SetTargetPointer(null);
        if (exclusion == 1) target.Health = 0;
        Walk(actor, 80); Assert.Equal(0, actor.MoveDirection); Assert.Null(sim.CaptureState().StayOnLiftRandomState);
    }

    [Fact]
    public void FailedMoveDoesNotConsumeLiftDraw()
    {
        var (sim, actor, _) = Room(); actor.MoveDirection = 8;
        ActorWanderActions.Wander(actor, ActorWanderFlags.NoRandomTurn | ActorWanderFlags.StopIfBlocked);
        Assert.Null(sim.CaptureState().StayOnLiftRandomState);
    }

    [Fact]
    public void LiftAvoidanceShortCircuitsModerateHazardDraw()
    {
        var (sim, actor, _) = Room(); actor.AvoidHazards = true;
        sim.Motions.Add(new SectorMotion { SectorIndex = 1, Kind = MotionKind.Ceiling, CrushDamage = 10, CeilingDirection = 1 });
        var state = sim.CaptureState(); state.StayOnLiftRandomState = 0; sim.RestoreState(state);
        Walk(actor, 80); Assert.Equal(8, actor.MoveDirection); Assert.Null(sim.CaptureState().AvoidCrushRandomState);
    }

    [Fact]
    public void TagComparisonUsesPrimaryTagOrSameUntaggedSector()
    {
        var (sim, actor, target) = Room(firstTag: 0, secondTag: 0);
        Assert.False(ActorMovementHazards.SharesPrimaryTag(sim, actor, target));
        target.SectorIndex = actor.SectorIndex;
        Assert.True(ActorMovementHazards.SharesPrimaryTag(sim, actor, target));
    }

    [Theory]
    [InlineData("STAYONLIFT")]
    [InlineData("Actor.stayonlift")]
    public void SharedFlagAndRevivalRestoreDefault(string name)
    {
        var (_, actor, _) = Room(); Assert.True(AcsActorFlags.TryGet(actor, name, out var value)); Assert.True(value);
        Assert.True(AcsActorFlags.TrySet(actor, name, false)); Assert.False(actor.StayOnLift);
        actor.StayOnLift = true; ActorRaise.ReviveSupported(actor); Assert.False(actor.StayOnLift);
    }

    [Fact]
    public void SaveRestoresRandomFlagAndChecksumAndOlderSaveClearsState()
    {
        var (sim, actor, _) = Room(); actor.StayOnLift = false; var old = SimSavegame.Write(sim, true);
        actor.StayOnLift = true; sim.NextStayOnLiftByte(); sim.RestoreState(sim.CaptureState());
        var checksum = sim.Checksum; var saved = SimSavegame.Write(sim, true); var expected = sim.NextStayOnLiftByte();
        SimSavegame.Apply(sim, saved); Assert.True(actor.StayOnLift); Assert.Equal(checksum, sim.Checksum);
        Assert.Equal(expected, sim.NextStayOnLiftByte());
        SimSavegame.Apply(sim, old); Assert.False(actor.StayOnLift); Assert.Null(sim.CaptureState().StayOnLiftRandomState);
    }

    [Fact]
    public void InvalidFlagAndRandomHeaderReject()
    {
        var (sim, _, _) = Room(); var state = sim.CaptureState(); state.Actors[0].StayOnLiftFlag = 2;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        sim.NextStayOnLiftByte(); var bytes = SimSavegame.Write(sim, true);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 16), 275);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-stayonliftrandom-header", error);
    }

    private static void Walk(Actor actor, int speed)
    {
        actor.ReactionTime = 0; actor.MoveCount = 100; actor.MoveDirection = 0; actor.MovementSpeed = Fixed.FromInt(speed);
        ActorWanderActions.Wander(actor, ActorWanderFlags.NoRandomTurn | ActorWanderFlags.StopIfBlocked);
    }

    private static (AuthoritySimulation, Actor, Actor) Room(bool compatible = false, int firstTag = 7, int secondTag = 7)
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128, Tag = firstTag }, new LevelSector { CeilingHeight = 128, Tag = secondTag }],
            Sides = geometry.Sides, Lines = geometry.Lines, Things = [new LevelThing { Type = 3004, X = -40 }]
        }, compat: compatible ? CompatSurface.StayOnLift : CompatSurface.None);
        var actor = sim.Actors.Single(); var target = sim.AddBot(100, 0); actor.SetTargetPointer(target); actor.StayOnLift = true;
        actor.ReactionTime = 0; actor.MoveCount = 100;
        sim.Motions.Add(new SectorMotion { SectorIndex = 0, Kind = MotionKind.Lift });
        return (sim, actor, target);
    }
}
