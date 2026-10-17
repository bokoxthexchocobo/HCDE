using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedDroppedPickupCombatTests
{
    [Theory]
    [InlineData(54, 2011)]
    [InlineData(64, 2007)]
    [InlineData(78, 2001)]
    public void CombatDefaultsReachDropsAndReconstruct(int index, int editor)
    {
        var patch = Patch(index, 9); var sim = Room(patch);
        Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), editor)); var drop = Drop(sim);
        AssertDefaults(drop); var saved = SimSavegame.Write(sim, true);
        drop.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved); AssertDefaults(Drop(sim));
        Assert.Equal(saved, SimSavegame.Write(sim, true));
        var fresh = Room(patch); SimSavegame.Apply(fresh, saved); AssertDefaults(Drop(fresh));
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        for (var i = 0; i < 5; i++) { sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum); }
    }

    [Theory]
    [InlineData(9, true)]
    [InlineData(10, false)]
    [InlineData(-1, false)]
    public void DroppedGroupDefaultsControlProjectileAndSplashImmunity(int group, bool expected)
    {
        var sim = Room(Patch(64, group)); Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), PickupCatalog.Clip));
        var drop = Drop(sim); var source = new Actor { DoomEdNum = 3004, ProjectileGroup = 9, SplashGroup = 9 };
        Assert.Equal(expected, drop.ProjectileImmune(source)); Assert.Equal(expected, drop.SplashImmune(source));
        var saved = SimSavegame.Write(sim, true); var fresh = Room(Patch(64, group)); SimSavegame.Apply(fresh, saved);
        Assert.Equal(expected, Drop(fresh).ProjectileImmune(source)); Assert.Equal(expected, Drop(fresh).SplashImmune(source));
    }

    [Fact]
    public void RuntimeDamageAndReactionOverridesRestoreThenReturnToClassDefaults()
    {
        var patch = Patch(64, 9); var sim = Room(patch);
        Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), PickupCatalog.Clip)); var baseline = SimSavegame.Write(sim, true);
        var drop = Drop(sim); drop.Damage = 23; drop.ReactionTime = 41;
        var changed = SimSavegame.Write(sim, true); var fresh = Room(patch); SimSavegame.Apply(fresh, changed);
        Assert.Equal(23, Drop(fresh).Damage); Assert.Equal(41, Drop(fresh).ReactionTime);
        SimSavegame.Apply(fresh, baseline); AssertDefaults(Drop(fresh)); Assert.Equal(baseline, SimSavegame.Write(fresh, true));
    }

    private static void AssertDefaults(Actor actor)
    {
        Assert.Equal(17, actor.Damage); Assert.Equal(12, actor.ReactionTime);
        Assert.Equal(7, actor.InfightingGroup); Assert.Equal(9, actor.ProjectileGroup); Assert.Equal(9, actor.SplashGroup);
    }
    private static DehackedPatchResult Patch(int index, int group)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nReaction time = 12\nMissile damage = 17\nInfighting group = 7\nProjectile group = {group}\nSplash group = {group}\n");
        Assert.Empty(patch.Errors); return patch;
    }
    private static Actor Drop(AuthoritySimulation sim) => sim.Actors.Single(a => a.SpawnedDroppedPickup);
    private static AuthoritySimulation Room(DehackedPatchResult patch) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 256 }] },
        dehacked: patch, compat: CompatSurface.NoTossDrops);
}
