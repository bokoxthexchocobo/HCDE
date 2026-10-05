using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorActivateSpecialMethodTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ActorMethodForwardsDeathAndActivator(bool death)
    {
        var sim = Room(); var actor = sim.Actors[0]; actor.Special = 19;
        actor.VelocityX = Fixed.FromInt(8);
        Assert.True(actor.ActivateSpecial(actor, death));
        Assert.Equal(0, actor.VelocityX.ToDouble());
        Assert.Equal(death ? 0 : 19, actor.Special);
    }

    [Fact]
    public void StateFrameCanActivateOwningActorSpecial()
    {
        var sim = Room(); var actor = sim.Actors[0]; actor.Special = 112;
        actor.SpecialArgs[0] = 7; actor.SpecialArgs[1] = 35;
        actor.States.Configure(actor, [new ActorFrame(-1, 0, self => Assert.True(self.ActivateSpecial(null)))], 0);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void UnboundActorReportsMissingSimulation()
    {
        Assert.Throws<InvalidOperationException>(() => new Actor().ActivateSpecial(null));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }],
    });
}
