namespace HCDE.Client.Tests;

public class VmIwadInfoParserTests
{
    [Fact]
    public void SingleRecordIgnoresLaterIwadBeforeParsingItsBody()
    {
        var warnings = new List<string>();
        var entry = VmIwadInfoParser.ParseSingle("IWAD { Name = First } IWAD malformed rest", warnings.Add);
        Assert.Equal("First", entry.Definition.Name);
        Assert.Contains("Multiple IWAD records ignored", Assert.Single(warnings));
        Assert.Throws<IOException>(() => VmIwadInfoParser.ParseSingle("IWAD { Name = First } IWAD",
            _ => throw new IOException("warning failed")));
        Assert.Throws<InvalidDataException>(() => VmIwadInfoParser.ParseSingle("Names { first.wad }"));
        Assert.Throws<InvalidDataException>(() => VmIwadInfoParser.ParseSingle("IWAD { Name = First } Order {}"));
        Assert.Throws<InvalidDataException>(() => VmIwadInfoParser.ParseSingle(""));
        Assert.Throws<InvalidDataException>(() => VmIwadInfoParser.Parse("IWAD { Name = First } IWAD malformed rest"));
    }

    [Theory]
    [InlineData("0x10", "16")]
    [InlineData("-0X10", "-16")]
    [InlineData("077", "63")]
    [InlineData("+012", "10")]
    [InlineData("MAXINT", "2147483647")]
    [InlineData("4294967295", "-1")]
    [InlineData("9223372036854775808", "-1")]
    [InlineData("-9223372036854775809", "0")]
    [InlineData("\" 16\"", "16")]
    [InlineData("\"\"", "0")]
    public void NumericMetadataUsesNativeBaseDetectionAndIntNarrowing(string token, string expected)
    {
        var entry = Assert.Single(VmIwadInfoParser.Parse("IWAD { Name = Test LoadLights = " + token
            + " IWADName = test.wad, " + token + " }").Entries);
        Assert.Equal(expected, entry.Properties["loadlights"][0]);
        Assert.Equal(expected, entry.Properties["iwadname"][1]);
    }

    [Theory]
    [InlineData("09")]
    [InlineData("0x")]
    [InlineData("maxint")]
    [InlineData("+")]
    [InlineData("\"16 \"")]
    [InlineData("0b10")]
    public void InvalidNativeNumberTokensAreRejected(string token)
    {
        Assert.Throws<InvalidDataException>(() => VmIwadInfoParser.Parse("IWAD { Name = Test LoadLights = " + token + " }"));
    }

    [Fact]
    public void NativeCatalogParsesAndIdentifiesDoomTwoWithoutHandWrittenDefinitions()
    {
        using var stream = typeof(VmIwadInfoParserTests).Assembly.GetManifestResourceStream("native-iwadinfo.txt")!;
        using var reader = new StreamReader(stream);
        var catalog = VmIwadInfoParser.Parse(reader.ReadToEnd());
        Assert.True(catalog.Entries.Count > 30);
        Assert.Contains("doom2.wad", catalog.Names);
        Assert.NotEmpty(catalog.Order);
        var match = VmIwadIdentifier.Identify(catalog.Entries.Select(entry => entry.Definition).ToArray(), [new("MAP01")]);
        Assert.NotNull(match);
        Assert.Equal("DOOM 2: Hell on Earth", match.Name);
        Assert.Equal("doom.id.doom2.commercial", match.AutoName);
        Assert.Equal(VmStartupGameType.Doom, match.Game);
        Assert.Equal("doom.doom2.commercial", VmStartupGameFilters.Build(match.Game, match.AutoName)[^1]);
    }

    [Fact]
    public void PropertiesListsDefaultsAndCommentsFollowCatalogRules()
    {
        var catalog = VmIwadInfoParser.Parse("""
            /* catalog */ iWaD {
                NAME = "first // title" Name = "last title"
                Autoname = "test.game" MustContain = "MAP01", "MARKER"
                MustContain = "MORE" NoKeyboardCheats SkipBexStringsIfLanguage
                Compatibility = poly1, Shorttex Compatibility = stairs
                IWADName = "test.wad", -2 BannerColors = "ff ff ff", "black"
                Load = "a.wad", "b.wad" LoadLights = -1
            }
            Names { "test.wad" } Order { "last title" }
            """);
        var entry = Assert.Single(catalog.Entries);
        Assert.Equal("last title", entry.Definition.Name);
        Assert.Equal(VmStartupGameType.Doom, entry.Definition.Game);
        Assert.Equal(new[] { "MAP01", "MARKER", "MORE" }, entry.Definition.MustContain);
        Assert.Equal("mapinfo/mindefaults.txt", entry.Properties["MAPINFO"][0]);
        Assert.Equal(new[] { "poly1", "Shorttex", "stairs" }, entry.Properties["compatibility"]);
        Assert.Equal(new[] { "test.wad", "-2" }, entry.Properties["iwadname"]);
        Assert.Equal("true", entry.Properties["nokeyboardcheats"][0]);
        Assert.Equal(new[] { "", "test.wad" }, catalog.Names);
        Assert.Equal(new[] { "last title" }, catalog.Order);
    }

    [Theory]
    [InlineData("IWAD { Name = \"x\" Unknown = 1 }")]
    [InlineData("IWAD { Name = \"x\" Game = unknown }")]
    [InlineData("IWAD { Name = \"x\" Compatibility = unknown }")]
    [InlineData("IWAD { Name = \"x\" LoadLights = abc }")]
    [InlineData("IWAD { Name = \"x\" BannerColors = red }")]
    [InlineData("IWAD { Name = \"x\" MustContain = MAP01, }")]
    [InlineData("IWAD { Name = \"x\"")]
    [InlineData("IWAD { Name = \"unterminated }")]
    [InlineData("/* unterminated")]
    [InlineData("UNKNOWN {}")]
    public void MalformedCatalogsFailWithSourceContext(string text)
    {
        var error = Assert.Throws<InvalidDataException>(() => VmIwadInfoParser.Parse(text));
        Assert.Contains("IWADINFO at offset", error.Message);
    }
}
