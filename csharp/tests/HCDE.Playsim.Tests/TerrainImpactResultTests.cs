using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TerrainImpactResultTests
{
    [Theory]
    [InlineData(true, false, false, -8)]
    [InlineData(false, false, false, -8)]
    [InlineData(true, true, true, -8)]
    [InlineData(false, true, true, -8)]
    [InlineData(true, true, false, -6)]
    [InlineData(true, true, false, -8)]
    public void ImpactResultReportsLiquidIndependentlyOfEffects(bool liquid, bool splash, bool suppress, int velocity)
    {
        var sim = Room(liquid, splash);
        var player = sim.Players.Single(); player.DontSplash = suppress; player.VelocityZ = Fixed.FromInt(velocity);
        Assert.Equal(liquid, TerrainDamage.Land(sim, player));
        Assert.Equal(splash && !suppress && velocity < -6, sim.PendingSoundRequests.Any(request => request.Channel == 3));
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void DeadPlayerCanSplashUnlessFlagged(bool dontSplash, bool effects)
    {
        var sim = Room(true, true);
        var player = sim.Players.Single();
        ActorDamage.Apply(player, 10000);
        Assert.True(player.IsDead);
        sim.DrainSoundRequests();
        player.DontSplash = dontSplash; player.VelocityZ = Fixed.FromInt(-8);
        Assert.True(TerrainDamage.Land(sim, player));
        Assert.Equal(effects, sim.PendingSoundRequests.Any(request => request.Channel == 3));
    }

    [Fact]
    public void SavedDeadAirbornePlayerSplashesOnActualImpact()
    {
        var sim = Room(true, true); var player = sim.Players.Single();
        ActorDamage.Apply(player, 10000); sim.DrainSoundRequests();
        player.Z = Fixed.FromInt(8); player.OnGround = false; player.VelocityZ = Fixed.FromInt(-16);
        var saved = SimSavegame.Write(sim, true);
        var restored = Room(true, true); SimSavegame.Apply(restored, saved);
        sim.Tick(); restored.Tick();
        Assert.Contains(sim.PendingSoundRequests, request => request.Channel == 3 && request.Sound == "water/splash");
        Assert.Equal(sim.PendingSoundRequests, restored.PendingSoundRequests);
        Assert.Equal(player.RespawnEarliestTic, restored.Players.Single().RespawnEarliestTic);
        Assert.Equal(sim.Checksum, restored.Checksum);
    }

    private static AuthoritySimulation Room(bool liquid, bool splash) => AuthoritySimulation.Start(new PlayLevel
    {
        TerrainDefinitions = [new("Surface", Splash: splash ? "S" : "", IsLiquid: liquid)],
        DefaultTerrain = "Surface", TerrainSplashes = splash ? ["S"] : [],
        TerrainSplashDefinitions = splash ? [new("S", "water/splash")] : [],
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }],
    });
}
