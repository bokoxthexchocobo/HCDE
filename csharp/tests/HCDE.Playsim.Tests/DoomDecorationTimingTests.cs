using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DoomDecorationTimingTests
{
    [Theory]
    [InlineData(85, 4, 4, 4, 4)]
    [InlineData(86, 4, 4, 4, 4)]
    [InlineData(44, 4, 4, 4, 4)]
    [InlineData(45, 4, 4, 4, 4)]
    [InlineData(46, 4, 4, 4, 4)]
    [InlineData(55, 4, 4, 4, 4)]
    [InlineData(56, 4, 4, 4, 4)]
    [InlineData(57, 4, 4, 4, 4)]
    [InlineData(36, 14, 14)]
    [InlineData(41, 6, 6, 6, 6)]
    [InlineData(42, 6, 6, 6)]
    [InlineData(29, 6, 6)]
    [InlineData(26, 6, 8)]
    [InlineData(49, 10, 15, 8, 6)]
    [InlineData(63, 10, 15, 8, 6)]
    [InlineData(70, 4, 4, 4)]
    public void NativeFrameDurationsLoopExactly(int type, params int[] durations)
    {
        var sim = Room(type); var actor = sim.Actors.Single();
        Assert.InRange(actor.States.RemainingTics, 1, durations[0]);
        actor.States.Enter(actor, 0);
        for (var cycle = 0; cycle < 2; cycle++)
            for (var frame = 0; frame < durations.Length; frame++)
            {
                Assert.Equal(frame, actor.States.Current); Assert.Equal(durations[frame], actor.States.RemainingTics);
                for (var tic = 1; tic < durations[frame]; tic++) { sim.Tick(); Assert.Equal(frame, actor.States.Current); }
                sim.Tick();
            }
        Assert.Equal(0, actor.States.Current); Assert.False(actor.Destroyed);
    }

    [Fact]
    public void SaveRestoresUnevenLoopMidFrame()
    {
        var sim = Room(49); var initialTics = sim.Actors.Single().States.RemainingTics;
        for (var i = 0; i < initialTics + 3; i++) sim.Tick();
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var state, out var error), error);
        var restored = Room(49); restored.RestoreState(state);
        var actor = restored.Actors.Single(); Assert.Equal(1, actor.States.Current); Assert.Equal(12, actor.States.RemainingTics);
        for (var i = 0; i < 12; i++) restored.Tick();
        Assert.Equal(2, actor.States.Current); Assert.Equal(8, actor.States.RemainingTics);
    }

    [Fact]
    public void StaticDecorationHoldsSpawnIndefinitely()
    {
        var sim = Room(30); var actor = sim.Actors.Single();
        for (var i = 0; i < 100; i++) sim.Tick();
        Assert.Equal(0, actor.States.Current); Assert.Equal(-1, actor.States.RemainingTics); Assert.False(actor.Destroyed);
    }

    private static AuthoritySimulation Room(int type) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 160 }], Things = [new LevelThing { Type = type }],
    });
}
