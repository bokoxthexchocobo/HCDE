namespace HCDE.Playsim.Tests;

public class VmStbInclude1314SliceTests
{
    [Fact]
    public void Expand_SplicesTheIncludeAndResumesTheParentLine()
    {
        var files = new Dictionary<string, string>
        {
            ["main"] = "hello\n#include \"a\"\nworld",
            ["a"] = "int x;\n",
            ["b"] = "B",
            ["nested"] = "#include \"b\"\n",
            ["two"] = "A\n#include \"a\"\nB\n#include \"b\"\nC",
        };
        var vm = new VmStbInclude1314(name => files.TryGetValue(name, out string? text) ? text : null);
        var names = new List<string>();
        string error = "stale";
        string got = vm.IncludeFile("main", names, ref error);
        Assert.Equal("", error);
        Assert.Equal(new[] { "main", "a" }, names);
        Assert.Equal(
            "\n#line 1 1 // main\nhello\n\n#line 1 2 // a\nint x;\n\n#line 3 1 // main\n\nworld",
            got);

        names.Clear();
        error = "";
        got = vm.IncludeString("  #include \"b\" junk\nX", "root", names, ref error);
        Assert.Equal("", error);
        Assert.Equal(new[] { "root", "b" }, names);
        Assert.Equal(
            "\n#line 1 1 // root\n\n#line 1 2 // b\nB\n#line 2 1 // root\n\nX",
            got);

        names.Clear();
        got = vm.IncludeString("#include \"unterminated\n#include <keep>\n#include\"b\"\n", "root", names, ref error);
        Assert.Equal(new[] { "root" }, names);
        Assert.Equal(
            "\n#line 1 1 // root\n#include \"unterminated\n#include <keep>\n#include\"b\"\n",
            got);

        names.Clear();
        got = vm.IncludeFile("two", names, ref error);
        Assert.Equal(new[] { "two", "a", "b" }, names);
        Assert.Equal(
            "\n#line 1 1 // two\nA\n\n#line 1 2 // a\nint x;\n\n#line 3 1 // two\n\nB\n\n#line 1 3 // b\nB\n#line 5 1 // two\n\nC",
            got);

        names.Clear();
        got = vm.IncludeString("#include \"nested\"\n", "top", names, ref error);
        Assert.Equal(new[] { "top", "nested", "b" }, names);
        Assert.Equal(
            "\n#line 1 1 // top\n\n#line 1 2 // nested\n\n#line 1 3 // b\nB\n#line 2 2 // nested\n\n\n#line 2 1 // top\n\n",
            got);
    }

    [Fact]
    public void Load_ClearsOnEntryAndAppendsTheMissingName()
    {
        var files = new Dictionary<string, string>
        {
            ["empty"] = "",
            ["plain"] = "abc",
            ["good"] = "ok",
            ["parent"] = "#include \"good\"\n#include \"missing\"\n",
            ["crlf"] = "one\r\n#include \"good\"\r\n",
        };
        var vm = new VmStbInclude1314(name => files.TryGetValue(name, out string? text) ? text : null);
        var names = new List<string>();
        string error = "stale";
        Assert.Equal("\n#line 1 1 // empty\n", vm.IncludeFile("empty", names, ref error));
        Assert.Equal("", error);
        Assert.Equal(new[] { "empty" }, names);

        names.Clear();
        error = "stale";
        Assert.Equal("\n#line 1 1 // plain\nabc", vm.IncludeString("abc", "plain", names, ref error));
        Assert.Equal("", error);

        names.Clear();
        error = "stale";
        Assert.Equal("", vm.IncludeFile("missing", names, ref error));
        Assert.Equal("staleError: couldn't load 'missing'", error);
        Assert.Empty(names);

        names.Clear();
        error = "";
        Assert.Equal("", vm.IncludeFile("parent", names, ref error));
        Assert.Equal("Error: couldn't load 'missing'", error);
        Assert.Equal(new[] { "parent", "good" }, names);

        names.Clear();
        error = "";
        Assert.Equal(
            "\n#line 1 1 // crlf\none\r\n\n#line 1 2 // good\nok\n#line 3 1 // crlf\n\r\n",
            vm.IncludeFile("crlf", names, ref error));
        Assert.Equal("", error);
        Assert.Equal(new[] { "crlf", "good" }, names);
    }
}
