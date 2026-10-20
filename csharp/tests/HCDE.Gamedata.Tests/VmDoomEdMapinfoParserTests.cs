namespace HCDE.Gamedata.Tests;

public class VmDoomEdMapinfoParserTests
{
    [Fact]
    public void MatchSpecialMapthing_ResolvesPlayerStart()
    {
        Assert.Equal(1, VmDoomEdMapinfoParser.MatchSpecialMapthing("$Player1Start"));
        Assert.Equal(-1, VmDoomEdMapinfoParser.MatchSpecialMapthing("DoomImp"));
    }

    [Fact]
    public void TryParseDoomEdNums_ReadsClassAndSpecial()
    {
        var tokens = VmLockDefTokenize.Tokenize("{ 3001 = DoomImp 10 = $DeathmatchStart , noskillflags , 1 }");
        var i = 0;
        var map = new Dictionary<int, VmMapinfoEdMapItem>();
        var errors = new List<string>();
        Assert.True(VmDoomEdMapinfoParser.TryParseDoomEdNums(tokens, ref i, map, null, errors));
        Assert.Equal("DoomImp", map[3001].ClassName);
        Assert.Equal(9, map[10].Special);
        Assert.True(map[10].NoSkillFlags);
        Assert.Equal(1, map[10].Args[0]);
    }

    [Fact]
    public void EnumerateSortedForDump_OrdersByEdNum()
    {
        var map = new Dictionary<int, VmMapinfoEdMapItem>
        {
            [3003] = new() { ClassName = "Baron" },
            [3001] = new() { ClassName = "Imp" }
        };
        var lines = VmDoomEdMapinfoParser.EnumerateSortedForDump(map).ToList();
        Assert.Equal(3001, lines[0].EdNum);
        Assert.Equal("Imp", lines[0].Label);
    }

    [Fact]
    public void TryParseDoomEdNums_DetectsDuplicate()
    {
        var tokens = VmLockDefTokenize.Tokenize("{ 1 = A 1 = B }");
        var i = 0;
        var map = new Dictionary<int, VmMapinfoEdMapItem>();
        var errors = new List<string>();
        Assert.False(VmDoomEdMapinfoParser.TryParseDoomEdNums(tokens, ref i, map, null, errors));
        Assert.Contains(errors, e => e.Contains("more than once"));
    }
}
