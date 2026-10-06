using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LookSeeStateTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AcquisitionEntersAvailableSeeState(bool remembered)
    {
        var (sim, hunter, target) = Room(remembered);
        hunter.SeeState = hunter.DeathState;
        hunter.Brain!.Tick(sim, hunter);
        Assert.Equal(target.Id, hunter.Brain.TargetId);
        Assert.Equal(hunter.SeeState, hunter.States.Current);
    }

    [Fact]
    public void AcquisitionWithoutSeeLabelKeepsSpawnState()
    {
        var (sim, hunter, target) = Room(false);
        hunter.SeeState = -1;
        hunter.Brain!.Tick(sim, hunter);
        Assert.Equal(target.Id, hunter.Brain.TargetId);
        Assert.Equal(hunter.SpawnState, hunter.States.Current);
    }

    [Fact]
    public void ActivePainStatePreventsLookTransition()
    {
        var (sim, hunter, _) = Room(false);
        hunter.SeeState = hunter.DeathState;
        hunter.States.Enter(hunter, hunter.PainState);
        hunter.Brain!.Tick(sim, hunter);
        Assert.Equal(hunter.PainState, hunter.States.Current);
        Assert.Equal(MonsterMode.Pain, hunter.Brain.Mode);
    }

    private static (AuthoritySimulation, Actor, Actor) Room(bool remembered)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 },
                new LevelThing { Type = remembered ? 3001 : 1, X = 200 }],
        });
        var hunter = sim.Actors[0]; var target = sim.Actors[1];
        hunter.ReactionTime = 0;
        if (remembered) hunter.Brain!.RestoreTargetMemory(new SimTargetMemory(null, target.Id, null));
        return (sim, hunter, target);
    }
}
