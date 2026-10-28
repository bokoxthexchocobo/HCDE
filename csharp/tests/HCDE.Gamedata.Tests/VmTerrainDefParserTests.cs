namespace HCDE.Gamedata.Tests;

public class VmTerrainDefParserTests
{
    [Fact]
    public void ParseTerrain_AppliesDamageAndFriction()
    {
        var db = new VmTerrainDatabase();
        VmTerrainInit.MakeDefaultTerrain(db);
        db.Splashes.Add(new VmSplashDef { Name = "Water" });
        var errors = new List<string>();
        Assert.True(VmTerrainDefParser.TryParseTerrainKeyword(
            "terrain Mud { splash Water damageamount 5 damagetype Lava friction 1.0 liquid }", db, errors), string.Join("; ", errors));
        var mud = Assert.Single(db.Terrains, t => t.Name == "Mud");
        Assert.Equal("Fire", mud.DamageType);
        Assert.True(mud.IsLiquid);
    }
}
