namespace HCDE.Client.Tests;

public class VmIwadCompanionResourcesTests
{
    [Fact]
    public void ConfiguredSearchesPreserveNormalPriorityAndRecursiveFileOnlyLookup()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var normal = Directory.CreateDirectory(Path.Combine(root, "normal")).FullName;
        var nested = Directory.CreateDirectory(Path.Combine(root, "tree", "nested")).FullName;
        var name = Guid.NewGuid().ToString("N") + ".pk3";
        var entry = Assert.Single(VmIwadInfoParser.Parse("IWAD { Name = Test Load = \":" + name + "\" }").Entries);
        try
        {
            var file = Path.Combine(nested, name);
            File.WriteAllBytes(file, []);
            Assert.Empty(VmIwadCompanionResources.Resolve(entry, "game.wad", warning: _ => { }, searchDirectories: [Path.GetDirectoryName(nested)!]));
            Assert.Equal(file, Assert.Single(VmIwadCompanionResources.Resolve(entry, "game.wad", recursiveDirectories: [Path.GetDirectoryName(nested)!])));
            var first = Path.Combine(normal, name);
            Directory.CreateDirectory(first);
            Assert.Equal(first, Assert.Single(VmIwadCompanionResources.Resolve(entry, "game.wad", searchDirectories: [normal], recursiveDirectories: [Path.GetDirectoryName(nested)!])));
            File.Delete(file);
            Directory.CreateDirectory(file);
            Assert.Empty(VmIwadCompanionResources.Resolve(entry, "game.wad", warning: _ => { }, recursiveDirectories: [Path.GetDirectoryName(nested)!]));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Theory]
    [InlineData("--file-search-dir")]
    [InlineData("--file-recursive-dir")]
    public void FileSearchArgumentsRoundTripAndRejectMissingValues(string option)
    {
        Assert.True(LauncherPlan.TryParse(["--iwad", "game.wad", option, "one", option, "two"], out var selection, out _));
        Assert.True(LauncherPlan.TryParse(LauncherPlan.BuildArgs(selection), out var copy, out _));
        Assert.Equal(new[] { "one", "two" }, option == "--file-search-dir" ? copy.FileSearchDirectories : copy.RecursiveFileSearchDirectories);
        foreach (var tail in new[] { Array.Empty<string>(), new[] { " " }, new[] { "--map", "MAP01" } })
        {
            Assert.False(LauncherPlan.TryParse(["--iwad", "game.wad", option, .. tail], out _, out var error));
            Assert.Equal("file-search-directory", error);
        }
    }

    [Fact]
    public void NativeLoadOrderSearchModeExtensionAndDuplicatesArePreserved()
    {
        var entry = Assert.Single(VmIwadInfoParser.Parse("""
            IWAD { Name = Test Load = "local", ":engine.pk3", "missing.pk3", "local" }
            """).Entries);
        var root = Path.Combine(Path.GetTempPath(), "hcde-companion-tests");
        var attempts = new List<(string Name, bool ProgramFirst)>();
        var warnings = new List<string>();
        var result = VmIwadCompanionResources.Resolve(entry, Path.Combine(root, "game.wad"), (name, programFirst) =>
        {
            attempts.Add((name, programFirst));
            return name.EndsWith("local.wad", StringComparison.Ordinal) ? Path.Combine(root, "local.wad")
                : name == "engine.pk3" ? Path.Combine(root, "engine.pk3") : null;
        }, warnings.Add);
        Assert.Equal(new[] { Path.Combine(root, "local.wad"), Path.Combine(root, "engine.pk3") }, result);
        Assert.Equal(("engine.pk3", true), attempts[2]);
        Assert.EndsWith("/local", attempts[0].Name);
        Assert.False(attempts[0].ProgramFirst);
        Assert.Contains("missing.pk3", Assert.Single(warnings));
        Assert.DoesNotContain(attempts, attempt => attempt.Name.EndsWith("missing.pk3.wad", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("")]
    [InlineData(":")]
    public void EmptyLoadNamesFailExplicitly(string load)
    {
        var entry = Assert.Single(VmIwadInfoParser.Parse("IWAD { Name = Test Load = \"" + load + "\" }").Entries);
        Assert.Throws<InvalidDataException>(() => VmIwadCompanionResources.Resolve(entry, "game.wad"));
    }

    [Fact]
    public void ResolverAndWarningFailuresPropagate()
    {
        var entry = Assert.Single(VmIwadInfoParser.Parse("IWAD { Name = Test Load = missing }").Entries);
        Assert.Throws<IOException>(() => VmIwadCompanionResources.Resolve(entry, "game.wad",
            (_, _) => throw new IOException("search failed")));
        Assert.Throws<IOException>(() => VmIwadCompanionResources.Resolve(entry, "game.wad",
            (_, _) => null, _ => throw new IOException("warning failed")));
    }
}
