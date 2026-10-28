namespace HCDE.Playsim.Tests;

public class VmFScriptParserScriptWait942SliceTests
{
    [Fact]
    public void WaitDelayTicksFromCentiseconds_ScalesWithTicRate() =>
        Assert.Equal(35, VmFScriptParserScriptWait942Slice.WaitDelayTicksFromCentiseconds(100));

    [Fact]
    public void BuildDelayWait_RejectsWrongArgCount() =>
        Assert.Null(VmFScriptParserScriptWait942Slice.BuildDelayWait(2, 100));

    [Fact]
    public void BuildTagWait_SetsTagWaitKind()
    {
        var suspend = VmFScriptParserScriptWait942Slice.BuildTagWait(1, 4);
        Assert.NotNull(suspend);
        Assert.Equal(VmFScriptScript.WtTagwait, suspend!.Value.WaitType);
    }

    [Fact]
    public void StartScriptNumberInRange_RejectsNegative() =>
        Assert.False(VmFScriptParserScriptWait942Slice.StartScriptNumberInRange(-1));

    [Fact]
    public void BuildStartScript_ReturnsNullWhenChildMissing() =>
        Assert.Null(
            VmFScriptParserScriptWait942Slice.BuildStartScript(1, 3, new object(), null, new object()));

    [Fact]
    public void ScriptRunningReturn_ReturnsOneWhenFound() =>
        Assert.Equal(1, VmFScriptParserScriptWait942Slice.ScriptRunningReturn(true));

    [Fact]
    public void BuildScriptWaitPre_ThrowsTerminatorFlag() =>
        Assert.True(VmFScriptParserScriptWait942Slice.BuildScriptWaitPre(1, 9)!.Value.ThrowsTerminator);
}
