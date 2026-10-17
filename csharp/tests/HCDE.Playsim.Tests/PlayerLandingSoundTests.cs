using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerLandingSoundTests
{
    [Theory]
    [InlineData(-12, 12, false)]
    [InlineData(-13, 12, true)]
    [InlineData(-10, 9.5, true)]
    public void FloorSoundUsesStrictGruntSpeedAndNativeChannels(int velocity, double threshold, bool grunts)
    {
        var sim = Room(); var player = sim.Players.Single(); player.GruntSpeed = threshold;
        Drop(player, velocity); ActorPhysics.Step(sim, player);
        var requests = sim.DrainSoundRequests();
        Assert.Equal(grunts ? 2 : 1, requests.Length);
        Assert.Contains(requests, sound => sound.Sound == "*land" && sound.Channel == 0);
        Assert.Equal(grunts, requests.Any(sound => sound.Sound == "*grunt" && sound.Channel == 2));
        Assert.All(requests, sound =>
        { Assert.Equal((int)SimulationSoundFlags.NoRumble, sound.Flags); Assert.Equal(player.Id, sound.ActorId); });
    }

    [Theory]
    [InlineData(-10, false, 0)]
    [InlineData(-13, false, 1)]
    [InlineData(-13, true, 2)]
    public void LiquidSuppressesFloorLandSoundButNotGruntOrSupportLand(int velocity, bool support, int count)
    {
        var sim = Room(liquid: true); var player = sim.Players.Single();
        Actor? platform = support ? sim.AddBot(0, 0) : null;
        if (support) player.Z = Fixed.FromInt(60);
        player.VelocityZ = Fixed.FromInt(velocity); player.PlayerLandedMakeGruntSound(platform);
        var sounds = sim.DrainSoundRequests(); Assert.Equal(count, sounds.Length);
        Assert.Equal(support, sounds.Any(sound => sound.Sound == "*land"));
    }

    [Fact]
    public void AliasedLandAndGruntOnlyPlayGruntOnce()
    {
        var sim = Room(sameSounds: true); var player = sim.Players.Single();
        Drop(player, -13); ActorPhysics.Step(sim, player);
        Assert.Equal("*grunt", Assert.Single(sim.DrainSoundRequests()).Sound);
    }

    [Theory]
    [InlineData(-8, false, false)]
    [InlineData(-13, true, false)]
    [InlineData(-13, false, true)]
    public void SoftNoGravityAndIceEarlyReturnSkipLandingAudio(int velocity, bool noGravity, bool ice)
    {
        var sim = Room(); var player = sim.Players.Single(); player.NoGravity = noGravity;
        player.DamageType = ice ? "Ice" : null;
        Drop(player, velocity); ActorPhysics.Step(sim, player); Assert.Empty(sim.PendingSoundRequests);
    }

    [Fact]
    public void DeadPlayerDoesNotProduceLandingAudio()
    {
        var sim = Room(); var player = sim.Players.Single(); ActorDamage.Apply(player, 1000);
        sim.DrainSoundRequests(); Drop(player, -13); ActorPhysics.Step(sim, player);
        Assert.DoesNotContain(sim.PendingSoundRequests, sound => sound.Sound is "*land" or "*grunt");
    }

    [Fact]
    public void ActualActorSupportLandingQueuesSoundsBeforeVelocityStops()
    {
        var sim = Room(liquid: true); var player = sim.Players.Single(); sim.AddBot(0, 0);
        Drop(player, -13); player.Z = Fixed.FromInt(60); ActorPhysics.Step(sim, player);
        Assert.True(player.OnMobj);
        Assert.Contains(sim.PendingSoundRequests, sound => sound.Sound == "*grunt");
        Assert.Contains(sim.PendingSoundRequests, sound => sound.Sound == "*land");
    }

    [Fact]
    public void GruntSpeedSaveChecksumAndOlderDefaultArePreserved()
    {
        var sim = Room(); var player = sim.Players.Single(); var older = SimSavegame.Write(sim, true);
        sim.RestoreState(sim.CaptureState()); var baseline = sim.Checksum;
        player.GruntSpeed = 9.5; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(baseline, sim.Checksum);
        var saved = SimSavegame.Write(sim, true); Assert.Equal(317, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var restored = Room(); SimSavegame.Apply(restored, saved);
        Assert.Equal(9.5, restored.Players.Single().GruntSpeed); Assert.Equal(saved, SimSavegame.Write(restored, true));
        Drop(player, -10); Drop(restored.Players.Single(), -10);
        ActorPhysics.Step(sim, player); ActorPhysics.Step(restored, restored.Players.Single());
        Assert.Equal(sim.PendingSoundRequests, restored.PendingSoundRequests);
        SimSavegame.Apply(restored, older); Assert.Equal(12, restored.Players.Single().GruntSpeed);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void InvalidGruntSpeedIsRejected(double value)
    {
        var sim = Room(); var state = sim.CaptureState(); state.Actors[0].GruntSpeed = value;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
    }

    private static void Drop(PlayerPawn player, int velocity)
    { player.Z = Fixed.FromInt(1); player.OnGround = false; player.VelocityZ = Fixed.FromInt(velocity); }

    private static AuthoritySimulation Room(bool liquid = false, bool sameSounds = false) => AuthoritySimulation.Start(new PlayLevel
    {
        TerrainDefinitions = [new("Surface", IsLiquid: liquid)], DefaultTerrain = "Surface",
        SoundDefinitions = new Dictionary<string, string> { ["*grunt"] = "DSGRUNT", ["*land"] = "DSLAND" },
        SoundAliases = sameSounds ? new Dictionary<string, string> { ["*land"] = "*grunt" } : new Dictionary<string, string>(),
        Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }],
    });
}
