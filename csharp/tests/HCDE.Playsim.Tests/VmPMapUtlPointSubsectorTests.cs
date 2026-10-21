namespace HCDE.Playsim.Tests;

public class VmPMapUtlPointSubsectorTests
{
    [Fact]
    public void PointOnSideSlow_ClassifiesVerticalPartition()
    {
        var node = new VmPMapUtlPointSubsector.BspNode { X = 10, Dy = 1 };
        Assert.Equal(1, VmPMapUtlPointSubsector.PointOnSideSlow(5, 0, node));
    }

    [Fact]
    public void VanillaPointOnLineSide_MatchesHorizontalCase()
    {
        Assert.Equal(1, VmPMapUtlPointSubsector.VanillaPointOnLineSide(0, 5, 10, 0, 0, 1));
    }

    [Fact]
    public void PointInSubsector_WalksToLeaf()
    {
        var leaf = new object();
        var root = new VmPMapUtlPointSubsector.BspNode
        {
            Child0 = leaf,
            Child0IsSubsector = true,
            Child1 = new VmPMapUtlPointSubsector.BspNode(),
        };
        Assert.Same(leaf, VmPMapUtlPointSubsector.PointInSubsector(1, 2, root, (_, _, _) => 0));
    }

    [Fact]
    public void BoxOnLineSide_ReturnsNegativeWhenStraddling()
    {
        var side = VmPMapUtlPointSubsector.BoxOnLineSide(0, 10, 20, 0, 10, 5, 0, 1, (_, _, _, _, _, _) => 0);
        Assert.Equal(0, side);
    }
}
