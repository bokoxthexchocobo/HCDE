using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PatrolMarkerTests
{
    [Theory]
    [InlineData(PatrolMarkers.PointType)]
    [InlineData(PatrolMarkers.SpecialType)]
    public void NativeMarkerDefaultsAreApplied(int type)
    {
        var sim = Room(type); var marker = Marker(sim, type);
        Assert.Equal(8, marker.Radius.ToDouble()); Assert.Equal(8, marker.Height.ToDouble());
        Assert.Equal(10, marker.Mass); Assert.Equal(1000, marker.Health);
        Assert.True(marker.NoGravity); Assert.True(marker.NoBlockmap);
        Assert.False(marker.Solid); Assert.False(marker.Shootable); Assert.False(marker.IsMonster);
        Assert.Null(marker.Brain); Assert.False(marker.CanTakeDamage); Assert.False(marker.IsBlockmapActor);
        Assert.Equal(ActorRenderStyle.FromNative(0), marker.RenderStyle);
        Assert.Equal(marker.RenderStyle, marker.DefaultRenderStyle);
    }

    [Theory]
    [InlineData(PatrolMarkers.PointType)]
    [InlineData(PatrolMarkers.SpecialType)]
    public void MarkerDoesNotBlockMovementOrOrdinaryAim(int type)
    {
        var sim = Room(type); var player = sim.Players.Single();
        Assert.True(ActorPhysics.TryMove(sim, player, 100, 0, out _));
        player.X = Fixed.FromInt(0);
        Assert.Null(CombatTrace.FindTarget(sim, player, 1024));
    }

    [Theory]
    [InlineData(PatrolMarkers.PointType)]
    [InlineData(PatrolMarkers.SpecialType)]
    public void IdentityReconstructionPreservesMarkerAndGoalPointer(int type)
    {
        var sim = Room(type); var marker = Marker(sim, type); var hunter = sim.AddBot(-200, 0);
        hunter.GoalId = marker.Id; marker.SpecialArgs[0] = 42; marker.SpecialArgs[1] = 3;
        var id = marker.Id; var saved = SimSavegame.Write(sim, true);
        marker.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        marker = Marker(sim, type); Assert.Equal(id, marker.Id); Assert.Equal(id, hunter.GoalId);
        Assert.Equal(42, marker.SpecialArgs[0]); Assert.Equal(3, marker.SpecialArgs[1]);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Theory]
    [InlineData(PatrolMarkers.PointType)]
    [InlineData(PatrolMarkers.SpecialType)]
    public void MapOverridesStillApplyToMarkerDefaults(int type)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = type, Alpha = 0.25, RenderStyle = 6, ScaleX = 2, Health = -42 }]
        });
        var marker = Marker(sim, type); Assert.Equal(42, marker.Health); Assert.Equal(2, marker.ScaleX);
        Assert.Equal(0.25, marker.Alpha); Assert.Equal(ActorRenderStyle.Translucent, marker.RenderStyle);
        marker.RestoreRenderStyle(); Assert.Equal(ActorRenderStyle.FromNative(0), marker.RenderStyle);
    }

    private static Actor Marker(AuthoritySimulation sim, int type) => sim.Actors.Single(a => a.DoomEdNum == type && !a.Destroyed);
    private static AuthoritySimulation Room(int type) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = type, Id = 7, X = 100 }]
    });
}
