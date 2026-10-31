namespace HCDE.Playsim.Tests;

public class VmHwCvars1307SliceTests
{
    [Fact]
    public void Gamma_ClampsAndResyncsWithoutLeavingTheGuardSet()
    {
        VmHwCvars1307.ResetProcess();
        var cvars = new VmHwCvars1307();
        Assert.Equal(2, cvars.FogMode);
        Assert.Equal(4, cvars.PortalRecursions);
        Assert.True(cvars.PlaneReflection);
        Assert.False(cvars.PlaneReflectionI);
        Assert.Equal(VmHwCvars1307.GammaDefault, cvars.VidGamma);
        Assert.Equal(0f, cvars.VidFixGamma);
        Assert.Equal(1f, cvars.VidContrast);
        Assert.Equal(1f, cvars.VidSaturation);

        cvars.SetVidGamma(9f);
        Assert.Equal(VmHwCvars1307.GammaHigh, cvars.VidGamma);
        Assert.Equal(VmHwCvars1307.GammaHigh, cvars.VidGammaCompat);
        Assert.Equal(1f, cvars.VidFixGamma);
        Assert.False(VmHwCvars1307.GammaSyncing);

        cvars.SetVidGamma(0f);
        Assert.Equal(VmHwCvars1307.GammaLow, cvars.VidGamma);
        Assert.Equal(VmHwCvars1307.GammaLow, cvars.VidGammaCompat);
        Assert.Equal(VmHwCvars1307.GammaLowFix, cvars.VidFixGamma);

        cvars.SetVidFixGamma(5f);
        Assert.Equal(1f, cvars.VidFixGamma);
        Assert.Equal(VmHwCvars1307.GammaHigh, cvars.VidGamma);
        Assert.Equal(VmHwCvars1307.GammaLow, cvars.VidGammaCompat);

        cvars.SetVidGamma(cvars.VidGamma);
        Assert.Equal(VmHwCvars1307.GammaHigh, cvars.VidGammaCompat);
        Assert.Equal(1f, cvars.VidFixGamma);

        cvars.SetVidGammaCompat(4f);
        Assert.Equal(VmHwCvars1307.GammaHigh, cvars.VidGammaCompat);
        Assert.Equal(VmHwCvars1307.GammaHigh, cvars.VidGamma);
        Assert.Equal(1f, cvars.VidFixGamma);

        cvars.SetVidGamma(VmHwCvars1307.GammaDefault);
        Assert.Equal(VmHwCvars1307.GammaDefault, cvars.VidGammaCompat);
        Assert.Equal(0f, cvars.VidFixGamma);

        cvars.SetVidFixGamma(-10f);
        Assert.Equal(VmHwCvars1307.GammaLowFix, cvars.VidFixGamma);
        Assert.Equal(VmHwCvars1307.FixGammaToGamma(VmHwCvars1307.GammaLowFix), cvars.VidGamma);
        Assert.Equal(VmHwCvars1307.GammaDefault, cvars.VidGammaCompat);
        Assert.False(VmHwCvars1307.GammaSyncing);

        cvars.SetContrast(-1f);
        Assert.Equal(0f, cvars.VidContrast);
        cvars.SetContrast(9f);
        Assert.Equal(5f, cvars.VidContrast);
        cvars.SetContrast(2f);
        Assert.Equal(2f, cvars.VidContrast);
        cvars.SetSaturation(-4f);
        Assert.Equal(-3f, cvars.VidSaturation);
        cvars.SetSaturation(4f);
        Assert.Equal(3f, cvars.VidSaturation);

        cvars.SetFogMode(5);
        Assert.Equal(2, cvars.FogMode);
        cvars.SetFogMode(-3);
        Assert.Equal(0, cvars.FogMode);
        cvars.SetPortalRecursions(20);
        Assert.Equal(16, cvars.PortalRecursions);
        cvars.SetPortalRecursions(-1);
        Assert.Equal(0, cvars.PortalRecursions);
        cvars.SetPlaneReflection(false);
        Assert.False(cvars.PlaneReflection);
        Assert.False(cvars.PlaneReflectionI);
        cvars.SetPlaneReflection(true);
        Assert.True(cvars.PlaneReflectionI);

        var other = new VmHwCvars1307();
        other.SetVidGamma(1f);
        Assert.Equal(1f, other.VidGamma);
        Assert.Equal(VmHwCvars1307.GammaToFixGamma(1f), other.VidFixGamma);
        Assert.Equal(VmHwCvars1307.FixGammaToGamma(VmHwCvars1307.GammaLowFix), cvars.VidGamma);
        Assert.False(VmHwCvars1307.GammaSyncing);
    }

    [Fact]
    public void BlackWhite_UsesTheOtherInternalAndFiltersReenter()
    {
        VmHwCvars1307.ResetProcess();
        var cvars = new VmHwCvars1307();
        Assert.Equal(1f, cvars.VidIBlackpoint);
        Assert.Equal(1f, cvars.VidIWhitepoint);
        Assert.Equal(16f, cvars.TextureFilterAnisotropic);
        Assert.Equal(6, cvars.TextureFilter);
        Assert.False(cvars.Precache);
        Assert.Equal(2, cvars.SatFormula);
        Assert.True(cvars.Portals && cvars.Mirrors && cvars.MirrorEnvmap && cvars.Seamless);

        cvars.VidIWhitepoint = 0.5f;
        cvars.SetBlackpoint(1f);
        Assert.Equal(1f, cvars.VidBlackpoint);
        Assert.Equal(Black(1f, 0.5f), cvars.VidIBlackpoint);

        cvars.SetBlackpoint(2f);
        Assert.Equal(1f, cvars.VidBlackpoint);
        Assert.Equal(Black(1f, 0.5f), cvars.VidIBlackpoint);

        cvars.SetBlackpoint(-1f);
        Assert.Equal(0f, cvars.VidBlackpoint);
        Assert.Equal(Black(0f, 0.5f), cvars.VidIBlackpoint);

        cvars.VidIBlackpoint = 1f;
        cvars.SetWhitepoint(-4f);
        Assert.Equal(-1f, cvars.VidWhitepoint);
        Assert.Equal(White(-1f, 1f), cvars.VidIWhitepoint);

        cvars.SetWhitepoint(9f);
        Assert.Equal(2f, cvars.VidWhitepoint);
        Assert.Equal(White(2f, 1f), cvars.VidIWhitepoint);

        cvars.SetBlackpoint(0f);
        Assert.Equal(Black(0f, cvars.VidIWhitepoint), cvars.VidIBlackpoint);
        float black = cvars.VidIBlackpoint;
        cvars.SetWhitepoint(0f);
        Assert.Equal(0f, cvars.VidWhitepoint);
        Assert.Equal(White(0f, black), cvars.VidIWhitepoint);

        cvars.SetTextureFilterAnisotropic(-2f);
        Assert.Equal(-2f, cvars.TextureFilterAnisotropic);
        Assert.Equal(1, cvars.FilterModeCalls);
        cvars.SetTextureFilter(3);
        Assert.Equal(3, cvars.TextureFilter);
        Assert.Equal(2, cvars.FilterModeCalls);
        cvars.SetTextureFilter(9);
        Assert.Equal(6, cvars.TextureFilter);
        Assert.Equal(4, cvars.FilterModeCalls);
        cvars.SetTextureFilter(-2);
        Assert.Equal(6, cvars.TextureFilter);
        Assert.Equal(6, cvars.FilterModeCalls);
        cvars.SetShadowmapFilter(8);
        Assert.Equal(8, cvars.ShadowmapFilter);
        cvars.SetShadowmapFilter(9);
        Assert.Equal(0, cvars.ShadowmapFilter);
        cvars.SetShadowmapFilter(-1);
        Assert.Equal(0, cvars.ShadowmapFilter);
        Assert.Equal(6, cvars.FilterModeCalls);

        cvars.Precache = true;
        cvars.SatFormula = 0;
        cvars.Portals = false;
        cvars.Mirrors = false;
        cvars.MirrorEnvmap = false;
        cvars.Seamless = false;
        Assert.True(cvars.Precache);
        Assert.Equal(0, cvars.SatFormula);
        Assert.False(cvars.Portals || cvars.Mirrors || cvars.MirrorEnvmap || cvars.Seamless);
    }

    static float Black(float self, float white)
    {
        float squared = self * self;
        float bound = (float)(1 - VmHwCvars1307.BwGap);
        float buffer = (float)(white - VmHwCvars1307.BwGap);
        return Math.Min(Math.Min(buffer, squared), bound);
    }

    static float White(float self, float black)
    {
        float shaped = self + 1f;
        float bound = (float)(0 + VmHwCvars1307.BwGap);
        float buffer = (float)(black + VmHwCvars1307.BwGap);
        shaped = (shaped * shaped * shaped + 1f) / 2f;
        return Math.Max(Math.Max(buffer, shaped), bound);
    }
}
