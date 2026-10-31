using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmLighting1277SliceTests
{
    [Fact]
    public void FogTable_DividesInIntegersAndIgnoresNightVision()
    {
        var level = new VmLighting1277();
        Assert.Equal(70, level.DistFog);
        Assert.Equal(8, level.WeaponLight);
        Assert.False(level.EnhancedNightVision);
        Assert.Equal("105", VmLighting1277.Num(level.Table0[0]));
        Assert.Equal("104", VmLighting1277.Num(level.Table0[1]));
        Assert.Equal("50", VmLighting1277.Num(level.Table0[127]));
        Assert.Equal("50", VmLighting1277.Num(level.Table0[128]));
        Assert.Equal("35", VmLighting1277.Num(level.Table0[163]));
        Assert.Equal("35", VmLighting1277.Num(level.Table0[164]));
        Assert.Equal("8", VmLighting1277.Num(level.Table0[215]));
        Assert.Equal("1", VmLighting1277.Num(level.Table0[229]));
        Assert.Equal("0", VmLighting1277.Num(level.Table0[230]));
        Assert.Equal("0", VmLighting1277.Num(level.Table0[255]));
        Assert.Equal("227", VmLighting1277.Num(level.Table1[0]));
        Assert.Equal("226", VmLighting1277.Num(level.Table1[1]));
        Assert.Equal("42", VmLighting1277.Num(level.Table1[127]));
        Assert.Equal("7", VmLighting1277.Num(level.Table1[128]));
        Assert.Equal("4.215909", VmLighting1277.Num(level.Table1[163]));
        Assert.Equal("4.13636351", VmLighting1277.Num(level.Table1[164]));
        Assert.Equal("0.0795454532", VmLighting1277.Num(level.Table1[215]));
        Assert.Equal("0", VmLighting1277.Num(level.Table1[216]));

        level.EnhancedNightVision = true;
        level.WeaponLight = 99;
        level.SetDistFog(70);
        Assert.Equal("105", VmLighting1277.Num(level.Table0[0]));
        Assert.Equal("227", VmLighting1277.Num(level.Table1[0]));
        Assert.True(level.EnhancedNightVision);
        Assert.Equal(99, level.WeaponLight);

        level.SetDistFog(0);
        Assert.Equal("0", VmLighting1277.Num(level.Table0[0]));
        Assert.Equal("6", VmLighting1277.Num(level.Table1[0]));
        Assert.Equal("0", VmLighting1277.Num(level.Table1[128]));

        level.SetDistFog(-70);
        Assert.Equal("-105", VmLighting1277.Num(level.Table0[0]));
        Assert.Equal("-35", VmLighting1277.Num(level.Table0[164]));
        Assert.Equal("-215", VmLighting1277.Num(level.Table1[0]));
    }

    [Fact]
    public void Levels_ClampAfterTheCurveAndKeepTheEmptyFogArms()
    {
        var level = new VmLighting1277();
        Assert.Equal(0, level.CalcLightLevel(VmLighting1277.Doom, 0, 50, false, 0, false));
        Assert.Equal(0, level.CalcLightLevel(VmLighting1277.Doom, -4, 80, false, 0, false));
        Assert.Equal(22, level.CalcLightLevel(VmLighting1277.Doom, 101, 0, false, 0, false));
        Assert.Equal(15, level.CalcLightLevel(VmLighting1277.Doom, 101, -30, false, 0, false));
        Assert.Equal(12, level.CalcLightLevel(VmLighting1277.Doom, 50, 10, false, 0, false));
        Assert.Equal(210, level.CalcLightLevel(VmLighting1277.Doom, 200, 10, false, 0, false));
        Assert.Equal(85, level.CalcLightLevel(VmLighting1277.DoomBright, 80, 5, false, 0, false));
        Assert.Equal(85, level.CalcLightLevel(VmLighting1277.Doom, 80, 5, true, 0, false));
        Assert.Equal(12, level.CalcLightLevel(VmLighting1277.LinearStandard, 50, 10, true, 0, true));
        Assert.Equal(116, level.CalcLightLevel(VmLighting1277.LinearStandard, 150, 3, true, 0, true));
        Assert.Equal(16, level.CalcLightLevel(VmLighting1277.ZDoomSoftware, 80, 0, false, 4, false));
        Assert.Equal(85, level.CalcLightLevel(VmLighting1277.ZDoomSoftware, 80, 5, false, 0, false));
        Assert.Equal(39, level.CalcLightLevel(VmLighting1277.Build, 110, 0, false, 1, false));
        Assert.Equal(255, level.CalcLightLevel(VmLighting1277.LinearStandard, 250, 20, false, 0, false));
        Assert.Equal(1, level.CalcLightLevel(VmLighting1277.Doom, 40, -100, false, 0, false));
        Assert.Equal(22, VmLighting1277.RoundHalfEven(192f - (192 - 101) * 1.87f));
        Assert.Equal(140, VmLighting1277.RoundHalfEven(192f - (192 - 164) * 1.87f));

        uint tint = 0x0180c040;
        Assert.Equal("0180c040", level.CalcLightColor(VmLighting1277.ZDoomSoftware, 128, tint, 0).ToString("x8"));
        Assert.Equal("ff406020", level.CalcLightColor(VmLighting1277.Doom, 128, tint, 0).ToString("x8"));
        Assert.Equal("ff80a05f", level.CalcLightColor(VmLighting1277.ZDoomSoftware, 128, tint, 128).ToString("x8"));
        Assert.Equal("ff000102", level.CalcLightColor(VmLighting1277.Doom, 200, 0x00010203, 0).ToString("x8"));
        Assert.Equal("ff909090", level.CalcLightColor(VmLighting1277.Doom, 400, 0x00ffffff, 0).ToString("x8"));
        Assert.Equal("fffefefe", level.CalcLightColor(VmLighting1277.Doom, -10, 0, 200).ToString("x8"));

        Assert.Equal("18", VmLighting1277.Num(level.GetFogDensity(VmLighting1277.DoomLegacy, 10, 0, 40, 0)));
        Assert.Equal(VmLighting1277.DoomLegacy, level.LastFogMode);
        level.FogDensity = 12;
        Assert.Equal("12", VmLighting1277.Num(level.GetFogDensity(VmLighting1277.DoomLegacy, 10, 0, 40, 0)));
        level.FogDensity = 0;
        Assert.Equal("-3", VmLighting1277.Num(level.GetFogDensity(VmLighting1277.Doom, 10, 0x00ffffff, -3, 0)));
        Assert.Equal("105", VmLighting1277.Num(level.GetFogDensity(VmLighting1277.LinearStandard, 0, 0, 0, 0)));
        Assert.Equal("0", VmLighting1277.Num(level.GetFogDensity(VmLighting1277.Doom, 300, 0, 0, 0)));
        Assert.Equal("227", VmLighting1277.Num(level.GetFogDensity(VmLighting1277.Doom, -4, 0, 0, 0)));
        Assert.Equal("0", VmLighting1277.Num(level.GetFogDensity(VmLighting1277.DoomSoftware, 10, 0, 0, 0)));
        Assert.Equal(VmLighting1277.DoomSoftware, level.LastFogMode);
        Assert.Equal("213", VmLighting1277.Num(level.GetFogDensity(VmLighting1277.DoomSoftware, 10, 0, 0, 3)));
        Assert.Equal(VmLighting1277.Doom, level.LastFogMode);
        Assert.Equal("213", VmLighting1277.Num(level.GetFogDensity(VmLighting1277.Build, 10, 0, 0, 0)));
        level.Flags3 = VmLighting1277.NoLightFade;
        Assert.Equal("0", VmLighting1277.Num(level.GetFogDensity(VmLighting1277.Doom, 10, 0, 0, 0)));
        level.Flags3 = 0;
        level.OutsideFogDensity = 9;
        level.OutsideFog = 0x00112233;
        Assert.Equal("9", VmLighting1277.Num(level.GetFogDensity(VmLighting1277.Doom, 10, 0xff112233, 0, 0)));
        level.OutsideFog = 0xff112233;
        Assert.Equal("245", VmLighting1277.Num(level.GetFogDensity(VmLighting1277.Doom, 10, 0x00112233, 0, 0)));
        level.OutsideFogDensity = 0;
        level.FogDensity = 15;
        Assert.Equal("15", VmLighting1277.Num(level.GetFogDensity(VmLighting1277.Doom, 10, 0x00000001, 0, 0)));
        level.FogDensity = 0;
        Assert.Equal("55", VmLighting1277.Num(level.GetFogDensity(VmLighting1277.Doom, 200, 0x00000001, 0, 0)));
        Assert.Equal("30", VmLighting1277.Num(level.GetFogDensity(VmLighting1277.Doom, 247, 0x00000001, 0, 0)));
        Assert.Equal("0", VmLighting1277.Num(level.GetFogDensity(VmLighting1277.Doom, 248, 0x00000001, 0, 0)));
        Assert.Equal("255", VmLighting1277.Num(level.GetFogDensity(VmLighting1277.Doom, -5, 0x00000001, 0, 0)));

        level.SkyFlat = 9;
        var front = new VmLightSector1277 { Fade = 0x00010203, LightLevel = 100, Ceiling = 1 };
        var back = new VmLightSector1277 { Fade = 0, LightLevel = 250, Ceiling = 2 };
        Assert.False(level.CheckFog(front, front, VmLighting1277.Doom));
        Assert.False(level.CheckFog(new VmLightSector1277 { Fade = 0, LightLevel = 10, Ceiling = 1 }, back, VmLighting1277.Doom));
        front.Fade = 0x80010203;
        front.LightLevel = 255;
        Assert.True(level.CheckFog(front, back, VmLighting1277.Doom));
        front.Fade = 0x00010203;
        front.LightLevel = 248;
        Assert.False(level.CheckFog(front, back, VmLighting1277.Doom));
        front.LightLevel = 100;
        back.Fade = 0x00040506;
        back.LightLevel = 200;
        Assert.False(level.CheckFog(front, back, VmLighting1277.Doom));
        back.LightLevel = 250;
        Assert.True(level.CheckFog(front, back, VmLighting1277.Doom));
        front.Ceiling = 9;
        back.Ceiling = 9;
        Assert.False(level.CheckFog(front, back, VmLighting1277.Doom));
        back.Ceiling = 2;
        Assert.True(level.CheckFog(front, back, VmLighting1277.Doom));
        back.Fade = 0;
        Assert.True(level.CheckFog(front, back, VmLighting1277.Doom));
        back.Fade = 0x00040506;
        level.FogDensity = 4;
        Assert.False(level.CheckFog(front, back, VmLighting1277.DoomLegacy));
        back.Fade = 0;
        Assert.True(level.CheckFog(front, back, VmLighting1277.DoomLegacy));
        level.FogDensity = 0;
        level.OutsideFogDensity = 9;
        level.OutsideFog = 0x00040506;
        back.Fade = 0x11040506;
        Assert.False(level.CheckFog(front, back, VmLighting1277.Doom));
        level.OutsideFog = 0xff040506;
        Assert.True(level.CheckFog(front, back, VmLighting1277.Doom));
    }
}
