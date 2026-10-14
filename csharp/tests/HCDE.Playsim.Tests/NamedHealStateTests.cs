using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NamedHealStateTests
{
    [Theory]
    [InlineData("Heal")]
    [InlineData("hEaL")]
    public void NamedHealCanBeFoundByStateLookup(string name)
    {
        var actor = new Actor { HealState = ActorStateMachine.Corpse };
        Assert.True(AcsActorStates.TryFindNamedState(actor, name, true, out var state));
        Assert.Equal(ActorStateMachine.Corpse, state);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1000)]
    public void MissingOrInvalidHealLabelDoesNotChangeRaiserState(int state)
    {
        var sim = Room(); Ready(sim); var raiser = new Actor { HealState = state };
        Assert.True(ArchvileActions.TryRaise(sim, raiser, sim.Players.Single()));
        Assert.Equal(ActorStateMachine.Spawn, raiser.States.Current);
    }

    [Fact]
    public void NamedHealRunsBeforeCorpseRevivalWhenNoOverrideIsPassed()
    {
        var sim = Room(); var corpse = Ready(sim); bool? dead = null;
        var raiser = new Actor { HealState = 1 };
        raiser.States.Configure(raiser, [new(-1, 0), new(-1, 1, _ => dead = corpse.IsDead)], 0);
        Assert.True(ArchvileActions.TryRaise(sim, raiser, sim.Players.Single()));
        Assert.True(dead); Assert.Equal(1, raiser.States.Current); Assert.False(corpse.IsDead);
    }

    [Fact]
    public void ExplicitStateTakesPrecedenceOverNamedHeal()
    {
        var sim = Room(); Ready(sim); var raiser = new Actor { HealState = ActorStateMachine.Pain };
        Assert.True(ArchvileActions.TryRaise(sim, raiser, sim.Players.Single(), ActorStateMachine.Corpse));
        Assert.Equal(ActorStateMachine.Corpse, raiser.States.Current);
    }

    [Fact]
    public void HealMetadataParticipatesInChecksum()
    {
        var sim = Room(); var other = Room(); other.Players.Single().HealState = ActorStateMachine.Corpse;
        sim.Tick(); other.Tick(); Assert.NotEqual(sim.Checksum, other.Checksum);
    }

    private static Actor Ready(AuthoritySimulation sim)
    {
        var actor = sim.Actors.Single(a => a.IsMonster); actor.Health = 0;
        actor.States.Enter(actor, ActorStateMachine.Corpse); return actor;
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }]
    });
}
