using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsModuleAddressTests
{
    [Theory]
    [InlineData(52)]
    [InlineData(53)]
    [InlineData(79)]
    [InlineData(84)]
    public void OrdinaryBranchesResolveModuleOffsets(int opcode)
    {
        var code = new List<byte>();
        if (opcode != 52) { Word(code, 3); Word(code, opcode == 79 ? 0 : 17); }
        Word(code, opcode); if (opcode == 84) Word(code, 17);
        var operand = code.Count; Word(code, 0); Word(code, 1);
        var target = code.Count; Action(code);
        var bytes = code.ToArray(); BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(operand), 64 + target);
        Run(new AcsProgram { Number = 1, Code = bytes, CodeBaseOffset = 64 });
    }

    [Theory]
    [InlineData(64)]
    [InlineData(65)]
    [InlineData(66)]
    [InlineData(67)]
    public void SortedTableAlignsInModuleRatherThanSlice(int origin)
    {
        var code = new List<byte>(); Word(code, 3); Word(code, 17); Word(code, 256);
        while ((origin + code.Count) % 4 != 0) code.Add(255);
        Word(code, 1); Word(code, 17); var operand = code.Count; Word(code, 0); Word(code, 1);
        var target = code.Count; Action(code);
        var bytes = code.ToArray(); BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(operand), origin + target);
        Run(new AcsProgram { Number = 1, Code = bytes, CodeBaseOffset = origin });
    }

    [Fact]
    public void JumpTableTargetsAreNotRelocatedTwice()
    {
        var code = new List<byte>(); Word(code, 3); Word(code, 0); Word(code, 363); Word(code, 1);
        var target = code.Count; Action(code);
        Run(new AcsProgram { Number = 1, Code = code.ToArray(), CodeBaseOffset = 64, JumpPoints = [target] });
    }

    [Fact]
    public void RestartReturnsToLocalEntryPoint()
    {
        var code = new List<byte>(); Word(code, 69); // Restart loops at the local entry, bounded by budget.
        var sim = Room(); sim.Acs.Add(new AcsProgram { Number = 1, Code = code.ToArray(), CodeBaseOffset = 64 });
        sim.Acs.Enqueue(1); sim.Acs.Tick(sim); Assert.Equal(1, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(63)]
    [InlineData(72)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void BranchOutsideSliceStopsSafely(int target)
    {
        var code = new List<byte>(); Word(code, 52); Word(code, target);
        var sim = Room(); sim.Acs.Add(new AcsProgram { Number = 1, Code = code.ToArray(), CodeBaseOffset = 64 });
        sim.Acs.Enqueue(1); sim.Acs.Tick(sim); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void InvalidCodeRegionRejectsRegistration(int origin)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Room().Acs.Add(new AcsProgram
            { Code = new byte[4], CodeBaseOffset = origin }));
    }

    [Fact]
    public void BaseOffsetParticipatesInChecksum()
    {
        var first = Room(); var second = Room();
        first.Acs.Add(new AcsProgram { Code = new byte[4] });
        second.Acs.Add(new AcsProgram { Code = new byte[4], CodeBaseOffset = 64 });
        Assert.NotEqual(first.Acs.Checksum, second.Acs.Checksum);
    }

    private static void Action(List<byte> code)
    { foreach (var word in new[] { 10, 112, 7, 35, 1 }) Word(code, word); }
    private static void Word(List<byte> code, int value)
    {
        var bytes = new byte[4]; BinaryPrimitives.WriteInt32LittleEndian(bytes, value); code.AddRange(bytes);
    }
    private static void Run(AcsProgram program)
    {
        var sim = Room(); sim.Acs.Add(program); sim.Acs.Enqueue(1); sim.Acs.Tick(sim);
        Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128, LightLevel = 128 }] });
}
