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
                (int)AcsPcode.TranslationRange3,
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
        Assert.Equal((int)AcsPcode.TranslationRange3, instructions[10].Opcode);
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
                (int)AcsPcode.PushFunction, 9,
                (int)AcsPcode.CallStack,
                (int)AcsPcode.GotoStack,
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
        Assert.Equal((int)AcsPcode.PushFunction, instructions[0].Opcode);
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
                (int)AcsPcode.GetPlayerInput,
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
        Assert.Equal((int)AcsPcode.GetPlayerInput, instructions[9].Opcode);
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
                (int)AcsPcode.LsScriptVar,
                (int)AcsPcode.RsGlobalArray,
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
        Assert.Equal((int)AcsPcode.LsScriptVar, instructions[4].Opcode);
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
                (int)AcsPcode.StrCpyToWorldCharRange,
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
                (int)AcsPcode.PushFunction, 1,
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
                (int)AcsPcode.GotoStack,
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
        Assert.Equal((int)AcsPcode.GotoStack, instructions[6].Opcode);
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
                (int)AcsPcode.GotoStack,
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
        Assert.Equal((int)AcsPcode.GotoStack, instructions[8].Opcode);
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
                (int)AcsPcode.PushFunction, 4,
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
        Assert.Equal((int)AcsPcode.PushFunction, instructions[8].Opcode);
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
                (int)AcsPcode.PushFunction, 4,
                (int)AcsPcode.CallStack,
                (int)AcsPcode.GotoStack,
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
        Assert.Equal((int)AcsPcode.PushFunction, instructions[4].Opcode);
        Assert.Equal((int)AcsPcode.CallStack, instructions[5].Opcode);
        Assert.Equal((int)AcsPcode.GotoStack, instructions[6].Opcode);
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
                (int)AcsPcode.GotoStack,
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
        Assert.Equal((int)AcsPcode.GotoStack, instructions[7].Opcode);
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
                (int)AcsPcode.TranslationRange3,
                (int)AcsPcode.PushFunction, 1,
                (int)AcsPcode.ScriptWaitNamed,
                (int)AcsPcode.CallStack,
                (int)AcsPcode.GotoStack,
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
        Assert.Equal((int)AcsPcode.TranslationRange3, instructions[3].Opcode);
        Assert.Equal((int)AcsPcode.PushFunction, instructions[4].Opcode);
        Assert.Equal((int)AcsPcode.CallStack, instructions[6].Opcode);
        Assert.Equal((int)AcsPcode.GotoStack, instructions[7].Opcode);
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
                (int)AcsPcode.GotoStack,
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
