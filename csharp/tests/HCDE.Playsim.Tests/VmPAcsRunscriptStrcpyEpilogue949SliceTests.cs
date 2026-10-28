using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class VmPAcsRunscriptStrcpyEpilogue949SliceTests
{
    [Fact]
    public void StrCpyDestinationIndex_AddsOffset() =>
        Assert.Equal(7, VmPAcsRunscriptStrcpyEpilogue949Slice.StrCpyDestinationIndex(3, 4));

    [Fact]
    public void StrCpyRangeArgsInvalid_WhenSourceOffsetNegative() =>
        Assert.True(VmPAcsRunscriptStrcpyEpilogue949Slice.StrCpyRangeArgsInvalid(1, -1));

    [Fact]
    public void MapCharRangeArrayWritable_RequiresPositiveIdAndMapVar() =>
        Assert.True(VmPAcsRunscriptStrcpyEpilogue949Slice.MapCharRangeArrayWritable(4, true));

    [Fact]
    public void IsStrCpyToScriptCharRange_MatchesOpcode() =>
        Assert.True(VmPAcsRunscriptStrcpyEpilogue949Slice.IsStrCpyToScriptCharRange((int)AcsPcode.StrCpyToScriptCharRange));

    [Fact]
    public void ApplyConsoleCommandStackPop_DropsThreeSlots()
    {
        var sp = 6;
        VmPAcsRunscriptStrcpyEpilogue949Slice.ApplyConsoleCommandStackPop(ref sp);
        Assert.Equal(3, sp);
    }

    [Fact]
    public void ShouldRestoreSavedBehavior_OnDivideByZero() =>
        Assert.True(VmPAcsRunscriptStrcpyEpilogue949Slice.ShouldRestoreSavedBehavior(
            VmPAcsIsClientSideScript.ScriptState.DivideBy0));

    [Fact]
    public void ResumeSuspendedRunningScript_WhenSuspended() =>
        Assert.True(VmPAcsRunscriptStrcpyEpilogue949Slice.ResumeSuspendedRunningScript(
            VmPAcsIsClientSideScript.ScriptState.Suspended));

    [Fact]
    public void SkipNewScriptWhenAlreadyRunning_WithoutAlwaysFlag() =>
        Assert.True(VmPAcsRunscriptStrcpyEpilogue949Slice.SkipNewScriptWhenAlreadyRunning(false, true));

    [Fact]
    public void AdvanceStringOffset_ClampsPastEnd() =>
        Assert.Equal(3, VmPAcsRunscriptStrcpyEpilogue949Slice.AdvanceStringOffset("abc", 9));
}
