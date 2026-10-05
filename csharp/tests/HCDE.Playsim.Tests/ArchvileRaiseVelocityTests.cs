using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ArchvileRaiseVelocityTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NearbyAttemptClearsHorizontalAndPreservesVerticalVelocity(bool blocked)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
        });
        var corpse = sim.Actors[1];
        corpse.Health = 0; corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.VelocityX = Fixed.FromInt(3); corpse.VelocityY = Fixed.FromInt(4);
        corpse.VelocityZ = Fixed.FromInt(2);
        var archvile = new Actor { X = Fixed.FromInt(-20), Radius = Fixed.FromInt(20) };
        if (blocked) sim.Players.Single().X = corpse.X;
        Assert.Equal(!blocked, ArchvileActions.TryRaise(sim, archvile, sim.Players.Single()));
        Assert.Equal(default(Fixed), corpse.VelocityX);
        Assert.Equal(default(Fixed), corpse.VelocityY);
        Assert.Equal(Fixed.FromInt(2), corpse.VelocityZ);
    }
}
