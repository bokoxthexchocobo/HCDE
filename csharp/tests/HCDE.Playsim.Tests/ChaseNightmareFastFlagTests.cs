using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseNightmareFastFlagTests
{
    [Theory]
    [InlineData(true, 4, 5)]
    [InlineData(false, 4, 10)]
    [InlineData(true, 0, 10)]
    [InlineData(true, 6, 5)]
    [InlineData(true, 20, 5)]
    [InlineData(true, 260, 5)]
    [InlineData(true, 1028, 5)]
    public void NativeBitAndSupportedCombinationsUseSkillGate(bool fast, int flags, int expected)
    {
        var (sim, actor) = Room(fast);
        actor.Brain!.Tick(sim, actor, (MonsterChaseFlags)flags, false, false);
        Assert.Equal(expected, actor.States.RemainingTics);
    }

    [Fact]
    public void FlagDoesNotPersistIntoLaterOrdinaryChaseCalls()
    {
        var (sim, actor) = Room(true);
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.NightmareFast, false, false);
        Assert.Equal(5, actor.States.RemainingTics);
        actor.Brain.Tick(sim, actor, useMelee: false, useMissile: false);
        Assert.Equal(5, actor.States.RemainingTics);
    }

    [Fact]
    public void UnsupportedFlagStillFailsBeforeTimingMutation()
    {
        var (sim, actor) = Room(true);
        Assert.Throws<NotSupportedException>(() => actor.Brain!.Tick(sim, actor, (MonsterChaseFlags)5));
        Assert.Equal(10, actor.States.RemainingTics);
    }

    private static (AuthoritySimulation, Actor) Room(bool fast)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [] },
            spawnOptions: new SpawnOptions(FastMonsters: fast));
        var actor = new Actor { Simulation = sim, Brain = new MonsterBrain(MonsterAttack.Melee) };
        actor.States.Configure(actor, [new(10, 0)], 0);
        return (sim, actor);
    }
}
