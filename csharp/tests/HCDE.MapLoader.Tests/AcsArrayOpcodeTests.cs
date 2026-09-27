using System.Buffers.Binary;

namespace HCDE.MapLoader.Tests;

public class AcsArrayOpcodeTests
{
    [Theory]
    [InlineData(AcsPcode.PushWorldArray, 226)]
    [InlineData(AcsPcode.AssignWorldArray, 227)]
    [InlineData(AcsPcode.AddWorldArray, 228)]
    [InlineData(AcsPcode.SubWorldArray, 229)]
    [InlineData(AcsPcode.MulWorldArray, 230)]
    [InlineData(AcsPcode.DivWorldArray, 231)]
    [InlineData(AcsPcode.ModWorldArray, 232)]
    [InlineData(AcsPcode.IncWorldArray, 233)]
    [InlineData(AcsPcode.DecWorldArray, 234)]
    [InlineData(AcsPcode.PushGlobalArray, 235)]
    [InlineData(AcsPcode.AssignGlobalArray, 236)]
    [InlineData(AcsPcode.AddGlobalArray, 237)]
    [InlineData(AcsPcode.SubGlobalArray, 238)]
    [InlineData(AcsPcode.MulGlobalArray, 239)]
    [InlineData(AcsPcode.DivGlobalArray, 240)]
    [InlineData(AcsPcode.ModGlobalArray, 241)]
    [InlineData(AcsPcode.IncGlobalArray, 242)]
    [InlineData(AcsPcode.DecGlobalArray, 243)]
    [InlineData(AcsPcode.GiveInventoryDirect, 144)]
    [InlineData(AcsPcode.TakeInventoryDirect, 146)]
    [InlineData(AcsPcode.CheckInventoryDirect, 148)]
    [InlineData(AcsPcode.SetMusic, 153)]
    [InlineData(AcsPcode.SetMusicDirect, 154)]
    [InlineData(AcsPcode.LocalSetMusic, 155)]
    [InlineData(AcsPcode.LocalSetMusicDirect, 156)]
    [InlineData(AcsPcode.PrintFixed, 157)]
    [InlineData(AcsPcode.PrintLocalized, 158)]
    [InlineData(AcsPcode.PushByte, 167)]
    [InlineData(AcsPcode.SetActorProperty, 245)]
    [InlineData(AcsPcode.GetActorProperty, 246)]
    [InlineData(AcsPcode.ActivatorTid, 248)]
    public void EnumMatchesNativeWireNumbers(AcsPcode opcode, int expected) => Assert.Equal(expected, (int)opcode);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EveryWorldAndGlobalArrayInstructionConsumesExactlyOneIndex(bool compact)
    {
        var data = new List<byte>(); var offsets = new List<int>();
        foreach (var opcode in Enumerable.Range(226, 18))
        {
            offsets.Add(data.Count); EmitOpcode(data, opcode, compact);
            var operandStart = data.Count;
            if (compact) data.Add(255); else EmitWord(data, 255);
            for (var length = operandStart; length < data.Count; length++)
                Assert.False(Walk(data.Take(length).ToArray(), compact, out _, out _));
        }
        EmitOpcode(data, 1, compact);
        Assert.True(Walk(data.ToArray(), compact, out var instructions, out var terminated));
        Assert.True(terminated); Assert.Equal(19, instructions.Count);
        Assert.Equal(Enumerable.Range(226, 18), instructions.Take(18).Select(i => i.Opcode));
        Assert.Equal(offsets, instructions.Take(18).Select(i => i.Offset));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CorrectedNeighborsAndFormerArrayNumbersRetainNativeWidths(bool compact)
    {
        (int Opcode, int Words)[] sequence = [(144, 2), (146, 2), (148, 1),
            (153, 0), (154, 3), (155, 0), (156, 3), (157, 0), (158, 0),
            (244, 0), (245, 0), (246, 0), (247, 0), (248, 0), (249, 0),
            (306, 1), (307, 1), (308, 1), (309, 1), (310, 1), (311, 1), (312, 1), (313, 1), (314, 1)];
        var data = new List<byte>(); var offsets = new List<int>();
        foreach (var entry in sequence)
        {
            offsets.Add(data.Count); EmitOpcode(data, entry.Opcode, compact);
            for (var i = 0; i < entry.Words; i++)
            {
                if (compact && entry.Opcode >= 306) data.Add(19);
                else EmitWord(data, 19);
            }
        }
        EmitOpcode(data, 1, compact);
        Assert.True(Walk(data.ToArray(), compact, out var instructions, out var terminated));
        Assert.True(terminated); Assert.Equal(sequence.Length + 1, instructions.Count);
        Assert.Equal(sequence.Select(e => e.Opcode), instructions.Take(sequence.Length).Select(i => i.Opcode));
        Assert.Equal(offsets, instructions.Take(sequence.Length).Select(i => i.Offset));
    }

    [Fact]
    public void CompactPushByteAndPlayerNumberHaveDifferentWidths()
    {
        Assert.True(Walk([167, 255, 240, 7, 1], true, out var instructions, out var terminated));
        Assert.True(terminated);
        Assert.Equal(new[] { 167, 247, 1 }, instructions.Select(i => i.Opcode));
        Assert.Equal(new[] { 0, 2, 4 }, instructions.Select(i => i.Offset));
    }

    private static bool Walk(byte[] bytes, bool compact, out IReadOnlyList<MapBehaviorInstruction> instructions, out bool terminated) =>
        MapBehaviorBytecodeWalker.TryWalkScript(bytes, compact ? MapBehaviorFormat.AcsLittleEnhanced : MapBehaviorFormat.AcsOld,
            0, out instructions, out terminated, out _);

    private static void EmitOpcode(List<byte> bytes, int opcode, bool compact)
    {
        if (!compact) EmitWord(bytes, opcode);
        else if (opcode < 240) bytes.Add((byte)opcode);
        else bytes.AddRange([240, (byte)(opcode - 240)]);
    }

    private static void EmitWord(List<byte> bytes, int value)
    {
        var word = new byte[4]; BinaryPrimitives.WriteInt32LittleEndian(word, value); bytes.AddRange(word);
    }
}
