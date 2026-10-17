using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DontSplashTests
{
    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void FlagSuppressesImpactSoundAndAlertButNotLandingDamage(bool dontSplash, bool effects)
    {
        var sim = Room();
        sim.Tick(); sim.DrainSoundRequests();
        var player = sim.Players.Single();
        var listener = sim.Actors.Single(actor => actor.IsMonster);
        listener.LastHeardTargetId = null;
        var health = player.Health;
        player.DontSplash = dontSplash;
        player.VelocityZ = Fixed.FromInt(-16);
        TerrainDamage.Land(sim, player);
        Assert.Equal(health - 7, player.Health);
        Assert.Equal(effects, sim.PendingSoundRequests.Any(request => request.Channel == 3));
        Assert.Equal(effects ? player.Id : (uint?)null, listener.LastHeardTargetId);
    }

    [Fact]
    public void AcsFlagSupportsQualifiedCaseInsensitiveNames()
    {
        var player = Room().Players.Single();
        Assert.True(AcsActorFlags.TrySet(player, "actor.dontsplash", true));
        Assert.True(AcsActorFlags.TryGet(player, "DONTSPLASH", out var value));
        Assert.True(value);
        Assert.True(AcsActorFlags.TrySet(player, "DONTSPLASH", false));
        Assert.False(player.DontSplash);
    }

    [Fact]
    public void SaveRoundTripRestoresFlagAndChecksumAndOldSaveClearsIt()
    {
        var sim = Room(); var player = sim.Players.Single();
        sim.RestoreState(sim.CaptureState());
        var older = SimSavegame.Write(sim, true); var original = sim.Checksum;
        player.DontSplash = true;
        sim.RestoreState(sim.CaptureState());
        Assert.NotEqual(original, sim.Checksum);
        var saved = SimSavegame.Write(sim, true); var checksum = sim.Checksum;
        Assert.Equal(302, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var restored = Room(); SimSavegame.Apply(restored, saved);
        Assert.True(restored.Players.Single().DontSplash);
        Assert.Equal(checksum, restored.Checksum);
        Assert.Equal(saved, SimSavegame.Write(restored, true));
        SimSavegame.Apply(restored, older);
        Assert.False(restored.Players.Single().DontSplash);
        Assert.Equal(original, restored.Checksum);
    }

    [Fact]
    public void InvalidBinaryFlagIsRejected()
    {
        var sim = Room(); sim.Players.Single().DontSplash = true;
        var saved = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 8), 2);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
    }

    [Fact]
    public void FlagDoesNotSuppressPeriodicTerrainDamageSound()
    {
        var sim = Room(); sim.Tick(); sim.DrainSoundRequests();
        var player = sim.Players.Single(); player.DontSplash = true;
        sim.Level.TerrainDefinitions = [sim.Level.TerrainDefinitions[0] with { DamageTimeMask = 0 }];
        TerrainDamage.Tick(sim, player);
        Assert.Contains(sim.PendingSoundRequests, request => request.Channel == 0 && request.Sound == "water/splash");
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        TerrainDefinitions = [new("Water", DamageAmount: 7, DamageTimeMask: 31, DamageOnLand: true, Splash: "S")],
        TerrainSplashes = ["S"], TerrainSplashDefinitions = [new("S", "water/splash")], DefaultTerrain = "Water",
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, X = 100 }],
    });
}
