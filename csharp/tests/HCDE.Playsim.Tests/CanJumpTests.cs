using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CanJumpTests
{
    [Theory]
    [InlineData(false, -40)]
    [InlineData(true, 40)]
    public void JumpEligibilityAllowsAirborneWalkingWithoutSettling(bool canJump, int expectedX)
    {
        var sim = MovementRoom(0, 128); var actor = sim.Actors.Single();
        actor.CanJump = canJump; actor.Z = Fixed.FromInt(16); actor.AllowDropOff = true;
        Walk(actor);
        Assert.Equal(expectedX, actor.X.ToDouble()); Assert.Equal(16, actor.Z.ToDouble());
    }

    [Fact]
    public void JumpEligibilityUsesBlockedMoveHeightCorrectionWithoutFloatFlag()
    {
        var sim = MovementRoom(64, 128); var actor = sim.Actors.Single(); actor.CanJump = true;
        Walk(actor);
        Assert.Equal(4, actor.Z.ToDouble()); Assert.True(actor.InFloat); Assert.False(actor.Floating);
        Assert.InRange(actor.X.ToDouble(), -39, -20);
        Assert.Null(sim.CaptureState().NewChaseDirRandomState);
    }

    [Fact]
    public void SavedJumpEligibilityContinuesMovementIdentically()
    {
        var sim = MovementRoom(64, 128); var actor = sim.Actors.Single(); actor.CanJump = true;
        Walk(actor); var saved = SimSavegame.Write(sim, true);
        Walk(actor); var x = actor.X; var z = actor.Z;
        SimSavegame.Apply(sim, saved); Walk(actor);
        Assert.Equal(x, actor.X); Assert.Equal(z, actor.Z); Assert.True(actor.CanJump);
    }

    private static void Walk(Actor actor)
    {
        actor.ReactionTime = 0; actor.MoveCount = 10; actor.MovementSpeed = Fixed.FromInt(80);
        ActorWanderActions.Wander(actor, ActorWanderFlags.NoRandomTurn | ActorWanderFlags.StopIfBlocked);
    }

    private static AuthoritySimulation MovementRoom(short floor, short ceiling)
    {
        var geometry = GameplayFoundationTests.TwoRooms(floor, ceiling).Level;
        return AuthoritySimulation.Start(new PlayLevel {
            Sectors = geometry.Sectors, Lines = geometry.Lines, Sides = geometry.Sides,
            Things = [new LevelThing { Type = 3004, X = -40 }]
        });
    }

    [Theory]
    [InlineData("CANJUMP")]
    [InlineData("Actor.canjump")]
    public void FlagSupportsSharedScriptAccess(string name)
    {
        var actor = Room().Actors[0];
        Assert.True(AcsActorFlags.TrySet(actor, name, true));
        Assert.True(AcsActorFlags.TryGet(actor, name, out var value)); Assert.True(value);
        Assert.True(AcsActorFlags.TrySet(actor, name, false)); Assert.False(actor.CanJump);
    }

    [Fact]
    public void SavedFlagRestoresChecksumAndOlderSaveClearsIt()
    {
        var sim = Room(); var actor = sim.Actors[0]; var old = SimSavegame.Write(sim, true); var original = sim.Checksum;
        actor.CanJump = true; sim.RestoreState(sim.CaptureState()); var hash = sim.Checksum;
        Assert.NotEqual(original, hash); var saved = SimSavegame.Write(sim, true);
        actor.CanJump = false; SimSavegame.Apply(sim, saved);
        Assert.True(actor.CanJump); Assert.Equal(hash, sim.Checksum);
        SimSavegame.Apply(sim, old); Assert.False(actor.CanJump); Assert.Equal(original, sim.Checksum);
    }

    [Fact]
    public void InvalidFlagRejectsStructuredAndBinaryArchives()
    {
        var sim = Room(); sim.Actors[0].CanJump = true;
        var state = sim.CaptureState(); state.Actors[0].CanJumpFlag = 2;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        var bytes = SimSavegame.Write(sim, true); var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-canjump-state", error);
    }

    [Fact]
    public void RevivalRestoresRepresentedDefault()
    {
        var actor = Room().Actors[0]; actor.CanJump = true;
        ActorRaise.ReviveSupported(actor); Assert.False(actor.CanJump);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Blockmap = new MapBlockmapRecord(new MapBlockmapHeader(0, 0, 8, 8), []),
        Things = [new LevelThing { Type = 3004, X = 64, Y = 64, Friendly = true },
            new LevelThing { Type = 3001, X = 320, Y = 64 }, new LevelThing { Type = 1, X = 500, Y = 64 }]
    });
}
