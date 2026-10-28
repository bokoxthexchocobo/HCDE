namespace HCDE.Gamedata.Tests;

public class VmDoomEdSpawnMapParserTests
{
    [Fact]
    public void TryParseSpawnMap_ReadsPairs()
    {
        var tokens = VmLockDefTokenize.Tokenize("3001 = DoomImp }");
        var i = 0;
        var map = new Dictionary<int, VmMapinfoSpawnItem>();
        var errors = new List<string>();
        Assert.True(VmDoomEdSpawnMapParser.TryParseSpawnMap(tokens, ref i, map, "Spawn number", errors));
        Assert.Equal("DoomImp", map[3001].ClassName);
    }
}
