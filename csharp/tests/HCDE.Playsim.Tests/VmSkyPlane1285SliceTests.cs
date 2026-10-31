using System.Globalization;
using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmSkyPlane1285SliceTests
{
    static string Num(double value)
    {
        return value.ToString("G9", CultureInfo.InvariantCulture);
    }

    static VmSkyTex1285 Tex(int id, int scaled, int width, double scaleX = 1, double scaleY = 1)
    {
        return new VmSkyTex1285
        {
            Id = id,
            ScaledHeight = scaled,
            Height = scaled,
            PhysicalHeight = scaled,
            HeightBits = 8,
            Width = width,
            ScaleX = scaleX,
            ScaleY = scaleY,
        };
    }

    [Fact]
    public void Setup_PicksTheOffsetAndTheSecondCylinderFromScaleY()
    {
        var sky = new VmSkyPlane1285
        {
            Sky1 = Tex(1, 256, 100, 1, 2),
            Sky2 = Tex(2, 16, 50, 4, 0.01),
            SkyMode = 2,
            Stretch = true,
            ViewWidth = 320,
            ViewHeight = 200,
            FreeLookHeight = 200,
        };
        sky.Sky1!.SkyOffset = 5;
        sky.Build();
        Assert.Equal(166, sky.SkyOffset);
        Assert.Equal("-69.5228426", Num(sky.SkyTextureMid));
        Assert.Equal("1.5390625", Num(sky.SkyScale));
        Assert.Equal("0.64974618", Num(sky.SkyIScale));
        Assert.Equal(1024, sky.Sky1Cyl);
        Assert.Equal(50, sky.Sky2Cyl);

        sky.Stretch = false;
        sky.Sky1 = Tex(1, 240, 10, 1, 1);
        sky.Build();
        Assert.Equal(150, sky.SkyOffset);
        Assert.Equal(-40, sky.SkyTextureMid);
        sky.Sky1 = Tex(1, 200, 10);
        sky.Build();
        Assert.Equal(110, sky.SkyOffset);
        sky.Sky1 = Tex(1, 199, 10);
        sky.Build();
        Assert.Equal(138, sky.SkyOffset);
        Assert.Equal(-28, sky.SkyTextureMid);
        sky.Sky1 = Tex(1, 127, 10);
        sky.Build();
        Assert.Equal(138, sky.SkyOffset);
        Assert.Equal(0, sky.SkyTextureMid);
        sky.OldFreeLook = true;
        sky.Sky1 = Tex(1, 256, 2000, 1, 1);
        sky.Build();
        Assert.Equal(0, sky.SkyOffset);
        Assert.Equal(2000, sky.Sky1Cyl);

        sky.ViewWidth = 0;
        sky.Stretch = false;
        sky.Sky1 = Tex(1, 16, 8);
        sky.Build();
        Assert.Equal(0, sky.SkyScale);
        Assert.Equal(0, sky.SkyIScale);

        var missing = new VmSkyPlane1285 { Sky1 = Tex(1, 16, 8), Sky2 = null };
        missing.Build();
        Assert.Equal(0, missing.SkyOffset);
        Assert.Equal(0, missing.Sky1Cyl);
    }

    [Fact]
    public void Render_KeepsTheSky1ModulusAndFlipsOnlyTheAngleX()
    {
        var sky = new VmSkyPlane1285
        {
            Sky1 = Tex(1, 16, 64),
            Sky2 = Tex(2, 16, 50, 1, 0.01),
            ViewWidth = 320,
            ViewHeight = 200,
            FreeLookHeight = 200,
            LinearSky = true,
            SkyMode = 2,
            Sky1Pos = 100,
            Sky2Pos = 9,
            Cap = tex => (tex.Id, tex.Id + 1),
        };
        sky.Sky1!.HeightBits = 4;
        sky.Sky1.PhysicalHeight = 16;
        sky.Sky1.Height = 16;
        sky.Build();
        sky.Render(1, 0, 0, 2, new[] { 10, 5 }, new[] { 14, 5 });
        Assert.Equal(1, sky.FrontId);
        Assert.Equal(0, sky.BackId);
        Assert.Equal(100, sky.FrontPos);
        Assert.Equal(0u, sky.SkyFlip);
        Assert.Equal(
            "single 0 10 4 4217896960 1048576 8388708 409600 1 1 2 0",
            Assert.Single(sky.Columns));

        sky.Mirror = true;
        sky.Render(1, 0, 10, 11, new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10 }, new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 12 });
        Assert.Equal(
            "single 10 10 2 4217896960 1048576 59244644 2892800 1 1 2 310",
            Assert.Single(sky.Columns));
        sky.Mirror = false;

        sky.LinearSky = false;
        sky.XToView[0] = 0x10000;
        sky.Render(1, 0, 0, 1, new[] { 10 }, new[] { 11 });
        Assert.Contains(" 1124 ", Assert.Single(sky.Columns));
        sky.LinearSky = true;

        sky.Sky1.Height = 100;
        sky.Sky1.PhysicalHeight = 100;
        sky.Sky1.HeightBits = 6;
        sky.Sky1.ScaleY = 1;
        sky.Render(1, 0, 0, 1, new[] { 10 }, new[] { 12 });
        Assert.Equal(100, sky.LastDrawHeight);
        Assert.Contains(" 1761607 167772 ", Assert.Single(sky.Columns));
        sky.Sky1.Height = 400;
        sky.Sky1.PhysicalHeight = 400;
        sky.Sky1.ScaleY = 100;
        sky.Render(1, 0, 0, 1, new[] { 10 }, new[] { 11 });
        Assert.Equal((short)-25536, sky.LastDrawHeight);
        sky.Sky1.Height = 16;
        sky.Sky1.PhysicalHeight = 16;
        sky.Sky1.HeightBits = 4;
        sky.Sky1.ScaleY = 1;

        sky.Flags = VmSkyPlane1285.LevelDoubleSky | VmSkyPlane1285.LevelSwapSkies;
        sky.Sky1Pos = 67108864 + 7;
        sky.Sky2Pos = 3276800 + 3;
        sky.Render(1, 0, 0, 1, new[] { 10 }, new[] { 11 });
        Assert.Equal(1, sky.FrontId);
        Assert.Equal(2, sky.BackId);
        Assert.Equal(7, sky.FrontPos);
        Assert.Equal(3, sky.BackPos);
        Assert.StartsWith("double ", Assert.Single(sky.Columns));

        sky.Flags = VmSkyPlane1285.LevelSwapSkies;
        sky.Sky1Pos = 15;
        sky.Render(1, 0, 0, 1, new[] { 10 }, new[] { 11 });
        Assert.Equal(2, sky.FrontId);
        Assert.Equal(0, sky.BackId);
        Assert.Equal(15, sky.FrontPos);

        sky.Flags = 0;
        sky.Sky1Pos = -10;
        sky.Render(1, 0, 0, 1, new[] { 10 }, new[] { 11 });
        Assert.Equal(-10, sky.FrontPos);
        sky.Sky2Pos = 67108864 + 4;
        sky.Render(1, VmSkyPlane1285.SkyFlatFlag, 0, 1, new[] { 10 }, new[] { 11 });
        Assert.Equal(2, sky.FrontId);
        Assert.Equal(4, sky.FrontPos);
        Assert.Equal(50, sky.FrontCyl);

        sky.Flags = VmSkyPlane1285.LevelForceTiled | VmSkyPlane1285.LevelSwapSkies;
        sky.Stretch = true;
        sky.YawDegrees = 90;
        sky.Lines.Add(new VmSkyLine1285());
        sky.Lines.Add(new VmSkyLine1285
        {
            Top = new VmSkyTex1285 { Id = 3, Valid = true, YOffset = 1, ScaledHeight = 16, Height = 16 },
            Bottom = new VmSkyTex1285 { Id = 4, Valid = true, ScaledHeight = 16, Height = 16, Width = 8, XOffset = 0.5, YOffset = 40 },
            Arg2 = 0,
        });
        sky.Render(1, VmSkyPlane1285.SkyFlatFlag | 2, 0, 1, new[] { 10 }, new[] { 11 });
        Assert.Equal(4, sky.FrontId);
        Assert.Equal(~0u, sky.SkyFlip);
        Assert.Equal(unchecked(VmSkyPlane1285.Angle90 + 32768u), sky.SkyAngle);
        Assert.Equal("0.524590164", Num(sky.SkyMid));
        Assert.Equal(0, sky.Columns[0].Split(' ')[8] == "0" ? 0 : 1);

        sky.Lines[1].Bottom!.Missing = true;
        sky.Lines[1].Arg2 = 1;
        sky.Sky1Pos = 4;
        sky.Render(1, VmSkyPlane1285.SkyFlatFlag | 2, 0, 1, new[] { 10 }, new[] { 11 });
        Assert.Equal(2, sky.FrontId);
        Assert.Equal(0u, sky.SkyFlip);
        Assert.Equal(4, sky.FrontPos);
    }
}
