using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ThingActivationSwitchFlagTests
{
    [Theory]
    [InlineData(true, 256, 0)]
    [InlineData(false, 512, 0)]
    [InlineData(true, 1280, 1536)]
    [InlineData(false, 1536, 1280)]
    [InlineData(true, 1536, 1536)]
    [InlineData(false, 1280, 1280)]
    public void DirectThingActionSynchronizesOnlyMatchingActivationFlag(bool activate, int flags, int remaining)
    {
        var sim = Room(); var actor = sim.Actors[0]; actor.ActivationType = flags; actor.Dormant = activate;
        Assert.True(ThingActivation.Execute(sim, actor, 0, activate));
        Assert.Equal(remaining, actor.ActivationType); Assert.Equal(!activate, actor.Dormant);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DirectActionAndSpecialSwitchAlternateConsistently(bool activate)
    {
        var sim = Room(); var actor = sim.Actors[0]; actor.ActivationType = activate ? 1280 : 1536;
        actor.Dormant = activate;
        Assert.True(ThingActivation.Execute(sim, null, 7, activate));
        Assert.True(actor.ActivateSpecial(null));
        Assert.Equal(activate, actor.Dormant);
        Assert.Equal(activate ? 1280 : 1536, actor.ActivationType);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004, Id = 7 }],
    });
}
