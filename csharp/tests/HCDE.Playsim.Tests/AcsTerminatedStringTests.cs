using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class AcsTerminatedStringTests
{
    [Theory]
    [InlineData(0, 97)]
    [InlineData(1, 98)]
    [InlineData(2, 0)]
    [InlineData(3, 0)]
    public void GetCharDoesNotReadBeyondNativeTerminator(int index, int expected)
    {
        foreach (var pooled in new[] { false, true })
        {
            var pool = new AcsGlobalStrings(); var id = pooled ? pool.Add("ab\0hidden") : 0;
            Assert.Equal(expected, Invoke(AcsCallFunctions.GetChar, [id, index], ["ab\0hidden"], pool));
        }
    }

    [Theory]
    [InlineData(AcsCallFunctions.StrLeft)]
    [InlineData(AcsCallFunctions.StrRight)]
    [InlineData(AcsCallFunctions.StrMid)]
    public void SlicesExcludeSuffixPastTerminator(int function)
    {
        var pool = new AcsGlobalStrings(); var id = pool.Add("ab\0hidden");
        int[] args = function == AcsCallFunctions.StrMid ? [id, 1, 99] : [id, 99];
        var result = Invoke(function, args, [], pool);
        Assert.Equal(function == AcsCallFunctions.StrMid ? "b" : "ab", pool.Get(result));
    }

    [Theory]
    [InlineData(AcsCallFunctions.StrCmp)]
    [InlineData(AcsCallFunctions.StrICmp)]
    public void ComparisonsIgnoreSuffixAfterTerminator(int function)
    {
        var pool = new AcsGlobalStrings(); var left = pool.Add("ab\0suffix"); var right = pool.Add("ab");
        Assert.Equal(0, Invoke(function, [left, right], [], pool));
        Assert.Equal(0, Invoke(function, [left, right, 99], [], pool));
    }

    [Fact]
    public void LeadingTerminatorProducesEmptySlice()
    {
        var pool = new AcsGlobalStrings(); var id = pool.Add("\0hidden");
        var result = Invoke(AcsCallFunctions.StrRight, [id, 99], [], pool); Assert.Equal("", pool.Get(result));
    }

    [Fact]
    public void LookupDoesNotModifyStoredPoolEntry()
    {
        var pool = new AcsGlobalStrings(); var id = pool.Add("ab\0hidden");
        Assert.Equal("ab", AcsStringIds.LookupTerminated(id, [], pool)); Assert.Equal("ab\0hidden", pool.Get(id));
    }

    private static int Invoke(int function, int[] args, string[] module, AcsGlobalStrings pool)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
        var stack = new List<int> { 77 }; stack.AddRange(args);
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new(), module, pool, function, args.Length, out var result));
        Assert.Equal(new[] { 77 }, stack); return result;
    }
}
