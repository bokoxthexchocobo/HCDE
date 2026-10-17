using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedDeadDecorationTests
{
    [Theory]
    [InlineData(112, 22, "DeadCacodemon")]
    [InlineData(113, 15, "DeadMarine")]
    [InlineData(114, 18, "DeadZombieMan")]
    [InlineData(115, 21, "DeadDemon")]
    [InlineData(116, 23, "DeadLostSoul")]
    [InlineData(117, 20, "DeadDoomImp")]
    [InlineData(118, 19, "DeadShotgunGuy")]
    [InlineData(119, 10, "GibbedMarine")]
    [InlineData(120, 12, "GibbedMarineExtra")]
    [InlineData(122, 24, "Gibs")]
    public void IdentityUsesBaseDefaultsAndSupportsRemappedReconstruction(int index, int editor, string name)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nID # = 9098\nMass = 200\n"); Assert.Empty(patch.Errors);
        var definition = patch.Actors.Single(a => a.Index == index);
        Assert.Equal(name, definition.Name); Assert.Equal(editor, definition.OriginalDoomEdNum);
        var sim = Room(9098, patch); var actor = Assert.Single(sim.Actors);
        Assert.Equal(1000, actor.Health); Assert.Equal(16, actor.Height.ToDouble()); Assert.Equal(20, actor.Radius.ToDouble());
        Assert.False(actor.Solid); Assert.False(actor.Shootable); Assert.False(actor.Corpse); Assert.False(actor.IsDead);
        Assert.False(actor.IsMonster); Assert.False(actor.NoGravity); Assert.False(actor.SpawnCeiling); Assert.Null(actor.Brain);
        Assert.Equal(0, actor.ProjectilePassHeight.Raw);
        var saved = SimSavegame.Write(sim, true); actor.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        Assert.NotSame(actor, Assert.Single(sim.Actors)); Assert.Equal(saved, SimSavegame.Write(sim, true));
        var fresh = Room(9098, patch); SimSavegame.Apply(fresh, saved); Assert.Equal(saved, SimSavegame.Write(fresh, true));
        for (var i = 0; i < 8; i++) { sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum); }
    }

    [Theory]
    [InlineData(113, 15)]
    [InlineData(114, 18)]
    [InlineData(122, 24)]
    public void PrimaryFlagsCanMakeDecorationBlockMissilesWithoutImplicitDeath(int index, ushort editor)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nBits = SOLID\nHit points = 77\n"); Assert.Empty(patch.Errors);
        var sim = Room(editor, patch); var target = Assert.Single(sim.Actors); target.X = Fixed.FromInt(30);
        Assert.True(target.Solid); Assert.False(target.Shootable); Assert.False(target.Corpse); Assert.Equal(77, target.Health);
        var owner = sim.AddBot(-200, 0); owner.Brain = null;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma); missile.SetXYZ(0, 0, 8);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        missile.Tick(); Assert.True(missile.Destroyed); Assert.Equal(77, target.Health);
    }

    [Fact]
    public void GibsRetainsGenericCrushSpawnAndDeadLostSoulRetainsTimedRemoval()
    {
        var patch = DehackedPatch.Apply("Thing 122\nMass = 200\n\nThing 116\nMass = 200\n"); Assert.Empty(patch.Errors);
        var gibs = Room(24, patch); var actor = Assert.Single(gibs.Actors);
        Assert.Equal(actor.GenericCrushState, actor.SpawnState); Assert.Equal(actor.SpawnState, actor.States.Current);
        var skull = Room(23, patch); var lost = Assert.Single(skull.Actors);
        for (var i = 0; i < 6; i++) skull.Tick(); Assert.True(lost.Destroyed);
        for (var i = 0; i < 8; i++) gibs.Tick(); Assert.False(actor.Destroyed);
    }

    [Fact]
    public void LayeredDimensionsAndFlagsDoNotMutateOriginalBaseDefaults()
    {
        var original = DehackedPatch.Apply("Thing 113\nMass = 200\n");
        var changed = DehackedPatch.Apply("Thing 113\nHeight = 2097152\nBits = CORPSE\n", original); Assert.Empty(changed.Errors);
        var actor = Assert.Single(Room(15, changed).Actors); Assert.Equal(32, actor.Height.ToDouble()); Assert.True(actor.Corpse);
        Assert.False(actor.IsDead); Assert.False(Assert.Single(Room(15, original).Actors).Corpse);
        Assert.Equal(16, original.Actors.Single(a => a.Index == 113).Height);
    }

    private static AuthoritySimulation Room(ushort type, DehackedPatchResult patch) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = type }], Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
}
