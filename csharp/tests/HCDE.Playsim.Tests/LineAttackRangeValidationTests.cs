using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LineAttackRangeValidationTests
{
    private static AuthoritySimulation Room(bool wall = false) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Sides = [new LevelSide { Sector = 0 }],
        Lines = wall ? [new LevelLine { X1 = 32, X2 = 32, Y1 = 128, Y2 = -128,
            SideFront = 0, SideBack = -1, Health = 100 }] : [],
        Things = [new LevelThing { Type = 1 }],
    });

    [Theory]
    [InlineData(-1)]
    [InlineData(-64)]
    [InlineData(0)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void NonpositiveOrNonfiniteHelperRangeDoesNotBecomeForwardAttack(double range)
    {
        var sim = Room(); var source = sim.Players.Single(); var target = sim.AddBot(32, 0);
        var health = target.Health; var count = sim.Actors.Count; var random = sim.CombatRandomState;
        Assert.Equal(0, AcsLineAttack.Attack(sim, source, 0, 0, 7, "None", range, puffTid: 42));
        Assert.Equal(health, target.Health);
        Assert.Equal(count, sim.Actors.Count);
        Assert.Equal(random, sim.CombatRandomState);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NegativeAcsRangeDoesNotDamageForwardActorOrWall(bool wall)
    {
        var sim = Room(wall); var source = sim.Players.Single(); var target = sim.AddBot(48, 0);
        var health = target.Health; var count = sim.Actors.Count;
        var stack = new List<int> { 987, 0, 0, 0, 7, 0, 0, -64 << 16, 0, 42 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding { Value = source },
            [], new AcsGlobalStrings(), AcsCallFunctions.LineAttack, 9, out var result));
        Assert.Equal(0, result); Assert.Equal(new[] { 987 }, stack);
        Assert.Equal(health, target.Health); Assert.Equal(count, sim.Actors.Count);
        if (wall) Assert.Equal(100, sim.Level.Lines[0].Health);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OmittedOrZeroAcsRangeKeepsNativeDefault(bool explicitZero)
    {
        var sim = Room(); var source = sim.Players.Single(); var target = sim.AddBot(1000, 0);
        var health = target.Health;
        var stack = new List<int> { 0, 0, 0, 7 };
        if (explicitZero) stack.AddRange([0, 0, 0]);
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding { Value = source },
            [], new AcsGlobalStrings(), AcsCallFunctions.LineAttack, explicitZero ? 7 : 4, out var result));
        Assert.Equal(0, result); Assert.Empty(stack);
        Assert.Equal(health - 7, target.Health);
    }
}
