namespace HCDE.Playsim.Tests;

public class VmPMobjNightmareHashTests
{
    [Fact]
    public void PositionBlocked_DestroyPath() =>
        Assert.True(VmPMobjNightmareHash.PositionBlocked(false));

    [Fact]
    public void TidZeroSkipsHash() =>
        Assert.True(VmPMobjNightmareHash.TidZeroSkipsHash(0));

    [Fact]
    public void LinearSearch_WhenStartNonZero() =>
        Assert.True(VmPMobjNightmareHash.LinearSearch(5));
}
