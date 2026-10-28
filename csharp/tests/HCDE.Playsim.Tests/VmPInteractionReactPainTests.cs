namespace HCDE.Playsim.Tests;

public class VmPInteractionReactPainTests
{
    [Fact]
    public void React_BlocksWhenDead() =>
        Assert.True(VmPInteractionReactPain.DeadOrNoDamage(false, 0, 5));

    [Fact]
    public void React_Godmode2Blocks() =>
        Assert.True(VmPInteractionReactPain.GodmodeBlocksReact(
            VmPInteractionReactPain.CfGodmode2, 0, 10));

    [Fact]
    public void React_WoundAtThreshold() =>
        Assert.True(VmPInteractionReactPain.WoundStateTriggers(5, 10));

    [Fact]
    public void Pain_ForcedAlwaysRolls() =>
        Assert.True(VmPInteractionReactPain.PainRollSucceeded(true, 255, 1));

    [Fact]
    public void Pain_ElectricFlinch() =>
        Assert.True(VmPInteractionReactPain.ElectricFlinchRoll(50));

    [Fact]
    public void FakePain_FromCausePain() =>
        Assert.True(VmPInteractionReactPain.IsFakePain(0, 0, 0, true, VmPInteractionReactPain.Mf7Causepain));
}
