using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsScriptControlTests
{
    [Theory]
    [InlineData(false, 1)]
    [InlineData(false, 2)]
    [InlineData(false, 3)]
    [InlineData(false, 4)]
    [InlineData(false, 5)]
    [InlineData(true, 1)]
    [InlineData(true, 2)]
    [InlineData(true, 3)]
    [InlineData(true, 4)]
    [InlineData(true, 5)]
    public void DirectAndStackExecuteForwardArgumentsAndScheduleChildNextTick(bool stack, int count)
    {
        var sim = Room(); int[] args = [2, 0, 10, 20, 30]; var words = new List<int>();
        if (stack)
        {
            foreach (var value in args.Take(count)) words.AddRange([3, value]);
            words.AddRange([3 + count, 80]);
        }
        else { words.AddRange([8 + count, 80]); words.AddRange(args.Take(count)); }
        words.Add(1); Add(sim, 1, 0, words.ToArray());
        Add(sim, 2, 3, 3, 7, 28, 0, 28, 1, 14, 28, 2, 14, 5, 112, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.Equal(128, sim.LightOf(0)); Assert.Equal(1, sim.Acs.RunningCount);
        sim.Acs.Tick(sim); Assert.Equal(args.Skip(2).Take(Math.Max(0, count - 2)).Sum(), sim.LightOf(0));
    }

    [Theory]
    [InlineData(263, 80, 999, 0, 0)]
    [InlineData(382, 80, 999, 0, 0)]
    [InlineData(263, 81, 999, 0, 1)]
    [InlineData(382, 82, 999, 0, 1)]
    [InlineData(263, 81, 999, 2, 0)]
    [InlineData(382, 82, 999, 2, 0)]
    public void ResultSpecialReturnsControlSuccessAndPreservesLowerStack(int opcode, int special, int script, int map, int expected)
    {
        var sim = Room(); Add(sim, 1, 0, 3, 7, 3, script, 3, map, 3, 0, 3, 0, 3, 0, opcode, special, 5, 112, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.Equal(expected, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(81)]
    [InlineData(82)]
    public void DirectSelfControlStopsBeforeFollowingMutation(int special)
    {
        var sim = Room(); Add(sim, 1, 0, 10, special, 1, 0, 10, 112, 7, 35, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); Assert.Equal(128, sim.LightOf(0));
        if (special == 81)
        {
            Assert.Equal(1, sim.Acs.SuspendedCount); Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
            Assert.Equal(35, sim.LightOf(0));
        }
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(81)]
    [InlineData(82)]
    public void ControlPreventsLaterScheduledTargetFromExecuting(int special)
    {
        var sim = Room(); Add(sim, 1, 0, 10, special, 2, 0, 1); Add(sim, 2, 0, 10, 112, 7, 35, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); Assert.True(sim.Acs.TryExecute(2, [])); sim.Acs.Tick(sim);
        Assert.Equal(128, sim.LightOf(0)); Assert.Equal(special == 81 ? 1 : 0, sim.Acs.RunningCount);
    }

    [Fact]
    public void DuplicateSelfExecuteReturnsFalseWithoutSpawningAnotherInstance()
    {
        var sim = Room(); Add(sim, 1, 0, 3, 7, 3, 1, 3, 0, 3, 0, 3, 0, 3, 0, 263, 80, 5, 112, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.Equal(0, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void TruncatedDirectControlCannotSuspendTarget()
    {
        var sim = Room(); Add(sim, 1, 0, 10, 81, 2); Add(sim, 2, 0, 10, 112, 7, 35, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); Assert.True(sim.Acs.TryExecute(2, [])); sim.Acs.Tick(sim);
        Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.SuspendedCount);
    }

    private static void Add(AuthoritySimulation sim, int number, int argumentCount, params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = number, ArgumentCount = argumentCount, Code = bytes });
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Format = MapDataFormat.HexenBinary, Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }] });
}
