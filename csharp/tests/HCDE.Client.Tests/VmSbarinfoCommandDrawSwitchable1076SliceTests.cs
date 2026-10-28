namespace HCDE.Client.Tests;

public class VmSbarinfoCommandDrawSwitchable1076SliceTests
{
    [Fact]
    public void ShouldEnableConditionAndParse_BlocksInvulnerability()
    {
        Assert.False(VmSbarinfoCommandDrawSwitchable1076Slice.ShouldEnableConditionAndParse(
            sawAndAndToken: true,
            VmSbarinfoCommandDrawSwitchable1075Slice.SwitchableDrawCondition.Invulnerability));
        Assert.True(VmSbarinfoCommandDrawSwitchable1076Slice.ShouldEnableConditionAndParse(
            true,
            VmSbarinfoCommandDrawSwitchable1075Slice.SwitchableDrawCondition.KeySlot));
    }

    [Fact]
    public void CountConditionalImagesToLoad_MatchesNativeLoopBound()
    {
        Assert.Equal(1, VmSbarinfoCommandDrawSwitchable1076Slice.CountConditionalImagesToLoad(false));
        Assert.Equal(3, VmSbarinfoCommandDrawSwitchable1076Slice.CountConditionalImagesToLoad(true));
    }

    [Fact]
    public void ResolveKeySlotDrawAlt_MatchesDualKeyTable()
    {
        Assert.Equal(0, VmSbarinfoCommandDrawSwitchable1076Slice.ResolveKeySlotDrawAlt(true, true, true));
        Assert.Equal(2, VmSbarinfoCommandDrawSwitchable1076Slice.ResolveKeySlotDrawAlt(true, true, false));
        Assert.Equal(3, VmSbarinfoCommandDrawSwitchable1076Slice.ResolveKeySlotDrawAlt(true, false, true));
        Assert.Equal(0, VmSbarinfoCommandDrawSwitchable1076Slice.ResolveKeySlotDrawAlt(false, true, false));
    }

    [Fact]
    public void ApplyKeySlotNumberMatch_UpdatesExpectedSlots()
    {
        var (f1, f2) = VmSbarinfoCommandDrawSwitchable1076Slice.ApplyKeySlotNumberMatch(
            keyNumber: 5,
            expectedFirst: 5,
            expectedSecond: 9,
            conditionAnd: true,
            foundFirst: false,
            foundSecond: false);
        Assert.True(f1);
        Assert.False(f2);
    }

    [Fact]
    public void ResolveWeaponSlotDrawAlt_TogglesOnOwnedWeapon()
    {
        Assert.Equal(0, VmSbarinfoCommandDrawSwitchable1076Slice.ResolveWeaponSlotDrawAlt(true));
        Assert.Equal(1, VmSbarinfoCommandDrawSwitchable1076Slice.ResolveWeaponSlotDrawAlt(false));
    }

    [Fact]
    public void ResolveInvulnerabilityDrawAlt_DetectsGodModeCheats()
    {
        Assert.Equal(1, VmSbarinfoCommandDrawSwitchable1076Slice.ResolveInvulnerabilityDrawAlt(
            VmSbarinfoCommandDrawSwitchable1076Slice.CheatGodMode2));
        Assert.Equal(0, VmSbarinfoCommandDrawSwitchable1076Slice.ResolveInvulnerabilityDrawAlt(0));
    }

    [Fact]
    public void EvaluateArmorTypeMatch_Uses1075Compare()
    {
        Assert.True(VmSbarinfoCommandDrawSwitchable1076Slice.EvaluateArmorTypeMatch(
            armorNameIndex: 3,
            expectedArmorTypeIndex: 3,
            VmSbarinfoCommandDrawSwitchable1075Slice.SwitchableCompareOperator.Greater,
            threshold: 10,
            armorAmount: 20));
    }

    [Fact]
    public void ResolveArmorTypeDrawAlt_MatchesNativePriority()
    {
        Assert.Equal(3, VmSbarinfoCommandDrawSwitchable1076Slice.ResolveArmorTypeDrawAlt(
            conditionAnd: true,
            matchesFirst: false,
            matchesSecond: true));
        Assert.Equal(0, VmSbarinfoCommandDrawSwitchable1076Slice.ResolveArmorTypeDrawAlt(
            conditionAnd: false,
            matchesFirst: true,
            matchesSecond: false));
    }
}
