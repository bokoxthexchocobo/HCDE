using S = HCDE.Playsim.VmFarClipLine1389Slice;

namespace HCDE.Playsim.Tests;

public class VmFarClipLine1389SliceTests
{
    static S.Seg Edge(double x1, double y1, double x2, double y2) => new()
    {
        V1 = new S.Vec { X = x1, Y = y1 },
        V2 = new S.Vec { X = x2, Y = y2 }
    };

    static S.Host Ready()
    {
        var host = new S.Host { Width = 4, Sx1 = 1, Sx2 = 3, WindowRight = 8 };
        host.Ceiling = new short[4];
        host.Floor = new short[4];
        for (int i = 0; i < 4; i++) host.Floor[i] = 100;
        return host;
    }

    [Fact]
    public void RenderRejectsBackfacesWindowsAndPortals()
    {
        var back = new S.Far(Ready());
        back.Render(Edge(0, 1, 0, 2), new S.Sub(), new S.Mark(), new S.Mark());
        Assert.Equal(0, back.Host.Inits);
        Assert.False(back.Host.Clipped);

        var closed = Assert.Throws<NullReferenceException>(() => new S.Far(Ready()).Render(Edge(-1, 1, 1, 1), null, null, null));
        Assert.NotNull(closed);

        var init = Ready();
        init.InitRejects = true;
        var rejected = new S.Far(init);
        rejected.Render(Edge(-1, 1, 1, 1), new S.Sub(), null, null);
        Assert.Equal(1, init.Inits);
        Assert.False(init.Clipped);

        var right = Ready();
        right.Sx1 = 8;
        var atEdge = new S.Far(right);
        atEdge.Render(Edge(-1, 1, 1, 1), new S.Sub(), null, null);
        Assert.False(right.Clipped);

        var left = Ready();
        left.Sx2 = 0;
        left.WindowLeft = 0;
        new S.Far(left).Render(Edge(-1, 1, 1, 1), new S.Sub(), null, null);
        Assert.False(left.Clipped);

        var bare = Ready();
        var missing = Edge(-1, 1, 1, 1);
        missing.HasDef = false;
        new S.Far(bare).Render(missing, new S.Sub(), null, null);
        Assert.False(bare.Clipped);

        var portal = Ready();
        portal.HasPortal = true;
        portal.PortalClips = true;
        new S.Far(portal).Render(Edge(-1, 1, 1, 1), new S.Sub(), null, null);
        Assert.False(portal.Clipped);

        var sky = Ready();
        sky.HasPortal = true;
        sky.PortalClips = true;
        sky.InSkybox = true;
        var sub = new S.Sub();
        sub.Sector.Ceiling.V1 = 3;
        sub.Sector.Ceiling.V2 = 4;
        sub.Sector.Floor.V1 = 1;
        sub.Sector.Floor.V2 = 2;
        var drawn = new S.Far(sky);
        drawn.Render(Edge(-1, 1, 1, 1), sub, new S.Mark(), new S.Mark());
        Assert.True(sky.Clipped);
        Assert.False(drawn.Prepped);
        Assert.Equal(3.0, drawn.Ceil1);
        Assert.Equal(4.0, drawn.Ceil2);
        Assert.Equal(1.0, drawn.Floor1);
        Assert.Equal(2.0, drawn.Floor2);
    }

    [Fact]
    public void SegmentCopiesFloorYAndMarksFakePlanes()
    {
        var host = Ready();
        host.Mirror = S.XFlip;
        host.Ceiling[1] = 40;
        host.Ceiling[2] = 40;
        host.Floor[1] = 70;
        host.Floor[2] = 70;
        host.FillBottom = row => { row[0] = 9; row[1] = 100; row[2] = 100; row[3] = 9; };
        var ceiling = new S.Mark { Top = new short[4], Bottom = new short[4] };
        var floor = new S.Mark { Top = new short[4], Bottom = new short[4] };
        var nextCeiling = new S.Mark { Top = new short[4], Bottom = new short[4] };
        var nextFloor = new S.Mark { Top = new short[4], Bottom = new short[4] };
        host.Range = (mark, _, _) => ReferenceEquals(mark, ceiling) ? nextCeiling : ReferenceEquals(mark, floor) ? nextFloor : mark;
        var far = new S.Far(host);
        far.Render(Edge(-1, 1, 1, 1), new S.Sub(), floor, ceiling);
        Assert.True(far.Segment(1, 3));
        Assert.Equal(1, host.Projects);
        Assert.True(host.Flip);
        Assert.Equal(new short[] { 9, 100, 100, 9 }, far.Top);
        Assert.Equal((short)70, far.Bottom[1]);
        Assert.Same(nextCeiling, far.Ceiling);
        Assert.Equal((short)40, nextCeiling.Top[1]);
        Assert.Equal((short)70, nextCeiling.Bottom[1]);
        Assert.Equal((short)0, nextFloor.Top[1]);
        Assert.True(far.Segment(1, 3));
        Assert.Equal(1, host.Projects);

        var fakeHost = Ready();
        fakeHost.Fake = true;
        fakeHost.Opaque = S.Kind.FakeFloor;
        fakeHost.FakeFloor = new short[] { 0, 90, 90, 0 };
        fakeHost.Ceiling[1] = 40;
        fakeHost.Floor[1] = 70;
        fakeHost.FillBottom = row => row[1] = 100;
        var fakeFloor = new S.Mark { Top = new short[4], Bottom = new short[4] };
        var fake = new S.Far(fakeHost) { Floor = fakeFloor };
        Assert.True(fake.Segment(1, 2));
        Assert.Equal((short)70, fake.Floor.Top[1]);
        Assert.Equal((short)90, fake.Floor.Bottom[1]);

        var high = Ready();
        high.ViewHeight = 200;
        high.Fake = true;
        high.Opaque = S.Kind.FakeFloor;
        high.FakeFloor = new short[] { 0, 250, 0, 0 };
        high.Floor[1] = 300;
        var highFloor = new S.Mark { Top = new short[4], Bottom = new short[4] };
        var guard = new S.Far(high) { Floor = highFloor, Bottom = new short[4], Top = new short[4], Prepped = true };
        var abort = Assert.Throws<S.AssertAbort>(() => guard.Segment(1, 2));
        Assert.Equal("bottom <= viewheight", abort.Message);
        Assert.Equal((short)0, highFloor.Bottom[1]);

        high.Asserts = false;
        Assert.True(guard.Segment(1, 2));
        Assert.Equal((short)250, highFloor.Bottom[1]);
    }
}
