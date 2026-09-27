using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsSharedArrayTests
{
    public static IEnumerable<object[]> Bitwise()
    {
        foreach (var scope in new[] { 0, 1 })
        {
            yield return [296 + scope, scope, unchecked((int)0x80000005), unchecked((int)0x8000000A), int.MinValue];
            yield return [303 + scope, scope, unchecked((int)0x80000005), unchecked((int)0x8000000A), 15];
            yield return [310 + scope, scope, unchecked((int)0x80000005), unchecked((int)0x8000000A), unchecked((int)0x8000000F)];
            yield return [317 + scope, scope, 1, 31, int.MinValue];
            yield return [324 + scope, scope, -9, 1, -5];
        }
    }

    [Theory]
    [MemberData(nameof(Bitwise))]
    public void BitwiseUpdatesPreserveLowerStackAndPersist(int opcode, int scope, int initial, int operand, int expected)
    {
        var start = scope == 0 ? 226 : 235; var bank = scope == 0 ? 255 : 63;
        var sim = Room(); Run(sim, 1, 3, int.MinValue, 3, initial, start + 1, bank,
            3, 7, 3, int.MinValue, 3, operand, opcode, bank, 3, 35, 5, 112, 1);
        Assert.Equal(35, sim.LightOf(0));
        Run(sim, 2, 3, 7, 3, int.MinValue, start, bank, 3, expected, 19, 5, 112, 1);
        Assert.Equal(1, sim.LightOf(0));
    }

    [Theory]
    [MemberData(nameof(Bitwise))]
    public void BitwiseMissingOperandPreservesElement(int opcode, int scope, int initial, int operand, int expected)
    {
        _ = operand; _ = expected;
        var start = scope == 0 ? 226 : 235;
        var sim = Room(); Run(sim, 1, 3, -1, 3, initial, start + 1, 0,
            3, -1, opcode, 0, 10, 112, 7, 35, 1);
        Assert.Equal(128, sim.LightOf(0));
        Run(sim, 2, 3, 7, 3, -1, start, 0, 3, initial, 19, 5, 112, 1);
        Assert.Equal(1, sim.LightOf(0));
    }

    [Theory]
    [MemberData(nameof(Bitwise))]
    public void BitwiseInvalidArrayStopsBeforeFollowingMutation(int opcode, int scope, int initial, int operand, int expected)
    {
        _ = initial; _ = expected;
        var sim = Room(); Run(sim, 1, 3, -1, 3, operand, opcode, scope == 0 ? 256 : 64, 10, 112, 7, 35, 1);
        Assert.Equal(128, sim.LightOf(0));
    }

    [Theory]
    [InlineData(317, 32, 17)]
    [InlineData(318, -1, int.MinValue)]
    [InlineData(324, 32, 17)]
    [InlineData(325, -1, 0)]
    public void ArrayShiftsUseManagedMaskedCountContract(int opcode, int count, int expected)
    {
        var start = opcode is 318 or 325 ? 235 : 226;
        var sim = Room(); Run(sim, 1, 3, 0, 3, 17, start + 1, 0, 3, 0, 3, count, opcode, 0,
            3, 7, 3, 0, start, 0, 3, expected, 19, 5, 112, 1);
        Assert.Equal(1, sim.LightOf(0));
    }

    [Theory]
    [InlineData(296)]
    [InlineData(311)]
    public void TruncatedBitwiseIndexDoesNotMutateElements(int opcode)
    {
        var sim = Room(); Run(sim, 1, 3, 0, 3, 19, 227, 0, 3, 0, 3, 23, 236, 0,
            3, 0, 3, -1, opcode);
        Run(sim, 2, 3, 7, 3, 0, 226, 0, 3, 0, 235, 0, 14, 5, 112, 1);
        Assert.Equal(42, sim.LightOf(0));
    }

    public static IEnumerable<object[]> Arithmetic()
    {
        foreach (var start in new[] { 226, 235 })
        {
            yield return [start, 1, -7, 3, 3];
            yield return [start, 2, -7, 3, -4];
            yield return [start, 3, -7, 3, -10];
            yield return [start, 4, -7, 3, -21];
            yield return [start, 5, -7, 3, -2];
            yield return [start, 6, -7, 3, -1];
            yield return [start, 2, int.MaxValue, 1, int.MinValue];
            yield return [start, 5, int.MinValue, -1, int.MinValue];
        }
    }

    [Theory]
    [MemberData(nameof(Arithmetic))]
    public void ArithmeticPersistsAcrossScriptsAndPreservesLowerStack(int start, int operation, int initial, int operand, int expected)
    {
        var sim = Room(); var bank = start == 226 ? 255 : 63;
        Run(sim, 1, 3, -19, 3, initial, start + 1, bank, 1);
        Run(sim, 2, 3, 7, 3, -19, 3, operand, start + operation, bank,
            3, -19, start, bank, 3, expected, 19, 5, 112, 1);
        Assert.Equal(1, sim.LightOf(0));
    }

    [Theory]
    [InlineData(226, int.MinValue)]
    [InlineData(226, int.MaxValue)]
    [InlineData(235, -1)]
    [InlineData(235, int.MaxValue)]
    public void MissingSignedElementReadsZeroAndIncrementDecrementConsumeOnlyIndex(int start, int key)
    {
        var sim = Room(); Run(sim, 1, 3, 7, 3, key, start, 0, 5, 112, 1);
        Assert.Equal(0, sim.LightOf(0));
        Run(sim, 2, 3, 7, 3, key, start + 7, 0, 3, key, start + 7, 0,
            3, key, start + 8, 0, 3, key, start, 0, 5, 112, 1);
        Assert.Equal(1, sim.LightOf(0));
    }

    [Theory]
    [InlineData(226, -1)]
    [InlineData(226, 256)]
    [InlineData(235, -1)]
    [InlineData(235, 64)]
    public void InvalidArrayIdStopsBeforeFollowingMutation(int start, int bank)
    {
        var sim = Room(); Run(sim, 1, 3, 0, 3, 19, start + 1, bank, 10, 112, 7, 35, 1);
        Assert.Equal(128, sim.LightOf(0));
    }

    [Theory]
    [InlineData(231)]
    [InlineData(232)]
    [InlineData(240)]
    [InlineData(241)]
    public void ZeroDivisorPreservesElementAndStopsScript(int opcode)
    {
        var start = opcode < 235 ? 226 : 235;
        var sim = Room(); Run(sim, 1, 3, 0, 3, 19, start + 1, 0,
            3, 0, 3, 0, opcode, 0, 10, 112, 7, 35, 1);
        Assert.Equal(128, sim.LightOf(0));
        Run(sim, 2, 3, 7, 3, 0, start, 0, 5, 112, 1); Assert.Equal(19, sim.LightOf(0));
    }

    [Theory]
    [InlineData(226)]
    [InlineData(227)]
    [InlineData(233)]
    [InlineData(235)]
    [InlineData(236)]
    [InlineData(243)]
    public void MissingStackArgumentsStopBeforeMutation(int opcode)
    {
        var sim = Room(); Run(sim, 1, opcode, 0, 10, 112, 7, 35, 1);
        Assert.Equal(128, sim.LightOf(0));
    }

    [Theory]
    [InlineData(227)]
    [InlineData(236)]
    public void AssignmentRequiresBothIndexAndValue(int opcode)
    {
        var sim = Room(); Run(sim, 1, 3, 19, opcode, 0, 10, 112, 7, 35, 1);
        Assert.Equal(128, sim.LightOf(0));
    }

    [Fact]
    public void ArrayIdsScopesAndScalarSlotsRemainSeparate()
    {
        var sim = Room(); Run(sim, 1, 3, 0, 3, 2, 227, 0, 3, 0, 3, 4, 227, 1,
            3, 0, 3, 8, 236, 0, 3, 16, 27, 0, 3, 32, 181, 0,
            3, 7, 3, 0, 226, 0, 3, 0, 226, 1, 14, 3, 0, 235, 0, 14, 30, 0, 14, 182, 0, 14, 5, 112, 1);
        Assert.Equal(62, sim.LightOf(0));
    }

    [Fact]
    public void ArrayChecksumUsesValuesRatherThanInsertionOrder()
    {
        var a = Room(); var b = Room();
        Run(a, 1, 3, -1, 3, 19, 227, 0, 3, 2, 3, 20, 227, 0, 1);
        Run(b, 1, 3, 2, 3, 20, 227, 0, 3, -1, 3, 19, 227, 0, 1);
        Run(a, 1, 1); Run(b, 1, 1); Assert.Equal(a.Acs.Checksum, b.Acs.Checksum);
        Run(b, 1, 3, -1, 3, 21, 227, 0, 1); Run(b, 1, 1);
        Assert.NotEqual(a.Acs.Checksum, b.Acs.Checksum);
    }

    [Fact]
    public void ReadingOrWritingZeroDoesNotCreateChecksumOnlyState()
    {
        var a = Room(); var b = Room();
        Run(a, 1, 3, -1, 226, 0, 54, 3, 2, 3, 0, 236, 0, 1);
        Run(a, 1, 1); Run(b, 1, 1); Assert.Equal(a.Acs.Checksum, b.Acs.Checksum);
    }

    private static void Run(AuthoritySimulation sim, int number, params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = number, Code = bytes });
        Assert.True(sim.Acs.Enqueue(number)); sim.Acs.Tick(sim); Assert.Equal(0, sim.Acs.RunningCount);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Format = MapDataFormat.HexenBinary, Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }] });
}
