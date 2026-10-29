namespace HCDE.Playsim.Tests;

public class VmCommandLine1193SliceTests
{
    [Fact]
    public void Parse_ExpandsQuotesEscapesAndCvars()
    {
        VmCommandLine1193Slice.Clear();
        Assert.Equal('\u001c', VmCommandLine1193Slice.TextColorEscape);
        var calls = new List<string>();
        VmCommandLine1193Slice.FindCVar = name =>
        {
            calls.Add(name);
            if (name == "map")
                return "e1m1 lobby";
            if (name == "")
                return "empty-name";
            return null;
        };
        var line = VmCommandLine1193Slice.Create("  echo \"a\\\"b\\\\c\\c\" $map $missing $\"hi\" tail\t", false);
        Assert.Equal("  echo \"a\\\"b\\\\c\\c\" $map $missing $\"hi\" tail\t", line.Args());
        Assert.Equal(-1, line.ArgcValue);
        Assert.Equal(7, line.Argc());
        Assert.Equal(7, line.Argc());
        Assert.Equal(new[] { "map", "missing", "" }, calls);
        Assert.Equal("echo", line[0]);
        Assert.Equal("a\"b\\c\u001c", line[1]);
        Assert.Equal("e1m1 lobby", line[2]);
        Assert.Equal("$missing", line[3]);
        Assert.Equal("empty-name", line[4]);
        Assert.Equal("hi", line[5]);
        Assert.Equal("tail", line[6]);
        var expected = "echo".Length + 1 + "a\"b\\c\u001c".Length + 1 + "e1m1 lobby".Length + 1
            + "$missing".Length + 1 + "empty-name".Length + 1 + "hi".Length + 1 + "tail".Length + 1;
        Assert.Equal(expected, line.ArgSize);

        var literal = VmCommandLine1193Slice.Create("\"a\\\"b\"", true);
        Assert.Equal(3, literal.Argc());
        Assert.Equal("a\\", literal[0]);
        Assert.Equal("b", literal[1]);
        Assert.Equal("", literal[2]);

        var high = VmCommandLine1193Slice.Create("a\u00e9b \"\u00e9\"", false);
        Assert.Equal(new[] { "a", "b", "\u00e9" }, new[] { high[0], high[1], high[2] });
        Assert.Equal(3, high.Argc());
    }

    [Fact]
    public void Shift_DuplicatesTheTailUntilTheVectorIsRebuilt()
    {
        VmCommandLine1193Slice.Clear();
        var line = VmCommandLine1193Slice.Create("a b c", false);
        line.Shift();
        Assert.Null(line.Argv);
        Assert.Equal(-1, line.ArgcValue);
        Assert.Equal("a", line[0]);
        Assert.Equal("b", line[1]);
        Assert.Equal("c", line[2]);
        Assert.Equal(6, line.ArgSize);
        Assert.Equal(3, line.Argc());
        var tail = line.Argv![2];
        line.Shift();
        Assert.Equal(3, line.Argc());
        Assert.Equal("b", line[0]);
        Assert.Equal("c", line[1]);
        Assert.Same(tail, line.Argv[1]);
        Assert.Same(tail, line[2]);
        line.Shift();
        Assert.Equal("c", line[0]);
        Assert.Same(tail, line[2]);
        line.Destroy();
        Assert.Null(line.Argv);
        Assert.Equal("a", line[0]);
        Assert.Equal("b", line[1]);
        Assert.Equal("c", line[2]);
        Assert.Equal(3, line.Argc());
    }
}
