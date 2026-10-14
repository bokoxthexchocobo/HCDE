namespace HCDE.Gamedata.Tests;

public class SndInfoPlayerSoundTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CompatibilityRandomReplacementClearsCapturedTargetAndUpdatesOwnerOnlyForSingleChoice(bool single)
    {
        Assert.True(SndInfoMapMusicParser.TryParseData(
            "$playersound base male *death DSDEATH\n$playercompat base male *death legacy",
            out var first, out var error), error);
        var oldTarget = first.PlayerCompatibilityTargets["legacy"];
        Assert.True(SndInfoMapMusicParser.TryParseData("$random LEGACY { " + (single ? "newSound" : "") + " }",
            out var next, out error, first), error);
        Assert.Equal("", next.Sounds[oldTarget]);
        Assert.Equal("DSDEATH", first.Sounds[oldTarget]);
        Assert.Equal(single ? "newSound" : oldTarget, next.Aliases["legacy"]);
        Assert.Equal(next.Aliases["legacy"], next.PlayerCompatibilityTargets["legacy"]);
        Assert.Equal(single ? -1 : 2, next.Settings["legacy"].NearLimit);
        Assert.Equal(oldTarget, next.PlayerSounds[0].Target);
    }

    [Fact]
    public void LaterResourceOverrideFollowsSingleChoiceCompatibilityLink()
    {
        Assert.True(SndInfoMapMusicParser.TryParseData(
            "$playersound base male *death DSDEATH\n$playercompat base male *death legacy\n$random legacy { choice }",
            out var first, out var error), error);
        Assert.True(SndInfoMapMusicParser.TryParseData("legacy DSNEW", out var next, out error, first), error);
        Assert.Equal("DSNEW", next.Sounds["choice"]);
        Assert.Equal("choice", next.Aliases["legacy"]);
        Assert.Equal("", next.Sounds[next.PlayerSounds[0].Target]);
    }

    [Fact]
    public void UnmappedCompatibilityReservesSlotWithoutRegisteringClassAndStaysSilentAfterMapping()
    {
        Assert.True(SndInfoMapMusicParser.TryParseData(
            "$playersound base male *death DSDEATH\n$playercompat custom female *new legacy",
            out var first, out var error), error);
        Assert.Single(first.PlayerSounds);
        Assert.Contains("*new", first.PlayerReservedSlots);
        Assert.Equal("", first.Sounds["legacy"]);
        Assert.True(SndInfoMapMusicParser.TryParseData("$playersound custom female *new DSNEW", out var next, out error, first), error);
        Assert.Equal(2, next.PlayerSounds.Count);
        Assert.Equal("", next.Sounds["legacy"]);
        Assert.Equal("", next.PlayerCompatibilityTargets["legacy"]);
        Assert.Single(first.PlayerSounds);
    }

    [Theory]
    [InlineData("*NEW DSNEW")]
    [InlineData("$alias *NEW ordinary")]
    [InlineData("$random *NEW { ordinary }")]
    public void UnmappedCompatibilityReserveRejectsOrdinaryDefinitionsAcrossLumps(string replacement)
    {
        Assert.True(SndInfoMapMusicParser.TryParseData("$playercompat base male *new legacy", out var first, out var error), error);
        Assert.False(SndInfoMapMusicParser.TryParseData(replacement, out _, out error, first));
        Assert.Equal("Sounds that are reserved for players cannot be reassigned.", error);
    }

    [Theory]
    [InlineData("legacy DSNEW")]
    [InlineData("$alias legacy ordinary")]
    public void SilentCompatibilityTargetReplacementRejectsExplicitly(string replacement)
    {
        Assert.True(SndInfoMapMusicParser.TryParseData("$playercompat base male *new legacy", out var first, out var error), error);
        Assert.False(SndInfoMapMusicParser.TryParseData(replacement, out _, out error, first));
        Assert.Equal("Replacing a silent player compatibility target is unsupported.", error);
    }

    [Fact]
    public void SilentCompatibilityCanBeReboundAfterSlotIsDefined()
    {
        Assert.True(SndInfoMapMusicParser.TryParseData("$playercompat base male *new legacy", out var first, out var error), error);
        Assert.True(SndInfoMapMusicParser.TryParseData(
            "$playersound base male *new DSNEW\n$playercompat base male *new legacy", out var next, out error, first), error);
        Assert.False(next.Sounds.ContainsKey("legacy"));
        Assert.Equal(next.PlayerSounds[0].Target, next.Aliases["legacy"]);
        Assert.Equal("DSNEW", next.Sounds[next.Aliases["legacy"]]);
    }

    [Theory]
    [InlineData("legacy DSNEW", false)]
    [InlineData("$alias LEGACY replacement", true)]
    public void CompatibilityReplacementUpdatesCapturedTargetAcrossLumps(string replacement, bool alias)
    {
        Assert.True(SndInfoMapMusicParser.TryParseData(
            "$playersound base male *death DSDEATH\n$playercompat base male *death legacy",
            out var first, out var error), error);
        var target = first.PlayerCompatibilityTargets["legacy"];
        Assert.Equal(target, first.Aliases["legacy"]);
        Assert.True(SndInfoMapMusicParser.TryParseData(replacement, out var next, out error, first), error);
        Assert.Equal(target, next.Aliases["legacy"]);
        if (alias) Assert.Equal("replacement", next.Aliases[target]);
        else Assert.Equal("DSNEW", next.Sounds[target]);
        Assert.Equal("DSDEATH", first.Sounds[target]);
        Assert.Equal(target, next.PlayerSounds[0].Target);
    }

    [Fact]
    public void MultiChoiceCompatibilityReplacementClearsOldTargetAndPreservesChoices()
    {
        Assert.True(SndInfoMapMusicParser.TryParseData(
            "$playersound base male *death DSDEATH\n$playercompat base male *death legacy",
            out var first, out var error), error);
        Assert.True(SndInfoMapMusicParser.TryParseData("$random LEGACY { a b a }", out var next, out error, first), error);
        Assert.Equal(new[] { "a", "b", "a" }, next.RandomGroups["legacy"]);
        Assert.False(next.Aliases.ContainsKey("legacy"));
        var oldTarget = first.PlayerCompatibilityTargets["legacy"];
        Assert.Equal("", next.Sounds[oldTarget]);
        Assert.Equal("DSDEATH", first.Sounds[oldTarget]);
        Assert.Equal(-1, next.Settings["legacy"].NearLimit);
    }

    [Theory]
    [InlineData("legacy DSNEW")]
    [InlineData("$alias legacy choice")]
    [InlineData("$random legacy { choice }")]
    [InlineData("$playercompat base male *death legacy")]
    public void RedefiningMultiChoiceCompatibilityRejectsExplicitlyAcrossLumps(string replacement)
    {
        Assert.True(SndInfoMapMusicParser.TryParseData(
            "$playersound base male *death DSDEATH\n$playercompat base male *death legacy\n$random legacy { a b }",
            out var first, out var error), error);
        Assert.False(SndInfoMapMusicParser.TryParseData(replacement, out _, out error, first));
        Assert.Equal("Redefining a random player compatibility sound is unsupported.", error);
        Assert.Equal(new[] { "a", "b" }, first.RandomGroups["legacy"]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RepeatedCompatibilityDefinitionClearsOldTargetWithoutMutatingEarlierLump(bool sameSlot)
    {
        Assert.True(SndInfoMapMusicParser.TryParseData(
            "$playersound base male *death DSDEATH\n$playersound base male *pain DSPAIN\n"
            + "$playercompat base male *death legacy", out var first, out var error), error);
        var oldTarget = first.PlayerCompatibilityTargets["legacy"];
        Assert.True(SndInfoMapMusicParser.TryParseData(
            "$playercompat base male " + (sameSlot ? "*death" : "*pain") + " LEGACY",
            out var next, out error, first), error);
        Assert.Equal("", next.Sounds[oldTarget]);
        Assert.Equal("DSDEATH", first.Sounds[oldTarget]);
        var selected = next.PlayerSounds.Single(p => p.Slot == (sameSlot ? "*death" : "*pain")).Target;
        Assert.Equal(selected, next.PlayerCompatibilityTargets["legacy"]);
        Assert.Equal(selected, next.Aliases["legacy"]);
        Assert.Equal(sameSlot ? "" : "DSPAIN", next.Sounds[selected]);
    }

    [Fact]
    public void RepeatedCompatibilityDefinitionClearsOldAliasAndResetsInheritedLimit()
    {
        Assert.True(SndInfoMapMusicParser.TryParseData(
            "$playeralias base male *death original\n$playersound base male *pain DSPAIN\n"
            + "$playercompat base male *death legacy\n$alias legacy replacement",
            out var first, out var error), error);
        Assert.True(SndInfoMapMusicParser.TryParseData("$playercompat base male *pain legacy", out var next, out error, first), error);
        Assert.False(next.Aliases.ContainsKey("original"));
        Assert.Equal("", next.Sounds["original"]);
        Assert.Equal(2, next.Settings["original"].NearLimit);
        Assert.Equal("replacement", first.Aliases["original"]);
    }

    [Fact]
    public void CompatibilityCapturesAliasBeforePlayerMappingChanges()
    {
        Assert.True(SndInfoMapMusicParser.TryParseData(
            "$playeralias base male *death original\n$playercompat base male *death legacy\n"
            + "$playeralias base male *death replacement",
            out var data, out var error), error);
        Assert.Equal("original", data.Aliases["legacy"]);
        Assert.Equal("replacement", data.PlayerSounds[0].Target);
    }

    [Theory]
    [InlineData("*DEATH DSOTHER")]
    [InlineData("$alias *DEATH other")]
    [InlineData("$random *DEATH { other another }")]
    [InlineData("$random *DEATH { }")]
    public void ReservedPlayerSlotCannotBecomeOrdinarySound(string replacement)
    {
        const string definition = "$playersound base male *death DSDEATH\n";
        Assert.False(SndInfoMapMusicParser.TryParseData(definition + replacement, out _, out var error));
        Assert.Equal("Sounds that are reserved for players cannot be reassigned.", error);
        Assert.True(SndInfoMapMusicParser.TryParseData(definition, out var first, out error), error);
        Assert.False(SndInfoMapMusicParser.TryParseData(replacement, out _, out error, first));
        Assert.Single(first.PlayerSounds);
        Assert.Equal("DSDEATH", first.Sounds[first.PlayerSounds[0].Target]);
    }

    [Fact]
    public void OrdinaryAliasMayTargetReservedPlayerSlot()
    {
        Assert.True(SndInfoMapMusicParser.TryParseData(
            "$playersound base male *death DSDEATH\n$alias ordinary *death $volume *death 0.5",
            out var data, out var error), error);
        Assert.Equal("*death", data.Aliases["ordinary"]);
        Assert.Equal(0.5f, data.Settings["*death"].Volume);
    }

    [Theory]
    [InlineData("custom", "female")]
    [InlineData("custom", "male")]
    public void DuplicateMissingSlotDoesNotUseRuntimeMaleOrDefaultFallback(string className, string gender)
    {
        var text = "$playeralias base male *death base/death\n"
            + "$playeralias custom male *other custom/other\n"
            + "$playeralias custom female *other female/other\n"
            + $"$playersounddup {className} {gender} *copy *death";
        Assert.True(SndInfoMapMusicParser.TryParseData(text, out var data, out var error), error);
        Assert.Equal("", data.PlayerSounds.Single(p => p.Slot == "*copy").Target);
    }

    [Fact]
    public void DuplicateMissingFemaleSlotDoesNotUseDefinedMaleSlot()
    {
        var text = "$playeralias base male *death base/death\n"
            + "$playeralias base female *other female/other\n"
            + "$playersounddup base female *copy *death";
        Assert.True(SndInfoMapMusicParser.TryParseData(text, out var data, out var error), error);
        Assert.Equal("", data.PlayerSounds.Single(p => p.Slot == "*copy").Target);
    }

    [Theory]
    [InlineData("ABSENT")]
    [InlineData("DSEMPTY")]
    public void DuplicateCapturesResourceMappingWithoutCheckingPhysicalAvailability(string resource)
    {
        var text = "$playersound base male *death DSDEATH\n"
            + $"$playersound custom male *death {resource}\n"
            + "$playersounddup custom male *copy *death";
        Assert.True(SndInfoMapMusicParser.TryParseData(text, out var data, out var error), error);
        var copy = data.PlayerSounds.Single(p => p.Slot == "*copy");
        Assert.Equal(resource, data.Sounds[copy.Target]);
    }

    [Theory]
    [InlineData("custom", "female", "female/death")]
    [InlineData("custom", "other", "male/death")]
    [InlineData("missing", "male", "base/death")]
    public void DuplicateCapturesSelectedMappingBeforeLaterAliasOverride(string className, string gender, string expected)
    {
        var text = "$playeralias base male *death base/death\n"
            + "$playeralias custom male *death male/death\n"
            + "$playeralias custom female *death female/death\n"
            + $"$playersounddup {className} {gender} *copy *DEATH\n"
            + $"$playeralias {className} {gender} *death replacement";
        Assert.True(SndInfoMapMusicParser.TryParseData(text, out var data, out var error), error);
        Assert.Equal(expected, data.PlayerSounds.Single(p => p.Slot == "*copy").Target);
    }

    [Fact]
    public void DuplicateUsesEarlierLumpAndDoesNotMutateIt()
    {
        Assert.True(SndInfoMapMusicParser.TryParseData("$playersound base male *death DSDEATH", out var first, out var error), error);
        Assert.True(SndInfoMapMusicParser.TryParseData("$playersounddup BASE male *copy *death", out var next, out error, first), error);
        Assert.Single(first.PlayerSounds);
        Assert.Equal(first.PlayerSounds[0].Target, next.PlayerSounds[1].Target);
        Assert.Equal("DSDEATH", next.Sounds[next.PlayerSounds[1].Target]);
    }

    [Fact]
    public void SelfDuplicateWithoutMappingIsSilent()
    {
        Assert.True(SndInfoMapMusicParser.TryParseData("$playersounddup base male *copy *copy", out var data, out var error), error);
        Assert.Equal("", Assert.Single(data.PlayerSounds).Target);
    }

    [Theory]
    [InlineData("$playersounddup base male *copy unknown")]
    [InlineData("ordinary DSDEATH\n$playersounddup base male *copy ordinary")]
    [InlineData("$playersounddup base male *copy")]
    public void InvalidDuplicateRejects(string text)
    {
        Assert.False(SndInfoMapMusicParser.TryParseData(text, out _, out var error));
        Assert.NotNull(error);
    }

    [Fact]
    public void PlayerSoundsAndAliasesAccumulateWithCaseInsensitiveOverrides()
    {
        Assert.True(SndInfoMapMusicParser.TryParseData("$playersound player male *death DSDEATH\n$playeralias custom female *death custom/death", out var first, out var error), error);
        Assert.Equal(2, first.PlayerSounds.Count);
        Assert.Equal("DSDEATH", first.Sounds[first.PlayerSounds[0].Target]);
        Assert.True(SndInfoMapMusicParser.TryParseData("$playeralias PLAYER male *DEATH replacement", out var next, out error, first), error);
        Assert.Equal(2, next.PlayerSounds.Count); Assert.Equal("replacement", next.PlayerSounds[0].Target);
        Assert.NotEqual(first.PlayerSounds[0].Target, next.PlayerSounds[0].Target);
    }

    [Theory]
    [InlineData("$playersound player male *death")]
    [InlineData("$playeralias player invalid *death sound")]
    [InlineData("$playersound player male *death $alias")]
    [InlineData("*death DSDEATH\n$playersound player male *death DSOTHER")]
    public void InvalidPlayerDefinitionsReject(string text)
    {
        Assert.False(SndInfoMapMusicParser.TryParseData(text, out _, out var error)); Assert.NotNull(error);
    }
}
