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
    public void ResolveActivationFromFlags_ExpandsAnyCross()
    {
        const int spacMask = 0x1c00;
        const int anyCross = 1 << 3;
        const int impact = 1;
        const int pcross = 2;
        var act = VmMapLoaderLoadLineDefs.ResolveActivationFromFlags(anyCross << 0, spacMask, anyCross, impact, pcross);
        Assert.Equal(impact | pcross, act);
    }

    [Fact]
    public void PatchMissingFirstSide_ReplacesNoIndex()
    {
        ushort side = VmMapLoaderLoadLineDefs.NoIndex;
        VmMapLoaderLoadLineDefs.PatchMissingFirstSide(ref side, 0);
        Assert.Equal((ushort)0, side);
    }
}
