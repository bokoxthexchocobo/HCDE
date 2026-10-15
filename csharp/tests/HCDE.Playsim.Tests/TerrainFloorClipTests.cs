using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TerrainFloorClipTests
{
    [Theory]
    [InlineData(8, 3, 3)]
    [InlineData(8, 0, 0)]
    [InlineData(-2, 3, -2)]
    public void GroundContactUsesShallowestTerrain(double left, double right, double expected)
    {
        var (sim, actor) = Room(left, right); actor.X = Fixed.FromInt(-8);
        ActorPropertyActions.AdjustFloorClip(actor); Assert.Equal(expected, actor.FloorClip);
    }

    [Fact]
    public void AirborneActorClearsClipAndSpecialFloorClipPreservesIt()
    {
        var (_, actor) = Room(); actor.Z = Fixed.FromInt(8); actor.FloorClip = 7;
        actor.SpecialFloorClip = true; ActorPropertyActions.AdjustFloorClip(actor); Assert.Equal(7, actor.FloorClip);
        actor.SpecialFloorClip = false; ActorPropertyActions.AdjustFloorClip(actor); Assert.Equal(0, actor.FloorClip);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 3)]
    public void SmartMovementUpdatesClipOnlyWhenEnabled(bool enabled, double expected)
    {
        var (_, actor) = Room(); actor.FloorClipEnabled = enabled;
        actor.ReactionTime = 0; actor.MoveCount = 100; actor.MovementSpeed = Fixed.FromInt(80);
        ActorWanderActions.Wander(actor, ActorWanderFlags.NoRandomTurn | ActorWanderFlags.StopIfBlocked);
        Assert.Equal(Fixed.FromInt(40), actor.X); Assert.Equal(expected, actor.FloorClip);
    }

    [Theory]
    [InlineData("FLOORCLIP")]
    [InlineData("Actor.SPECIALFLOORCLIP")]
    public void SharedFlagAccessAndRevivalRestoreDefaults(string name)
    {
        var (_, actor) = Room(); Assert.True(AcsActorFlags.TrySet(actor, name, true));
        Assert.True(AcsActorFlags.TryGet(actor, name, out var value)); Assert.True(value);
        ActorRaise.ReviveSupported(actor); Assert.False(actor.FloorClipEnabled || actor.SpecialFloorClip);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void FlagsAndDepthRoundTripWithChecksumAndLegacyReset(int flags)
    {
        var (sim, actor) = Room(); var old = SimSavegame.Write(sim, true);
        actor.FloorClipEnabled = (flags & 1) != 0; actor.SpecialFloorClip = (flags & 2) != 0; actor.FloorClip = 7;
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum; var saved = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, old); Assert.False(actor.FloorClipEnabled || actor.SpecialFloorClip); Assert.Equal(0, actor.FloorClip);
        SimSavegame.Apply(sim, saved); Assert.Equal(checksum, sim.Checksum); Assert.Equal(7, actor.FloorClip);
        Assert.Equal(flags, sim.CaptureState().Actors.Single().FloorClipFlags);
    }

    [Fact]
    public void InvalidFlagsRejectStructuredAndBinaryState()
    {
        var (sim, actor) = Room(); actor.FloorClipEnabled = true;
        var state = sim.CaptureState(); state.Actors[0].FloorClipFlags = 4;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        var bytes = SimSavegame.Write(sim, true); var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), 4);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-floorclipflags-state", error);
    }

    [Fact]
    public void TerrainDepthParticipatesInSimulationChecksum()
        => Assert.NotEqual(Room(8).Item1.Checksum, Room(9).Item1.Checksum);

    [Fact]
    public void PlayerViewHeightTracksClipDeltaAndSavedContinuation()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            TerrainDefinitions = [new LevelTerrainDefinition("Water", FootClip: 8)], DefaultTerrain = "Water",
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
        });
        var player = sim.Players.Single(); var initial = player.ViewHeight;
        ActorPropertyActions.AdjustFloorClip(player); Assert.Equal(initial + 8, player.ViewHeight);
        ActorPropertyActions.AdjustFloorClip(player); Assert.Equal(initial + 8, player.ViewHeight);
        var saved = SimSavegame.Write(sim, true); player.Z = Fixed.FromInt(1);
        ActorPropertyActions.AdjustFloorClip(player); Assert.Equal(initial, player.ViewHeight);
        SimSavegame.Apply(sim, saved); Assert.Equal(initial + 8, player.ViewHeight); Assert.Equal(8, player.FloorClip);
    }

    private static (AuthoritySimulation, Actor) Room(double left = 8, double right = 3)
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        var sim = AuthoritySimulation.Start(new PlayLevel {
            TerrainDefinitions = [new LevelTerrainDefinition("Left", FootClip: left), new LevelTerrainDefinition("Right", FootClip: right)],
            Sectors = [new LevelSector { CeilingHeight = 128, FloorTerrain = "Left" },
                new LevelSector { CeilingHeight = 128, FloorTerrain = "Right" }],
            Lines = geometry.Lines, Sides = geometry.Sides, Things = [new LevelThing { Type = 3004, X = -40 }]
        });
        return (sim, sim.Actors.Single());
    }
}
