namespace HCDE.Playsim.Tests;

public class VmPMapTraceBleedTests
{
    [Fact]
    public void SkipBleed_WhenNoBloodFlag() =>
        Assert.True(VmPMapTraceBleed.SkipBleedActor(VmPMapTraceBleed.MfNoblood, 0, 0, false));

    [Fact]
    public void PlanBleed_LowDamageCanSkip() =>
        Assert.Equal(0, VmPMapTraceBleed.PlanBleedSpray(8, 100).Count);

    [Fact]
    public void PlanBleed_HighDamageSmearChance() =>
        Assert.Equal("BloodSmear", VmPMapTraceBleed.PlanBleedSpray(40, 0).BloodType);

    [Fact]
    public void DefaultBleedDistance_Fallback172() =>
        Assert.Equal(172, VmPMapTraceBleed.DefaultBleedDistance(0));

    [Fact]
    public void MissileBleed_SkipsBloodless() =>
        Assert.True(VmPMapTraceBleed.MissileBleedSkip(false, VmPMapTraceBleed.Mf3Bloodlessimpact));
}
