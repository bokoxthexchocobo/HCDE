using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class AcsDamageActorPointerTests
{
    [Theory]
    [InlineData(AcsActorPointer.Target)]
    [InlineData(AcsActorPointer.Master)]
    [InlineData(AcsActorPointer.Tracer)]
    public void TargetSelectorResolvesBeforeDamage(int selector)
    {
        var sim = Room(); var player = sim.Players.Single(); var origin = sim.AddBot(80, 0);
        origin.SetTargetPointer(player); origin.MasterId = player.Id; origin.TracerId = player.Id;
        Assert.Equal(10, Damage(sim, origin, [0, selector, 0, AcsActorPointer.Null, 10, 0]));
        Assert.Equal(90, player.Health); Assert.Equal(20, origin.Health);
    }

    [Fact]
    public void InflictorPointerControlsDamageAttributionAndMultiplier()
    {
        var sim = Room(); var player = sim.Players.Single(); var origin = sim.AddBot(80, 0);
        origin.SetTargetPointer(player); player.DamageMultiplier = Fixed.FromInt(2);
        Assert.Equal(20, Damage(sim, origin, [0, 0, 0, AcsActorPointer.Target, 10, 0]));
        Assert.Equal(player.Id, origin.LastDamageSourceId); Assert.Equal(0, origin.Health);
    }

    [Fact]
    public void MissingInflictorDoesNotFallBackToActivator()
    {
        var sim = Room(); var player = sim.Players.Single(); player.DamageMultiplier = Fixed.FromInt(2);
        Assert.Equal(10, Damage(sim, player, [0, 0, 999, 0, 10, 0]));
        Assert.Equal(90, player.Health); Assert.Null(player.LastDamageSourceId);
    }

    [Fact]
    public void ExplicitNullTargetDoesNotDamageActivator()
    {
        var sim = Room(); var player = sim.Players.Single();
        Assert.Equal(0, Damage(sim, player, [0, AcsActorPointer.Null, 0, 0, 10, 0])); Assert.Equal(100, player.Health);
    }

    [Fact]
    public void PooledDamageTypeUsesDrowningProtection()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerIronFeet(); var strings = new AcsGlobalStrings();
        var stack = new List<int> { 0, 0, 999, 0, 10, strings.Add("Drowning") };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = player }, [], strings,
            AcsCallFunctions.DamageActor, 6, out var result));
        Assert.Equal(0, result); Assert.Equal(100, player.Health);
    }

    private static int Damage(AuthoritySimulation sim, Actor actor, int[] args)
    {
        var stack = new List<int> { 77 }; stack.AddRange(args);
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = actor }, ["None"], new(),
            AcsCallFunctions.DamageActor, 6, out var result)); Assert.Equal(new[] { 77 }, stack); return result;
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
