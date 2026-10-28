namespace HCDE.Scripting.Tests;

public class ZccAssignResolveTests
{
    [Fact]
    public void BoolModifyAssignInvalid_BlocksBoolBase()
    {
        Assert.True(ZccAssignResolve.BoolModifyAssignInvalid(true, true, false));
        Assert.False(ZccAssignResolve.BoolModifyAssignInvalid(true, true, true));
    }

    [Fact]
    public void DynArrayAssignPolicy_RewritesAt4111()
    {
        Assert.Equal(
            ZccAssignResolve.AssignOutcome.RewriteToCopyCall,
            ZccAssignResolve.DynArrayAssignPolicy(true, ZccTypeCastResolve.MakeVersion(4, 11, 1), false, 4, 11, 1));
    }

    [Fact]
    public void MapAssignPolicy_WarnsOnPointerSwapBelow4111()
    {
        Assert.Equal(
            ZccAssignResolve.AssignOutcome.WarnPointerSwapOnly,
            ZccAssignResolve.MapAssignPolicy(true, 40000, true, 4, 11, 1));
    }

    [Fact]
    public void ClassifyContainerAssign_MapIteratorFails()
    {
        Assert.Equal(
            ZccAssignResolve.AssignOutcome.Fail,
            ZccAssignResolve.ClassifyContainerAssign(false, false, true, false, 50000, false));
    }

    [Fact]
    public void NormalizeBitWriteIndex_StripsInternalFlag()
    {
        Assert.Equal(3, ZccAssignResolve.NormalizeBitWriteIndex(0x10003));
    }

    [Fact]
    public void RunAssignResolvePipeline_BlocksArrayAssign()
    {
        var input = new ZccAssignResolve.AssignResolveInput(
            false, false, false, false, false, true, false,
            false, false, false, true, false, false, true, false, false,
            false, false, false, false, 50000, false, true, true, -1);
        var result = ZccAssignResolve.RunAssignResolvePipeline(input);
        Assert.Equal(ZccAssignResolve.AssignOutcome.Fail, result.Outcome);
        Assert.True(result.ArrayAssignBlocked);
    }

    [Fact]
    public void RunAssignResolvePipeline_NormalizesBitWrite()
    {
        var input = new ZccAssignResolve.AssignResolveInput(
            false, false, false, false, false, true, false,
            false, false, false, true, false, false, false, false, false,
            false, false, false, false, 50000, false, true, true, 0x10005);
        var result = ZccAssignResolve.RunAssignResolvePipeline(input);
        Assert.Equal(5, result.NormalizedBitWrite);
    }
}
