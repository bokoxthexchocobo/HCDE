using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RocketBlastFalloffTests
{
    [Theory]
    [InlineData(ProjectileKind.Rocket)]
    [InlineData(ProjectileKind.CyberRocket)]
    public void RocketsUseNativeSquareSurfaceDistanceAndTruncation(ProjectileKind kind)
    {
        foreach (var (x, y, z, damage) in new[]
        {
            (96d, 96d, 48d, 48), (-96d, -96d, 48d, 48),
            (96d, 0d, 144d, 3), (0d, 0d, 200d, 0),
            (127.5, 0d, 48d, 16), (144d, 0d, 48d, 0),
            (0d, 0d, 48d, 128), (143.5, 0d, 48d, 0),
        })
        {
            var sim = AuthoritySimulation.Start(new PlayLevel
            { Sectors = [new LevelSector { CeilingHeight = 512 }] }, rngSeed: 42);
            var owner = sim.AddBot(-1000, 0); owner.Brain = null;
            var target = sim.AddBot(x, y, 3001); target.Brain = null;
            target.Z = Fixed.FromDouble(z); target.Radius = Fixed.FromInt(16);
            target.Health = 1000; target.NoPain = true;
            var missile = sim.SpawnProjectile(owner, kind);
            missile.X = missile.Y = default; missile.Z = Fixed.FromInt(48);
            var random = sim.CombatRandomState;
            missile.ExplodeForAction();
            Assert.True(missile.Destroyed); Assert.Equal(1000 - damage, target.Health);
            Assert.Equal(random, sim.CombatRandomState);
            Assert.Equal(damage > 0 ? owner.Id : (uint?)null, target.LastDamageSourceId);
        }
    }
}
