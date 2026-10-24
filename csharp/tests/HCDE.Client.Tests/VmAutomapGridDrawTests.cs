namespace HCDE.Client.Tests;

public class VmAutomapGridDrawTests
{
    [Fact]
    public void ShouldRotateGridLines_Rotate2RequiresViewActive()
    {
        Assert.False(VmAutomapGridDraw.ShouldRotateGridLines(2, false));
        Assert.True(VmAutomapGridDraw.ShouldRotateGridLines(2, true));
        Assert.True(VmAutomapGridDraw.ShouldRotateGridLines(1, false));
    }

    [Fact]
    public void EnumerateVerticalGridLines_StepsByMapBlockUnits()
    {
        var ctx = new VmAutomapGridDraw.GridContext(320, 200, 0, 0, 0, 0, 0, false);
        var lines = VmAutomapGridDraw.EnumerateVerticalGridLines(ctx).Take(3).ToList();
        Assert.Equal(3, lines.Count);
        Assert.Equal(lines[0].Ax, lines[0].Bx);
        Assert.Equal(128, lines[1].Ax - lines[0].Ax, 1);
    }

    [Fact]
    public void EnumerateHorizontalGridLines_ProducesConstantY()
    {
        var ctx = new VmAutomapGridDraw.GridContext(320, 200, 64, 32, 0, 0, 0, false);
        var line = VmAutomapGridDraw.EnumerateHorizontalGridLines(ctx).First();
        Assert.Equal(line.Ay, line.By);
        Assert.True(line.Bx > line.Ax);
    }

    [Fact]
    public void AlignGridStart_SnapsToBlockmapGrid()
    {
        var start = VmAutomapGridDraw.AlignGridStart(100, 10, 0);
        Assert.Equal(0, start % VmAutomapGridDraw.MapBlockUnits, 3);
    }
}
