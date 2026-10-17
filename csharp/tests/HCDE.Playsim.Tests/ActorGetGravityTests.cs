using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorGetGravityTests
{
    [Theory]
    [InlineData(800, 1, 1, 1)]
    [InlineData(400, 2, 0.125, 0.125)]
    [InlineData(800, 0.5, 2, 1)]
    [InlineData(0, 2, 1, 0)]
    [InlineData(-800, 0.5, 1, -0.5)]
    [InlineData(800, 0, 1, 0)]
    [InlineData(800, 1, 0, 0)]
    public void QueryCombinesNativeFactorsAndMatchesSavedMovement(double level, double sector, double scale, double expected)
    {
        var sim = Room(level, sector); var player = sim.Players.Single();
        ActorPropertyActions.SetGravity(player, scale);
        player.Z = Fixed.FromInt(64); player.OnGround = false; player.VelocityZ = Fixed.FromInt(1);
        Assert.Equal(expected, player.GetGravity());
        var saved = SimSavegame.Write(sim, true); var fresh = Room(level, sector); SimSavegame.Apply(fresh, saved);
        Assert.Equal(expected, fresh.Players.Single().GetGravity());
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        sim.Tick(); fresh.Tick();
        Assert.Equal(Fixed.FromDouble(1 - expected), player.VelocityZ);
        Assert.Equal(SimSavegame.Write(sim, true), SimSavegame.Write(fresh, true));
    }

    [Fact]
    public void NoGravityOverridesAllFactorsAndRuntimeChangesAreImmediate()
    {
        var sim = Room(800, 2); var player = sim.Players.Single();
        ActorPropertyActions.NoGravity(player); Assert.Equal(0, player.GetGravity());
        sim.SetLevelGravity(400 * 65536); Assert.Equal(0, player.GetGravity());
        ActorPropertyActions.LowGravity(player); Assert.Equal(0.125, player.GetGravity());
        ActorPropertyActions.Gravity(player); Assert.Equal(1, player.GetGravity());
    }

    [Fact]
    public void DetachedNoGravityNeedsNoWorldButEnabledGravityRequiresOne()
    {
        var actor = new Actor { NoGravity = true }; Assert.Equal(0, actor.GetGravity());
        actor.NoGravity = false; Assert.Throws<InvalidOperationException>(() => actor.GetGravity());
    }

    private static AuthoritySimulation Room(double level, double sector) => AuthoritySimulation.Start(new PlayLevel {
        LevelGravity = level, Sectors = [new LevelSector { CeilingHeight = 256, Gravity = sector }],
        Things = [new LevelThing { Type = 1 }]
    });
}
