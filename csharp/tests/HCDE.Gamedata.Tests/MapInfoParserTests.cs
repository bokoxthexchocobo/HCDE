namespace HCDE.Gamedata.Tests;

public class MapInfoParserTests
{
    [Theory]
    [InlineData("\"TRACK: -2tail\"", "TRACK", -2)]
    [InlineData("\"TRACK:bad\"", "TRACK", 0)]
    [InlineData("\"\"", "", 0)]
    public void IntermissionGameDefaultsContinueWithoutMutatingPrior(string value, string music, int order)
    {
        Assert.True(MapInfoParser.TryParse("GameInfo { intermissionMusic = \"OLD:3\" }", out var prior, out var error), error);
        Assert.True(MapInfoParser.TryParse("GameInfo { intermissionMusic = " + value + " }", out var updated, out error, prior), error);
        Assert.Equal(new MapInfoMusic(music, order), updated.IntermissionMusic);
        Assert.Equal(new MapInfoMusic("OLD", 3), prior.IntermissionMusic);
    }

    [Theory]
    [InlineData("TRACK")]
    [InlineData("\"TRACK\", 3")]
    [InlineData("")]
    public void InvalidGameIntermissionMusicFails(string value)
    {
        Assert.False(MapInfoParser.TryParse("GameInfo { intermissionMusic = " + value + " }", out _, out var error));
        Assert.NotNull(error);
    }

    [Theory]
    [InlineData("map MAP01 One { music = \"TRACK:2\" intermusic = TALLY, 3 }")]
    [InlineData("defaultmap music TRACK 2 intermusic TALLY 3 map MAP01 One")]
    public void MapAndIntermissionMusicUseNativeOrdersAndDefaults(string text)
    {
        Assert.True(MapInfoParser.TryParse(text, out var info, out var error), error);
        var map = Assert.Single(info.Maps); Assert.Equal("TRACK", map.Music); Assert.Equal(2, map.MusicOrder);
        Assert.Equal("TALLY", map.IntermissionMusic); Assert.Equal(3, map.IntermissionMusicOrder);
    }

    [Theory]
    [InlineData("music =")]
    [InlineData("intermusic = TALLY, nope")]
    public void InvalidMapMusicFails(string property)
        => Assert.False(MapInfoParser.TryParse("map MAP01 One { " + property + " }", out _, out _));

    [Fact]
    public void GameFinaleDefaultsContinueAcrossInputs()
    {
        Assert.True(MapInfoParser.TryParse("GameInfo { finalemusic = \"MUSIC:3\" finaleflat = FLOOR }", out var first, out var error), error);
        Assert.True(MapInfoParser.TryParse("GameInfo { finaleflat = NEW }", out var second, out error, first), error);
        Assert.Equal("MUSIC", second.FinaleMusic); Assert.Equal(3, second.FinaleMusicOrder); Assert.Equal("NEW", second.FinaleFlat);
        Assert.Equal("FLOOR", first.FinaleFlat);
    }

    [Theory]
    [InlineData("finalemusic =")]
    [InlineData("finalemusic = MUSIC, nope")]
    [InlineData("finaleflat =")]
    public void InvalidGameFinaleDefaultsFail(string property)
        => Assert.False(MapInfoParser.TryParse("GameInfo { " + property + " }", out _, out _));

    [Theory]
    [InlineData("cluster 1 { cdtrack = -1 cdid = 0xFFFFFFFF }", -1, uint.MaxValue)]
    [InlineData("clusterdef 1 cdtrack 3 cdid 1234abcd", 3, 0x1234abcdu)]
    public void ClusterCdMetadataParsesAndResets(string text, int track, uint id)
    {
        Assert.True(MapInfoParser.TryParse(text, out var info, out var error), error);
        Assert.Equal(track, Assert.Single(info.Clusters).CdTrack); Assert.Equal(id, Assert.Single(info.Clusters).CdId);
        Assert.True(MapInfoParser.TryParse("cluster 1 { }", out var reset, out error, info), error);
        Assert.Equal(0, Assert.Single(reset.Clusters).CdTrack); Assert.Equal(0u, Assert.Single(reset.Clusters).CdId);
    }

    [Theory]
    [InlineData("cdtrack = nope")]
    [InlineData("cdid = 100000000")]
    [InlineData("cdid = nope")]
    public void InvalidClusterCdMetadataFails(string property)
        => Assert.False(MapInfoParser.TryParse("cluster 1 { " + property + " }", out _, out _));

    [Theory]
    [InlineData("cluster 1 { music = \"MUSIC:3\" }", 3)]
    [InlineData("cluster 1 { music = MUSIC, -2 }", -2)]
    [InlineData("clusterdef 1 music MUSIC 4 hub", 4)]
    [InlineData("cluster 1 { music = \"MUSIC: 2rest\" }", 2)]
    public void ClusterMusicParsesNativeOrderForms(string text, int order)
    {
        Assert.True(MapInfoParser.TryParse(text, out var info, out var error), error);
        Assert.Equal("MUSIC", Assert.Single(info.Clusters).Music); Assert.Equal(order, Assert.Single(info.Clusters).MusicOrder);
    }

    [Theory]
    [InlineData("music =")]
    [InlineData("music = MUSIC, nope")]
    public void InvalidClusterMusicFails(string property)
        => Assert.False(MapInfoParser.TryParse("cluster 1 { " + property + " }", out _, out _));

    [Theory]
    [InlineData("textmusic = Normal,")]
    [InlineData("textmusic = Normal, MUSIC, nope")]
    [InlineData("textpic = Normal,")]
    public void MalformedExitResourcesFail(string property)
        => Assert.False(MapInfoParser.TryParse("map MAP01 One { " + property + " }", out _, out _));

    [Fact]
    public void ExitTextListsAndDefaultsCopyWithoutSharingEntries()
    {
        Assert.True(MapInfoParser.TryParse("defaultmap { exittext = Normal, \"One\", \"Two\" } map MAP01 One { } map MAP02 Two { exittext = Normal, lookup, LABEL }", out var info, out var error), error);
        Assert.Equal("One\nTwo", info.FindMap("MAP01")!.ExitTexts["normal"].Text);
        Assert.Equal(new MapInfoExitText("LABEL", true), info.FindMap("MAP02")!.ExitTexts["Normal"]);
    }

    [Theory]
    [InlineData("exittext = Normal Text")]
    [InlineData("exittext = , Text")]
    [InlineData("exittext = Normal,")]
    public void MalformedExitTextFails(string property)
        => Assert.False(MapInfoParser.TryParse("map MAP01 One { " + property + " }", out _, out _));

    [Theory]
    [InlineData("cluster 1 { allowintermission entertextislump exittextislump entertext = INTRO exittext = OUTRO }")]
    [InlineData("clusterdef 1 allowintermission entertextislump exittextislump entertext INTRO exittext OUTRO")]
    public void ClusterFlagsDoNotConsumeFollowingProperties(string text)
    {
        Assert.True(MapInfoParser.TryParse(text, out var info, out var error), error);
        var cluster = Assert.Single(info.Clusters);
        Assert.True(cluster.AllowIntermission); Assert.True(cluster.EnterTextIsLump); Assert.True(cluster.ExitTextIsLump);
        Assert.Equal("INTRO", cluster.EnterText); Assert.Equal("OUTRO", cluster.ExitText);
    }

    [Fact]
    public void ClusterFlagsCopyAndResetWithExplicitRedefinition()
    {
        Assert.True(MapInfoParser.TryParse("cluster 1 { allowintermission entertextislump exittextislump }", out var first, out var error), error);
        Assert.True(MapInfoParser.TryParse("map MAP01 One { cluster = 1 }", out var second, out error, first), error);
        Assert.True(second.FindCluster(1)!.EnterTextIsLump);
        Assert.True(MapInfoParser.TryParse("cluster 1 { }", out var third, out error, second), error);
        Assert.True(first.FindCluster(1)!.AllowIntermission);
        Assert.False(third.FindCluster(1)!.AllowIntermission); Assert.False(third.FindCluster(1)!.EnterTextIsLump);
        Assert.False(third.FindCluster(1)!.ExitTextIsLump);
    }

    [Theory]
    [InlineData("flat = FLOOR", false, "FLOOR")]
    [InlineData("pic = IMAGE flat = FLOOR", true, "FLOOR")]
    [InlineData("flat = FLOOR pic = IMAGE", true, "IMAGE")]
    public void FinalePictureFlagFollowsNativeSetOnlySemantics(string properties, bool picture, string name)
    {
        Assert.True(MapInfoParser.TryParse("cluster 1 { " + properties + " }", out var info, out var error), error);
        var cluster = Assert.Single(info.Clusters); Assert.Equal(picture, cluster.FinaleIsPic); Assert.Equal(name, cluster.Flat);
    }

    [Fact]
    public void ClusterTextListsJoinLinesAndLookupCommaReadsLabel()
    {
        Assert.True(MapInfoParser.TryParse("cluster 1 { name = lookup, \"TITLE\" entertext = \"One\", \"Two\" exittext = \"$EXIT\" }", out var info, out var error), error);
        var cluster = Assert.Single(info.Clusters); Assert.Equal("TITLE", cluster.Name); Assert.True(cluster.LookupName);
        Assert.Equal("One\nTwo", cluster.EnterText); Assert.False(cluster.LookupEnterText);
        Assert.Equal("EXIT", cluster.ExitText); Assert.True(cluster.LookupExitText);
    }

    [Theory]
    [InlineData("entertext = \"One\",")]
    [InlineData("exittext = lookup,")]
    [InlineData("name =")]
    public void MissingClusterTextFails(string property)
        => Assert.False(MapInfoParser.TryParse("cluster 1 { " + property + " }", out _, out _));

    [Fact]
    public void LegacyClusterDefinitionParsesUntilNextMap()
    {
        Assert.True(MapInfoParser.TryParse("clusterdef 1 name Old entertext lookup INTRO music D_READ_M hub map 1 One cluster 1", out var info, out var error), error);
        var cluster = Assert.Single(info.Clusters); Assert.Equal("Old", cluster.Name);
        Assert.Equal("INTRO", cluster.EnterText); Assert.True(cluster.LookupEnterText); Assert.True(cluster.Hub);
        Assert.Equal("D_READ_M", cluster.Music); Assert.Equal(1, Assert.Single(info.Maps).Cluster);
    }

    [Fact]
    public void ExplicitClusterRedefinitionResetsFieldsAcrossInputs()
    {
        Assert.True(MapInfoParser.TryParse("cluster 1 { name = Old entertext = Text flat = FLOOR hub }", out var first, out var error), error);
        Assert.True(MapInfoParser.TryParse("clusterdef 1 name New", out var second, out error, first), error);
        Assert.Equal("Old", first.FindCluster(1)!.Name); Assert.True(first.FindCluster(1)!.Hub);
        var cluster = second.FindCluster(1)!; Assert.Equal("New", cluster.Name);
        Assert.Equal("", cluster.EnterText); Assert.Equal("", cluster.Flat); Assert.False(cluster.Hub);
    }

    [Theory]
    [InlineData("clusterdef nope")]
    [InlineData("clusterdef 1 { hub")]
    public void InvalidLegacyClusterFails(string text)
        => Assert.False(MapInfoParser.TryParse(text, out _, out _));

    [Theory]
    [InlineData("sky1 = SKY1", "SKY1")]
    [InlineData("sky1 = SKY1 sky2 = -NOFLAT-", "SKY1")]
    [InlineData("sky1 = SKY1 sky2 = SKY2", "SKY2")]
    public void SecondarySkyUsesNativeFallback(string properties, string expected)
    {
        Assert.True(MapInfoParser.TryParse("map MAP01 One { " + properties + " }", out var info, out var error), error);
        Assert.Equal(expected, Assert.Single(info.Maps).Sky2);
    }

    [Fact]
    public void LatestLevelNumberClaimClearsEarlierMapAcrossInputs()
    {
        Assert.True(MapInfoParser.TryParse("map MAP01 One { levelnum = 7 }", out var first, out var error), error);
        Assert.True(MapInfoParser.TryParse("map MAP02 Two { levelnum = 7 }", out var second, out error, first), error);
        Assert.Equal(7, first.FindMap("MAP01")!.LevelNum);
        Assert.Equal(0, second.FindMap("MAP01")!.LevelNum); Assert.Equal(7, second.FindMap("MAP02")!.LevelNum);
        Assert.All(second.Maps, map => Assert.Equal(0, map.Id24LevelNum));
    }

    [Theory]
    [InlineData("defaultmap gravity 400 aircontrol 0.25 map MAP01 One", 400, 0.25)]
    [InlineData("defaultmap gravity 400 adddefaultmap aircontrol 0.25 map MAP01 One", 400, 0.25)]
    [InlineData("defaultmap gravity 400 defaultmap aircontrol 0.25 map MAP01 One", 0, 0.25)]
    public void OldDefaultMapPropertiesStopAtNextDefinition(string text, double gravity, double control)
    {
        Assert.True(MapInfoParser.TryParse(text, out var info, out var error), error);
        var map = Assert.Single(info.Maps); Assert.Equal(gravity == 0 ? null : (double?)gravity, map.Gravity);
        Assert.Equal(control, map.AirControl);
    }

    [Theory]
    [InlineData("defaultmap { } map 1 One { }", true)]
    [InlineData("defaultmap { killeractivatesdeathspecials } map 1 One { }", true)]
    [InlineData("defaultmap { } map 1 One { killeractivatesdeathspecials }", false)]
    public void NumericHeaderAppliesHexenPolicyAfterDefaultsBeforeMapProperties(string text, bool own)
    {
        Assert.True(MapInfoParser.TryParse(text, out var info, out var error), error);
        Assert.True(Assert.Single(info.Maps).HexenHack); Assert.Equal(own, Assert.Single(info.Maps).ActivateOwnDeathSpecials);
    }

    [Fact]
    public void DefaultsAndDefinitionsContinueAcrossInputsWithoutMutatingPrevious()
    {
        Assert.True(MapInfoParser.TryParse("defaultmap { gravity = 400 aircontrol = 0.25 } map MAP01 One { } cluster 1 { name = Old }", out var first, out var error), error);
        Assert.True(MapInfoParser.TryParse("adddefaultmap { gravity = 200 } map MAP02 Two { } cluster 1 { name = New }", out var second, out error, first), error);
        Assert.Equal(400, first.FindMap("MAP01")!.Gravity); Assert.Equal("Old", first.FindCluster(1)!.Name);
        Assert.Equal(400, second.FindMap("MAP01")!.Gravity); Assert.Equal(200, second.FindMap("MAP02")!.Gravity);
        Assert.Equal(0.25, second.FindMap("MAP02")!.AirControl); Assert.Equal("New", second.FindCluster(1)!.Name);
        Assert.False(MapInfoParser.TryParse("adddefaultmap { gravity = nope }", out _, out _, second));
        Assert.True(MapInfoParser.TryParse("map MAP03 Three { }", out var third, out error, second), error);
        Assert.Equal(200, third.FindMap("MAP03")!.Gravity);
    }

    [Fact]
    public void DefaultMapResetsAndAddDefaultMapExtendsWithoutMutatingEarlierMaps()
    {
        const string text = "defaultmap { gravity = 400 aircontrol = 0.25 sky1 = SKY1 } map MAP01 One { } "
            + "adddefaultmap { gravity = 200 } map MAP02 Two { aircontrol = 0.5 } "
            + "defaultmap { nogravity } map MAP03 Three { }";
        Assert.True(MapInfoParser.TryParse(text, out var info, out var error), error);
        Assert.Equal(400, info.FindMap("MAP01")!.Gravity); Assert.Equal(0.25, info.FindMap("MAP01")!.AirControl);
        Assert.Equal(200, info.FindMap("MAP02")!.Gravity); Assert.Equal(0.5, info.FindMap("MAP02")!.AirControl);
        Assert.Equal("SKY1", info.FindMap("MAP02")!.Sky1);
        Assert.Equal(double.MaxValue, info.FindMap("MAP03")!.Gravity); Assert.Null(info.FindMap("MAP03")!.AirControl);
        Assert.Equal("", info.FindMap("MAP03")!.Sky1);
    }

    [Theory]
    [InlineData("defaultmap")]
    [InlineData("adddefaultmap { gravity = nope }")]
    [InlineData("defaultmap { aircontrol = 0.25")]
    public void MalformedDefaultMapFails(string text)
        => Assert.False(MapInfoParser.TryParse(text, out _, out _));

    [Theory]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("nope")]
    public void InvalidMapGravityFails(string value)
        => Assert.False(MapInfoParser.TryParse("map MAP01 Test { gravity = " + value + " }", out _, out _));

    [Theory]
    [InlineData("map MAP01 Test { aircontrol = 0.25 }", 0.25)]
    [InlineData("map 1 Test aircontrol 0", 0)]
    [InlineData("map MAP01 Test { aircontrol = 2 aircontrol = -1 }", -1)]
    public void AirControlParsesNativeFloatProperty(string text, double expected)
    {
        Assert.True(MapInfoParser.TryParse(text, out var info, out var error), error);
        Assert.Equal(expected, Assert.Single(info.Maps).AirControl);
    }

    [Theory]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("nope")]
    public void InvalidAirControlFails(string value)
        => Assert.False(MapInfoParser.TryParse("map MAP01 Test { aircontrol = " + value + " }", out _, out _));

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
