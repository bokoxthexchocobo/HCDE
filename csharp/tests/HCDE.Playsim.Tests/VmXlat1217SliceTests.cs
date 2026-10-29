namespace HCDE.Playsim.Tests;

public class VmXlat1217SliceTests
{
    public VmXlat1217SliceTests()
    {
        VmXlat1217Slice.Reset();
    }

    static void Token(string text, bool found, int type)
    {
        int stored = -99;
        bool ok = VmXlat1217Slice.FindToken(text, ref stored);
        Assert.Equal(found, ok);
        Assert.Equal(type, stored);
    }

    [Fact]
    public void FindToken_MatchesTheSortedKeywordsAndLeavesAMissUntouched()
    {
        Token("arg2", true, 25);
        Token("ARG3", true, 26);
        Token("arg4", true, 27);
        Token("Arg5", true, 28);
        Token("bitmask", true, 35);
        Token("clear", true, 36);
        Token("define", true, 14);
        Token("enum", true, 17);
        Token("flags", true, 24);
        Token("INCLUDE", true, 128);
        Token("lineflag", true, 39);
        Token("lineid", true, 21);
        Token("LINEID", true, 21);
        Token("maxlinespecial", true, 31);
        Token("nobitmask", true, 34);
        Token("sector", true, 33);
        Token("tag", true, 21);
        Token("TAG", true, 21);
        Token("line", false, -99);
        Token("zzz", false, -99);
        Token("arg", false, -99);
        Token("arg6", false, -99);
        Token("", false, -99);
        Assert.Throws<NullReferenceException>(() =>
        {
            int type = 7;
            VmXlat1217Slice.FindToken(null, ref type);
        });
    }

    [Fact]
    public void Load_ParsesOncePerNameAndKeepsNoneApartFromEmpty()
    {
        var log = new List<string>();
        int parsers = 0;
        Func<int> alloc = () =>
        {
            log.Add("alloc");
            return ++parsers;
        };
        Action<int, string?> lump = (parser, name) => log.Add("lump:" + parser + ":" + (name ?? "<null>"));
        Action<int> zero = parser => log.Add("zero:" + parser);
        Action<int> free = parser => log.Add("free:" + parser);

        var first = VmXlat1217Slice.Load("Foo", alloc, lump, zero, free);
        var again = VmXlat1217Slice.Load("FOO", alloc, lump, zero, free);
        Assert.Same(first, again);
        Assert.True(first.Loaded);
        Assert.Equal(new[] { "alloc", "lump:1:Foo", "zero:1", "free:1" }, log);

        log.Clear();
        var empty = VmXlat1217Slice.Load("", alloc, lump, zero, free);
        var none = VmXlat1217Slice.Load(null, alloc, lump, zero, free);
        Assert.NotSame(empty, none);
        Assert.NotSame(first, none);
        Assert.Equal(
            new[] { "alloc", "lump:2:", "zero:2", "free:2", "alloc", "lump:3:<null>", "zero:3", "free:3" },
            log);

        log.Clear();
        VmXlat1217Slice.Load(null, alloc, lump, zero, free);
        Assert.Empty(log);

        log.Clear();
        Assert.Throws<InvalidOperationException>(() =>
            VmXlat1217Slice.Load("Boom", alloc, (_, _) => throw new InvalidOperationException("stop"), zero, free));
        var boom = VmXlat1217Slice.Load("boom", alloc, lump, zero, free);
        Assert.True(boom.Loaded);
        Assert.Equal(new[] { "alloc", "alloc", "lump:5:boom", "zero:5", "free:5" }, log);

        VmXlat1217Slice.Reset();
        log.Clear();
        parsers = 0;
        var namedNone = VmXlat1217Slice.Load("NONE", alloc, lump, zero, free);
        var nullAfter = VmXlat1217Slice.Load(null, alloc, lump, zero, free);
        Assert.Same(namedNone, nullAfter);
        Assert.Equal(new[] { "alloc", "lump:1:NONE", "zero:1", "free:1" }, log);
    }
}
