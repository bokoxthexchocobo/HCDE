using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MissileCombatFlagTests
{
    [Theory]
    [InlineData(3004)]
    [InlineData(3001)]
    [InlineData(3005)]
    public void MissileEntrySetsBothFlagsAndDoesNotInterruptWindup(int type)
    {
        var sim = Room(type); var actor = Hunter(sim); actor.ReactionTime = 0;
        actor.Brain!.Tick(sim, actor); Assert.True(actor.JustAttacked); Assert.True(actor.InCombat);
        Assert.Equal(MonsterMode.Windup, actor.Brain.Mode);
        actor.Brain.Tick(sim, actor); Assert.True(actor.JustAttacked);
        for (var i = 0; i < 30; i++) actor.Brain.Tick(sim, actor);
        Assert.True(actor.InCombat);
    }

    [Fact]
    public void MeleeEntryDoesNotProduceMissileFlags()
    {
        var sim = Room(3002, 40); var actor = Hunter(sim); actor.ReactionTime = 0;
        actor.Brain!.Tick(sim, actor); Assert.Equal(MonsterMode.Windup, actor.Brain.Mode);
        Assert.False(actor.JustAttacked); Assert.False(actor.InCombat);
    }

    [Fact]
    public void GenericMissileTimelineKeepsGuardUntilAttackCompletes()
    {
        var sim = Room(3004); var actor = Hunter(sim); actor.Brain = new MonsterBrain(MonsterAttack.Fireball);
        actor.ReactionTime = 0; actor.Brain.Tick(sim, actor);
        Assert.True(actor.JustAttacked);
        for (var i = 0; i < 8; i++) actor.Brain.Tick(sim, actor);
        Assert.NotEmpty(sim.Actors.OfType<ProjectileActor>()); Assert.True(actor.JustAttacked);
        actor.Brain.Tick(sim, actor); Assert.False(actor.JustAttacked); Assert.True(actor.InCombat);
    }

    [Fact]
    public void MissileFlagsAndTimelineRoundTrip()
    {
        var sim = Room(3001); var actor = Hunter(sim); actor.ReactionTime = 0; actor.Brain!.Tick(sim, actor);
        var saved = SimSavegame.Write(sim, true); actor.Brain.Tick(sim, actor); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); actor.Brain.Tick(sim, actor); Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static Actor Hunter(AuthoritySimulation sim) => sim.Actors.Single(a => a is not PlayerPawn);
    private static AuthoritySimulation Room(int type, int distance = 300) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = type }, new LevelThing { Type = 1, X = distance }]
    });
}
