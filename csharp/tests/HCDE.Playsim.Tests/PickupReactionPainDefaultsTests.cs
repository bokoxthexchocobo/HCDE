using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PickupReactionPainDefaultsTests
{
    [Theory]
    [InlineData(54, 2011, false)]
    [InlineData(54, 2011, true)]
    [InlineData(64, 2007, false)]
    [InlineData(64, 2007, true)]
    [InlineData(78, 2001, false)]
    [InlineData(78, 2001, true)]
    public void StockAndPropertyOnlyPatchInheritNativeReactionAndPain(int index, ushort editor, bool patched)
    {
        var patch = DehackedPatch.Apply(patched ? $"Thing {index}\nMass = 240\n" : ""); Assert.Empty(patch.Errors);
        Assert.Equal(8, patch.Actors.Single(a => a.Index == index).ReactionTime);
        var sim = Room(editor, patch); var map = sim.Actors.Single(a => a.MapPickupThingIndex.HasValue);
        Assert.Equal(8, map.ReactionTime); Assert.Equal(0, map.PainChance);
        Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), editor)); var drop = Drop(sim);
        Assert.Equal(8, drop.ReactionTime); Assert.Equal(0, drop.PainChance);
        var saved = SimSavegame.Write(sim, true); map.Destroy(); drop.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
        var fresh = Room(editor, patch); SimSavegame.Apply(fresh, saved); Assert.Equal(saved, SimSavegame.Write(fresh, true));
        for (var i = 0; i < 5; i++) { sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum); }
    }

    [Theory]
    [InlineData("", 0, false)]
    [InlineData("Pain chance = 256", 256, false)]
    public void ShootablePickupPainRequiresExplicitChance(string body, int chance, bool expected)
    {
        var patch = DehackedPatch.Apply($"Thing 64\nBits = SPECIAL+SHOOTABLE\n{body}\n"); Assert.Empty(patch.Errors);
        var sim = Room(2007, patch); Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), PickupCatalog.Clip));
        var drop = Drop(sim); Assert.Equal(chance, drop.PainChance);
        Assert.Equal(expected, drop.TriggerPainChance());
    }

    [Fact]
    public void ExplicitZeroReactionAndPainAreRetained()
    {
        var patch = DehackedPatch.Apply("Thing 64\nReaction time = 0\nPain chance = 0\n");
        var sim = Room(2007, patch); Assert.Equal(0, sim.Actors.Single(a => a.MapPickupThingIndex.HasValue).ReactionTime);
        Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), PickupCatalog.Clip));
        Assert.Equal(0, Drop(sim).ReactionTime); Assert.Equal(0, Drop(sim).PainChance);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(2007, patch); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
    }

    private static Actor Drop(AuthoritySimulation sim) => sim.Actors.Single(a => a.SpawnedDroppedPickup);
    private static AuthoritySimulation Room(ushort editor, DehackedPatchResult patch) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = editor, X = 400 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch, compat: CompatSurface.NoTossDrops);
}
