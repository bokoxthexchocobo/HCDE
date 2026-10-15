using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NoClipTerrainCarryTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void ConveyorSkipsNoclipPlayersAndMonsters(bool player, bool noclip)
    {
        var sim = Room(0.5); Actor actor = player ? sim.Players.Single() : sim.AddBot(128, 0);
        actor.NoClip = noclip; actor.InScrollSector = true; sim.SetSectorScroll(0, 4, -2);
        var x = actor.X.ToDouble(); var y = actor.Y.ToDouble();
        ActorPhysics.ApplySectorScroll(sim, actor);
        Assert.Equal(x + (noclip ? 0 : 4), actor.X.ToDouble());
        Assert.Equal(y - (noclip ? 0 : 2), actor.Y.ToDouble());
    }

    [Theory]
    [InlineData(false, 0.5, 1)]
    [InlineData(true, 0.5, 1)]
    [InlineData(false, 0.98, 1)]
    [InlineData(true, 0.98, 1)]
    [InlineData(false, 0.5, 0.75)]
    [InlineData(true, 0.5, 0.75)]
    [InlineData(false, 0.98, 0.75)]
    [InlineData(true, 0.98, 0.75)]
    public void ThrustFrictionAndVelocityDecayAgreeForNoclip(bool noclip, double terrain, double multiplier)
    {
        var sim = Room(terrain); var player = sim.Players.Single(); player.NoClip = noclip;
        player.Friction = Fixed.FromDouble(multiplier); player.VelocityX = Fixed.FromInt(2);
        var expected = (noclip ? ActorPhysics.GroundFriction : terrain) * multiplier;
        Assert.Equal(expected, ActorPhysics.TerrainMovementFactors(sim, player).Friction, 10);
        ActorPhysics.Step(sim, player);
        Assert.Equal(Fixed.FromDouble(2 * expected), player.VelocityX);
    }

    [Fact]
    public void SavedNoclipConveyorAndFrictionContinueExactly()
    {
        var sim = Room(0.5); var player = sim.Players.Single(); player.NoClip = true;
        player.VelocityX = Fixed.FromInt(2); sim.SetSectorScroll(0, 4, 0);
        var saved = SimSavegame.Write(sim, true); Move(); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); Move(); Assert.Equal(expected, SimSavegame.Write(sim, true));
        void Move() { ActorPhysics.Step(sim, player); ActorPhysics.ApplySectorScroll(sim, player); }
    }

    private static AuthoritySimulation Room(double friction) => AuthoritySimulation.Start(new PlayLevel
    {
        TerrainDefinitions = [new LevelTerrainDefinition("Surface", Friction: friction, MoveFactor: 1024.0 / 65536)],
        DefaultTerrain = "Surface", Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
