using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ThingRaiseTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    public void SupportedCorpseReturnsToRaiseLifecycle(int tid)
    {
        var sim = Room(); var corpse = sim.Actors[1];
        corpse.Health = 0; corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.VelocityX = Fixed.FromInt(3);
        Assert.True(ThingRaise.Execute(sim, 17, corpse, tid, 2));
        Assert.Equal(corpse.ResurrectionHealth, corpse.Health);
        Assert.True(corpse.Solid && corpse.Shootable);
        Assert.Equal(default(Fixed), corpse.VelocityX);
        Assert.False(ThingRaise.Execute(sim, 17, corpse, tid, 2));
    }

    [Fact]
    public void MissingTargetsAndPlayerDoNotRaise()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = 0;
        Assert.False(ThingRaise.Execute(sim, 17, player, 0, 2));
        Assert.False(ThingRaise.Execute(sim, 17, null, 999, 2));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, Id = 7, X = 100 }],
    });
}
