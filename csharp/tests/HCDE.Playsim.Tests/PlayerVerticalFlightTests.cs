using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerVerticalFlightTests
{
    [Theory]
    [InlineData(128, 1)]
    [InlineData(-128, -1)]
    [InlineData(32767, 6)]
    [InlineData(-32767, -6)]
    public void FlyInputClampsAndEnablesGravityFreeMovement(int up, double expected)
    {
        var sim = Room(); var player = sim.Players.Single();
        ActorPropertyActions.ChangeFlag(player, "FLY", true);
        Assert.True(player.Fly);
        Step(sim, new PlayerCommand { UpMove = (short)up });
        Assert.True(player.NoGravity); Assert.Equal(64 + expected, player.Z.ToDouble());
    }

    [Fact]
    public void VerticalInputOverridesJumpAndUsesConfiguredSpeed()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Fly = true;
        player.NoGravity = true; player.MovementSpeed = Fixed.FromInt(2);
        Step(sim, new PlayerCommand { Jump = true, UpMove = 128 });
        Assert.Equal(66, player.Z.ToDouble());
    }

    [Fact]
    public void LandKeepsFlyAbilityForLaterTakeoff()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Fly = true; player.NoGravity = true;
        Step(sim, new PlayerCommand { UpMove = short.MinValue });
        Assert.False(player.NoGravity); Assert.True(player.Fly);
        Step(sim, new PlayerCommand { UpMove = 128 });
        Assert.True(player.NoGravity); Assert.Equal(65, player.Z.ToDouble());
    }

    [Fact]
    public void FlightFlagSurvivesArchiveAndResetsFromLegacySave()
    {
        var sim = Room(); var legacy = SimSavegame.Write(sim); sim.Players.Single().Fly = true;
        var bytes = SimSavegame.Write(sim); var loaded = Room(); SimSavegame.Apply(loaded, bytes);
        Assert.True(loaded.Players.Single().Fly); Assert.Equal(bytes, SimSavegame.Write(loaded));
        Step(loaded, new PlayerCommand { UpMove = 128 }); Assert.True(loaded.Players.Single().NoGravity);
        SimSavegame.Apply(loaded, legacy); Assert.False(loaded.Players.Single().Fly);
    }

    private static void Step(AuthoritySimulation sim, PlayerCommand command) { sim.QueueCommand(0, command); sim.Tick(); }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1, Z = 64 }]
    });
}
