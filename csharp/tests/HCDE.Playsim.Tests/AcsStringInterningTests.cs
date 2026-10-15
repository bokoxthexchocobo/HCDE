using System.Buffers.Binary;
using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class AcsStringInterningTests
{
    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("\u00e9")]
    [InlineData("\U0001f600")]
    [InlineData("ab\0suffix")]
    public void IdenticalStoredContentsReuseGlobalId(string text)
    {
        var pool = new AcsGlobalStrings(); var first = pool.Add(text);
        Assert.Equal(first, pool.Add(new string(text.ToCharArray()))); Assert.Equal(text, pool.Get(first));
        Assert.Equal(AcsStringIds.FromGlobalIndex(1), pool.Add("different"));
    }

    [Fact]
    public void InterningIsCaseSensitive()
    {
        var pool = new AcsGlobalStrings(); var lower = pool.Add("abc"); var upper = pool.Add("ABC");
        Assert.NotEqual(lower, upper); Assert.Equal(lower, pool.Add("abc")); Assert.Equal(upper, pool.Add("ABC"));
    }

    [Fact]
    public void ExistingIdsRemainStableAfterMoreInsertions()
    {
        var pool = new AcsGlobalStrings(); var first = pool.Add("first");
        for (var i = 0; i < 100; i++) pool.Add(i.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Assert.Equal(first, pool.Add("first")); Assert.Equal("first", pool.Get(first));
    }

    [Fact]
    public void RepeatedGetterIdsCompareEqualInsideScript()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
        int[] words = [(int)AcsPcode.PushNumber, 0, (int)AcsPcode.CallFunc, 1, AcsCallFunctions.GetActorClass,
            (int)AcsPcode.PushNumber, 0, (int)AcsPcode.CallFunc, 1, AcsCallFunctions.GetActorClass,
            (int)AcsPcode.Eq, (int)AcsPcode.IfNotGoto, 68,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35, (int)AcsPcode.Terminate];
        var code = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(code.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = code });
        Assert.True(sim.Acs.Enqueue(1, [], sim.Players.Single())); sim.Acs.Tick(sim);
        Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }
}
