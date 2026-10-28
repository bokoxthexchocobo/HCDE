namespace HCDE.Client.Tests;

public class VmSbarinfoCommandDrawSwitchable1075SliceTests
{
    [Fact]
    public void ResolveCompareOperationFromTokens_DefaultsToGreaterZero()
    {
        var (op, value, usedDefault) = VmSbarinfoCommandDrawSwitchable1075Slice.ResolveCompareOperationFromTokens(
            sawEqual: false,
            sawLess: false,
            sawGreater: false,
            sawLessOrEqual: false,
            sawGreaterOrEqual: false,
            sawNotEqual: false,
            parsedIntConst: 99);
        Assert.True(usedDefault);
        Assert.Equal(VmSbarinfoCommandDrawSwitchable1075Slice.SwitchableCompareOperator.Greater, op);
        Assert.Equal(0, value);
    }

    [Fact]
    public void EvaluateSwitchableCompare_CoversNativeOperators()
    {
        Assert.True(VmSbarinfoCommandDrawSwitchable1075Slice.EvaluateSwitchableCompare(
            VmSbarinfoCommandDrawSwitchable1075Slice.SwitchableCompareOperator.Equal, 5, 5));
        Assert.True(VmSbarinfoCommandDrawSwitchable1075Slice.EvaluateSwitchableCompare(
            VmSbarinfoCommandDrawSwitchable1075Slice.SwitchableCompareOperator.LessOrEqual, 10, 7));
        Assert.False(VmSbarinfoCommandDrawSwitchable1075Slice.EvaluateSwitchableCompare(
            VmSbarinfoCommandDrawSwitchable1075Slice.SwitchableCompareOperator.NotEqual, 3, 3));
    }

    [Fact]
    public void TryClassifySwitchableConditionKeyword_MapsParseHeadKeywords()
    {
        var (ok, cond) = VmSbarinfoCommandDrawSwitchable1075Slice.TryClassifySwitchableConditionKeyword("keyslot");
        Assert.True(ok);
        Assert.Equal(VmSbarinfoCommandDrawSwitchable1075Slice.SwitchableDrawCondition.KeySlot, cond);
    }

    [Fact]
    public void InitializeSwitchableImageFields_MatchesNativeCtorSentinels()
    {
        var (img0, img1, img2, _, _, _, _) =
            VmSbarinfoCommandDrawSwitchable1075Slice.InitializeSwitchableImageFields();
        Assert.Equal(-1, img0);
        Assert.Equal(-1, img1);
        Assert.Equal(-1, img2);
    }

    [Fact]
    public void ShouldParseInventoryConditionString_DetectsStringConstPath()
    {
        Assert.True(VmSbarinfoCommandDrawSwitchable1075Slice.ShouldParseInventoryConditionString(
            tokenWasIdentifier: false,
            conditionStillInventory: true));
        Assert.False(VmSbarinfoCommandDrawSwitchable1075Slice.ShouldParseInventoryConditionString(
            tokenWasIdentifier: true,
            conditionStillInventory: true));
    }

    [Fact]
    public void ShouldParseWeaponSlotIndex_OnlyForWeaponSlotCondition()
    {
        Assert.True(VmSbarinfoCommandDrawSwitchable1075Slice.ShouldParseWeaponSlotIndex(
            VmSbarinfoCommandDrawSwitchable1075Slice.SwitchableDrawCondition.WeaponSlot));
        Assert.False(VmSbarinfoCommandDrawSwitchable1075Slice.ShouldParseKeySlotIndex(
            VmSbarinfoCommandDrawSwitchable1075Slice.SwitchableDrawCondition.WeaponSlot));
    }

    [Fact]
    public void ShouldParseArmorTypeIdentifier_DetectsArmorTypeBranch()
    {
        Assert.True(VmSbarinfoCommandDrawSwitchable1075Slice.ShouldParseArmorTypeIdentifier(
            VmSbarinfoCommandDrawSwitchable1075Slice.SwitchableDrawCondition.ArmorType));
    }
}
