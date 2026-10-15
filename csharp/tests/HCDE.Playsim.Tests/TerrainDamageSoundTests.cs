using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TerrainDamageSoundTests
{
    [Theory]
    [InlineData(false, false, 93, 1)]
    [InlineData(true, false, 100, 0)]
    [InlineData(false, true, 100, 0)]
    public void PeriodicSoundRequiresActualUnprotectedDamage(bool protection, bool invulnerable, int health, int sounds)
    {
        var sim = Create();
        var player = sim.Players.Single();
        if (protection) player.GivePowerIronFeet();
        player.Invulnerable = invulnerable;
        TerrainDamage.Tick(sim, player);
        Assert.Equal(health, player.Health);
        var requests = sim.PendingSoundRequests.Where(request => request.Sound == "terrain/hot").ToArray();
        Assert.Equal(sounds, requests.Length);
        if (sounds == 0) return;
        var sound = Assert.Single(requests);
        Assert.Equal(player.Id, sound.ActorId);
        Assert.Equal(0, sound.Channel);
        Assert.Equal(1, sound.Volume);
        Assert.Equal(1.001f, sound.Attenuation);
    }

    [Fact]
    public void SplashSoundConfigurationChangesChecksumAndSurvivesFreshSaveContinuation()
    {
        var sim = Create();
        var different = Create("terrain/other");
        Assert.NotEqual(sim.Checksum, different.Checksum);
        var saved = SimSavegame.Write(sim);
        var restored = Create();
        SimSavegame.Apply(restored, saved);
        sim.Tick(); restored.Tick();
        Assert.Equal(sim.Checksum, restored.Checksum);
        Assert.Equal(sim.PendingSoundRequests, restored.PendingSoundRequests);
        Assert.Contains(sim.PendingSoundRequests, request => request.Sound == "terrain/hot");
    }

    private static AuthoritySimulation Create(string sound = "terrain/hot")
    {
        Assert.True(TerrainDefinitionParser.TryParseData($"splash S {{ sound {sound} }} terrain Hot {{ splash S damageamount 7 damagetimemask 0 allowprotection }} defaultterrain Hot",
            out var data, out var error), error);
        return AuthoritySimulation.Start(new PlayLevel
        {
            TerrainDefinitions = data.Definitions, DefaultTerrain = data.DefaultTerrain,
            TerrainSplashes = data.Splashes!, TerrainSplashDefinitions = data.SplashDefinitions!,
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }],
        });
    }
}
