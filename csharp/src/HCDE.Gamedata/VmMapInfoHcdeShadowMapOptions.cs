namespace HCDE.Gamedata;

public sealed class VmMapInfoHcdeShadowOverrides
{
    public int ShadowProfile { get; set; } = int.MinValue;
    public int ShadowQualityCap { get; set; } = int.MinValue;
    public int ShadowMaxLightsCap { get; set; } = int.MinValue;
}

/// <summary>MAPINFO <c>hcde_shadow*</c> option bodies from <c>g_mapinfo.cpp</c> (1738–1826).</summary>
public static class VmMapInfoHcdeShadowMapOptions
{
    public static void ApplyShadowProfile(VmMapInfoHcdeShadowOverrides overrides, int profile) =>
        overrides.ShadowProfile = VmMapInfoHcdeShadowOptions.ClampShadowProfile(profile);

    public static void ApplyShadowQualityCap(VmMapInfoHcdeShadowOverrides overrides, int qualityCap) =>
        overrides.ShadowQualityCap = qualityCap < 0
            ? -1
            : VmMapInfoHcdeShadowOptions.NormalizeShadowQualityCap(qualityCap);

    public static void ApplyShadowMaxLightsCap(VmMapInfoHcdeShadowOverrides overrides, int maxLights) =>
        overrides.ShadowMaxLightsCap = VmMapInfoHcdeShadowOptions.ClampShadowMaxLights(maxLights);
}
