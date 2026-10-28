namespace HCDE.Gamedata.Tests;

public class VmTerrainDocumentParserTests
{
    private sealed class DoomFilter : ITerrainGameFilter
    {
        public bool IsActiveGame(string gameSuffix) => gameSuffix.Equals("doom", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParseDocument_SplashAndIfGame()
    {
        var db = new VmTerrainDatabase();
        VmTerrainInit.MakeDefaultTerrain(db);
        var errors = new List<string>();
        const string text = "ifdoom splash Water { sound puddle } ifheretic splash Lava { sound lava } endif";
        Assert.True(VmTerrainDocumentParser.TryParseDocument(text, db, new DoomFilter(), errors), string.Join("; ", errors));
        Assert.Single(db.Splashes);
        Assert.Equal("puddle", db.Splashes[0].NormalSplashSound);
    }
}
