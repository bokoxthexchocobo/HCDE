using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ArchvileRaiseSpeedTests
{
    [Theory]
    [InlineData(5, false)]
    [InlineData(30, true)]
    [InlineData(-30, true)]
    public void ProjectedSearchUsesAbsoluteActorSpeed(int speed, bool raised)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 50 }],
        });
        var corpse = sim.Actors[1]; corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        var archvile = new Actor { MovementSpeed = Fixed.FromInt(speed), Radius = Fixed.FromInt(20) };
        Assert.Equal(raised, ArchvileActions.TryRaise(sim, archvile, sim.Players.Single()));
    }
}
