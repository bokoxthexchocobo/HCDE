namespace HCDE.Client.Tests;

public class VmSearchPathConfigurationTests
{
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
    public void UnsupportedMultilineAndEntriesBeforeSectionsFailExplicitly()
    {
        Assert.Throws<InvalidDataException>(() => VmSearchPathConfiguration.Parse("Path=one"));
        Assert.Throws<NotSupportedException>(() => VmSearchPathConfiguration.Parse("[Other]\nValue=<<<END\nbody\n>>>END"));
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
