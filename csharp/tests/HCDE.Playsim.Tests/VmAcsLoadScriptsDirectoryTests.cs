namespace HCDE.Playsim.Tests;

public class VmAcsLoadScriptsDirectoryTests
{
    [Fact]
    public void DecodeOldScriptNumber_SplitsPacked() =>
        Assert.Equal(42, VmAcsLoadScriptsDirectory.DecodeOldScriptNumber(1042));

    [Fact]
    public void CountEnhancedScriptsBy12_Divides() =>
        Assert.Equal(3, VmAcsLoadScriptsDirectory.CountEnhancedScriptsBy12(36));

    [Fact]
    public void TryResolveDuplicateSwap_ClosedWins()
    {
        Assert.True(VmAcsLoadScriptsDirectory.TryResolveDuplicateSwap(
            new VmAcsLoadScriptsDirectory.EnhancedScriptRecord(1, 0, 0, 0),
            new VmAcsLoadScriptsDirectory.EnhancedScriptRecord(1, VmAcsLoadScriptsDirectory.ScriptClosed, 0, 0),
            out var swap));
        Assert.True(swap);
    }

    [Fact]
    public void SnamIndexToNameTableIndex_Negative() =>
        Assert.Equal(-3, VmAcsLoadScriptsDirectory.SnamIndexToNameTableIndex(2));

    [Fact]
    public void ShouldSkipEnhancedChunks_OldFormat() =>
        Assert.True(VmAcsLoadScriptsDirectory.ShouldSkipEnhancedChunks(VmAcsLoadScriptsDirectory.AcsOld));

    [Fact]
    public void ResolveEnhancedScriptCount_UsesEightByteStrideForMagicHeader()
    {
        Assert.Equal(4, VmAcsLoadScriptsDirectory.ResolveEnhancedScriptCount(0x00494341, 32));
        Assert.Equal(3, VmAcsLoadScriptsDirectory.ResolveEnhancedScriptCount(0, 36));
    }

    [Fact]
    public void RunLoadScriptsDirectory_OldFormat_SortsAndSwapsClosedDuplicate()
    {
        var scratch = new VmAcsLoadScriptsDirectory.ScriptsDirectoryScratch();
        var entries = new[]
        {
            new VmAcsLoadScriptsDirectory.OldScriptRecord(1, 0, 0, 10),
            new VmAcsLoadScriptsDirectory.OldScriptRecord(1, VmAcsLoadScriptsDirectory.ScriptClosed, 0, 20),
            new VmAcsLoadScriptsDirectory.OldScriptRecord(2, 0, 0, 30),
        };
        VmAcsLoadScriptsDirectory.RunLoadScriptsDirectory(scratch, new VmAcsLoadScriptsDirectory.LoadScriptsDirectoryInput(
            VmAcsLoadScriptsDirectory.AcsOld,
            0,
            entries,
            null,
            0,
            null,
            null,
            null,
            null,
            null,
            null));
        Assert.Equal(3, scratch.NumScripts);
        Assert.Equal(VmAcsLoadScriptsDirectory.ScriptClosed, scratch.Scripts[0].Type);
        Assert.True(scratch.Scripts[0].Number <= scratch.Scripts[1].Number);
    }

    [Fact]
    public void ApplySflgChunkEntries_UpdatesFlagsWhenScriptExists()
    {
        var slot = new VmAcsLoadScriptsDirectory.ScriptPtrSlot { Number = 5 };
        var map = new Dictionary<int, VmAcsLoadScriptsDirectory.ScriptPtrSlot> { [5] = slot };
        VmAcsLoadScriptsDirectory.ApplySflgChunkEntries(
            new[] { new VmAcsLoadScriptsDirectory.ChunkFlagEntry(5, 7) },
            map);
        Assert.Equal(7, slot.Flags);
    }
}
