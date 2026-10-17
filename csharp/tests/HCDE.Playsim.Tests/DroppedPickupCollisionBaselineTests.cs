using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DroppedPickupCollisionBaselineTests
{
    [Theory]
    [InlineData("SPECIAL+SHOOTABLE+COUNTKILL", false, false, false)]
    [InlineData("SPECIAL+SOLID+SHOOTABLE+COUNTKILL", true, false, false)]
    [InlineData("SPECIAL+SHOOTABLE+COUNTKILL+NOBLOCKMAP", false, true, false)]
    [InlineData("SPECIAL+SHOOTABLE+COUNTKILL+DROPPED", false, false, true)]
    public void RevivalRestoresDroppedClassCollisionInsteadOfGenericFlags(string bits, bool solid, bool noBlockmap, bool dropped)
    {
        var patch = DehackedPatch.Apply($"Thing 64\nBits = {bits}\nHit points = 10\n"); Assert.Empty(patch.Errors);
        var sim = Room(patch); Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), PickupCatalog.Clip));
        var drop = Drop(sim); Assert.True(drop.ResurrectionCollisionFlags.HasValue);
        ActorDamage.Apply(drop, 20, damageType: "Ice"); Assert.True(drop.IceCorpse); Assert.True(drop.Solid);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(patch); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        foreach (var actor in new[] { drop, Drop(fresh) })
        {
            actor.NoBlockmap = !noBlockmap;
            ActorRaise.ReviveSupported(actor);
            Assert.Equal(solid, actor.Solid); Assert.True(actor.Shootable); Assert.True(actor.IsMonster);
            Assert.Equal(noBlockmap, actor.NoBlockmap); Assert.Equal(dropped, actor.Dropped); Assert.True(actor.CanPass);
            Assert.False(actor.IceCorpse); Assert.False(actor.Corpse); Assert.Equal(10, actor.Health);
            ActorRaise.EnterRaiseState(actor, null);
        }
        sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum);
    }

    [Fact]
    public void UnpatchedDropCapturesNonBlockingNonShootableClassDefaults()
    {
        var sim = Room(DehackedPatch.Apply("")); Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), PickupCatalog.Clip));
        var drop = Drop(sim); Assert.True(drop.ResurrectionCollisionFlags.HasValue);
        drop.Solid = drop.Shootable = true; drop.IsMonster = true; drop.NoBlockmap = true;
        ActorRaise.ReviveSupported(drop);
        Assert.False(drop.Solid); Assert.False(drop.Shootable); Assert.False(drop.IsMonster); Assert.False(drop.NoBlockmap);
        Assert.False(drop.Dropped);
    }

    private static Actor Drop(AuthoritySimulation sim) => sim.Actors.Single(a => a.SpawnedDroppedPickup);
    private static AuthoritySimulation Room(DehackedPatchResult patch) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 256 }] },
        dehacked: patch, compat: CompatSurface.NoTossDrops);
}
