using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorDamageFamilyTests
{
    [Theory]
    [InlineData(false, 10, null, 40)]
    [InlineData(true, 10, null, 40)]
    [InlineData(false, -10, null, 60)]
    [InlineData(true, -10, null, 60)]
    [InlineData(false, 10, "ZombieMan", 50)]
    [InlineData(true, 10, "ZombieMan", 50)]
    public void FamilyActionsSelectRelatedActorsAndApplyFilters(bool siblings, int amount, string? filter, int expected)
    {
        var sim = Room(); var parent = sim.AddBot(0, 0, 3004); var caller = siblings ? sim.AddBot(32, 0, 3004) : parent;
        if (siblings) caller.MasterId = parent.Id;
        var first = sim.AddBot(100, 0, 3001); var second = sim.AddBot(200, 0, 3001); var unrelated = sim.AddBot(300, 0, 3001);
        first.MasterId = second.MasterId = parent.Id;
        first.Health = second.Health = unrelated.Health = 50;
        if (siblings) ActorHealthActions.DamageSiblings(caller, amount, classFilter: filter, sourceSelector: AcsActorPointer.Null);
        else ActorHealthActions.DamageChildren(caller, amount, classFilter: filter, sourceSelector: AcsActorPointer.Null);
        Assert.Equal(expected, first.Health); Assert.Equal(expected, second.Health);
        Assert.Equal(50, unrelated.Health); Assert.Equal(20, caller.Health); Assert.Equal(20, parent.Health);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        first.Health = 1; sim.RestoreState(state);
        Assert.Equal(expected, first.Health); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void UnparentedActorsAreNotSiblings()
    {
        var sim = Room(); var caller = sim.AddBot(0, 0); sim.AddBot(100, 0);
        var bytes = SimSavegame.Write(sim);
        ActorHealthActions.DamageSiblings(caller, 100);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
