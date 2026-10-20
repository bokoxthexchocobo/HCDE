namespace HCDE.Client.Tests;

public class VmStringTableFinishTests
{
    private sealed class FakeLumps : IVmStringTableLumpSource
    {
        private readonly Dictionary<string, List<VmStringTableLump>> lumps = new(StringComparer.OrdinalIgnoreCase);
        public void Add(string name, int fileNum, string text)
        {
            if (!lumps.TryGetValue(name, out var list)) { list = []; lumps[name] = list; }
            list.Add(new VmStringTableLump(list.Count, fileNum, text));
        }
        public IEnumerable<VmStringTableLump> EnumerateLumps(string lumpName) =>
            lumps.GetValueOrDefault(lumpName) ?? [];
    }

    [Fact]
    public void TryLoad_SkipsBinaryLump()
    {
        var table = new VmStringTable();
        Assert.False(VmStringTableLoader.TryLoad(table, 1, "bin\0ary", table.LangIds, null));
    }

    [Fact]
    public void InsertString_ExpandsMacrosPerGender()
    {
        var table = new VmStringTable();
        table.RegisterMacro("G", ["a", "b", "c", "d"]);
        table.InsertString(1, table.DefaultTableId, "X", "@[G]");
        table.UpdateLanguage("en-US");
        Assert.Equal("a", table.CheckString("X", out _, 0));
        Assert.Equal("b", table.CheckString("X", out _, 1));
    }

    [Fact]
    public void LoadStrings_SkipsBinaryLanguageLump()
    {
        var fs = new FakeLumps();
        fs.Add("LANGUAGE", 1, "bad\0");
        var table = new VmStringTable();
        table.LoadStrings(fs, "en-US");
        Assert.False(table.Exists("ANY"));
    }
}
