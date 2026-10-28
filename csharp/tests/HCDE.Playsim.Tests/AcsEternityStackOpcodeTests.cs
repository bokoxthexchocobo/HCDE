using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsEternityStackOpcodeTests
{
    [Fact]
    public void SaveString_PushesGlobalPoolIdFromPrintWork()
    {
        var sim = Room();
        var words = new List<int>
        {
            (int)AcsPcode.BeginPrint,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PrintString,
            (int)AcsPcode.SaveString,
            (int)AcsPcode.StrLen,
        };
        var check = words.Count;
        words.AddRange([(int)AcsPcode.PushNumber, 5, (int)AcsPcode.Eq, (int)AcsPcode.IfNotGoto, 0,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35, (int)AcsPcode.Terminate]);
        words[check + 4] = (words.Count - 1) * 4;
        Execute(sim, words.ToArray(), ["hello"]);
        Assert.Equal(35, sim.LightOf(0));
        Assert.Equal("hello", sim.Acs.GlobalStringAt(AcsStringIds.StringPoolMarker));
    }

    [Fact]
    public void PushFunction_PushesFunctionIndexByte()
    {
        var sim = Room();
        var words = new List<int>
        {
            (int)AcsPcode.PushFunction, 42,
            (int)AcsPcode.PushNumber, 42,
            (int)AcsPcode.Eq,
        };
        var branch = words.Count;
        words.AddRange([(int)AcsPcode.IfNotGoto, 0, (int)AcsPcode.Lspec2Direct, 112, 7, 35, (int)AcsPcode.Terminate]);
        words[branch + 1] = (words.Count - 1) * 4;
        Execute(sim, words.ToArray(), []);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallStack_InvokesZeroArgInvasionQueryFromStackTag()
    {
        var sim = Room();
        sim.Invasion.Enabled = true;
        sim.Invasion.Configure(3, 0, 1);
        sim.Invasion.Tick(sim);
        Execute(sim,
        [
            (int)AcsPcode.PushNumber, AcsCallFunctions.InvasionGetWave,
            (int)AcsPcode.CallStack,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 0,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35,
            (int)AcsPcode.Terminate,
        ], []);
        Assert.Equal(35, sim.LightOf(0));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128, LightLevel = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });

    private static void Execute(AuthoritySimulation sim, int[] words, string[] strings)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes, StringTable = strings });
        Assert.True(sim.Acs.TryExecute(1, [], sim.Players.Single()));
        sim.Acs.Tick(sim);
        Assert.Equal(0, sim.Acs.RunningCount);
    }

}
