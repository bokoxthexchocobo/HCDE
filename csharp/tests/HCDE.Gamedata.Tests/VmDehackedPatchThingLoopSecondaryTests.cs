namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchThingLoopSecondaryTests
{
    [Fact]
    public void ResolveTranslucentAlpha_Half() =>
        Assert.Equal(0.5, VmDehackedPatchThingLoopSecondary.ResolveTranslucentAlpha(2));

    [Fact]
    public void HeightPairInvalid_WhenMismatched() =>
        Assert.True(VmDehackedPatchThingLoopSecondary.HeightPairInvalid(true, false));

    [Fact]
    public void NormalizeSpeed_ScalesLarge() =>
        Assert.Equal(1.0, VmDehackedPatchThingLoopSecondary.NormalizeSpeed(65536));

    [Fact]
    public void SelectMissileBounceFlags_MissileVsActor()
    {
        Assert.Equal(VmDehackedPatchThingLoopSecondary.BounceClassicDeh,
            VmDehackedPatchThingLoopSecondary.SelectMissileBounceFlags(true));
        Assert.Equal(VmDehackedPatchThingLoopSecondary.BounceGrenadeDeh,
            VmDehackedPatchThingLoopSecondary.SelectMissileBounceFlags(false));
    }

    [Fact]
    public void ApplyBitsLayer0_RemapsTouchyToFlags6()
    {
        var acc = new VmDehackedPatchThingLoopSecondary.BitsTokenAccumulator();
        acc.Changed[0] = true;
        acc.Values[0] = VmDehackedPatchThingLoopSecondary.MfTouchy;
        var scratch = new VmDehackedPatchThingLoopSecondary.ThingBitsPatchScratch();
        VmDehackedPatchThingLoopSecondary.ApplyBitsLayer0(acc, scratch, 2, 0, 0);
        Assert.Equal(0x2000000, scratch.Flags6);
        Assert.Equal(0, scratch.Flags & (int)VmDehackedPatchThingLoopSecondary.MfTouchy);
    }

    [Fact]
    public void ApplyPatchThingFinalize_NormalizesLargeSpeed()
    {
        var scratch = new VmDehackedPatchThingLoopSecondary.ThingBitsPatchScratch { Flags = VmDehackedPatchThingLoopSecondary.MfSpecial };
        var input = new VmDehackedPatchThingLoopSecondary.PatchThingFinalizeInput(
            0, 0, 2, false, false, false, false, false, 0, 65536, false);
        var (speed, _, _, push) = VmDehackedPatchThingLoopSecondary.ApplyPatchThingFinalize(input, scratch);
        Assert.Equal(1.0, speed);
        Assert.True(push);
    }
}
