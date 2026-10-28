namespace HCDE.Playsim.Tests;

public class VmPTagManagerTests
{
    [Fact]
    public void SectorTagIterator_FindsTaggedSectors()
    {
        var mgr = new VmPTagManager();
        mgr.SetSectorCount(4);
        mgr.AddSectorTag(1, 7);
        mgr.AddSectorTag(3, 7);
        mgr.HashTags();
        var it = mgr.CreateSectorTagIterator(7);
        var found = new List<int>();
        int n;
        while ((n = it.Next()) >= 0)
            found.Add(n);
        Assert.Contains(1, found);
        Assert.Contains(3, found);
    }

    [Fact]
    public void LineIdIterator_WalksHashChain()
    {
        var mgr = new VmPTagManager();
        mgr.AddLineId(5, 9);
        mgr.AddLineId(8, 9);
        mgr.HashTags();
        var it = new VmPTagManager.LineIdIterator(9, mgr.IdHashFirst, mgr.AllIds);
        Assert.Equal(5, it.Next());
        Assert.Equal(8, it.Next());
        Assert.Equal(-1, it.Next());
    }

    [Fact]
    public void SectorHasTag_DetectsDuplicateTags()
    {
        var mgr = new VmPTagManager();
        mgr.AddSectorTag(2, 4);
        Assert.True(mgr.SectorHasTag(2, 4));
        mgr.AddSectorTag(2, 4);
        Assert.Equal(1, mgr.CountSectorTags(2));
    }

    [Fact]
    public void LineHasId_FindsMatchingId()
    {
        var mgr = new VmPTagManager();
        mgr.AddLineId(4, 11);
        Assert.True(mgr.LineHasId(4, 11));
        Assert.Equal(11, mgr.GetFirstLineId(4));
    }

    [Fact]
    public void GetSectorTag_ByIndex()
    {
        var mgr = new VmPTagManager();
        mgr.AddSectorTag(1, 5);
        mgr.AddSectorTag(1, 6);
        Assert.Equal(5, mgr.GetSectorTag(1, 0));
        Assert.Equal(6, mgr.GetSectorTag(1, 1));
    }
}
