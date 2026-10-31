namespace HCDE.Playsim.Tests;

public class VmVrMode1264SliceTests
{
    const string TableGolden =
"""
mode 0 1 1.000000 1.000000 1.000000 0.000000 1.000000 0.000000 0.000000
mode 0 1 1.000000 1.000000 1.000000 0.000000 1.000000 0.000000 0.000000
mode 14 2 0.707107 0.707107 1.000000 -0.500000 1.000000 0.500000 1.000000
mode 10 1 1.000000 1.000000 1.000000 0.000000 1.000000 0.000000 0.000000
mode 3 2 0.500000 1.000000 2.000000 -0.500000 0.500000 0.500000 0.500000
view 5 -3 100 100 4 4 160 240
hud 0.006250 0.000000 0.000000 0.000000 0.000000 -0.005000 0.000000 0.000000 0.000000 0.000000 -1.000000 0.000000 -2.000000 1.000000 -0.000000 1.000000

""";

    const string EyeGolden =
"""
mode 5 1 1.000000 1.000000 1.000000 -0.500000 1.000000 0.000000 0.000000
shift 0.000000 1.271000 0.000000
shift 0.000000 0.000000 0.000000
frustum 0.625000 0.000000 0.000000 0.000000 0.000000 1.000000 0.000000 0.000000 0.024219 0.000000 -1.000153 -1.000000 0.000000 0.000000 -10.000763 0.000000
iso 0.781250 0.000000 0.000000 0.000000 0.000000 1.250000 0.000000 0.000000 0.000000 0.000000 -0.000031 0.000000 -0.000000 -0.000000 -0.999927 1.000000
mode 0 1 1.000000 1.000000 1.000000 0.000000 1.000000 0.000000 0.000000
shift 0.000000 0.000000 0.000000
persp 0.750000 0.000000 0.000000 0.000000 0.000000 1.200000 0.000000 0.000000 0.000000 0.000000 -1.000153 -1.000000 0.000000 0.000000 -10.000763 0.000000

""";

    [Fact]
    public void Modes_ForceMonoAndScaleTheFullFrame()
    {
        var off = VmVrMode1264.Select(false, true, true, VmVrMode1264.SideBySideFull);
        var forced = VmVrMode1264.Select(true, true, true, VmVrMode1264.Checker);
        var open = VmVrMode1264.Select(true, false, true, VmVrMode1264.Checker);
        var gap = VmVrMode1264.Select(true, false, false, 10);
        var full = VmVrMode1264.Select(true, false, false, VmVrMode1264.SideBySideFull);
        Assert.Equal(1, off.EyeCount);
        Assert.Equal(1, forced.EyeCount);
        Assert.Equal(VmVrMode1264.ISqrt2, open.Horizontal);
        Assert.Equal(1, gap.EyeCount);
        Assert.Equal(0f, gap.Shift[0]);
        Assert.Equal(2, full.EyeCount);
        Assert.Equal(2f, full.Weapon);
        Assert.Equal(0.5f, full.Scale[0]);

        var stereo = VmVrMode1264.FromMode(VmVrMode1264.GreenMagenta);
        Assert.Equal(2, stereo.EyeCount);
        Assert.Equal(1f, stereo.Horizontal);
        Assert.Equal(-0.5f, stereo.Shift[0]);
        Assert.Equal(0.5f, stereo.Shift[1]);
        Assert.Equal(1f, VmVrMode1264.FromMode(VmVrMode1264.RedCyan).Weapon);
        Assert.Equal(1f, VmVrMode1264.FromMode(VmVrMode1264.Quad).Vertical);
        Assert.Equal(1f, VmVrMode1264.FromMode(VmVrMode1264.AmberBlue).Horizontal);
        Assert.Equal(1f, VmVrMode1264.FromMode(VmVrMode1264.SideBySideLetter).Weapon);
        Assert.Equal(0.5f, VmVrMode1264.FromMode(VmVrMode1264.SideBySideSquished).Horizontal);
        Assert.Equal(1f, VmVrMode1264.FromMode(VmVrMode1264.ColumnInterleaved).Weapon);
        Assert.Equal(0.5f, VmVrMode1264.FromMode(VmVrMode1264.TopBottom).Vertical);
        Assert.Equal(0.5f, VmVrMode1264.FromMode(VmVrMode1264.RowInterleaved).Vertical);
        Assert.Equal(0.5f, VmVrMode1264.FromMode(VmVrMode1264.RightEye).Shift[0]);
        Assert.Equal(0f, VmVrMode1264.FromMode(VmVrMode1264.RightEye).Shift[1]);

        full.Width = 640;
        full.Height = 400;
        full.Adjust(10, -3, 200, 100, 8, 4, 320, 240);
        Assert.Equal(-3, full.SceneTop);
        Assert.Equal(5, full.SceneLeft);
        float[] hud = full.Hud();
        Assert.Equal(-0f, hud[14]);
        Assert.Equal(1f, hud[15]);
        Assert.Equal(TableGolden, string.Join('\n', off.Log.Concat(forced.Log).Concat(open.Log).Concat(gap.Log).Concat(full.Log)) + "\n");
    }

    [Fact]
    public void Eyes_ShiftTheFrustumAndLeaveMonoAtTheOrigin()
    {
        var left = VmVrMode1264.Select(true, false, false, VmVrMode1264.LeftEye);
        left.SwapEyes = true;
        left.ViewShift(0, 90f);
        left.ViewShift(1, 0f);
        Assert.Equal(0f, left.ShiftOf(1));
        left.SwapEyes = false;
        float[] frustum = left.Projection(0, 90f, 1.6f, 1f, false);
        Assert.Equal(0.024219f, frustum[8], 5);
        float[] iso = left.Projection(0, 90f, 1.6f, 1.25f, true);
        Assert.Equal(1f, iso[15]);
        Assert.NotEqual(0f, iso[10]);

        var mono = VmVrMode1264.Select(true, false, false, VmVrMode1264.Mono);
        mono.SwapEyes = true;
        mono.ViewShift(0, 45f);
        float[] persp = mono.Projection(0, 90f, 1.6f, 1.2f, false);
        Assert.Equal(-1f, persp[11]);
        Assert.Equal(0f, persp[15]);
        Assert.Equal(EyeGolden, string.Join('\n', left.Log.Concat(mono.Log)) + "\n");
    }
}
