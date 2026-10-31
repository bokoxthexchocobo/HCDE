namespace HCDE.Playsim.Tests;

public class VmVisiblePlanes1317SliceTests
{
    [Fact]
    public void Find_HashesTheOriginalDistanceAndRewritesSkyFlats()
    {
        var list = new VmVisiblePlanes1317();
        list.SkyFlat = 99;
        list.ViewpointExtraLight = 2;
        list.StackedViewPos = new VmVisiblePlanes1317.Vec3 { X = 1, Y = 2, Z = 3 };
        list.ViewPos = list.StackedViewPos;
        var floor = new VmVisiblePlanes1317.PlaneEq();
        floor.Set(0, 0, 2, 1);
        Assert.Equal(9, VmVisiblePlanes1317.CalcHash(2, 3, 1));
        object colormap = new object();
        var xform = new VmVisiblePlanes1317.Xform { XOffs = 4, YOffs = 1, BaseYOffs = 2, XScale = 2, YScale = 3 };

        VmVisiblePlanes1317.VisPlane first = list.FindPlane(
            floor, 2, 3, foggy: true, 0.25, true, xform, 7, null, colormap,
            VmVisiblePlanes1317.FakeKind.Normal, 0);
        Assert.Same(first, list.Slot(9));
        Assert.Equal(3, first.LightLevel);
        Assert.Equal(VmVisiblePlanes1317.Opaque, first.Alpha);
        Assert.Equal(0, first.Sky);
        Assert.Null(first.Portal);
        Assert.True(first.Additive);
        Assert.Equal(320, first.Left);
        Assert.Equal(0, first.Right);
        Assert.Same(first, list.FindPlane(
            floor, 2, 3, foggy: true, 0.25, true, xform, 7, null, colormap,
            VmVisiblePlanes1317.FakeKind.Normal, 0));

        list.ViewpointExtraLight = 2;
        VmVisiblePlanes1317.VisPlane bright = list.FindPlane(
            floor, 2, 3, foggy: false, 0.25, true, xform, 7, null, colormap,
            VmVisiblePlanes1317.FakeKind.Normal, 0);
        Assert.NotSame(first, bright);
        Assert.Equal(3 + 32, bright.LightLevel);
        Assert.Same(bright, list.Slot(VmVisiblePlanes1317.CalcHash(2, 35, 1)));

        list.ViewPos = new VmVisiblePlanes1317.Vec3 { X = 9 };
        VmVisiblePlanes1317.VisPlane moved = list.FindPlane(
            floor, 2, 35, foggy: true, 0, false, xform, 0, null, colormap,
            VmVisiblePlanes1317.FakeKind.Normal, 0);
        Assert.NotSame(bright, moved);
        Assert.True(moved.ViewPos.Same(list.StackedViewPos));
        Assert.False(moved.ViewPos.Same(list.ViewPos));

        var skyHeight = new VmVisiblePlanes1317.PlaneEq();
        skyHeight.Set(5, 6, 4, 8);
        var skyPortal = new VmVisiblePlanes1317.PlanePortal
        {
            Kind = VmVisiblePlanes1317.PortalKind.SkyViewpoint,
        };
        VmVisiblePlanes1317.VisPlane sky = list.FindPlane(
            skyHeight, 99, 50, foggy: false, 0.5, true, xform, 4, skyPortal, colormap,
            VmVisiblePlanes1317.FakeKind.Normal, 0);
        Assert.Same(sky, list.Slot(VmVisiblePlanes1317.MaxVisplanes));
        Assert.Equal(0, sky.LightLevel);
        Assert.False(sky.Additive);
        Assert.Equal(0, sky.Xform.XOffs);
        Assert.Equal(1, sky.Xform.XScale);
        Assert.Equal(0, sky.Height.D);
        Assert.Equal(4, sky.Height.Z);
        Assert.Equal(32768, sky.Alpha);
        var otherDistance = new VmVisiblePlanes1317.PlaneEq();
        otherDistance.Set(1, 1, 4, 99);
        Assert.Same(sky, list.FindPlane(
            otherDistance, 99, 1, false, 0, false, default, 0, skyPortal, null,
            VmVisiblePlanes1317.FakeKind.Normal, 0));

        var stacked = new VmVisiblePlanes1317.PlanePortal
        {
            Kind = VmVisiblePlanes1317.PortalKind.StackedSectorThing,
        };
        list.StackedYaw = 15;
        VmVisiblePlanes1317.VisPlane zeroAlpha = list.FindPlane(
            floor, 1, 0, true, 0, false, xform, 3, stacked, colormap,
            VmVisiblePlanes1317.FakeKind.Normal, 0);
        Assert.Same(zeroAlpha, list.FindPlane(
            floor, 9, 80, true, 0, false, default, 3, stacked, null,
            VmVisiblePlanes1317.FakeKind.Normal, 0));
        var shifted = xform;
        shifted.YOffs = 3;
        shifted.BaseYOffs = 0;
        VmVisiblePlanes1317.VisPlane painted = list.FindPlane(
            floor, 4, 6, true, 0.5, true, xform, 3, stacked, colormap,
            VmVisiblePlanes1317.FakeKind.Normal, 0);
        Assert.NotSame(zeroAlpha, painted);
        Assert.Same(painted, list.FindPlane(
            floor, 4, 6, true, 0.5, true, shifted, 3, stacked, colormap,
            VmVisiblePlanes1317.FakeKind.Normal, 0));

        list.InsideSky = portal => portal == skyPortal;
        VmVisiblePlanes1317.VisPlane fake = list.FindPlane(
            floor, 2, 3, true, 0.25, true, xform, 11, skyPortal, colormap,
            VmVisiblePlanes1317.FakeKind.FakeFloor, 0x1234);
        Assert.Equal(unchecked((int)(0x80000000u | 0x1234u)), fake.Sky);
        Assert.True(fake.Sky < 0);
        Assert.Null(fake.Portal);
        Assert.Equal(VmVisiblePlanes1317.Opaque, fake.Alpha);
        Assert.NotSame(fake, list.Slot(VmVisiblePlanes1317.MaxVisplanes));
    }

    [Fact]
    public void Range_KeepsFakesAndRestoresTheCameraAfterHeightRedraw()
    {
        var list = new VmVisiblePlanes1317(mainThread: true);
        list.ViewActive = true;
        list.ViewWidth = 40;
        list.StackedViewPos = new VmVisiblePlanes1317.Vec3 { X = 4, Y = 5, Z = 6 };
        list.ViewPos = new VmVisiblePlanes1317.Vec3 { X = 8 };
        list.ViewYaw = 12;
        var height = new VmVisiblePlanes1317.PlaneEq();
        height.Set(0, 0, 2, -4);
        Assert.Equal(2, height.Zat0());
        var xform = new VmVisiblePlanes1317.Xform { XScale = 1, YScale = 1 };
        VmVisiblePlanes1317.VisPlane third = list.FindPlane(
            height, 2, 0, true, 0, false, xform, 0, null, null,
            VmVisiblePlanes1317.FakeKind.Normal, 0);
        VmVisiblePlanes1317.VisPlane second = list.FindPlane(
            height, 2, 0, true, 0, false, xform, 0, null, null,
            VmVisiblePlanes1317.FakeKind.Normal, 0);
        VmVisiblePlanes1317.VisPlane first = list.FindPlane(
            height, 2, 0, true, 0, false, xform, 0, null, null,
            VmVisiblePlanes1317.FakeKind.Normal, 0);
        Assert.Same(first, list.Slot(VmVisiblePlanes1317.CalcHash(2, 0, -4)));
        Assert.Same(second, first.Next);
        Assert.Same(third, second.Next);
        first.Sky = unchecked((int)0x80000000);
        second.Sky = 0;
        third.Sky = -2;
        int bucket = VmVisiblePlanes1317.CalcHash(2, 0, -4);
        list.ClearKeepFakePlanes();
        Assert.Same(first, list.Slot(bucket));
        Assert.Same(third, first.Next);
        Assert.Null(third.Next);
        Assert.Null(second.Next);

        var portal = new VmVisiblePlanes1317.PlanePortal
        {
            Kind = VmVisiblePlanes1317.PortalKind.Other,
        };
        first.Portal = portal;
        first.Left = 10;
        first.Right = 12;
        first.Top = new ushort[40];
        Array.Fill(first.Top, VmVisiblePlanes1317.EmptyColumn);
        Assert.Same(first, list.GetRange(first, 20, 22));
        Assert.Equal(10, first.Left);
        Assert.Equal(22, first.Right);

        first.Left = 10;
        first.Right = 20;
        first.Top[15] = 5;
        first.Lights = new object();
        list.ViewActive = true;
        VmVisiblePlanes1317.VisPlane split = list.GetRange(first, 8, 22);
        Assert.NotSame(first, split);
        Assert.Equal(10, first.Left);
        Assert.Equal(20, first.Right);
        Assert.Equal(8, split.Left);
        Assert.Equal(22, split.Right);
        Assert.Same(first.Lights, split.Lights);
        Assert.Null(split.Top);
        Assert.Same(split, list.Slot(VmVisiblePlanes1317.MaxVisplanes));
        Assert.True(list.HasPortalPlanes());

        list.ViewActive = false;
        first.Top[15] = 5;
        VmVisiblePlanes1317.VisPlane hashed = list.GetRange(first, 8, 18);
        Assert.Same(hashed, list.Slot(VmVisiblePlanes1317.CalcHash(first.Picnum, first.LightLevel, first.Height.D)));
        Assert.Throws<InvalidOperationException>(() => list.GetRange(first, 0, 0));

        Assert.Same(split, list.PopFirstPortalPlane());
        Assert.Null(split.Next);
        Assert.False(list.HasPortalPlanes());

        list.ClearPortalPlanes();
        hashed.Sky = 0;
        list.MainThread = true;
        int drawn = list.Render();
        Assert.Equal(1, list.Clocks);
        Assert.Equal(1, list.Unclocks);
        Assert.Equal(1, drawn);
        Assert.Equal(VmVisiblePlanes1317.Opaque, list.Draws[0].Alpha);
        Assert.False(list.Draws[0].Additive);
        Assert.False(list.Draws[0].Masked);
        Assert.Same(hashed, list.Draws[0].Plane);

        list.Draws.Clear();
        list.MainThread = false;
        int clocks = list.Clocks;
        list.PortalUniq = 4;
        Assert.Equal(0, list.Render());
        Assert.Equal(clocks, list.Clocks);

        list.PortalUniq = 0;
        first.Sky = unchecked((int)(0x80001234));
        first.Additive = true;
        first.MirrorFlags = 7;
        first.ViewPos = new VmVisiblePlanes1317.Vec3 { X = 30 };
        first.ViewAngle = 44;
        third.Sky = -1;
        third.Height = height;
        third.MirrorFlags = 3;
        third.ViewPos = new VmVisiblePlanes1317.Vec3 { X = 31 };
        third.ViewAngle = 45;
        third.Additive = false;
        list.RenderHeight(2);
        Assert.Equal(2, list.Draws.Count);
        Assert.Equal(0x1234, list.Draws[0].Alpha);
        Assert.True(list.Draws[0].Masked);
        Assert.True(list.Draws[0].Additive);
        Assert.Equal(30, list.Draws[0].Pos.X);
        Assert.Equal(7, list.Draws[0].Mirror);
        Assert.Equal(3, list.PortalMirror);
        Assert.Equal(8, list.ViewPos.X);
        Assert.Equal(12, list.ViewYaw);
        list.RenderHeight(9);
        Assert.Equal(2, list.Draws.Count);

        list.Clear();
        Assert.Null(list.Slot(bucket));
        Assert.False(list.HasPortalPlanes());
    }
}
