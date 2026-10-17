using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedCeilingHeightTests
{
    [Theory]
    [InlineData(1, 56)]
    [InlineData(12, 56)]
    [InlineData(22, 110)]
    [InlineData(31, 42)]
    [InlineData(34, 8)]
    public void CeilingThingWithoutHeightRestoresNativeOriginalAfterEarlierPatch(int index, int expected)
    {
        var original = DehackedPatch.Apply($"Thing {index}\nPhysical height = 4194304\nProjectile pass height = 1048576\n");
        var ceiling = DehackedPatch.Apply($"Thing {index}\nBits = SPAWNCEILING\n", original);
        Assert.Empty(ceiling.Errors); var actor = ceiling.Actors.Single(a => a.Index == index);
        Assert.Equal(expected, actor.Height); Assert.True(actor.HeightPatched); Assert.Equal(0, actor.ProjectilePassHeightRaw);
        Assert.Equal(64, original.Actors.Single(a => a.Index == index).Height);
        Assert.Equal(1048576, original.Actors.Single(a => a.Index == index).ProjectilePassHeightRaw);
    }

    [Theory]
    [InlineData("Height = 4194304\nBits = SPAWNCEILING", 64, 0)]
    [InlineData("Bits = SPAWNCEILING\nHeight = 0", 0, 0)]
    [InlineData("Bits = SPAWNCEILING\nPhysical height = 4194304\nProjectile pass height = 1048576", 64, 16)]
    [InlineData("Physical height = 4194304\nProjectile pass height = 1048576\nBits = SPAWNCEILING", 64, 16)]
    public void SameBlockExplicitHeightPreventsOriginalHeightReset(string body, int height, int pass)
    {
        var patch = DehackedPatch.Apply($"Thing 12\n{body}\n"); Assert.Empty(patch.Errors);
        var actor = patch.Actors.Single(a => a.Index == 12);
        Assert.Equal(height, actor.Height); Assert.Equal(pass * 65536, actor.ProjectilePassHeightRaw);
    }

    [Fact]
    public void LaterUnrelatedBlockOnCeilingClassRestoresOriginalButNonCeilingDoesNot()
    {
        var original = DehackedPatch.Apply("Thing 12\nBits = SPAWNCEILING\nHeight = 4194304\n");
        var changed = DehackedPatch.Apply("Thing 12\nMass = 200\n", original);
        Assert.Equal(56, changed.Actors.Single(a => a.Index == 12).Height);
        Assert.Equal(64, original.Actors.Single(a => a.Index == 12).Height);
        var floor = DehackedPatch.Apply("Thing 12\nBits = 0\n", original);
        Assert.Equal(64, floor.Actors.Single(a => a.Index == 12).Height);
    }

    [Fact]
    public void MapDynamicPlayerAndProjectileUseRestoredHeightAndFreshSave()
    {
        var original = DehackedPatch.Apply("Thing 1\nHeight = 4194304\n\nThing 12\nHeight = 4194304\n\nThing 34\nHeight = 4194304\n");
        var patch = DehackedPatch.Apply("Thing 1\nBits = SPAWNCEILING\n\nThing 12\nBits = SPAWNCEILING\n\nThing 34\nBits = SPAWNCEILING\n", original);
        Assert.Empty(patch.Errors);
        var level = new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3001, X = 200 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] };
        var sim = AuthoritySimulation.Start(level, dehacked: patch);
        Assert.All(sim.Actors, a => { Assert.Equal(56, a.Height.ToDouble()); Assert.Equal(200, a.Z.ToDouble()); });
        var dynamic = sim.AddBot(400, 0, 3001); Assert.Equal(56, dynamic.Height.ToDouble()); Assert.Equal(200, dynamic.Z.ToDouble());
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Rocket);
        Assert.Equal(8, missile.Height.ToDouble()); Assert.Equal(0, missile.ProjectilePassHeight.Raw);
        var saved = SimSavegame.Write(sim, true); var fresh = AuthoritySimulation.Start(level, dehacked: patch); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        Assert.Equal(8, fresh.Actors.Single(a => a.Id == missile.Id).Height.ToDouble());
    }

    [Fact]
    public void ExplicitHeightInOneBlockDoesNotSuppressResetInNextBlockOfSamePatch()
    {
        var patch = DehackedPatch.Apply("Thing 12\nBits = SPAWNCEILING\nHeight = 4194304\n\nThing 12\nMass = 200\n");
        Assert.Empty(patch.Errors); Assert.Equal(56, patch.Actors.Single(a => a.Index == 12).Height);
    }
}
