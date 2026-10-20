namespace HCDE.Client;

/// <summary>Native <c>gameconfigfile.cpp</c> <c>LegacyGammaToFixGamma</c>.</summary>
public static class VmGameConfigLegacyGamma
{
    public static float LegacyGammaToFixGamma(float value)
    {
        if (value < 0.1f) value = 0.1f;
        else if (value > 3.0f) value = 3.0f;
        return (value - 2.2f) / (3.0f - 2.2f);
    }
}
