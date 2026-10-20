namespace HCDE.Gamedata.Tests;

public class VmTerrainFlatCatalogTests
{
    [Fact]
    public void Floor_OptionalUnknownFlat_SkipsTerrainWhenCatalogMissing()
    {
        var db = new VmTerrainDatabase();
        VmTerrainInit.MakeDefaultTerrain(db);
        var catalog = new VmTerrainFlatNameCatalog(new Dictionary<string, int> { ["FLOOR1"] = 3 });
        var errors = new List<string>();
        Assert.True(VmTerrainDocumentParser.TryParseDocument(
            "terrain Mud { } floor optional NOFLAT Mud", db, new AllGames(), errors, catalog), string.Join("; ", errors));
        Assert.Empty(db.FloorTerrainByTextureIndex);
    }

    [Fact]
    public void Floor_ResolvesTextureIndexFromCatalog()
    {
        var db = new VmTerrainDatabase();
        VmTerrainInit.MakeDefaultTerrain(db);
        var catalog = new VmTerrainFlatNameCatalog(new Dictionary<string, int> { ["FLOOR1"] = 3 });
        var errors = new List<string>();
        Assert.True(VmTerrainDocumentParser.TryParseDocument(
            "terrain Mud { } floor FLOOR1 Mud", db, new AllGames(), errors, catalog), string.Join("; ", errors));
        Assert.Equal(1, db.FloorTerrainByTextureIndex[3]);
    }

    private sealed class AllGames : ITerrainGameFilter
    {
        public bool IsActiveGame(string gameSuffix) => true;
    }
}
