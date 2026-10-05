using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseBridgeFlagTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void SuccessfulRaiseRemovesTemporaryMonsterSupport(bool archvile, bool blocked)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
        });
        var corpse = sim.Actors[1];
        corpse.ActsLikeBridge = true;
        var rider = Rider(corpse);
        ActorPhysics.FitToSector(sim, rider);
        Assert.True(rider.OnMobj);
        Assert.Equal(corpse.Height, rider.Z);
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        if (blocked) sim.Ceilings[0] = 10;
        Assert.Equal(!blocked, archvile
            ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 0) == true);
        Assert.Equal(blocked, corpse.ActsLikeBridge);
        if (!blocked)
        {
            rider = Rider(corpse);
            ActorPhysics.FitToSector(sim, rider);
            Assert.False(rider.OnMobj);
            Assert.Equal(default(Fixed), rider.Z);
        }
    }

    private static Actor Rider(Actor support) => new()
    {
        X = support.X, Y = support.Y, SectorIndex = 0, OnGround = true,
        MaxStepHeight = Fixed.FromInt(64),
    };
}
