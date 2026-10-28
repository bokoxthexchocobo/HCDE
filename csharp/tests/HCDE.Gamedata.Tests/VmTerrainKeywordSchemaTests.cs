namespace HCDE.Gamedata.Tests;

public class VmTerrainKeywordSchemaTests
{
    private sealed class AllGames : ITerrainGameFilter
    {
        public bool IsActiveGame(string game) => true;
    }

    [Fact]
    public void OuterKeywords_MatchesNativeTable()
    {
        Assert.Equal(9, VmTerrainKeywordSchema.OuterKeywords.Count);
        Assert.Equal("defaultterrain", VmTerrainKeywordSchema.OuterKeywords[^1]);
    }

    [Fact]
    public void LoadTerrainDocuments_ParsesSplashBlock()
    {
        var db = new VmTerrainDatabase();
        const string text = "ifdoom splash Water { sound puddle } endif";
        var errors = new List<string>();
        VmTerrainInit.LoadTerrainDocuments([text], db, new AllGames(), null, errors);
        Assert.Empty(errors);
        Assert.Single(db.Splashes);
    }
}
