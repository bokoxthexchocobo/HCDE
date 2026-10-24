namespace HCDE.Gamedata.Tests;

public class VmEMapInfoDocumentLoopTests
{
    [Fact]
    public void ApplySky2Mirror_CopiesSky1WhenNoFlat()
    {
        var draft = new VmEMapInfoLevelDraft { SkyPic1 = "SKY1", SkyPic2 = "-NOFLAT-" };
        VmEMapInfoDocumentLoop.ApplySky2Mirror(draft);
        Assert.Equal("SKY1", draft.SkyPic2);
    }

    [Fact]
    public void IsKnownEternityMap_LooksUpRegistry()
    {
        var reg = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase) { ["MAP01"] = true };
        Assert.True(VmEMapInfoDocumentLoop.IsKnownEternityMap(reg, "map01"));
    }

    [Fact]
    public void ResolveExtraDataLump_PrefersMapLump()
    {
        var lumps = new Dictionary<string, int> { ["MAP01"] = 5 };
        Assert.Equal(5, VmEMapInfoDocumentLoop.ResolveExtraDataLump(lumps, "MAP01", "x", _ => 9));
    }
}
