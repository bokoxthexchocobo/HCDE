using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedDroppedPickupExtendedTests
{
    [Theory]
    [InlineData("PASSMOBJ")]
    [InlineData("NOTELEPORT")]
    [InlineData("CANSLIDE")]
    [InlineData("INVULNERABLE")]
    [InlineData("DORMANT")]
    [InlineData("REFLECTIVE")]
    [InlineData("FLOORCLIP")]
    [InlineData("PUSHABLE")]
    public void SecondaryDefaultMatchesMapActorAndReconstructs(string flag)
    {
        var patch = DehackedPatch.Apply($"Thing 64\nBits = SPECIAL+{flag}\n"); Assert.Empty(patch.Errors);
        var sim = Room(patch); var map = sim.Actors.Single(a => a.MapPickupThingIndex.HasValue);
        Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), PickupCatalog.Clip));
        var drop = Drop(sim); Assert.Equal(flag != "DORMANT", Flag(drop, flag));
        // Map activation applies the map thing's dormancy separately from class defaults.
        if (flag != "DORMANT") Assert.Equal(Flag(map, flag), Flag(drop, flag));
        drop.SetXYZ(200, 0, 40); drop.VelocityX = drop.VelocityY = drop.VelocityZ = default;
        var bytes = SimSavegame.Write(sim, true);
        drop.Destroy(); sim.Tick(); SimSavegame.Apply(sim, bytes); Assert.Equal(flag != "DORMANT", Flag(Drop(sim), flag));
        Assert.Equal(bytes, SimSavegame.Write(sim, true));
        var fresh = Room(patch); SimSavegame.Apply(fresh, bytes); Assert.Equal(flag != "DORMANT", Flag(Drop(fresh), flag));
        Assert.Equal(bytes, SimSavegame.Write(fresh, true));
        for (var i = 0; i < 5; i++) { sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LowGravityAffectsFallingUnlessNoGravityIsSet(bool noGravity)
    {
        var patch = DehackedPatch.Apply($"Thing 64\nBits = SPECIAL+LOGRAV{(noGravity ? "+NOGRAVITY" : "")}\n");
        Assert.Empty(patch.Errors); var sim = Room(patch);
        Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), PickupCatalog.Clip, inventoryToss: true)); var drop = Drop(sim);
        drop.SetXYZ(200, 0, 64); drop.OnGround = false; drop.VelocityX = drop.VelocityY = drop.VelocityZ = default;
        Assert.Equal(0.25, drop.Gravity.ToDouble()); sim.Tick();
        Assert.Equal(noGravity ? 0 : -0.25, drop.VelocityZ.ToDouble());
        var saved = SimSavegame.Write(sim, true); var fresh = Room(patch); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
    }

    [Fact]
    public void ExtendedCombatAndProjectileHeightDefaultsReachDropFactory()
    {
        var patch = DehackedPatch.Apply("Thing 64\nPhysical height = 2097152\nProjectile pass height = 524288\nPain chance = 17\nMelee range = 6553600\n");
        Assert.Empty(patch.Errors); var sim = Room(patch);
        Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), PickupCatalog.Clip)); var drop = Drop(sim);
        Assert.Equal(32, drop.Height.ToDouble()); Assert.Equal(8, drop.ProjectilePassHeight.ToDouble());
        Assert.Equal(17, drop.PainChance); Assert.Equal(80, drop.MeleeRange.ToDouble());
        var bytes = SimSavegame.Write(sim, true); var fresh = Room(patch); SimSavegame.Apply(fresh, bytes);
        Assert.Equal(bytes, SimSavegame.Write(fresh, true));
    }

    private static bool Flag(Actor actor, string name) => name switch {
        "PASSMOBJ" => actor.CanPass, "NOTELEPORT" => actor.NoTeleport, "CANSLIDE" => actor.CanSlide,
        "INVULNERABLE" => actor.Invulnerable, "DORMANT" => actor.Dormant, "REFLECTIVE" => actor.Reflective,
        "FLOORCLIP" => actor.FloorClipEnabled, "PUSHABLE" => actor.Pushable,
        _ => throw new ArgumentOutOfRangeException(nameof(name)),
    };
    private static Actor Drop(AuthoritySimulation sim) => sim.Actors.Single(a => a.SpawnedDroppedPickup);
    private static AuthoritySimulation Room(DehackedPatchResult patch) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2007, X = 400 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch, compat: CompatSurface.NoTossDrops);
}
