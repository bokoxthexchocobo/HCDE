using HCDE.Gamedata;

namespace HCDE.MapLoader.Tests;

public class PlayerSoundResolutionTests
{
    [Theory]
    [InlineData(0, "DSFIRST")]
    [InlineData(1, "DSSECOND")]
    [InlineData(2, "DSFIRST")]
    public void MultiChoiceCompatibilityPlaybackUsesOneDrawAndLeavesPlayerSlotSilent(int random, string expected)
    {
        Assert.True(SndInfoMapMusicParser.TryParseData(
            "$playersound base male *death DSDEATH\n$playercompat base male *death legacy\n"
            + "$random legacy { a b a }\na DSFIRST b DSSECOND", out var data, out var error), error);
        var level = new PlayLevel { PlayerSounds = data.PlayerSounds, SoundDefinitions = data.Sounds,
            SoundAliases = data.Aliases, SoundSettings = data.Settings, RandomSoundGroups = data.RandomGroups };
        var draws = 0;
        Assert.True(SoundResourceResolver.TryResolveNamed(level, "legacy", () => { draws++; return random; },
            out var resource, out _, out error), error);
        Assert.Equal(expected, resource); Assert.Equal(1, draws);
        Assert.True(SoundResourceResolver.TryResolveNamed(level, "*death", null, out resource, out _, out error), error);
        Assert.Equal("", resource);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RandomCompatibilityReplacementSeparatesLegacyPlaybackFromPlayerSlot(bool single)
    {
        Assert.True(SndInfoMapMusicParser.TryParseData(
            "$playersound base male *death DSDEATH\n$playercompat base male *death legacy\n"
            + "$random legacy { " + (single ? "choice" : "") + " }\nchoice DSNEW", out var data, out var error), error);
        var level = new PlayLevel { PlayerSounds = data.PlayerSounds, SoundDefinitions = data.Sounds,
            SoundAliases = data.Aliases, SoundSettings = data.Settings };
        Assert.True(SoundResourceResolver.TryResolveNamed(level, "legacy", null, out var resource, out _, out error), error);
        Assert.Equal(single ? "DSNEW" : "", resource);
        Assert.True(SoundResourceResolver.TryResolveNamed(level, "*death", null, out resource, out _, out error), error);
        Assert.Equal("", resource);
    }

    [Fact]
    public void UnmappedCompatibilityAndReservedSlotResolveSilently()
    {
        Assert.True(SndInfoMapMusicParser.TryParseData("$playercompat base male *new legacy", out var data, out var error), error);
        var level = new PlayLevel { PlayerSounds = data.PlayerSounds, SoundDefinitions = data.Sounds,
            SoundAliases = data.Aliases, SoundSettings = data.Settings };
        foreach (var name in new[] { "legacy", "*new" })
        {
            Assert.True(SoundResourceResolver.TryResolveNamed(level, name, null, out var resource, out _, out error), error);
            Assert.Equal("", resource);
        }
    }

    [Fact]
    public void CompatibilityResourceReplacementResolvesForBothNames()
    {
        Assert.True(SndInfoMapMusicParser.TryParseData(
            "$playersound base male *death DSDEATH\n$playercompat base male *death legacy\nlegacy DSNEW",
            out var data, out var error), error);
        var level = new PlayLevel { PlayerSounds = data.PlayerSounds, SoundDefinitions = data.Sounds,
            SoundAliases = data.Aliases, SoundSettings = data.Settings };
        foreach (var name in new[] { "legacy", "*death" })
        {
            Assert.True(SoundResourceResolver.TryResolveNamed(level, name, null, out var resource, out _, out error), error);
            Assert.Equal("DSNEW", resource);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OrdinaryAliasAndRandomChoiceReachPlayerReserveWithoutExtraRandomDraws(bool random)
    {
        var level = new PlayLevel { PlayerSounds = [new("base", 0, "*death", "mapped")],
            SoundDefinitions = new Dictionary<string, string> { ["mapped"] = "DSDEATH" },
            SoundAliases = new Dictionary<string, string> { ["entry"] = random ? "group" : "*death" },
            RandomSoundGroups = new Dictionary<string, IReadOnlyList<string>> { ["group"] = ["*death"] }
        };
        var draws = 0;
        Assert.True(SoundResourceResolver.TryResolveNamed(level, "entry", () => { draws++; return 0; },
            out var resource, out var name, out var error), error);
        Assert.Equal("DSDEATH", resource); Assert.Equal("mapped", name); Assert.Equal(random ? 1 : 0, draws);
    }

    [Fact]
    public void MixedOrdinaryAndPlayerAliasCycleRejects()
    {
        var level = new PlayLevel { PlayerSounds = [new("base", 0, "*death", "alias")],
            SoundAliases = new Dictionary<string, string> { ["alias"] = "*death" } };
        Assert.False(SoundResourceResolver.TryResolveNamed(level, "alias", null, out _, out _, out var error));
        Assert.StartsWith("sound-alias-cycle:", error);
    }

    [Theory]
    [InlineData(true, "male")]
    [InlineData(false, "default")]
    public void UnavailableGenderSoundFallsBackToMaleThenDefaultClass(bool maleAvailable, string expected)
    {
        var level = new PlayLevel { PlayerSounds = [new("base", 0, "*death", "default"),
            new("custom", 0, "*death", "male"), new("custom", 1, "*death", "female")] };
        Assert.True(SoundResourceResolver.TryResolvePlayerSound(level, "*death", "custom", out var sound, out var error,
            1, target => target == "default" || maleAvailable && target == "male"), error);
        Assert.Equal(expected, sound);
    }

    [Theory]
    [InlineData("custom", 0, "custom/male")]
    [InlineData("CUSTOM", 1, "custom/female")]
    [InlineData("custom", 2, "custom/male")]
    [InlineData("missing", 0, "base/death")]
    public void ClassAndGenderSelectNativeFallbacks(string name, int gender, string expected)
    {
        var level = new PlayLevel { PlayerSounds = [new("base", 0, "*death", "base/death"),
            new("custom", 0, "*death", "custom/male"), new("custom", 1, "*death", "custom/female")] };
        Assert.True(SoundResourceResolver.TryResolvePlayerSound(level, "*DEATH", name, out var result, out var error, gender), error);
        Assert.Equal(expected, result);
        var copy = level.CopyForSimulation(); Assert.Equal(level.PlayerSounds, copy.PlayerSounds);
    }

    [Fact]
    public void AliasCyclesRejectAndMissingSlotsFallBackToFirstClass()
    {
        var level = new PlayLevel { PlayerSounds = [new("base", 0, "*death", "base/death"),
            new("custom", 0, "*pain", "*other"), new("custom", 0, "*other", "*pain")] };
        Assert.True(SoundResourceResolver.TryResolvePlayerSound(level, "*death", "custom", out var result, out var error), error);
        Assert.Equal("base/death", result);
        Assert.False(SoundResourceResolver.TryResolvePlayerSound(level, "*pain", "custom", out _, out error));
        Assert.Equal("sound-player-alias-cycle", error);
    }
}
