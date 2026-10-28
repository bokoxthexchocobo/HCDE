namespace HCDE.Playsim.Tests;

public class VmP3DFloorColormapAddTests
{
    [Fact]
    public void GetColormap_MergesTargetFadeWhenNeeded()
    {
        var target = new VmP3DFloorColormapAdd.MapSector();
        target.Colormap.FadeColor = 0xff0000;
        var model = new VmP3DFloorColormapAdd.MapSector();
        model.Colormap.FadeColor = 0;
        model.Colormap.LightColor = 0x11;
        var ff = new VmP3DFloorColormapAdd.FFloor { Target = target, Model = model };
        var cm = VmP3DFloorColormapAdd.GetColormap(ff);
        Assert.Equal(0xff0000, cm.FadeColor);
        Assert.Equal(0x11, cm.LightColor);
    }

    [Fact]
    public void Add3DFloor_PushesFloorOntoTargetSector()
    {
        var sec = new VmP3DFloorColormapAdd.MapSector();
        var sec2 = new VmP3DFloorColormapAdd.MapSector();
        sec2.FloorPlane.ZAtPoint = (_, _) => 0;
        sec2.CeilingPlane.ZAtPoint = (_, _) => 128;
        VmP3DFloorColormapAdd.Add3DFloor(sec, sec2, null, VmP3DFloorColormapAdd.FfExists, 200);
        Assert.Single(sec.XFloor.FFloors);
        Assert.Same(sec2, sec.XFloor.FFloors[0].Model);
    }

    [Fact]
    public void GetBlend_ReturnsModelFadeWhenNotFadeWalls()
    {
        var target = new VmP3DFloorColormapAdd.MapSector();
        target.Colormap.FadeColor = 0x10;
        var model = new VmP3DFloorColormapAdd.MapSector();
        model.Colormap.FadeColor = 0x20;
        var ff = new VmP3DFloorColormapAdd.FFloor { Target = target, Model = model, Flags = 0 };
        Assert.Equal(0x20, VmP3DFloorColormapAdd.GetBlend(ff));
    }

    [Fact]
    public void Add3DFloor_BothPlanesAddsInsideCopy()
    {
        var sec = new VmP3DFloorColormapAdd.MapSector();
        var sec2 = new VmP3DFloorColormapAdd.MapSector();
        VmP3DFloorColormapAdd.Add3DFloor(
            sec,
            sec2,
            null,
            VmP3DFloorColormapAdd.FfExists | VmP3DFloorColormapAdd.FfBothplanes,
            128);
        Assert.Equal(2, sec.XFloor.FFloors.Count);
    }

    [Fact]
    public void Add3DFloor_SkipsWhenAlreadyAttached()
    {
        var sec = new VmP3DFloorColormapAdd.MapSector();
        var sec2 = new VmP3DFloorColormapAdd.MapSector();
        sec2.XFloor.Attached.Add(sec);
        VmP3DFloorColormapAdd.Add3DFloor(sec, sec2, null, VmP3DFloorColormapAdd.FfExists, 200);
        Assert.Empty(sec.XFloor.FFloors);
    }

    [Fact]
    public void CheckFor3DFloorHit_DetectsSolidTop()
    {
        var model = new VmP3DFloorColormapAdd.MapSector { SecActTarget = new object() };
        var sec = new VmP3DFloorColormapAdd.MapSector();
        var ff = new VmP3DFloorColormapAdd.FFloor
        {
            Flags = VmP3DFloorColormapAdd.FfExists | VmP3DFloorColormapAdd.FfSolid,
            Model = model,
            Target = sec,
        };
        ff.Top.Plane = new VmP3DFloorColormapAdd.SecPlane();
        ff.Top.Plane.ZAtPoint = (_, _) => 32;
        sec.XFloor.FFloors.Add(ff);
        var mo = new VmP3DFloorColormapAdd.MapActor { Sector = sec, X = 0, Y = 0 };
        Assert.True(VmP3DFloorColormapAdd.CheckFor3DFloorHit(mo, 32, false));
        Assert.Same(model, mo.BlockingFloor);
    }
}
