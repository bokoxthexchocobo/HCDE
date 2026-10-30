namespace HCDE.Playsim.Tests;

public class VmDynLight1252SliceTests
{
    const string HeadGolden =
"""
light group=0 r=0.000000 dist=0.000000 side=0
light reject radius
light group=0 r=10.000000 dist=20.000000 side=0
light reject dist
light group=0 r=10.000000 dist=1.000000 side=1
light reject side
light group=0 r=10.000000 dist=1.000000 side=0
add slot=0 0.000000,-1.000000,0.000000,10.000000,1.000000,1.000000,1.000000,1025.000000,0.000000,0.000000,0.000000,0.000000,0.000000,0.000000,-1.000000,-1.000000
light group=0 r=10.000000 dist=3.000000 side=1
add slot=2 1.000000,3.000000,2.000000,10.000000,0.200000,0.200000,0.200000,1025.000000,0.000000,0.000000,0.000000,0.000000,0.000000,0.000000,-1.000000,-1.000000

""";

    const string TailGolden =
"""
---- edge
add slot=1 0.000000,0.000000,0.000000,4.000000,0.732051,0.732051,0.732051,1025.000000,0.000000,0.000000,0.000000,0.000000,0.000000,0.000000,-1.000000,-1.000000
add slot=0 4.000000,6.000000,5.000000,8.000000,1.000000,0.501961,0.000000,-5.000000,0.000000,0.000000,0.000000,0.000000,0.000000,0.000000,0.250000,3.000000
add slot=0 9.000000,7.000000,8.000000,2.000000,1.000000,1.000000,1.000000,-1025.000000,-0.000000,0.000000,-1.000000,1.000000,1.000000,-1.000000,-1.000000,-1.000000

""";

    [Fact]
    public void Get_RejectsADarkSideAndKeepsALightOnThePlane()
    {
        string dump = Build(out VmDynLight1252 vm);
        int cut = dump.IndexOf("---- edge\n", StringComparison.Ordinal);
        string head = dump[..cut];
        Assert.Contains("light reject radius\n", head, StringComparison.Ordinal);
        Assert.Contains("light reject dist\n", head, StringComparison.Ordinal);
        Assert.Contains("light reject side\n", head, StringComparison.Ordinal);
        Assert.Contains("add slot=2 ", head, StringComparison.Ordinal);
        Assert.Equal(16, vm.Arrays[2].Count);
        Assert.Equal(1f, vm.Arrays[2][0]);
        Assert.Equal(3f, vm.Arrays[2][1]);
        Assert.Equal(2f, vm.Arrays[2][2]);
        Assert.Equal(0.2f, vm.Arrays[2][4]);
        Assert.Equal(1025f, vm.Arrays[2][7]);
        Assert.True(vm.Sprites);
        Assert.True(vm.Particles);
        Assert.Equal(HeadGolden, head);
    }

    [Fact]
    public void Add_FoldsSubtractiveColorAndASpotDirection()
    {
        string dump = Build(out _);
        string tail = dump[dump.IndexOf("---- edge\n", StringComparison.Ordinal)..];
        Assert.Contains("add slot=1 ", tail, StringComparison.Ordinal);
        Assert.Contains("add slot=0 ", tail, StringComparison.Ordinal);
        Assert.Contains("-5.000000", tail, StringComparison.Ordinal);
        Assert.Contains("0.000000,0.000000,-1.000000,1.000000", tail, StringComparison.Ordinal);
        Assert.Equal(TailGolden, tail);
    }

    static string Build(out VmDynLight1252 vm)
    {
        vm = new VmDynLight1252();
        var plane = new VmDynLight1252.Plane { Ny = 1f };
        var dead = new VmDynLight1252.Light { Radius = 0f };
        Assert.False(vm.GetLight(plane, dead, 0, false));

        var far = new VmDynLight1252.Light { Z = 20, Radius = 10f };
        Assert.False(vm.GetLight(plane, far, 0, false));

        var under = new VmDynLight1252.Light { Z = -1, Radius = 10f };
        Assert.False(vm.GetLight(plane, under, 0, true));
        Assert.True(vm.GetLight(plane, under, 0, false));

        var lamp = new VmDynLight1252.Light
        {
            X = 1, Y = 2, Z = 3, Radius = 10f, Additive = true,
        };
        Assert.True(vm.GetLight(plane, lamp, 0, true));
        vm.Log.Add("---- edge");

        var sub = new VmDynLight1252.Light
        {
            Radius = 4f, Subtractive = true, Red = 255, Green = 255, Blue = 255,
        };
        vm.Add(sub, 0, false);

        var tinted = new VmDynLight1252.Light
        {
            X = 4, Y = 5, Z = 6, Radius = 8f,
            HasTarget = true, LightMultAlpha = true, Alpha = 0.5f, Intensity = 2f,
            Red = 255, Green = 128, Blue = 0,
            ShadowMap = true, ShadowIndex = 4, Attenuated = true,
            Linearity = 0.25f, Soft = 3f,
        };
        vm.Add(tinted, 0, false);

        var spot = new VmDynLight1252.Light
        {
            X = 0, Y = 0, Z = 0, Radius = 2f, Spot = true,
            YawDeg = 90, PitchDeg = 0, InnerDeg = 0, OuterDeg = 180,
            OffX = 9, OffY = 8, OffZ = 7,
        };
        vm.Add(spot, 3, true);
        return string.Join('\n', vm.Log) + "\n";
    }
}
