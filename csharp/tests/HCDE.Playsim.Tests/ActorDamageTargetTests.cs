using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorDamageTargetTests
{
    [Theory]
    [InlineData(10, null, 40)]
    [InlineData(-10, null, 60)]
    [InlineData(0, null, 50)]
    [InlineData(10, "DoomImp", 40)]
    [InlineData(10, "ZombieMan", 50)]
    public void FrameActionDamagesOrHealsTargetAndRetainsCallerContext(int amount, string? filter, int expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var caller = sim.AddBot(0, 0, 3004); var target = sim.AddBot(100, 0, 3001, thingId: 7);
        caller.Health = 100; target.Health = 50; caller.Brain!.SetTargetThingId(sim, 7);
        caller.States.Configure(caller, [new(-1, 0), new(-1, 0,
            Action: self => ActorHealthActions.DamageTarget(self, amount, classFilter: filter))], 0);
        caller.States.Enter(caller, 1);
        Assert.Equal(expected, target.Health); Assert.Equal(100, caller.Health);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        target.Health = 1; sim.RestoreState(state);
        Assert.Equal(expected, target.Health); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void MissingTargetDoesNotDamageCaller()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var caller = sim.AddBot(0, 0); var before = SimSavegame.Write(sim);
        ActorHealthActions.DamageTarget(caller, 100);
        Assert.Equal(before, SimSavegame.Write(sim));
    }
}
