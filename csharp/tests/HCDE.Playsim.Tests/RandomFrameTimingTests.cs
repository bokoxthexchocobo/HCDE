using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RandomFrameTimingTests
{
    [Theory]
    [InlineData(1, false)]
    [InlineData(9, true)]
    [InlineData(65535, false)]
    public void RandomRangePrecedesFastScalingAndLeavesCombatStreamAlone(int range, bool fast)
    {
        var sim = Room(); var control = Room(); var actor = sim.Actors[0]; actor.AlwaysFast = fast;
        var frame = new ActorFrame(5, 0, Fast: true, TicRange: range);
        for (var i = 0; i < 64; i++)
        {
            var raw = 5 + (int)(control.NextStateRandom() % ((uint)range + 1));
            Assert.Equal(fast ? raw - (raw >> 1) : raw, frame.GetTics(actor));
        }
        Assert.Equal(control.NextCombatRandom(), sim.NextCombatRandom());
        Assert.Equal(control.NextJumpRandom(), sim.NextJumpRandom());
    }

    [Fact]
    public void SavePreservesStreamAndRestoresTicsWithoutDrawingAgain()
    {
        var sim = Room(); var actor = sim.Actors[0];
        actor.States.Configure(actor, [new(-1, 0), new(5, 0, TicRange: 9)], 0);
        var older = SimSavegame.Write(sim);
        actor.States.Enter(actor, 1); var tics = actor.States.RemainingTics;
        var saved = SimSavegame.Write(sim); var next = sim.NextStateRandom();
        SimSavegame.Apply(sim, saved); Assert.Equal(tics, actor.States.RemainingTics);
        Assert.Equal(next, sim.NextStateRandom());
        SimSavegame.Apply(sim, older);
        Assert.Equal(Room().NextStateRandom(), sim.NextStateRandom());
    }

    [Fact]
    public void ZeroRangeDoesNotDrawAndInvalidOrDetachedRangeFailsExplicitly()
    {
        var sim = Room(); var control = Room();
        Assert.Equal(5, new ActorFrame(5, 0).GetTics(sim.Actors[0]));
        Assert.Equal(control.NextStateRandom(), sim.NextStateRandom());
        Assert.Throws<InvalidOperationException>(() => new ActorFrame(5, 0, TicRange: 1).GetTics(new Actor()));
        Assert.Throws<ArgumentException>(() => sim.Actors[0].States.Configure(sim.Actors[0],
            [new(-1, 0, TicRange: 65536)], 0));
    }

    private static AuthoritySimulation Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }] }, rngSeed: 42);
        sim.Actors[0].Brain = null; return sim;
    }
}
