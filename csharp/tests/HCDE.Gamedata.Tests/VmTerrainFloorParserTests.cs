namespace HCDE.Gamedata.Tests;

public class VmTerrainFloorParserTests
{
    private sealed class AllGames : ITerrainGameFilter
    {
        public bool IsActiveGame(string gameSuffix) => true;
    }

    [Fact]
    public void FloorAndDefaultTerrain_MapFlats()
    {
        var db = new VmTerrainDatabase();
        VmTerrainInit.MakeDefaultTerrain(db);
        var errors = new List<string>();
        Assert.True(VmTerrainDocumentParser.TryParseDocument(
            "terrain Mud { } floor FLOOR1 Mud defaultterrain Mud", db, new AllGames(), errors), string.Join("; ", errors));
        Assert.Equal(1, db.FloorTerrains["FLOOR1"]);
        Assert.Equal(1, db.DefaultTerrainIndex);
    }
}
