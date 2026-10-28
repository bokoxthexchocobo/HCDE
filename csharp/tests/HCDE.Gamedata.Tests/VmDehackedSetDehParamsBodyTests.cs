namespace HCDE.Gamedata.Tests;

public class VmDehackedSetDehParamsBodyTests
{
    [Fact]
    public void TurnAngleDegrees_PassesThrough() =>
        Assert.Equal(90f, VmDehackedSetDehParamsBody.TurnAngleDegrees(90));

    [Fact]
    public void SpawnItemArgs_DefaultsAmmoAndTranslation()
    {
        var args = VmDehackedSetDehParamsBody.SpawnItemArgs("item", misc2: 4);
        Assert.Equal(0, args.useAmmo);
        Assert.Equal(4f, args.height);
    }

    [Fact]
    public void ScratchMeleeArgs_MatchesNativeDefaults() =>
        Assert.Equal((4, 1, 0, 0, true), VmDehackedSetDehParamsBody.ScratchMeleeArgs(4, 2));

    [Fact]
    public void SetDehParamsValidatePointerNative_RejectsNegative() =>
        Assert.False(VmDehackedSetDehParamsBody.SetDehParamsValidatePointerNative(-1, 10, out _));

    [Fact]
    public void SanitizePatchNameNative_ReplacesInvalidChars() =>
        Assert.Equal("patch_name", VmDehackedSetDehParamsBody.SanitizePatchNameNative("patch:name"));

    [Fact]
    public void RandomJumpArgs_MatchesNative() =>
        Assert.Equal((50, 3), VmDehackedSetDehParamsBody.RandomJumpArgs(3, 50));

    [Fact]
    public void LineEffectArgs_PassThrough() =>
        Assert.Equal((11, 4), VmDehackedSetDehParamsBody.LineEffectArgs(11, 4));

    [Fact]
    public void BuildPrintableDehackedNameNative_FormatsPath() =>
        Assert.Contains("Dehacked.patch", VmDehackedSetDehParamsBody.BuildPrintableDehackedNameNative("patch", 1, "A_Fire", 2, 3));

    [Fact]
    public void MbfFactoryTableEntryCountNative_MatchesNativeTable() =>
        Assert.Equal(38, VmDehackedSetDehParamsBody.MbfFactoryTableEntryCountNative());

    [Fact]
    public void EmitStandardActionParametersNative_EmitsEachSlot()
    {
        var seen = new List<int>();
        VmDehackedSetDehParamsBody.EmitStandardActionParametersNative(3, seen.Add);
        Assert.Equal([0, 1, 2], seen);
    }

    [Fact]
    public void TryValidateSetDehParamsNative_AcceptsInRangePointer()
    {
        var plan = VmDehackedSetDehParamsBody.TryValidateSetDehParamsNative(2, 10, 10, 1, 2);
        Assert.True(plan.Ok);
        Assert.False(plan.ReturnsState);
    }
}
