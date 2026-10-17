using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedDroppedPickupFlagTests
{
    [Theory]
    [InlineData("SPECIAL+SOLID", true, false, false)]
    [InlineData("SPECIAL+SHOOTABLE", false, true, false)]
    [InlineData("SPECIAL+NOGRAVITY+NOCLIP", false, false, true)]
    [InlineData("0", false, false, false)]
    public void DynamicDropUsesPrimaryFlagsAndReconstructs(string bits, bool solid, bool shootable, bool noGravity)
    {
        var sim = Room(bits); Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), PickupCatalog.Clip));
        var drop = Drop(sim); drop.SetXYZ(200, 0, 40); drop.VelocityX = drop.VelocityY = drop.VelocityZ = default;
        Assert.Equal(solid, drop.Solid); Assert.Equal(shootable, drop.Shootable);
        Assert.False(drop.NoGravity); Assert.Equal(noGravity, drop.NoClip);
        Assert.Equal(bits != "0", drop.SpecialPickup); Assert.True(drop.Dropped);
        Assert.Equal(solid, drop.BlocksActors); Assert.Equal(shootable, drop.CanTakeDamage);
        var bytes = SimSavegame.Write(sim, true);
        drop.Destroy(); sim.Tick(); SimSavegame.Apply(sim, bytes);
        Assert.Equal(bytes, SimSavegame.Write(sim, true));
        var fresh = Room(bits); SimSavegame.Apply(fresh, bytes);
        Assert.Equal(bytes, SimSavegame.Write(fresh, true));
        for (var i = 0; i < 5; i++) { sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum); }
    }

    [Fact]
    public void ShootableDropTakesDamageAndRemovesThroughItsPickupTable()
    {
        var sim = Room("SPECIAL+SHOOTABLE"); Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), PickupCatalog.Clip));
        var drop = Drop(sim); drop.NoPain = true; ActorDamage.Apply(drop, 20);
        Assert.True(drop.IsDead); Assert.True(drop.Destroyed);
        sim.Tick(); Assert.True(drop.Destroyed);
    }

    [Fact]
    public void RemovingSpecialPreventsCollection()
    {
        var sim = Room("0"); var player = sim.Players.Single(); player.Inventory.Bullets = 0;
        Assert.True(sim.SpawnDroppedPickup(player, PickupCatalog.Clip));
        Drop(sim).SetXYZ(0, 0, 0); sim.Tick();
        Assert.False(Drop(sim).Destroyed); Assert.Equal(0, player.Inventory.Bullets);
    }

    [Fact]
    public void InventoryTossClearsSolidAndSpecialButRetainsShootableAcrossSave()
    {
        const string bits = "SPECIAL+SOLID+SHOOTABLE";
        var sim = Room(bits); Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), PickupCatalog.Clip, inventoryToss: true));
        var drop = Drop(sim); Assert.False(drop.Solid); Assert.False(drop.SpecialPickup); Assert.True(drop.Shootable);
        drop.SetXYZ(200, 0, 20); drop.VelocityX = drop.VelocityY = drop.VelocityZ = default;
        var bytes = SimSavegame.Write(sim, true); var fresh = Room(bits); SimSavegame.Apply(fresh, bytes);
        Assert.Equal(bytes, SimSavegame.Write(fresh, true));
        for (var i = 0; i < 30; i++) { sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum); }
        Assert.True(Drop(fresh).SpecialPickup); Assert.False(Drop(fresh).Solid); Assert.True(Drop(fresh).Shootable);
    }

    private static Actor Drop(AuthoritySimulation sim) => sim.Actors.Single(a => a.SpawnedDroppedPickup);
    private static AuthoritySimulation Room(string bits)
    {
        var patch = DehackedPatch.Apply($"Thing 64\nBits = {bits}\nHit points = 10\n"); Assert.Empty(patch.Errors);
        return AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch,
            compat: CompatSurface.NoTossDrops);
    }
}
