using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class AcsGetCharByteTests
{
    [Theory]
    [InlineData("A\u00e9Z", 0, 65)]
    [InlineData("A\u00e9Z", 1, -61)]
    [InlineData("A\u00e9Z", 2, -87)]
    [InlineData("A\u00e9Z", 3, 90)]
    [InlineData("A\u00e9Z", 4, 0)]
    [InlineData("\U0001f600", 0, -16)]
    [InlineData("\U0001f600", 3, -128)]
    [InlineData("\u00e9\0Z", 2, 0)]
    [InlineData("\u00e9", -1, 0)]
    public void GetCharReturnsNativeSignedUtf8Byte(string text, int index, int expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
        var pool = new AcsGlobalStrings();
        foreach (var pooled in new[] { false, true })
        {
            var id = pooled ? pool.Add(text) : 0; var stack = new List<int> { 77, id, index };
            Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new(), [text], pool,
                AcsCallFunctions.GetChar, 2, out var result));
            Assert.Equal(expected, result); Assert.Equal(new[] { 77 }, stack);
        }
    }
}
