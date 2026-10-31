namespace HCDE.Playsim.Tests;

public class VmFogBoundary1257SliceTests
{
    const string SplitGolden =
"""
shade 1048576
fill 1 3 3
base
light3 24.000000 128 1
map 0 off 0
ident 2
mark 4 2
fill 1 4 1
light2 8.000000 1048576
map 8 off 2048
section 2 5 1
fog 2 1 1
fog 3 1 1
fog 4 1 1
fill 2 4 0
light2 0.000000 1048576
map 16 off 4096
section 2 6 0
fog 2 0 0
fog 3 0 0
fog 4 0 0
fog 5 0 0

""";

    const string SpanGolden =
"""
shade 1310720
fill 0 5 2
base
light3 0.000000 200 0
map 20 off 5120
fog 0 2 2
fog 1 2 2
mark 5 1
fog 5 1 1
fog 4 1 2
mark 1 0
section 1 4 0
fog 1 0 0
fog 2 0 2
fog 3 0 2

""";

    [Fact]
    public void Split_SkipsTheIdentityMapAndDrawsOnAColormapChange()
    {
        var vm = new VmFogBoundary1257
        {
            ToShade = (level, fog) =>
            {
                Assert.Equal(128, level);
                Assert.True(fog);
                return 16 << 16;
            },
        };
        short[] up = [2, 2, 1, 1];
        short[] down = [6, 5, 5, 4];
        vm.Render(0, 4, up, down, 128, true, 24f, 8f);
        Assert.Equal(16, vm.Map);
        Assert.Equal(4096, vm.MapOffset);
        Assert.Equal(0f, vm.Light);
        Assert.Equal(1048576, vm.WallShade);
        Assert.Equal((short)1, vm.SpanEnd[1]);
        Assert.Equal((short)0, vm.SpanEnd[2]);
        Assert.Equal((short)0, vm.SpanEnd[5]);
        Assert.Equal(SplitGolden, string.Join('\n', vm.Log) + "\n");
    }

    [Fact]
    public void Span_DrawsAClosedRowAndClampsTheLookup()
    {
        Assert.Equal(0, VmFogBoundary1257.PalLookup(24f, 16 << 16));
        Assert.Equal(16, VmFogBoundary1257.PalLookup(0f, 16 << 16));
        Assert.Equal(31, VmFogBoundary1257.PalLookup(0f, 40 << 16));
        Assert.Equal(0, VmFogBoundary1257.PalLookup(0f, -5));
        Assert.Equal(0, VmFogBoundary1257.PalLookup(100f, 24 << 16));
        Assert.Equal(8, VmFogBoundary1257.PalLookup(-8f, 0));
        Assert.Equal(1, VmFogBoundary1257.PalLookup(0.5f / 65536f, 65536));

        var blank = new VmFogBoundary1257 { ToShade = (_, _) => 0 };
        blank.Render(0, 1, [3], [3], 0, false, 24f, 0f);
        Assert.Equal(0, blank.Map);
        Assert.DoesNotContain(blank.Log, line => line.StartsWith("fog ", StringComparison.Ordinal));
        Assert.DoesNotContain(blank.Log, line => line.StartsWith("fill ", StringComparison.Ordinal));

        var vm = new VmFogBoundary1257 { ToShade = (level, fog) => level == 200 && !fog ? 20 << 16 : -1 };
        vm.Render(0, 3, [1, 2, 0], [4, 6, 5], 200, false, 0f, 0f);
        Assert.Equal(20, vm.Map);
        Assert.Equal(5120, vm.MapOffset);
        Assert.Equal((short)0, vm.SpanEnd[1]);
        Assert.Equal((short)2, vm.SpanEnd[3]);
        Assert.Equal((short)2, vm.SpanEnd[4]);
        Assert.Equal((short)1, vm.SpanEnd[5]);
        Assert.Equal(SpanGolden, string.Join('\n', vm.Log) + "\n");
    }
}
