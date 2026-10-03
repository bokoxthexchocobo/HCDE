using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedDimensionTests
{
    [Theory]
    [InlineData("Width", 0)]
    [InlineData("Width", -65536)]
    [InlineData("Width", 98304)]
    [InlineData("Height", 0)]
    [InlineData("Height", -65536)]
    [InlineData("Height", 98304)]
    public void ExplicitDimensionsSurviveSpawnWithoutFallback(string field, int raw)
    {
        var actor = Spawn(DehackedPatch.Apply($"Thing 2\n{field} = {raw}\n"));
        Assert.Equal(raw, field == "Width" ? actor.Radius.Raw : actor.Height.Raw);
        Assert.Equal(field == "Width" ? 56 : 20, field == "Width" ? actor.Height.ToDouble() : actor.Radius.ToDouble());
    }

    [Fact]
    public void ChainedUnrelatedPatchKeepsExplicitZeroDimensions()
    {
        var first = DehackedPatch.Apply("Thing 2\nWidth = 0\nHeight = 0\n");
        var second = DehackedPatch.Apply("Thing 2\nHit points = 88\n", first);
        var actor = Spawn(second);
        Assert.Equal(0, actor.Radius.Raw); Assert.Equal(0, actor.Height.Raw);
        Assert.True(first.Actors.Single(record => record.Index == 2).WidthPatched);
        Assert.True(first.Actors.Single(record => record.Index == 2).HeightPatched);
    }

    private static Actor Spawn(DehackedPatchResult patch) => Assert.Single(AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 3004 }],
    }, dehacked: patch).Actors);
}
