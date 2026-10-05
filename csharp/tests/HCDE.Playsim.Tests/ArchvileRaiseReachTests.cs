using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ArchvileRaiseReachTests
{
    [Theory]
    [InlineData(50, 0, true)]
    [InlineData(100, 100, false)]
    public void ReachUsesOriginalCorpseRadius(int position, int currentRadius, bool raised)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = position }],
        });
        var corpse = sim.Actors[1];
        corpse.Health = 0; corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.Radius = Fixed.FromInt(currentRadius);
        Assert.Equal(raised, ArchvileActions.TryRaise(sim,
            new Actor { Radius = Fixed.FromInt(20), MovementSpeed = Fixed.FromInt(15) }, sim.Players.Single()));
        Assert.Equal(!raised, corpse.IsDead);
    }
}
