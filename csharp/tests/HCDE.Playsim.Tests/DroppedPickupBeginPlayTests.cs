using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DroppedPickupBeginPlayTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void BeginPlayProcessesDormancyForOrdinaryAndMonsterInventory(bool monster, bool inventoryToss)
    {
        var patch = DehackedPatch.Apply($"Thing 44\nBits = SPECIAL+DORMANT{(monster ? "+COUNTKILL" : "")}\n");
        Assert.Empty(patch.Errors); var sim = Room(patch);
        Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), PickupCatalog.GreenArmor, inventoryToss: inventoryToss));
        var drop = Drop(sim); Assert.Equal(monster, drop.IsMonster); Assert.Equal(monster, drop.Dormant);
        Assert.Equal(monster ? -1 : 6, drop.States.RemainingTics);
        drop.SetXYZ(200, 0, 0); drop.VelocityX = drop.VelocityY = drop.VelocityZ = default;
        var saved = SimSavegame.Write(sim, true); drop.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
        var fresh = Room(patch); SimSavegame.Apply(fresh, saved); Assert.Equal(saved, SimSavegame.Write(fresh, true));
        for (var i = 0; i < 6; i++) { sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum); }
        Assert.Equal(monster ? 0 : 1, Drop(fresh).States.Current);
        if (monster)
        {
            Drop(fresh).Activate(null); Assert.False(Drop(fresh).Dormant);
            Assert.Equal(1, Drop(fresh).States.RemainingTics); fresh.Tick(); Assert.Equal(1, Drop(fresh).States.Current);
        }
    }

    private static Actor Drop(AuthoritySimulation sim) => sim.Actors.Single(a => a.SpawnedDroppedPickup);
    private static AuthoritySimulation Room(DehackedPatchResult patch) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 256 }] },
        dehacked: patch, compat: CompatSurface.NoTossDrops);
}
