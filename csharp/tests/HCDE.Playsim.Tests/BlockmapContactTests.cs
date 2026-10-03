using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BlockmapContactTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void ExcludedSolidTargetDoesNotBlockMovement(bool excluded, bool moved)
    {
        var sim = Room(); var target = Assert.Single(sim.Actors); target.NoBlockmap = excluded;
        var mover = new Actor { Health = 100, Solid = true, X = Fixed.FromInt(-64), SectorIndex = 0 };
        Assert.Equal(moved, ActorPhysics.TryMove(sim, mover, 64, 0, out _));
        mover.X = target.X;
        Assert.Equal(excluded, ActorPhysics.CanOccupy(sim, mover));
    }

    [Fact]
    public void ExcludedMoverStillCollidesWithRegisteredTargets()
    {
        var sim = Room();
        var mover = new Actor { Health = 100, Solid = true, NoBlockmap = true, X = Fixed.FromInt(-64), SectorIndex = 0 };
        Assert.False(ActorPhysics.TryMove(sim, mover, 64, 0, out _));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ProjectileContactSkipsOnlyExcludedTargets(bool excluded)
    {
        var sim = Room(); var target = Assert.Single(sim.Actors);
        target.NoBlockmap = excluded; target.Brain!.Enabled = false;
        var owner = new Actor { Health = 100, X = Fixed.FromInt(-64) };
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        var health = target.Health;
        for (var i = 0; i < 8; i++) sim.Tick();
        Assert.Equal(excluded, target.Health == health);
        Assert.Equal(!excluded, missile.Destroyed);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 3004 }],
    });
}
