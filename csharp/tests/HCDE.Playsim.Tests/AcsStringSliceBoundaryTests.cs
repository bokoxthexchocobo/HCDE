using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class AcsStringSliceBoundaryTests
{
    [Theory]
    [InlineData(AcsCallFunctions.StrLeft, -1, "abcdef")]
    [InlineData(AcsCallFunctions.StrRight, int.MinValue, "abcdef")]
    [InlineData(AcsCallFunctions.StrLeft, 0, "")]
    [InlineData(AcsCallFunctions.StrRight, 2, "ef")]
    [InlineData(AcsCallFunctions.StrLeft, int.MaxValue, "abcdef")]
    public void EdgeLengthsFollowNativeUnsignedClamping(int function, int length, string expected)
    {
        foreach (var pooled in new[] { false, true }) Assert.Equal(expected, Slice(function, 0, length, pooled));
    }

    [Theory]
    [InlineData(-1, 2, "")]
    [InlineData(int.MinValue, -1, "")]
    [InlineData(6, 2, "")]
    [InlineData(2, -1, "cdef")]
    [InlineData(2, int.MaxValue, "cdef")]
    [InlineData(2, 2, "cd")]
    [InlineData(2, 0, "")]
    public void MidPositionAndLengthHandleNegativeAndOverflowBounds(int position, int length, string expected)
    {
        foreach (var pooled in new[] { false, true }) Assert.Equal(expected, Slice(AcsCallFunctions.StrMid, position, length, pooled));
    }

    private static string Slice(int function, int position, int length, bool pooled)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
        var globals = new AcsGlobalStrings(); var id = pooled ? globals.Add("abcdef") : 0;
        var stack = new List<int> { 77, id }; if (function == AcsCallFunctions.StrMid) stack.Add(position); stack.Add(length);
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new(), ["abcdef"], globals,
            function, function == AcsCallFunctions.StrMid ? 3 : 2, out var result));
        Assert.Equal(new[] { 77 }, stack); Assert.True(AcsStringIds.IsGlobalPool(result)); return globals.Get(result);
    }
}
