using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PhysicsFloorClipTests
{
    [Theory]
    [InlineData(4, -8, 0, 8)]
    [InlineData(8, 1, 9, 0)]
    public void VerticalPhysicsRefreshesClipAtFinalHeight(int z, int velocity, int finalZ, int clip)
    {
        var (sim, actor) = Room(); actor.Z = Fixed.FromInt(z); actor.VelocityZ = Fixed.FromInt(velocity);
        actor.NoGravity = true; actor.OnGround = false; actor.FloorClip = 6;
        ActorPhysics.Step(sim, actor);
        Assert.Equal(Fixed.FromInt(finalZ), actor.Z); Assert.Equal(clip, actor.FloorClip);
    }

    [Fact]
    public void HorizontalPhysicsUpdatesClipAfterTerrainTransition()
    {
        var (sim, actor) = Room(); actor.X = Fixed.FromInt(-24); actor.VelocityX = Fixed.FromInt(30);
        actor.FloorClip = 8; ActorPhysics.Step(sim, actor);
        Assert.Equal(Fixed.FromInt(6), actor.X); Assert.Equal(3, actor.FloorClip);
    }

    [Fact]
    public void PlacementUsesSuppliedSimulationWithoutActorBinding()
    {
        var (sim, _) = Room(); var actor = new Actor { X = Fixed.FromInt(-40), Z = Fixed.FromInt(30), FloorClipEnabled = true };
        ActorPhysics.PlaceOnFloor(sim, actor);
        Assert.Equal(default, actor.Z); Assert.Equal(8, actor.FloorClip);
    }

    [Fact]
    public void SectorFittingRefreshesFloorContactAfterMovingPlane()
    {
        var (sim, actor) = Room(); sim.Floors[0] = 16;
        ActorPhysics.FitToSector(sim, actor); Assert.Equal(Fixed.FromInt(16), actor.Z); Assert.Equal(8, actor.FloorClip);
        sim.Floors[0] = 0; ActorPhysics.FitToSector(sim, actor, carryFloor: false);
        Assert.Equal(Fixed.FromInt(16), actor.Z); Assert.Equal(0, actor.FloorClip);
    }

    [Fact]
    public void ActorTopSupportClearsTerrainClipping()
    {
        var (sim, actor) = Room(); var carrier = sim.AddBot(-40, 0); carrier.Height = Fixed.FromInt(8);
        actor.CanPass = true; actor.Z = Fixed.FromInt(8); actor.FloorClip = 8;
        ActorPhysics.FitToSector(sim, actor, carryFloor: false);
        Assert.Equal(Fixed.FromInt(8), actor.Z); Assert.Equal(0, actor.FloorClip);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DisabledAndSpecialFlagsPreserveManualClip(bool special)
    {
        var (sim, actor) = Room(); actor.FloorClipEnabled = special; actor.SpecialFloorClip = special;
        actor.FloorClip = 7; actor.Z = Fixed.FromInt(8); actor.NoGravity = true;
        ActorPhysics.Step(sim, actor); Assert.Equal(7, actor.FloorClip);
    }

    [Fact]
    public void SaveAcrossLandingReplaysPhysicsAndClip()
    {
        var (sim, actor) = Room(); actor.Z = Fixed.FromInt(4); actor.VelocityZ = Fixed.FromInt(-8);
        actor.OnGround = false; var saved = SimSavegame.Write(sim, true);
        ActorPhysics.Step(sim, actor); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); ActorPhysics.Step(sim, actor);
        Assert.Equal(8, actor.FloorClip); Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void ChargingPhysicsAlsoClearsClipAboveTerrain()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            TerrainDefinitions = [new LevelTerrainDefinition("Water", FootClip: 8)], DefaultTerrain = "Water",
            Sectors = [new LevelSector { CeilingHeight = 128 }]
        });
        var actor = sim.AddBot(0, 0, 3006); var target = sim.AddBot(1000, 0);
        actor.Brain!.StartCharge(actor, target); actor.FloorClipEnabled = true; actor.FloorClip = 8;
        actor.Z = Fixed.FromInt(8); actor.VelocityX = actor.VelocityY = default; actor.VelocityZ = Fixed.FromInt(1);
        ActorPhysics.Step(sim, actor); Assert.Equal(0, actor.FloorClip);
    }

    private static (AuthoritySimulation, Actor) Room()
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        var sim = AuthoritySimulation.Start(new PlayLevel {
            TerrainDefinitions = [new LevelTerrainDefinition("Left", FootClip: 8), new LevelTerrainDefinition("Right", FootClip: 3)],
            Sectors = [new LevelSector { CeilingHeight = 128, FloorTerrain = "Left" }, new LevelSector { CeilingHeight = 128, FloorTerrain = "Right" }],
            Sides = geometry.Sides, Lines = geometry.Lines, Things = [new LevelThing { Type = 3004, X = -40 }]
        });
        var actor = sim.Actors.Single(); actor.FloorClipEnabled = true; return (sim, actor);
    }
}
