using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorFloorClipActionTests
{
    [Theory]
    [InlineData(0, false, 8)]
    [InlineData(1, false, 0)]
    [InlineData(0, true, 5)]
    [InlineData(1, true, 5)]
    public void SetUsesTerrainUnlessSpecialAndSurvivesFreshSave(int height, bool special, double expected)
    {
        var sim = Room(); var player = sim.Players.Single();
        player.Z = Fixed.FromInt(height); player.SpecialFloorClip = special; player.FloorClip = 5;
        var view = player.ViewHeight; var defaultClip = player.DefaultFloorClip;
        ActorPropertyActions.SetFloorClip(player);
        Assert.True(player.FloorClipEnabled); Assert.Equal(expected, player.FloorClip);
        Assert.Equal(view + (special ? 0 : expected - 5), player.ViewHeight);
        Assert.Equal(defaultClip, player.DefaultFloorClip); Assert.Equal(special, player.SpecialFloorClip);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        Assert.True(fresh.Players.Single().FloorClipEnabled);
        Assert.Equal(expected, fresh.Players.Single().FloorClip);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UnsetClearsEvenSpecialClipWithoutChangingViewOrDefaults(bool special)
    {
        var sim = Room(); var player = sim.Players.Single();
        player.SpecialFloorClip = special; player.FloorClipEnabled = true; player.FloorClip = 5;
        var view = player.ViewHeight; var defaultClip = player.DefaultFloorClip;
        ActorPropertyActions.UnsetFloorClip(player);
        Assert.False(player.FloorClipEnabled); Assert.Equal(0, player.FloorClip);
        Assert.Equal(view, player.ViewHeight); Assert.Equal(defaultClip, player.DefaultFloorClip);
        Assert.Equal(special, player.SpecialFloorClip);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        Assert.False(fresh.Players.Single().FloorClipEnabled); Assert.Equal(0, fresh.Players.Single().FloorClip);
    }

    [Fact]
    public void ActionsExecuteThroughActorStates()
    {
        var sim = Room(); var actor = sim.Players.Single();
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1, ActorPropertyActions.SetFloorClip),
            new(-1, 2, ActorPropertyActions.UnsetFloorClip)], 0);
        actor.States.Enter(actor, 1); Assert.True(actor.FloorClipEnabled); Assert.Equal(8, actor.FloorClip);
        actor.States.Enter(actor, 2); Assert.False(actor.FloorClipEnabled); Assert.Equal(0, actor.FloorClip);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        TerrainDefinitions = [new LevelTerrainDefinition("Water", FootClip: 8)], DefaultTerrain = "Water",
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
    });
}
