namespace HCDE.Playsim.Tests;

public class VmNodeClassify1215SliceTests
{
    static void Expect(int x, int y, int dx, int dy, int ax, int ay, int bx, int by, int result, int side0, int side1)
    {
        var got = VmNodeClassify1215Slice.Classify(x, y, dx, dy, ax, ay, bx, by);
        Assert.Equal((result, side0, side1), got);
    }

    [Fact]
    public void Classify_SortsFarSidesAndTheFarBoundary()
    {
        Expect(0, 0, 65536, 0, 0, 262144, 100, 300000, 1, 1, 1);
        Expect(0, 0, 65536, 0, 0, -262144, 100, -300000, 0, -1, -1);
        Expect(0, 0, 65536, 0, 0, 262144, 0, -262144, -1, 1, -1);
        Expect(0, 0, 65536, 0, 0, -262144, 0, 262144, -1, -1, 1);
        Expect(0, 0, 65536, 0, 0, 262144, 0, 1, 1, 1, 0);
        Expect(0, 0, 65536, 0, 0, -262144, 0, -1, 0, -1, 0);
        Expect(0, 0, 65536, 0, 0, 262143, 10, 262143, 1, 1, 1);
        Expect(0, 0, 65536, 0, 0, 0, 0, 262144, 1, 0, 1);
        Expect(100, -50, 32768, -32768, 0, 100000, -100000, 0, -1, 1, -1);
        Expect(100, -50, 32768, -32768, 500000, -500000, -500000, 500000, 0, -1, -1);
        Expect(2147483647, -2147483648, 65536, -1, 0, 0, 1, 1, 1, 1, 1);
        Expect(0, 0, 0, 0, 1, 2, 3, 4, 1, 1, 1);
        Expect(0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1);
    }

    [Fact]
    public void Classify_UsesEpsilonAndSplitterDirectionWhenBothEndsAreOnTheLine()
    {
        Expect(0, 0, 65536, 0, 0, 0, 65536, 0, 0, 0, 0);
        Expect(0, 0, 65536, 0, 65536, 0, 0, 0, 1, 0, 0);
        Expect(0, 0, 65536, 0, 5, 0, 5, 0, 1, 0, 0);
        Expect(0, 0, 65536, 0, 0, 1, 100, 1, 0, 0, 0);
        Expect(0, 0, 65536, 0, 0, 7, 100, 7, 1, 1, 1);
        Expect(0, 0, 65536, 0, 0, 6, 100, -6, 0, 0, 0);
        Expect(10, 20, 0, 65536, 10, 0, 10, 400000, 0, 0, 0);
        Expect(10, 20, 0, 65536, 10, 400000, 10, 0, 1, 0, 0);
        Expect(10, 20, 0, 65536, 10, 20, 10, 20, 1, 0, 0);
        Expect(10, 20, 0, 65536, 11, 0, 9, 100, 0, 0, 0);
        Expect(100, -50, 32768, -32768, 100, -50, 32868, -32818, 0, 0, 0);
        Expect(100, -50, 32768, -32768, 32868, -32818, 100, -50, 1, 0, 0);
        Expect(0, 0, -65536, 0, 0, 0, -100, 0, 0, 0, 0);
        Expect(0, 0, -65536, 0, -100, 0, 0, 0, 1, 0, 0);
        Expect(0, 0, -65536, 0, 0, 0, 0, 0, 1, 0, 0);
        Expect(0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0);
        Expect(0, 0, 1, 0, 0, 1, 5, 1, 0, 0, 0);
        Expect(0, 0, 1, 0, 0, 8, 5, 8, 1, 1, 1);
        Expect(0, 0, 1, 0, 0, 3, 5, -3, 0, 0, 0);
        Expect(2147483647, -2147483648, 65536, -1, 2147483647, -2147483648, 2147483646, -2147483647, 1, 0, 0);
        Assert.Equal(17179869184.0, VmNodeClassify1215Slice.FarEnough);
        Assert.Equal(6.5536, VmNodeClassify1215Slice.SideEpsilon);
    }
}
