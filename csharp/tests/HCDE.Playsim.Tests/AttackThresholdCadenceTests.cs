using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AttackThresholdCadenceTests
{
    [Theory]
    [InlineData(3004)]
    [InlineData(3001)]
    [InlineData(66)]
    public void ProfileWindupAndFiringPreserveThreshold(int type)
    {
        var (sim, actor) = Room(type); actor.Brain!.Tick(sim, actor);
        Assert.Equal(9, actor.Brain.Threshold); Assert.Equal(MonsterMode.Windup, actor.Brain.Mode);
        for (var i = 0; i < 6; i++) actor.Brain.Tick(sim, actor);
        Assert.Equal(9, actor.Brain.Threshold);
    }

    [Fact]
    public void GenericAttackPausesThenChaseResumesCountdown()
    {
        var (sim, actor) = Room(3004); actor.Brain = new MonsterBrain(MonsterAttack.Fireball);
        actor.SetTargetPointer(sim.Players.Single());
        actor.Brain.SetChaseThreshold(10, false); actor.ReactionTime = 0;
        actor.Brain.Tick(sim, actor); Assert.Equal(9, actor.Brain.Threshold);
        for (var i = 0; i < 8; i++) actor.Brain.Tick(sim, actor);
        Assert.Equal(9, actor.Brain.Threshold); Assert.NotEmpty(sim.Actors.OfType<ProjectileActor>());
        actor.Brain.Tick(sim, actor); Assert.Equal(8, actor.Brain.Threshold);
    }

    [Fact]
    public void SavedAttackPreservesPausedThreshold()
    {
        var (sim, actor) = Room(3001); actor.Brain!.Tick(sim, actor);
        var saved = SimSavegame.Write(sim, true); actor.Brain.Tick(sim, actor); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); actor.Brain.Tick(sim, actor);
        Assert.Equal(9, actor.Brain.Threshold); Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static (AuthoritySimulation Sim, Actor Actor) Room(int type)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = type }, new LevelThing { Type = 1, X = 300 }] });
        var actor = sim.Actors.Single(a => a.ClassDoomEdNum == type); actor.SetTargetPointer(sim.Players.Single());
        actor.Brain!.SetChaseThreshold(10, false); actor.ReactionTime = 0; return (sim, actor);
    }
}
