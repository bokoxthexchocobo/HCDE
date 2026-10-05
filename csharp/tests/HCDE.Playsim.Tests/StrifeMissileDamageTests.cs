using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class StrifeMissileDamageTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ScriptFlagSelectsNativeDiceMask(bool strife)
    {
        for (var seed = 1; seed <= 16; seed++)
        {
            var (sim, missile, target) = Setup(seed);
            Assert.True(AcsActorFlags.TrySet(missile, "Actor.STRIFEDAMAGE", strife));
            var (prediction, _, _) = Setup(seed);
            var random = prediction.NextCombatRandom();
            var expected = 5 * (1 + (int)(random & (strife ? 3u : 7u)));
            missile.Tick();
            Assert.Equal(expected, 1000 - target.Health);
        }
    }

    [Fact]
    public void ExpressionStillBypassesStrifeDice()
    {
        var (sim, missile, target) = Setup(1); missile.StrifeDamage = true;
        missile.DamageExpression = _ => 9;
        var random = sim.CombatRandomState; missile.Tick();
        Assert.Equal(9, 1000 - target.Health); Assert.Equal(random, sim.CombatRandomState);
    }

    [Fact]
    public void FlagCanBeQueriedAndCleared()
    {
        var actor = new Actor();
        Assert.True(AcsActorFlags.TrySet(actor, "strifedamage", true));
        Assert.True(AcsActorFlags.TryGet(actor, "Actor.STRIFEDAMAGE", out var value)); Assert.True(value);
        Assert.True(AcsActorFlags.TrySet(actor, "STRIFEDAMAGE", false)); Assert.False(actor.StrifeDamage);
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile, Actor Target) Setup(int seed)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] }, rngSeed: seed);
        var owner = sim.AddBot(-200, 0, 3004); owner.Brain = null;
        var target = sim.AddBot(30, 0, 3001); target.Brain = null; target.Health = 1000; target.NoPain = true;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        return (sim, missile, target);
    }
}
