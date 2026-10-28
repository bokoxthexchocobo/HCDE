namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchThingNumericTests
{
    [Fact]
    public void TryApplyNumericProperty_SetsHitPoints()
    {
        var target = new VmDehackedThingPatchTarget();
        var flags = new VmDehackedThingPatchFlags();
        Assert.True(VmDehackedPatchThingNumeric.TryApplyNumericProperty(
            "Hit points", "150", target, flags));
        Assert.Equal(150, target.Health);
    }

    [Fact]
    public void TryApplyNumericProperty_AppliesTranslucency()
    {
        var target = new VmDehackedThingPatchTarget();
        var flags = new VmDehackedThingPatchFlags();
        Assert.True(VmDehackedPatchThingNumeric.TryApplyNumericProperty(
            "Translucency", "65536", target, flags));
        Assert.Equal(1, target.Alpha, 3);
        Assert.True(flags.HadStyle);
    }

    [Fact]
    public void IsSupportedNumeric_RejectsOutOfRange() =>
        Assert.False(VmDehackedPatchThingNumeric.IsSupportedNumeric(long.MaxValue));
}
