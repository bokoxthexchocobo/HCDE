namespace HCDE.Client.Tests;

public class VmFindFile1369SliceTests
{
    [Fact]
    public void WildConfigAndMissing()
    {
        var slice = NewSlice(_ => false);
        var wads = new List<string>();
        slice.AddWildFile(wads, null, "*.wad", null, false);
        slice.AddWildFile(wads, "", "*.wad", null, false);
        Assert.Empty(wads);
        Assert.Empty(slice.Trace);

        slice.DirExists = path => path == "doom.wad";
        slice.AddWildFile(wads, "doom.wad", "*.wad", null, false);
        Assert.Equal(new[] { "doom.wad" }, wads);
        Assert.DoesNotContain("ScanDirectory", slice.Trace);

        slice = NewSlice(_ => false);
        slice.Scan = (path, name, noSubdir) =>
        {
            Assert.Equal("mods/", path);
            Assert.Equal("*.wad", name);
            Assert.True(noSubdir);
            return
            [
                new VmFindFile1369Slice.ScanHit { FilePath = "mods/a.wad", IsDirectory = false },
                new VmFindFile1369Slice.ScanHit { FilePath = "mods/dir", IsDirectory = true },
            ];
        };
        wads = [];
        slice.AddWildFile(wads, "mods/*", "*.wad", null, false);
        Assert.Equal(new[] { "mods/a.wad", "mods/dir" }, wads);

        slice = NewSlice(_ => false);
        slice.Scan = (_, _, _) => null;
        slice.AddWildFile(wads, "mods/*", "*.wad", null, true);
        Assert.Equal(new[] { "optional wad not found: mods/*\n" }, slice.Prints);
        Assert.Null(slice.FatalMessage);

        slice.Prints.Clear();
        slice.AddWildFile(wads, "need.wad", null, null, false);
        Assert.Equal("wad not found: need.wad\n", slice.Prints[^1]);
        Assert.Equal(
            "Cannot find wad 'need.wad'\nTo ignore this error and continue, set cvar i_exit_on_not_found to 1\n",
            slice.FatalMessage);

        slice = NewSlice(path => path == "doom*.wad");
        var seen = new List<string>();
        slice.DirExists = path =>
        {
            seen.Add(path);
            return path == "doom*.wad";
        };
        wads = [];
        slice.AddWildFile(wads, "doom", "*.wad", null, false);
        Assert.Equal(new[] { "doom", "doom*.wad" }, seen);
        Assert.Equal(new[] { "doom*.wad" }, wads);
        Assert.DoesNotContain("ScanDirectory", slice.Trace);

        var config = new VmFindFile1369Slice.ConfigFile();
        config.Add("Game.Autoload", "Path", "$MOD/a.wad");
        config.Add("Game.Autoload", "Note", "skip");
        config.Add("Game.Autoload", "path", "b.wad");
        config.Add("FileSearch.Directories", "Path", "/nowhere");
        slice = NewSlice(path => path == "/mods/a.wad");
        slice.ExpandEnv = value => value == "$MOD/a.wad" ? "/mods/a.wad" : value;
        slice.Scan = (path, name, noSubdir) =>
        {
            Assert.Equal(".", path);
            Assert.Equal("b.wad", name);
            Assert.True(noSubdir);
            return [new VmFindFile1369Slice.ScanHit { FilePath = "found/b.wad" }];
        };
        wads = [];
        slice.AddConfigFiles(wads, "missing", "*.wad", config, false);
        slice.AddConfigFiles(wads, "Game.Autoload", "*.wad", null, false);
        Assert.Empty(wads);
        slice.AddConfigFiles(wads, "Game.Autoload", "*.wad", config, false);
        Assert.Equal(new[] { "/mods/a.wad", "found/b.wad" }, wads);
        Assert.Equal(2, slice.Trace.Count(name => name == "ExpandEnvVars"));
        Assert.Throws<InvalidOperationException>(() => slice.AddConfigFiles(wads, null, "*.wad", config, false));

        slice = NewSlice(_ => false);
        slice.Scan = (path, name, _) =>
        {
            Assert.Equal(".", path);
            Assert.Equal(@"a\b.wad", name);
            return [];
        };
        slice.ExitOnNotFound = 0;
        slice.AddWildFile(wads, @"a\b.wad", null, null, false);
        Assert.Equal(@"wad not found: a\b.wad" + "\n", slice.Prints[^1]);
        Assert.Null(slice.FatalMessage);
    }

    [Fact]
    public void SearchAndDirectory()
    {
        var slice = NewSlice(_ => false);
        Assert.Null(slice.BaseFileSearch(null, ".wad", true, null));
        Assert.Null(slice.BaseFileSearch("", ".wad", false, null));
        Assert.Empty(slice.Trace);

        slice.ProgDir = "PROG";
        slice.DirExists = path => path == "PROG/a";
        Assert.Equal("PROG/a", slice.BaseFileSearch("a", null, true, null));
        Assert.Equal("PROG/a", slice.LastFound);
        Assert.Equal(new[] { "DirEntryExists" }, slice.Trace);

        slice.ProgDir = "PROG/";
        slice.Trace.Clear();
        slice.DirExists = path => path == "PROG/a";
        Assert.Equal("PROG/a", slice.BaseFileSearch("a", null, true, null));
        slice.ProgDir = "";
        Assert.Throws<InvalidOperationException>(() => slice.BaseFileSearch("a", null, true, null));

        slice = NewSlice(path => path == "a");
        Assert.Equal("a", slice.BaseFileSearch("a", null, false, null));

        var config = new VmFindFile1369Slice.ConfigFile();
        config.Add("filesearch.directories", "Other", "nope");
        config.Add("filesearch.directories", "Path", "mods");
        config.Add("filesearch.directories", "RecursivePath", "tree/");
        config.Add("filesearch.directories", "PathList", "a::b:");
        slice = NewSlice(path => path == "tree/sub/a.wad");
        slice.RecursiveExists = (dir, file) =>
        {
            Assert.Equal("tree", dir);
            Assert.Equal("a.wad", file);
            return "tree/sub/a.wad";
        };
        Assert.Equal("tree/sub/a.wad", slice.BaseFileSearch("a.wad", null, false, config));
        Assert.Contains("NicePath", slice.Trace);
        Assert.DoesNotContain("ExpandEnvVars", slice.Trace);

        slice = NewSlice(path => path == "B/a.wad");
        slice.NicePath = value => value == "a" ? "" : value == "b" ? "B" : value;
        slice.ExpandEnv = value =>
        {
            Assert.Equal("a::b:", value);
            return value;
        };
        Assert.Equal("B/a.wad", slice.BaseFileSearch("a.wad", null, false, config));

        var root = new List<string>();
        slice = NewSlice(_ => false);
        slice.DirExists = path =>
        {
            root.Add(path);
            return false;
        };
        Assert.Null(slice.BaseFileSearch("/", ".wad", false, null));
        Assert.Equal(new[] { "/", "/" + ".wad" }, root);

        slice = NewSlice(_ => false);
        var dirs = new List<(string Path, string Pattern, bool NoSubdir)>();
        slice.Scan = (path, pattern, noSubdir) =>
        {
            dirs.Add((path, pattern, noSubdir));
            if (pattern == "*.pk3")
                return null;
            return
            [
                new VmFindFile1369Slice.ScanHit { FilePath = "dir", IsDirectory = true },
                new VmFindFile1369Slice.ScanHit { FilePath = "one.wad", IsDirectory = false },
            ];
        };
        var wads = new List<string>();
        var configRef = config;
        slice.AddFile = (list, file, check, position, optional, seen) =>
        {
            Assert.True(check);
            Assert.Equal(-1, position);
            Assert.False(optional);
            Assert.Same(configRef, seen);
            list.Add(file);
        };
        Assert.Throws<InvalidOperationException>(() => slice.AddDirectory(wads, null, "*.wad", config, false));
        slice.AddDirectory(wads, "mods", null, config, false);
        slice.AddDirectory(wads, "mods", "", config, false);
        slice.AddDirectory(wads, "mods", "*.pk3", config, false);
        Assert.Equal(new[] { "one.wad", "one.wad" }, wads);
        Assert.Equal(("mods", "*.wad", true), dirs[0]);
        Assert.Equal(("mods", "*.wad", true), dirs[1]);
        Assert.Equal(("mods", "*.pk3", true), dirs[2]);

        slice.ExitOnNotFound = 0;
        slice.FileNotFound(VmFindFile1369Slice.RequireFile, "wad", "x");
        Assert.Equal("wad not found: x\n", slice.Prints[^1]);
        Assert.Null(slice.FatalMessage);
        slice.ExitOnNotFound = VmFindFile1369Slice.RequireIwad | VmFindFile1369Slice.RequireFile | VmFindFile1369Slice.RequireOptFile;
        slice.FileNotFound(VmFindFile1369Slice.RequireOptFile, "optional wad", "y");
        Assert.Equal(
            "Cannot find optional wad 'y'\nTo ignore this error and continue, set cvar i_exit_on_not_found to 3\n",
            slice.FatalMessage);

        slice = NewSlice(_ => false);
        slice.RecursiveExists = (dir, file) =>
        {
            Assert.Equal("", dir);
            Assert.Equal("z", file);
            return "";
        };
        config = new VmFindFile1369Slice.ConfigFile();
        config.Add("FileSearch.Directories", "RecursivePath", "/");
        Assert.Null(slice.BaseFileSearch("z", null, false, config));
        Assert.Contains("RecursiveFileExists", slice.Trace);
    }

    static VmFindFile1369Slice NewSlice(Func<string, bool> exists)
    {
        var slice = new VmFindFile1369Slice
        {
            DirExists = exists,
            NicePath = value => value,
            ExpandEnv = value => value,
            RecursiveExists = (_, _) => "",
            Scan = (_, _, _) => null,
            Fatal = _ => { },
        };
        slice.AddFile = (wads, file, check, position, _, _) =>
        {
            Assert.True(check);
            Assert.Equal(-1, position);
            wads.Add(file);
        };
        return slice;
    }
}
