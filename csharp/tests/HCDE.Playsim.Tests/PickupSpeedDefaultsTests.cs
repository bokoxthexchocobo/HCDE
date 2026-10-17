using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PickupSpeedDefaultsTests
{
    [Theory]
    [InlineData("", false, 0)]
    [InlineData("Mass = 240", false, 0)]
    [InlineData("Speed = 12", false, 12)]
    [InlineData("Speed = -3", false, -3)]
    [InlineData("Speed = 98304", false, 1.5)]
    [InlineData("Speed = 12\nFast speed = 9", true, 9)]
    [InlineData("Speed = 12\nFast speed = -1", true, 12)]
    [InlineData("Speed = 12\nFast speed = 0", true, 0)]
    [InlineData("Speed = 12\nFast speed = 9", false, 12)]
    public void MapAndDroppedPickupUseClassSpeedBeforeFastOverride(string body, bool fast, double expected)
    {
        var patch = DehackedPatch.Apply(body.Length == 0 ? "" : $"Thing 64\n{body}\n"); Assert.Empty(patch.Errors);
        var sim = Room(patch, fast); var map = sim.Actors.Single(a => a.MapPickupThingIndex.HasValue);
        Assert.Equal(expected, map.MovementSpeed.ToDouble());
        Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), PickupCatalog.Clip)); var drop = Drop(sim);
        Assert.Equal(expected, drop.MovementSpeed.ToDouble()); Assert.Equal(drop.MovementSpeed.Raw, drop.SpawnTuning!.Value.SpeedRaw);
        var saved = SimSavegame.Write(sim, true); map.Destroy(); drop.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
        var fresh = Room(patch, fast); SimSavegame.Apply(fresh, saved); Assert.Equal(saved, SimSavegame.Write(fresh, true));
        for (var i = 0; i < 5; i++) { sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum); }
    }

    [Fact]
    public void RuntimeSpeedOverrideAndBaselineReloadUsePatchedClassSpeed()
    {
        var patch = DehackedPatch.Apply("Thing 64\nSpeed = 12\n"); var sim = Room(patch, false);
        Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), PickupCatalog.Clip));
        var baseline = SimSavegame.Write(sim, true); Drop(sim).MovementSpeed = Fixed.FromInt(23);
        Drop(sim).HasTuningOverride = true;
        var changed = SimSavegame.Write(sim, true); var fresh = Room(patch, false); SimSavegame.Apply(fresh, changed);
        Assert.Equal(23, Drop(fresh).MovementSpeed.ToDouble()); SimSavegame.Apply(fresh, baseline);
        Assert.Equal(12, Drop(fresh).MovementSpeed.ToDouble()); Assert.Equal(baseline, SimSavegame.Write(fresh, true));
    }

    private static Actor Drop(AuthoritySimulation sim) => sim.Actors.Single(a => a.SpawnedDroppedPickup);
    private static AuthoritySimulation Room(DehackedPatchResult patch, bool fast) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2007, X = 400 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch,
        spawnOptions: new SpawnOptions(FastMonsters: fast), compat: CompatSurface.NoTossDrops);
}
