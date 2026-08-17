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
