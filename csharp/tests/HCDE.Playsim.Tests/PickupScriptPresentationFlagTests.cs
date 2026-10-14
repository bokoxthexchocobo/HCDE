using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PickupScriptPresentationFlagTests
{
    [Theory]
    [InlineData("QUIET", 0, 0, 1)]
    [InlineData("inventory.quiet", 0, 0, 1)]
    [InlineData("NOSCREENFLASH", 0, 1, 1)]
    [InlineData("Inventory.NoScreenFlash", 0, 1, 1)]
    [InlineData("NOATTENPICKUPSOUND", 6, 1, 0)]
    [InlineData("Inventory.NoAttenPickupSound", 6, 1, 0)]
    public void ScriptChangeControlsContactAndSurvivesRestore(string flag, int flash, int sounds, int attenuation)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = PickupCatalog.HealthBonus }]
        });
        sim.DrainSoundRequests();
        var item = sim.Actors.Single(a => a.DoomEdNum == PickupCatalog.HealthBonus);
        ActorPropertyActions.ChangeFlag(item, flag, true);
        Assert.True(AcsActorFlags.TryGet(item, flag, out var enabled));
        Assert.True(enabled);
        var saved = SimSavegame.Write(sim, true);
        ActorPropertyActions.ChangeFlag(item, flag, false);
        Assert.True(AcsActorFlags.TryGet(item, flag, out enabled));
        Assert.False(enabled);
        SimSavegame.Apply(sim, saved);
        sim.Tick();
        Assert.Equal(101, sim.Players.Single().Health);
        Assert.Equal(flash, sim.Players.Single().BonusCount);
        var requests = sim.DrainSoundRequests();
        Assert.Equal(sounds, requests.Length);
        if (sounds != 0) Assert.Equal(attenuation, Assert.Single(requests).Attenuation);
    }

    [Theory]
    [InlineData("QUIET")]
    [InlineData("NOSCREENFLASH")]
    [InlineData("NOATTENPICKUPSOUND")]
    public void InventoryFlagsRejectWrongClassesAndQualifiers(string flag)
    {
        var item = new Actor { DoomEdNum = PickupCatalog.HealthBonus };
        foreach (var name in new[] { "Actor." + flag, "Actor.Inventory." + flag, "Monster." + flag })
        {
            Assert.False(AcsActorFlags.TrySet(item, name, true));
            Assert.False(AcsActorFlags.TryGet(item, name, out _));
        }
        Assert.False(item.PickupQuiet);
        Assert.False(item.NoScreenFlash);
        Assert.False(item.NoAttenPickupSound);
        Assert.False(AcsActorFlags.TrySet(new PlayerPawn(), flag, true));
        Assert.False(AcsActorFlags.TryGet(new Actor(), "Inventory." + flag, out _));
        item.Destroy();
        Assert.False(AcsActorFlags.TrySet(item, flag, true));
        Assert.False(AcsActorFlags.TryGet(item, flag, out _));
    }
}
