namespace HCDE.Gamedata.Tests;

public class VmMapInfoClusterExitTextRemapTests
{
    [Fact]
    public void TryRemapHexenExitLump_BuildsKey()
    {
        Assert.True(VmMapInfoClusterExitTextRemap.TryRemapHexenExitLump(
            "HEXEN.WAD", "CLUS1MSG", _ => true, out var key));
        Assert.Contains("CLUS1MSG", key);
    }
}
