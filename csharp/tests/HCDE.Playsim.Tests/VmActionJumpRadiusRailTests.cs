namespace HCDE.Playsim.Tests;

public class VmActionJumpRadiusRailTests
{
    [Fact]
    public void JumpShouldChangeState_AlwaysWhenMaxChance256() =>
        Assert.True(VmActionJumpRadiusRail.JumpShouldChangeState(256, 300));

    [Fact]
    public void JumpShouldChangeState_WhenRollBelowChance() =>
        Assert.True(VmActionJumpRadiusRail.JumpShouldChangeState(100, 50));

    [Fact]
    public void SumBfgDamage_MatchesNativePrFormula()
    {
        // Native: sum_{i<damageSteps} ((pr_bfgselfdamage() & 7) + 1) — see p_actionfunctions.cpp ~972–973.
        // pr=2 => (2&7)+1=3 per iteration; damageSteps=2 => 6 total (not 8, which needs pr=3).
        Assert.Equal(6, VmActionJumpRadiusRail.SumBfgDamage(2, () => 2));
    }

    [Fact]
    public void RadiusDamageSteps_ScalesByDistance() =>
        Assert.Equal(5, VmActionJumpRadiusRail.RadiusDamageSteps(10, 5, 10));

    [Fact]
    public void CustomRailgunShouldAbort_WhenAimWithoutTarget() =>
        Assert.True(VmActionJumpRadiusRail.CustomRailgunShouldAbort(true, false));

    [Fact]
    public void NormalizeRailRange_DefaultsWhenZero() =>
        Assert.Equal(8192.0, VmActionJumpRadiusRail.NormalizeRailRange(0));

    [Fact]
    public void NormalizeRailSparsity_DefaultsWhenZero() =>
        Assert.Equal(1.0, VmActionJumpRadiusRail.NormalizeRailSparsity(0));

    [Fact]
    public void SpreadJitter_ScalesWhenNotExplicitAngle()
    {
        var (xy, z) = VmActionJumpRadiusRail.SpreadJitter(false, 10, 20, 128);
        Assert.Equal(5.0, xy, 3);
        Assert.Equal(10.0, z, 3);
    }

    [Fact]
    public void StealthVisDirOnFire_SetsWhenStealth() =>
        Assert.Equal(1, VmActionJumpRadiusRail.StealthVisDirOnFire(VmActionJumpRadiusRail.MfStealth));
}
