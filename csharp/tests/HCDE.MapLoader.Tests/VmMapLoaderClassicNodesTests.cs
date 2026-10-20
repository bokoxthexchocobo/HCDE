namespace HCDE.MapLoader.Tests;

public class VmMapLoaderClassicNodesTests
{
    [Fact]
    public void ShouldAttemptLoad_RejectsEmptyNodesWithoutSingleSubsector()
    {
        Assert.False(VmMapLoaderClassicNodes.ShouldAttemptLoad(0, 28, 0, 2));
        Assert.True(VmMapLoaderClassicNodes.ShouldAttemptLoad(0, 28, 0, 1));
    }

    [Fact]
    public void ResolveChild_InvalidSubsector_WhenIndexTooLarge()
    {
        var used = new int[4];
        var r = VmMapLoaderClassicNodes.ResolveChild(0x80000003, 0x80000000, 2, 4, 0, used);
        Assert.Equal(VmMapLoaderClassicNodes.ChildResolution.InvalidSubsector, r);
    }

    [Fact]
    public void ResolveChild_NodeAlreadyUsed_WhenDuplicate()
    {
        var used = new int[4];
        used[2] = 1;
        var r = VmMapLoaderClassicNodes.ResolveChild(2, 0x80000000, 8, 4, 1, used);
        Assert.Equal(VmMapLoaderClassicNodes.ChildResolution.NodeAlreadyUsed, r);
    }
}
