using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsNoBlockmapTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CallFunctionMutationImmediatelyControlsProjectileContact(bool excluded)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var owner = sim.AddBot(-200, 0, 3004);
        var target = sim.AddBot(30, 0, 3001);
        owner.Brain = target.Brain = null;
        target.ThingId = 77; target.PainChance = 0;
        var health = target.Health;
        var stack = new List<int> { 77, 0, excluded ? 1 : 0 };
        var binding = new AcsActivatorBinding { Value = owner };
        var globals = new AcsGlobalStrings();
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, binding, ["Actor.NOBLOCKMAP"], globals,
            AcsCallFunctions.SetActorFlag, 3, out var changed));
        Assert.Equal(1, changed); Assert.Empty(stack);
        stack.AddRange([77, 0]);
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, binding, ["noblockmap"], globals,
            AcsCallFunctions.CheckFlag, 2, out var enabled));
        Assert.Equal(excluded ? 1 : 0, enabled);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        missile.Tick();
        Assert.Equal(!excluded, missile.Destroyed);
        Assert.Equal(excluded, target.Health == health);
    }

    [Fact]
    public void CallFunctionCountsAllMatchingTidActors()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var first = sim.AddBot(0, 0, 3001); var second = sim.AddBot(100, 0, 3001);
        first.ThingId = second.ThingId = 77;
        var stack = new List<int> { 77, 0, 1 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding(), ["NOBLOCKMAP"],
            new AcsGlobalStrings(), AcsCallFunctions.SetActorFlag, 3, out var changed));
        Assert.Equal(2, changed);
        Assert.True(first.NoBlockmap); Assert.True(second.NoBlockmap);
    }
}
