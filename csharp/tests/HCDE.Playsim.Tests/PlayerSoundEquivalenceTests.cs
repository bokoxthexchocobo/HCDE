using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerSoundEquivalenceTests
{
    [Theory]
    [InlineData("player", false)]
    [InlineData("custom", true)]
    [InlineData("CUSTOM", true)]
    public void PlayerSoundClassDeterminesSlotEquivalenceAcrossFreshSave(string soundClass, bool same)
    {
        var sim = Room(); var actor = sim.Players.Single(); actor.SoundClass = soundClass;
        ActorSoundActions.StartSoundIfNotSame(actor, "landAlias", "*grunt");
        Assert.Equal(same ? 0 : 1, sim.DrainSoundRequests().Length);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        ActorSoundActions.StartSoundIfNotSame(fresh.Players.Single(), "*land", "*grunt");
        Assert.Equal(same ? 0 : 1, fresh.DrainSoundRequests().Length);
    }

    [Fact]
    public void DifferentRandomHeadersRemainDifferentWithoutSelectingMembers()
    {
        var sim = Room(); var actor = sim.Players.Single(); actor.SoundClass = "custom";
        sim.Level.PlayerSounds = [new("custom", 0, "*land", "groupA"), new("custom", 0, "*grunt", "groupB")];
        sim.Level.RandomSoundGroups = new Dictionary<string, IReadOnlyList<string>> { ["groupA"] = ["shared"], ["groupB"] = ["shared"] };
        ActorSoundActions.StartSoundIfNotSame(actor, "*land", "*grunt");
        Assert.Equal("*land", Assert.Single(sim.DrainSoundRequests()).Sound);
    }

    [Fact]
    public void PlayerAliasCycleFailsWithoutQueuingPlayback()
    {
        var sim = Room(); var actor = sim.Players.Single();
        sim.Level.PlayerSounds = [new("player", 0, "*land", "*grunt"), new("player", 0, "*grunt", "*land")];
        Assert.Throws<InvalidOperationException>(() => ActorSoundActions.StartSoundIfNotSame(actor, "*land", "shared"));
        Assert.Empty(sim.DrainSoundRequests());
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 128 }],
        SoundDefinitions = new Dictionary<string, string> { ["land"] = "LAND", ["grunt"] = "GRUNT", ["shared"] = "SHARED" },
        SoundAliases = new Dictionary<string, string> { ["landAlias"] = "*land" },
        PlayerSounds = [new("player", 0, "*land", "land"), new("player", 0, "*grunt", "grunt"),
            new("custom", 0, "*land", "shared"), new("custom", 0, "*grunt", "shared")],
    });
}
