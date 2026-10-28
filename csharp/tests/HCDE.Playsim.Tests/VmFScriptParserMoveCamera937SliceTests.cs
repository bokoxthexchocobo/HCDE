namespace HCDE.Playsim.Tests;

public class VmFScriptParserMoveCamera937SliceTests
{
    [Fact]
    public void MoveToward_ClampsToSpeed()
    {
        var (x, _, _) = VmFScriptParserMoveCamera937Slice.MoveToward(0, 0, 0, 10, 0, 0, 4);
        Assert.Equal(4, x);
    }

    [Fact]
    public void BuildMoveCamera_RejectsNullTarget()
    {
        Assert.Null(VmFScriptParserMoveCamera937Slice.BuildMoveCamera(
            new object(), null, 0, 0, 0, 0, 1, 1, 5, 0, 90, 5));
    }

    [Fact]
    public void BuildMoveCamera_ReturnsZeroWhenFinished()
    {
        var invoke = VmFScriptParserMoveCamera937Slice.BuildMoveCamera(
            new object(), new object(), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        Assert.NotNull(invoke);
        Assert.Equal(0, invoke!.Value.ReturnCode);
    }

    [Fact]
    public void AwakenActor_UsesTriggerWhenNoArgs()
    {
        var trigger = new object();
        Assert.Same(trigger, VmFScriptParserMoveCamera937Slice.AwakenActor(0, null, trigger));
    }

    [Fact]
    public void DeltaAngleDegrees_WrapsAcrossZero()
    {
        Assert.Equal(-90, VmFScriptParserMoveCamera937Slice.DeltaAngleDegrees(0, 270));
    }

    [Fact]
    public void SetWeaponRequiresTwoArgs() =>
        Assert.True(VmFScriptParserMoveCamera937Slice.SetWeaponRequiresTwoArgs(2));

    [Fact]
    public void MobjValueRequiresArgs() =>
        Assert.True(VmFScriptParserMoveCamera937Slice.MobjValueRequiresArgs(1));
}
