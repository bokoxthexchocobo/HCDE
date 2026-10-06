using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerPredictedLandingTests
{
    [Theory]
    [InlineData(0, true)]
    [InlineData(30, true)]
    [InlineData(36, false)]
    public void FallingPlayerUsesSquareSupportBounds(int offset, bool lands)
    {
        var (sim, player, support) = Setup(); support.X = support.Y = Fixed.FromInt(offset);
        ActorPhysics.Step(sim, player);
        Assert.Equal(56, player.Z.ToDouble());
        Assert.Equal(lands ? 0 : -5, player.VelocityZ.ToDouble());
        Assert.Equal(lands, player.OnMobj); Assert.Equal(lands, player.OnGround);
    }

    [Fact]
    public void HighestPredictedBlockerWins()
    {
        var (sim, player, _) = Setup(); var upper = sim.AddBot(0, 0); upper.Z = Fixed.FromInt(10);
        ActorPhysics.Step(sim, player);
        Assert.Equal(66, player.Z.ToDouble()); Assert.Equal(default, player.VelocityZ);
        Assert.True(player.OnMobj);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void ExcludedActorDoesNotStopFall(int mode)
    {
        var (sim, player, support) = Setup();
        support.ThruActors = mode == 0; support.NoBlockmap = mode == 1; support.Solid = mode != 2;
        support.SpecialPickup = mode == 3;
        ActorPhysics.Step(sim, player);
        Assert.Equal(56, player.Z.ToDouble()); Assert.Equal(-5, player.VelocityZ.ToDouble());
        Assert.False(player.OnMobj);
    }

    [Fact]
    public void LandedStateSurvivesSaveAndNextStep()
    {
        var (sim, player, support) = Setup(); support.X = support.Y = Fixed.FromInt(30);
        ActorPhysics.Step(sim, player);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var (restored, rider, platform) = Setup(); restored.RestoreState(state);
        Assert.Equal(support.X, platform.X); Assert.True(rider.OnMobj);
        ActorPhysics.Step(restored, rider);
        Assert.Equal(56, rider.Z.ToDouble()); Assert.True(rider.OnMobj);
    }

    private static (AuthoritySimulation Sim, PlayerPawn Player, Actor Support) Setup()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 256 }] });
        var player = sim.Players.Single(); var support = sim.AddBot(0, 0);
        player.Z = Fixed.FromInt(60); player.VelocityZ = Fixed.FromInt(-4); player.OnGround = false;
        return (sim, player, support);
    }
}
