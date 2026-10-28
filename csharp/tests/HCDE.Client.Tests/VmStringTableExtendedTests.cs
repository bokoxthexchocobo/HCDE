namespace HCDE.Client.Tests;

public class VmStringTableExtendedTests
{
    private sealed class FakeLumps : IVmStringTableLumpSource
    {
        private readonly Dictionary<string, List<VmStringTableLump>> lumps = new(StringComparer.OrdinalIgnoreCase);

        public void Add(string name, int fileNum, string text)
        {
            if (!lumps.TryGetValue(name, out var list))
            {
                list = [];
                lumps[name] = list;
            }

            list.Add(new VmStringTableLump(list.Count, fileNum, text));
        }

        public IEnumerable<VmStringTableLump> EnumerateLumps(string lumpName) =>
            lumps.GetValueOrDefault(lumpName) ?? [];
    }

    [Fact]
    public void LoadLanguage_AcceptsQuotedEnUs()
    {
        var table = new VmStringTable();
        table.LoadLanguage(1, """
            [
            "en-US"
            ]
            GREET = hi ;
            """);
        table.UpdateLanguage("en-US");
        Assert.Equal("hi", table.GetString("GREET"));
    }

    [Fact]
    public void LoadLanguage_AcceptsSplitEnUs()
    {
        var table = new VmStringTable();
        table.LoadLanguage(1, """
            [
            en-US
            ]
            GREET = split ;
            """);
        table.UpdateLanguage("en-US");
        Assert.Equal("split", table.GetString("GREET"));
    }

    [Fact]
    public void ParseLanguageCsv_LoadsIdentifierRow()
    {
        var table = new VmStringTable();
        var csv = """
            identifier,default
            HELLO,world
            """;
        Assert.True(VmStringTableLanguageCsv.TryParse(table, 2, csv, table.LangIds, null));
        table.UpdateLanguage("en-US");
        Assert.Equal("world", table.GetString("HELLO"));
    }

    [Fact]
    public void LoadStrings_ReadsLanguageAndMacros()
    {
        var fs = new FakeLumps();
        fs.Add("LMACROS", 1, """
            name,a,b,c,d,e
            PLURAL,.,one,two,three,four
            """);
        fs.Add("LANGUAGE", 1, """
            [default]
            ITEM = @[PLURAL] ;
            """);
        var table = new VmStringTable();
        table.LoadStrings(fs, "en-US");
        Assert.Equal("one", table.GetString("ITEM"));
    }
}
