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
