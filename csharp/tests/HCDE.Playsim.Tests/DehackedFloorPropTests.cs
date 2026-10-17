using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedFloorPropTests
{
    [Theory]
    [InlineData(121, 28, "HeadsOnAStick", 64, 16)]
    [InlineData(123, 27, "HeadOnAStick", 56, 16)]
    [InlineData(124, 29, "HeadCandles", 42, 16)]
    [InlineData(125, 25, "DeadStick", 64, 16)]
    [InlineData(126, 26, "LiveStick", 64, 16)]
    [InlineData(127, 54, "BigTree", 108, 32)]
    [InlineData(128, 70, "BurningBarrel", 32, 16)]
    public void NativeIdentityAndDefaultsSupportRemappedDestroyedReconstruction(int index, int editor, string name, int height, int radius)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nHit points = 77\nID # = 9088\n"); Assert.Empty(patch.Errors);
        var definition = patch.Actors.Single(a => a.Index == index);
        Assert.Equal(name, definition.Name); Assert.Equal(editor, definition.OriginalDoomEdNum);
        var sim = Room(9088, patch); var actor = Assert.Single(sim.Actors);
        Assert.Equal(77, actor.Health); Assert.Equal(height, actor.Height.ToDouble()); Assert.Equal(radius, actor.Radius.ToDouble());
        Assert.True(actor.Solid); Assert.False(actor.Shootable); Assert.False(actor.NoGravity); Assert.False(actor.SpawnCeiling);
        Assert.Equal(-16, actor.ProjectilePassHeight.ToDouble()); Assert.Equal(0, actor.Z.ToDouble()); Assert.Null(actor.Brain);
        var saved = SimSavegame.Write(sim, true); actor.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        Assert.NotSame(actor, Assert.Single(sim.Actors)); Assert.Equal(saved, SimSavegame.Write(sim, true));
        var fresh = Room(9088, patch); SimSavegame.Apply(fresh, saved); Assert.Equal(saved, SimSavegame.Write(fresh, true));
        sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void DefaultNegativePassHeightUsesMissileClipCompatibility(bool clip, bool stops)
    {
        var patch = DehackedPatch.Apply("Thing 127\nMass = 200\n"); Assert.Empty(patch.Errors);
        var sim = Room(54, patch, clip); var target = Assert.Single(sim.Actors); target.X = Fixed.FromInt(30);
        var owner = sim.AddBot(-200, 0); owner.Brain = null;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma); missile.SetXYZ(0, 0, 20);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        SimSavegame.Apply(sim, SimSavegame.Write(sim, true)); missile.Tick(); Assert.Equal(stops, missile.Destroyed);
    }

    [Fact]
    public void OrdinaryHeightClearsInheritedPassHeightAndPairedHeightCanReplaceIt()
    {
        var original = DehackedPatch.Apply("Thing 127\nMass = 200\n");
        var changed = DehackedPatch.Apply("Thing 127\nHeight = 4194304\n", original); Assert.Empty(changed.Errors);
        var actor = Assert.Single(Room(54, changed).Actors); Assert.Equal(64, actor.Height.ToDouble()); Assert.Equal(0, actor.ProjectilePassHeight.Raw);
        var paired = DehackedPatch.Apply("Thing 127\nPhysical height = 2097152\nProjectile pass height = 1048576\n", changed);
        actor = Assert.Single(Room(54, paired).Actors); Assert.Equal(32, actor.Height.ToDouble()); Assert.Equal(16, actor.ProjectilePassHeight.ToDouble());
        Assert.Equal(-16 * 65536, original.Actors.Single(a => a.Index == 127).ProjectilePassHeightRaw);
    }

    [Theory]
    [InlineData(124, 29)]
    [InlineData(126, 26)]
    [InlineData(128, 70)]
    public void AnimatedPropsKeepExistingStateTimingWhenPatched(int index, ushort editor)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nMass = 200\n");
        var sim = Room(editor, patch); var reference = Room(editor, null);
        for (var i = 0; i < 40; i++)
        {
            sim.Tick(); reference.Tick(); var actor = sim.Actors.Single(); var stock = reference.Actors.Single();
            Assert.Equal(stock.States.Current, actor.States.Current); Assert.Equal(stock.States.RemainingTics, actor.States.RemainingTics);
        }
    }

    private static AuthoritySimulation Room(ushort type, DehackedPatchResult? patch, bool clip = false) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = type }], Sectors = [new LevelSector { CeilingHeight = 256 }] },
        dehacked: patch, compat: clip ? CompatSurface.MissileClip : CompatSurface.None);
}
