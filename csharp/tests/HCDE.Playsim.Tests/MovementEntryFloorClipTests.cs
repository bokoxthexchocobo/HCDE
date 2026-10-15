using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MovementEntryFloorClipTests
{
    [Fact]
    public void DirectMoveUpdatesTerrainClipImmediately()
    {
        var (sim, actor) = Room(); actor.FloorClip = 8;
        Assert.True(ActorPhysics.TryMove(sim, actor, 40, 0, out _));
        Assert.Equal(3, actor.FloorClip); Assert.Equal(1, actor.SectorIndex);
    }

    [Fact]
    public void FailedDirectMovePreservesPositionAndManualClip()
    {
        var (sim, actor) = Room(); actor.FloorClip = 7;
        Assert.False(ActorPhysics.TryMove(sim, actor, 140, 0, out _));
        Assert.Equal(Fixed.FromInt(-40), actor.X); Assert.Equal(7, actor.FloorClip);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DirectMoveHonorsDisabledAndSpecialOverrides(bool special)
    {
        var (sim, actor) = Room(); actor.FloorClipEnabled = special; actor.SpecialFloorClip = special; actor.FloorClip = 7;
        Assert.True(ActorPhysics.TryMove(sim, actor, 40, 0, out _)); Assert.Equal(7, actor.FloorClip);
    }

    [Theory]
    [InlineData(false, 0, 3)]
    [InlineData(true, 32, 0)]
    public void TeleportRefreshUsesFinalFlyingHeight(bool flying, int expectedZ, int clip)
    {
        var (sim, actor) = Room(); actor.NoGravity = flying; actor.Z = Fixed.FromInt(32);
        actor.FloorClip = 7;
        Assert.True(LineSpecials.Execute(sim, actor, LineSpecials.Teleport, 0));
        Assert.Equal(1, actor.SectorIndex); Assert.Equal(expectedZ, actor.Z.ToDouble()); Assert.Equal(clip, actor.FloorClip);
    }

    [Fact]
    public void FailedTeleportPreservesClip()
    {
        var (sim, actor) = Room(); actor.NoTeleport = true; actor.FloorClip = 7;
        Assert.False(LineSpecials.Execute(sim, actor, LineSpecials.Teleport, 0)); Assert.Equal(7, actor.FloorClip);
    }

    [Fact]
    public void SpecialFloorClipSurvivesTeleport()
    {
        var (sim, actor) = Room(); actor.SpecialFloorClip = true; actor.FloorClip = 7;
        Assert.True(LineSpecials.Execute(sim, actor, LineSpecials.Teleport, 0)); Assert.Equal(7, actor.FloorClip);
    }

    [Fact]
    public void SavedTeleportResultRestoresIntoFreshMap()
    {
        var (sim, actor) = Room(); actor.NoGravity = true; actor.Z = Fixed.FromInt(32);
        Assert.True(LineSpecials.Execute(sim, actor, LineSpecials.Teleport, 0));
        sim.RestoreState(sim.CaptureState());
        var saved = SimSavegame.Write(sim, true); var (restored, loaded) = Room(); SimSavegame.Apply(restored, saved);
        Assert.Equal(actor.Z, loaded.Z); Assert.Equal(0, loaded.FloorClip);
        Assert.Equal(sim.Checksum, restored.Checksum);
    }

    private static (AuthoritySimulation, PlayerPawn) Room()
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        var sim = AuthoritySimulation.Start(new PlayLevel {
            TerrainDefinitions = [new LevelTerrainDefinition("Left", FootClip: 8), new LevelTerrainDefinition("Right", FootClip: 3)],
            Sectors = [new LevelSector { CeilingHeight = 128, FloorTerrain = "Left" }, new LevelSector { CeilingHeight = 128, FloorTerrain = "Right" }],
            Sides = geometry.Sides, Lines = geometry.Lines,
            Things = [new LevelThing { Type = 1, X = -40 }, new LevelThing { Type = LineSpecials.TeleportDestType, X = 40 }]
        });
        var actor = sim.Players.Single(); actor.FloorClipEnabled = true; return (sim, actor);
    }
}
