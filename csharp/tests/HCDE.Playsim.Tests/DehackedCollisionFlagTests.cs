using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedCollisionFlagTests
{
    [Theory]
    [InlineData(0, false, false)]
    [InlineData(2, true, false)]
    [InlineData(4, false, true)]
    [InlineData(6, true, true)]
    public void ExplicitBitsInitializeSolidAndShootable(int bits, bool solid, bool shootable)
    {
        var sim = Room(bits);
        var actor = sim.Actors.Single(candidate => candidate.DoomEdNum == 3004);
        Assert.Equal(solid, actor.Solid);
        Assert.Equal(shootable, actor.Shootable);
    }

    [Theory]
    [InlineData(2, false)]
    [InlineData(6, true)]
    public void ShootableFlagControlsActualDamage(int bits, bool hurt)
    {
        var sim = Room(bits);
        var actor = sim.Actors.Single(candidate => candidate.DoomEdNum == 3004);
        var health = actor.Health;
        ActorDamage.Apply(actor, 1);
        Assert.Equal(health - (hurt ? 1 : 0), actor.Health);
    }

    [Theory]
    [InlineData(4, true)]
    [InlineData(6, false)]
    public void SolidFlagControlsActorMovementBlocking(int bits, bool canMove)
    {
        var sim = Room(bits);
        Assert.Equal(canMove, ActorPhysics.TryMove(sim, sim.Players.Single(), 100, 0, out _));
    }

    private static AuthoritySimulation Room(int bits) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, X = 100 }],
    }, dehacked: DehackedPatch.Apply($"Thing 2\nBits = {bits}\n"));
}
