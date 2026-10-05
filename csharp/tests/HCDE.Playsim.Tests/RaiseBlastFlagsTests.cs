using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseBlastFlagsTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void SuccessfulRaiseRestoresBlastTransferEligibility(bool archvile, bool blocked)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
        });
        var corpse = sim.Actors[1];
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.DontBlast = corpse.Blasted = true;
        if (blocked) sim.Ceilings[0] = 10;
        Assert.Equal(!blocked, archvile
            ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 0) == true);
        Assert.Equal(blocked, corpse.DontBlast);
        Assert.Equal(blocked, corpse.Blasted);
        if (!blocked)
        {
            var source = sim.AddBot(-30, 0);
            source.Brain = corpse.Brain = null;
            source.Blasted = true;
            source.VelocityX = Fixed.FromInt(3);
            Assert.False(ActorPhysics.TryMove(sim, source, -10, 0, out _));
            Assert.Equal(Fixed.FromInt(3), corpse.VelocityX);
        }
    }
}
