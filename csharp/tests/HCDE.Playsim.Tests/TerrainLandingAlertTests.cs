using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TerrainLandingAlertTests
{
    [Theory]
    [InlineData(false, false, -7, true)]
    [InlineData(true, false, -7, false)]
    [InlineData(false, true, -7, false)]
    [InlineData(false, false, -6, false)]
    public void LandingNoiseHonorsSplashAndListenerPolicies(bool noAlert, bool noSplashAlert, int velocity, bool heard)
    {
        var sim = Create(noAlert);
        var player = sim.Players.Single();
        var listener = sim.Actors.Single(actor => actor.IsMonster);
        listener.NoSplashAlert = noSplashAlert;
        player.VelocityZ = Fixed.FromInt(velocity);
        TerrainDamage.Land(sim, player);
        Assert.Equal(heard ? player.Id : (uint?)null, listener.LastHeardTargetId);
    }

    [Fact]
    public void NoAlertChangesChecksumAndSurvivesLevelCopyAndSaveContinuation()
    {
        var baseline = Create(false);
        var sim = Create(true);
        Assert.NotEqual(baseline.Checksum, sim.Checksum);
        Assert.True(Assert.Single(sim.Level.TerrainSplashDefinitions).NoAlert);
        var saved = SimSavegame.Write(sim);
        var restored = Create(true);
        SimSavegame.Apply(restored, saved);
        sim.Players.Single().VelocityZ = Fixed.FromInt(-8);
        restored.Players.Single().VelocityZ = Fixed.FromInt(-8);
        TerrainDamage.Land(sim, sim.Players.Single());
        TerrainDamage.Land(restored, restored.Players.Single());
        Assert.Equal(sim.Checksum, restored.Checksum);
        Assert.Null(restored.Actors.Single(actor => actor.IsMonster).LastHeardTargetId);
    }

    private static AuthoritySimulation Create(bool noAlert)
    {
        var flag = noAlert ? "noalert" : "";
        Assert.True(TerrainDefinitionParser.TryParseData($"splash S {{ {flag} }} terrain Water {{ splash S }} defaultterrain Water",
            out var data, out var error), error);
        return AuthoritySimulation.Start(new PlayLevel
        {
            TerrainDefinitions = data.Definitions, DefaultTerrain = data.DefaultTerrain,
            TerrainSplashes = data.Splashes!, TerrainSplashDefinitions = data.SplashDefinitions!,
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, X = 100 }],
        });
    }
}
