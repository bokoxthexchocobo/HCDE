namespace HCDE.Client.Tests;

public class VmSbarinfoCommandDrawBar1090SliceTests
{
    [Fact]
    public void TryClassifyDrawBarParseFlagKeyword_MapsInterpolate()
    {
        var (ok, flag) = VmSbarinfoCommandDrawBar1090Slice.TryClassifyDrawBarParseFlagKeyword("interpolate");
        Assert.True(ok);
        Assert.Equal(VmSbarinfoCommandDrawBar1090Slice.DrawBarParseFlag.Interpolate, flag);
    }

    [Fact]
    public void ResolveDrawBarParseFlagListContinuation_ContinuesOnPipe()
    {
        var (cont, comma) = VmSbarinfoCommandDrawBar1090Slice.ResolveDrawBarParseFlagListContinuation(
            nextTokenIsPipe: true);
        Assert.True(cont);
        Assert.False(comma);
    }

    [Fact]
    public void ShouldReplaceDrawBarPowerupGiverClass_WhenMissing()
    {
        Assert.True(VmSbarinfoCommandDrawBar1090Slice.ShouldReplaceDrawBarPowerupGiverClass(
            classFound: false,
            isPowerupGiverKind: false));
    }

    [Fact]
    public void ApplyDrawBarReverseWhenBorderNonZero_FlipsReverse()
    {
        Assert.True(VmSbarinfoCommandDrawBar1090Slice.ApplyDrawBarReverseWhenBorderNonZero(
            border: 4,
            reverse: false));
    }

    [Fact]
    public void ResolveDrawBarInterpolationSpeedForType_HealthUsesScriptSpeed()
    {
        var speed = VmSbarinfoCommandDrawBar1090Slice.ResolveDrawBarInterpolationSpeedForType(
            VmSbarinfoCommandDrawBar1089Slice.DrawBarValueType.Health,
            scriptInterpolateHealth: true,
            scriptHealthInterpolationSpeed: 12,
            scriptInterpolateArmor: false,
            scriptArmorInterpolationSpeed: 0,
            parsedInterpolationSpeed: 3);
        Assert.Equal(12, speed);
    }

    [Fact]
    public void ApplyCommandDrawBarResetState_ClearsDrawValueOnly()
    {
        var (drawValue, pixel) = VmSbarinfoCommandDrawBar1090Slice.ApplyCommandDrawBarResetState(
            currentDrawValue: 0.75,
            currentPixel: 2);
        Assert.Equal(0.0, drawValue);
        Assert.Equal(2, pixel);
    }

    [Fact]
    public void ClampDrawBarDisplayedHealth_SuppressesNegative()
    {
        Assert.Equal(0, VmSbarinfoCommandDrawBar1090Slice.ClampDrawBarDisplayedHealth(-5));
    }

    [Fact]
    public void ResolveDrawBarHealthTickMax_UsesClassMaxByDefault()
    {
        Assert.Equal(
            200,
            VmSbarinfoCommandDrawBar1090Slice.ResolveDrawBarHealthTickMax(
                useMaximumConstant: false,
                maximumConstant: 0,
                comparerItemFound: false,
                comparerItemAmount: 0,
                classMaxHealth: 200));
    }

    [Fact]
    public void ResolveDrawBarAmmoSlotTick_EmptyAmmoUsesMaxOne()
    {
        var (value, max) = VmSbarinfoCommandDrawBar1090Slice.ResolveDrawBarAmmoSlotTick(
            ammoTypeMissing: true,
            ammoCount: 50,
            maxAmountWhenPresent: 99);
        Assert.Equal(0, value);
        Assert.Equal(1, max);
    }

    [Fact]
    public void ClassifyDrawBarOrientationKeyword_RejectsUnknown()
    {
        var (ok, _, unknown) = VmSbarinfoCommandDrawBar1090Slice.ClassifyDrawBarOrientationKeyword("diagonal");
        Assert.False(ok);
        Assert.True(unknown);
    }
}
