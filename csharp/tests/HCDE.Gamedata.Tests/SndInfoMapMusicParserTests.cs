namespace HCDE.Gamedata.Tests;

public class SndInfoMapMusicParserTests
{
    [Theory]
    [InlineData("", SoundRolloffType.Doom)]
    [InlineData("linear", SoundRolloffType.Linear)]
    [InlineData("log", SoundRolloffType.Log)]
    [InlineData("custom", SoundRolloffType.Custom)]
    public void RolloffParsesTypesAndRetainsTypeWhenOmitted(string type, SoundRolloffType expected)
    {
        Assert.True(SndInfoMapMusicParser.TryParseData("$rolloff sound " + type + " 10 20 $rolloff * linear 30 40", out var prior, out var error), error);
        Assert.Equal(new SoundRolloff(expected, 10, 20), prior.Settings["sound"].Rolloff);
        Assert.True(SndInfoMapMusicParser.TryParseData("$rolloff sound -1 5", out var next, out error, prior), error);
        Assert.Equal(new SoundRolloff(expected, -1, 5), next.Settings["sound"].Rolloff);
        Assert.Equal(10, prior.Settings["sound"].Rolloff!.MinDistance);
        Assert.Equal(prior.GlobalRolloff, next.GlobalRolloff);
    }

    [Theory]
    [InlineData("$rolloff")]
    [InlineData("$rolloff sound bad 1 2")]
    [InlineData("$rolloff sound linear 1")]
    [InlineData("$rolloff sound NaN 2")]
    public void MalformedRolloffFails(string text)
    {
        Assert.False(SndInfoMapMusicParser.TryParseData(text, out _, out var error)); Assert.NotNull(error);
    }

    [Theory]
    [InlineData("$volume pending 0.5")]
    [InlineData("$singular pending")]
    [InlineData("$alias owner pending")]
    [InlineData("$random owner{pending other}")]
    public void TentativeRegistrationClearsOnDefinitionWithoutChangingPitch(string text)
    {
        Assert.True(SndInfoMapMusicParser.TryParseData(text, out var prior, out var error), error);
        Assert.True(prior.Settings["pending"].IsTentative);
        Assert.True(SndInfoMapMusicParser.TryParseData("$pitchshiftrange 7 pending RESOURCE", out var next, out error, prior), error);
        Assert.False(next.Settings["pending"].IsTentative); Assert.Equal(0, next.Settings["pending"].PitchMask);
        Assert.True(prior.Settings["pending"].IsTentative);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(3, 7)]
    [InlineData(99, 127)]
    public void GlobalPitchDefaultsClampAndContinueAcrossInputs(int shift, int expected)
    {
        Assert.True(SndInfoMapMusicParser.TryParseData("$pitchshiftrange " + shift + " first RESOURCE", out var prior, out var error), error);
        Assert.Equal(expected, prior.CurrentPitchMask); Assert.Equal(expected, prior.Settings["first"].PitchMask);
        Assert.True(SndInfoMapMusicParser.TryParseData("second NEXT $pitchshiftrange 1 first REPLACED", out var next, out error, prior), error);
        Assert.Equal(expected, next.Settings["second"].PitchMask); Assert.Equal(expected, next.Settings["first"].PitchMask);
        Assert.Equal(expected, prior.CurrentPitchMask); Assert.Equal(1, next.CurrentPitchMask);
    }

    [Fact]
    public void ForwardTargetsRetainZeroMaskWhileNewOwnersInheritGlobal()
    {
        Assert.True(SndInfoMapMusicParser.TryParseData("$pitchshiftrange 3 $alias owner target $random group{choice other} target RESOURCE choice OTHER", out var data, out var error), error);
        Assert.Equal(7, data.Settings["owner"].PitchMask); Assert.Equal(7, data.Settings["group"].PitchMask);
        Assert.Equal(0, data.Settings["target"].PitchMask); Assert.Equal(0, data.Settings["choice"].PitchMask);
    }

    [Theory]
    [InlineData("$pitchshiftrange")]
    [InlineData("$pitchshiftrange bad")]
    public void InvalidGlobalPitchFails(string text)
    {
        Assert.False(SndInfoMapMusicParser.TryParseData(text, out _, out var error)); Assert.NotNull(error);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(3, 7)]
    [InlineData(99, 127)]
    public void PitchShiftClampsToNativeMask(int shift, int expected)
    {
        Assert.True(SndInfoMapMusicParser.TryParseData("$pitchshift sound " + shift + " $singular SOUND sound RESOURCE", out var data, out var error), error);
        Assert.Equal(expected, data.Settings["sound"].PitchMask); Assert.True(data.Settings["sound"].Singular);
    }

    [Fact]
    public void PitchSetContinuationClearsAbsentMaximumAndPreservesOtherSettings()
    {
        Assert.True(SndInfoMapMusicParser.TryParseData("$pitchset sound 1.5 2 $pitchshift sound 4 $singular sound", out var prior, out var error), error);
        Assert.True(SndInfoMapMusicParser.TryParseData("$pitchset SOUND -0.5 sound RESOURCE", out var next, out error, prior), error);
        Assert.Equal(2, prior.Settings["sound"].DefPitchMax);
        Assert.Equal(-0.5f, next.Settings["sound"].DefPitch); Assert.Equal(0, next.Settings["sound"].DefPitchMax);
        Assert.Equal(15, next.Settings["sound"].PitchMask); Assert.True(next.Settings["sound"].Singular);
    }

    [Theory]
    [InlineData("$singular")]
    [InlineData("$pitchshift sound bad")]
    [InlineData("$pitchset sound")]
    [InlineData("$pitchset sound NaN")]
    [InlineData("$pitchset sound 1 Infinity")]
    public void InvalidPitchOrSingularSettingsFail(string text)
    {
        Assert.False(SndInfoMapMusicParser.TryParseData(text, out _, out var error)); Assert.NotNull(error);
    }

    [Theory]
    [InlineData(-2, 0)]
    [InlineData(3, 3)]
    [InlineData(999, 255)]
    public void SoundLimitsClampAndSquareSignedDistances(int value, int expected)
    {
        Assert.True(SndInfoMapMusicParser.TryParseData("$limit sound " + value + " -12.5 sound RESOURCE", out var data, out var error), error);
        Assert.Equal(expected, data.Settings["sound"].NearLimit);
        Assert.Equal(156.25f, data.Settings["sound"].LimitRange);
    }

    [Fact]
    public void LimitWithoutDistanceRetainsRangeAndAliasRedefinitionResetsInheritance()
    {
        Assert.True(SndInfoMapMusicParser.TryParseData("$limit sound 4 12 $limit sound 5 $alias sound target", out var prior, out var error), error);
        Assert.Equal(-1, prior.Settings["sound"].NearLimit); Assert.Equal(144, prior.Settings["sound"].LimitRange);
        Assert.True(SndInfoMapMusicParser.TryParseData("sound RESOURCE", out var next, out error, prior), error);
        Assert.Equal(2, next.Settings["sound"].NearLimit); Assert.Equal(65536, next.Settings["sound"].LimitRange);
    }

    [Theory]
    [InlineData("$limit")]
    [InlineData("$limit sound bad")]
    [InlineData("$limit sound 3 1e100")]
    public void MalformedLimitsFail(string text)
    {
        Assert.False(SndInfoMapMusicParser.TryParseData(text, out _, out var error)); Assert.NotNull(error);
    }

    [Fact]
    public void SoundSettingsPersistAcrossDefinitionsAndContinuation()
    {
        Assert.True(SndInfoMapMusicParser.TryParseData("$volume sound 2.5 $attenuation SOUND -1 sound RESOURCE $alias sound target", out var prior, out var error), error);
        Assert.Equal(new SndInfoSoundSettings(2.5f, -1, -1), prior.Settings["sound"]);
        Assert.True(SndInfoMapMusicParser.TryParseData("$volume sound 0 $attenuation other 3 $random sound { A B }", out var next, out error, prior), error);
        Assert.Equal(new SndInfoSoundSettings(0, -1, -1), next.Settings["SOUND"]);
        Assert.Equal(new SndInfoSoundSettings(1, 3, IsTentative: true), next.Settings["other"]);
        Assert.Equal(2.5f, prior.Settings["sound"].Volume);
    }

    [Theory]
    [InlineData("$volume")]
    [InlineData("$attenuation sound bad")]
    [InlineData("$volume sound NaN")]
    [InlineData("$attenuation sound 1e100")]
    [InlineData("$volume $map 1")]
    public void MalformedSoundSettingsFail(string text)
    {
        Assert.False(SndInfoMapMusicParser.TryParseData(text, out var data, out var error));
        Assert.Empty(data.Settings); Assert.NotNull(error);
    }

    [Fact]
    public void RandomGroupsPreserveDuplicateChoicesAndContinuation()
    {
        Assert.True(SndInfoMapMusicParser.TryParseData("$random group{A B A} $random single { target } $random empty{}", out var data, out var error), error);
        Assert.Equal(new[] { "A", "B", "A" }, data.RandomGroups["GROUP"]);
        Assert.Equal("target", data.Aliases["single"]); Assert.Equal("", data.Sounds["empty"]);
        Assert.True(SndInfoMapMusicParser.TryParseData("$alias group target", out var next, out error, data), error);
        Assert.True(data.RandomGroups.ContainsKey("group")); Assert.False(next.RandomGroups.ContainsKey("group"));
    }

    [Theory]
    [InlineData("group RESOURCE")]
    [InlineData("$alias group target")]
    [InlineData("$random group {}")]
    public void LaterDefinitionsReplaceRandomGroups(string replacement)
    {
        Assert.True(SndInfoMapMusicParser.TryParseData("$random group { A B } " + replacement, out var data, out var error), error);
        Assert.Empty(data.RandomGroups);
    }

    [Theory]
    [InlineData("$random")]
    [InlineData("$random group A")]
    [InlineData("$random group { A")]
    [InlineData("$random group { $alias A B }")]
    public void MalformedRandomGroupsFail(string text)
    {
        Assert.False(SndInfoMapMusicParser.TryParseData(text, out var data, out var error));
        Assert.Empty(data.RandomGroups); Assert.NotNull(error);
    }

    [Theory]
    [InlineData("$alias weapon/fire weapon/target", true)]
    [InlineData("$alias weapon/fire weapon/target weapon/fire NEW", false)]
    public void AliasesReplaceResourcesAndResourcesClearAliases(string suffix, bool alias)
    {
        Assert.True(SndInfoMapMusicParser.TryParseData("weapon/fire OLD " + suffix, out var data, out var error), error);
        Assert.Equal(alias, data.Aliases.ContainsKey("WEAPON/FIRE"));
        Assert.Equal(!alias, data.Sounds.ContainsKey("WEAPON/FIRE"));
        if (alias) Assert.Equal("weapon/target", data.Aliases["WEAPON/FIRE"]);
        else Assert.Equal("NEW", data.Sounds["WEAPON/FIRE"]);
    }

    [Fact]
    public void AliasForwardReferencesAndContinuationRemainIndependent()
    {
        Assert.True(SndInfoMapMusicParser.TryParseData("$alias A B $alias B C", out var prior, out var error), error);
        Assert.True(SndInfoMapMusicParser.TryParseData("B RESOURCE C FINAL", out var next, out error, prior), error);
        Assert.Equal("C", prior.Aliases["b"]); Assert.False(next.Aliases.ContainsKey("b"));
        Assert.Equal("B", next.Aliases["a"]); Assert.Equal("FINAL", next.Sounds["c"]);
    }

    [Theory]
    [InlineData("$alias")]
    [InlineData("$alias A")]
    [InlineData("$alias A $map 1 TRACK")]
    [InlineData("$alias A = B")]
    public void MalformedAliasesFail(string text)
    {
        Assert.False(SndInfoMapMusicParser.TryParseData(text, out var data, out var error));
        Assert.Empty(data.Aliases); Assert.NotNull(error);
    }

    [Theory]
    [InlineData("weapons/pistol DSPIS $map 1 TRACK weapons/pistol NEW")]
    [InlineData("\"weapons/pistol\"=DSPIS $map 1 TRACK weapons/pistol=NEW")]
    public void LogicalSoundsCoexistWithMusicAndReplaceCaseInsensitively(string text)
    {
        Assert.True(SndInfoMapMusicParser.TryParseData(text, out var data, out var error), error);
        Assert.Equal("NEW", data.Sounds["WEAPONS/PISTOL"]);
        Assert.Equal("TRACK", data.MapMusic[1]);
        Assert.True(SndInfoMapMusicParser.TryParseData("weapons/pistol FINAL", out var next, out error, data), error);
        Assert.Equal("NEW", data.Sounds["weapons/pistol"]); Assert.Equal("FINAL", next.Sounds["weapons/pistol"]);
        Assert.Equal("TRACK", next.MapMusic[1]);
    }

    [Theory]
    [InlineData("sound")]
    [InlineData("sound = FIRST other SECOND")]
    [InlineData("sound FIRST other = SECOND")]
    [InlineData("sound $map 1 TRACK")]
    public void InvalidLogicalMappingsFailWithoutPartialResults(string text)
    {
        Assert.False(SndInfoMapMusicParser.TryParseData(text, out var data, out var error));
        Assert.Empty(data.Sounds); Assert.Empty(data.MapMusic); Assert.NotNull(error);
    }

    [Theory]
    [InlineData("", "TRACK:3", 5)]
    [InlineData("music = BODY, 2", "BODY", 2)]
    [InlineData("levelnum = 9", "TRACK:3", 5)]
    public void MusicDefaultAppliesAtHeaderBeforeMapProperties(string body, string resource, int order)
    {
        Assert.True(SndInfoMapMusicParser.TryParse("$map 1 \"TRACK:3\" $map 9 OTHER", out var music, out var error), error);
        Assert.True(MapInfoParser.TryParse("defaultmap { music = INHERITED, 5 } map MAP01 One { " + body + " }", out var info, out error, mapMusic: music), error);
        var map = Assert.Single(info.Maps);
        Assert.Equal(resource, map.Music); Assert.Equal(order, map.MusicOrder);
    }

    [Fact]
    public void MapMusicPreservesNativeNumbersResourcesAndReplacement()
    {
        Assert.True(SndInfoMapMusicParser.TryParse("// comment\n$MAP 1 OLD /* ignored */ $map 0 IGNORE $map -2 \"TRACK:3\" $map 1 NEW", out var music, out var error), error);
        Assert.Equal(2, music.Count);
        Assert.Equal("NEW", music[1]); Assert.Equal("TRACK:3", music[-2]);
    }

    [Fact]
    public void ContinuationCopiesPriorAndAcceptsEmptyResource()
    {
        Assert.True(SndInfoMapMusicParser.TryParse("$map 1 OLD", out var prior, out var error), error);
        Assert.True(SndInfoMapMusicParser.TryParse("$map 1 \"\" $map 2 NEXT", out var next, out error, prior), error);
        Assert.Equal("OLD", prior[1]); Assert.Equal("", next[1]); Assert.Equal("NEXT", next[2]);
    }

    [Theory]
    [InlineData("$map")]
    [InlineData("$map bad TRACK")]
    [InlineData("$map 1 $map 2 TRACK")]
    [InlineData("$map 1 \"unfinished")]
    [InlineData("/* unfinished")]
    [InlineData("$ambient 1 sound periodic 1 1")]
    public void MalformedOrUnsupportedInputFails(string text)
    {
        Assert.False(SndInfoMapMusicParser.TryParse(text, out var music, out var error));
        Assert.Empty(music); Assert.NotNull(error);
    }
}
