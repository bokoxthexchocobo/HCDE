namespace HCDE.Playsim.Tests;

public class VmActionMonsterRefireTests
{
    [Fact]
    public void RefireRollBelowProbability() =>
        Assert.True(VmActionMonsterRefire.RefireRollBelowProbability(5, 10));

    [Fact]
    public void RefireShouldJump_WhenNoTarget() =>
        Assert.True(VmActionMonsterRefire.RefireShouldJump(null, false, 1, true));

    [Fact]
    public void ApplyAngleWhenRef_RequiresNonNull() =>
        Assert.True(VmActionMonsterRefire.ApplyAngleWhenRef(new object(), true));

    [Fact]
    public void ApplyPitchWhenRef_RequiresNonNull() =>
        Assert.True(VmActionMonsterRefire.ApplyPitchWhenRef(new object()));
}
