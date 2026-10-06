using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CorpseCollisionFlagTests
{
    [Theory]
    [InlineData(false, true, false, false)]
    [InlineData(true, false, false, true)]
    [InlineData(false, true, true, true)]
    [InlineData(true, true, true, true)]
    public void VerticalBlockerProbeUsesCorpseFlagAndIceException(bool dead, bool corpse, bool ice, bool blocks)
    {
        var sim = Room(); var obstacle = sim.AddBot(0, 0, 3004);
        if (dead) obstacle.Health = 0;
        obstacle.Corpse = corpse; obstacle.Solid = true;
        var mover = new Actor { Solid = true, IceCorpse = ice, Height = Fixed.FromInt(56), Radius = Fixed.FromInt(20) };
        Assert.Equal(blocks ? obstacle : null, ActorPhysics.FindZBlocker(sim, mover));
    }

    [Theory]
    [InlineData(false, true, false, false)]
    [InlineData(true, false, false, true)]
    [InlineData(false, true, true, true)]
    public void RipperPassageUsesCorpseFlagAndShootability(bool dead, bool corpse, bool shootable, bool blocks)
    {
        var sim = Room(); var obstacle = sim.AddBot(0, 0, 3004);
        if (dead) obstacle.Health = 0;
        obstacle.Corpse = corpse; obstacle.Solid = true; obstacle.Shootable = shootable;
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.ImpBall);
        missile.Rip = true; missile.X = missile.Y = missile.Z = default; missile.SectorIndex = 0;
        Assert.Equal(blocks ? 2 : (int?)null, ActorJumpActions.CheckBlock(missile, 2));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 500 }]
    });
}
