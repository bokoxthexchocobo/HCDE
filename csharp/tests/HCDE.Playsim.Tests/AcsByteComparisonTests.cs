using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class AcsByteComparisonTests
{
    [Theory]
    [InlineData("\U00010000", "\ue000", false, -1, 1)]
    [InlineData("\u00e9", "\u00ea", false, 1, 0)]
    [InlineData("\u00e9", "\u00ea", false, 2, -1)]
    [InlineData("\u00e4", "\u00c4", true, -1, 1)]
    [InlineData("ABC", "abc", true, -1, 0)]
    [InlineData("ABC", "abc", false, -1, -1)]
    [InlineData("abc", "abcdef", false, 3, 0)]
    [InlineData("abc", "abcdef", false, 4, -1)]
    [InlineData("abc", "xyz", false, 0, 0)]
    [InlineData("abc\0hidden", "abc", true, -1, 0)]
    public void ComparisonsUseByteOrderingLimitsAndAsciiCaseFolding(string left, string right, bool ignoreCase, int length, int sign)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
        var pool = new AcsGlobalStrings();
        foreach (var pooled in new[] { false, true })
        {
            var first = pooled ? pool.Add(left) : 0; var second = pooled ? pool.Add(right) : 1;
            var stack = new List<int> { 77, first, second, length };
            Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new(), [left, right], pool,
                ignoreCase ? AcsCallFunctions.StrICmp : AcsCallFunctions.StrCmp, 3, out var result));
            Assert.Equal(sign, Math.Sign(result)); Assert.Equal(new[] { 77 }, stack);
        }
    }
}
