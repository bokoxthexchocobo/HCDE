using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SkullChargePainGateTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void PainReactionRequiresChargeToBeInactiveEvenWhenForced(bool charging, bool forcePain)
    {
        var sim = Room(); var soul = Soul(sim);
        soul.PainChance = forcePain ? 0 : 256;
        if (charging) soul.Brain!.StartCharge(soul, sim.Players.Single());
        var health = soul.Health;
        ActorDamage.Apply(soul, 1, inflictor: new Actor { ForcePain = forcePain });
        Assert.Equal(health - 1, soul.Health);
        Assert.Equal(!charging, soul.States.Current == soul.PainState);
        Assert.Equal(charging, soul.Brain!.Charging);
    }

    [Fact]
    public void SuppressedChargePainDoesNotConsumePainRandomRoll()
    {
        var sim = Room(); var soul = Soul(sim);
        soul.PainChance = 128;
        soul.Brain!.StartCharge(soul, sim.Players.Single());
        var randomState = sim.CombatRandomState;
        ActorDamage.Apply(soul, 1);
        Assert.Equal(randomState, sim.CombatRandomState);
        Assert.NotEqual(soul.PainState, soul.States.Current);
        soul.Brain.StopCharge(soul);
        ActorDamage.Apply(soul, 1);
        Assert.NotEqual(randomState, sim.CombatRandomState);
    }

    private static Actor Soul(AuthoritySimulation sim) => sim.Actors.Single(a => a.DoomEdNum == 3006);
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }],
        Things = [new LevelThing { Type = 1, X = 800 }, new LevelThing { Type = 3006, Z = 100 }],
    });
}
