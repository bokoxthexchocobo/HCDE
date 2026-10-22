namespace HCDE.Playsim.Tests;

public class VmPInteractionPoisonDamageTests
{
    [Fact]
    public void PoisonMobj_AdditiveDamage()
    {
        var target = new VmPInteractionPoisonDamage.PoisonActor { PoisonDamageReceived = 5 };
        VmPInteractionPoisonDamage.PoisonMobj(
            new(), target, VmPInteractionPoisonDamage.Mf6Additivepoisondamage, 0, 0, null, 10, 0, 0, 1);
        Assert.Equal(15, target.PoisonDamageReceived);
    }

    [Fact]
    public void PoisonPlayer_GodmodeBlocks()
    {
        var player = new VmPInteractionPoisonDamage.PoisonPlayerState { Cheats = VmPInteractionPoisonDamage.CfGodmode };
        var mo = new VmPInteractionPoisonDamage.PoisonActor();
        Assert.False(VmPInteractionPoisonDamage.PoisonPlayer(
            player, mo, null, null, 10, false, 1, 0, 0, true));
    }

    [Fact]
    public void PoisonDamage_DeathCallsDie()
    {
        var died = false;
        var player = new VmPInteractionPoisonDamage.PoisonPlayerState { Health = 10 };
        var target = new VmPInteractionPoisonDamage.PoisonActor { Health = 10 };
        VmPInteractionPoisonDamage.PoisonDamage(new()
        {
            CallDie = (_, _) => died = true,
        }, player, target, null, 50, 1, 1);
        Assert.True(died);
    }

    [Fact]
    public void MirrorOrVisualSkips_BlocksPoisonMobj()
    {
        var target = new VmPInteractionPoisonDamage.PoisonActor();
        VmPInteractionPoisonDamage.PoisonMobj(new() { MirrorTarget = true }, target, 0, 0, 0, null, 5, 1, 1, 1);
        Assert.Equal(0, target.PoisonDamageReceived);
    }
}
