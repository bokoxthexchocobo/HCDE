using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BlastedWallDamageTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(31, 0)]
    [InlineData(32, 1)]
    [InlineData(100, 3)]
    [InlineData(128, 4)]
    [InlineData(-32, 0)]
    public void BlockedBlastedMoveDealsMassScaledDamage(int mass, int damage)
    {
        var sim = Room(); var actor = Assert.Single(sim.Actors);
        actor.Blasted = true; actor.Mass = mass;
        Assert.False(ActorPhysics.TryMove(sim, actor, 40, 0, out var wall)); Assert.NotNull(wall);
        Assert.Equal(1000 - damage, actor.Health); Assert.Equal(0, actor.X.Raw);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InvulnerabilityBlocksWallDamage(bool invulnerable)
    {
        var sim = Room(); var actor = Assert.Single(sim.Actors); actor.Blasted = true; actor.Mass = 128;
        actor.Invulnerable = invulnerable;
        Assert.False(ActorPhysics.TryMove(sim, actor, 40, 0, out _));
        Assert.Equal(invulnerable ? 1000 : 996, actor.Health);
    }

    [Fact]
    public void IncomingDamageFactorAppliesToWallDamage()
    {
        var sim = Room(); var actor = Assert.Single(sim.Actors); actor.Blasted = true; actor.Mass = 128;
        actor.DamageFactor = Fixed.FromDouble(0.5);
        Assert.False(ActorPhysics.TryMove(sim, actor, 40, 0, out _)); Assert.Equal(998, actor.Health);
    }

    [Fact]
    public void UnblockedBlastedMoveDoesNotDamage()
    {
        var sim = Room(); var actor = Assert.Single(sim.Actors); actor.Blasted = true; actor.Mass = 128;
        Assert.True(ActorPhysics.TryMove(sim, actor, 0, 10, out _)); Assert.Equal(1000, actor.Health);
    }

    [Fact]
    public void OrdinaryBlockedMoveDoesNotDamage()
    {
        var sim = Room(); var actor = Assert.Single(sim.Actors); actor.Mass = 128;
        Assert.False(ActorPhysics.TryMove(sim, actor, 40, 0, out _)); Assert.Equal(1000, actor.Health);
    }

    private static AuthoritySimulation Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }],
            Lines = [new LevelLine { X1 = 24, X2 = 24, Y1 = -128, Y2 = 128, SideBack = -1 }] });
        var actor = Assert.Single(sim.Actors); actor.Brain = null; actor.Health = 1000; actor.PainChance = 0;
        return sim;
    }
}
