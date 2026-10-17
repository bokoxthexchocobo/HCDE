using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedOlderHangingTests
{
    [Theory]
    [InlineData(102, 49, "BloodyTwitch", 68, true)]
    [InlineData(103, 50, "Meat2", 84, true)]
    [InlineData(104, 51, "Meat3", 84, true)]
    [InlineData(105, 52, "Meat4", 68, true)]
    [InlineData(106, 53, "Meat5", 52, true)]
    [InlineData(107, 59, "NonsolidMeat2", 84, false)]
    [InlineData(108, 60, "NonsolidMeat4", 68, false)]
    [InlineData(109, 61, "NonsolidMeat3", 52, false)]
    [InlineData(110, 62, "NonsolidMeat5", 52, false)]
    [InlineData(111, 63, "NonsolidTwitch", 68, false)]
    public void NativeIdentityDefaultsAndRemappedReconstruction(int index, ushort editor, string name, int height, bool solid)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nHit points = 77\nID # = 9099\n"); Assert.Empty(patch.Errors);
        var definition = patch.Actors.Single(a => a.Index == index);
        Assert.Equal(name, definition.Name); Assert.Equal(editor, definition.OriginalDoomEdNum);
        var sim = Room(9099, patch); var actor = Assert.Single(sim.Actors);
        Assert.Equal(77, actor.Health); Assert.Equal(height, actor.Height.ToDouble()); Assert.Equal(solid ? 16 : 20, actor.Radius.ToDouble());
        Assert.Equal(solid, actor.Solid); Assert.False(actor.Shootable); Assert.True(actor.NoGravity); Assert.True(actor.SpawnCeiling);
        Assert.Equal(160 - height, actor.Z.ToDouble()); Assert.Null(actor.Brain);
        var saved = SimSavegame.Write(sim, true); actor.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        Assert.NotSame(actor, Assert.Single(sim.Actors)); Assert.Equal(saved, SimSavegame.Write(sim, true));
        var fresh = Room(9099, patch); SimSavegame.Apply(fresh, saved); Assert.Equal(saved, SimSavegame.Write(fresh, true));
        sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum);
    }

    [Theory]
    [InlineData(102, 49, true)]
    [InlineData(111, 63, false)]
    public void ParsedDefaultSolidityControlsActualMissileContact(int index, ushort editor, bool stops)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nMass = 200\n"); Assert.Empty(patch.Errors);
        var sim = Room(editor, patch); var target = Assert.Single(sim.Actors); target.X = Fixed.FromInt(30);
        var owner = sim.AddBot(-200, 0); owner.Brain = null;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma); missile.SetXYZ(0, 0, 100);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        missile.Tick(); Assert.Equal(stops, missile.Destroyed);
    }

    [Fact]
    public void NonsolidMeatThreeRetainsNativeMismatchBetweenStockAndDehackedOriginalHeight()
    {
        var stock = DehackedPatch.Apply(""); Assert.Equal(84, stock.Actors.Single(a => a.Index == 109).Height);
        Assert.Equal(84, Assert.Single(Room(61, stock).Actors).Height.ToDouble());
        var patched = DehackedPatch.Apply("Thing 109\nMass = 200\n", stock);
        Assert.Equal(52, Assert.Single(Room(61, patched).Actors).Height.ToDouble());
        Assert.Equal(84, stock.Actors.Single(a => a.Index == 109).Height);
    }

    [Theory]
    [InlineData(103, 50, 84)]
    [InlineData(108, 60, 68)]
    public void LayeringRestoresOriginalHeightAndPrimaryBitsCanReplaceInheritedDefaults(int index, ushort editor, int nativeHeight)
    {
        var original = DehackedPatch.Apply($"Thing {index}\nPhysical height = 2097152\nProjectile pass height = 1048576\n");
        var patched = Assert.Single(Room(editor, original).Actors); Assert.Equal(32, patched.Height.ToDouble());
        var reset = DehackedPatch.Apply($"Thing {index}\nMass = 200\n", original); Assert.Empty(reset.Errors);
        var actor = Assert.Single(Room(editor, reset).Actors); Assert.Equal(nativeHeight, actor.Height.ToDouble());
        Assert.Equal(0, actor.ProjectilePassHeight.Raw); Assert.Equal(32, original.Actors.Single(a => a.Index == index).Height);
        var floor = DehackedPatch.Apply($"Thing {index}\nBits = SOLID\nHeight = 2097152\n", reset);
        actor = Assert.Single(Room(editor, floor).Actors); Assert.True(actor.Solid); Assert.False(actor.NoGravity);
        Assert.False(actor.SpawnCeiling); Assert.Equal(0, actor.Z.ToDouble()); Assert.Equal(32, actor.Height.ToDouble());
    }

    private static AuthoritySimulation Room(ushort type, DehackedPatchResult patch) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = type }], Sectors = [new LevelSector { CeilingHeight = 160 }] }, dehacked: patch);
}
