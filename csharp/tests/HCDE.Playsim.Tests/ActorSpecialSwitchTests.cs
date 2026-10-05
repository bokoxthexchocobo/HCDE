using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSpecialSwitchTests
{
    [Theory]
    [InlineData(256, true, 0)]
    [InlineData(512, false, 0)]
    [InlineData(1024, true, 1536)]
    [InlineData(1280, true, 1536)]
    [InlineData(1536, false, 1280)]
    [InlineData(1792, true, 1536)]
    public void NonDeathActivationConsumesStateFlags(int flags, bool activate, int remaining)
    {
        var sim = Room(); var thing = sim.Actors[1]; thing.Dormant = activate; thing.ActivationType = flags;
        Assert.True(ActorSpecialActions.ActivateSpecial(sim, thing, sim.Players.Single()));
        Assert.Equal(!activate, thing.Dormant);
        Assert.Equal(remaining, thing.ActivationType);
    }

    [Fact]
    public void SwitchAlternatesOnRepeatedActivation()
    {
        var sim = Room(); var thing = sim.Actors[1]; thing.Dormant = true; thing.ActivationType = 1024;
        ActorSpecialActions.ActivateSpecial(sim, thing, null); Assert.False(thing.Dormant);
        ActorSpecialActions.ActivateSpecial(sim, thing, null); Assert.True(thing.Dormant);
        ActorSpecialActions.ActivateSpecial(sim, thing, null); Assert.False(thing.Dormant);
    }

    [Theory]
    [InlineData(false, true, 19)]
    [InlineData(true, true, 0)]
    [InlineData(true, false, 19)]
    public void NonDeathSpecialUsesTriggerAndClearsOnlyOnSuccess(bool clear, bool triggerPresent, int remaining)
    {
        var sim = Room(); var thing = sim.Actors[1]; var player = sim.Players.Single();
        thing.Special = 19; thing.ActivationType = clear ? 32 : 0;
        player.VelocityX = Fixed.FromInt(8);
        Assert.Equal(triggerPresent, ActorSpecialActions.ActivateSpecial(sim, thing, triggerPresent ? player : null));
        Assert.Equal(remaining, thing.Special);
        Assert.Equal(triggerPresent ? 0 : 8, player.VelocityX.ToDouble());
    }

    [Fact]
    public void DeathDoesNotConsumeStateFlags()
    {
        var sim = Room(); var thing = sim.Actors[1]; thing.ActivationType = 1536;
        Assert.False(ActorSpecialActions.ActivateSpecial(sim, thing, null, true));
        Assert.Equal(1536, thing.ActivationType); Assert.False(thing.Dormant);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        ActivateOwnDeathSpecials = true, Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
    });
}
