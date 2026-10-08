using System.Buffers.Binary;

namespace HCDE.MapLoader.Tests;

public class HexenBinaryFriendTests
{
    [Fact]
    public void RolloffDefaultsAndPerSoundSettingsLoadAndCopy()
    {
        var wad = WithMapInfo(("SNDINFO", "$rolloff * linear 10 100 $rolloff sound log 20 2"), ("SNDINFO", "sound RESOURCE"));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.Equal(new HCDE.Gamedata.SoundRolloff(HCDE.Gamedata.SoundRolloffType.Linear, 10, 100), level.GlobalSoundRolloff);
        Assert.Equal(HCDE.Gamedata.SoundRolloffType.Log, level.SoundSettings["sound"].Rolloff!.Type);
        Assert.Equal(level.GlobalSoundRolloff, level.CopyForSimulation().GlobalSoundRolloff);
    }

    [Fact]
    public void UnboundForwardTargetsResolveToSilenceAndLaterDefinitionsBindThem()
    {
        var wad = WithMapInfo(("SNDINFO", "$alias owner missing $volume pending 0.25"));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.True(SoundResourceResolver.TryResolve(level, "owner", out var resource, out error), error);
        Assert.Equal("", resource);
        Assert.True(SoundResourceResolver.TryResolve(level.CopyForSimulation(), "pending", out resource, out error), error);
        Assert.Equal("", resource);
        Assert.False(SoundResourceResolver.TryResolve(level, "unknown", out _, out _));
        wad = WithMapInfo(("SNDINFO", "$alias owner missing"), ("SNDINFO", "missing RESOURCE"));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out level, out error), error);
        Assert.True(SoundResourceResolver.TryResolve(level, "owner", out resource, out error), error);
        Assert.Equal("RESOURCE", resource); Assert.False(level.SoundSettings["missing"].IsTentative);
    }

    [Fact]
    public void GlobalPitchDefaultsLoadAcrossLumpsAndCopies()
    {
        var wad = WithMapInfo(("SNDINFO", "$pitchshiftrange 3 first RESOURCE"), ("SNDINFO", "second OTHER"));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.Equal(7, level.SoundSettings["second"].PitchMask);
        Assert.Equal(7, level.CopyForSimulation().SoundSettings["second"].PitchMask);
    }

    [Fact]
    public void PitchAndSingularSettingsLoadAndCopy()
    {
        var wad = WithMapInfo(("SNDINFO", "$singular sound $pitchshift sound 3 $pitchset sound 1.25 1.75"),
            ("SNDINFO", "sound RESOURCE"));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        var setting = level.SoundSettings["SOUND"];
        Assert.True(setting.Singular); Assert.Equal(7, setting.PitchMask);
        Assert.Equal(1.25f, setting.DefPitch); Assert.Equal(1.75f, setting.DefPitchMax);
        Assert.Equal(1.5f, HCDE.Gamedata.SoundPitchCalculator.Calculate(setting, true, nextFloat: () => 0.5f));
        Assert.Equal(setting, level.CopyForSimulation().SoundSettings["sound"]);
    }

    [Fact]
    public void SoundLimitsLoadAndCopyAcrossLumps()
    {
        var wad = WithMapInfo(("SNDINFO", "$limit sound 4 10"), ("SNDINFO", "sound RESOURCE $limit SOUND 7"));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.Equal(7, level.SoundSettings["sound"].NearLimit); Assert.Equal(100, level.SoundSettings["sound"].LimitRange);
        Assert.Equal(level.SoundSettings["sound"], level.CopyForSimulation().SoundSettings["SOUND"]);
    }

    [Fact]
    public void SoundSettingsLoadAcrossLumpsAndCopy()
    {
        var wad = WithMapInfo(("SNDINFO", "$volume sound 0.25 $attenuation sound 2"),
            ("SNDINFO", "sound RESOURCE $volume SOUND 0.5"));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.Equal(new HCDE.Gamedata.SndInfoSoundSettings(0.5f, 2), level.SoundSettings["sound"]);
        var copy = level.CopyForSimulation();
        Assert.Equal(level.SoundSettings["sound"], copy.SoundSettings["SOUND"]);
        Assert.NotSame(level.SoundSettings, copy.SoundSettings);
    }

    [Fact]
    public void WadsDisableRecursiveSoundsAfterAllDefinitionsLoad()
    {
        var wad = WithMapInfo(("SNDINFO", "$alias a b $alias prefix a $alias fixed fixed"),
            ("SNDINFO", "$random b{safe a} safe RESOURCE fixed FINAL"));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.Equal(new[] { "a", "b" }, level.DisabledSounds.Order());
        Assert.Equal(level.DisabledSounds, level.CopyForSimulation().DisabledSounds);
        Assert.True(SoundResourceResolver.TryResolve(level, "prefix", out var resource, out error), error);
        Assert.Equal("", resource);
        Assert.True(SoundResourceResolver.TryResolve(level, "fixed", out resource, out error), error);
        Assert.Equal("FINAL", resource);
    }

    [Fact]
    public void RandomGroupsLoadAndCopyWithoutSelectingAChoice()
    {
        var wad = WithMapInfo(("SNDINFO", "$random group{A B A} A FIRST B SECOND"));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.Equal(new[] { "A", "B", "A" }, level.RandomSoundGroups["GROUP"]);
        var copy = level.CopyForSimulation();
        Assert.NotSame(level.RandomSoundGroups, copy.RandomSoundGroups);
        Assert.NotSame(level.RandomSoundGroups["group"], copy.RandomSoundGroups["group"]);
        Assert.False(SoundResourceResolver.TryResolve(level, "group", out _, out error));
        Assert.StartsWith("sound-random-selection-required", error);
        Assert.True(SoundResourceResolver.TryResolve(copy, "GROUP", () => 1, out var resource, out error), error);
        Assert.Equal("SECOND", resource);
    }

    [Fact]
    public void SoundAliasesLoadAcrossLumpsAndCopy()
    {
        var wad = WithMapInfo(("SNDINFO", "fire OLD $alias fire target $alias other fire"),
            ("SNDINFO", "target NEW $alias FIRE final final LAST"));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.Equal("final", level.SoundAliases["fire"]);
        Assert.Equal("fire", level.SoundAliases["other"]);
        Assert.False(level.SoundDefinitions.ContainsKey("fire"));
        Assert.Equal("LAST", level.SoundDefinitions["final"]);
        Assert.True(SoundResourceResolver.TryResolve(level, "other", out var resource, out error), error);
        Assert.Equal("LAST", resource);
        var copy = level.CopyForSimulation();
        Assert.Equal("final", copy.SoundAliases["FIRE"]);
        Assert.NotSame(level.SoundAliases, copy.SoundAliases);
    }

    [Fact]
    public void SoundMappingsLoadInArchiveOrderAndSurviveSimulationCopy()
    {
        var wad = WithMapInfo(("SNDINFO", "weapons/pistol DSPIS $map 1 MUSIC"),
            ("SNDINFO", "WEAPONS/PISTOL = NEW misc/empty = \"\""),
            ("MAPINFO", "map MAP01 One { }"));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.Equal("NEW", level.SoundDefinitions["weapons/pistol"]);
        Assert.Equal("", level.SoundDefinitions["misc/empty"]); Assert.Equal("MUSIC", level.Music);
        var copy = level.CopyForSimulation();
        Assert.Equal("NEW", copy.SoundDefinitions["WEAPONS/PISTOL"]);
        Assert.NotSame(level.SoundDefinitions, copy.SoundDefinitions);
    }

    [Theory]
    [InlineData("music = BODY, 2", "BODY", 2)]
    [InlineData("music = \"\"", "", 0)]
    [InlineData("levelnum = 9", "DEFAULT", 0)]
    public void SndInfoDefaultsRespectMapBodyPrecedence(string properties, string resource, int order)
    {
        var wad = WithMapInfo(("SNDINFO", "$map 1 DEFAULT $map 9 OTHER"),
            ("MAPINFO", "map MAP01 One { music = OLD }"),
            ("ZMAPINFO", "map MAP01 One { " + properties + " }"));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.Equal(resource, level.Music); Assert.Equal(order, level.MusicOrder);
    }

    [Theory]
    [InlineData("$map bad TRACK")]
    [InlineData("$ambient 1 sound periodic 1 1")]
    public void InvalidOrUnsupportedSndInfoFailsWadLoading(string text)
    {
        Assert.False(LevelBuilder.TryFromWad(WithMapInfo(("SNDINFO", text)), "MAP01", out _, out var error));
        Assert.NotNull(error);
    }

    [Theory]
    [InlineData("", 4, 0x1234u)]
    [InlineData("cdtrack = -1 cdid = 0xFFFFFFFF", -1, uint.MaxValue)]
    [InlineData("cdtrack = 0 cdid = 0", 0, 0u)]
    public void MapCdSettingsLoadInheritedValuesAndOverrides(string properties, int track, uint disc)
    {
        var wad = WithMapInfo(("MAPINFO", "defaultmap { cdtrack = 4 cdid = 1234 } map MAP01 One { " + properties + " }"),
            ("MAPINFO", "map MAP02 Two { cdtrack = 7 cdid = AABB }"));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.Equal(track, level.CdTrack); Assert.Equal(disc, level.CdId);
        Assert.Equal(track, level.CopyForSimulation().CdTrack); Assert.Equal(disc, level.CopyForSimulation().CdId);
    }

    [Theory]
    [InlineData("cdtrack = bad")]
    [InlineData("cdid =")]
    [InlineData("cdid = xyz")]
    [InlineData("cdid = 100000000")]
    public void MalformedMapCdSettingsFailLoading(string property)
    {
        Assert.False(LevelBuilder.TryFromWad(WithMapInfo(("MAPINFO", "map MAP01 One { " + property + " }")), "MAP01", out _, out var error));
        Assert.NotNull(error);
    }

    [Theory]
    [InlineData("", "GAME", 7)]
    [InlineData("intermusic = LOCAL, 3", "LOCAL", 3)]
    [InlineData("mapintermusic = MAP02, DEST, 4", "DEST", 4)]
    [InlineData("mapintermusic = MAP02, \"\", 2", "", 2)]
    public void GameIntermissionFallbackLoadsAndCopies(string property, string resource, int order)
    {
        var wad = WithMapInfo(("MAPINFO", "GameInfo { intermissionMusic = \"OLD:1\" }"),
            ("ZMAPINFO", "GameInfo { intermissionMusic = \"GAME:7\" } map MAP01 One { " + property + " }"),
            ("ZMAPINFO", "GameInfo { titlepage = TITLE }"));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.Equal(new HCDE.Gamedata.MapInfoMusic("GAME", 7), level.GameIntermissionMusic);
        Assert.Equal(new HCDE.Gamedata.MapInfoMusic(resource, order), level.ResolveIntermissionMusic("map02"));
        Assert.Equal(level.ResolveIntermissionMusic("MAP02"), level.CopyForSimulation().ResolveIntermissionMusic("MAP02"));
    }

    [Theory]
    [InlineData("mapintermusic = MAP02, \"DEST:4\"", "DEST", 4)]
    [InlineData("mapintermusic = MAP02, \"\", 2", "", 2)]
    [InlineData("mapintermusic = MAP03, OTHER", "LOCAL", 3)]
    public void DestinationIntermissionMusicPrecedesMapMusic(string property, string resource, int order)
    {
        var wad = WithMapInfo(("MAPINFO", "defaultmap { intermusic = LOCAL, 3 " + property + " } map MAP01 One { } map MAP02 Two { mapintermusic = MAP02, SECOND }"));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        var fallback = new HCDE.Gamedata.MapInfoMusic("GAME", 7);
        Assert.Equal(new HCDE.Gamedata.MapInfoMusic(resource, order), level.ResolveIntermissionMusic("map02", fallback));
        var copy = level.CopyForSimulation();
        copy.MapIntermissionMusic["MAP02"] = fallback;
        Assert.Equal(new HCDE.Gamedata.MapInfoMusic(resource, order), level.ResolveIntermissionMusic("map02", fallback));
    }

    [Fact]
    public void EmptyMapIntermissionMusicUsesGameDefault()
    {
        var level = new PlayLevel { IntermissionMusicOrder = 4 };
        var fallback = new HCDE.Gamedata.MapInfoMusic("GAME", 7);
        Assert.Equal(fallback, level.ResolveIntermissionMusic("MAP02", fallback));
    }

    [Theory]
    [InlineData("mapintermusic = , TRACK")]
    [InlineData("mapintermusic = MAP02 TRACK")]
    [InlineData("mapintermusic = MAP02,")]
    [InlineData("mapintermusic = MAP02, TRACK, bad")]
    public void InvalidDestinationMusicFailsLoading(string property)
    {
        Assert.False(LevelBuilder.TryFromWad(WithMapInfo(("MAPINFO", "map MAP01 One { " + property + " }")), "MAP01", out _, out var error));
        Assert.NotNull(error);
    }

    [Theory]
    [InlineData("", "FLOOR", "MUSIC", 3)]
    [InlineData("textmusic = Normal, OVERRIDE, 2 textpic = Normal, IMAGE", "IMAGE", "OVERRIDE", 2)]
    [InlineData("textflat = Normal, FLOOR2", "FLOOR2", "MUSIC", 3)]
    public void ExplicitExitUsesGameFinaleDefaultsUnlessDefined(string extra, string background, string music, int order)
    {
        var text = "GameInfo { finalemusic = \"MUSIC:3\" finaleflat = FLOOR } map MAP01 One { exittext = Normal, Text " + extra + " } map MAP02 Two { }";
        Assert.True(HCDE.Gamedata.MapInfoParser.TryParse(text, out var info, out var error), error);
        Assert.True(ClusterTextResolver.TryResolveTransition([], info, "MAP01", "MAP02", false, out var result, out error), error);
        Assert.Equal(background, result!.Background); Assert.Equal(music, result.Music); Assert.Equal(order, result.MusicOrder);
    }

    [Theory]
    [InlineData(true, 7)]
    [InlineData(false, 3)]
    public void ClusterCdSelectionUsesSelectedTrackAndDestinationDisc(bool entry, int track)
    {
        var text = "map MAP01 One { cluster = 1 } map MAP02 Two { cluster = 2 } cluster 1 { exittext = Exit cdtrack = 3 cdid = 1111 } cluster 2 { cdtrack = 7 cdid = 2222 "
            + (entry ? "entertext = Entry " : "") + "}";
        Assert.True(HCDE.Gamedata.MapInfoParser.TryParse(text, out var info, out var error), error);
        Assert.True(ClusterTextResolver.TryResolveTransition([], info, "MAP01", "MAP02", false, out var result, out error), error);
        Assert.Equal(track, result!.CdTrack); Assert.Equal(0x2222u, result.CdId);
    }

    [Theory]
    [InlineData("exittext = Normal, Text textpic = Normal, IMAGE textflat = Normal, FLOOR textmusic = Normal, \"D_READ_M:3\"", "FLOOR", false, 3)]
    [InlineData("textflat = Normal, FLOOR textpic = Normal, IMAGE textmusic = Normal, D_READ_M, 2 exittext = Normal, Text", "IMAGE", true, 2)]
    public void ExitResourcesMergeIndependentlyAndReachTransition(string properties, string background, bool picture, int order)
    {
        Assert.True(HCDE.Gamedata.MapInfoParser.TryParse("map MAP01 One { " + properties + " } map MAP02 Two { }", out var info, out var error), error);
        Assert.True(ClusterTextResolver.TryResolveTransition([], info, "MAP01", "MAP02", false, out var result, out error), error);
        Assert.Equal("Text", result!.Text); Assert.Equal(background, result.Background);
        Assert.Equal(picture, result.BackgroundIsPicture); Assert.Equal("D_READ_M", result.Music); Assert.Equal(order, result.MusicOrder);
    }

    [Fact]
    public void ResourceOnlyExitRecordSuppressesClusterFallback()
    {
        Assert.True(HCDE.Gamedata.MapInfoParser.TryParse("map MAP01 One { cluster = 1 textmusic = Normal, MUSIC } map MAP02 Two { cluster = 2 } cluster 2 { entertext = Entry }", out var info, out var error), error);
        Assert.True(ClusterTextResolver.TryResolveTransition([], info, "MAP01", "MAP02", false, out var result, out error), error);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("exittext = MAP02, Destination", false, "Destination")]
    [InlineData("exittext = MAP02, Destination exittext = Normal, NormalText", false, "NormalText")]
    [InlineData("exittext = MAP02, Destination exittext = Secret, lookup, SECRET", true, "$SECRET")]
    [InlineData("exittext = Normal, \"\"", false, null)]
    public void ExplicitExitTextPrecedesClusterSelection(string properties, bool secret, string? expected)
    {
        var text = "map MAP01 One { cluster = 1 noclustertext " + properties + " } map MAP02 Two { cluster = 1 } cluster 1 { entertext = Cluster }";
        Assert.True(HCDE.Gamedata.MapInfoParser.TryParse(text, out var info, out var error), error);
        Assert.True(ClusterTextResolver.TryResolveTransition([], info, "MAP01", "MAP02", false, out var result, out error, secret), error);
        Assert.Equal(expected, result?.Text);
    }

    [Theory]
    [InlineData(false, false, false, "Entry", true)]
    [InlineData(true, false, false, "Exit", false)]
    [InlineData(false, true, false, null, false)]
    [InlineData(false, false, true, null, false)]
    public void ClusterTransitionPrefersEntryAndHonorsSuppression(bool noEntry, bool suppress, bool deathmatch, string? expected, bool entering)
    {
        var text = "map MAP01 One { cluster = 1 " + (suppress ? "noclustertext " : "")
            + "} map MAP02 Two { cluster = 2 } cluster 1 { exittext = Exit pic = IMAGE } cluster 2 { "
            + (noEntry ? "" : "entertext = Entry ") + "}";
        Assert.True(HCDE.Gamedata.MapInfoParser.TryParse(text, out var info, out var error), error);
        Assert.True(ClusterTextResolver.TryResolveTransition(WithMapInfo(), info, "MAP01", "MAP02", deathmatch, out var result, out error), error);
        Assert.Equal(expected, result?.Text);
        if (result is not null) { Assert.Equal(entering, result.Entering); Assert.Equal(entering ? "-" : "IMAGE", result.Background); }
    }

    [Fact]
    public void ClusterTransitionUsesLumpTextAndSameClusterSkipsIt()
    {
        Assert.True(HCDE.Gamedata.MapInfoParser.TryParse("map MAP01 One { cluster = 1 } map MAP02 Two { cluster = 2 } cluster 2 { entertext = INTRO entertextislump }", out var info, out var error), error);
        Assert.True(ClusterTextResolver.TryResolveTransition(WithMapInfo(("INTRO", "Loaded")), info, "MAP01", "MAP02", false, out var result, out error), error);
        Assert.Equal("Loaded", result!.Text);
        Assert.True(ClusterTextResolver.TryResolveTransition([], info, "MAP02", "MAP02", false, out result, out error), error); Assert.Null(result);
        Assert.False(ClusterTextResolver.TryResolveTransition([], info, "MISSING", "MAP02", false, out _, out error)); Assert.NotNull(error);
    }

    [Fact]
    public void ClusterLumpTextUsesLatestDefinitionAndTruncatesAtNull()
    {
        var wad = WithMapInfo(("INTRO", "Old"), ("INTRO", "New\nLine\0Ignored"));
        var cluster = new HCDE.Gamedata.MapInfoCluster { EnterText = "intro", EnterTextIsLump = true, LookupEnterText = true };
        Assert.True(ClusterTextResolver.TryResolve(wad, cluster, true, out var text, out var error), error);
        Assert.Equal("New\nLine", text); Assert.Equal("intro", cluster.EnterText);
    }

    [Theory]
    [InlineData(false, false, "Literal")]
    [InlineData(false, true, "$Literal")]
    [InlineData(true, true, "Unknown text lump 'Literal'")]
    public void ClusterExitTextUsesNativeResolutionPriority(bool lump, bool lookup, string expected)
    {
        var cluster = new HCDE.Gamedata.MapInfoCluster { ExitText = "Literal", ExitTextIsLump = lump, LookupExitText = lookup };
        Assert.True(ClusterTextResolver.TryResolve(WithMapInfo(), cluster, false, out var text, out var error), error);
        Assert.Equal(expected, text);
    }

    [Fact]
    public void InvalidTextArchiveFailsAndEmptyTextNeedsNoArchive()
    {
        var cluster = new HCDE.Gamedata.MapInfoCluster { EnterText = "INTRO", EnterTextIsLump = true };
        Assert.False(ClusterTextResolver.TryResolve([], cluster, true, out _, out var error)); Assert.NotNull(error);
        cluster.EnterText = ""; Assert.True(ClusterTextResolver.TryResolve([], cluster, true, out var text, out error), error);
        Assert.Equal("", text);
    }

    [Theory]
    [InlineData("adddefaultmap { gravity = 200 }", 200, 0.25)]
    [InlineData("defaultmap { gravity = 200 }", 200, LevelBuilder.DefaultAirControl)]
    public void DefaultMapPhysicsContinueAcrossLumps(string change, double gravity, double airControl)
    {
        var wad = WithMapInfo(("MAPINFO", "defaultmap { gravity = 400 aircontrol = 0.25 }"),
            ("MAPINFO", change + " map MAP01 Test { }"));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.Equal(gravity, level.LevelGravity); Assert.Equal(airControl, level.AirControl);
    }

    [Fact]
    public void DefaultMapPhysicsReachLoadedMapAndCopy()
    {
        var wad = WithMapInfo(("MAPINFO", "defaultmap { gravity = 400 aircontrol = 0.25 } adddefaultmap { gravity = 200 } map MAP01 Test { }"));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.Equal(200, level.LevelGravity); Assert.Equal(0.25, level.AirControl);
        Assert.Equal(200, level.CopyForSimulation().LevelGravity);
    }

    [Theory]
    [InlineData("gravity = 400", 400)]
    [InlineData("gravity = 0", 800)]
    [InlineData("nogravity", 0)]
    [InlineData("nogravity gravity = -800", -800)]
    [InlineData("gravity = 400 nogravity", 0)]
    public void MapGravityLoadsNativeUnitsAndSentinels(string property, double expected)
    {
        var wad = WithMapInfo(("MAPINFO", "map MAP01 Test { " + property + " }"));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.Equal(expected, level.LevelGravity); Assert.Equal(expected, level.CopyForSimulation().LevelGravity);
    }

    [Theory]
    [InlineData("MAPINFO", "map MAP01 Test { aircontrol = 0.5 }", 0.5)]
    [InlineData("ZMAPINFO", "map MAP01 Test { aircontrol = 0 }", LevelBuilder.DefaultAirControl)]
    [InlineData("MAPINFO", "map MAP01 Test { }", LevelBuilder.DefaultAirControl)]
    [InlineData("ZMAPINFO", "map MAP01 Test { aircontrol = -0.5 }", -0.5)]
    [InlineData("MAPINFO", "map MAP02 Test { aircontrol = 0.5 }", 0.25)]
    public void MapAirControlLoadsWithMapAndLumpPrecedence(string name, string text, double expected)
    {
        var wad = WithMapInfo(("MAPINFO", "map MAP01 Test { aircontrol = 0.25 }"), (name, text));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.Equal(expected, level.AirControl); Assert.Equal(expected, level.CopyForSimulation().AirControl);
    }

    [Fact]
    public void WadsWithoutMapInfoUseNativeDefaultAirControl()
    {
        Assert.True(LevelBuilder.TryFromWad(WithMapInfo(), "MAP01", out var level, out var error), error);
        Assert.Equal(LevelBuilder.DefaultAirControl, level.AirControl);
        Assert.Equal(LevelBuilder.DefaultAirControl, level.CopyForSimulation().AirControl);
    }

    [Theory]
    [InlineData("map 1 \"Original\" { }", false)]
    [InlineData("map MAP01 \"Extended\" { }", true)]
    [InlineData("map 2 \"Other\" { }", true)]
    [InlineData("map 1 \"Original\" { } map MAP01 \"Extended\" { }", true)]
    [InlineData("map MAP01 \"Extended\" { } map 1 \"Original\" { }", false)]
    public void MapInfoCompatibilityMasksExtendedThingFlags(string text, bool expected)
    {
        var wad = WithMapInfo(("MAPINFO", text));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        var thing = Assert.Single(level.Things);
        Assert.Equal(expected, thing.Friendly);
        Assert.True(thing.Dormant);
        Assert.True(thing.Ambush);
        Assert.True(thing.Single);
        Assert.Equal(42, thing.Id);
    }

    [Fact]
    public void ZmapInfoOverridesMapInfoCompatibility()
    {
        var wad = WithMapInfo(("MAPINFO", "map 1 \"Original\" { }"),
            ("ZMAPINFO", "map MAP01 \"Extended\" { }"));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.True(Assert.Single(level.Things).Friendly);
    }

    [Fact]
    public void LaterMapInfoLumpsOverrideEarlierDefinition()
    {
        var wad = WithMapInfo(("MAPINFO", "map MAP01 \"Extended\" { }"),
            ("MAPINFO", "map 1 \"Original\" { }"));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.False(Assert.Single(level.Things).Friendly);
    }

    [Fact]
    public void InvalidMapInfoReportsFailure()
    {
        Assert.False(LevelBuilder.TryFromWad(WithMapInfo(("MAPINFO", "map")), "MAP01", out _, out var error));
        Assert.False(string.IsNullOrWhiteSpace(error));
    }

    private static byte[] WithMapInfo(params (string Name, string Text)[] info)
    {
        var wad = HexenLevelTests.HexenWad();
        Assert.True(WadArchiveReader.TryReadDirectory(wad, out var entries, out _));
        var things = entries.Single(entry => entry.Name == "THINGS");
        BinaryPrimitives.WriteUInt16LittleEndian(wad.AsSpan((int)things.FilePosition + 12), 0x211A);
        var lumps = entries.Select(entry => (entry.Name,
            wad.AsSpan((int)entry.FilePosition, (int)entry.Size).ToArray())).ToList();
        lumps.AddRange(info.Select(item => (item.Name, System.Text.Encoding.UTF8.GetBytes(item.Text))));
        return MapsModsTests.Wad(lumps.ToArray());
    }

    [Theory]
    [InlineData("terrain Hot modify { }", "Fire")]
    [InlineData("terrain Hot { damagetype Ice }", "Ice")]
    public void TerrainLumpsLoadInOrderAndCopyDefinitions(string later, string expected)
    {
        var wad = WithMapInfo(("TERRAIN", "terrain Hot { damagetype Lava }"), ("TERRAIN", later));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.Equal(expected, Assert.Single(level.TerrainDefinitions).DamageType);
        Assert.Equal(expected, Assert.Single(level.CopyForSimulation().TerrainDefinitions).DamageType);
    }

    [Fact]
    public void InvalidTerrainLumpReportsFailure()
    {
        Assert.False(LevelBuilder.TryFromWad(WithMapInfo(("TERRAIN", "terrain Hot { damagetype }")), "MAP01", out _, out var error));
        Assert.NotNull(error);
    }

    [Fact]
    public void TerrainMappingAndDefaultLoadFromWad()
    {
        var wad = WithMapInfo(("TERRAIN", "terrain Hot { damagetype Lava } defaultterrain Hot floor optional FLOOR Hot"));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.Equal("Hot", level.DefaultTerrain);
        Assert.Equal(new LevelFloorTerrain("FLOOR", "Hot"), Assert.Single(level.FloorTerrainMappings));
        Assert.Equal("Fire", level.FloorTerrainDamageType(0));
    }

    [Theory]
    [InlineData("MAPINFO", "GameInfo { DefaultDropStyle = 1 }", 1)]
    [InlineData("MAPINFO", "GameInfo { titlepage = \"TITLEPIC\" }", 2)]
    [InlineData("ZMAPINFO", "GameInfo { DefaultDropStyle = 1 }", 1)]
    public void GameDefaultLoadsWithLumpPrecedence(string name, string text, int expected)
    {
        var wad = WithMapInfo(("MAPINFO", "GameInfo { DefaultDropStyle = 2 }"), (name, text));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.Equal(expected, level.DefaultDropStyle);
        Assert.Equal(expected, level.CopyForSimulation().DefaultDropStyle);
    }

    [Fact]
    public void DamageDefinitionsLoadAndLaterLumpsReplaceThem()
    {
        var wad = WithMapInfo(("MAPINFO", "DamageType Acid { NoArmor Factor = 2 }"),
            ("MAPINFO", "DamageType acid { Factor = 0.5 } DamageType Fire { NoArmor }"));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        var acid = level.DamageTypes.Single(d => d.Name.Equals("acid", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(0.5, acid.Factor); Assert.False(acid.NoArmor);
        Assert.Equal(2, level.CopyForSimulation().DamageTypes.Count);
    }

    [Fact]
    public void ZmapInfoDamageDefinitionsReplaceMapInfoSource()
    {
        var wad = WithMapInfo(("MAPINFO", "DamageType Acid { NoArmor }"),
            ("ZMAPINFO", "DamageType Fire { Factor = 0 }"));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.Equal("Fire", Assert.Single(level.DamageTypes).Name);
    }

    [Theory]
    [InlineData(0x102, false, false, false)]
    [InlineData(0x2102, true, false, false)]
    [InlineData(0x2182, true, false, false)]
    [InlineData(0x2112, true, true, false)]
    [InlineData(0x210A, true, false, true)]
    [InlineData(0x211A, true, true, true)]
    [InlineData(0xA102, true, false, false)]
    [InlineData(0x8102, false, false, false)]
    public void HexenFriendlyBitIsIndependentOfClassDormantAndAmbush(int flags, bool friendly, bool dormant, bool ambush)
    {
        var wad = HexenLevelTests.HexenWad();
        Assert.True(WadArchiveReader.TryReadDirectory(wad, out var entries, out var directoryError), directoryError);
        var things = entries.Single(entry => entry.Name == "THINGS");
        BinaryPrimitives.WriteUInt16LittleEndian(wad.AsSpan((int)things.FilePosition + 12), (ushort)flags);
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.Equal(MapDataFormat.HexenBinary, level.Format);
        var thing = Assert.Single(level.Things);
        Assert.Equal(friendly, thing.Friendly);
        Assert.Equal(dormant, thing.Dormant);
        Assert.Equal(ambush, thing.Ambush);
        Assert.True(thing.Single);
        Assert.Equal(4, thing.SkillMask);
        Assert.Equal(42, thing.Id);
        Assert.Equal(80, thing.Special);
    }
}
