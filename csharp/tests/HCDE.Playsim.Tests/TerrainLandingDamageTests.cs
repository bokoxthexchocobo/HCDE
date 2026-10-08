using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TerrainLandingDamageTests
{
    [Theory]
    [InlineData(true, true, 3, 93)]
    [InlineData(false, true, 3, 100)]
    [InlineData(true, false, 3, 100)]
    [InlineData(true, true, 0, 100)]
    public void LandingOnlyDamageUsesPlayerSplashFlagAndBitMaskGate(bool splash, bool landing, int mask, int expected)
    {
        var sim = Room(splash, landing, mask); var player = sim.Players.Single();
        player.Z = Fixed.FromInt(32); player.NoGravity = true; sim.Tick();
        // Disable periodic damage to isolate the native impact path's separate policy.
        player.NoSectorDamage = true; player.Z = Fixed.FromInt(8); player.OnGround = false; player.VelocityZ = Fixed.FromInt(-16);
        sim.Tick(); Assert.Equal(expected, player.Health);
        sim.Tick(); Assert.Equal(expected, player.Health);
    }

    [Fact]
    public void ScheduledTickDoesNotDoubleImpactDamage()
    {
        var sim = Room(true, true, 3); var player = sim.Players.Single();
        player.Z = Fixed.FromInt(8); player.OnGround = false; player.VelocityZ = Fixed.FromInt(-16);
        sim.Tick(); Assert.Equal(93, player.Health);
    }

    [Fact]
    public void SavedAirbornePoseResumesLandingDamage()
    {
        var sim = Room(true, true, 3); var player = sim.Players.Single();
        player.Z = Fixed.FromInt(32); player.NoGravity = true; sim.Tick();
        player.Z = Fixed.FromInt(8); player.OnGround = false; player.VelocityZ = Fixed.FromInt(-16);
        var saved = SimSavegame.Write(sim); var restored = Room(true, true, 3); SimSavegame.Apply(restored, saved);
        sim.Tick(); restored.Tick(); Assert.Equal(93, player.Health);
        Assert.Equal(sim.Checksum, restored.Checksum); Assert.Equal(SimSavegame.Write(sim), SimSavegame.Write(restored));
    }

    private static AuthoritySimulation Room(bool splash, bool landing, int mask)
    {
        var text = (splash ? "splash S { } " : "") + $"terrain Hot {{ damagetype Fire damageamount 7 damagetimemask {mask} splash S "
            + (landing ? "damageonland " : "") + "} defaultterrain Hot";
        Assert.True(TerrainDefinitionParser.TryParseData(text, out var data, out var error), error);
        return AuthoritySimulation.Start(new PlayLevel
        {
            TerrainDefinitions = data.Definitions, TerrainSplashes = data.Splashes ?? Array.Empty<string>(), DefaultTerrain = data.DefaultTerrain,
            Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }],
        }, rngSeed: 42);
    }
}
