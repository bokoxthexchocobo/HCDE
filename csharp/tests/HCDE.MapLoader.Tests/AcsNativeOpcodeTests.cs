using System.Buffers.Binary;

namespace HCDE.MapLoader.Tests;

public class AcsNativeOpcodeTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TrigonometryHasNoImmediateOperands(bool compact)
    {
        byte[] bytes = compact ? [220, 221, 222, 1] : [220, 0, 0, 0, 221, 0, 0, 0, 222, 0, 0, 0, 1, 0, 0, 0];
        Assert.Equal(new[] { 220, 221, 222 }, new[] { (int)AcsPcode.Sin, (int)AcsPcode.Cos, (int)AcsPcode.VectorAngle });
        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(bytes,
            compact ? MapBehaviorFormat.AcsLittleEnhanced : MapBehaviorFormat.AcsOld, 0,
            out var instructions, out var terminated, out var error), error);
        Assert.True(terminated);
        Assert.Equal(new[] { 220, 221, 222, 1 }, instructions.Select(instruction => instruction.Opcode));
        Assert.Equal(compact ? new[] { 0, 1, 2, 3 } : new[] { 0, 4, 8, 12 }, instructions.Select(instruction => instruction.Offset));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HealthAndArmorQueriesHaveNoImmediateOperands(bool compact)
    {
        byte[] bytes = compact ? [120, 121, 1] : [120, 0, 0, 0, 121, 0, 0, 0, 1, 0, 0, 0];
        Assert.Equal(120, (int)AcsPcode.PlayerHealth);
        Assert.Equal(121, (int)AcsPcode.PlayerArmorPoints);
        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(bytes,
            compact ? MapBehaviorFormat.AcsLittleEnhanced : MapBehaviorFormat.AcsOld, 0,
            out var instructions, out var terminated, out var error), error);
        Assert.True(terminated);
        Assert.Equal(new[] { 120, 121, 1 }, instructions.Select(instruction => instruction.Opcode));
        Assert.Equal(compact ? new[] { 0, 1, 2 } : new[] { 0, 4, 8 }, instructions.Select(instruction => instruction.Offset));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ActorLightQueryUsesNativeOpcodeWithoutImmediateOperands(bool compact)
    {
        byte[] bytes = compact ? [240, 100, 1] : [84, 1, 0, 0, 1, 0, 0, 0];
        Assert.Equal(340, (int)AcsPcode.GetActorLightLevel);
        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(bytes,
            compact ? MapBehaviorFormat.AcsLittleEnhanced : MapBehaviorFormat.AcsOld, 0,
            out var instructions, out var terminated, out var error), error);
        Assert.True(terminated); Assert.Equal(2, instructions.Count);
        Assert.Equal(340, instructions[0].Opcode); Assert.Equal(compact ? 2 : 4, instructions[1].Offset);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NativeShiftInstructionsConsumeOneIndex(bool compact)
    {
        var bytes = new List<byte>(); var offsets = new List<int>();
        foreach (var opcode in Enumerable.Range(312, 14))
        {
            offsets.Add(bytes.Count);
            if (compact) bytes.AddRange([240, (byte)(opcode - 240), 19]);
            else
            {
                var instruction = new byte[8]; BinaryPrimitives.WriteInt32LittleEndian(instruction, opcode);
                BinaryPrimitives.WriteInt32LittleEndian(instruction.AsSpan(4), 19); bytes.AddRange(instruction);
            }
            Assert.False(MapBehaviorBytecodeWalker.TryWalkScript(bytes.ToArray().AsSpan(0, bytes.Count - 1),
                compact ? MapBehaviorFormat.AcsLittleEnhanced : MapBehaviorFormat.AcsOld, 0, out _, out _, out _));
        }
        bytes.Add(1); if (!compact) bytes.AddRange([0, 0, 0]);
        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(bytes.ToArray(),
            compact ? MapBehaviorFormat.AcsLittleEnhanced : MapBehaviorFormat.AcsOld, 0,
            out var instructions, out var terminated, out var error), error);
        Assert.True(terminated); Assert.Equal(15, instructions.Count);
        Assert.Equal(Enumerable.Range(312, 14), instructions.Take(14).Select(i => i.Opcode));
        Assert.Equal(offsets, instructions.Take(14).Select(i => i.Offset));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ScalarBitwiseInstructionsConsumeOneIndex(bool compact)
    {
        int[] opcodes = [291, 292, 294, 298, 299, 301, 305, 306, 308];
        var bytes = new List<byte>(); var offsets = new List<int>();
        foreach (var opcode in opcodes)
        {
            offsets.Add(bytes.Count);
            if (compact) bytes.AddRange([240, (byte)(opcode - 240), 19]);
            else
            {
                var instruction = new byte[8];
                BinaryPrimitives.WriteInt32LittleEndian(instruction, opcode);
                BinaryPrimitives.WriteInt32LittleEndian(instruction.AsSpan(4), 19);
                bytes.AddRange(instruction);
            }
            Assert.False(MapBehaviorBytecodeWalker.TryWalkScript(bytes.ToArray().AsSpan(0, bytes.Count - 1),
                compact ? MapBehaviorFormat.AcsLittleEnhanced : MapBehaviorFormat.AcsOld, 0, out _, out _, out _));
        }
        bytes.Add(1); if (!compact) bytes.AddRange([0, 0, 0]);
        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(bytes.ToArray(),
            compact ? MapBehaviorFormat.AcsLittleEnhanced : MapBehaviorFormat.AcsOld, 0,
            out var instructions, out var terminated, out var error), error);
        Assert.True(terminated);
        Assert.Equal(opcodes, instructions.Take(9).Select(instruction => instruction.Opcode));
        Assert.Equal(offsets, instructions.Take(9).Select(instruction => instruction.Offset));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FixedArithmeticAndAdjacentControlsUseNativeOperandWidths(bool compact)
    {
        int[] opcodes = [135, 136, 137, 138, 139, 140, 141, 1];
        var bytes = new List<byte>(); var offsets = new List<int>();
        foreach (var opcode in opcodes)
        {
            offsets.Add(bytes.Count);
            if (compact) bytes.Add((byte)opcode);
            else
            {
                var word = new byte[4]; BinaryPrimitives.WriteInt32LittleEndian(word, opcode); bytes.AddRange(word);
            }
            if (opcode is 139 or 141) bytes.AddRange([0x34, 0x12, 1, 0]);
        }
        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(bytes.ToArray(),
            compact ? MapBehaviorFormat.AcsLittleEnhanced : MapBehaviorFormat.AcsOld, 0,
            out var instructions, out var terminated, out var error), error);
        Assert.True(terminated);
        Assert.Equal(opcodes, instructions.Select(instruction => instruction.Opcode));
        Assert.Equal(offsets, instructions.Select(instruction => instruction.Offset));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NativeStackOperationsHaveNoOperands(bool compact)
    {
        int[] opcodes = [216, 217, 330, 69, 1];
        var bytes = new List<byte>(); var offsets = new List<int>();
        foreach (var opcode in opcodes)
        {
            offsets.Add(bytes.Count);
            if (compact)
            {
                if (opcode >= 240) bytes.AddRange([240, (byte)(opcode - 240)]);
                else bytes.Add((byte)opcode);
            }
            else
            {
                var word = new byte[4]; BinaryPrimitives.WriteInt32LittleEndian(word, opcode); bytes.AddRange(word);
            }
        }
        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(bytes.ToArray(),
            compact ? MapBehaviorFormat.AcsLittleEnhanced : MapBehaviorFormat.AcsOld, 0,
            out var instructions, out var terminated, out var error), error);
        Assert.True(terminated);
        Assert.Equal(opcodes, instructions.Select(instruction => instruction.Opcode));
        Assert.Equal(offsets, instructions.Select(instruction => instruction.Offset));
    }

    [Fact]
    public void ResultCallUsesNativeOpcode263() => Assert.Equal(263, (int)AcsPcode.Lspec5Result);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ResultCallOperandWidthMatchesBytecodeFormat(bool compact)
    {
        byte[] bytes = compact ? [240, 23, 112, 1] : [7, 1, 0, 0, 112, 0, 0, 0, 1, 0, 0, 0];
        var format = compact ? MapBehaviorFormat.AcsLittleEnhanced : MapBehaviorFormat.AcsOld;
        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(bytes, format, 0,
            out var instructions, out var terminated, out var error), error);
        Assert.True(terminated); Assert.Equal(2, instructions.Count);
        Assert.Equal(263, instructions[0].Opcode);
        Assert.Equal(compact ? 3 : 8, instructions[1].Offset);
        Assert.False(MapBehaviorBytecodeWalker.TryWalkScript(bytes.AsSpan(0, compact ? 2 : 7), format, 0,
            out _, out _, out _));
    }

    [Theory]
    [InlineData(381, false)]
    [InlineData(382, false)]
    [InlineData(381, true)]
    [InlineData(382, true)]
    public void ExtendedSpecialUsesFourByteOperandInBothFormats(int opcode, bool compact)
    {
        var prefix = compact ? 2 : 4;
        var bytes = new byte[prefix + 4 + (compact ? 1 : 4)];
        if (compact) { bytes[0] = 240; bytes[1] = (byte)(opcode - 240); }
        else BinaryPrimitives.WriteInt32LittleEndian(bytes, opcode);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(prefix), 0x12345678);
        bytes[prefix + 4] = 1;
        var format = compact ? MapBehaviorFormat.AcsLittleEnhanced : MapBehaviorFormat.AcsOld;
        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(bytes, format, 0,
            out var instructions, out var terminated, out var error), error);
        Assert.True(terminated); Assert.Equal(2, instructions.Count);
        Assert.Equal(prefix + 4, instructions[1].Offset);
        for (var length = prefix; length < prefix + 4; length++)
            Assert.False(MapBehaviorBytecodeWalker.TryWalkScript(bytes.AsSpan(0, length), format, 0, out _, out _, out _));
    }

    [Theory]
    [InlineData(AcsPcode.CallFunc, 351)]
    [InlineData(AcsPcode.LsScriptVar, 312)]
    [InlineData(AcsPcode.LsMapVar, 313)]
    [InlineData(AcsPcode.LsWorldVar, 314)]
    [InlineData(AcsPcode.LsGlobalVar, 315)]
    [InlineData(AcsPcode.LsMapArray, 316)]
    [InlineData(AcsPcode.LsWorldArray, 317)]
    [InlineData(AcsPcode.LsGlobalArray, 318)]
    [InlineData(AcsPcode.RsScriptVar, 319)]
    [InlineData(AcsPcode.RsMapVar, 320)]
    [InlineData(AcsPcode.RsWorldVar, 321)]
    [InlineData(AcsPcode.RsGlobalVar, 322)]
    [InlineData(AcsPcode.RsMapArray, 323)]
    [InlineData(AcsPcode.RsWorldArray, 324)]
    [InlineData(AcsPcode.RsGlobalArray, 325)]
    [InlineData(AcsPcode.AndScriptVar, 291)]
    [InlineData(AcsPcode.AndMapVar, 292)]
    [InlineData(AcsPcode.AndGlobalVar, 294)]
    [InlineData(AcsPcode.EorScriptVar, 298)]
    [InlineData(AcsPcode.EorMapVar, 299)]
    [InlineData(AcsPcode.EorGlobalVar, 301)]
    [InlineData(AcsPcode.OrScriptVar, 305)]
    [InlineData(AcsPcode.OrMapVar, 306)]
    [InlineData(AcsPcode.OrGlobalVar, 308)]
    [InlineData(AcsPcode.SinglePlayer, 135)]
    [InlineData(AcsPcode.FixedMul, 136)]
    [InlineData(AcsPcode.FixedDiv, 137)]
    [InlineData(AcsPcode.SetGravity, 138)]
    [InlineData(AcsPcode.SetGravityDirect, 139)]
    [InlineData(AcsPcode.SetAirControl, 140)]
    [InlineData(AcsPcode.SetAirControlDirect, 141)]
    [InlineData(AcsPcode.Dup, 216)]
    [InlineData(AcsPcode.Swap, 217)]
    [InlineData(AcsPcode.NegateBinary, 330)]
    [InlineData(AcsPcode.Restart, 69)]
    [InlineData(AcsPcode.SaveString, 352)]
    [InlineData(AcsPcode.PrintMapCharRange, 353)]
    [InlineData(AcsPcode.PrintWorldCharRange, 354)]
    [InlineData(AcsPcode.PrintGlobalCharRange, 355)]
    [InlineData(AcsPcode.StrCpyToMapCharRange, 356)]
    [InlineData(AcsPcode.StrCpyToWorldCharRange, 357)]
    [InlineData(AcsPcode.StrCpyToGlobalCharRange, 358)]
    [InlineData(AcsPcode.PushFunction, 359)]
    [InlineData(AcsPcode.CallStack, 360)]
    [InlineData(AcsPcode.ScriptWaitNamed, 361)]
    [InlineData(AcsPcode.TranslationRange3, 362)]
    [InlineData(AcsPcode.GotoStack, 363)]
    [InlineData(AcsPcode.AssignScriptArray, 364)]
    [InlineData(AcsPcode.PushScriptArray, 365)]
    [InlineData(AcsPcode.AddScriptArray, 366)]
    [InlineData(AcsPcode.SubScriptArray, 367)]
    [InlineData(AcsPcode.MulScriptArray, 368)]
    [InlineData(AcsPcode.DivScriptArray, 369)]
    [InlineData(AcsPcode.ModScriptArray, 370)]
    [InlineData(AcsPcode.IncScriptArray, 371)]
    [InlineData(AcsPcode.DecScriptArray, 372)]
    [InlineData(AcsPcode.AndScriptArray, 373)]
    [InlineData(AcsPcode.EorScriptArray, 374)]
    [InlineData(AcsPcode.OrScriptArray, 375)]
    [InlineData(AcsPcode.LsScriptArray, 376)]
    [InlineData(AcsPcode.RsScriptArray, 377)]
    [InlineData(AcsPcode.TranslationRange4, 383)]
    [InlineData(AcsPcode.TranslationRange5, 384)]
    [InlineData(AcsPcode.SetLineMonsterBlocking, 104)]
    [InlineData(AcsPcode.AssignGlobalVar, 181)]
    [InlineData(AcsPcode.PushGlobalVar, 182)]
    [InlineData(AcsPcode.AddGlobalVar, 183)]
    [InlineData(AcsPcode.SubGlobalVar, 184)]
    [InlineData(AcsPcode.MulGlobalVar, 185)]
    [InlineData(AcsPcode.DivGlobalVar, 186)]
    [InlineData(AcsPcode.ModGlobalVar, 187)]
    [InlineData(AcsPcode.IncGlobalVar, 188)]
    [InlineData(AcsPcode.DecGlobalVar, 189)]
    public void EnumMatchesNativeWireIds(AcsPcode opcode, int wire) => Assert.Equal(wire, (int)opcode);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GlobalVariableIndicesDoNotConsumeFollowingInstructions(bool compact)
    {
        var bytes = new List<byte>(); var offsets = new List<int>();
        foreach (var opcode in Enumerable.Range(181, 9))
        {
            offsets.Add(bytes.Count);
            if (compact) bytes.AddRange([(byte)opcode, 255]);
            else
            {
                var word = new byte[4]; BinaryPrimitives.WriteInt32LittleEndian(word, opcode); bytes.AddRange(word);
                BinaryPrimitives.WriteInt32LittleEndian(word, 255); bytes.AddRange(word);
            }
        }
        offsets.Add(bytes.Count);
        if (compact) bytes.AddRange([104, 1]);
        else bytes.AddRange([104, 0, 0, 0, 1, 0, 0, 0]);
        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(bytes.ToArray(),
            compact ? MapBehaviorFormat.AcsLittleEnhanced : MapBehaviorFormat.AcsOld, 0,
            out var instructions, out var terminated, out var error), error);
        Assert.True(terminated); Assert.Equal(11, instructions.Count);
        for (var i = 0; i < offsets.Count; i++) Assert.Equal(offsets[i], instructions[i].Offset);
        Assert.Equal(104, instructions[9].Opcode);
    }

    [Theory]
    [InlineData(181)]
    [InlineData(182)]
    [InlineData(183)]
    [InlineData(184)]
    [InlineData(185)]
    [InlineData(186)]
    [InlineData(187)]
    [InlineData(188)]
    [InlineData(189)]
    public void CompactGlobalVariableOpcodeRequiresIndex(byte opcode)
    {
        Assert.False(MapBehaviorBytecodeWalker.TryWalkScript(new byte[] { opcode },
            MapBehaviorFormat.AcsLittleEnhanced, 0, out _, out _, out var error));
        Assert.Equal("behavior-script-bytecode-truncated", error);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NativeFunctionArrayAndTranslationBlockStaysAligned(bool compact)
    {
        var bytes = new List<byte>(); var expected = new List<(int Opcode, int Offset)>();
        foreach (var opcode in Enumerable.Range(359, 19).Concat(new[] { 383, 384, 1 }))
        {
            expected.Add((opcode, bytes.Count));
            var operand = opcode == 359 || opcode is >= 364 and <= 377;
            if (compact)
            {
                if (opcode < 240) bytes.Add((byte)opcode);
                else { bytes.Add(240); bytes.Add((byte)(opcode - 240)); }
                if (operand) bytes.Add(9);
            }
            else
            {
                var word = new byte[4]; BinaryPrimitives.WriteInt32LittleEndian(word, opcode); bytes.AddRange(word);
                if (operand) { BinaryPrimitives.WriteInt32LittleEndian(word, 9); bytes.AddRange(word); }
            }
        }
        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(bytes.ToArray(),
            compact ? MapBehaviorFormat.AcsLittleEnhanced : MapBehaviorFormat.AcsOld, 0,
            out var instructions, out var terminated, out var error), error);
        Assert.True(terminated); Assert.Equal(expected.Count, instructions.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].Opcode, instructions[i].Opcode);
            Assert.Equal(expected[i].Offset, instructions[i].Offset);
        }
    }

    [Fact]
    public void WordCallFollowedByStringRangesRemainsAligned()
    {
        int[] words = [351, 0, 19700, 352, 353, 354, 355, 356, 357, 358, 1];
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(bytes, MapBehaviorFormat.AcsOld, 0,
            out var instructions, out var terminated, out var error), error);
        Assert.True(terminated); Assert.Equal(9, instructions.Count);
        Assert.Equal(2, instructions[0].OperandWordCount);
        Assert.Equal(16, instructions[2].Offset); Assert.Equal(353, instructions[2].Opcode);
    }

    [Fact]
    public void CompactCallReadsByteCountAndTwoByteFunctionThenNextOpcode()
    {
        // Extended opcode 351, zero arguments, function 19700 (0x4cf4), print range, terminate.
        byte[] bytes = [240, 111, 0, 244, 76, 240, 113, 1];
        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(bytes, MapBehaviorFormat.AcsLittleEnhanced, 0,
            out var instructions, out var terminated, out var error), error);
        Assert.True(terminated); Assert.Equal(3, instructions.Count);
        Assert.Equal(351, instructions[0].Opcode);
        Assert.Equal(5, instructions[1].Offset); Assert.Equal(353, instructions[1].Opcode);
        Assert.Equal(7, instructions[2].Offset);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void CompactCallRejectsEveryTruncatedOperandLength(int operands)
    {
        byte[] bytes = [240, 111, 0, 244];
        Assert.False(MapBehaviorBytecodeWalker.TryWalkScript(bytes.AsSpan(0, 2 + operands),
            MapBehaviorFormat.AcsLittleEnhanced, 0, out _, out _, out var error));
        Assert.Equal("behavior-script-bytecode-truncated", error);
    }

    [Theory]
    [InlineData(240)]
    [InlineData(255)]
    public void TruncatedExtendedOpcodeReturnsRejectionInsteadOfThrowing(byte first)
    {
        Assert.False(MapBehaviorBytecodeWalker.TryWalkScript(new byte[] { first },
            MapBehaviorFormat.AcsLittleEnhanced, 0, out _, out _, out var error));
        Assert.Equal("behavior-script-bytecode-truncated", error);
    }
}
