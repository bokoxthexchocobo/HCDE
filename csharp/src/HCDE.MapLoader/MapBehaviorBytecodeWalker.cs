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
            var operandBytes = TryGetLittleEnhancedOperandSkipBytes(data, offset, opcode, opcodeBytes, out var skipBytes)
                ? skipBytes
                : GetLittleEnhancedOperandSkipBytes(opcode);
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
        (int)AcsPcode.Goto or (int)AcsPcode.IfGoto or (int)AcsPcode.IfNotGoto => 1,
        (int)AcsPcode.ScriptWaitDirect => 1,
        (int)AcsPcode.CaseGoto => 2,
        >= (int)AcsPcode.Lspec1 and <= (int)AcsPcode.Ge => 0,
        >= (int)AcsPcode.AssignScriptVar and <= (int)AcsPcode.DecWorldVar => 1,
        (int)AcsPcode.Drop or (int)AcsPcode.Delay => 0,
        >= (int)AcsPcode.Add and <= (int)AcsPcode.Restart => 0,
        >= (int)AcsPcode.AndLogical and <= (int)AcsPcode.UnaryMinus => 0,
        (int)AcsPcode.LineSide or (int)AcsPcode.ScriptWait => 0,
        (int)AcsPcode.ClearLineSpecial => 0,
        (int)AcsPcode.BeginPrint or (int)AcsPcode.EndPrint or (int)AcsPcode.EndPrintBold => 0,
        (int)AcsPcode.PrintString or (int)AcsPcode.PrintNumber or (int)AcsPcode.PrintCharacter
            or (int)AcsPcode.PrintFixed or (int)AcsPcode.PrintLocalized or (int)AcsPcode.PrintName => 0,
        >= (int)AcsPcode.PlayerCount and <= (int)AcsPcode.ThingSound => 0,
        >= (int)AcsPcode.ActivatorSound and <= (int)AcsPcode.LocalAmbientSound => 0,
        (int)AcsPcode.Lspec6 or (int)AcsPcode.Lspec6Direct => 0,
        (int)AcsPcode.SpawnDirect => 6,
        (int)AcsPcode.SpawnSpotDirect => 4,
        (int)AcsPcode.ConsoleCommandDirect => 3,
        (int)AcsPcode.ConsoleCommand => 0,
        (int)AcsPcode.FixedMul or (int)AcsPcode.FixedDiv => 0,
        (int)AcsPcode.SetGravity or (int)AcsPcode.SetAirControl => 0,
        (int)AcsPcode.SetGravityDirect or (int)AcsPcode.SetAirControlDirect => 1,
        (int)AcsPcode.AssignGlobalVar or (int)AcsPcode.PushGlobalVar => 1,
        (int)AcsPcode.StartTranslation
            or (int)AcsPcode.TranslationRange1 or (int)AcsPcode.TranslationRange2
            or (int)AcsPcode.TranslationRange3 or (int)AcsPcode.TranslationRange4
            or (int)AcsPcode.TranslationRange5 or (int)AcsPcode.EndTranslation => 0,
        (int)AcsPcode.Call or (int)AcsPcode.CallDiscard => 1,
        (int)AcsPcode.ReturnVoid or (int)AcsPcode.ReturnVal => 0,
        (int)AcsPcode.PushMapArray or (int)AcsPcode.AssignMapArray
            or (int)AcsPcode.AddMapArray or (int)AcsPcode.SubMapArray
            or (int)AcsPcode.MulMapArray or (int)AcsPcode.DivMapArray
            or (int)AcsPcode.ModMapArray or (int)AcsPcode.IncMapArray
            or (int)AcsPcode.DecMapArray => 1,
        (int)AcsPcode.PushWorldArray or (int)AcsPcode.AssignWorldArray
            or (int)AcsPcode.AddWorldArray or (int)AcsPcode.SubWorldArray
            or (int)AcsPcode.MulWorldArray or (int)AcsPcode.DivWorldArray
            or (int)AcsPcode.ModWorldArray or (int)AcsPcode.IncWorldArray
            or (int)AcsPcode.DecWorldArray => 1,
        (int)AcsPcode.PushGlobalArray or (int)AcsPcode.AssignGlobalArray
            or (int)AcsPcode.AddGlobalArray => 1,
        (int)AcsPcode.GiveInventoryDirect or (int)AcsPcode.TakeInventoryDirect
            or (int)AcsPcode.CheckInventoryDirect => 2,
        (int)AcsPcode.SetMusic or (int)AcsPcode.LocalSetMusic or (int)AcsPcode.MusicChange => 0,
        (int)AcsPcode.SetMusicDirect or (int)AcsPcode.LocalSetMusicDirect => 3,
        (int)AcsPcode.MoreHudMessage or (int)AcsPcode.OptHudMessage
            or (int)AcsPcode.EndHudMessage or (int)AcsPcode.EndHudMessageBold
            or (int)AcsPcode.SetStyle or (int)AcsPcode.SetFont => 0,
        (int)AcsPcode.SetStyleDirect => 2,
        (int)AcsPcode.SetFontDirect => 1,
        (int)AcsPcode.GiveInventory or (int)AcsPcode.ClearInventory => 0,
        (int)AcsPcode.Lspec1DirectB => 1,
        (int)AcsPcode.Lspec2DirectB => 1,
        (int)AcsPcode.Lspec3DirectB => 1,
        (int)AcsPcode.Lspec4DirectB => 2,
        (int)AcsPcode.Lspec5DirectB => 2,
        (int)AcsPcode.DelayDirectB or (int)AcsPcode.RandomDirectB => 1,
        (int)AcsPcode.Dup or (int)AcsPcode.Swap => 0,
        _ => UnknownOperandWords,
    };

    private static bool TryGetLittleEnhancedOperandSkipBytes(
        ReadOnlySpan<byte> data,
        int offset,
        int opcode,
        int opcodeBytes,
        out int operandBytes)
    {
        operandBytes = 0;
        if (opcode != (int)AcsPcode.PushBytes)
            return false;

        var countOffset = offset + opcodeBytes;
        if (countOffset >= data.Length)
            return false;

        operandBytes = 1 + data[countOffset];
        return true;
    }

    private static int GetLittleEnhancedOperandSkipBytes(int opcode) => opcode switch
    {
        (int)AcsPcode.Nop => 0,
        (int)AcsPcode.Terminate or (int)AcsPcode.Suspend => EndScript,
        (int)AcsPcode.PushNumber => 4,
        (int)AcsPcode.Lspec1Direct => 5,
        (int)AcsPcode.Lspec2Direct => 6,
        (int)AcsPcode.Lspec3Direct => 7,
        (int)AcsPcode.Lspec4Direct => 8,
        (int)AcsPcode.Lspec5Direct => 9,
        (int)AcsPcode.DelayDirect => 4,
        (int)AcsPcode.Goto or (int)AcsPcode.IfGoto or (int)AcsPcode.IfNotGoto => 4,
        (int)AcsPcode.ScriptWaitDirect => 4,
        (int)AcsPcode.CaseGoto => 8,
        >= (int)AcsPcode.Lspec1 and <= (int)AcsPcode.Ge => 0,
        >= (int)AcsPcode.AssignScriptVar and <= (int)AcsPcode.DecWorldVar => 1,
        (int)AcsPcode.Drop or (int)AcsPcode.Delay => 0,
        >= (int)AcsPcode.Add and <= (int)AcsPcode.Restart => 0,
        >= (int)AcsPcode.AndLogical and <= (int)AcsPcode.UnaryMinus => 0,
        (int)AcsPcode.LineSide or (int)AcsPcode.ScriptWait => 0,
        (int)AcsPcode.ClearLineSpecial => 0,
        (int)AcsPcode.BeginPrint or (int)AcsPcode.EndPrint or (int)AcsPcode.EndPrintBold => 0,
        (int)AcsPcode.PrintString or (int)AcsPcode.PrintNumber or (int)AcsPcode.PrintCharacter
            or (int)AcsPcode.PrintFixed or (int)AcsPcode.PrintLocalized or (int)AcsPcode.PrintName => 0,
        >= (int)AcsPcode.PlayerCount and <= (int)AcsPcode.ThingSound => 0,
        >= (int)AcsPcode.ActivatorSound and <= (int)AcsPcode.LocalAmbientSound => 0,
        (int)AcsPcode.Lspec6 or (int)AcsPcode.Lspec6Direct => 0,
        (int)AcsPcode.SpawnDirect => 24,
        (int)AcsPcode.SpawnSpotDirect => 16,
        (int)AcsPcode.ConsoleCommandDirect => 12,
        (int)AcsPcode.ConsoleCommand => 0,
        (int)AcsPcode.FixedMul or (int)AcsPcode.FixedDiv => 0,
        (int)AcsPcode.SetGravity or (int)AcsPcode.SetAirControl => 0,
        (int)AcsPcode.SetGravityDirect or (int)AcsPcode.SetAirControlDirect => 4,
        (int)AcsPcode.AssignGlobalVar or (int)AcsPcode.PushGlobalVar => 4,
        (int)AcsPcode.StartTranslation
            or (int)AcsPcode.TranslationRange1 or (int)AcsPcode.TranslationRange2
            or (int)AcsPcode.TranslationRange3 or (int)AcsPcode.TranslationRange4
            or (int)AcsPcode.TranslationRange5 or (int)AcsPcode.EndTranslation => 0,
        (int)AcsPcode.Call or (int)AcsPcode.CallDiscard => 1,
        (int)AcsPcode.ReturnVoid or (int)AcsPcode.ReturnVal => 0,
        (int)AcsPcode.PushMapArray or (int)AcsPcode.AssignMapArray
            or (int)AcsPcode.AddMapArray or (int)AcsPcode.SubMapArray
            or (int)AcsPcode.MulMapArray or (int)AcsPcode.DivMapArray
            or (int)AcsPcode.ModMapArray or (int)AcsPcode.IncMapArray
            or (int)AcsPcode.DecMapArray => 1,
        (int)AcsPcode.PushWorldArray or (int)AcsPcode.AssignWorldArray
            or (int)AcsPcode.AddWorldArray or (int)AcsPcode.SubWorldArray
            or (int)AcsPcode.MulWorldArray or (int)AcsPcode.DivWorldArray
            or (int)AcsPcode.ModWorldArray or (int)AcsPcode.IncWorldArray
            or (int)AcsPcode.DecWorldArray => 1,
        (int)AcsPcode.PushGlobalArray or (int)AcsPcode.AssignGlobalArray
            or (int)AcsPcode.AddGlobalArray => 1,
        (int)AcsPcode.GiveInventoryDirect or (int)AcsPcode.TakeInventoryDirect
            or (int)AcsPcode.CheckInventoryDirect => 8,
        (int)AcsPcode.SetMusic or (int)AcsPcode.LocalSetMusic or (int)AcsPcode.MusicChange => 0,
        (int)AcsPcode.SetMusicDirect or (int)AcsPcode.LocalSetMusicDirect => 12,
        (int)AcsPcode.MoreHudMessage or (int)AcsPcode.OptHudMessage
            or (int)AcsPcode.EndHudMessage or (int)AcsPcode.EndHudMessageBold
            or (int)AcsPcode.SetStyle or (int)AcsPcode.SetFont => 0,
        (int)AcsPcode.SetStyleDirect => 8,
        (int)AcsPcode.SetFontDirect => 4,
        (int)AcsPcode.GiveInventory or (int)AcsPcode.ClearInventory => 0,
        (int)AcsPcode.Lspec1DirectB => 2,
        (int)AcsPcode.Lspec2DirectB => 3,
        (int)AcsPcode.Lspec3DirectB => 4,
        (int)AcsPcode.Lspec4DirectB => 5,
        (int)AcsPcode.Lspec5DirectB => 6,
        (int)AcsPcode.DelayDirectB => 1,
        (int)AcsPcode.RandomDirectB => 2,
        (int)AcsPcode.PushByte => 1,
        (int)AcsPcode.Push2Bytes => 2,
        (int)AcsPcode.Push3Bytes => 3,
        (int)AcsPcode.Push4Bytes => 4,
        (int)AcsPcode.Push5Bytes => 5,
        (int)AcsPcode.Dup or (int)AcsPcode.Swap => 0,
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
