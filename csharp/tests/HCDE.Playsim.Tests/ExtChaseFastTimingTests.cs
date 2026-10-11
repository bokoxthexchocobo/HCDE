using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ExtChaseFastTimingTests
{
    [Theory]
    [InlineData(-1, -1)]
    [InlineData(1, 1)]
    [InlineData(3, 3)]
    [InlineData(4, 3)]
    [InlineData(5, 3)]
    [InlineData(6, 3)]
    [InlineData(7, 4)]
    [InlineData(10, 5)]
    public void FastOptionUsesNativeCeilingHalfWithThreeTickMinimum(int initial, int expected)
    {
        var actor = Pawn(true, initial);
        ActorChaseActions.ExtChase(actor, false, false, false, nightmareFast: true);
        Assert.Equal(expected, actor.States.RemainingTics);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void TimingRequiresBothActionOptionAndFastMonsterSetting(bool fast, bool option)
    {
        var actor = Pawn(fast, 10);
        ActorChaseActions.ExtChase(actor, false, false, false, nightmareFast: option);
        Assert.Equal(10, actor.States.RemainingTics);
    }

    [Fact]
    public void TimingChangeDoesNotReenterStateAction()
    {
        var actor = Pawn(true, 10); var calls = 0;
        actor.States.Configure(actor, [new(10, 0, _ => calls++)], 0);
        ActorChaseActions.ExtChase(actor, false, false, false, nightmareFast: true);
        Assert.Equal(1, calls); Assert.Equal(0, actor.States.Current);
        Assert.Equal(5, actor.States.RemainingTics);
    }

    private static Actor Pawn(bool fast, int tics)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [] },
            spawnOptions: new SpawnOptions(FastMonsters: fast));
        var actor = new Actor { Simulation = sim, Brain = new MonsterBrain(MonsterAttack.Melee) };
        actor.States.Configure(actor, [new(tics, 0)], 0);
        return actor;
    }
}
