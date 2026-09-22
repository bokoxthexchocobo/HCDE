namespace HCDE.Gamedata.Tests;

public class MapInfoParserTests
{
    [Fact]
    public void TryParse_ReadsBootFieldsForADoom2Map()
    {
        const string text = """
            map MAP01 lookup HUSTR_1
            {
                levelnum = 1
                next = MAP02
                secretnext = MAP31
                cluster = 5
                sky1 = SKY1, 0.5
            }
            cluster 5
            {
                exittext = "Look"
                flat = FLOOR7_2
            }
            """;

        Assert.True(MapInfoParser.TryParse(text, out var set, out var error), error);
        var map = set.FindMap("MAP01");
        Assert.NotNull(map);
        Assert.Equal("HUSTR_1", map.LevelName);
        Assert.True(map.LookupLevelName);
        Assert.Equal(1, map.LevelNum);
        Assert.Equal("MAP02", map.NextMap);
        Assert.Equal("MAP31", map.SecretNextMap);
        Assert.Equal(5, map.Cluster);
        Assert.Equal("SKY1", map.Sky1);
        Assert.Equal(0.5 * (35 / 1000.0), map.SkySpeed1, precision: 8);
        var cluster = set.FindCluster(5);
        Assert.NotNull(cluster);
        Assert.Equal("Look", cluster.ExitText);
        Assert.Equal("FLOOR7_2", cluster.Flat);
        Assert.False(cluster.FinaleIsPic);
        Assert.False(cluster.Hub);
    }

    [Fact]
    public void TryParse_NumericMapAndNextUseHexenWarp()
    {
        const string text = """
            map 1 "Winnowing Hall"
            {
                next = 2
                cluster = 1
                sky1 = SKY2, 256
            }
            """;

        Assert.True(MapInfoParser.TryParse(text, out var set, out var error), error);
        var map = set.FindMap("MAP01");
        Assert.NotNull(map);
        Assert.True(map.HexenHack);
        Assert.Equal("Winnowing Hall", map.LevelName);
        Assert.Equal("&wt@02", map.NextMap);
        Assert.Equal(1, map.Cluster);
        var cluster = set.FindCluster(1);
        Assert.NotNull(cluster);
        Assert.True(cluster.Hub);
        Assert.Equal(1.0 * (35 / 1000.0), map.SkySpeed1, precision: 8);
    }

    [Fact]
    public void DefaultLevelNum_MatchesMapAndEpisodeNames()
    {
        Assert.Equal(1, MapInfoParser.DefaultLevelNum("MAP01", out var id24));
        Assert.Equal(1, id24);
        Assert.Equal(1, MapInfoParser.DefaultLevelNum("E1M1", out id24));
        Assert.Equal(1, id24);
        Assert.Equal(13, MapInfoParser.DefaultLevelNum("E2M3", out id24));
        Assert.Equal(3, id24);
        Assert.Equal(0, MapInfoParser.DefaultLevelNum("E1M10", out id24));
        Assert.Equal(10, id24);
    }

    [Fact]
    public void TryParse_ClusterPicAndDollarLookup()
    {
        const string text = """
            cluster 2
            {
                name = "$TXT_CLUS2"
                pic = INTERPIC
                hub
            }
            """;

        Assert.True(MapInfoParser.TryParse(text, out var set, out var error), error);
        var cluster = set.FindCluster(2);
        Assert.NotNull(cluster);
        Assert.Equal("TXT_CLUS2", cluster.Name);
        Assert.True(cluster.LookupName);
        Assert.Equal("INTERPIC", cluster.Flat);
        Assert.True(cluster.FinaleIsPic);
        Assert.True(cluster.Hub);
    }

    [Fact]
    public void TryParse_ExistingClusterDefinitionIsNotForcedIntoAHub()
    {
        const string text = """
            cluster 1
            {
                name = "Start"
            }
            map 1 "Winnowing Hall"
            {
                cluster = 1
            }
            """;

        Assert.True(MapInfoParser.TryParse(text, out var set, out var error), error);
        var cluster = set.FindCluster(1);
        Assert.NotNull(cluster);
        Assert.False(cluster.Hub);
        Assert.Equal("Start", cluster.Name);
    }
}
