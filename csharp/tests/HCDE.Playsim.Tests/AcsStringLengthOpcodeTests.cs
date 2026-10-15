using System.Buffers.Binary;
using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class AcsStringLengthOpcodeTests
{
    [Theory]
    [InlineData("", 0)]
    [InlineData("abc", 3)]
    [InlineData("\u00e9", 2)]
    [InlineData("\U0001f600", 4)]
    [InlineData("ab\0hidden", 2)]
    [InlineData("\0hidden", 0)]
    public void OpcodeCountsTerminatedUtf8BytesAndContinuesScript(string text, int expected)
    {
        foreach (var pooled in new[] { false, true }) Run(text, expected, pooled);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(999)]
    [InlineData(AcsStringIds.StringPoolMarker + 999)]
    public void InvalidStringIdsReturnZeroAndContinue(int id) => Run("unused", 0, false, id);

    [Fact]
    public void MissingOperandStopsScriptBeforeLaterActions()
    {
        var sim = Room(); Add(sim, [(int)AcsPcode.StrLen, (int)AcsPcode.Lspec2Direct, 112, 7, 35], []);
        Assert.NotEqual(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    private static void Run(string text, int expected, bool pooled, int id = 0)
    {
        var sim = Room(); var words = new List<int> { (int)AcsPcode.PushNumber, id };
        if (pooled) words.AddRange([(int)AcsPcode.PushNumber, int.MaxValue,
            (int)AcsPcode.CallFunc, 2, AcsCallFunctions.StrLeft]);
        var prefix = words.Count;
        words.AddRange([(int)AcsPcode.StrLen, (int)AcsPcode.PushNumber, expected, (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 0, (int)AcsPcode.Lspec2Direct, 112, 7, 35, (int)AcsPcode.Terminate]);
        words[prefix + 5] = (words.Count - 1) * 4;
        Add(sim, words.ToArray(), [text]); Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    private static void Add(AuthoritySimulation sim, int[] words, string[] strings)
    {
        var code = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(code.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = code, StringTable = strings });
        Assert.True(sim.Acs.Enqueue(1, [], sim.Players.Single())); sim.Acs.Tick(sim);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
