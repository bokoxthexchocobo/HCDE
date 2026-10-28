namespace HCDE.Playsim.Tests;

public class VmPAcsStartScript951SliceTests
{
    [Fact]
    public void MapNamesMatchForScript_IgnoresCaseWithinEightChars()
    {
        Assert.True(VmPAcsStartScript951Slice.MapNamesMatchForScript("MAP01ABC".AsSpan(), "map01abc".AsSpan()));
    }

    [Fact]
    public void ShouldDeferStartOnOtherMap_WhenTargetDiffers()
    {
        Assert.True(VmPAcsStartScript951Slice.ShouldDeferStartOnOtherMap("MAP01".AsSpan(), "MAP02".AsSpan()));
    }

    [Fact]
    public void DenyNonNetScriptRequest_BlocksNetPukeInNetgame()
    {
        Assert.True(VmPAcsStartScript951Slice.DenyNonNetScriptRequest(
            VmPAcsStartScript951Slice.AcsNetFlag,
            0,
            false,
            false,
            true,
            true,
            false));
    }

    [Fact]
    public void DenyNonNetScriptRequest_AllowsLocalConsoleRequester()
    {
        Assert.False(VmPAcsStartScript951Slice.DenyNonNetScriptRequest(
            VmPAcsStartScript951Slice.AcsNetFlag,
            0,
            false,
            false,
            false,
            true,
            true));
    }

    [Fact]
    public void DeferredExecuteTypeForStartFlags_UsesAlwaysDefer()
    {
        Assert.Equal(
            VmPAcsDLevelScriptCtor950Slice.DeferExecuteAlways,
            VmPAcsStartScript951Slice.DeferredExecuteTypeForStartFlags(VmPLnspecArgMacros.AcsAlwaysFlag));
    }

    [Fact]
    public void StartScriptResultWhenRunning_ReturnsRunScriptWhenWantResult()
    {
        Assert.Equal(42, VmPAcsStartScript951Slice.StartScriptResultWhenRunning(true, 42, true));
        Assert.Equal(1, VmPAcsStartScript951Slice.StartScriptResultWhenRunning(false, 42, true));
    }

    [Fact]
    public void SuspendOrTerminateOnOtherMap_WhenMapsDiffer()
    {
        Assert.True(VmPAcsStartScript951Slice.SuspendOrTerminateOnOtherMap("MAP01".AsSpan(), "MAP02".AsSpan()));
    }

    [Fact]
    public void ScriptStateNameIndex_AlignsWithNativeStateNames()
    {
        Assert.Equal(7, VmPAcsStartScript951Slice.ScriptStateNameIndex(
            VmPAcsIsClientSideScript.ScriptState.PleaseRemove));
        Assert.Equal("PleaseRemove", VmPAcsStartScript951Slice.ScriptStateName(
            VmPAcsIsClientSideScript.ScriptState.PleaseRemove));
    }

    [Fact]
    public void ResolveDeferredPlayerNumOnAdd_MissingWithoutPlayer()
    {
        Assert.False(VmPAcsStartScript951Slice.ResolveDeferredPlayerNumOnAdd(new object(), null, 0, out var num));
        Assert.Equal(-1, num);
    }
}
