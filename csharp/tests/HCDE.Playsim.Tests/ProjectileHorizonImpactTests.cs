using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileHorizonImpactTests
{
    [Theory]
    [InlineData(ProjectileKind.Rocket, true)]
    [InlineData(ProjectileKind.Bfg, true)]
    [InlineData(ProjectileKind.Plasma, true)]
    [InlineData(ProjectileKind.Rocket, false)]
    [InlineData(ProjectileKind.Bfg, false)]
    [InlineData(ProjectileKind.Plasma, false)]
    public void HorizonRemovesMissileWithoutExplosionOrRandomDraws(ProjectileKind kind, bool horizon)
    {
        var sim = Room(horizon); var owner = sim.Players.Single(); owner.Health = 10000;
        var target = sim.AddBot(40, 60); target.Brain = null; target.Health = 10000;
        var missile = sim.SpawnProjectile(owner, kind);
        var random = sim.CombatRandomState;
        for (var i = 0; i < 10 && !missile.Destroyed; i++) sim.Tick();
        Assert.True(missile.Destroyed); Assert.True(missile.X.ToDouble() < 64);
        if (horizon)
        {
            Assert.Equal(10000, owner.Health); Assert.Equal(10000, target.Health);
            Assert.Equal(1000, sim.Level.Lines[0].Health); Assert.Equal(random, sim.CombatRandomState);
        }
        else
        {
            Assert.True(sim.Level.Lines[0].Health < 1000); Assert.NotEqual(random, sim.CombatRandomState);
            if (kind == ProjectileKind.Rocket) Assert.True(target.Health < 10000);
        }
    }

    [Theory]
    [InlineData(ProjectileKind.Rocket)]
    [InlineData(ProjectileKind.Plasma)]
    public void ActorBeforeHorizonStillReceivesDirectImpact(ProjectileKind kind)
    {
        var sim = Room(true); var target = sim.AddBot(40, 0); target.Brain = null; target.Health = 10000;
        var missile = sim.SpawnProjectile(sim.Players.Single(), kind);
        for (var i = 0; i < 10 && !missile.Destroyed; i++) sim.Tick();
        Assert.True(missile.Destroyed); Assert.True(target.Health < 10000);
        Assert.Equal(1000, sim.Level.Lines[0].Health);
    }

    [Fact]
    public void BfgHorizonSuppressesSprayThatOrdinaryWallWouldApply()
    {
        var first = Room(true); var second = Room(false);
        foreach (var sim in new[] { first, second })
        {
            var target = sim.AddBot(40, 40); target.Brain = null; target.Health = 10000;
            var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Bfg);
            for (var i = 0; i < 10 && !missile.Destroyed; i++) sim.Tick();
        }
        Assert.Equal(10000, first.Actors.Single(a => a is BotPawn).Health);
        Assert.True(second.Actors.Single(a => a is BotPawn).Health < 10000);
    }

    private static AuthoritySimulation Room(bool horizon) => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Sides = [new LevelSide { Sector = 0 }],
        Lines = [new LevelLine { X1 = 64, Y1 = 128, X2 = 64, Y2 = -128,
            SideFront = 0, SideBack = -1, Health = 1000, Special = horizon ? 9 : 0 }],
        Things = [new LevelThing { Type = 1 }] });
}
