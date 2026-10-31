namespace HCDE.Playsim.Tests;

public class VmArgv1336SliceTests
{
    [Fact]
    public void RegisterHelpOrderAndWrapping()
    {
        var cat = new VmArgv1336.Catalog();
        var first = cat.Register("-b", "beta", "Second", "", "", false);
        var file = cat.Register("-File", "alpha", "Load", "PATH", "Loads a file", false);
        var early = cat.Register("-a", "alpha", "A", "", "", false);
        var hidden = cat.Register("-secret", "", "Hidden", "HOW", "", false);
        var shown = cat.Register("-wide", "alpha", "one two three four", "", "", false);

        Assert.Equal("Unknown", hidden.Section);
        Assert.True(hidden.Advanced);
        Assert.Equal("Hidden", hidden.Details);
        Assert.Equal("Loads a file", file.Details);
        Assert.Throws<InvalidOperationException>(() => cat.Register("-File", "alpha", "x", "", "", false));
        Assert.Equal(5, cat.Available.Count);

        cat.PrintHelp(false);
        Assert.Equal(
            new[]
            {
                "\n\x1b[1mBeta options:\x1b[0m\n",
                "\n",
                "\x1b[1m    -b\x1b[0m Second\n",
                "\n\x1b[1mAlpha options:\x1b[0m\n",
                "\n",
                "\x1b[1m -file\x1b[0m Load\n",
                "\x1b[1m    -a\x1b[0m A\n",
                "\x1b[1m -wide\x1b[0m one two three four\n",
            },
            cat.Printed);
        Assert.DoesNotContain("-secret", string.Join("", cat.Printed), StringComparison.Ordinal);

        cat.Printed.Clear();
        cat.TerminalColumns = 20;
        cat.PrintHelp(false);
        Assert.Equal("\x1b[1m -wide\x1b[0m one two\n", cat.Printed[7]);
        Assert.Equal("\x1b[1m      \x1b[0m three four\n", cat.Printed[8]);

        cat.Printed.Clear();
        cat.TerminalColumns = 8;
        cat.PrintHelp(true);
        Assert.Contains("\n\x1b[1m-secret\x1b[0m \x1b[2mHOW\x1b[0m\nHidden\n", cat.Printed);
        Assert.Contains("\n\x1b[1m-File\x1b[0m \x1b[2mPATH\x1b[0m\nLoads a file\n", cat.Printed);
        Assert.Equal("beta", first.Section);
        Assert.Equal("alpha", early.Section);
        Assert.Equal("one two three four", shown.Summary);
    }

    [Fact]
    public void ArgsCopyFlushAndCheckParm()
    {
        var cat = new VmArgv1336.Catalog();
        var arg = cat.Register("-file", "file", "File", "", "", false);
        var raw = new VmArgv1336.Args(2, new[] { "-file", "map" });
        Assert.Equal(new[] { "-file", "map" }, raw.Argv);
        raw.SetRawArgs(1, new[] { "only" });
        Assert.Equal(new[] { "only" }, raw.Argv);
        raw.FlushArgs();
        Assert.Empty(raw.Argv);

        var source = new VmArgv1336.Args(cat, 0, null);
        source.SetRawArgs(2, new[] { "a", "b" });
        var copy = new VmArgv1336.Args(source);
        source.Argv.Add("c");
        Assert.Equal(new[] { "a", "b" }, copy.Argv);
        var assigned = new VmArgv1336.Args();
        assigned.Assign(source);
        Assert.Equal(new[] { "a", "b", "c" }, assigned.Argv);
        assigned.Assign(assigned);
        Assert.Equal(new[] { "a", "b", "c" }, assigned.Argv);

        var appended = new VmArgv1336.Args(cat, 2, new[] { "x", "y" });
        Assert.Equal(new[] { "x", "y" }, appended.Argv);
        var skipped = new VmArgv1336.Args(cat, 0, new[] { "nope" });
        Assert.Empty(skipped.Argv);
        var missing = new VmArgv1336.Args(cat, 2, null);
        Assert.Empty(missing.Argv);

        IReadOnlyList<VmArgv1336.Arg>? seen = null;
        var startSeen = -1;
        cat.CheckParmMany = (args, checks, start) =>
        {
            seen = checks;
            startSeen = start;
            Assert.Same(appended, args);
            return 4;
        };
        Assert.Equal(4, appended.CheckParm(arg, 2));
        Assert.Equal(2, startSeen);
        Assert.NotNull(seen);
        Assert.Single(seen!);
        Assert.Same(arg, seen![0]);
        Assert.Equal(4, appended.CheckParm(arg));
        Assert.Equal(1, startSeen);
    }
}
