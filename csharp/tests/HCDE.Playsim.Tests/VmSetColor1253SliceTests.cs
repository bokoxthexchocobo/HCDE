namespace HCDE.Playsim.Tests;

public class VmSetColor1253SliceTests
{
    const string ColorGolden =
"""
color 00FFFFFF 0.500000 desat=0
soft 255
color 00FFFFFF 0.250000 desat=0
nosoft
calclevel mode=2 light=200 rel=-5 weapon=1 blend=4 -> 40
calccolor mode=2 light=40 pe=00112233 blend=4 -> 00ABCDEF
color 00ABCDEF 0.750000 desat=9
nosoft
calclevel mode=5 light=300 rel=10 weapon=0 blend=0 -> 40
calccolor mode=5 light=40 pe=00112233 blend=0 -> 00ABCDEF
color 00ABCDEF 1.000000 desat=9
soft 255 blend=0
calclevel mode=8 light=10 rel=-40 weapon=0 blend=0 -> 40
calccolor mode=8 light=40 pe=00112233 blend=0 -> 00ABCDEF
color 00ABCDEF 1.000000 desat=9
soft 0 blend=0
---- shader
parms 1.000000 0.000000
parms 7.450000 128.000000
parms 1.000000 0.000000
parms 1.007895 128.000000
parms 1.750000 509.333313
parms 1.000000 0.000000

""";

    const string FogGolden =
"""
fogprep dens=105.000000 color=00808080 sky=1 fogmode=2
parms 1.000000 0.000000
fog on
fog 00808080 105.000000
fogprep dens=105.000000 color=00808080 sky=1 fogmode=0
fog off
fog 00000000 0.000000
fogprep dens=0.000000 color=00000000 sky=0 fogmode=2
fog off
fog 00000000 0.000000
fogdensity mode=2 light=0 color=FF000000 sector=12 blend=0 -> 40.000000
fogprep dens=40.000000 color=00000000 sky=0 fogmode=2
calclevel mode=2 light=0 rel=3 weapon=0 blend=0 -> 10
parms 7.450000 128.000000
fog on
fog 00000000 40.000000
fogdensity mode=2 light=64 color=80ABCDEF sector=7 blend=0 -> 40.000000
fogprep dens=40.000000 color=00ABCDEF sky=0 fogmode=2
parms 1.000000 0.000000
fog on
fog 00000000 40.000000
fogdensity mode=16 light=90 color=0000FF00 sector=4 blend=0 -> 10.000000
fogprep dens=10.000000 color=0000FF00 sky=0 fogmode=2
parms 1.000000 0.000000
fog on
fog 0000FF00 10.000000
soft 255
fogdensity mode=8 light=40 color=00000000 sector=9 blend=3 -> 10.000000
fogprep dens=10.000000 color=00000000 sky=0 fogmode=2
calclevel mode=8 light=40 rel=-2 weapon=0 blend=3 -> 80
parms 1.150000 181.333328
fog on
fog 00000000 10.000000
fogdensity mode=5 light=20 color=00010203 sector=6 blend=0 -> 50.000000
fogprep dens=50.000000 color=00010203 sky=0 fogmode=2
parms 10.000000 0.032258
fog on
fog 00010203 50.000000
soft 255
fogdensity mode=5 light=20 color=00010203 sector=6 blend=0 -> 0.000000
fogprep dens=0.000000 color=00010203 sky=0 fogmode=2
fog off
fog 00000000 0.000000

""";

    [Fact]
    public void Color_ClampsRelativeLightAndZerosAUnitShaderFactor()
    {
        var vm = new VmSetColor1253();
        var cm = new VmSetColor1253.Colormap
        {
            LightColor = 0x112233,
            Desaturation = 9,
            BlendFactor = 4,
        };
        vm.CalcLevel = (_, _, _, _, _) => 40;
        vm.CalcColor = (_, _, _, _) => 0xABCDEF;
        vm.SetColor(VmSetColor1253.DoomSoftware, 128, 0, true, cm, 0.5f, false);
        vm.SetColor(VmSetColor1253.LinearStandard, 128, 0, true, cm, 0.25f, true);
        vm.SetColor(VmSetColor1253.Doom, 200, -5, false, cm, 0.75f, true);
        cm.BlendFactor = 0;
        vm.SetColor(VmSetColor1253.Build, 300, 10, false, cm, 1f, false);
        vm.SetColor(VmSetColor1253.ZDoomSoftware, 10, -40, false, cm, 1f, false);
        vm.Log.Add("---- shader");
        vm.SetShaderLight(0f, 50f);
        vm.SetShaderLight(10f, 0f);
        vm.SetShaderLight(96f, 0f);
        vm.SetShaderLight(95f, 0f);
        vm.Visibility = 16f;
        vm.SetShaderLight(48f, 95f);
        vm.SetShaderLight(200f, 200f);
        string dump = string.Join('\n', vm.Log) + "\n";
        Assert.Contains("soft 255\n", dump, StringComparison.Ordinal);
        Assert.Contains("soft 0 blend=0\n", dump, StringComparison.Ordinal);
        Assert.Contains("parms 7.450000 128.000000\n", dump, StringComparison.Ordinal);
        Assert.Contains("parms 1.007895 128.000000\n", dump, StringComparison.Ordinal);
        Assert.Contains("parms 1.750000 509.333313\n", dump, StringComparison.Ordinal);
        Assert.Equal(ColorGolden, dump);
    }

    [Fact]
    public void Fog_DensifiesInASkyboxAndFoldsBlackFogThroughTheShader()
    {
        var vm = new VmSetColor1253();
        vm.HasFadeTable = true;
        vm.InSkybox = true;
        vm.SetFog(VmSetColor1253.LinearStandard, 100, 0, false, null, false);
        vm.FogMode = 0;
        vm.SetFog(VmSetColor1253.Doom, 100, 0, true, null, false);
        vm.HasFadeTable = false;
        vm.InSkybox = false;
        vm.FogMode = 2;
        vm.SetFog(VmSetColor1253.Doom, 100, 0, true, new VmSetColor1253.Colormap { FadeColor = 0xFF112233 }, false);
        var black = new VmSetColor1253.Colormap { FadeColor = 0xFF000000, FogDensity = 12 };
        vm.FogDensity = (_, _, _, _, _) => 40f;
        vm.CalcLevel = (_, _, _, _, _) => 10;
        vm.SetFog(VmSetColor1253.Doom, 0, 3, false, black, false);
        var tint = new VmSetColor1253.Colormap { FadeColor = 0x80ABCDEF, FogDensity = 7 };
        vm.SetFog(VmSetColor1253.Doom, 64, 1, false, tint, true);
        vm.BrightFog = true;
        var green = new VmSetColor1253.Colormap { FadeColor = 0x0000FF00, FogDensity = 4 };
        vm.FogDensity = (_, _, _, _, _) => 10f;
        vm.SetFog(VmSetColor1253.DoomSoftware, 90, 2, false, green, false);
        var blend = new VmSetColor1253.Colormap { FadeColor = 0, BlendFactor = 3, FogDensity = 9 };
        vm.CalcLevel = (_, _, _, _, _) => 80;
        vm.SetFog(VmSetColor1253.ZDoomSoftware, 40, -2, false, blend, false);
        var built = new VmSetColor1253.Colormap { FadeColor = 0x00010203, FogDensity = 6 };
        vm.FogDensity = (_, _, _, _, _) => 50f;
        vm.SetFog(VmSetColor1253.Build, 20, 0, false, built, false);
        vm.FogDensity = (_, _, _, _, _) => 0f;
        vm.SetFog(VmSetColor1253.Build, 20, 0, false, built, false);
        string dump = string.Join('\n', vm.Log) + "\n";
        Assert.Contains("fogprep dens=105.000000 color=00808080 sky=1 fogmode=0\n", dump, StringComparison.Ordinal);
        Assert.Contains("fogdensity mode=2 light=0 color=FF000000 sector=12 blend=0 -> 40.000000\n", dump, StringComparison.Ordinal);
        Assert.Contains("parms 7.450000 128.000000\n", dump, StringComparison.Ordinal);
        Assert.Contains("fog 00000000 40.000000\n", dump, StringComparison.Ordinal);
        Assert.Contains("parms 1.150000 181.333328\n", dump, StringComparison.Ordinal);
        Assert.Contains("parms 10.000000 0.032258\n", dump, StringComparison.Ordinal);
        Assert.Contains("soft 255\n", dump, StringComparison.Ordinal);
        Assert.Equal(FogGolden, dump);
    }
}
