using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorDirectSizeSaveTests
{
    [Theory]
    [InlineData(3001, false)]
    [InlineData(3001, true)]
    [InlineData(3005, false)]
    [InlineData(3005, true)]
    public void AbsentSizeRestoresRecordedClassDimensions(int type, bool height)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, type);
        var radius = actor.Radius; var originalHeight = actor.Height;
        var bytes = SimSavegame.Write(sim);
        Assert.True(ActorPropertyActions.SetSize(actor, height ? -1 : 12, height ? 30 : -1));
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(radius, actor.Radius); Assert.Equal(originalHeight, actor.Height);
        Assert.False(actor.HasSizeOverride);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void BaselineRestoreRemovesLaterRadiusCollision()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); sim.AddBot(50, 0);
        var bytes = SimSavegame.Write(sim);
        Assert.True(ActorPropertyActions.SetSize(actor, radius: 40));
        Assert.False(ActorPropertyActions.SetSize(actor, height: actor.Height.ToDouble(), testPosition: true));
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(bytes, SimSavegame.Write(sim));
        Assert.True(ActorPropertyActions.SetSize(actor, height: actor.Height.ToDouble(), testPosition: true));
    }

    [Fact]
    public void PlayerBaselineRestoresRadiusAndStandingHeightTogether()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
        var player = Assert.Single(sim.Players); var radius = player.Radius; var height = player.Height;
        var fullHeight = player.FullHeight; var bytes = SimSavegame.Write(sim);
        Assert.True(ActorPropertyActions.SetSize(player, 40, 30));
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(radius, player.Radius); Assert.Equal(height, player.Height);
        Assert.Equal(fullHeight, player.FullHeight); Assert.Equal(1, player.CrouchFactor);
        Assert.False(player.HasSizeOverride); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(3001, false)]
    [InlineData(3001, true)]
    [InlineData(3005, false)]
    [InlineData(3005, true)]
    public void DirectDimensionsDifferingFromSpawnDefaultsRoundTrip(int type, bool height)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, type);
        var radius = actor.Radius; var originalHeight = actor.Height;
        if (height) actor.Height = Fixed.FromInt(30); else actor.Radius = Fixed.FromInt(12);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.Radius = radius; actor.Height = originalHeight; sim.RestoreState(state);
        Assert.Equal(height ? radius : Fixed.FromInt(12), actor.Radius);
        Assert.Equal(height ? Fixed.FromInt(30) : originalHeight, actor.Height);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void UnchangedDefaultsDoNotAddSizeRecords()
    {
        var sim = Room(); sim.AddBot(0, 0, 3001); sim.AddBot(100, 0, 3005);
        Assert.All(sim.CaptureState().Actors, pose => Assert.Null(pose.Size));
    }

    [Fact]
    public void AcsDimensionsRemainReadOnly()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var radius = actor.Radius; var height = actor.Height;
        AcsActorProperties.Set(sim, actor, 0, AcsActorProperties.Radius, 0);
        AcsActorProperties.Set(sim, actor, 0, AcsActorProperties.Height, 0);
        Assert.Equal(radius.Raw, AcsActorProperties.Get(sim, actor, 0, AcsActorProperties.Radius));
        Assert.Equal(height.Raw, AcsActorProperties.Get(sim, actor, 0, AcsActorProperties.Height));
        Assert.Null(sim.CaptureState().Actors.Single().Size);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
