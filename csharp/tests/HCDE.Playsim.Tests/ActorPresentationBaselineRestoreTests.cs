using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorPresentationBaselineRestoreTests
{
    [Theory]
    [InlineData("orientation")]
    [InlineData("bob")]
    [InlineData("scale")]
    public void AbsentPresentationRestoresDefaultsWhileExplicitDefaultsRetainMarkers(string property)
    {
        var sim = Room(); var baseline = sim.AddBot(0, 0); var explicitDefault = sim.AddBot(100, 0);
        Set(explicitDefault, property, false);
        var bytes = SimSavegame.Write(sim);
        Set(baseline, property, true); Set(explicitDefault, property, true);
        SimSavegame.Apply(sim, bytes);
        foreach (var actor in sim.Actors)
        {
            Assert.Equal(0, actor.SpriteAngle); Assert.Equal(0, actor.SpriteRotation);
            Assert.Equal(0, actor.FloatBobPhase);
            Assert.Equal(1, actor.ScaleX); Assert.Equal(1, actor.ScaleY);
        }
        Assert.False(Marker(baseline, property)); Assert.True(Marker(explicitDefault, property));
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void AbsentSpeciesRestoresNativeFamilyImmunityAfterLaterOverride()
    {
        var sim = Room(); var spectre = sim.AddBot(0, 0, 58); var demon = sim.AddBot(100, 0, 3002);
        var bytes = SimSavegame.Write(sim);
        ActorPropertyActions.SetSpecies(spectre, "LaterSpecies");
        Assert.False(spectre.ProjectileImmune(demon));
        SimSavegame.Apply(sim, bytes);
        Assert.Null(spectre.Species); Assert.False(spectre.HasSpeciesOverride);
        Assert.Equal("Demon", spectre.SpeciesName);
        Assert.True(spectre.IsSameSpecies(demon)); Assert.True(spectre.ProjectileImmune(demon));
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    private static void Set(Actor actor, string property, bool changed)
    {
        switch (property)
        {
            case "orientation":
                ActorOrientationActions.SetSpriteAngle(actor, changed ? 90 : 0);
                ActorOrientationActions.SetSpriteRotation(actor, changed ? -180 : 0);
                break;
            case "bob": ActorPropertyActions.SetFloatBobPhase(actor, changed ? 63 : 0); break;
            case "scale": ActorPropertyActions.SetScale(actor, changed ? 2 : 1, changed ? -3 : 1); break;
            default: throw new ArgumentOutOfRangeException(nameof(property));
        }
    }

    private static bool Marker(Actor actor, string property) => property switch
    {
        "orientation" => actor.HasSpriteOrientationOverride,
        "bob" => actor.HasFloatBobPhaseOverride,
        "scale" => actor.HasScaleOverride,
        _ => throw new ArgumentOutOfRangeException(nameof(property)),
    };

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
