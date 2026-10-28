namespace HCDE.MapLoader.Tests;

public class VmMapLoaderNodeLoadOrchestrationTests
{
    [Fact]
    public void CombineNodeLoadResults_RequiresAllThree()
    {
        Assert.False(VmMapLoaderNodeLoadOrchestration.CombineNodeLoadResults(true, true, false));
    }

    [Fact]
    public void MatchesAnyNodeSignature_AcceptsZnoded()
    {
        var sigs = VmMapLoaderNodeLoadOrchestration.EnumerateZnodedSignatures().ToArray();
        Assert.True(VmMapLoaderNodeLoadOrchestration.MatchesAnyNodeSignature(sigs[0], sigs));
    }

    [Fact]
    public void ShouldClearLaxMonsterActivation_ForDoomFormat()
    {
        Assert.True(VmMapLoaderNodeLoadOrchestration.ShouldClearLaxMonsterActivation(false, false, 0));
    }

    [Fact]
    public void ResolveHeadGameNodeIndex_PrefersGameNodes()
    {
        Assert.Equal(4, VmMapLoaderNodeLoadOrchestration.ResolveHeadGameNodeIndex(5, 10));
    }
}
