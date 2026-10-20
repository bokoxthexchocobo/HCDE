namespace HCDE.MapLoader.Tests;

public class VmMapLoaderLoadLineDefsTests
{
    [Fact]
    public void IsVertexIndexInvalid_DetectsOutOfRange()
    {
        Assert.True(VmMapLoaderLoadLineDefs.IsVertexIndexInvalid(5, 1, 5));
    }

    [Fact]
    public void IsZeroLengthLine_DetectsCoincidentVertices()
    {
        Assert.True(VmMapLoaderLoadLineDefs.IsZeroLengthLine(1, 2, 0, 0, 0, 0));
    }

    [Fact]
    public void ShouldForceNodeBuildOnSkip_WhenLinesRemoved()
    {
        Assert.True(VmMapLoaderLoadLineDefs.ShouldForceNodeBuildOnSkip(true));
    }

    [Fact]
    public void CompactZeroLengthDoomLines_RemovesCoincident()
    {
        var lines = new[]
        {
            new VmMapLoaderLoadLineDefs.DoomLineRecord(0, 1, 0, VmMapLoaderLoadLineDefs.NoIndex),
            new VmMapLoaderLoadLineDefs.DoomLineRecord(1, 1, 0, VmMapLoaderLoadLineDefs.NoIndex),
        };
        var (kept, skipped, sides) = VmMapLoaderLoadLineDefs.CompactZeroLengthDoomLines(
            lines, v => (v, 0), 2);
        Assert.Single(kept);
        Assert.Equal(1, skipped);
        Assert.Equal(1, sides);
    }

    [Fact]
    public void ResolveActivationFromFlags_ExpandsAnyCross()
    {
        const int mask = 0x1c00;
        const int anyCross = 1 << 3;
        var act = VmMapLoaderLoadLineDefs.ResolveActivationFromFlags(anyCross << 10, mask, anyCross, 1, 2);
        Assert.Equal(3, act);
    }

    [Fact]
    public void PatchMissingFirstSide_ReplacesNoIndex()
    {
        ushort side = VmMapLoaderLoadLineDefs.NoIndex;
        VmMapLoaderLoadLineDefs.PatchMissingFirstSide(ref side, 0);
        Assert.Equal((ushort)0, side);
    }
}
