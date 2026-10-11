namespace HCDE.Client.Tests;

public class VmIwadSearchDirectoryTests
{
    [Fact]
    public void DependencySearchRespectsDirectoryPriorityAndRecursion()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var selectedDirectory = Directory.CreateDirectory(Path.Combine(root, "selected")).FullName;
        var searchDirectory = Directory.CreateDirectory(Path.Combine(root, "library")).FullName;
        var nested = Directory.CreateDirectory(Path.Combine(searchDirectory, "nested")).FullName;
        try
        {
            var selected = Path.Combine(selectedDirectory, "game.wad");
            var dependency = Path.Combine(nested, "base.wad");
            // An empty valid WAD is sufficient for a definition with no requirements.
            var catalog = VmIwadInfoParser.Parse("IWAD { Name = Game Required = Base MustContain = GAME } IWAD { Name = Base } NAMES { base.wad }");
            File.WriteAllBytes(dependency, [80, 87, 65, 68, 0, 0, 0, 0, 12, 0, 0, 0]);
            Assert.Throws<InvalidDataException>(() => VmIwadRequiredResource.Resolve(catalog.Entries[0], catalog, selected,
                searchDirectories: [searchDirectory]));
            Assert.Equal(dependency, VmIwadRequiredResource.Resolve(catalog.Entries[0], catalog, selected,
                recursiveDirectories: [searchDirectory]));
            var first = Path.Combine(searchDirectory, "base.wad");
            File.Copy(dependency, first);
            Assert.Equal(first, VmIwadRequiredResource.Resolve(catalog.Entries[0], catalog, selected,
                searchDirectories: [searchDirectory], recursiveDirectories: [searchDirectory]));
            var adjacent = Path.Combine(selectedDirectory, "base.wad");
            File.Copy(dependency, adjacent);
            Assert.Equal(adjacent, VmIwadRequiredResource.Resolve(catalog.Entries[0], catalog, selected,
                searchDirectories: [searchDirectory]));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void SearchOptionsRoundTripRepeatedDirectories()
    {
        Assert.True(LauncherPlan.TryParse(["--iwad", "game.wad", "--iwad-search-dir", "one",
            "--iwad-search-dir", "two", "--iwad-recursive-dir", "tree"], out var selection, out _));
        Assert.True(LauncherPlan.TryParse(LauncherPlan.BuildArgs(selection), out var parsed, out _));
        Assert.Equal(new[] { "one", "two" }, parsed.IwadSearchDirectories);
        Assert.Equal(new[] { "tree" }, parsed.RecursiveIwadSearchDirectories);
    }

    [Theory]
    [InlineData("--iwad-search-dir")]
    [InlineData("--iwad-recursive-dir")]
    public void SearchOptionsRejectMissingOrBlankPaths(string option)
    {
        foreach (var suffix in new[] { Array.Empty<string>(), new[] { " " }, new[] { "--map", "MAP01" } })
        {
            Assert.False(LauncherPlan.TryParse(["--iwad", "game.wad", option, .. suffix], out _, out var error));
            Assert.Equal("iwad-search-directory", error);
        }
    }
}
