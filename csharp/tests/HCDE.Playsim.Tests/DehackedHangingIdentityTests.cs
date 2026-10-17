using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedHangingIdentityTests
{
    [Theory]
    [InlineData(129, 73, "HangNoGuts", 88)]
    [InlineData(130, 74, "HangBNoBrain", 88)]
    [InlineData(131, 75, "HangTLookingDown", 64)]
    [InlineData(132, 76, "HangTSkull", 64)]
    [InlineData(133, 77, "HangTLookingUp", 64)]
    [InlineData(134, 78, "HangTNoBrain", 64)]
    public void NativeIdentityBindsPatchAndReconstructsDestroyedMapDecoration(int index, ushort type, string name, int height)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nHit points = 77\n"); Assert.Empty(patch.Errors);
        var definition = patch.Actors.Single(a => a.Index == index);
        Assert.Equal(name, definition.Name); Assert.Equal(type, definition.OriginalDoomEdNum);
        var sim = Room(type, patch); var actor = Assert.Single(sim.Actors);
        Assert.Equal(77, actor.Health); Assert.Equal(height, actor.Height.ToDouble()); Assert.Equal(16, actor.Radius.ToDouble());
        Assert.Equal(256 - height, actor.Z.ToDouble()); Assert.True(actor.Solid); Assert.False(actor.Shootable);
        Assert.True(actor.NoGravity); Assert.True(actor.SpawnCeiling); Assert.Null(actor.Brain);
        var saved = SimSavegame.Write(sim, true); actor.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        var restored = Assert.Single(sim.Actors); Assert.NotSame(actor, restored);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
        var fresh = Room(type, patch); SimSavegame.Apply(fresh, saved); Assert.Equal(saved, SimSavegame.Write(fresh, true));
    }

    [Theory]
    [InlineData(129, 73, 88)]
    [InlineData(130, 74, 88)]
    [InlineData(131, 75, 64)]
    [InlineData(132, 76, 64)]
    [InlineData(133, 77, 64)]
    [InlineData(134, 78, 64)]
    public void StockCeilingFlagsTriggerOriginalHeightResetAcrossLayering(int index, ushort type, int originalHeight)
    {
        var original = DehackedPatch.Apply($"Thing {index}\nPhysical height = 2097152\nProjectile pass height = 1048576\n");
        Assert.Empty(original.Errors); var actor = Assert.Single(Room(type, original).Actors);
        Assert.Equal(32, actor.Height.ToDouble()); Assert.Equal(16, actor.ProjectilePassHeight.ToDouble());
        var reset = DehackedPatch.Apply($"Thing {index}\nMass = 200\n", original); Assert.Empty(reset.Errors);
        var restored = Assert.Single(Room(type, reset).Actors);
        Assert.Equal(originalHeight, restored.Height.ToDouble()); Assert.Equal(0, restored.ProjectilePassHeight.Raw);
        Assert.Equal(32, original.Actors.Single(a => a.Index == index).Height);
    }

    [Fact]
    public void NamedBitsReplaceStockFlagsAndExplicitDimensionsRemainEffective()
    {
        var patch = DehackedPatch.Apply("Thing 129\nBits = SOLID\nHeight = 2097152\nWidth = 786432\n"); Assert.Empty(patch.Errors);
        var actor = Assert.Single(Room(73, patch).Actors);
        Assert.True(actor.Solid); Assert.False(actor.Shootable); Assert.False(actor.SpawnCeiling); Assert.False(actor.NoGravity);
        Assert.Equal(32, actor.Height.ToDouble()); Assert.Equal(12, actor.Radius.ToDouble()); Assert.Equal(0, actor.Z.ToDouble());
    }

    [Fact]
    public void EditorRemappingRetainsNativeDecorationDefaults()
    {
        var patch = DehackedPatch.Apply("Thing 129\nID # = 9073\nHeight = 2097152\n"); Assert.Empty(patch.Errors);
        var actor = Assert.Single(Room(9073, patch).Actors);
        Assert.Equal(9073, actor.DoomEdNum); Assert.Equal(73, actor.ClassDoomEdNum);
        Assert.Equal(32, actor.Height.ToDouble()); Assert.Equal(224, actor.Z.ToDouble()); Assert.Null(actor.Brain);
    }

    [Fact]
    public void UnpatchedDefinitionsKeepExistingMapBehaviorAndUnknownGapRemainsInvalid()
    {
        var patch = DehackedPatch.Apply("");
        var original = Assert.Single(Room(73, null).Actors); var unchanged = Assert.Single(Room(73, patch).Actors);
        Assert.Equal(original.Height, unchanged.Height); Assert.Equal(original.Z, unchanged.Z); Assert.Equal(original.Health, unchanged.Health);
        Assert.Contains(DehackedPatch.Apply("Thing 10000\nHit points = 77\n").Errors,
            error => error.Contains("out of range", StringComparison.Ordinal));
    }

    private static AuthoritySimulation Room(ushort type, DehackedPatchResult? patch) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = type }], Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
}
