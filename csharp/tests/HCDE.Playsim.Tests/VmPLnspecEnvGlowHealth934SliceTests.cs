namespace HCDE.Playsim.Tests;

public class VmPLnspecEnvGlowHealth934SliceTests
{
    [Fact]
    public void GlobalFogSwitchExhausted_RejectsUnknownType() =>
        Assert.True(VmPLnspecEnvGlowHealth934Slice.GlobalFogSwitchExhausted(9));

    [Fact]
    public void BuildFloorGlow_InvalidatesColorWhenHeightNegative()
    {
        var glow = VmPLnspecEnvGlowHealth934Slice.BuildFloorGlow(-1, 10, 20, 30);
        Assert.Equal(VmPLnspecEnvGlowHealth934Slice.GlowColorInvalidate, glow.Color);
    }

    [Fact]
    public void BuildLineHealth_ClampsNegative()
    {
        var invoke = VmPLnspecEnvGlowHealth934Slice.BuildLineHealth(-5, false);
        Assert.Equal(0, invoke.Health);
    }

    [Fact]
    public void BuildSectorHealth_SelectsCeilingPart()
    {
        var invoke = VmPLnspecEnvGlowHealth934Slice.BuildSectorHealth(
            VmPLnspecEnvGlowHealth934Slice.SecPartCeiling, 50, true, false, false);
        Assert.NotNull(invoke);
        Assert.Equal(VmPLnspecEnvGlowHealth934Slice.SectorHealthTarget.Ceiling, invoke!.Value.Target);
        Assert.True(invoke.Value.PropagateGroup);
    }

    [Fact]
    public void DispatchName_MapsGlassBreakOpcode()
    {
        Assert.Equal("LS_GlassBreak", VmPLnspecEnvGlowHealth934Slice.DispatchName(49));
    }

    [Fact]
    public void DispatchIsDoorCluster_RecognizesRaiseRange() =>
        Assert.True(VmPLnspecEnvGlowHealth934Slice.DispatchIsDoorCluster(12));

    [Fact]
    public void CountDispatchEntries_MatchesNativeHead() =>
        Assert.Equal(58, VmPLnspecEnvGlowHealth934Slice.CountDispatchEntries());
}
