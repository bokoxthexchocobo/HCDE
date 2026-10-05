using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSpecialActivatorTests
{
    [Theory]
    [InlineData(1, false)]
    [InlineData(1, true)]
    [InlineData(129, false)]
    [InlineData(129, true)]
    public void ThingActsUsesDyingActorEvenWithTriggerActs(int activation, bool nullSource)
    {
        var sim = Room(); var player = sim.Players.Single(); var victim = sim.Actors[1];
        victim.ActivationType = activation;
        player.VelocityX = Fixed.FromInt(8); victim.VelocityX = Fixed.FromInt(6);
        ActorDamage.Apply(victim, 1000, nullSource ? null : player);
        Assert.Equal(8, player.VelocityX.ToDouble());
        Assert.Equal(0, victim.VelocityX.ToDouble());
        Assert.Equal(19, victim.Special);
    }

    [Theory]
    [InlineData(32, false, 0)]
    [InlineData(32, true, 19)]
    [InlineData(33, true, 0)]
    [InlineData(0, false, 19)]
    public void OriginalHexenClearsOnlySuccessfulClearSpecial(int activation, bool nullSource, int remainingSpecial)
    {
        var sim = Room(); var victim = sim.Actors[1]; victim.ActivationType = activation;
        ActorDamage.Apply(victim, 1000, nullSource ? null : sim.Players.Single());
        Assert.Equal(remainingSpecial, victim.Special);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        HexenHack = true, Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20, Special = 19 }],
    });
}
