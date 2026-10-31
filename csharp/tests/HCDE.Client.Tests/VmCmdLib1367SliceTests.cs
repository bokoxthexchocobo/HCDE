namespace HCDE.Client.Tests;

public class VmCmdLib1367SliceTests
{
    [Fact]
    public void StrBin1Escapes()
    {
        var slice = new VmCmdLib1367Slice();
        Assert.Throws<InvalidOperationException>(() => slice.StrBin1(null));
        Assert.Equal("", slice.StrBin1(""));
        Assert.Equal("abc", slice.StrBin1("abc"));
        Assert.Equal("", slice.StrBin1("\\"));
        Assert.Equal("\\", slice.StrBin1("\\\\"));
        Assert.Equal("\n", slice.StrBin1("\\n"));
        Assert.Equal("q", slice.StrBin1("\\q"));
        Assert.Equal("\u001c", slice.StrBin1("\\c"));
        Assert.Equal("?", slice.StrBin1("\\?"));
        Assert.Equal("", slice.StrBin1("\\\n"));
        Assert.Equal("\0", slice.StrBin1("\\x"));
        Assert.Equal("\u0004", slice.StrBin1("\\x4"));
        Assert.Equal("\u0004G", slice.StrBin1("\\x4G"));
        Assert.Equal("\0GG", slice.StrBin1("\\xGG"));
        Assert.Equal("\u00ff", slice.StrBin1("\\xFF"));
        Assert.Equal("\n", slice.StrBin1("\\X0a"));
        Assert.Equal("\u00123", slice.StrBin1("\\x123"));
        Assert.Equal("\0", slice.StrBin1("\\0"));
        Assert.Equal("\u0007", slice.StrBin1("\\7"));
        Assert.Equal("\u00078", slice.StrBin1("\\78"));
        Assert.Equal("\n", slice.StrBin1("\\12"));
        Assert.Equal("S", slice.StrBin1("\\123"));
        Assert.Equal("S4", slice.StrBin1("\\1234"));
        Assert.Equal("\u00ff", slice.StrBin1("\\377"));
        Assert.Equal("\0", slice.StrBin1("\\400"));
        Assert.Equal("\u00ff", slice.StrBin1("\\777"));
        Assert.Equal("a\nb", slice.StrBin1("a\\nb"));
        Assert.Equal("say \"hi\"", slice.StrBin1("say \\\"hi\\\""));
        Assert.Equal("linecont", slice.StrBin1("line\\\ncont"));
        Assert.Equal("\a\b\f\t\r\v", slice.StrBin1("\\a\\b\\f\\t\\r\\v"));
        Assert.Equal("ab", slice.StrBin1("ab\0\\n"));
        Assert.Equal(1, slice.StrBin1("\\x").Length);
        Assert.Equal(1, slice.StrBin1("\\400").Length);
    }

    [Fact]
    public void ExpandEnvAndNicePath()
    {
        var slice = new VmCmdLib1367Slice { ProgDir = "DIR/" };
        var env = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["FOO"] = "bar",
            ["EMPTY"] = "",
            ["ZZ"] = "z",
            ["NIL"] = null,
            ["CUT"] = "ab\0cd",
        };
        slice.GetEnv = name => env.TryGetValue(name, out string? value) ? value : null;
        Assert.Equal("", slice.ExpandEnv(null));
        Assert.Equal("plain", slice.ExpandEnv("plain"));
        Assert.Equal("abar/b", slice.ExpandEnv("a$FOO/b"));
        Assert.Equal("", slice.ExpandEnv("$EMPTY"));
        Assert.Equal("", slice.ExpandEnv("$MISSING"));
        Assert.Equal("", slice.ExpandEnv("$NIL"));
        Assert.Equal("ab", slice.ExpandEnv("$CUT"));
        slice.Trace.Clear();
        Assert.Equal("DIR/", slice.ExpandEnv("$progdir"));
        Assert.Equal("DIR/", slice.ExpandEnv("$PROGDIR"));
        Assert.Equal("DIR/", slice.ExpandEnv("$ProgDir"));
        Assert.Empty(slice.Trace);
        Assert.Equal("$$", slice.ExpandEnv("$$"));
        Assert.Equal("$", slice.ExpandEnv("$"));
        Assert.Equal("a$", slice.ExpandEnv("a$"));
        Assert.Equal("${FOO}", slice.ExpandEnv("${FOO}"));
        Assert.Equal("bar-x", slice.ExpandEnv("$FOO-x"));
        Assert.Equal("prebarz post", slice.ExpandEnv("pre$FOO$ZZ post"));
        Assert.Equal("", slice.ExpandEnv("$1A"));
        Assert.Equal("$ FOO", slice.ExpandEnv("$ FOO"));
        Assert.Equal("", slice.ExpandEnv("$progdir2"));
        Assert.Equal("xbar$", slice.ExpandEnv("x$FOO$"));
        slice.ProgDir = "";
        Assert.Equal("/tail", slice.ExpandEnv("$progdir/tail"));
        slice.ProgDir = "DIR/";
        var bare = new VmCmdLib1367Slice();
        Assert.Throws<InvalidOperationException>(() => bare.ExpandEnv("$FOO"));
        Assert.Equal("keep", bare.ExpandEnv("keep"));

        Assert.Equal("", slice.NicePath(null));
        Assert.Equal("", slice.NicePath(""));
        Assert.Equal("abar", slice.NicePath("a$FOO"));
        slice.GetUid = () => 4;
        int seenUid = -1;
        slice.GetPwUid = uid =>
        {
            seenUid = uid;
            return new VmCmdLib1367Slice.Passwd { Dir = "/home/me" };
        };
        slice.Trace.Clear();
        Assert.Equal("/home/me", slice.NicePath("~"));
        Assert.Equal(4, seenUid);
        Assert.Equal(new[] { "getuid", "getpwuid" }, slice.Trace);
        slice.Trace.Clear();
        Assert.Equal("/home/me/x", slice.NicePath("~/x"));
        Assert.Equal("/home/me/", slice.NicePath("~/"));
        Assert.Equal("/home/me/abar", slice.NicePath("~/a$FOO"));
        Assert.DoesNotContain("getpwnam", slice.Trace);
        slice.GetPwNam = who => who == "~bob" ? new VmCmdLib1367Slice.Passwd { Dir = "/home/bob" } : null;
        Assert.Equal("/home/bob", slice.NicePath("~bob"));
        Assert.Equal("~bob", slice.Who);
        Assert.Equal("/home/bob/abar", slice.NicePath("~bob/a$FOO"));
        Assert.Equal("~ghost", slice.NicePath("~ghost"));
        slice.GetPwUid = _ => new VmCmdLib1367Slice.Passwd { Dir = null };
        Assert.Equal("/x", slice.NicePath("~/x"));
        slice.GetPwUid = _ => null;
        Assert.Equal("~", slice.NicePath("~"));
        var noUid = new VmCmdLib1367Slice();
        var uidMiss = Assert.Throws<InvalidOperationException>(() => noUid.NicePath("~"));
        Assert.Equal("getuid", uidMiss.Message);
        noUid.GetUid = () => 1;
        var pwMiss = Assert.Throws<InvalidOperationException>(() => noUid.NicePath("~"));
        Assert.Equal("getpwuid", pwMiss.Message);
        Assert.Equal(new[] { "getuid" }, noUid.Trace);
        var noNam = new VmCmdLib1367Slice();
        var namMiss = Assert.Throws<InvalidOperationException>(() => noNam.NicePath("~bob"));
        Assert.Equal("getpwnam", namMiss.Message);
        Assert.Equal("~bob", noNam.Who);
        slice.Trace.Clear();
        slice.GetPwNam = _ => null;
        Assert.Equal("~$", slice.NicePath("~$"));
        Assert.Contains("getpwnam", slice.Trace);
        Assert.DoesNotContain("getuid", slice.Trace);

        var win = new VmCmdLib1367Slice { Windows = true, ProgDir = "DIR/", GetEnv = slice.GetEnv };
        Assert.Throws<InvalidOperationException>(() => win.NicePath(null));
        Assert.Equal(".", win.NicePath(""));
        Assert.Equal("~", win.NicePath("~"));
        win.ProgDir = "DIR";
        Assert.Equal("DIR/x", win.NicePath("$progdir/x"));
        Assert.Empty(win.Trace);
        Assert.Equal("abar", win.NicePath("a$FOO"));
    }
}
