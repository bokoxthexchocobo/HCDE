namespace HCDE.Gamedata.Tests;

public class VmDecalDocumentLoaderTests
{
    [Fact]
    public void LoadAllDecalLumps_ParsesDecalBlock()
    {
        var lib = new VmDecalLibCore();
        var db = new VmDecalDatabase();
        var errors = new List<string>();
        VmDecalDocumentLoader.LoadAllDecalLumps(["decal Blood { pic BLOODY }"], lib, db, errors);
        Assert.Empty(errors);
        Assert.True(db.TryGetDecal("Blood", out var decal));
        Assert.NotNull(decal);
    }
}
