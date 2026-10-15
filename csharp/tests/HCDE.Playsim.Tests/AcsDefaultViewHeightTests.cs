using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class AcsDefaultViewHeightTests
{
    [Theory]
    [InlineData(6)]
    [InlineData(49)]
    public void TransientEyeHeightDoesNotChangeDefaultHeightQuery(double eyeHeight)
    {
        var sim = Room(); var player = sim.Players.Single(); player.ViewHeight = eyeHeight;
        Assert.Equal(41 * 65536, Query(sim, player));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(8)]
    public void CrouchAdjustmentUsesConfiguredDefault(int ticks)
    {
        var sim = Room(); var player = sim.Players.Single(); player.DefaultViewHeight = Fixed.FromInt(48);
        for (var i = 0; i < ticks; i++) { player.Pending = new PlayerCommand { Crouch = true }; sim.Tick(); }
        var expected = (int)Math.Floor(48 * player.CrouchFactor * 65536);
        player.ViewHeight = 3;
        Assert.Equal(expected, Query(sim, player));
    }

    [Fact]
    public void DeathLoweringDoesNotReplaceDefaultViewHeight()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = 0; sim.Tick();
        Assert.True(player.ViewHeight < 41); Assert.Equal(41 * 65536, Query(sim, player));
    }

    [Fact]
    public void TerrainFootClipDoesNotChangeReportedDefaultHeight()
    {
        var sim = Room(); var player = sim.Players.Single(); player.FloorClipEnabled = true;
        sim.Level.TerrainDefinitions = [new LevelTerrainDefinition("Water", FootClip: 8)];
        sim.Level.DefaultTerrain = "Water";
        ActorPropertyActions.AdjustFloorClip(player);
        Assert.Equal(8, player.FloorClip); Assert.Equal(49, player.ViewHeight);
        Assert.Equal(41 * 65536, Query(sim, player));
    }

    [Fact]
    public void SavedDefaultAndCrouchDetermineRestoredQuery()
    {
        var sim = Room(); var player = sim.Players.Single(); player.DefaultViewHeight = Fixed.FromInt(48);
        player.Pending = new PlayerCommand { Crouch = true }; sim.Tick(); var expected = Query(sim, player);
        var saved = SimSavegame.Write(sim, true); player.DefaultViewHeight = Fixed.FromInt(12);
        SimSavegame.Apply(sim, saved); Assert.Equal(expected, Query(sim, player));
    }

    [Fact]
    public void NonplayerRetainsDefaultHalfHeightCameraPolicy()
    {
        var sim = Room(); var actor = sim.AddBot(80, 0); actor.Height = Fixed.FromInt(60);
        Assert.Equal(30 * 65536, Query(sim, actor));
    }

    private static int Query(AuthoritySimulation sim, Actor actor)
    {
        var stack = new List<int> { 77, 0 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = actor }, [], new(),
            AcsCallFunctions.GetActorViewHeight, 1, out var result)); Assert.Equal(new[] { 77 }, stack); return result;
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
