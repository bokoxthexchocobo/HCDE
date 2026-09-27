using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsGlobalVariableTests
{
    [Theory]
    [InlineData(181, 7, 3, 3)]
    [InlineData(183, 7, 3, 10)]
    [InlineData(184, 7, 3, 4)]
    [InlineData(185, 7, 3, 21)]
    [InlineData(186, -7, 3, -2)]
    [InlineData(187, -7, 3, -1)]
    [InlineData(183, int.MaxValue, 1, int.MinValue)]
    [InlineData(186, int.MinValue, -1, int.MinValue)]
    [InlineData(187, int.MinValue, -1, 0)]
    public void ArithmeticUpdatesSharedSignedValue(int opcode, int initial, int operand, int expected)
    {
        var sim = Room(); Run(sim, 3, initial, 181, 63, 3, operand, opcode, 63, 1);
        AssertGlobal(sim, expected);
    }

    [Fact]
    public void IncrementDecrementNeedNoStackAndNewSimulationStartsAtZero()
    {
        var sim = Room(); Run(sim, 188, 63, 188, 63, 189, 63, 1); AssertGlobal(sim, 1);
        AssertGlobal(Room(), 0);
    }

    [Theory]
    [InlineData(186)]
    [InlineData(187)]
    public void ZeroDivisorStopsScriptWithoutMutatingGlobal(int opcode)
    {
        var sim = Room(); Run(sim, 3, 7, 181, 63, 3, 0, opcode, 63, 188, 63, 1);
        AssertGlobal(sim, 7);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(64)]
    [InlineData(int.MaxValue)]
    public void InvalidIndexStopsBeforeLaterMutation(int index)
    {
        var sim = Room(); Run(sim, 3, 7, 181, index, 188, 63, 1); AssertGlobal(sim, 0);
    }

    [Fact]
    public void MissingStackValueDoesNotOverwriteExistingGlobal()
    {
        var sim = Room(); Run(sim, 3, 7, 181, 63, 1);
        Run(sim, 181, 63, 188, 63, 1); AssertGlobal(sim, 7);
    }

    [Fact]
    public void GlobalValuesParticipateInSimulationChecksum()
    {
        var a = Room(); var b = Room(); Run(a, 3, 7, 181, 63, 1);
        a.Tick(); b.Tick(); Assert.NotEqual(a.Checksum, b.Checksum);
    }

    private static void AssertGlobal(AuthoritySimulation sim, int expected)
    {
        // Compare full signed value inside ACS; a mismatch applies light 35, equality skips it.
        Run(sim, 182, 63, 3, expected, 15, 53, 32, 1, 10, 112, 7, 35, 1);
        Assert.Equal(128, sim.LightOf(0));
    }

    private static void Run(AuthoritySimulation sim, params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.Enqueue(1)); sim.Acs.Tick(sim); Assert.Equal(0, sim.Acs.RunningCount);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
    });
}
