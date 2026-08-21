namespace HCDE.MapLoader.Tests;

public class MapBehaviorBytecodeWalkerTests
{
    [Fact]
    public void TryWalkScript_OldFormat_ReadsPushNumberAndTerminate()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: true);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(2, instructions.Count);
        Assert.Equal((int)AcsPcode.PushNumber, instructions[0].Opcode);
        Assert.Equal(1, instructions[0].OperandWordCount);
        Assert.Equal((int)AcsPcode.Terminate, instructions[1].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsIfNotGotoScriptWaitAndLspec6()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                (int)AcsPcode.PushNumber, 1,
                (int)AcsPcode.IfNotGoto, 20,
                (int)AcsPcode.ScriptWaitDirect, 5,
                (int)AcsPcode.Lspec6,
                (int)AcsPcode.CaseGoto, 7, 30,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(6, instructions.Count);
        Assert.Equal((int)AcsPcode.IfNotGoto, instructions[1].Opcode);
        Assert.Equal(1, instructions[1].OperandWordCount);
        Assert.Equal((int)AcsPcode.ScriptWaitDirect, instructions[2].Opcode);
        Assert.Equal(1, instructions[2].OperandWordCount);
        Assert.Equal((int)AcsPcode.Lspec6, instructions[3].Opcode);
        Assert.Equal(0, instructions[3].OperandWordCount);
        Assert.Equal((int)AcsPcode.CaseGoto, instructions[4].Opcode);
        Assert.Equal(2, instructions[4].OperandWordCount);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsPrintStackAndDirectSpecials()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                (int)AcsPcode.BeginPrint,
                (int)AcsPcode.PushNumber, 7,
                (int)AcsPcode.PrintNumber,
                (int)AcsPcode.PushNumber, 65,
                (int)AcsPcode.PrintCharacter,
                (int)AcsPcode.EndPrint,
                (int)AcsPcode.GiveInventoryDirect, 1001, 2,
                (int)AcsPcode.ConsoleCommandDirect, 2001, 2002, 2003,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(9, instructions.Count);
        Assert.Equal((int)AcsPcode.BeginPrint, instructions[0].Opcode);
        Assert.Equal((int)AcsPcode.PrintNumber, instructions[2].Opcode);
        Assert.Equal(0, instructions[2].OperandWordCount);
        Assert.Equal((int)AcsPcode.PrintCharacter, instructions[4].Opcode);
        Assert.Equal(0, instructions[4].OperandWordCount);
        Assert.Equal((int)AcsPcode.GiveInventoryDirect, instructions[6].Opcode);
        Assert.Equal(2, instructions[6].OperandWordCount);
        Assert.Equal((int)AcsPcode.ConsoleCommandDirect, instructions[7].Opcode);
        Assert.Equal(3, instructions[7].OperandWordCount);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsHudMessageAndDirectByteSpecials()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                (int)AcsPcode.MoreHudMessage,
                (int)AcsPcode.OptHudMessage,
                (int)AcsPcode.EndHudMessage,
                (int)AcsPcode.SetFontDirect, 9001,
                (int)AcsPcode.Lspec1DirectB, 0x00020001,
                (int)AcsPcode.Lspec2DirectB, 0x00030201,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(7, instructions.Count);
        Assert.Equal((int)AcsPcode.MoreHudMessage, instructions[0].Opcode);
        Assert.Equal((int)AcsPcode.OptHudMessage, instructions[1].Opcode);
        Assert.Equal((int)AcsPcode.EndHudMessage, instructions[2].Opcode);
        Assert.Equal((int)AcsPcode.SetFontDirect, instructions[3].Opcode);
        Assert.Equal(1, instructions[3].OperandWordCount);
        Assert.Equal((int)AcsPcode.Lspec1DirectB, instructions[4].Opcode);
        Assert.Equal(1, instructions[4].OperandWordCount);
        Assert.Equal((int)AcsPcode.Lspec2DirectB, instructions[5].Opcode);
        Assert.Equal(1, instructions[5].OperandWordCount);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsMusicDirectAndStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                (int)AcsPcode.MusicChange,
                (int)AcsPcode.SetMusicDirect, 1001, 2, 0,
                (int)AcsPcode.LocalSetMusicDirect, 2001, 3, 1,
                (int)AcsPcode.Dup,
                (int)AcsPcode.Swap,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(6, instructions.Count);
        Assert.Equal((int)AcsPcode.MusicChange, instructions[0].Opcode);
        Assert.Equal(0, instructions[0].OperandWordCount);
        Assert.Equal((int)AcsPcode.SetMusicDirect, instructions[1].Opcode);
        Assert.Equal(3, instructions[1].OperandWordCount);
        Assert.Equal((int)AcsPcode.LocalSetMusicDirect, instructions[2].Opcode);
        Assert.Equal(3, instructions[2].OperandWordCount);
        Assert.Equal((int)AcsPcode.Dup, instructions[3].Opcode);
        Assert.Equal((int)AcsPcode.Swap, instructions[4].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsGravityAirControlAndGlobalVarOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                (int)AcsPcode.FixedMul,
                (int)AcsPcode.SetGravityDirect, 0x3F800000,
                (int)AcsPcode.SetAirControlDirect, 0x40000000,
                (int)AcsPcode.PushGlobalVar, 12,
                (int)AcsPcode.AssignGlobalVar, 13,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(6, instructions.Count);
        Assert.Equal((int)AcsPcode.FixedMul, instructions[0].Opcode);
        Assert.Equal((int)AcsPcode.SetGravityDirect, instructions[1].Opcode);
        Assert.Equal(1, instructions[1].OperandWordCount);
        Assert.Equal((int)AcsPcode.SetAirControlDirect, instructions[2].Opcode);
        Assert.Equal(1, instructions[2].OperandWordCount);
        Assert.Equal((int)AcsPcode.PushGlobalVar, instructions[3].Opcode);
        Assert.Equal(1, instructions[3].OperandWordCount);
        Assert.Equal((int)AcsPcode.AssignGlobalVar, instructions[4].Opcode);
        Assert.Equal(1, instructions[4].OperandWordCount);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsCallDiscardAndGlobalArrayOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                (int)AcsPcode.Call, 7,
                (int)AcsPcode.CallDiscard, 8,
                (int)AcsPcode.ReturnVoid,
                (int)AcsPcode.PushGlobalArray, 3,
                (int)AcsPcode.AssignGlobalArray, 4,
                (int)AcsPcode.AddGlobalArray, 5,
                (int)AcsPcode.ReturnVal,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(8, instructions.Count);
        Assert.Equal((int)AcsPcode.Call, instructions[0].Opcode);
        Assert.Equal(1, instructions[0].OperandWordCount);
        Assert.Equal((int)AcsPcode.CallDiscard, instructions[1].Opcode);
        Assert.Equal((int)AcsPcode.ReturnVoid, instructions[2].Opcode);
        Assert.Equal(0, instructions[2].OperandWordCount);
        Assert.Equal((int)AcsPcode.PushGlobalArray, instructions[3].Opcode);
        Assert.Equal((int)AcsPcode.AssignGlobalArray, instructions[4].Opcode);
        Assert.Equal((int)AcsPcode.AddGlobalArray, instructions[5].Opcode);
        Assert.Equal((int)AcsPcode.ReturnVal, instructions[6].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsMapWorldArrayAndTranslationOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                (int)AcsPcode.StartTranslation,
                (int)AcsPcode.TranslationRange1,
                (int)AcsPcode.TranslationRange2,
                (int)AcsPcode.EndTranslation,
                (int)AcsPcode.PushMapArray, 1,
                (int)AcsPcode.AssignMapArray, 2,
                (int)AcsPcode.AddMapArray, 3,
                (int)AcsPcode.PushWorldArray, 4,
                (int)AcsPcode.AssignWorldArray, 5,
                (int)AcsPcode.AddWorldArray, 6,
                362, // PCD_TRANSLATIONRANGE3 wire
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(12, instructions.Count);
        Assert.Equal((int)AcsPcode.StartTranslation, instructions[0].Opcode);
        Assert.Equal(0, instructions[0].OperandWordCount);
        Assert.Equal((int)AcsPcode.TranslationRange1, instructions[1].Opcode);
        Assert.Equal((int)AcsPcode.PushMapArray, instructions[4].Opcode);
        Assert.Equal(1, instructions[4].OperandWordCount);
        Assert.Equal((int)AcsPcode.PushWorldArray, instructions[7].Opcode);
        Assert.Equal(1, instructions[7].OperandWordCount);
        Assert.Equal(362, instructions[10].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsScriptArrayAndStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                359, 9,
                (int)AcsPcode.CallStack,
                363,
                (int)AcsPcode.PushScriptArray, 1,
                (int)AcsPcode.AssignScriptArray, 2,
                (int)AcsPcode.AddScriptArray, 3,
                (int)AcsPcode.Dup,
                (int)AcsPcode.Swap,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(9, instructions.Count);
        Assert.Equal(359, instructions[0].Opcode);
        Assert.Equal(1, instructions[0].OperandWordCount);
        Assert.Equal((int)AcsPcode.CallStack, instructions[1].Opcode);
        Assert.Equal(0, instructions[1].OperandWordCount);
        Assert.Equal((int)AcsPcode.PushScriptArray, instructions[3].Opcode);
        Assert.Equal((int)AcsPcode.AddScriptArray, instructions[5].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsScriptCharRangeAndEternityOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                (int)AcsPcode.CallFunc, 2, 99,
                (int)AcsPcode.SaveString,
                (int)AcsPcode.ScriptWaitNamed,
                (int)AcsPcode.PrintScriptCharArray,
                (int)AcsPcode.PrintScriptCharRange,
                (int)AcsPcode.StrCpyToScriptCharRange,
                (int)AcsPcode.PrintMapCharRange,
                (int)AcsPcode.StrCpyToGlobalCharRange,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(9, instructions.Count);
        Assert.Equal((int)AcsPcode.CallFunc, instructions[0].Opcode);
        Assert.Equal(2, instructions[0].OperandWordCount);
        Assert.Equal((int)AcsPcode.ScriptWaitNamed, instructions[2].Opcode);
        Assert.Equal((int)AcsPcode.PrintScriptCharRange, instructions[4].Opcode);
        Assert.Equal(0, instructions[4].OperandWordCount);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsInventoryAndGlobalArrayOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                (int)AcsPcode.TakeInventory,
                (int)AcsPcode.CheckInventory,
                (int)AcsPcode.PlayerHealth,
                (int)AcsPcode.IsNetworkGame,
                (int)AcsPcode.SubGlobalArray, 3,
                (int)AcsPcode.IncGlobalArray, 5,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(7, instructions.Count);
        Assert.Equal((int)AcsPcode.TakeInventory, instructions[0].Opcode);
        Assert.Equal(0, instructions[0].OperandWordCount);
        Assert.Equal((int)AcsPcode.SubGlobalArray, instructions[4].Opcode);
        Assert.Equal(1, instructions[4].OperandWordCount);
    }

    [Fact]
    public void TryWalkScript_LittleEnhanced_ReadsInventoryAndGlobalArrayOps()
    {
        var bytecode = new byte[]
        {
            240, 7, 42,
            (byte)AcsPcode.TakeInventory,
            (byte)AcsPcode.CheckInventory,
            240, 2, 3,
            240, 8, 9,
            (byte)AcsPcode.Terminate,
        };
        var lump = TestWadBuilder.BuildBehaviorLumpWithBytecode(
            MapBehaviorFormat.AcsLittleEnhanced,
            scriptCount: 1,
            bytecode);

        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(6, instructions.Count);
        Assert.Equal((int)AcsPcode.PushByte, instructions[0].Opcode);
        Assert.Equal((int)AcsPcode.TakeInventory, instructions[1].Opcode);
        Assert.Equal((int)AcsPcode.SubGlobalArray, instructions[3].Opcode);
        Assert.Equal(1, instructions[3].OperandWordCount);
        Assert.Equal((int)AcsPcode.DecGlobalArray, instructions[4].Opcode);
        Assert.Equal(1, instructions[4].OperandWordCount);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsActorPropertyAndGetterOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                (int)AcsPcode.GetActorAngle,
                (int)AcsPcode.GetActorFloorZ,
                (int)AcsPcode.SetActorProperty,
                (int)AcsPcode.GetActorProperty,
                (int)AcsPcode.SetGravityDirectB, 4,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(6, instructions.Count);
        Assert.Equal((int)AcsPcode.GetActorAngle, instructions[0].Opcode);
        Assert.Equal(0, instructions[0].OperandWordCount);
        Assert.Equal((int)AcsPcode.SetGravityDirectB, instructions[4].Opcode);
        Assert.Equal(1, instructions[4].OperandWordCount);
    }

    [Fact]
    public void TryWalkScript_LittleEnhanced_ReadsDirectSpecialOps()
    {
        var bytecode = new byte[]
        {
            (byte)AcsPcode.SetMusicDirect, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12,
            (byte)AcsPcode.GiveInventoryDirect, 13, 14, 15, 16, 17, 18, 19, 20,
            240, 87, 21, 22, 23, 24,
            240, 42,
            (byte)AcsPcode.Terminate,
        };
        var lump = TestWadBuilder.BuildBehaviorLumpWithBytecode(
            MapBehaviorFormat.AcsLittleEnhanced,
            scriptCount: 1,
            bytecode);

        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(5, instructions.Count);
        Assert.Equal((int)AcsPcode.SetMusicDirect, instructions[0].Opcode);
        Assert.Equal(12, instructions[0].OperandWordCount);
        Assert.Equal((int)AcsPcode.SetGravityDirectB, instructions[2].Opcode);
        Assert.Equal(4, instructions[2].OperandWordCount);
        Assert.Equal((int)AcsPcode.GetActorCeilingZ, instructions[3].Opcode);
        Assert.Equal(0, instructions[3].OperandWordCount);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsActorInventoryOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                (int)AcsPcode.GiveActorInventory,
                (int)AcsPcode.TakeActorInventory,
                (int)AcsPcode.CheckActorInventory,
                (int)AcsPcode.ClearActorInventory,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(5, instructions.Count);
        Assert.Equal((int)AcsPcode.GiveActorInventory, instructions[0].Opcode);
        Assert.Equal(0, instructions[0].OperandWordCount);
        Assert.Equal((int)AcsPcode.ClearActorInventory, instructions[3].Opcode);
    }

    [Fact]
    public void TryWalkScript_LittleEnhanced_ReadsTranslationRangeOps()
    {
        var bytecode = new byte[]
        {
            (byte)AcsPcode.StartTranslation,
            (byte)AcsPcode.TranslationRange1,
            (byte)AcsPcode.TranslationRange2,
            240, 124,
            (byte)AcsPcode.EndTranslation,
            (byte)AcsPcode.Terminate,
        };
        var lump = TestWadBuilder.BuildBehaviorLumpWithBytecode(
            MapBehaviorFormat.AcsLittleEnhanced,
            scriptCount: 1,
            bytecode);

        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(6, instructions.Count);
        Assert.Equal((int)AcsPcode.TranslationRange3, instructions[3].Opcode);
        Assert.Equal((int)AcsPcode.EndTranslation, instructions[4].Opcode);
    }

    [Fact]
    public void TryWalkScript_LittleEnhanced_ReadsScriptArrayShiftFollowUpAndEternityStackOps()
    {
        var bytecode = new byte[]
        {
            240, 136, 1, // PCD_LSSCRIPTARRAY wire (shadow EorScriptArray enum alias)
            240, 137, 2, // PCD_RSSCRIPTARRAY wire (shadow OrScriptArray enum alias)
            (byte)AcsPcode.Terminate,
        };
        var lump = TestWadBuilder.BuildBehaviorLumpWithBytecode(
            MapBehaviorFormat.AcsLittleEnhanced,
            scriptCount: 1,
            bytecode);

        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(3, instructions.Count);
        Assert.Equal(376, instructions[0].Opcode);
        Assert.Equal(1, instructions[0].OperandWordCount);
        Assert.Equal(377, instructions[1].Opcode);
        Assert.Equal(1, instructions[1].OperandWordCount);
    }

    [Fact]
    public void TryWalkScript_LittleEnhanced_ReadsMapArrayShiftFollowUpAndEternityStackOps()
    {
        var bytecode = new byte[]
        {
            240, 157, 1, // PCD_LSMAPARRAY wire (shadow LsMapArray enum alias)
            240, 164, 2, // PCD_RSMAPARRAY wire (shadow RsMapArray enum alias)
            (byte)AcsPcode.Terminate,
        };
        var lump = TestWadBuilder.BuildBehaviorLumpWithBytecode(
            MapBehaviorFormat.AcsLittleEnhanced,
            scriptCount: 1,
            bytecode);

        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(3, instructions.Count);
        Assert.Equal(397, instructions[0].Opcode);
        Assert.Equal(1, instructions[0].OperandWordCount);
        Assert.Equal(404, instructions[1].Opcode);
        Assert.Equal(1, instructions[1].OperandWordCount);
    }

    [Fact]
    public void TryWalkScript_LittleEnhanced_ReadsWorldArrayShiftFollowUpAndEternityStackOps()
    {
        var bytecode = new byte[]
        {
            240, 158, 1, // PCD_LSWORLDARRAY wire (shadow LsWorldArray enum alias)
            240, 165, 2, // PCD_RSWORLDARRAY wire (shadow RsWorldArray enum alias)
            (byte)AcsPcode.Terminate,
        };
        var lump = TestWadBuilder.BuildBehaviorLumpWithBytecode(
            MapBehaviorFormat.AcsLittleEnhanced,
            scriptCount: 1,
            bytecode);

        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(3, instructions.Count);
        Assert.Equal(398, instructions[0].Opcode);
        Assert.Equal(1, instructions[0].OperandWordCount);
        Assert.Equal(405, instructions[1].Opcode);
        Assert.Equal(1, instructions[1].OperandWordCount);
    }

    [Fact]
    public void TryWalkScript_LittleEnhanced_ReadsPushByteAndPushBytes()
    {
        var bytecode = new byte[]
        {
            240, 7, 42,
            (byte)AcsPcode.Push2Bytes, 1, 2,
            (byte)AcsPcode.PushBytes, 2, 9, 8,
            (byte)AcsPcode.Terminate,
        };
        var lump = TestWadBuilder.BuildBehaviorLumpWithBytecode(
            MapBehaviorFormat.AcsLittleEnhanced,
            scriptCount: 1,
            bytecode);

        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(4, instructions.Count);
        Assert.Equal((int)AcsPcode.PushByte, instructions[0].Opcode);
        Assert.Equal(1, instructions[0].OperandWordCount);
        Assert.Equal((int)AcsPcode.Push2Bytes, instructions[1].Opcode);
        Assert.Equal(2, instructions[1].OperandWordCount);
        Assert.Equal((int)AcsPcode.PushBytes, instructions[2].Opcode);
        Assert.Equal(3, instructions[2].OperandWordCount);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsSectorLineAndGlobalStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                (int)AcsPcode.LineSide,
                (int)AcsPcode.SetLineTexture,
                (int)AcsPcode.SetLineBlocking,
                (int)AcsPcode.SetLineSpecial,
                (int)AcsPcode.ClearLineSpecial,
                (int)AcsPcode.SectorSound,
                (int)AcsPcode.SetLineMonsterBlocking,
                (int)AcsPcode.PlayerBlueSkull,
                (int)AcsPcode.SetThingSpecial,
                (int)AcsPcode.AddGlobalVar, 1,
                (int)AcsPcode.Dup,
                (int)AcsPcode.Swap,
                (int)AcsPcode.SectorDamage,
                (int)AcsPcode.ChangeLevel,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(15, instructions.Count);
        Assert.Equal((int)AcsPcode.SetLineMonsterBlocking, instructions[6].Opcode);
        Assert.Equal((int)AcsPcode.AddGlobalVar, instructions[9].Opcode);
        Assert.Equal(1, instructions[9].OperandWordCount);
        Assert.Equal((int)AcsPcode.SectorDamage, instructions[12].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsThingDamageAndUseInventoryOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                (int)AcsPcode.ThingDamage2,
                (int)AcsPcode.UseInventory,
                (int)AcsPcode.UseActorInventory,
                (int)AcsPcode.CheckActorCeilingTexture,
                (int)AcsPcode.CheckActorFloorTexture,
                (int)AcsPcode.GetActorLightLevel,
                (int)AcsPcode.SetMugshotState,
                (int)AcsPcode.ThingCountSector,
                (int)AcsPcode.ThingCountNameSector,
                347, // PCD_GETPLAYERINPUT wire (shadows GetPlayerInput enum alias)
                (int)AcsPcode.PrintBind,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(12, instructions.Count);
        Assert.Equal((int)AcsPcode.ThingDamage2, instructions[0].Opcode);
        Assert.Equal((int)AcsPcode.UseInventory, instructions[1].Opcode);
        Assert.Equal((int)AcsPcode.ThingCountNameSector, instructions[8].Opcode);
        Assert.Equal(347, instructions[9].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsActorPitchStateAndEternityShiftOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                (int)AcsPcode.GetActorPitch,
                (int)AcsPcode.SetActorPitch,
                (int)AcsPcode.SetActorState,
                (int)AcsPcode.NegateBinary,
                (int)AcsPcode.LsWorldVar,
                (int)AcsPcode.RsGlobalVar,
                (int)AcsPcode.MorphActor,
                (int)AcsPcode.UnmorphActor,
                (int)AcsPcode.ClassifyActor,
                (int)AcsPcode.PrintBinary,
                (int)AcsPcode.PrintHex,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(12, instructions.Count);
        Assert.Equal((int)AcsPcode.GetActorPitch, instructions[0].Opcode);
        Assert.Equal((int)AcsPcode.LsWorldVar, instructions[4].Opcode);
        Assert.Equal((int)AcsPcode.ClassifyActor, instructions[8].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsActorLightTextureAndMorphOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                (int)AcsPcode.CheckActorCeilingTexture,
                (int)AcsPcode.CheckActorFloorTexture,
                (int)AcsPcode.GetActorLightLevel,
                (int)AcsPcode.CheckPlayerCamera,
                (int)AcsPcode.SetActorAngle,
                (int)AcsPcode.SetActorPosition,
                (int)AcsPcode.MorphActor,
                (int)AcsPcode.UnmorphActor,
                (int)AcsPcode.ClassifyActor,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(10, instructions.Count);
        Assert.Equal((int)AcsPcode.CheckActorCeilingTexture, instructions[0].Opcode);
        Assert.Equal((int)AcsPcode.GetActorLightLevel, instructions[2].Opcode);
        Assert.Equal((int)AcsPcode.SetActorPosition, instructions[5].Opcode);
        Assert.Equal((int)AcsPcode.ClassifyActor, instructions[8].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsSpawnProjectileAndCharRangeOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                (int)AcsPcode.ThingProjectile2,
                (int)AcsPcode.SpawnProjectile,
                (int)AcsPcode.SpawnSpotFacing,
                (int)AcsPcode.ThingCountName,
                (int)AcsPcode.PrintWorldCharRange,
                357, // PCD_STRCPYTOWORLDCHRANGE wire
                (int)AcsPcode.PrintGlobalCharRange,
                (int)AcsPcode.StrCpyToMapCharRange,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(9, instructions.Count);
        Assert.Equal((int)AcsPcode.ThingProjectile2, instructions[0].Opcode);
        Assert.Equal((int)AcsPcode.SpawnSpotFacing, instructions[2].Opcode);
        Assert.Equal((int)AcsPcode.PrintWorldCharRange, instructions[4].Opcode);
        Assert.Equal((int)AcsPcode.StrCpyToMapCharRange, instructions[7].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsMarineScreenAndEternityStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                (int)AcsPcode.SetMarineWeapon,
                (int)AcsPcode.SetMarineSprite,
                (int)AcsPcode.GetScreenWidth,
                (int)AcsPcode.GetScreenHeight,
                (int)AcsPcode.SetHudSize,
                359, 1,
                (int)AcsPcode.Lspec5Ex,
                (int)AcsPcode.Lspec5ExResult,
                (int)AcsPcode.CallStack,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(10, instructions.Count);
        Assert.Equal((int)AcsPcode.SetMarineWeapon, instructions[0].Opcode);
        Assert.Equal((int)AcsPcode.GetScreenHeight, instructions[3].Opcode);
        Assert.Equal((int)AcsPcode.Lspec5Ex, instructions[6].Opcode);
        Assert.Equal((int)AcsPcode.CallStack, instructions[8].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsHudScreenFollowUpAndStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                (int)AcsPcode.SetHudSize,
                (int)AcsPcode.GetCvar,
                (int)AcsPcode.GetLineRowOffset,
                (int)AcsPcode.SetResultValue,
                (int)AcsPcode.CaseGotoSorted, 1, 10, 20,
                (int)AcsPcode.Lspec5Result,
                363,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(8, instructions.Count);
        Assert.Equal((int)AcsPcode.SetHudSize, instructions[0].Opcode);
        Assert.Equal((int)AcsPcode.GetCvar, instructions[1].Opcode);
        Assert.Equal((int)AcsPcode.GetLineRowOffset, instructions[2].Opcode);
        Assert.Equal((int)AcsPcode.CaseGotoSorted, instructions[4].Opcode);
        Assert.Equal(3, instructions[4].OperandWordCount);
        Assert.Equal((int)AcsPcode.Lspec5Result, instructions[5].Opcode);
        Assert.Equal(363, instructions[6].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsCvarResultFollowUpAndStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                (int)AcsPcode.GetCvar,
                (int)AcsPcode.SetResultValue,
                (int)AcsPcode.GetSectorFloorZ,
                (int)AcsPcode.GetLevelInfo,
                (int)AcsPcode.ChangeSky,
                (int)AcsPcode.PlayerInGame,
                (int)AcsPcode.EndLog,
                (int)AcsPcode.GetAmmoCapacity,
                (int)AcsPcode.CallStack,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(10, instructions.Count);
        Assert.Equal((int)AcsPcode.GetCvar, instructions[0].Opcode);
        Assert.Equal((int)AcsPcode.GetSectorFloorZ, instructions[2].Opcode);
        Assert.Equal((int)AcsPcode.GetLevelInfo, instructions[3].Opcode);
        Assert.Equal((int)AcsPcode.EndLog, instructions[6].Opcode);
        Assert.Equal((int)AcsPcode.CallStack, instructions[8].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsSectorLevelFollowUpAndStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                (int)AcsPcode.GetSectorFloorZ,
                (int)AcsPcode.GetSectorLightLevel,
                (int)AcsPcode.PrintMapCharArray,
                (int)AcsPcode.PrintWorldCharArray,
                (int)AcsPcode.GetLevelInfo,
                (int)AcsPcode.GrabInput,
                (int)AcsPcode.PlayerClass,
                (int)AcsPcode.SaveString,
                363,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(10, instructions.Count);
        Assert.Equal((int)AcsPcode.GetSectorFloorZ, instructions[0].Opcode);
        Assert.Equal((int)AcsPcode.GetSectorLightLevel, instructions[1].Opcode);
        Assert.Equal((int)AcsPcode.PrintMapCharArray, instructions[2].Opcode);
        Assert.Equal((int)AcsPcode.GetLevelInfo, instructions[4].Opcode);
        Assert.Equal((int)AcsPcode.PlayerClass, instructions[6].Opcode);
        Assert.Equal(363, instructions[8].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsInputFollowUpAndBitwiseStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                (int)AcsPcode.GrabInput,
                (int)AcsPcode.SetMousePointer,
                (int)AcsPcode.AndScriptVar, 0,
                (int)AcsPcode.EorMapVar, 1,
                (int)AcsPcode.OrWorldVar, 2,
                308, 3, // PCD_ORGLOBALVAR wire (shadows AddWorldArray enum alias)
                346, // PCD_CHECKPLAYERCAMERA wire
                347, // PCD_GETPLAYERINPUT wire (shadows GetPlayerInput enum alias)
                359, 4,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(10, instructions.Count);
        Assert.Equal((int)AcsPcode.GrabInput, instructions[0].Opcode);
        Assert.Equal((int)AcsPcode.AndScriptVar, instructions[2].Opcode);
        Assert.Equal(346, instructions[6].Opcode);
        Assert.Equal(347, instructions[7].Opcode);
        Assert.Equal(359, instructions[8].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsBitwiseShiftAndEternityStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                313, 0, // PCD_LSSCRIPTVAR wire (shadows IncWorldArray enum alias)
                317, 1, // PCD_LSSCRIPTARRAY wire
                321, 2, // PCD_RSSCRIPTVAR wire
                324, 3, // PCD_RSGLOBALVAR wire (326 shadows GetActorProperty enum alias)
                359, 4,
                (int)AcsPcode.CallStack,
                363,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(8, instructions.Count);
        Assert.Equal(313, instructions[0].Opcode);
        Assert.Equal(1, instructions[0].OperandWordCount);
        Assert.Equal(317, instructions[1].Opcode);
        Assert.Equal(321, instructions[2].Opcode);
        Assert.Equal(324, instructions[3].Opcode);
        Assert.Equal(359, instructions[4].Opcode);
        Assert.Equal((int)AcsPcode.CallStack, instructions[5].Opcode);
        Assert.Equal(363, instructions[6].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsPlayerInfoAndEternityStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                328, // PCD_GETPLAYERINFO wire (shadows SetAirControlDirectB enum alias)
                329, // PCD_CHANGELEVEL wire
                330, // PCD_SECTORDAMAGE wire
                331, // PCD_REPLACETEXTURES wire
                (int)AcsPcode.ScriptWaitNamed,
                (int)AcsPcode.SaveString,
                (int)AcsPcode.CallStack,
                363,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(9, instructions.Count);
        Assert.Equal(328, instructions[0].Opcode);
        Assert.Equal(0, instructions[0].OperandWordCount);
        Assert.Equal(329, instructions[1].Opcode);
        Assert.Equal(330, instructions[2].Opcode);
        Assert.Equal(331, instructions[3].Opcode);
        Assert.Equal((int)AcsPcode.ScriptWaitNamed, instructions[4].Opcode);
        Assert.Equal((int)AcsPcode.CallStack, instructions[6].Opcode);
        Assert.Equal(363, instructions[7].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsNegateActorPitchAndEternityStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                332, // PCD_NEGATEBINARY wire (shadows NegateBinary enum alias)
                333, // PCD_GETACTORPITCH wire
                334, // PCD_SETACTORPITCH wire
                362, // PCD_TRANSLATIONRANGE3 wire
                359, 1,
                (int)AcsPcode.ScriptWaitNamed,
                (int)AcsPcode.CallStack,
                363,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(9, instructions.Count);
        Assert.Equal(332, instructions[0].Opcode);
        Assert.Equal(0, instructions[0].OperandWordCount);
        Assert.Equal(333, instructions[1].Opcode);
        Assert.Equal(334, instructions[2].Opcode);
        Assert.Equal(362, instructions[3].Opcode);
        Assert.Equal(359, instructions[4].Opcode);
        Assert.Equal((int)AcsPcode.CallStack, instructions[6].Opcode);
        Assert.Equal(363, instructions[7].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsPrintBindActorStateAndEternityStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                335, // PCD_PRINTBIND wire (shadows PrintBind enum alias)
                336, // PCD_SETACTORSTATE wire
                (int)AcsPcode.AssignScriptArray, 1,
                (int)AcsPcode.PushScriptArray, 2,
                (int)AcsPcode.AddScriptArray, 3,
                (int)AcsPcode.CallStack,
                363,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(8, instructions.Count);
        Assert.Equal(335, instructions[0].Opcode);
        Assert.Equal(0, instructions[0].OperandWordCount);
        Assert.Equal(336, instructions[1].Opcode);
        Assert.Equal((int)AcsPcode.AssignScriptArray, instructions[2].Opcode);
        Assert.Equal(1, instructions[2].OperandWordCount);
        Assert.Equal((int)AcsPcode.CallStack, instructions[5].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsThingDamageUseInventoryAndEternityStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                335, // PCD_THINGDAMAGE2 wire (shadows PrintBind enum alias)
                336, // PCD_USEINVENTORY wire
                337, // PCD_USEACTORINVENTORY wire (shadows ThingDamage2 enum alias)
                (int)AcsPcode.SubScriptArray, 1,
                (int)AcsPcode.MulScriptArray, 2,
                (int)AcsPcode.DecScriptArray, 3,
                (int)AcsPcode.CallStack,
                363,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(9, instructions.Count);
        Assert.Equal(335, instructions[0].Opcode);
        Assert.Equal(0, instructions[0].OperandWordCount);
        Assert.Equal(336, instructions[1].Opcode);
        Assert.Equal(337, instructions[2].Opcode);
        Assert.Equal((int)AcsPcode.SubScriptArray, instructions[3].Opcode);
        Assert.Equal(1, instructions[3].OperandWordCount);
        Assert.Equal((int)AcsPcode.CallStack, instructions[6].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsActorTextureLightAndEternityStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                420, // PCD_CHECKACTORCEILINGTEXTURE wire
                421, // PCD_CHECKACTORFLOORTEXTURE wire
                342, // PCD_GETACTORLIGHTLEVEL wire
                343, // PCD_SETMUGSHOTSTATE wire (shadows Lspec5Result enum alias)
                (int)AcsPcode.EorScriptArray, 1,
                (int)AcsPcode.OrScriptArray, 2,
                (int)AcsPcode.AndScriptArray, 3,
                (int)AcsPcode.CallStack,
                363,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(10, instructions.Count);
        Assert.Equal(420, instructions[0].Opcode);
        Assert.Equal(0, instructions[0].OperandWordCount);
        Assert.Equal(421, instructions[1].Opcode);
        Assert.Equal(342, instructions[2].Opcode);
        Assert.Equal(343, instructions[3].Opcode);
        Assert.Equal((int)AcsPcode.EorScriptArray, instructions[4].Opcode);
        Assert.Equal(1, instructions[4].OperandWordCount);
        Assert.Equal((int)AcsPcode.CallStack, instructions[7].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsThingCountCameraAndEternityStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                344, // PCD_THINGCOUNTSECTOR wire
                345, // PCD_THINGCOUNTNAMESECTOR wire
                346, // PCD_CHECKPLAYERCAMERA wire
                347, // PCD_GETPLAYERINPUT wire (shadows GetPlayerInput enum alias)
                (int)AcsPcode.MulScriptArray, 1,
                (int)AcsPcode.ModScriptArray, 2,
                (int)AcsPcode.IncScriptArray, 3,
                (int)AcsPcode.CallStack,
                363,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(10, instructions.Count);
        Assert.Equal(344, instructions[0].Opcode);
        Assert.Equal(0, instructions[0].OperandWordCount);
        Assert.Equal(345, instructions[1].Opcode);
        Assert.Equal(346, instructions[2].Opcode);
        Assert.Equal(347, instructions[3].Opcode);
        Assert.Equal((int)AcsPcode.MulScriptArray, instructions[4].Opcode);
        Assert.Equal(1, instructions[4].OperandWordCount);
        Assert.Equal((int)AcsPcode.CallStack, instructions[7].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsScriptArrayFollowUpAndEternityStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                362, // PCD_TRANSLATIONRANGE3 wire
                364, 1, // PCD_ASSIGNSCRIPTARRAY wire
                365, 2, // PCD_PUSHSCRIPTARRAY wire
                366, 3, // PCD_ADDSCRIPTARRAY wire
                (int)AcsPcode.SubScriptArray, 4,
                (int)AcsPcode.MulScriptArray, 5,
                (int)AcsPcode.CallStack,
                363, // PCD_GOTOSTACK wire
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(9, instructions.Count);
        Assert.Equal(362, instructions[0].Opcode);
        Assert.Equal(0, instructions[0].OperandWordCount);
        Assert.Equal(364, instructions[1].Opcode);
        Assert.Equal(1, instructions[1].OperandWordCount);
        Assert.Equal(365, instructions[2].Opcode);
        Assert.Equal(1, instructions[2].OperandWordCount);
        Assert.Equal(366, instructions[3].Opcode);
        Assert.Equal(1, instructions[3].OperandWordCount);
        Assert.Equal((int)AcsPcode.SubScriptArray, instructions[4].Opcode);
        Assert.Equal((int)AcsPcode.CallStack, instructions[6].Opcode);
        Assert.Equal(363, instructions[7].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsScriptCharArrayFollowUpAndEternityStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                378, // PCD_PRINTSCRIPTCHARARRAY wire
                379, // PCD_PRINTSCRIPTCHRANGE wire
                380, // PCD_STRCPYTOSCRIPTCHRANGE wire
                359, 1, // PCD_PUSHFUNCTION wire
                361, // PCD_SCRIPTWAITNAMED wire
                360, // PCD_CALLSTACK wire
                363, // PCD_GOTOSTACK wire
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(8, instructions.Count);
        Assert.Equal(378, instructions[0].Opcode);
        Assert.Equal(0, instructions[0].OperandWordCount);
        Assert.Equal(379, instructions[1].Opcode);
        Assert.Equal(380, instructions[2].Opcode);
        Assert.Equal(359, instructions[3].Opcode);
        Assert.Equal(1, instructions[3].OperandWordCount);
        Assert.Equal(361, instructions[4].Opcode);
        Assert.Equal(360, instructions[5].Opcode);
        Assert.Equal(363, instructions[6].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsScriptArrayAssignFollowUpAndEternityStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                364, 1, // PCD_ASSIGNSCRIPTARRAY wire
                365, 2, // PCD_PUSHSCRIPTARRAY wire
                366, 3, // PCD_ADDSCRIPTARRAY wire
                367, 4, // PCD_SUBSCRIPTARRAY wire
                368, 5, // PCD_MULSCRIPTARRAY wire
                359, 1, // PCD_PUSHFUNCTION wire
                361, // PCD_SCRIPTWAITNAMED wire
                360, // PCD_CALLSTACK wire
                363, // PCD_GOTOSTACK wire
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(10, instructions.Count);
        Assert.Equal(364, instructions[0].Opcode);
        Assert.Equal(1, instructions[0].OperandWordCount);
        Assert.Equal(365, instructions[1].Opcode);
        Assert.Equal(1, instructions[1].OperandWordCount);
        Assert.Equal(366, instructions[2].Opcode);
        Assert.Equal(1, instructions[2].OperandWordCount);
        Assert.Equal(367, instructions[3].Opcode);
        Assert.Equal(1, instructions[3].OperandWordCount);
        Assert.Equal(368, instructions[4].Opcode);
        Assert.Equal(1, instructions[4].OperandWordCount);
        Assert.Equal(359, instructions[5].Opcode);
        Assert.Equal(361, instructions[6].Opcode);
        Assert.Equal(360, instructions[7].Opcode);
        Assert.Equal(363, instructions[8].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsGotoStackFollowUpAndEternityStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                360, // PCD_CALLSTACK wire
                362, // PCD_TRANSLATIONRANGE3 wire
                363, // PCD_GOTOSTACK wire
                359, 1, // PCD_PUSHFUNCTION wire
                361, // PCD_SCRIPTWAITNAMED wire
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(6, instructions.Count);
        Assert.Equal(360, instructions[0].Opcode);
        Assert.Equal(0, instructions[0].OperandWordCount);
        Assert.Equal(362, instructions[1].Opcode);
        Assert.Equal(363, instructions[2].Opcode);
        Assert.Equal(359, instructions[3].Opcode);
        Assert.Equal(1, instructions[3].OperandWordCount);
        Assert.Equal(361, instructions[4].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsTranslationRangeFollowUpAndEternityStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                444, // PCD_TRANSLATIONRANGE3 wire
                465, // PCD_TRANSLATIONRANGE4 wire
                466, // PCD_TRANSLATIONRANGE5 wire
                359, 1, // PCD_PUSHFUNCTION wire
                361, // PCD_SCRIPTWAITNAMED wire
                360, // PCD_CALLSTACK wire
                363, // PCD_GOTOSTACK wire
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(8, instructions.Count);
        Assert.Equal(444, instructions[0].Opcode);
        Assert.Equal(0, instructions[0].OperandWordCount);
        Assert.Equal(465, instructions[1].Opcode);
        Assert.Equal(466, instructions[2].Opcode);
        Assert.Equal(359, instructions[3].Opcode);
        Assert.Equal(1, instructions[3].OperandWordCount);
        Assert.Equal(361, instructions[4].Opcode);
        Assert.Equal(360, instructions[5].Opcode);
        Assert.Equal(363, instructions[6].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsLspec5ExFollowUpAndEternityStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                381, // PCD_LSPEC5EX wire
                382, // PCD_LSPEC5EXRESULT wire
                359, 1, // PCD_PUSHFUNCTION wire
                361, // PCD_SCRIPTWAITNAMED wire
                360, // PCD_CALLSTACK wire
                363, // PCD_GOTOSTACK wire
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(7, instructions.Count);
        Assert.Equal(381, instructions[0].Opcode);
        Assert.Equal(0, instructions[0].OperandWordCount);
        Assert.Equal(382, instructions[1].Opcode);
        Assert.Equal(359, instructions[2].Opcode);
        Assert.Equal(1, instructions[2].OperandWordCount);
        Assert.Equal(361, instructions[3].Opcode);
        Assert.Equal(360, instructions[4].Opcode);
        Assert.Equal(363, instructions[5].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsScriptArrayShiftFollowUpAndEternityStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                317, 1, // PCD_LSSCRIPTARRAY wire (313-324 shift block)
                376, 2, // PCD_LSSCRIPTARRAY wire (shadow EorScriptArray enum alias)
                377, 3, // PCD_RSSCRIPTARRAY wire (shadow OrScriptArray enum alias)
                359, 4, // PCD_PUSHFUNCTION wire
                361, // PCD_SCRIPTWAITNAMED wire
                360, // PCD_CALLSTACK wire
                363, // PCD_GOTOSTACK wire
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(8, instructions.Count);
        Assert.Equal(317, instructions[0].Opcode);
        Assert.Equal(1, instructions[0].OperandWordCount);
        Assert.Equal(376, instructions[1].Opcode);
        Assert.Equal(1, instructions[1].OperandWordCount);
        Assert.Equal(377, instructions[2].Opcode);
        Assert.Equal(1, instructions[2].OperandWordCount);
        Assert.Equal(359, instructions[3].Opcode);
        Assert.Equal(1, instructions[3].OperandWordCount);
        Assert.Equal(361, instructions[4].Opcode);
        Assert.Equal(360, instructions[5].Opcode);
        Assert.Equal(363, instructions[6].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsMapArrayShiftFollowUpAndEternityStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                314, 1, // PCD_LSMAPVAR wire (313-324 shift block)
                397, 2, // PCD_LSMAPARRAY wire (shadow LsMapArray enum alias)
                404, 3, // PCD_RSMAPARRAY wire (shadow RsMapArray enum alias)
                359, 4, // PCD_PUSHFUNCTION wire
                361, // PCD_SCRIPTWAITNAMED wire
                360, // PCD_CALLSTACK wire
                363, // PCD_GOTOSTACK wire
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(8, instructions.Count);
        Assert.Equal(314, instructions[0].Opcode);
        Assert.Equal(1, instructions[0].OperandWordCount);
        Assert.Equal(397, instructions[1].Opcode);
        Assert.Equal(1, instructions[1].OperandWordCount);
        Assert.Equal(404, instructions[2].Opcode);
        Assert.Equal(1, instructions[2].OperandWordCount);
        Assert.Equal(359, instructions[3].Opcode);
        Assert.Equal(1, instructions[3].OperandWordCount);
        Assert.Equal(361, instructions[4].Opcode);
        Assert.Equal(360, instructions[5].Opcode);
        Assert.Equal(363, instructions[6].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsWorldArrayShiftFollowUpAndEternityStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                315, 1, // PCD_LSWORLDVAR wire (313-324 shift block)
                398, 2, // PCD_LSWORLDARRAY wire (shadow LsWorldArray enum alias)
                405, 3, // PCD_RSWORLDARRAY wire (shadow RsWorldArray enum alias)
                359, 4, // PCD_PUSHFUNCTION wire
                361, // PCD_SCRIPTWAITNAMED wire
                360, // PCD_CALLSTACK wire
                363, // PCD_GOTOSTACK wire
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(8, instructions.Count);
        Assert.Equal(315, instructions[0].Opcode);
        Assert.Equal(1, instructions[0].OperandWordCount);
        Assert.Equal(398, instructions[1].Opcode);
        Assert.Equal(1, instructions[1].OperandWordCount);
        Assert.Equal(405, instructions[2].Opcode);
        Assert.Equal(1, instructions[2].OperandWordCount);
        Assert.Equal(359, instructions[3].Opcode);
        Assert.Equal(1, instructions[3].OperandWordCount);
        Assert.Equal(361, instructions[4].Opcode);
        Assert.Equal(360, instructions[5].Opcode);
        Assert.Equal(363, instructions[6].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsScriptVarShiftFollowUpAndEternityStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                313, 1, // PCD_LSSCRIPTVAR wire (313-324 shift block)
                393, 2, // PCD_LSSCRIPTVAR wire (shadow LsScriptVar enum alias)
                400, 3, // PCD_RSSCRIPTVAR wire (shadow RsScriptVar enum alias)
                359, 4, // PCD_PUSHFUNCTION wire
                361, // PCD_SCRIPTWAITNAMED wire
                360, // PCD_CALLSTACK wire
                363, // PCD_GOTOSTACK wire
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(8, instructions.Count);
        Assert.Equal(313, instructions[0].Opcode);
        Assert.Equal(1, instructions[0].OperandWordCount);
        Assert.Equal(393, instructions[1].Opcode);
        Assert.Equal(1, instructions[1].OperandWordCount);
        Assert.Equal(400, instructions[2].Opcode);
        Assert.Equal(1, instructions[2].OperandWordCount);
        Assert.Equal(359, instructions[3].Opcode);
        Assert.Equal(1, instructions[3].OperandWordCount);
        Assert.Equal(361, instructions[4].Opcode);
        Assert.Equal(360, instructions[5].Opcode);
        Assert.Equal(363, instructions[6].Opcode);
    }

    [Fact]
    public void TryWalkScript_LittleEnhanced_ReadsScriptVarShiftFollowUpAndEternityStackOps()
    {
        var bytecode = new byte[]
        {
            240, 153, 1, // PCD_LSSCRIPTVAR wire (shadow LsScriptVar enum alias)
            240, 160, 2, // PCD_RSSCRIPTVAR wire (shadow RsScriptVar enum alias)
            (byte)AcsPcode.Terminate,
        };
        var lump = TestWadBuilder.BuildBehaviorLumpWithBytecode(
            MapBehaviorFormat.AcsLittleEnhanced,
            scriptCount: 1,
            bytecode);

        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(3, instructions.Count);
        Assert.Equal(393, instructions[0].Opcode);
        Assert.Equal(1, instructions[0].OperandWordCount);
        Assert.Equal(400, instructions[1].Opcode);
        Assert.Equal(1, instructions[1].OperandWordCount);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsMapVarShiftFollowUpAndEternityStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                314, 1, // PCD_LSMAPVAR wire (313-324 shift block)
                394, 2, // PCD_LSMAPVAR wire (shadow LsMapVar enum alias)
                401, 3, // PCD_RSMAPVAR wire (shadow RsMapVar enum alias)
                359, 4, // PCD_PUSHFUNCTION wire
                361, // PCD_SCRIPTWAITNAMED wire
                360, // PCD_CALLSTACK wire
                363, // PCD_GOTOSTACK wire
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(8, instructions.Count);
        Assert.Equal(314, instructions[0].Opcode);
        Assert.Equal(1, instructions[0].OperandWordCount);
        Assert.Equal(394, instructions[1].Opcode);
        Assert.Equal(1, instructions[1].OperandWordCount);
        Assert.Equal(401, instructions[2].Opcode);
        Assert.Equal(1, instructions[2].OperandWordCount);
        Assert.Equal(359, instructions[3].Opcode);
        Assert.Equal(1, instructions[3].OperandWordCount);
        Assert.Equal(361, instructions[4].Opcode);
        Assert.Equal(360, instructions[5].Opcode);
        Assert.Equal(363, instructions[6].Opcode);
    }

    [Fact]
    public void TryWalkScript_LittleEnhanced_ReadsMapVarShiftFollowUpAndEternityStackOps()
    {
        var bytecode = new byte[]
        {
            240, 154, 1, // PCD_LSMAPVAR wire (shadow LsMapVar enum alias)
            240, 161, 2, // PCD_RSMAPVAR wire (shadow RsMapVar enum alias)
            (byte)AcsPcode.Terminate,
        };
        var lump = TestWadBuilder.BuildBehaviorLumpWithBytecode(
            MapBehaviorFormat.AcsLittleEnhanced,
            scriptCount: 1,
            bytecode);

        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(3, instructions.Count);
        Assert.Equal(394, instructions[0].Opcode);
        Assert.Equal(1, instructions[0].OperandWordCount);
        Assert.Equal(401, instructions[1].Opcode);
        Assert.Equal(1, instructions[1].OperandWordCount);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsGlobalArrayShiftFollowUpAndEternityStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                316, 1, // PCD_LSGLOBALVAR wire (313-324 shift block)
                399, 2, // PCD_LSGLOBALARRAY wire (shadow LsGlobalArray enum alias)
                406, 3, // PCD_RSGLOBALARRAY wire (shadow RsGlobalArray enum alias)
                359, 4, // PCD_PUSHFUNCTION wire
                361, // PCD_SCRIPTWAITNAMED wire
                360, // PCD_CALLSTACK wire
                363, // PCD_GOTOSTACK wire
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(8, instructions.Count);
        Assert.Equal(316, instructions[0].Opcode);
        Assert.Equal(1, instructions[0].OperandWordCount);
        Assert.Equal(399, instructions[1].Opcode);
        Assert.Equal(1, instructions[1].OperandWordCount);
        Assert.Equal(406, instructions[2].Opcode);
        Assert.Equal(1, instructions[2].OperandWordCount);
        Assert.Equal(359, instructions[3].Opcode);
        Assert.Equal(1, instructions[3].OperandWordCount);
        Assert.Equal(361, instructions[4].Opcode);
        Assert.Equal(360, instructions[5].Opcode);
        Assert.Equal(363, instructions[6].Opcode);
    }

    [Fact]
    public void TryWalkScript_LittleEnhanced_ReadsGlobalArrayShiftFollowUpAndEternityStackOps()
    {
        var bytecode = new byte[]
        {
            240, 159, 1, // PCD_LSGLOBALARRAY wire (shadow LsGlobalArray enum alias)
            240, 166, 2, // PCD_RSGLOBALARRAY wire (shadow RsGlobalArray enum alias)
            (byte)AcsPcode.Terminate,
        };
        var lump = TestWadBuilder.BuildBehaviorLumpWithBytecode(
            MapBehaviorFormat.AcsLittleEnhanced,
            scriptCount: 1,
            bytecode);

        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(3, instructions.Count);
        Assert.Equal(399, instructions[0].Opcode);
        Assert.Equal(1, instructions[0].OperandWordCount);
        Assert.Equal(406, instructions[1].Opcode);
        Assert.Equal(1, instructions[1].OperandWordCount);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsScriptArrayShiftAndEternityStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                376, 1, // PCD_LSSCRIPTARRAY wire
                377, 2, // PCD_RSSCRIPTARRAY wire
                359, 3, // PCD_PUSHFUNCTION wire
                361, // PCD_SCRIPTWAITNAMED wire
                360, // PCD_CALLSTACK wire
                363, // PCD_GOTOSTACK wire
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(7, instructions.Count);
        Assert.Equal(376, instructions[0].Opcode);
        Assert.Equal(1, instructions[0].OperandWordCount);
        Assert.Equal(377, instructions[1].Opcode);
        Assert.Equal(1, instructions[1].OperandWordCount);
        Assert.Equal(359, instructions[2].Opcode);
        Assert.Equal(1, instructions[2].OperandWordCount);
        Assert.Equal(361, instructions[3].Opcode);
        Assert.Equal(0, instructions[3].OperandWordCount);
        Assert.Equal(360, instructions[4].Opcode);
        Assert.Equal(363, instructions[5].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsPushFunctionScriptWaitAndEternityStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                360, // PCD_CALLSTACK wire
                363, // PCD_GOTOSTACK wire
                359, 1,
                (int)AcsPcode.ScriptWaitNamed,
                (int)AcsPcode.DecScriptArray, 2,
                (int)AcsPcode.AndScriptArray, 3,
                (int)AcsPcode.CallStack,
                363,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(9, instructions.Count);
        Assert.Equal(360, instructions[0].Opcode);
        Assert.Equal(0, instructions[0].OperandWordCount);
        Assert.Equal(363, instructions[1].Opcode);
        Assert.Equal(359, instructions[2].Opcode);
        Assert.Equal(1, instructions[2].OperandWordCount);
        Assert.Equal((int)AcsPcode.ScriptWaitNamed, instructions[3].Opcode);
        Assert.Equal((int)AcsPcode.DecScriptArray, instructions[4].Opcode);
        Assert.Equal((int)AcsPcode.CallStack, instructions[6].Opcode);
        Assert.Equal(363, instructions[7].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsCharRangeFollowUpAndEternityStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                354, // PCD_PRINTWORLDCHRANGE wire
                355, // PCD_PRINTGLOBALCHRANGE wire
                356, // PCD_STRCPYTOMAPCHRANGE wire
                357, // PCD_STRCPYTOWORLDCHRANGE wire
                358, // PCD_STRCPYTOGLOBALCHRANGE wire
                (int)AcsPcode.PushScriptArray, 1,
                (int)AcsPcode.IncScriptArray, 2,
                (int)AcsPcode.OrScriptArray, 3,
                (int)AcsPcode.CallStack,
                363,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(11, instructions.Count);
        Assert.Equal(354, instructions[0].Opcode);
        Assert.Equal(0, instructions[0].OperandWordCount);
        Assert.Equal(358, instructions[4].Opcode);
        Assert.Equal((int)AcsPcode.PushScriptArray, instructions[5].Opcode);
        Assert.Equal(1, instructions[5].OperandWordCount);
        Assert.Equal((int)AcsPcode.CallStack, instructions[8].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsCallFuncSaveStringAndEternityStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                351, 2, 99, // PCD_CALLFUNC wire
                352, // PCD_SAVESTRING wire
                (int)AcsPcode.PushScriptArray, 1,
                (int)AcsPcode.MulScriptArray, 2,
                (int)AcsPcode.DivScriptArray, 3,
                (int)AcsPcode.CallStack,
                363,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(8, instructions.Count);
        Assert.Equal(351, instructions[0].Opcode);
        Assert.Equal(2, instructions[0].OperandWordCount);
        Assert.Equal(352, instructions[1].Opcode);
        Assert.Equal(0, instructions[1].OperandWordCount);
        Assert.Equal((int)AcsPcode.PushScriptArray, instructions[2].Opcode);
        Assert.Equal(1, instructions[2].OperandWordCount);
        Assert.Equal((int)AcsPcode.CallStack, instructions[5].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsPrintBinaryHexAndEternityStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                348, // PCD_CLASSIFYACTOR wire
                349, // PCD_PRINTBINARY wire
                350, // PCD_PRINTHEX wire
                (int)AcsPcode.PushScriptArray, 1,
                (int)AcsPcode.AssignScriptArray, 2,
                (int)AcsPcode.SubScriptArray, 3,
                (int)AcsPcode.CallStack,
                363,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(9, instructions.Count);
        Assert.Equal(348, instructions[0].Opcode);
        Assert.Equal(0, instructions[0].OperandWordCount);
        Assert.Equal(349, instructions[1].Opcode);
        Assert.Equal(350, instructions[2].Opcode);
        Assert.Equal((int)AcsPcode.PushScriptArray, instructions[3].Opcode);
        Assert.Equal(1, instructions[3].OperandWordCount);
        Assert.Equal((int)AcsPcode.CallStack, instructions[6].Opcode);
    }

    [Fact]
    public void TryWalkScript_OldFormat_ReadsMorphClassifyAndEternityStackOps()
    {
        var lump = TestWadBuilder.BuildBehaviorLump(
            MapBehaviorFormat.AcsOld,
            scriptCount: 1,
            includeTerminateBytecode: false,
            bytecodeOpcodes:
            [
                427, // PCD_MORPHACTOR wire
                428, // PCD_UNMORPHACTOR wire
                430, // PCD_CLASSIFYACTOR wire
                431, // PCD_PRINTBINARY wire
                432, // PCD_PRINTHEX wire
                (int)AcsPcode.PushScriptArray, 1,
                (int)AcsPcode.AssignScriptArray, 2,
                (int)AcsPcode.AddScriptArray, 3,
                (int)AcsPcode.CallStack,
                363,
                (int)AcsPcode.Terminate,
            ]);
        Assert.True(MapBehaviorCodec.TryProbe(lump, out var record, out _));
        Assert.True(MapBehaviorDirectoryCodec.TryReadScripts(
            record.Data,
            record.Format,
            record.DirectoryOffset,
            out var scripts,
            out _));
        Assert.Single(scripts);

        Assert.True(MapBehaviorBytecodeWalker.TryWalkScript(
            record.Data,
            record.Format,
            scripts[0].Address,
            out var instructions,
            out var terminated,
            out _));

        Assert.True(terminated);
        Assert.Equal(11, instructions.Count);
        Assert.Equal(427, instructions[0].Opcode);
        Assert.Equal(0, instructions[0].OperandWordCount);
        Assert.Equal(428, instructions[1].Opcode);
        Assert.Equal(430, instructions[2].Opcode);
        Assert.Equal(431, instructions[3].Opcode);
        Assert.Equal(432, instructions[4].Opcode);
        Assert.Equal((int)AcsPcode.PushScriptArray, instructions[5].Opcode);
        Assert.Equal(1, instructions[5].OperandWordCount);
        Assert.Equal((int)AcsPcode.CallStack, instructions[8].Opcode);
    }

    [Fact]
    public void TryDecode_IncludesScriptBytecodeBodies()
    {
        var wad = TestWadBuilder.BuildMinimalMapWad("MAP01", includeBehavior: true);
        Assert.True(MapLumpCatalogReader.TryReadMap(wad, "MAP01", out var catalog, out _));
        Assert.True(BinaryMapBehaviorDecoder.TryDecode(wad, catalog, out var behavior, out _));
        Assert.True(behavior.IsPresent);
        Assert.Single(behavior.ScriptBodies);
        Assert.True(behavior.ScriptBodies[0].TerminatedNormally);
        Assert.Equal(2, behavior.ScriptBodies[0].Instructions.Count);
    }
}
