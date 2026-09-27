using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsStairTests
{
    [Theory]
    [InlineData(false, 217)]
    [InlineData(true, 217)]
    [InlineData(false, 270)]
    [InlineData(true, 270)]
    [InlineData(false, 271)]
    [InlineData(true, 271)]
    [InlineData(false, 272)]
    [InlineData(true, 272)]
    [InlineData(false, 273)]
    [InlineData(true, 273)]
    public void ScriptStairCallsForwardResetAndHoldTagWait(bool stack, int special)
    {
        var sim = Room(); bool sync = special is 271 or 272; var words = new List<int>();
        int[] args = [7, 8, 2, sync ? 6 : 1, sync ? 0 : 6];
        if (stack) { foreach (var arg in args) words.AddRange([3, arg]); words.AddRange([8, special]); }
        else { words.AddRange([13, special]); words.AddRange(args); }
        words.AddRange([62, 7, 10, 112, 7, 35, 1]); Add(sim, words.ToArray());
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        sim.Tick(); Assert.Equal(special is 270 or 272 ? -1 : 1, sim.FloorOf(0));
        sim.Tick(); Assert.Equal(special is 270 or 272 ? -2 : 2, sim.FloorOf(0));
        for (var i = 0; i < 3; i++) sim.Tick(); Assert.Equal(128, sim.LightOf(0));
        sim.Tick(); Assert.Equal(128, sim.LightOf(0));
        sim.Tick();
        if (!sync) { Assert.Equal(128, sim.LightOf(0)); sim.Tick(); }
        Assert.Equal(0, sim.FloorOf(0)); Assert.Equal(35, sim.LightOf(0));
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(263, 7, 8, 1)]
    [InlineData(382, 7, 8, 1)]
    [InlineData(263, 999, 8, 0)]
    [InlineData(382, 0, 8, 0)]
    [InlineData(263, 7, 0, 0)]
    public void ResultFormsReportActivationOutcome(int opcode, int tag, int speed, int expected)
    {
        var sim = Room(); Add(sim, 3, 7, 3, tag, 3, speed, 3, 2, 3, 0, 3, 0, opcode, 217, 5, 112, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); Assert.Equal(expected, sim.LightOf(0));
        sim.Tick(); Assert.Equal(expected, sim.FloorOf(0));
    }

    [Fact]
    public void TruncatedDirectStairCallCannotPartiallyStartChain()
    {
        var sim = Room(); Add(sim, 13, 217, 7, 8, 2, 0);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); sim.Tick();
        Assert.Equal(0, sim.FloorOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    private static void Add(AuthoritySimulation sim, params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Format = MapDataFormat.HexenBinary, Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128, LightLevel = 128 }] });
}
