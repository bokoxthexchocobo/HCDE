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
