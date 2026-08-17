using System.Buffers.Binary;

namespace HCDE.MapLoader;

public readonly struct MapBehaviorInstruction
{
    public MapBehaviorInstruction(int offset, int opcode, int operandWordCount)
    {
        Offset = offset;
        Opcode = opcode;
        OperandWordCount = operandWordCount;
    }

    public int Offset { get; }
    public int Opcode { get; }
    public int OperandWordCount { get; }
}

public readonly struct MapBehaviorScriptBytecode
{
    public MapBehaviorScriptBytecode(
        int scriptNumber,
        IReadOnlyList<MapBehaviorInstruction> instructions,
        bool terminatedNormally)
    {
        ScriptNumber = scriptNumber;
        Instructions = instructions;
        TerminatedNormally = terminatedNormally;
    }

    public int ScriptNumber { get; }
    public IReadOnlyList<MapBehaviorInstruction> Instructions { get; }
    public bool TerminatedNormally { get; }
}

public static class MapBehaviorBytecodeWalker
{
    private const int UnknownOperandWords = -1;
    private const int EndScript = -2;

    public static bool TryWalkScripts(
        ReadOnlySpan<byte> data,
        MapBehaviorFormat format,
        IReadOnlyList<MapBehaviorScriptEntry> scripts,
        out IReadOnlyList<MapBehaviorScriptBytecode> scriptBodies,
        out string? rejectReason)
    {
        scriptBodies = Array.Empty<MapBehaviorScriptBytecode>();
        rejectReason = null;

        if (scripts.Count == 0)
            return true;

        var bodies = new MapBehaviorScriptBytecode[scripts.Count];
        for (var i = 0; i < scripts.Count; i++)
        {
            if (!TryWalkScript(data, format, scripts[i].Address, out var instructions, out var terminated, out rejectReason))
                return false;

            bodies[i] = new MapBehaviorScriptBytecode(scripts[i].Number, instructions, terminated);
        }

        scriptBodies = bodies;
        return true;
    }

    public static bool TryWalkScript(
        ReadOnlySpan<byte> data,
        MapBehaviorFormat format,
        uint scriptAddress,
        out IReadOnlyList<MapBehaviorInstruction> instructions,
        out bool terminatedNormally,
        out string? rejectReason)
    {
        instructions = Array.Empty<MapBehaviorInstruction>();
        terminatedNormally = false;
        rejectReason = null;

        if (scriptAddress >= data.Length)
            return Reject("behavior-script-address-out-of-range", out instructions, out terminatedNormally, out rejectReason);

        return format switch
        {
            MapBehaviorFormat.AcsOld or MapBehaviorFormat.AcsEnhanced
                => WalkWordOpcodes(data, (int)scriptAddress, out instructions, out terminatedNormally, out rejectReason),
            MapBehaviorFormat.AcsLittleEnhanced
                => WalkLittleEnhancedOpcodes(data, (int)scriptAddress, out instructions, out terminatedNormally, out rejectReason),
            _ => Reject("behavior-format-unknown", out instructions, out terminatedNormally, out rejectReason),
        };
    }

    private static bool WalkWordOpcodes(
        ReadOnlySpan<byte> data,
        int offset,
        out IReadOnlyList<MapBehaviorInstruction> instructions,
        out bool terminatedNormally,
        out string? rejectReason)
    {
        instructions = Array.Empty<MapBehaviorInstruction>();
        terminatedNormally = false;
        rejectReason = null;

        var list = new List<MapBehaviorInstruction>();
        while (offset + 4 <= data.Length)
        {
            var opcode = BinaryPrimitives.ReadInt32LittleEndian(data[offset..]);
            var operandWords = GetWordOperandSkipCount(opcode);
            if (operandWords == EndScript)
            {
                list.Add(new MapBehaviorInstruction(offset, opcode, 0));
                terminatedNormally = opcode == (int)AcsPcode.Terminate || opcode == (int)AcsPcode.Suspend;
                instructions = list;
                return true;
            }

            if (operandWords == UnknownOperandWords)
                return Reject($"behavior-unknown-pcode-{opcode}", out instructions, out terminatedNormally, out rejectReason);

            if (offset + 4 + operandWords * 4 > data.Length)
                return Reject("behavior-script-bytecode-truncated", out instructions, out terminatedNormally, out rejectReason);

            list.Add(new MapBehaviorInstruction(offset, opcode, operandWords));
            offset += 4 + operandWords * 4;
        }

        instructions = list;
        return true;
    }

    private static bool WalkLittleEnhancedOpcodes(
        ReadOnlySpan<byte> data,
        int offset,
        out IReadOnlyList<MapBehaviorInstruction> instructions,
        out bool terminatedNormally,
        out string? rejectReason)
    {
        instructions = Array.Empty<MapBehaviorInstruction>();
        terminatedNormally = false;
        rejectReason = null;

        var list = new List<MapBehaviorInstruction>();
        while (offset < data.Length)
        {
            var first = data[offset];
            var opcode = first >= 240
                ? 240 + ((first - 240) << 8) + data[offset + 1]
                : first;
            var opcodeBytes = first >= 240 ? 2 : 1;
            var operandBytes = GetLittleEnhancedOperandSkipBytes(opcode);
            if (operandBytes == EndScript)
            {
                list.Add(new MapBehaviorInstruction(offset, opcode, 0));
                terminatedNormally = opcode == (int)AcsPcode.Terminate || opcode == (int)AcsPcode.Suspend;
                instructions = list;
                return true;
            }

            if (operandBytes == UnknownOperandWords)
                return Reject($"behavior-unknown-pcode-{opcode}", out instructions, out terminatedNormally, out rejectReason);

            if (offset + opcodeBytes + operandBytes > data.Length)
                return Reject("behavior-script-bytecode-truncated", out instructions, out terminatedNormally, out rejectReason);

            list.Add(new MapBehaviorInstruction(offset, opcode, operandBytes));
            offset += opcodeBytes + operandBytes;
        }

        instructions = list;
        return true;
    }

    private static int GetWordOperandSkipCount(int opcode) => opcode switch
    {
        (int)AcsPcode.Nop => 0,
        (int)AcsPcode.Terminate or (int)AcsPcode.Suspend => EndScript,
        (int)AcsPcode.PushNumber => 1,
        (int)AcsPcode.Lspec1Direct => 2,
        (int)AcsPcode.Lspec2Direct => 3,
        (int)AcsPcode.Lspec3Direct => 4,
        (int)AcsPcode.Lspec4Direct => 5,
        (int)AcsPcode.Lspec5Direct => 6,
        (int)AcsPcode.DelayDirect => 1,
        (int)AcsPcode.RandomDirect => 2,
        (int)AcsPcode.ThingCountDirect => 2,
        (int)AcsPcode.TagWaitDirect => 1,
        (int)AcsPcode.PolyWaitDirect => 1,
        (int)AcsPcode.ChangeFloorDirect => 2,
        (int)AcsPcode.ChangeCeilingDirect => 2,
        (int)AcsPcode.Goto or (int)AcsPcode.IfGoto => 1,
        >= (int)AcsPcode.Lspec1 and <= (int)AcsPcode.Ge => 0,
        >= (int)AcsPcode.AssignScriptVar and <= (int)AcsPcode.PushWorldVar => 1,
        (int)AcsPcode.Drop or (int)AcsPcode.Delay => 0,
        >= (int)AcsPcode.Add and <= (int)AcsPcode.Restart => 0,
        _ => UnknownOperandWords,
    };

    private static int GetLittleEnhancedOperandSkipBytes(int opcode) => opcode switch
    {
        (int)AcsPcode.Nop => 0,
        (int)AcsPcode.Terminate or (int)AcsPcode.Suspend => EndScript,
        (int)AcsPcode.PushNumber => 4,
        (int)AcsPcode.Lspec1Direct => 5,
        (int)AcsPcode.DelayDirect => 4,
        (int)AcsPcode.Goto or (int)AcsPcode.IfGoto => 4,
        >= (int)AcsPcode.Lspec1 and <= (int)AcsPcode.Ge => 0,
        >= (int)AcsPcode.AssignScriptVar and <= (int)AcsPcode.PushWorldVar => 1,
        (int)AcsPcode.Drop or (int)AcsPcode.Delay => 0,
        _ => UnknownOperandWords,
    };

    private static bool Reject(
        string reason,
        out IReadOnlyList<MapBehaviorInstruction> instructions,
        out bool terminatedNormally,
        out string? rejectReason)
    {
        instructions = Array.Empty<MapBehaviorInstruction>();
        terminatedNormally = false;
        rejectReason = reason;
        return false;
    }
}
