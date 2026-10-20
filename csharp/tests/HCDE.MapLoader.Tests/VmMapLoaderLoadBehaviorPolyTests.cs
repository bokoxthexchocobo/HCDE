namespace HCDE.MapLoader.Tests;

public class VmMapLoaderLoadBehaviorTests
{
    [Fact]
    public void ShouldLoadBehaviorModule_WhenLumpPresent()
    {
        Assert.True(VmMapLoaderLoadBehavior.ShouldLoadBehaviorModule(128));
    }

    [Fact]
    public void ShouldUnloadAcsOnCheckFailure_WhenNotGood()
    {
        Assert.True(VmMapLoaderLoadBehavior.ShouldUnloadAcsOnCheckFailure(false));
    }
}

public class VmMapLoaderGetPolySpotsTests
{
    [Fact]
    public void IsPolySpotSpecial_MatchesAnchorRange()
    {
        Assert.True(VmMapLoaderGetPolySpots.IsPolySpotSpecial(1));
    }

    [Fact]
    public void CollectPolyStart_SplitsAnchorAndSpot()
    {
        var anchors = new List<VmMapLoaderGetPolySpots.PolyStart>();
        var spots = new List<VmMapLoaderGetPolySpots.PolyStart>();
        VmMapLoaderGetPolySpots.CollectPolyStart(
            new[]
            {
                new VmMapLoaderGetPolySpots.MapThingPolyCandidate(null, 1, 1.0, 2.0, 7),
                new VmMapLoaderGetPolySpots.MapThingPolyCandidate(null, 2, 3.0, 4.0, 8)
            },
            anchors,
            spots);
        Assert.Single(anchors);
        Assert.Single(spots);
    }
}

public class VmMapLoaderCalcIndicesTests
{
    [Fact]
    public void ApplyCalcIndices_FillsSequential()
    {
        var arrays = new VmMapLoaderCalcIndices.LevelIndexArrays
        {
            LineNums = new int[3]
        };
        VmMapLoaderCalcIndices.ApplyCalcIndices(arrays);
        Assert.Equal(new[] { 0, 1, 2 }, arrays.LineNums);
    }
}

public class VmMapLoaderLoadLevelPreambleTests
{
    [Fact]
    public void ResolveTranslator_PrefersMapInfo()
    {
        var translator = VmMapLoaderLoadLevelPreamble.ResolveTranslator(
            "map.xlat",
            null,
            "MAP01",
            "xlat/doom.txt",
            _ => false);
        Assert.Equal("map.xlat", translator);
    }

    [Fact]
    public void ApplyDummySwitchesFlag_SetsForDoomFormat()
    {
        var flags = VmMapLoaderLoadLevelPreamble.ApplyDummySwitchesFlag(0, 0x100, false, false);
        Assert.Equal(0x100u, flags);
    }
}
