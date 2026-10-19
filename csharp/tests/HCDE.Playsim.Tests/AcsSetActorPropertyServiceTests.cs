using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsSetActorPropertyServiceTests
{
    [Fact]
    public void SetsSpeedAndAmbushFlags()
    {
        var level = new PlayLevel { MapName = "MAP01", Things = [new LevelThing { Type = 3001, X = 0, Y = 0 }] };
        var sim = AuthoritySimulation.Start(level);
        var actor = sim.Actors.First();
        Assert.True(AcsSetActorPropertyService.TryApply(
            actor,
            AcsSetActorPropertyService.AcsActorProperty.Speed,
            65536,
            out _));
        Assert.Equal(1, actor.MovementSpeed.ToDouble());
        Assert.True(AcsSetActorPropertyService.TryApply(
            actor,
            AcsSetActorPropertyService.AcsActorProperty.Ambush,
            1,
            out _));
        Assert.True(actor.Ambush);
    }

    [Fact]
    public void SetsMassScoreAndStencilColor()
    {
        var level = new PlayLevel { MapName = "MAP01", Things = [new LevelThing { Type = 3001, X = 0, Y = 0 }] };
        var sim = AuthoritySimulation.Start(level);
        var actor = sim.Actors.First();
        Assert.True(AcsSetActorPropertyService.TryApply(
            actor, AcsSetActorPropertyService.AcsActorProperty.Mass, 200, out _));
        Assert.True(AcsSetActorPropertyService.TryApply(
            actor, AcsSetActorPropertyService.AcsActorProperty.Score, 5, out _));
        Assert.True(AcsSetActorPropertyService.TryApply(
            actor, AcsSetActorPropertyService.AcsActorProperty.StencilColor, 0x00ff0000, out _));
        Assert.Equal(200, actor.Mass);
        Assert.Equal(5, actor.Score);
        Assert.Equal(0x00ff0000u, actor.StencilFillColor);
    }
}
