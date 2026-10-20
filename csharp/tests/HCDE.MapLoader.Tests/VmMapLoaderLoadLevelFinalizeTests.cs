namespace HCDE.MapLoader.Tests;

public class VmMapLoaderLoadLevelFinalizeTests
{
    [Fact]
    public void CombineClassicNodeLoadResults_RequiresAllThree()
    {
        Assert.True(VmMapLoaderClassicNodeFallback.CombineClassicNodeLoadResults(true, true, true));
        Assert.False(VmMapLoaderClassicNodeFallback.CombineClassicNodeLoadResults(true, false, true));
    }

    [Fact]
    public void ResolveHeadGameNodeIndex_PrefersGameNodes()
    {
        Assert.Equal(2, VmMapLoaderLoadLevelFinalize.ResolveHeadGameNodeIndex(3, 5));
    }

    [Fact]
    public void ShouldLoadLightmapAfterSpawn_WhenNotForceRebuild()
    {
        Assert.True(VmMapLoaderLoadLevelFinalize.ShouldLoadLightmapAfterSpawn(false));
    }

    [Fact]
    public void ComputeNodeLength_UsesHypotenuse()
    {
        Assert.Equal(5, VmMapLoaderLoadLevelFinalize.ComputeNodeLength(3, 4));
    }

    [Fact]
    public void ShouldFallbackToZdbsp_WhenNanoFails()
    {
        Assert.True(VmMapLoaderForceNodeBuild.ShouldFallbackToZdbsp(false));
    }
}
