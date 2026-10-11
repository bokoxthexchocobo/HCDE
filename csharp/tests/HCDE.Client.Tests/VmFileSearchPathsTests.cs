namespace HCDE.Client.Tests;

public class VmFileSearchPathsTests
{
    [Fact]
    public void ListsExpandBeforeSplittingAndAgainAfterSplittingInOrder()
    {
        var paths = VmFileSearchPaths.Expand([
            new(VmFileSearchPathKind.RecursivePath, "tree"),
            new(VmFileSearchPathKind.PathList, "$LIST"),
            new(VmFileSearchPathKind.Path, "$MISSING"),
            new(VmFileSearchPathKind.Path, "")],
            name => name switch { "LIST" => "one" + Path.PathSeparator + Path.PathSeparator + "$SECOND", "SECOND" => "two", _ => null });
        Assert.Equal(new[] { "tree", "one", "two", "." }, paths.Select(path => path.Value));
        Assert.Equal(VmFileSearchPathKind.RecursivePath, paths[0].Kind);
        Assert.All(paths.Skip(1), path => Assert.Equal(VmFileSearchPathKind.Path, path.Kind));
    }

    [Fact]
    public void InterleavedSearchPrioritySurvivesLauncherRoundTripAndLookup()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var tree = Directory.CreateDirectory(Path.Combine(root, "tree", "nested")).FullName;
        var normal = Directory.CreateDirectory(Path.Combine(root, "normal")).FullName;
        var name = Guid.NewGuid().ToString("N") + ".pk3";
        try
        {
            File.WriteAllBytes(Path.Combine(tree, name), []);
            File.WriteAllBytes(Path.Combine(normal, name), []);
            Assert.True(LauncherPlan.TryParse(["--iwad", "game.wad", "--search-recursive-path", Path.GetDirectoryName(tree)!,
                "--search-path-list", normal], out var selection, out _));
            Assert.True(LauncherPlan.TryParse(LauncherPlan.BuildArgs(selection), out var copy, out _));
            Assert.Equal(selection.OrderedFileSearchPaths, copy.OrderedFileSearchPaths);
            var entry = Assert.Single(VmIwadInfoParser.Parse("IWAD { Name = Test Load = \":" + name + "\" }").Entries);
            Assert.Equal(Path.Combine(tree, name), Assert.Single(VmIwadCompanionResources.Resolve(entry, "game.wad",
                orderedSearchPaths: VmFileSearchPaths.Expand(copy.OrderedFileSearchPaths))));
            Assert.Equal(Path.Combine(normal, name), Assert.Single(VmIwadCompanionResources.Resolve(entry, "game.wad",
                orderedSearchPaths: VmFileSearchPaths.Expand(copy.OrderedFileSearchPaths.Reverse().ToArray()))));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Theory]
    [InlineData("--search-path")]
    [InlineData("--search-recursive-path")]
    [InlineData("--search-path-list")]
    public void OrderedOptionsRejectMissingOrBlankValues(string option)
    {
        foreach (var suffix in new[] { Array.Empty<string>(), new[] { " " }, new[] { "--map", "MAP01" } })
        {
            Assert.False(LauncherPlan.TryParse(["--iwad", "game.wad", option, .. suffix], out _, out var error));
            Assert.Equal("file-search-directory", error);
        }
    }
}
