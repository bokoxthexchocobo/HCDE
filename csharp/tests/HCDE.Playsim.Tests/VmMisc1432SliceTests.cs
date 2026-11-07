using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmMisc1432SliceTests
{
    [Fact]
    public void ResponseFilesExpandAndStopAtLimit()
    {
        Assert.Equal(156, VmMisc1432Slice.BandStart);
        Assert.Equal(406, VmMisc1432Slice.BandEnd);
        var parsed = new List<string>();
        int size = VmMisc1432Slice.Parse("a b", parsed, out int count);
        Assert.Equal(2, count);
        Assert.Equal(4, size);
        Assert.Equal(new[] { "a", "b" }, parsed);
        parsed.Clear();
        Assert.Equal(3, VmMisc1432Slice.Parse("\"hi\"", parsed, out count));
        Assert.Equal(1, count);
        Assert.Equal("hi", Assert.Single(parsed));
        parsed.Clear();
        Assert.Equal(4, VmMisc1432Slice.Parse("\"a\\\"b\"", parsed, out count));
        Assert.Equal("a\"b", Assert.Single(parsed));
        parsed.Clear();
        VmMisc1432Slice.Parse("foo\"bar\"", parsed, out count);
        Assert.Equal(new[] { "foo", "bar" }, parsed);
        Assert.Equal(0, VmMisc1432Slice.Parse("   ", null, out count));
        Assert.Equal(0, count);
        Assert.Throws<NullReferenceException>(() => VmMisc1432Slice.Parse(null, null, out _));

        var s = new VmMisc1432Slice.State();
        s.Args.AddRange(new[] { "game", "@resp" });
        s.Files["resp"] = "# c\n  -file foo.wad\n\nkeep # hash\n";
        VmMisc1432Slice.FindResponse(s);
        Assert.Equal(new[] { "game", "-file", "foo.wad", "keep", "#", "hash" }, s.Args);
        Assert.Equal("Found response file resp!\n", s.Lines[0]);
        Assert.Equal("Added 1 response file, now have 6 command-line args:\n", s.Lines[1]);
        Assert.Equal("-file\n", s.Lines[2]);

        var nested = new VmMisc1432Slice.State();
        nested.Args.AddRange(new[] { "game", "@a", "-z" });
        nested.Files["a"] = "@b -y";
        nested.Files["b"] = "-b";
        VmMisc1432Slice.FindResponse(nested);
        Assert.Equal(new[] { "game", "-b", "-y", "-z" }, nested.Args);
        Assert.Contains("Added 2 response files, now have 4 command-line args:\n", nested.Lines);

        var missing = new VmMisc1432Slice.State();
        missing.Args.AddRange(new[] { "game", "@nope" });
        VmMisc1432Slice.FindResponse(missing);
        Assert.Equal(new[] { "game" }, missing.Args);
        Assert.Equal("No such response file (nope)!\n", Assert.Single(missing.Lines));

        var limited = new VmMisc1432Slice.State();
        limited.Args.Add("game");
        for (int n = 0; n < 101; n++)
        {
            limited.Args.Add("@f" + n);
            limited.Files["f" + n] = "-n";
        }
        VmMisc1432Slice.FindResponse(limited);
        Assert.Equal(101, limited.Args.Count);
        Assert.Equal("game", limited.Args[0]);
        Assert.All(limited.Args.Skip(1), arg => Assert.Equal("-n", arg));
        int hit = limited.Lines.IndexOf("Response file limit of 100 hit.\n");
        int ignored = limited.Lines.IndexOf("Ignored response file f100.\n");
        Assert.True(hit >= 0 && ignored > hit);
    }

    [Fact]
    public void ConfigSaveKeepsRequestedPath()
    {
        var empty = new VmMisc1432Slice.State();
        Assert.True(VmMisc1432Slice.Save(empty, "x.ini"));
        VmMisc1432Slice.WriteIni(empty, new[] { "writeini" });
        Assert.Equal("Config saved.\n", Assert.Single(empty.Lines));
        Assert.Throws<NullReferenceException>(() => VmMisc1432Slice.OpenConfig(empty));

        var s = new VmMisc1432Slice.State
        {
            Config = new VmMisc1432Slice.Config { Path = "old.ini", Game = "Doom" },
        };
        Assert.True(VmMisc1432Slice.Save(s, "new.ini"));
        Assert.Equal("old.ini", Assert.Single(s.SavedPaths));
        Assert.Equal("new.ini", s.Config!.Path);
        Assert.Equal(new[] { "global", "game", "write" }, s.Config.Ops);
        s.Config.Game = "";
        s.Config.Ops.Clear();
        s.Config.WriteOk = false;
        s.Errno = "Permission denied";
        VmMisc1432Slice.WriteIni(s, new[] { "writeini", "x.ini" });
        Assert.Equal("x.ini", s.Config.Path);
        Assert.Equal("Writing config failed: Permission denied\n", s.Lines[0]);
        Assert.Equal(new[] { "global", "fail" }, s.Config.Ops);
        s.IniRetries.Add(true);
        VmMisc1432Slice.SaveFinal(s);
        Assert.Null(s.Config);
        Assert.Equal(new[] { "x.ini", "x.ini" }, s.FailedPaths);

        var opened = new VmMisc1432Slice.State
        {
            Config = new VmMisc1432Slice.Config { Path = "cfg/hcde.ini", Game = "Doom" },
        };
        VmMisc1432Slice.OpenConfig(opened);
        Assert.Equal("cfg/", opened.Opened);
        VmMisc1432Slice.Load(opened);
        Assert.Equal(new[] { "setup" }, opened.Config!.Ops);
        var bare = new VmMisc1432Slice.State { Config = new VmMisc1432Slice.Config { Path = "hcde.ini" } };
        VmMisc1432Slice.OpenConfig(bare);
        Assert.Equal("", bare.Opened);
    }
}
