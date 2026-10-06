using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CrouchMovementProbeTests
{
    [Theory]
    [InlineData(28, true)]
    [InlineData(40, true)]
    [InlineData(56, false)]
    public void FitProbeRestoresHeightOnSuccessAndRejection(int height, bool fits)
    {
        var sim = Room(40);
        var player = sim.Players.Single();
        player.Height = Fixed.FromInt(28);
        Assert.Equal(fits, ActorPhysics.FitsAtHeight(sim, player, height));
        Assert.Equal(28, player.Height.ToDouble());
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(-1)]
    public void InvalidFitHeightDoesNotMutateActor(double candidate)
    {
        var sim = Room(128);
        var player = sim.Players.Single();
        var height = player.Height;
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorPhysics.FitsAtHeight(sim, player, candidate));
        Assert.Equal(height, player.Height);
    }

    [Fact]
    public void FailedShrinkMovementStillAllowsCrouching()
    {
        var sim = Room(128);
        var player = sim.Players.Single();
        sim.Ceilings[0] = 32;
        sim.QueueCommand(0, new PlayerCommand { Crouch = true });
        sim.Tick();
        Assert.True(player.CrouchFactor < 1);
        Assert.True(player.Height.ToDouble() < 56);
    }

    private static AuthoritySimulation Room(short ceiling) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = ceiling }],
        Things = [new LevelThing { Type = 1 }]
    });
}
