namespace HCDE.Gamedata.Tests;

public class VmTerrainGenericParserTests
{
    [Fact]
    public void TerrainBody_ParsesDamageOnLandAndStepDistance()
    {
        var db = new VmTerrainDatabase();
        var errors = new List<string>();
        Assert.True(VmTerrainDefParser.TryParseTerrain("Mud", modify: false,
            "{ damageonland stepdistance 2.5 friction 1.0 }", db, errors), string.Join("; ", errors));
        var def = Assert.Single(db.Terrains);
        Assert.True(def.DamageOnLand);
        Assert.True(def.Friction > 0);
    }
}
