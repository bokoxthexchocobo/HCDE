namespace HCDE.Client.Tests;

public class VmSbarinfoCommandDrawString1078SliceTests
{
    [Fact]
    public void TryClassifyDrawStringValueKeyword_MapsDynamicSources()
    {
        var (ok, kind, invalid) = VmSbarinfoCommandDrawString1078Slice.TryClassifyDrawStringValueKeyword("globalarray");
        Assert.True(ok);
        Assert.Equal(VmSbarinfoCommandDrawString1078Slice.DrawStringValueKind.GlobalArray, kind);
        Assert.False(invalid);
    }

    [Fact]
    public void IsGlobalVarIndexInRange_MatchesNativeBound()
    {
        Assert.True(VmSbarinfoCommandDrawString1078Slice.IsGlobalVarIndexInRange(0));
        Assert.False(VmSbarinfoCommandDrawString1078Slice.IsGlobalVarIndexInRange(64));
    }

    [Fact]
    public void ShouldInvalidateDrawStringCacheOnReset_CoversClassAndSkill()
    {
        Assert.True(VmSbarinfoCommandDrawString1078Slice.ShouldInvalidateDrawStringCacheOnReset(
            VmSbarinfoCommandDrawString1078Slice.DrawStringValueKind.PlayerClass));
        Assert.False(VmSbarinfoCommandDrawString1078Slice.ShouldInvalidateDrawStringCacheOnReset(
            VmSbarinfoCommandDrawString1078Slice.DrawStringValueKind.LevelName));
    }

    [Fact]
    public void ShouldRefreshLevelBoundDrawString_DetectsLumpChange()
    {
        Assert.True(VmSbarinfoCommandDrawString1078Slice.ShouldRefreshLevelBoundDrawString(2, 1));
    }

    [Fact]
    public void FormatDrawStringLevelTime_MatchesNativeSplit()
    {
        Assert.Equal("01:02:03", VmSbarinfoCommandDrawString1078Slice.FormatDrawStringLevelTime(3723));
    }

    [Fact]
    public void SplitLevelTimeComponents_HandlesHourRollover()
    {
        var (h, m, s) = VmSbarinfoCommandDrawString1078Slice.SplitLevelTimeComponents(3661);
        Assert.Equal(1, h);
        Assert.Equal(1, m);
        Assert.Equal(1, s);
    }

    [Fact]
    public void ResolveConstantStringValueKind_UsesConstantEnum()
    {
        Assert.Equal(
            VmSbarinfoCommandDrawString1078Slice.DrawStringValueKind.Constant,
            VmSbarinfoCommandDrawString1078Slice.ResolveConstantStringValueKind());
    }

    [Fact]
    public void ShouldRefreshGlobalVarDrawString_ComparesCachedValue()
    {
        Assert.True(VmSbarinfoCommandDrawString1078Slice.ShouldRefreshGlobalVarDrawString(5, 4));
    }
}
