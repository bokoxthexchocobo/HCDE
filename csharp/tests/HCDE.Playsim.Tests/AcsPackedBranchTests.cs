using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsPackedBranchTests
{
    [Theory]
    [InlineData(52, 1)]
    [InlineData(52, 2)]
    [InlineData(52, 3)]
    [InlineData(53, 1)]
    [InlineData(53, 2)]
    [InlineData(53, 3)]
    [InlineData(79, 1)]
    [InlineData(79, 2)]
    [InlineData(79, 3)]
    [InlineData(84, 1)]
    [InlineData(84, 2)]
    [InlineData(84, 3)]
    public void BranchCanTargetEachUnalignedOffset(int opcode, int padding)
    {
        var code = new List<byte>();
        if (opcode != 52) { Word(code, 3); Word(code, opcode == 79 ? 0 : 17); }
        Word(code, opcode);
        if (opcode == 84) Word(code, 17);
        var targetOperand = code.Count; Word(code, 0);
        Word(code, 1); // Falling through must not reach the light action.
        code.AddRange(new byte[padding]); var target = code.Count;
        Word(code, 10); Word(code, 112); Word(code, 7); Word(code, 35); Word(code, 1);
        var bytes = code.ToArray(); BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(targetOperand), target);
        Assert.Equal(padding, target % 4); Run(bytes);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(3, true)]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(3, false)]
    public void SortedTableAlignsAfterPackedOperands(int count, bool match)
    {
        var code = new List<byte>(); Word(code, 175); code.Add((byte)count);
        code.AddRange(Enumerable.Repeat((byte)17, count));
        Word(code, 256); while (code.Count % 4 != 0) code.Add(255);
        Word(code, 1); Word(code, match ? 17 : 18); var targetOperand = code.Count; Word(code, 0);
        if (match) Word(code, 1);
        var target = code.Count;
        // On a miss the selector remains on stack. On a match it was removed.
        if (!match) Word(code, 54);
        Word(code, 10); Word(code, 112); Word(code, 7); Word(code, 35); Word(code, 1);
        var bytes = code.ToArray(); BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(targetOperand), target);
        Run(bytes);
    }

    private static void Run(byte[] code)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128, LightLevel = 128 }] });
        sim.Acs.Add(new AcsProgram { Number = 1, Code = code }); sim.Acs.Enqueue(1); sim.Acs.Tick(sim);
        Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }
    private static void Word(List<byte> bytes, int value)
    {
        var word = new byte[4]; BinaryPrimitives.WriteInt32LittleEndian(word, value); bytes.AddRange(word);
    }
}
