namespace HCDE.MapLoader.Tests;

public class VmMapLoaderExtendedNodesTests
{
    [Fact]
    public void TryParseExtendedNodeId_Znod_IsCompressedType0()
    {
        Assert.True(VmMapLoaderExtendedNodes.TryParseExtendedNodeId(0x444f4e5a, out var f));
        Assert.True(f.Compressed);
        Assert.Equal(0, f.GlNodeType);
    }

    [Fact]
    public void IsV4NodeHeader_MatchesSignature()
    {
        var h = new byte[] { (byte)'x', (byte)'N', (byte)'d', (byte)'4', 0, 0, 0, 0 };
        Assert.True(VmMapLoaderExtendedNodes.IsV4NodeHeader(h));
    }

    [Fact]
    public void NextZoneIndex_CountsUnassignedSectors()
    {
        Assert.Equal(2, VmMapLoaderExtendedNodes.NextZoneIndex(new ushort[] { 0xffff, 0, 0xffff }));
    }

    [Fact]
    public void ShouldUseCompressedLoader_RequiresOpenZipWhenCompressed()
    {
        Assert.False(VmMapLoaderExtendedNodes.ShouldUseCompressedLoader(true, false));
        Assert.True(VmMapLoaderExtendedNodes.ShouldUseCompressedLoader(true, true));
    }
}
