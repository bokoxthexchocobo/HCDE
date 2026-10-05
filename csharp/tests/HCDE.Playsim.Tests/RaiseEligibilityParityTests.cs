using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseEligibilityParityTests
{
    [Fact]
    public void EmptyCanRaiseQuerySucceeds()
    {
        var sim = Room();
        Assert.True(ActorRaise.CanRaiseAll(sim, 0, null));
        Assert.True(ActorRaise.CanRaiseAll(sim, 999, null));
        Assert.False(ActorRaise.CanRaiseAll(sim, 0, sim.Players.Single()));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(2, true)]
    public void PositionBypassUsesBitTwoAndFailedCheckClearsHorizontalVelocity(int flags, bool expected)
    {
        var sim = Room(); var corpse = sim.Actors[1];
        corpse.Health = 0; corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        corpse.X = sim.Players.Single().X; corpse.Y = sim.Players.Single().Y;
        corpse.VelocityX = Fixed.FromInt(3); corpse.VelocityZ = Fixed.FromInt(2);
        Assert.False(ActorRaise.CanRaise(sim, corpse));
        Assert.Equal(expected, ThingRaise.Execute(sim, 17, corpse, 0, flags));
        Assert.Equal(default(Fixed), corpse.VelocityX);
        Assert.Equal(Fixed.FromInt(2), corpse.VelocityZ);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, X = 100 }],
    });
}
