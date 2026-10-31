namespace HCDE.Playsim.Tests;

public class VmTrans1271SliceTests
{
    [Fact]
    public void Columns_SkipTheFrontAndAMissingPortal()
    {
        float eps = 1f / 65536f;
        var pass = new VmTrans1271 { PortalUniq = 4 };
        pass.Segs.Add(new VmPortalSeg1271 { Id = 1, HasCurline = false, Line = Mirror() });
        pass.Segs.Add(new VmPortalSeg1271 { Id = 2, Line = null });
        pass.Segs.Add(new VmPortalSeg1271 { Id = 3, Line = new VmLine1271() });
        pass.Segs.Add(new VmPortalSeg1271 { Id = 4, PortalUniq = 9, X1 = 0, X2 = 3, Line = Mirror() });
        var hidden = new VmLine1271 { PortalIndex = 0, PortalFlags = 1, Front = 1, Dx = 10 };
        pass.Segs.Add(new VmPortalSeg1271 { Id = 5, Side = 2, PortalUniq = 4, X1 = 5, X2 = 9, Line = hidden });
        var kept = new VmLine1271 { PortalIndex = 0, PortalFlags = 1, Front = 1, Dx = 10 };
        pass.Segs.Add(new VmPortalSeg1271 { Id = 6, PortalUniq = 4, X1 = 5, X2 = 9, Line = kept });
        pass.Segs.Add(new VmPortalSeg1271
        {
            Id = 7,
            PortalUniq = 4,
            X1 = 20,
            X2 = 24,
            Line = new VmLine1271 { PortalIndex = 2, Dx = 10 },
        });
        pass.Segs.Add(new VmPortalSeg1271
        {
            Id = 8,
            PortalUniq = 4,
            X1 = 20,
            X2 = 24,
            Line = new VmLine1271 { PortalIndex = 2, PortalFlags = 5, Dx = 10 },
        });
        pass.Collect();
        Assert.Equal(new[] { 4, 6, 8 }, pass.PortalSegs.Select(seg => seg.Id).ToArray());

        pass.InSkybox = true;
        Assert.False(pass.ClipColumn(6, 1, 1));
        pass.InSkybox = false;
        Assert.False(pass.ClipColumn(1, 1, 1));
        Assert.False(pass.ClipColumn(6, 1, 0));
        Assert.False(pass.ClipColumn(6, 1, (double)eps / 10.0));
        double back = Math.BitIncrement((double)eps) / 10.0;
        Assert.True(pass.ClipColumn(5, 1, back));
        Assert.True(pass.ClipColumn(8, 1, back));
        Assert.False(pass.ClipColumn(9, 1, back));
        Assert.True(pass.ClipColumn(22, 1, back));
        Assert.False(pass.ClipColumn(15, 1, back));

        var dead = new VmTrans1271(false);
        Assert.Throws<NullReferenceException>(() => dead.Clear());
        pass.Segs.Add(null);
        Assert.Throws<NullReferenceException>(() => pass.Collect());
        var broken = new VmTrans1271 { PortalUniq = 3 };
        broken.Sprites.Add(null);
        Assert.Throws<NullReferenceException>(() => broken.Render());
    }

    [Fact]
    public void Floors_DrawCeilingsBeforeTheUpwardPass()
    {
        var flat = new VmTrans1271 { MainThread = true, PortalUniq = 3 };
        flat.Sprites.Add(new VmMaskedSprite1271 { Id = 1, PortalUniq = 1 });
        flat.Sprites.Add(new VmMaskedSprite1271 { Id = 2, PortalUniq = 3 });
        flat.Sprites.Add(new VmMaskedSprite1271 { Id = 3, PortalUniq = 3 });
        flat.Segs.Add(new VmPortalSeg1271 { Id = 10, PortalUniq = 3, Flags = 0, X1 = 1, X2 = 2 });
        flat.Segs.Add(new VmPortalSeg1271 { Id = 11, PortalUniq = 9, Flags = 4, X1 = 1, X2 = 4 });
        flat.Segs.Add(new VmPortalSeg1271 { Id = 12, PortalUniq = 3, Flags = 4, X1 = 2, X2 = 6 });
        flat.Segs.Add(new VmPortalSeg1271 { Id = 13, PortalUniq = 3, Flags = 3, X1 = 0, X2 = 1 });
        flat.Segs.Add(new VmPortalSeg1271 { Id = 14, PortalUniq = 3, Flags = 8, X1 = 7, X2 = 8 });
        flat.Deinit();
        flat.Render();
        flat.Clear();
        Assert.Empty(flat.Sprites);
        Assert.Equal(
"""
voxel
clock idle
sort
groups
sprite 3 0 0 0 0 0
sprite 2 0 0 0 0 0
seg 12
seg 13
seg 14
unclock idle
""",
            string.Join('\n', flat.Log));

        var low = new VmHeight1271 { Height = 10 };
        var mid = new VmHeight1271 { Height = 30 };
        var high = new VmHeight1271 { Height = 50 };
        low.Next = mid;
        mid.Prev = low;
        mid.Next = high;
        high.Prev = mid;
        var stacked = new VmTrans1271
        {
            MainThread = true,
            Active = true,
            ClockSample = 4,
            ViewZ = 30,
            PortalUniq = 3,
            HeightTop = low,
            HeightCur = high,
        };
        stacked.Sprites.Add(new VmMaskedSprite1271 { Id = 2, PortalUniq = 3 });
        stacked.Segs.Add(new VmPortalSeg1271 { Id = 12, PortalUniq = 3, Flags = 8, X1 = 7, X2 = 8 });
        stacked.Render();
        Assert.Equal(0, stacked.Counter);
        Assert.True(stacked.Deleted);
        Assert.Equal(
"""
clock
sort
groups
sprite 2 0 1 0 0 50
seg 12
undrawn 12 7 8
plane 50
sprite 2 1 1 0 50 30
seg 12
undrawn 12 7 8
plane 30
sprite 2 1 0 1 10 0
seg 12
undrawn 12 7 8
plane 10
sprite 2 1 1 1 30 10
seg 12
undrawn 12 7 8
delete
unclock
""",
            string.Join('\n', stacked.Log));

        var onlyTop = new VmTrans1271
        {
            ViewZ = 0,
            HeightTop = new VmHeight1271 { Height = 100 },
            PortalUniq = 3,
        };
        onlyTop.Sprites.Add(new VmMaskedSprite1271 { Id = 1, PortalUniq = 3 });
        onlyTop.Render();
        Assert.True(onlyTop.Deleted);
        Assert.Equal(
"""
sort
groups
sprite 1 1 0 1 100 0
delete
""",
            string.Join('\n', onlyTop.Log));
    }

    static VmLine1271 Mirror() => new() { Special = VmTrans1271.LineMirror, Front = 1, Dx = 10 };
}
