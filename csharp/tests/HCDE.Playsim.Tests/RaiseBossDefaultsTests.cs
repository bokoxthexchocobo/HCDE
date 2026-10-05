using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseBossDefaultsTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void RevivalRestoresNonBossBlastEligibility(bool archvile, bool blocked)
    {
        var sim = Room();
        var corpse = sim.Actors[1];
        corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.Boss = true;
        if (blocked) sim.Ceilings[0] = 10;
        Assert.Equal(!blocked, archvile ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 0) == true);
        Assert.Equal(blocked, corpse.Boss);
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

    [Fact]
    public void NativeBossClassesCaptureBossDefaultBeforeMapOverrides()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 7 }, new LevelThing { Type = 16, X = 200 }],
        });
        foreach (var actor in sim.Actors)
        {
            Assert.True(actor.Boss);
            Assert.Equal(32, actor.ResurrectionDefenseFlags!.Value & 32);
        }
        Assert.Equal(32, sim.AddBot(400, 0, 7).ResurrectionDefenseFlags!.Value & 32);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
    });
}
