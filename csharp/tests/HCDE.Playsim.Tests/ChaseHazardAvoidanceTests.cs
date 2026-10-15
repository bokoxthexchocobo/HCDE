using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseHazardAvoidanceTests
{
    [Theory]
    [InlineData(true, false, 8)]
    [InlineData(false, true, 8)]
    [InlineData(false, false, 0)]
    public void EnteringHarmfulFloorInvalidatesDirectionWithoutRandomDraw(bool flag, bool compatibility, int direction)
    {
        var (sim, actor) = Room(compatibility); actor.AvoidHazards = flag;
        Walk(actor); Assert.Equal(Fixed.FromInt(40), actor.X); Assert.Equal(direction, actor.MoveDirection);
        Assert.Null(sim.CaptureState().AvoidCrushRandomState);
    }

    [Fact]
    public void NoSectorDamageExemptsDamagingFloor()
    {
        var (sim, actor) = Room(); actor.NoSectorDamage = true;
        Walk(actor); Assert.Equal(0, actor.MoveDirection);
    }

    [Fact]
    public void ExistingDangerDoesNotInvalidateMovementAgain()
    {
        var (sim, actor) = Room(); sim.Level.Sectors[0].DamageAmount = 10;
        Walk(actor); Assert.Equal(0, actor.MoveDirection); Assert.Null(sim.CaptureState().AvoidCrushRandomState);
    }

    [Theory]
    [InlineData(-1, -1)]
    [InlineData(1, 1)]
    [InlineData(0, 0)]
    public void CrusherDirectionDeterminesSeverity(int direction, int severity)
    {
        var (sim, actor) = Room(); sim.Level.Sectors[1].DamageAmount = 0;
        sim.Motions.Add(new SectorMotion { SectorIndex = 0, Kind = MotionKind.Ceiling, CrushDamage = 10, CeilingDirection = direction });
        Assert.Equal(severity, ActorMovementHazards.UnderDamage(sim, actor));
    }

    [Fact]
    public void EnteringRisingCrusherUsesOnlyAvoidCrushStream()
    {
        var (sim, actor) = Room(); sim.Level.Sectors[1].DamageAmount = 0;
        sim.Motions.Add(new SectorMotion { SectorIndex = 1, Kind = MotionKind.Ceiling, CrushDamage = 10, CeilingDirection = 1 });
        var expected = Room().Item1; var draw = expected.NextAvoidCrushByte();
        Walk(actor); Assert.Equal(draw < 200 ? 8 : 0, actor.MoveDirection);
        Assert.Equal(expected.CaptureState().AvoidCrushRandomState, sim.CaptureState().AvoidCrushRandomState);
        Assert.Null(sim.CaptureState().NewChaseDirRandomState);
    }

    [Fact]
    public void EnteringDescendingCrusherIsSeriousWithoutRandomDraw()
    {
        var (sim, actor) = Room(); sim.Level.Sectors[1].DamageAmount = 0;
        sim.Motions.Add(new SectorMotion { SectorIndex = 1, Kind = MotionKind.Ceiling, CrushDamage = 10, CeilingDirection = -1 });
        Walk(actor); Assert.Equal(8, actor.MoveDirection); Assert.Null(sim.CaptureState().AvoidCrushRandomState);
    }

    [Theory]
    [InlineData("AVOIDHAZARDS")]
    [InlineData("Actor.avoidhazards")]
    public void SharedFlagAndRevivalRestoreDefault(string name)
    {
        var (_, actor) = Room(); Assert.True(AcsActorFlags.TryGet(actor, name, out var value)); Assert.True(value);
        Assert.True(AcsActorFlags.TrySet(actor, name, false)); Assert.False(actor.AvoidHazards);
        actor.AvoidHazards = true; ActorRaise.ReviveSupported(actor); Assert.False(actor.AvoidHazards);
    }

    [Fact]
    public void SaveRestoresFlagRandomAndChecksumAndLegacyClearsThem()
    {
        var (sim, actor) = Room(); actor.AvoidHazards = false; var old = SimSavegame.Write(sim, true);
        actor.AvoidHazards = true; sim.NextAvoidCrushByte(); sim.RestoreState(sim.CaptureState());
        var checksum = sim.Checksum; var saved = SimSavegame.Write(sim, true); var expected = sim.NextAvoidCrushByte();
        SimSavegame.Apply(sim, saved); Assert.True(actor.AvoidHazards); Assert.Equal(checksum, sim.Checksum);
        Assert.Equal(expected, sim.NextAvoidCrushByte());
        SimSavegame.Apply(sim, old); Assert.False(actor.AvoidHazards); Assert.Null(sim.CaptureState().AvoidCrushRandomState);
    }

    [Fact]
    public void InvalidFlagAndRandomArchiveHeaderReject()
    {
        var (sim, _) = Room(); var state = sim.CaptureState(); state.Actors[0].AvoidHazardsFlag = 2;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        sim.NextAvoidCrushByte(); var bytes = SimSavegame.Write(sim, true);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 16), 273);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-avoidcrushrandom-header", error);
    }

    private static void Walk(Actor actor)
    {
        actor.ReactionTime = 0; actor.MoveCount = 100; actor.MoveDirection = 0; actor.MovementSpeed = Fixed.FromInt(80);
        ActorWanderActions.Wander(actor, ActorWanderFlags.NoRandomTurn | ActorWanderFlags.StopIfBlocked);
    }

    private static (AuthoritySimulation, Actor) Room(bool compatible = false)
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128, HurtMonsters = true },
                new LevelSector { CeilingHeight = 128, DamageAmount = 10, HurtMonsters = true }],
            Sides = geometry.Sides, Lines = geometry.Lines, Things = [new LevelThing { Type = 3004, X = -40 }]
        }, compat: compatible ? CompatSurface.AvoidHazards : CompatSurface.None);
        var actor = sim.Actors.Single(); actor.AvoidHazards = true; return (sim, actor);
    }
}
