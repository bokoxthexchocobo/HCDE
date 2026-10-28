namespace HCDE.MapLoader.Tests;

public class VmMapLoaderPackedBlockmapTests
{
    [Fact]
    public void ComputeBlockHash_MatchesLinearCongruence()
    {
        var h = VmMapLoaderPackedBlockmap.ComputeBlockHash(new[] { 1, 2, 3 });
        Assert.Equal((1 * 12235 + 2) * 12235 + 3 & VmMapLoaderPackedBlockmap.HashMask, h);
    }

    [Fact]
    public void PackBlocks_DeduplicatesIdenticalLists()
    {
        var blocks = new[] { new[] { 4, 5 }, new[] { 4, 5 } };
        var packed = VmMapLoaderPackedBlockmap.PackBlocks(blocks, 2, 1);
        Assert.Equal(packed[4], packed[5]);
    }

    [Fact]
    public void PushHorizontalSpan_FillsInclusiveRange()
    {
        var lists = new List<int>[4];
        for (var i = 0; i < lists.Length; i++)
            lists[i] = new List<int>();
        VmMapLoaderCreateBlockMapSpan.PushHorizontalSpan(lists, 2, 0, 0, 1, 7);
        Assert.Equal(new[] { 7, 7 }, new[] { lists[0][0], lists[1][0] });
    }

    [Fact]
    public void ShouldInvokePackedBuilder_RejectsZeroWidth()
    {
        Assert.False(VmMapLoaderCreateBlockMapSpan.ShouldInvokePackedBuilder(0, 3));
    }
}
