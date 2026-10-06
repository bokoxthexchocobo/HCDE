using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorProximityInheritanceTests
{
    [Theory]
    [InlineData(58, "Demon", 0, false)]
    [InlineData(58, "Demon", 1, true)]
    [InlineData(58, "Spectre", 1, true)]
    [InlineData(3002, "Spectre", 1, false)]
    [InlineData(69, "BaronOfHell", 0, false)]
    [InlineData(69, "BaronOfHell", 1, true)]
    [InlineData(3003, "HellKnight", 1, false)]
    [InlineData(3001, "Actor", 1, true)]
    [InlineData(3001, "Actor", 0, false)]
    public void BuiltInAncestryIsDirectionalAndOptional(int type, string name, int flags, bool expected)
    {
        var sim = Room(); sim.AddBot(20, 0, type);
        Assert.Equal(expected, ActorProximity.Check(sim.Players.Single(), name, 100, flags: flags));
    }

    [Fact]
    public void PlayerPawnAncestorMatchesDoomPlayerButExactClassDoesNot()
    {
        var sim = Room(); var caller = sim.AddBot(20, 0);
        Assert.False(ActorProximity.Check(caller, "PlayerPawn", 100));
        Assert.True(ActorProximity.Check(caller, "PlayerPawn", 100, flags: 1));
        Assert.True(ActorProximity.Check(caller, "DoomPlayer", 100));
        Assert.Equal(1, ActorProximity.CountProximity(caller, "Actor", 100, flags: 1));
    }

    [Fact]
    public void InheritedClassesParticipateInTargetSelectionAndCounting()
    {
        var sim = Room(); var caller = sim.AddBot(0, 20); var spectre = sim.AddBot(20, 20, 58);
        sim.AddBot(40, 20, 3002);
        Assert.Equal(2, ActorProximity.CountProximity(caller, "Demon", 100, flags: 1));
        Assert.True(ActorProximity.Check(caller, "Demon", 100, flags: 1 | 64 | 1024));
        Assert.Equal(spectre.Id, caller.Brain!.TargetId);
    }

    [Fact]
    public void AcsAncestorFlagIncludesChildClassWithGlobalString()
    {
        var sim = Room(); sim.AddBot(20, 0, 69); var globals = new AcsGlobalStrings();
        var stack = new List<int> { 0, globals.Add("BaronOfHell"), 100 << 16, 1, 1 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding { Value = sim.Players.Single() },
            [], globals, AcsCallFunctions.CheckProximity, 5, out var result));
        Assert.Equal(1, result); Assert.Empty(stack);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
