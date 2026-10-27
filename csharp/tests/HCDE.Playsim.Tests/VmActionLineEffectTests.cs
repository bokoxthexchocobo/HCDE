namespace HCDE.Playsim.Tests;

public class VmActionLineEffectTests
{
    [Fact]
    public void SkipWhenLinedone_IsSet() =>
        Assert.True(VmActionLineEffect.SkipWhenLinedone(VmActionLineEffect.Mf6Linedone));

    [Fact]
    public void HasLineSpecial_WhenNonZero() =>
        Assert.True(VmActionLineEffect.HasLineSpecial(1));

    [Fact]
    public void MarkLinedoneWhenNonRepeating() =>
        Assert.True(VmActionLineEffect.MarkLinedoneWhenNonRepeating(executed: true, repeatSpecial: false));

    [Fact]
    public void WolfAttackAbortsWithoutTarget() =>
        Assert.True(VmActionLineEffect.WolfAttackAbortsWithoutTarget(null));

    [Fact]
    public void WolfAttackAbortsWithoutSight() =>
        Assert.True(VmActionLineEffect.WolfAttackAbortsWithoutSight(false));

    [Fact]
    public void LineEffectCanRun_WhenNotLinedone() =>
        Assert.True(VmActionLineEffect.LineEffectCanRun(0, 1));

    [Fact]
    public void ApplyLinedoneFlag_SetsBit() =>
        Assert.Equal(VmActionLineEffect.Mf6Linedone, VmActionLineEffect.ApplyLinedoneFlag(0, true, false));
}
