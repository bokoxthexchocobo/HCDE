namespace HCDE.Playsim.Tests;

public class VmDrawSeg1270SliceTests
{
    [Fact]
    public void Portals_HideTheOuterSegmentsAndSplitGroupsOfOneHundred()
    {
        var list = new VmDrawList1270 { ViewHeight = 200 };
        list.Clear();
        Assert.Equal(0, list.SegmentsCount());
        Assert.Equal(0, list.TranslucentCount());

        list.Push(Seg(1, 0, 3, 5, 5));
        list.Push(Seg(2, 1, 4, 2, 7));
        list.PushTranslucent(Seg(9, 0, 1, 0, 0));
        list.PushPortal();
        list.Push(Seg(3, 4, 6, 1, 8));
        list.PushTranslucent(Seg(8, 0, 1, 0, 0));
        Assert.Equal(1, list.SegmentsCount());
        Assert.Equal(1, list.TranslucentCount());
        Assert.Equal(3, list.Segment(0)!.Id);
        Assert.Equal(8, list.Translucent(0)!.Id);

        list.Build();
        Assert.Single(list.Groups);
        Assert.Equal(4, list.Groups[0].X1);
        Assert.Equal(6, list.Groups[0].X2);
        Assert.Equal(1f, list.Groups[0].Near);
        Assert.Equal(8f, list.Groups[0].Far);
        Assert.Equal(new short[] { 0, 0 }, list.Groups[0].Top);
        Assert.Equal(new short[] { 200, 200 }, list.Groups[0].Bottom);

        list.PopPortal();
        Assert.Equal(2, list.SegmentsCount());
        Assert.Equal(1, list.TranslucentCount());
        Assert.Equal(2, list.Segment(0)!.Id);
        Assert.Equal(1, list.Segment(1)!.Id);
        Assert.Equal(9, list.Translucent(0)!.Id);
        list.Build();
        Assert.Equal(0, list.Groups[0].Begin);
        Assert.Equal(2, list.Groups[0].End);
        Assert.Equal(0, list.Groups[0].X1);
        Assert.Equal(4, list.Groups[0].X2);
        Assert.Equal(2f, list.Groups[0].Near);
        Assert.Equal(7f, list.Groups[0].Far);

        list.PopPortal();
        Assert.Throws<IndexOutOfRangeException>(() => list.PopPortal());

        list.Clear();
        list.Push(null);
        Assert.Throws<NullReferenceException>(() => list.Build());
        list.Thread = null;
        list.Clear();
        list.Push(Seg(1, 0, 1, 0, 0));
        Assert.Throws<NullReferenceException>(() => list.Build());

        list.Thread = new VmFrameMem1270();
        list.Clear();
        for (int i = 0; i <= 100; i++)
            list.Push(Seg(i, (short)i, (short)(i + 2), i, i));
        list.Build();
        Assert.Equal(2, list.Groups.Count);
        Assert.Equal(0, list.Groups[0].Begin);
        Assert.Equal(100, list.Groups[0].End);
        Assert.Equal(1, list.Groups[0].X1);
        Assert.Equal(102, list.Groups[0].X2);
        Assert.Equal(1f, list.Groups[0].Near);
        Assert.Equal(100f, list.Groups[0].Far);
        Assert.Equal(101, list.Groups[0].Top.Length);
        Assert.Equal(0, list.Groups[0].Top[0]);
        Assert.Equal(200, list.Groups[0].Bottom[100]);
        Assert.Equal(100, list.Groups[1].Begin);
        Assert.Equal(101, list.Groups[1].End);
        Assert.Equal(0, list.Groups[1].X1);
        Assert.Equal(2, list.Groups[1].X2);
        Assert.Equal(0f, list.Groups[1].Near);
        Assert.Equal(0f, list.Groups[1].Far);
        Assert.Equal(new short[] { 200, 200 }, list.Groups[1].Bottom);
        Assert.Equal(4, list.Thread.Allocs);
    }

    [Fact]
    public void Clips_MergeSilhouetteAndRestoreOnlyTheDrawnRange()
    {
        var list = new VmDrawList1270 { ViewHeight = 200 };
        list.Clear();
        var newest = Seg(3, 8, 11, 10, 2);
        var middle = Seg(2, 12, 16, 3, 9);
        middle.Silhouette = 1;
        middle.BottomOrigin = 12;
        middle.Bottom = [50, 120, 40, 60];
        var oldest = Seg(1, 10, 14, 1, 4);
        oldest.Silhouette = 3;
        oldest.TopOrigin = 10;
        oldest.Top = [5, 6, 7, -3];
        oldest.BottomOrigin = 10;
        oldest.Bottom = [100, 90, 80, 70];
        list.Push(oldest);
        list.Push(middle);
        list.Push(newest);
        list.Build();
        var group = list.Groups[0];
        Assert.Equal(0, group.Begin);
        Assert.Equal(3, group.End);
        Assert.Equal(8, group.X1);
        Assert.Equal(16, group.X2);
        Assert.Equal(1f, group.Near);
        Assert.Equal(10f, group.Far);
        Assert.Equal(new short[] { 0, 0, 5, 6, 7, 0, 0, 0 }, group.Top);
        Assert.Equal(new short[] { 200, 200, 100, 90, 50, 70, 40, 60 }, group.Bottom);
        middle.Bottom[0] = 1;
        Assert.Equal(50, group.Bottom[4]);

        var mem = new VmFrameMem1270();
        var clip = new VmDrawClip1270 { ViewHeight = 40000 };
        short[] ceiling = [0, 1, 2, 3, 4, 5, 6, 7, 8];
        clip.SetTopClip(mem, 4, 7, ceiling);
        Assert.Equal(4, clip.TopAt(4));
        Assert.Equal(6, clip.TopAt(6));
        int allocs = mem.Allocs;
        Assert.Throws<NullReferenceException>(() => clip.SetTopClip(mem, 4, 7, (short[]?)null));
        Assert.Equal(allocs + 1, mem.Allocs);
        Assert.Equal(4, clip.TopAt(4));
        Assert.Throws<NullReferenceException>(() => clip.SetTopClip(null, 4, 7, ceiling));

        clip.SetTopClip(mem, 1, 3, (short)9);
        Assert.Equal(9, clip.TopAt(1));
        Assert.Equal(9, clip.TopAt(2));
        clip.SetBottomClip(mem, 4, 6, ceiling);
        Assert.Equal(4, clip.BottomAt(4));
        Assert.Equal(5, clip.BottomAt(5));
        clip.SetBottomClip(mem, 4, 6, (short)-7);
        Assert.Equal(-7, clip.BottomAt(4));
        Assert.Equal(-7, clip.BottomAt(5));

        clip.SetTopClip(mem, 4, 7, ceiling);
        clip.SetRangeDrawn(4, 7);
        Assert.True(clip.SprClipped);
        Assert.Equal((short)-25536, clip.TopAt(4));
        Assert.Equal((short)-25536, clip.TopAt(6));
        Assert.Throws<NullReferenceException>(() => clip.SetRangeUndrawn(4, 7));
        Assert.False(clip.SprClipped);
        Assert.Equal((short)-25536, clip.TopAt(4));
        clip.SetRangeUndrawn(4, 7);
        Assert.Equal((short)-25536, clip.TopAt(5));

        clip.SetBackupClip(mem, 4, 7, ceiling);
        clip.SetRangeDrawn(4, 7);
        clip.SetRangeUndrawn(5, 7);
        Assert.False(clip.SprClipped);
        Assert.Equal((short)-25536, clip.TopAt(4));
        Assert.Equal(5, clip.TopAt(5));
        Assert.Equal(6, clip.TopAt(6));
        clip.SetRangeUndrawn(4, 5);
        Assert.Equal((short)-25536, clip.TopAt(4));
    }

    static VmDrawSeg1270 Seg(int id, short x1, short x2, float sz1, float sz2) => new()
    {
        Id = id,
        X1 = x1,
        X2 = x2,
        Sz1 = sz1,
        Sz2 = sz2,
    };
}
