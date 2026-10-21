namespace HCDE.Playsim.Tests;

public class VmActionJumpRadiusRailTests
{
    [Fact]
    public void JumpShouldChangeState_AlwaysWhenMaxChance256() =>
        Assert.True(VmActionJumpRadiusRail.JumpShouldChangeState(256, 300));

    [Fact]
    public void SumBfgDamage_MatchesNativePrFormula()
    {
        // Native: sum_{i<damageSteps} ((pr_bfgselfdamage() & 7) + 1) — see p_actionfunctions.cpp ~972–973.
        // pr=2 => (2&7)+1=3 per iteration; damageSteps=2 => 6 total (not 8, which needs pr=3).
        Assert.Equal(6, VmActionJumpRadiusRail.SumBfgDamage(2, () => 2));
    }

    [Fact]
    public void CustomRailgunShouldAbort_WhenAimWithoutTarget() =>
        Assert.True(VmActionJumpRadiusRail.CustomRailgunShouldAbort(true, false));
}
