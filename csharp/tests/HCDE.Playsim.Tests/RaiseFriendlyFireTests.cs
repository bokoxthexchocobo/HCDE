using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseFriendlyFireTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void SuccessfulRaiseRemovesTemporaryFriendlyFirePermission(bool archvile, bool blocked)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
        });
        sim.Infighting = 0;
        var corpse = sim.Actors[1];
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.HarmFriends = true;
        if (blocked) sim.Ceilings[0] = 10;
        Assert.Equal(!blocked, archvile
            ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 0) == true);
        Assert.Equal(blocked, corpse.HarmFriends);
        if (!blocked)
        {
            var ally = sim.AddBot(200, 0, 3001);
            corpse.Friendly = ally.Friendly = true;
            corpse.FriendPlayer = ally.FriendPlayer = 1;
            var health = ally.Health;
            Assert.Equal(0, ActorDamage.Apply(ally, 10, corpse, inflictor: corpse).HealthLost);
            Assert.Equal(health, ally.Health);
            corpse.HarmFriends = true;
            Assert.Equal(10, ActorDamage.Apply(ally, 10, corpse, inflictor: corpse).HealthLost);
        }
    }
}
