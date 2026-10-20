namespace HCDE.MapLoader.Tests;

public class VmMapLoaderNodeLoadOrchestrationTests
{
    [Fact]
    public void CombineNodeLoadResults_RequiresAllThree()
    {
        Assert.False(VmMapLoaderNodeLoadOrchestration.CombineNodeLoadResults(true, true, false));
    }

    [Fact]
    public void ResolveHeadGameNodeIndex_PrefersGameNodes()
    {
        Assert.Equal(4, VmMapLoaderNodeLoadOrchestration.ResolveHeadGameNodeIndex(5, 10));
    }
}
