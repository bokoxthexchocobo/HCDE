using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PatchedDropSpawnRulesTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void ExplicitDropPositionAndGravityOverrideSurviveReconstruction(bool inventoryToss, bool noToss)
    {
        var sim = Room(noToss); var player = sim.Players.Single(); player.Z = Fixed.FromInt(48);
        Assert.True(sim.SpawnDroppedPickup(player, PickupCatalog.Clip, inventoryToss: inventoryToss));
        var drop = Drop(sim); Assert.True(drop.SpawnCeiling); Assert.Equal(inventoryToss, drop.NoGravity);
        Assert.Equal(48 + (inventoryToss ? 10 : noToss ? 0 : player.Height.ToDouble() / 2), drop.Z.ToDouble());
        Assert.False(drop.OnGround);
        var saved = SimSavegame.Write(sim, true); drop.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
        var fresh = Room(noToss); SimSavegame.Apply(fresh, saved); Assert.Equal(saved, SimSavegame.Write(fresh, true));
        Assert.Equal(inventoryToss, Drop(fresh).NoGravity);
        for (var i = 0; i < 5; i++) { sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NativeDropForcesGravityButRetainsLowGravityFactor(bool noToss)
    {
        var sim = Room(noToss); Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), PickupCatalog.Clip));
        var drop = Drop(sim); drop.SetXYZ(200, 0, 64); drop.OnGround = false;
        drop.VelocityX = drop.VelocityY = drop.VelocityZ = default;
        Assert.False(drop.NoGravity); Assert.Equal(0.25, drop.Gravity.ToDouble());
        sim.Tick(); Assert.Equal(-0.25, drop.VelocityZ.ToDouble());
    }

    private static Actor Drop(AuthoritySimulation sim) => sim.Actors.Single(a => a.SpawnedDroppedPickup);
    private static AuthoritySimulation Room(bool noToss)
    {
        var patch = DehackedPatch.Apply("Thing 64\nBits = SPECIAL+SPAWNCEILING+NOGRAVITY+LOGRAV\nHeight = 1048576\n");
        Assert.Empty(patch.Errors);
        return AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch,
            compat: noToss ? CompatSurface.NoTossDrops : CompatSurface.None);
    }
}
