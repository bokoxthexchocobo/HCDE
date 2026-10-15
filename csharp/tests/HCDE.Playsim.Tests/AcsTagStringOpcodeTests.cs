using System.Buffers.Binary;
using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class AcsTagStringOpcodeTests
{
    [Theory]
    [InlineData("", 0)]
    [InlineData("abc", 3)]
    [InlineData("\u00e9", 2)]
    [InlineData("\U0001f600", 4)]
    [InlineData("ab\0hidden", 2)]
    public void TaggedStringFeedsLengthAndContinuesScript(string text, int expected)
        => Run(text, [(int)AcsPcode.StrLen], expected);

    [Fact]
    public void TaggedClassNameFeedsCallFunction()
        => Run("DoomImp", [(int)AcsPcode.CallFunc, 1, AcsCallFunctions.CheckClass], 1);

    [Fact]
    public void MissingOperandStopsBeforeLaterActions()
    {
        var sim = Room(); Execute(sim, [(int)AcsPcode.TagString, (int)AcsPcode.Lspec2Direct, 112, 7, 35], []);
        Assert.NotEqual(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    private static void Run(string text, int[] consumer, int expected)
    {
        var sim = Room(); var words = new List<int> { (int)AcsPcode.PushNumber, 0, (int)AcsPcode.TagString };
        words.AddRange(consumer); var prefix = words.Count;
        words.AddRange([(int)AcsPcode.PushNumber, expected, (int)AcsPcode.Eq, (int)AcsPcode.IfNotGoto, 0,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35, (int)AcsPcode.Terminate]);
        words[prefix + 4] = (words.Count - 1) * 4;
        Execute(sim, words.ToArray(), [text]); Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
        Assert.Equal(text.Split('\0')[0], sim.Acs.GlobalStringAt(AcsStringIds.StringPoolMarker));
    }
    private static void Execute(AuthoritySimulation sim, int[] words, string[] strings)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes, StringTable = strings });
        Assert.True(sim.Acs.Enqueue(1, [], sim.Players.Single())); sim.Acs.Tick(sim);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
