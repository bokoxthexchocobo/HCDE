using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorProximityCountTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(32)]
    public void CountingReturnsEveryMatchRegardlessOfCountComparisonFlags(int flags)
    {
        var sim = Room(); sim.AddBot(10, 0, 3001); sim.AddBot(20, 0, 3001); sim.AddBot(30, 0, 3001);
        Assert.Equal(3, ActorProximity.CountProximity(sim.Players.Single(), "DoomImp", 100, flags));
    }

    [Fact]
    public void CountingUsesSameKilledAndReferenceFilters()
    {
        var sim = Room(); var caller = sim.AddBot(200, 0); var reference = sim.AddBot(0, 0, 3001);
        caller.Brain!.SetSpecialTarget(reference); var other = sim.AddBot(20, 0, 3001); other.Killed = true;
        Assert.Equal(0, ActorProximity.CountProximity(caller, "DoomImp", 100, pointerSelector: 2));
        Assert.Equal(1, ActorProximity.CountProximity(caller, "DoomImp", 100, flags: 8, pointerSelector: 2));
        Assert.Equal(0, ActorProximity.CountProximity(caller, "DoomImp", 100, flags: 8, pointerSelector: 1));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(2, 1)]
    public void AcsResolvesGlobalLocalAndMissingClassStrings(int source, int expected)
    {
        var sim = Room(); sim.AddBot(20, 0, 3001); var globals = new AcsGlobalStrings();
        var id = source switch { 0 => globals.Add("DoomImp"), 1 => AcsStringIds.FromGlobalIndex(999), _ => 0 };
        var stack = new List<int> { 0, id, 100 << 16 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding { Value = sim.Players.Single() },
            ["DoomImp"], globals, AcsCallFunctions.CheckProximity, 3, out var result));
        Assert.Equal(expected, result); Assert.Empty(stack);
    }

    [Fact]
    public void ActorClassCallResultCanBeUsedAsProximityClassArgument()
    {
        var sim = Room(); var imp = sim.AddBot(20, 0, 3001); imp.ThingId = 42;
        var globals = new AcsGlobalStrings(); var binding = new AcsActivatorBinding { Value = sim.Players.Single() };
        var stack = new List<int> { 42 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, binding, [], globals,
            AcsCallFunctions.GetActorClass, 1, out var classId));
        stack.AddRange([0, classId, 100 << 16]);
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, binding, [], globals,
            AcsCallFunctions.CheckProximity, 3, out var result));
        Assert.Equal(1, result); Assert.Empty(stack);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }], Things = [new LevelThing { Type = 1 }],
    });
}
