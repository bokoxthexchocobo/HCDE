using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerSoundSlotSelectionTests
{
    [Theory]
    [InlineData(5, -1, "*wimpydeath-Fire", 2)]
    [InlineData(10, -50, "*crazydeath-Fire", 2)]
    [InlineData(10, -100, "*xdeath-Fire", 2)]
    [InlineData(10, -100, "*gibbed-Fire", 4)]
    [InlineData(10, -1, "*death-Fire", 2)]
    public void DeathSelectsDamageSpecificPlayerSlot(int damage, int health, string slot, int channel)
    {
        var sim = Room(slot); var actor = sim.Players.Single();
        actor.LastDamageType = "Fire"; actor.DeathDamageAmount = damage;
        actor.Health = health;
        var request = sim.DrainSoundRequests().Last();
        Assert.Equal(slot, request.Sound); Assert.Equal(channel, request.Channel);
    }

    [Theory]
    [InlineData("*pain100-Fire")]
    [InlineData("*pain-Fire")]
    public void PainSelectsPlayerSlotAcrossFreshSave(string slot)
    {
        var sim = Room(slot); var actor = sim.Players.Single(); actor.LastDamageType = "Fire";
        var saved = SimSavegame.Write(sim, true); var fresh = Room(slot); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        ActorSoundActions.Pain(fresh.Players.Single());
        Assert.Equal(slot, Assert.Single(fresh.DrainSoundRequests()).Sound);
    }

    private static AuthoritySimulation Room(string slot) => AuthoritySimulation.Start(new PlayLevel {
        Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 128 }],
        SoundDefinitions = new Dictionary<string, string> { ["resolved"] = "SOUND" },
        PlayerSounds = [new("player", 0, slot, "resolved")],
    });
}
