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

    [Fact]
    public void ScaleTeamDamage_AppliesMultiplier() =>
        Assert.Equal(50, VmPInteractionDamageLate.ScaleTeamDamage(100, 0.5));

    [Fact]
    public void EndLevelHack_ClampsLethalDamage() =>
        Assert.Equal(9, VmPInteractionDamageLate.ClampEndLevelDamage(50, 10));

    [Fact]
    public void PlayerPhase_StopsOnInvuln() =>
        Assert.Equal(VmPInteractionDamageLate.PlayerDamagePhaseOutcome.StopZero,
            VmPInteractionDamageLate.EvaluatePlayerDamagePhase(new(
                true, 0, 0, false, false, 20, 100, false, true, false, false, false, false, false, false, false, 0)));

    [Fact]
    public void PowerDrain_PicksMaxStrength() =>
        Assert.Equal(30, VmPInteractionDamageLate.EvaluatePowerDrainDamage(
            new VmPInteractionDamageLate.PowerDrainContext(true, false, false, 0.1, 0.3, 100, false, true)));

    [Fact]
    public void ResolveDamageType_UsesInflictorDeathType() =>
        Assert.Equal(7, VmPInteractionDamageLate.ResolveDamageType(
            new VmPInteractionDamageLate.DeathTypeContext(1, 2, false, true, true, 0, 0, false, true, 7)));

    [Fact]
    public void ClampDamageCount_CapsAt100() =>
        Assert.Equal(100, VmPInteractionDamageLate.ClampDamageCount(500));
}
