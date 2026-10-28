namespace HCDE.Playsim.Tests;

public class VmPAcsDLevelScriptCtor950SliceTests
{
    [Fact]
    public void AlreadyRunningNonSuspended_WhenRunning() =>
        Assert.True(VmPAcsDLevelScriptCtor950Slice.AlreadyRunningNonSuspended(
            true, VmPAcsIsClientSideScript.ScriptState.Running));

    [Fact]
    public void LocalVarInitCount_ClampsToArgCount() =>
        Assert.Equal(3, VmPAcsDLevelScriptCtor950Slice.LocalVarInitCount(8, 3));

    [Fact]
    public void ShouldTrackInRunningScripts_WhenNotAlways() =>
        Assert.True(VmPAcsDLevelScriptCtor950Slice.ShouldTrackInRunningScripts(0));

    [Fact]
    public void ActivationBackSide_ReadsFlag() =>
        Assert.True(VmPAcsDLevelScriptCtor950Slice.ActivationBackSide(
            VmPLnspecArgMacros.AcsBacksideFlag(true)));

    [Fact]
    public void FlagsForDeferredExecuteAlways_SetsAlwaysBit() =>
        Assert.Equal(VmPLnspecArgMacros.AcsAlwaysFlag,
            VmPAcsDLevelScriptCtor950Slice.FlagsForDeferredExecute(
                VmPAcsDLevelScriptCtor950Slice.DeferExecuteAlways));

    [Fact]
    public void CopyDeferredArgs_PadsWithZeros()
    {
        Span<int> dest = stackalloc int[4];
        VmPAcsDLevelScriptCtor950Slice.CopyDeferredArgs(dest, new[] { 1, 2 });
        Assert.Equal(0, dest[3]);
    }

    [Fact]
    public void ResolveDeferredPlayerNum_MissingWhenNoPlayer() =>
        Assert.Equal(-1, VmPAcsDLevelScriptCtor950Slice.ResolveDeferredPlayerNum(new object(), null, 0));

    [Fact]
    public void ShouldPutLastForHexenHack_ServerMapOnly() =>
        Assert.True(VmPAcsDLevelScriptCtor950Slice.ShouldPutLastForHexenHack(false, true));

    [Fact]
    public void IsDeferredExecuteType_IncludesAlways() =>
        Assert.True(VmPAcsDLevelScriptCtor950Slice.IsDeferredExecuteType(
            VmPAcsDLevelScriptCtor950Slice.DeferExecuteAlways));
}
