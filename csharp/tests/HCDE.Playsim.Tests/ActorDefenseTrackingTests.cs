using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorDefenseTrackingTests
{
    [Theory]
    [InlineData("INVULNERABLE", false)]
    [InlineData("INVULNERABLE", true)]
    [InlineData("SHOOTABLE", false)]
    [InlineData("SHOOTABLE", true)]
    [InlineData("NONSHOOTABLE", false)]
    [InlineData("NONSHOOTABLE", true)]
    public void AcsFlagsRoundTripExplicitValues(string flag, bool value)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        Assert.True(AcsActorFlags.TrySet(actor, flag, value));
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        Assert.True(AcsActorFlags.TrySet(actor, flag, !value)); sim.RestoreState(state);
        Assert.True(AcsActorFlags.TryGet(actor, flag, out var actual)); Assert.Equal(value, actual);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(AcsActorProperties.Mass, 100)]
    [InlineData(AcsActorProperties.Invulnerable, 0)]
    public void AcsPropertyResetIsRetained(int property, int value)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        AcsActorProperties.Set(sim, actor, 0, property, value);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        AcsActorProperties.Set(sim, actor, 0, property, 9); sim.RestoreState(state);
        Assert.Equal(value, AcsActorProperties.Get(sim, actor, 0, property));
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void DirectChangesOfSpawnFlagsAndMassRestore(int property)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        if (property == 0) actor.Mass = -5;
        else if (property == 1) actor.Shootable = false;
        else actor.Invulnerable = true;
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.Mass = 100; actor.Shootable = true; actor.Invulnerable = false; sim.RestoreState(state);
        Assert.Equal(property == 0 ? -5 : 100, actor.Mass);
        Assert.Equal(property != 1, actor.Shootable); Assert.Equal(property == 2, actor.Invulnerable);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
