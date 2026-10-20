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
    public void TryResolveDuplicateSwap_ClosedWins() =>
        Assert.True(VmAcsLoadScriptsDirectory.TryResolveDuplicateSwap(
            new VmAcsLoadScriptsDirectory.EnhancedScriptRecord(1, 0, 0, 0),
            new VmAcsLoadScriptsDirectory.EnhancedScriptRecord(1, VmAcsLoadScriptsDirectory.ScriptClosed, 0, 0),
            out var swap));
        Assert.True(swap);

    [Fact]
    public void SnamIndexToNameTableIndex_Negative() =>
        Assert.Equal(-3, VmAcsLoadScriptsDirectory.SnamIndexToNameTableIndex(2));

    [Fact]
    public void ShouldSkipEnhancedChunks_OldFormat() =>
        Assert.True(VmAcsLoadScriptsDirectory.ShouldSkipEnhancedChunks(VmAcsLoadScriptsDirectory.AcsOld));
}
