using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AttackReactionCadenceTests
{
    [Theory]
    [InlineData(3004)]
    [InlineData(3001)]
    [InlineData(66)]
    public void ProfileAttackPreservesReactionUntilChaseResumes(int type)
    {
        var (sim, actor) = Room(type); actor.Brain!.Tick(sim, actor); actor.ReactionTime = 17;
        for (var i = 0; i < 80 && actor.Brain.AttackCooldown != 1; i++) actor.Brain.Tick(sim, actor);
        Assert.Equal(1, actor.Brain.AttackCooldown); Assert.Equal(17, actor.ReactionTime);
        actor.Brain.Tick(sim, actor); Assert.Equal(16, actor.ReactionTime);
    }

    [Theory]
    [InlineData(17)]
    [InlineData(-1)]
    public void GenericWindupAndFiringPreserveSignedReaction(int reaction)
    {
        var (sim, actor) = Room(3004); actor.Brain = new MonsterBrain(MonsterAttack.Fireball);
        actor.SetTargetPointer(sim.Players.Single()); actor.ReactionTime = 0;
        actor.Brain.Tick(sim, actor); actor.ReactionTime = reaction;
        for (var i = 0; i < 8; i++) actor.Brain.Tick(sim, actor);
        Assert.NotEmpty(sim.Actors.OfType<ProjectileActor>()); Assert.Equal(reaction, actor.ReactionTime);
        actor.Brain.Tick(sim, actor); Assert.Equal(reaction - 1, actor.ReactionTime);
    }

    [Fact]
    public void SavedAttackReplaysPreservedReaction()
    {
        var (sim, actor) = Room(3001); actor.Brain!.Tick(sim, actor); actor.ReactionTime = 17;
        var saved = SimSavegame.Write(sim, true); actor.Brain.Tick(sim, actor); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); actor.Brain.Tick(sim, actor);
        Assert.Equal(17, actor.ReactionTime); Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static (AuthoritySimulation Sim, Actor Actor) Room(int type)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = type }, new LevelThing { Type = 1, X = 300 }] });
        var actor = sim.Actors.Single(a => a.ClassDoomEdNum == type); actor.SetTargetPointer(sim.Players.Single());
        actor.ReactionTime = 0; return (sim, actor);
    }
}
