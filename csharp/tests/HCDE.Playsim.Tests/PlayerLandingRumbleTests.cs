using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerLandingRumbleTests
{
    [Theory]
    [InlineData(false, false, -10, "*land")]
    [InlineData(true, false, -12, null)]
    [InlineData(true, false, -13, "*grunt")]
    [InlineData(true, true, -10, "*land")]
    [InlineData(true, true, -13, "*land")]
    public void PatternSelectionUsesLiquidSupportAndStrictGruntThreshold(bool liquid, bool support,
        int velocity, string? expected)
    {
        var sim = Room(liquid); var player = sim.Players.Single(); var requests = Listen(sim);
        if (support) sim.AddBot(0, 0).Brain = null;
        Drop(player, velocity); if (support) player.Z = Fixed.FromInt(60);
        ActorPhysics.Step(sim, player);
        if (expected is null) Assert.Empty(requests);
        else Assert.Equal(new SimulationRumbleRequest(player.Id, expected), Assert.Single(requests));
    }

    [Fact]
    public void WorldPreferenceSuppressesRumbleWithoutSuppressingAudioOrChangingGameChecksum()
    {
        var sim = Room(); var player = sim.Players.Single(); var requests = Listen(sim);
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        sim.WorldHapticsEnabled = false; sim.RestoreState(sim.CaptureState()); Assert.Equal(checksum, sim.Checksum);
        Drop(player, -10); ActorPhysics.Step(sim, player);
        Assert.Empty(requests); Assert.Contains(sim.PendingSoundRequests, sound => sound.Sound == "*land");
    }

    [Fact]
    public void FatalFallingDamageStillRumblesBeforeLivingGruntIsSuppressed()
    {
        var sim = Room(); sim.ForcedFallingDamage = FallingDamageStyle.ZDoom;
        var player = sim.Players.Single(); var requests = Listen(sim);
        Drop(player, -84); ActorPhysics.Step(sim, player);
        Assert.True(player.IsDead); Assert.Equal(new[] { "*death", "*land" }, requests.Select(request => request.Pattern));
        Assert.DoesNotContain(sim.PendingSoundRequests, sound => sound.Sound == "*grunt");
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void NoGravityAndIceReturnSkipLandingRumble(bool noGravity, bool ice)
    {
        var sim = Room(); var player = sim.Players.Single(); var requests = Listen(sim);
        player.NoGravity = noGravity; player.DamageType = ice ? "Ice" : null;
        Drop(player, -13); ActorPhysics.Step(sim, player); Assert.Empty(requests);
    }

    [Fact]
    public void SavingAndRestoringDoNotEmitRumbleButReplayedImpactDoes()
    {
        var sim = Room(); var player = sim.Players.Single(); var requests = Listen(sim);
        Drop(player, -10); var saved = SimSavegame.Write(sim, true); Assert.Empty(requests);
        ActorPhysics.Step(sim, player); Assert.Single(requests); requests.Clear();
        SimSavegame.Apply(sim, saved); Assert.Empty(requests);
        ActorPhysics.Step(sim, player); Assert.Single(requests);
        var landed = SimSavegame.Write(sim, true); requests.Clear();
        SimSavegame.Apply(sim, landed); ActorPhysics.Step(sim, player); Assert.Empty(requests);
    }

    private static List<SimulationRumbleRequest> Listen(AuthoritySimulation sim)
    { var requests = new List<SimulationRumbleRequest>(); sim.RumbleRequested += requests.Add; return requests; }

    private static void Drop(PlayerPawn player, int velocity)
    { player.Z = Fixed.FromInt(1); player.OnGround = false; player.VelocityZ = Fixed.FromInt(velocity); }

    private static AuthoritySimulation Room(bool liquid = false) => AuthoritySimulation.Start(new PlayLevel
    {
        TerrainDefinitions = [new("Surface", IsLiquid: liquid)], DefaultTerrain = "Surface",
        Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }],
    });
}
