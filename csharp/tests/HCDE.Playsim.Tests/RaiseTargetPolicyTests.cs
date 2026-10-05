using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseTargetPolicyTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void RevivalResetsTemporaryTargetPolicy(bool archvile, bool blocked)
    {
        var sim = Room(); var corpse = sim.Actors[1];
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.NoTargetSwitch = corpse.QuickToRetaliate = corpse.NoHatePlayers = true;
        if (blocked) sim.Ceilings[0] = 10;
        Assert.Equal(!blocked, archvile
            ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 0) == true);
        Assert.Equal(blocked, corpse.NoTargetSwitch);
        Assert.Equal(blocked, corpse.QuickToRetaliate);
        Assert.Equal(blocked, corpse.NoHatePlayers);
        if (!blocked)
        {
            ActorDamage.Apply(corpse, 1, sim.Players.Single());
            Assert.Equal(sim.Players.Single().Id, corpse.Brain!.TargetId);
        }
    }

    [Fact]
    public void ArchvilePlayerHatePolicyIsCopiedAfterDefaultReset()
    {
        var sim = Room(); var corpse = sim.Actors[1];
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        Assert.True(ArchvileActions.TryRaise(sim, new Actor { NoHatePlayers = true }, sim.Players.Single()));
        Assert.True(corpse.NoHatePlayers);
        ActorDamage.Apply(corpse, 1, sim.Players.Single());
        Assert.Null(corpse.Brain!.TargetId);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
    });
}
