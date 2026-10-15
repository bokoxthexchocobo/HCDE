using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TerrainLandingSoundTests
{
    [Theory]
    [InlineData(100, -7, "water/normal")]
    [InlineData(9, -7, "water/small")]
    [InlineData(10, -7, "water/normal")]
    [InlineData(100, -6, "")]
    [InlineData(100, -1, "")]
    public void LandingSelectsSoundByMassAndSuppressesSlowLivingImpacts(int mass, int velocity, string expected)
    {
        var sim = Create();
        var player = sim.Players.Single();
        player.Mass = mass;
        player.VelocityZ = Fixed.FromInt(velocity);
        TerrainDamage.Land(sim, player);
        if (expected.Length == 0) { Assert.Empty(sim.PendingSoundRequests); return; }
        var sound = Assert.Single(sim.PendingSoundRequests);
        Assert.Equal(expected, sound.Sound);
        Assert.Equal(0u, sound.ActorId);
        Assert.Equal(3, sound.Channel);
        Assert.Equal(1.001f, sound.Attenuation);
        Assert.Equal(new SimulationSoundOrigin(0, 0, 0, 0), sound.DetachedOrigin);
        Assert.Equal(100, player.Health);
    }

    [Fact]
    public void ActualFloorImpactQueuesSplashWithoutPeriodicDamage()
    {
        var sim = Create();
        var player = sim.Players.Single();
        player.Z = Fixed.FromInt(8); player.OnGround = false; player.VelocityZ = Fixed.FromInt(-16);
        sim.Tick();
        Assert.Contains(sim.PendingSoundRequests, request => request.Sound == "water/normal" && request.Channel == 3);
    }

    private static AuthoritySimulation Create()
    {
        Assert.True(TerrainDefinitionParser.TryParseData("splash S { sound water/normal smallsound water/small } terrain Water { splash S } defaultterrain Water",
            out var data, out var error), error);
        return AuthoritySimulation.Start(new PlayLevel
        {
            TerrainDefinitions = data.Definitions, DefaultTerrain = data.DefaultTerrain,
            TerrainSplashes = data.Splashes!, TerrainSplashDefinitions = data.SplashDefinitions!,
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }],
        });
    }
}
