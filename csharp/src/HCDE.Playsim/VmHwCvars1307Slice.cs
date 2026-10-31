namespace HCDE.Playsim;

/// <summary>
/// Fog, gamma, black point, and texture-filter cvars from <c>hw_cvars.cpp</c> (1-203). Chunk 1307.
/// </summary>
public sealed class VmHwCvars1307
{
    public const float GammaDefault = 2.2f;
    public const float GammaHigh = 3.0f;
    public const float GammaLow = 0.1f;
    public const float GammaLowFix = (GammaLow - GammaDefault) / (GammaHigh - GammaDefault);
    public const double BwGap = 0.2;

    static bool _gammaSyncing;

    public int FogMode { get; private set; } = 2;
    public bool Portals { get; set; } = true;
    public bool Mirrors { get; set; } = true;
    public bool MirrorEnvmap { get; set; } = true;
    public bool Seamless { get; set; } = true;
    public int PortalRecursions { get; private set; } = 4;
    public bool PlaneReflectionI { get; private set; }
    public bool PlaneReflection { get; private set; } = true;
    public float VidGamma { get; private set; } = GammaDefault;
    public float VidGammaCompat { get; private set; } = GammaDefault;
    public float VidFixGamma { get; private set; }
    public float VidContrast { get; private set; } = 1f;
    public float VidSaturation { get; private set; } = 1f;
    public float VidIBlackpoint { get; set; } = 1f;
    public float VidIWhitepoint { get; set; } = 1f;
    public float VidBlackpoint { get; private set; }
    public float VidWhitepoint { get; private set; }
    public int SatFormula { get; set; } = 2;
    public float TextureFilterAnisotropic { get; private set; } = 16f;
    public int TextureFilter { get; private set; } = 6;
    public bool Precache { get; set; }
    public int ShadowmapFilter { get; private set; }
    public int FilterModeCalls { get; private set; }

    public static bool GammaSyncing => _gammaSyncing;

    public static void ResetProcess()
    {
        _gammaSyncing = false;
    }

    public static float ClampGamma(float value)
    {
        if (value < GammaLow)
            return GammaLow;
        if (value > GammaHigh)
            return GammaHigh;
        return value;
    }

    public static float GammaToFixGamma(float value)
    {
        return (ClampGamma(value) - GammaDefault) / (GammaHigh - GammaDefault);
    }

    public static float FixGammaToGamma(float value)
    {
        return ClampGamma(value * (GammaHigh - GammaDefault) + GammaDefault);
    }

    public void SetFogMode(int value)
    {
        FogMode = value;
        if (FogMode > 2)
            SetFogMode(2);
        if (FogMode < 0)
            SetFogMode(0);
    }

    public void SetPortalRecursions(int value)
    {
        PortalRecursions = value;
        if (PortalRecursions > 16)
            SetPortalRecursions(16);
        if (PortalRecursions < 0)
            SetPortalRecursions(0);
    }

    public void SetPlaneReflection(bool value)
    {
        PlaneReflection = value;
        PlaneReflectionI = value;
    }

    public void SetVidGamma(float value)
    {
        VidGamma = value;
        float clamped = ClampGamma(VidGamma);
        if (_gammaSyncing)
            return;
        _gammaSyncing = true;
        if (clamped != VidGamma)
            SetVidGamma(clamped);
        SetVidGammaCompat(clamped);
        SetVidFixGamma(GammaToFixGamma(clamped));
        _gammaSyncing = false;
    }

    public void SetVidGammaCompat(float value)
    {
        VidGammaCompat = value;
        float clamped = ClampGamma(VidGammaCompat);
        if (_gammaSyncing)
            return;
        _gammaSyncing = true;
        if (clamped != VidGammaCompat)
            SetVidGammaCompat(clamped);
        SetVidGamma(clamped);
        SetVidFixGamma(GammaToFixGamma(clamped));
        _gammaSyncing = false;
    }

    public void SetVidFixGamma(float value)
    {
        VidFixGamma = value;
        float clamped = Math.Clamp(VidFixGamma, GammaLowFix, 1f);
        if (_gammaSyncing)
            return;
        _gammaSyncing = true;
        if (clamped != VidFixGamma)
            SetVidFixGamma(clamped);
        SetVidGamma(FixGammaToGamma(clamped));
        _gammaSyncing = false;
    }

    public void SetContrast(float value)
    {
        VidContrast = value;
        if (VidContrast < 0)
            SetContrast(0);
        else if (VidContrast > 5)
            SetContrast(5);
    }

    public void SetSaturation(float value)
    {
        VidSaturation = value;
        if (VidSaturation < -3)
            SetSaturation(-3);
        else if (VidSaturation > 3)
            SetSaturation(3);
    }

    public void SetBlackpoint(float value)
    {
        VidBlackpoint = value;
        if (VidBlackpoint < 0)
            SetBlackpoint(0);
        if (VidBlackpoint > 1)
            SetBlackpoint(1);
        float squared = VidBlackpoint * VidBlackpoint;
        float bound = (float)(1 - BwGap);
        float buffer = (float)(VidIWhitepoint - BwGap);
        VidIBlackpoint = Math.Min(Math.Min(buffer, squared), bound);
    }

    public void SetWhitepoint(float value)
    {
        VidWhitepoint = value;
        if (VidWhitepoint < -1)
            SetWhitepoint(-1);
        if (VidWhitepoint > 2)
            SetWhitepoint(2);
        float shaped = VidWhitepoint + 1;
        float bound = (float)(0 + BwGap);
        float buffer = (float)(VidIBlackpoint + BwGap);
        shaped = (shaped * shaped * shaped + 1f) / 2f;
        VidIWhitepoint = Math.Max(Math.Max(buffer, shaped), bound);
    }

    public void SetTextureFilterAnisotropic(float value)
    {
        TextureFilterAnisotropic = value;
        FilterModeCalls++;
    }

    public void SetTextureFilter(int value)
    {
        TextureFilter = value;
        if (TextureFilter < 0 || TextureFilter > 6)
            SetTextureFilter(6);
        FilterModeCalls++;
    }

    public void SetShadowmapFilter(int value)
    {
        ShadowmapFilter = value;
        if (ShadowmapFilter < 0 || ShadowmapFilter > 8)
            SetShadowmapFilter(0);
    }
}
