namespace HCDE.Playsim.Tests;

public class VmPUserResurrectBrightTests
{
    [Fact]
    public void Resurrect_RejectsPlayerChunk() =>
        Assert.True(VmPUserResurrectBright.ResurrectRejected(false, true));

    [Fact]
    public void Resurrect_AllowsLiveMo() =>
        Assert.True(VmPUserResurrectBright.CanResurrect(
            new VmPUserResurrectBright.ResurrectPlan(false, false, "gun", null, 0)));

    [Fact]
    public void Resurrect_SkipsTranslationWhenFlagSet() =>
        Assert.False(VmPUserResurrectBright.ApplyTranslationAfterRevive(
            new VmPUserResurrectBright.ResurrectPlan(false, false, null, null, VmPUserResurrectBright.Mf2Donttranslate)));

    [Fact]
    public void Fullbright_DefaultMapsToNightVision() =>
        Assert.Equal(
            VmPUserResurrectBright.FbmodeNightvision,
            VmPUserResurrectBright.ResolveFullbrightMode(VmPUserResurrectBright.FbmodeDefault, true));

    [Fact]
    public void Fullbright_DefaultMapsToFullbright() =>
        Assert.Equal(
            VmPUserResurrectBright.FbmodeFullbright,
            VmPUserResurrectBright.ResolveFullbrightMode(VmPUserResurrectBright.FbmodeDefault, false));

    [Fact]
    public void Fullbright_ExplicitModePassthrough() =>
        Assert.Equal(7, VmPUserResurrectBright.ResolveFullbrightMode(7, false));

    [Fact]
    public void ClearInvisibleRenderFlags() =>
        Assert.Equal(0, VmPUserResurrectBright.ClearInvisibleRenderFlags(VmPUserResurrectBright.RfInvisible));
}
