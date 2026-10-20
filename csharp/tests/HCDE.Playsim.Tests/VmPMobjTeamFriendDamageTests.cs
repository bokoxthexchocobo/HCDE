namespace HCDE.Playsim.Tests;

public class VmPMobjTeamFriendDamageTests
{
    [Fact]
    public void GetTeam_UsesFriendPlayerWhenDesignatedNone() =>
        Assert.Equal(2, VmPMobjTeamFriendDamage.GetTeamFromDesignated(
            VmPMobjTeamFriendDamage.TeamNone, 1, 2));

    [Fact]
    public void Teammate_CoopFriendlyFlagMatch() =>
        Assert.True(VmPMobjTeamFriendDamage.IsTeammate(new VmPMobjTeamFriendDamage.TeammateQuery(
            false, false, false, true, true,
            VmPMobjTeamFriendDamage.MfFriendly,
            VmPMobjTeamFriendDamage.MfFriendly,
            0, 0)));

    [Fact]
    public void Teammate_TeamplaySameTeam() =>
        Assert.True(VmPMobjTeamFriendDamage.IsTeammate(new VmPMobjTeamFriendDamage.TeammateQuery(
            false, true, true, false, false, 0, 0, 3, 3)));

    [Fact]
    public void Friend_DmTeamplayRequiresTeammateOrLinked() =>
        Assert.True(VmPMobjTeamFriendDamage.IsFriend(new VmPMobjTeamFriendDamage.FriendQuery(
            VmPMobjTeamFriendDamage.MfFriendly,
            VmPMobjTeamFriendDamage.MfFriendly,
            true, true, true, 0, 0, false)));

    [Fact]
    public void Hostile_DmDifferentFriendPlayers() =>
        Assert.True(VmPMobjTeamFriendDamage.IsHostile(new VmPMobjTeamFriendDamage.HostileQuery(
            VmPMobjTeamFriendDamage.MfFriendly,
            VmPMobjTeamFriendDamage.MfFriendly,
            true, false, false, 1, 2, false)));

    [Fact]
    public void DoSpecial_GodmodeBlocks() =>
        Assert.Equal(VmPMobjTeamFriendDamage.SpecialDamageBlocked,
            VmPMobjTeamFriendDamage.DoSpecialDamage(new VmPMobjTeamFriendDamage.DoSpecialDamageContext(
                true, VmPMobjTeamFriendDamage.CfGodmode, 50, false, 0, 0)));

    [Fact]
    public void DoSpecial_PoisonHalves() =>
        Assert.Equal(25, VmPMobjTeamFriendDamage.DoSpecialDamage(
            new VmPMobjTeamFriendDamage.DoSpecialDamageContext(
                false, 0, 50, true, 5, int.MinValue)));

    [Fact]
    public void TakeSpecial_MassacreAlwaysApplies() =>
        Assert.Equal(99, VmPMobjTeamFriendDamage.TakeSpecialDamageWithAmount(
            new VmPMobjTeamFriendDamage.TakeSpecialDamageContext(
                false, true, VmPMobjTeamFriendDamage.NameMassacre,
                false, 0, 0, false, false, false, 0, false, false),
            99));

    [Fact]
    public void TakeSpecial_IceGenericFreeze() =>
        Assert.Equal(10, VmPMobjTeamFriendDamage.TakeSpecialDamageWithAmount(
            new VmPMobjTeamFriendDamage.TakeSpecialDamageContext(
                false, true, VmPMobjTeamFriendDamage.NameIce,
                false, 0, VmPMobjTeamFriendDamage.NameIce,
                false, false, false, 0, true, true),
            10));

    [Fact]
    public void CallVirtual_Flags() =>
        Assert.False(VmPMobjTeamFriendDamage.UseVirtualDoSpecialDamage(false));
}
