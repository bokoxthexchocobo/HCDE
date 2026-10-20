namespace HCDE.MapLoader.Tests;

public class VmMapLoaderZNodeLoadTests
{
    [Fact]
    public void EnsureHasVertexes_Zero_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => VmMapLoaderZNodeLoad.EnsureHasVertexes(0));
    }

    [Fact]
    public void ReadVertexRecords_ParsesLittleEndianPairs()
    {
        var lump = new byte[] { 0x02, 0x00, 0xfd, 0xff };
        var v = VmMapLoaderZNodeLoad.ReadVertexRecords(lump)[0];
        Assert.Equal(2, v.X);
        Assert.Equal(-3, v.Y);
    }

    [Fact]
    public void ReadZSegRecord_AdvancesOffset()
    {
        var data = new byte[] { 1, 0, 0, 0, 2, 0, 0, 0, 3, 0, 1 };
        var offset = 0;
        var seg = VmMapLoaderZNodeLoad.ReadZSegRecord(data, ref offset);
        Assert.Equal(1u, seg.V1);
        Assert.Equal(2u, seg.V2);
        Assert.Equal((ushort)3, seg.Line);
        Assert.Equal((byte)1, seg.Side);
        Assert.Equal(11, offset);
    }

    [Fact]
    public void ValidateGlPartner_OutOfRange_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            VmMapLoaderZNodeLoad.ValidateGlPartner(10, 5, 0, 0));
    }

    [Fact]
    public void ReadGlLineIndex_Type1_MapsFFFFToInvalid()
    {
        var data = new byte[] { 0xff, 0xff };
        var offset = 0;
        Assert.Equal(0xffffffffu, VmMapLoaderZNodeLoad.ReadGlLineIndex(data, ref offset, 1));
    }

    [Fact]
    public void EnsureSegCountMatches_Mismatch_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            VmMapLoaderZNodeLoad.EnsureSegCountMatches(3, 2));
    }

    [Fact]
    public void EnsureVertexNodeCounts_TooManyOrgVerts_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            VmMapLoaderZNodeLoad.EnsureVertexNodeCounts(50, 10));
    }

    [Fact]
    public void ComputeNewVertexReserve_WhenCountsMatch_ReturnsZeroReserve()
    {
        Assert.Equal(0, VmMapLoaderZNodeLoad.ComputeNewVertexReserve(10, 5, 15));
    }
}
