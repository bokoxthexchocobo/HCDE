namespace HCDE.Playsim.Tests;

public class VmPInteractionDamageLateTests
{
    [Fact]
    public void TeamDamage_ScalesTeammateHit() =>
        Assert.True(VmPInteractionDamageLate.EvaluateTeamDamage(
            new VmPInteractionDamageLate.TeamDamageContext(false, true, true, false, 0, 5)));

    [Fact]
    public void TeamDamage_StopsAtZero() =>
        Assert.False(VmPInteractionDamageLate.EvaluateTeamDamage(
            new VmPInteractionDamageLate.TeamDamageContext(false, true, true, false, 0, 0)));

    [Fact]
    public void PlayerInvuln_BlocksNonTelefrag() =>
        Assert.False(VmPInteractionDamageLate.EvaluatePlayerSurvivesInvuln(
            new VmPInteractionDamageLate.PlayerDamageContext(false, true, false, false, false, false, false, 10, false, false, true, 100)));

    [Fact]
    public void PlayerArmor_UsesInventory() =>
        Assert.True(VmPInteractionDamageLate.EvaluatePlayerArmor(
            new VmPInteractionDamageLate.PlayerDamageContext(false, false, false, false, false, false, true, 5, false, false, true, 100)));

    [Fact]
    public void MonsterBuddha_SavesAtOneHp() =>
        Assert.True(VmPInteractionDamageLate.EvaluateMonsterBuddha(
            new VmPInteractionDamageLate.DeathTailContext(0, false, true, false, false, false, false, false, true)));

    [Fact]
    public void DeathProceeds_WhenNoBuddha() =>
        Assert.True(VmPInteractionDamageLate.EvaluateDeathProceeds(
            new VmPInteractionDamageLate.DeathTailContext(0, false, false, false, false, false, false, false, true)));

    [Fact]
    public void DrainEligible_SkipsSamePlayer() =>
        Assert.False(VmPInteractionDamageLate.DrainEligible(true, false, true));

    [Fact]
    public void ReturnNonNegative_PreservesCancelSentinel() =>
        Assert.Equal(-1, VmPInteractionDamageLate.ReturnNonNegative(-5));
}
