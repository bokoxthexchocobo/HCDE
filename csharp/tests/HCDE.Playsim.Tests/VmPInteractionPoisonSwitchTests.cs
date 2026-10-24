namespace HCDE.Playsim.Tests;

public class VmPInteractionPoisonSwitchTests
{
    [Fact]
    public void InvulnBlocksPoison_WhenNotFoil() =>
        Assert.True(VmPInteractionPoisonSwitch.InvulnBlocksPoison(false, true, false, false));

    [Fact]
    public void InfightingSameGroup_Blocks() =>
        Assert.True(VmPInteractionPoisonSwitch.InfightingSameGroup(3, 3));

    [Fact]
    public void PoisonPlayerGodmode_Blocks() =>
        Assert.True(VmPInteractionPoisonSwitch.PoisonPlayerGodmode(1, 0, 0));

    [Fact]
    public void InvulnBlocksPoisonDamage_WhenNotTelefrag() =>
        Assert.True(VmPInteractionPoisonSwitch.InvulnBlocksPoisonDamage(false, false, false, true, false));
}
