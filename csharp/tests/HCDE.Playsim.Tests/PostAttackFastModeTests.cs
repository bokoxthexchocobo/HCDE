using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PostAttackFastModeTests
{
    [Theory]
    [InlineData(false, false, false, true)]
    [InlineData(true, false, false, false)]
    [InlineData(false, true, false, false)]
    [InlineData(true, true, false, false)]
    [InlineData(true, false, true, true)]
    [InlineData(false, false, true, true)]
    [InlineData(true, true, true, false)]
    [InlineData(false, true, true, false)]
    public void EffectiveActorFastModeControlsMovement(bool levelFast, bool alwaysFast, bool neverFast, bool moves)
    {
        var (sim, actor) = Room(levelFast); actor.AlwaysFast = alwaysFast; actor.NeverFast = neverFast;
        actor.JustAttacked = true;
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.NoPlayActive);
        Assert.False(actor.JustAttacked); Assert.Equal(moves, actor.VelocityX.Raw > 0);
        Assert.Equal(0, actor.Brain.WindupTics);
    }

    [Fact]
    public void SavedNeverFastOverrideReplaysPostAttackMovement()
    {
        var (sim, actor) = Room(true); actor.NeverFast = true; actor.JustAttacked = true;
        var saved = SimSavegame.Write(sim, true); actor.Brain!.Tick(sim, actor); var expected = SimSavegame.Write(sim, true);
        actor.NeverFast = false; SimSavegame.Apply(sim, saved); actor.Brain.Tick(sim, actor);
        Assert.True(actor.VelocityX.Raw > 0); Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static (AuthoritySimulation Sim, Actor Actor) Room(bool fast)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 1, X = 300 }] },
            spawnOptions: new SpawnOptions(FastMonsters: fast));
        var actor = sim.Actors.Single(a => a.ClassDoomEdNum == 3004); actor.SetTargetPointer(sim.Players.Single());
        return (sim, actor);
    }
}
