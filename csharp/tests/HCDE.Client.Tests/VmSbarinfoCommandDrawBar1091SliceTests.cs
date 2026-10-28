namespace HCDE.Client.Tests;

public class VmSbarinfoCommandDrawBar1091SliceTests
{
    [Fact]
    public void ResolveDrawBarInventoryItemTick_MissingItemZeroesValue()
    {
        var (value, max) = VmSbarinfoCommandDrawBar1091Slice.ResolveDrawBarInventoryItemTick(
            itemFound: false,
            amount: 10,
            maxAmount: 50);
        Assert.Equal(0, value);
        Assert.Equal(0, max);
    }

    [Fact]
    public void NormalizeDrawBarFillRatio_ClampsToOne()
    {
        Assert.Equal(1.0, VmSbarinfoCommandDrawBar1091Slice.NormalizeDrawBarFillRatio(
            value: 200,
            max: 100));
    }

    [Fact]
    public void ApplyDrawBarPowerupEffectDisplayTicks_IncrementsBoth()
    {
        var (value, max) = VmSbarinfoCommandDrawBar1091Slice.ApplyDrawBarPowerupEffectDisplayTicks(
            effectTics: 35,
            effectMax: 700);
        Assert.Equal(36, value);
        Assert.Equal(701, max);
    }

    [Fact]
    public void ComputeDrawBarSavePercentTick_AddsBasicArmor()
    {
        var (value, max) = VmSbarinfoCommandDrawBar1091Slice.ComputeDrawBarSavePercentTick(
            hexenArmorSlotSum: 10,
            basicArmorPresent: true,
            basicSavePercent: 0.5);
        Assert.Equal(60, value);
        Assert.Equal(100, max);
    }

    [Fact]
    public void ClampDrawBarAirtimeRemaining_NeverNegative()
    {
        Assert.Equal(0, VmSbarinfoCommandDrawBar1091Slice.ClampDrawBarAirtimeRemaining(
            airFinished: 5,
            mapTime: 20));
    }

    [Fact]
    public void StepDrawBarDrawValueInterpolation_SnapsWithinPixelThreshold()
    {
        Assert.Equal(
            0.5,
            VmSbarinfoCommandDrawBar1091Slice.StepDrawBarDrawValueInterpolation(
                drawValue: 0.52,
                targetValue: 0.5,
                pixelThreshold: 0.05,
                interpolationSpeed: 8,
                equalEpsilon: VmSbarinfoCommandDrawBar1091Slice.EqualEpsilon));
    }

    [Fact]
    public void ResolveDrawBarTickDrawValue_SkipsInterpolationWhenDisabled()
    {
        Assert.Equal(
            0.25,
            VmSbarinfoCommandDrawBar1091Slice.ResolveDrawBarTickDrawValue(
                drawValue: 0.9,
                normalizedTarget: 0.25,
                interpolationSpeed: 0,
                hudChanged: false,
                levelTime: 1,
                pixelUnset: true,
                foregroundDisplayWidth: 32));
    }

    [Fact]
    public void ShouldReplaceDrawBarComparatorInventoryClass_WhenNotInventory()
    {
        Assert.True(VmSbarinfoCommandDrawBar1091Slice.ShouldReplaceDrawBarComparatorInventoryClass(
            classFound: true,
            isInventoryKind: false));
    }

    [Fact]
    public void TryApplyDrawBarComparatorMaximumConstant_RequiresExtendedInt()
    {
        var (applied, useMax, maxVal) = VmSbarinfoCommandDrawBar1091Slice.TryApplyDrawBarComparatorMaximumConstant(
            extendedSyntax: true,
            hasIntConst: true,
            number: 75);
        Assert.True(applied);
        Assert.True(useMax);
        Assert.Equal(75, maxVal);
    }

    [Fact]
    public void CreateDrawBarAdditionalDataDefaults_MatchesNativeInit()
    {
        Assert.False(VmSbarinfoCommandDrawBar1091Slice.CreateDrawBarAdditionalDataDefaults().UseMaximumConstant);
    }
}
