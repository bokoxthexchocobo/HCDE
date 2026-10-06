using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HearingSeeStateTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HeardEnemyEntersAvailableSeeState(bool ambush)
    {
        var (sim, hunter, target) = Room(); hunter.Ambush = ambush;
        hunter.Brain!.Hear(sim, hunter, target);
        Assert.Equal(target.Id, hunter.Brain.TargetId);
        Assert.Equal(hunter.SeeState, hunter.States.Current);
    }

    [Fact]
    public void MissingSeeStateStillRecordsHeardEnemy()
    {
        var (sim, hunter, target) = Room(); hunter.SeeState = -1;
        hunter.Brain!.Hear(sim, hunter, target);
        Assert.Equal(target.Id, hunter.Brain.TargetId);
        Assert.Equal(hunter.SpawnState, hunter.States.Current);
    }

    [Fact]
    public void HearingDoesNotInterruptExistingPainState()
    {
        var (sim, hunter, target) = Room();
        hunter.States.Enter(hunter, hunter.PainState);
        hunter.Brain!.Hear(sim, hunter, target);
        Assert.Equal(target.Id, hunter.Brain.TargetId);
        Assert.Equal(hunter.PainState, hunter.States.Current);
    }

    [Fact]
    public void ExistingTargetPreventsSoundWakeTransition()
    {
        var (sim, hunter, target) = Room(); target.ThingId = 7;
        hunter.Brain!.SetTargetThingId(sim, 7);
        hunter.Brain.Hear(sim, hunter, target);
        Assert.Equal(hunter.SpawnState, hunter.States.Current);
    }

    private static (AuthoritySimulation, Actor, Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 3001, X = 200 }],
        });
        var hunter = sim.Actors[0];
        // Supply an available non-Spawn destination for the fixture's missing See label.
        hunter.SeeState = hunter.DeathState;
        return (sim, hunter, sim.Actors[1]);
    }
}
