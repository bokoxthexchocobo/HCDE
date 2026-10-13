namespace HCDE.Client.Tests;

public class VmSearchPathConfigurationTests
{
    [Fact]
    public void SoundfontDefaultsKeepBankOrderAndMigrateWithoutDuplicatePaths()
    {
        var expected = new[] { "$HOME/soundfonts", "$HOME/fm_banks", "$PROGDIR/soundfonts", "$PROGDIR/fm_banks" };
        Assert.Equal(expected, VmSearchPathConfiguration.Parse("").WithWindowsDefaults().SoundfontPaths.Select(path => path.Value));
        var old = VmSearchPathConfiguration.Parse("[SoundfontSearch.Directories]\nPath=$home/soundfonts\nPath=custom\nRecursivePath=tree");
        var migrated = old.WithWindowsLegacySearchMigration();
        Assert.True(migrated.HasSoundfontSection);
        Assert.Equal(new[] { "$home/soundfonts", "custom", "tree", "$HOME/fm_banks", "$PROGDIR/soundfonts", "$PROGDIR/fm_banks" }, migrated.SoundfontPaths.Select(path => path.Value));
        Assert.Equal(migrated.SoundfontPaths, migrated.WithWindowsLegacySearchMigration().SoundfontPaths);
        var modern = VmSearchPathConfiguration.Parse("[LastRun]\nVersion=227\n[SoundfontSearch.Directories]");
        Assert.Empty(modern.WithWindowsLegacySearchMigration().WithWindowsDefaults().SoundfontPaths);
    }

    [Theory]
    [InlineData("226", true)]
    [InlineData("227", false)]
    [InlineData("228", false)]
    [InlineData("226suffix", true)]
    [InlineData("2.27e2suffix", false)]
    [InlineData("invalid", true)]
    public void LegacyVersionsAppendOnlyMissingDefaults(string version, bool migrate)
    {
        var config = VmSearchPathConfiguration.Parse("[LastRun]\nVersion=" + version +
            "\nVersion=0\n[FileSearch.Directories]\nPath=$home\nRecursivePath=$PROGDIR\n[IWADSearch.Directories]");
        var migrated = config.WithWindowsLegacySearchMigration();
        Assert.Equal(migrate ? 3 : 2, migrated.FilePaths.Count);
        Assert.Equal(migrate ? 2 : 0, migrated.IwadPaths.Count);
        Assert.Equal("$home", migrated.FilePaths[0].Value);
        if (migrate) Assert.Equal("$PROGDIR", migrated.FilePaths[2].Value);
        Assert.Equal(migrated.FilePaths, migrated.WithWindowsLegacySearchMigration().FilePaths);
    }

    [Fact]
    public void MissingVersionMigratesExistingSectionsBeforeCreatingAbsentDefaults()
    {
        var config = VmSearchPathConfiguration.Parse("[FileSearch.Directories]");
        var migrated = config.WithWindowsLegacySearchMigration();
        Assert.Equal(2, migrated.FilePaths.Count);
        Assert.Empty(migrated.IwadPaths);
        Assert.Equal(2, migrated.WithWindowsDefaults().IwadPaths.Count);
    }

    [Fact]
    public void WindowsDefaultsFillAbsentSectionsButPreserveExplicitEmptySections()
    {
        var missing = VmSearchPathConfiguration.Parse("[Other]\nValue=one").WithWindowsDefaults();
        Assert.Equal(new[] { "$HOME", "$PROGDIR" }, missing.IwadPaths.Select(path => path.Value));
        Assert.Equal(new[] { "$HOME", "$PROGDIR" }, missing.FilePaths.Select(path => path.Value));
        var empty = VmSearchPathConfiguration.Parse("[iwadsearch.directories]\n[filesearch.directories]").WithWindowsDefaults();
        Assert.Empty(empty.IwadPaths);
        Assert.Empty(empty.FilePaths);
        var mixed = VmSearchPathConfiguration.Parse("[FileSearch.Directories]\nRecursivePath=custom").WithWindowsDefaults();
        Assert.Equal("custom", Assert.Single(mixed.FilePaths).Value);
        Assert.Equal(2, mixed.IwadPaths.Count);
        Assert.Equal(mixed, mixed.WithWindowsDefaults());
    }

    [Fact]
    public void ByteLoaderPreservesUnicodeAndNativeBomRejection()
    {
        var text = "[FileSearch.Directories]\nPath=caf\u00e9";
        var bytes = System.Text.Encoding.UTF8.GetBytes(text);
        Assert.Equal("caf\u00e9", Assert.Single(VmSearchPathConfiguration.ParseBytes(bytes).FilePaths).Value);
        Assert.Throws<InvalidDataException>(() => VmSearchPathConfiguration.ParseBytes([239, 187, 191, .. bytes]));
        Assert.Throws<InvalidDataException>(() => VmSearchPathConfiguration.ParseBytes([255, 254, 91, 0]));
        Assert.Throws<InvalidDataException>(() => VmSearchPathConfiguration.ParseBytes([255]));
    }

    [Fact]
    public void FileLoaderPropagatesMissingFilesAndReadsWithoutEncodingDetection()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".ini");
        Assert.Throws<FileNotFoundException>(() => VmSearchPathConfiguration.ReadFile(path));
        try
        {
            File.WriteAllBytes(path, System.Text.Encoding.UTF8.GetBytes("[IWADSearch.Directories]\nPath=library"));
            Assert.Equal("library", Assert.Single(VmSearchPathConfiguration.ReadFile(path).IwadPaths).Value);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void RepeatedSectionsAndKeysKeepNativeOrderAndLiteralValues()
    {
        var config = VmSearchPathConfiguration.Parse("""
            # comment
            [filesearch.directories]
            RecursivePath = tree
            Path = "quoted"
            ;Path = ignored
            PathList = one;two # literal
            [Other]
            Path = ignored
            [FileSearch.Directories]
            Path = last
            [IWADSearch.Directories]
            PathList = ignored
            Path =
            RecursivePath = nested
            """);
        Assert.Equal(new[] { "tree", "\"quoted\"", "one;two # literal", "last" }, config.FilePaths.Select(path => path.Value));
        Assert.Equal(new[] { "", "nested" }, config.IwadPaths.Select(path => path.Value));
        Assert.Equal(VmFileSearchPathKind.RecursivePath, config.FilePaths[0].Kind);
    }

    [Fact]
    public void EntriesBeforeSectionsFailExplicitly()
    {
        Assert.Throws<InvalidDataException>(() => VmSearchPathConfiguration.Parse("Path=one"));
    }

    [Theory]
    [InlineData("one\r\ntwo\r\n>>>END\r\n", "one\ntwo")]
    [InlineData("one\n>>>END extra\n", "one\n")]
    [InlineData("one\n >>>END\ntwo", "one\n >>>END\ntwo")]
    [InlineData("one\n", "one\n")]
    [InlineData(">>>END\n", "")]
    public void MultilineValuesMatchNativeTerminatorAndEofRules(string body, string expected)
    {
        var config = VmSearchPathConfiguration.Parse("[FileSearch.Directories]\nPath=<<<END\n" + body);
        Assert.Equal(expected, Assert.Single(config.FilePaths).Value);
    }

    [Fact]
    public void UnrelatedMultilineBodiesCannotIntroduceSearchSections()
    {
        var config = VmSearchPathConfiguration.Parse("[Other]\nValue=<<<END\n[FileSearch.Directories]\nPath=hidden\n>>>END\n[FileSearch.Directories]\nPath=visible");
        Assert.Equal("visible", Assert.Single(config.FilePaths).Value);
    }

    [Fact]
    public void ConfigurationOptionRoundTripsAndRejectsMissingValues()
    {
        Assert.True(LauncherPlan.TryParse(["--iwad", "game.wad", "--search-config", "paths.ini"], out var selection, out _));
        Assert.True(LauncherPlan.TryParse(LauncherPlan.BuildArgs(selection), out var copy, out _));
        Assert.Equal("paths.ini", copy.SearchPathConfigPath);
        Assert.False(LauncherPlan.TryParse(["--iwad", "game.wad", "--search-config"], out _, out var error));
        Assert.Equal("search-config-path", error);
    }
}
