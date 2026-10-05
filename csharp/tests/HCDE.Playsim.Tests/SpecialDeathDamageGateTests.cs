using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SpecialDeathDamageGateTests
{
    [Theory]
    [InlineData("Fire", false, 20)]
    [InlineData("Normal", false, 0)]
    [InlineData("Ice", false, 20)]
    [InlineData("Ice", true, 0)]
    [InlineData("Massacre", true, 20)]
    public void TypedOnlyActorFiltersDamageByAvailableDeath(string type, bool noFreeze, int lost)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
            dehacked: DehackedPatch.Apply($"Misc 0\nNo Autofreeze = {(noFreeze ? 1 : 0)}\n"));
        var player = sim.Players.Single();
        player.States.Configure(player, [new(-1, 0), new(-1, 1), new(-1, 2)], 0);
        player.DeathState = -1; player.SetTypedDeath("Fire", 1); player.GenericFreezeDeath = 2;
        Assert.Equal(lost, ActorDamage.Apply(player, 20, damageType: type).HealthLost);
    }

    [Fact]
    public void ForcedDamageBypassesSpecialDeathGate()
    {
        var actor = new Actor { Health = 100, DeathState = -1 };
        actor.SetTypedDeath("Fire", 2);
        Assert.Equal(20, ActorDamage.Apply(actor, 20, damageType: "Normal", flags: DamageFlags.Forced).HealthLost);
    }

    [Fact]
    public void OrdinaryDeathStateKeepsAcceptingOtherDamageTypes()
    {
        var actor = new Actor { Health = 100 }; actor.SetTypedDeath("Fire", 2);
        Assert.Equal(20, ActorDamage.Apply(actor, 20, damageType: "Normal").HealthLost);
    }
}
