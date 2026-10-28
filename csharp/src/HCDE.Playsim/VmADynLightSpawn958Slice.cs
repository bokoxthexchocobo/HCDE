namespace HCDE.Playsim;

/// <summary>
/// <c>GetLight</c>, actor <c>AttachLight</c>, and <c>ActivateLight</c> from <c>a_dynlight.cpp</c> (**1–150**).
/// Exclusive to chunk **958**; tick/activate body remains chunk **823**.
/// </summary>
public static class VmADynLightSpawn958Slice
{
    public const double InvalidSpawnLightX = -10_000_000;
    public const int DefaultShadowmapIndex = 1024;
    public const float DefaultSoftShadowRadius = -1f;
    public const float DefaultLinearity = -1f;
    public const int DefaultShadowMinQuality = -1;
    public const double DefaultLightDefIntensity = 1.0;

    public static bool ShouldPopLightFromFreeList(int freeListCount) => freeListCount > 0;

    public static bool ShouldAllocateLightFromArena(int freeListCount) => freeListCount == 0;

    public static bool LinkNewLightHasSuccessor(object? nextLight) => nextLight is not null;

    public static bool AbortActorLightAttach(int objectFlags) =>
        VmADynLightAttach955Slice.AbortLightAttachWhenEuthanizing(objectFlags);

    public static float ResolveUdmfSoftShadowRadius(float udmfValue, float currentValue) =>
        udmfValue >= 0f ? udmfValue : currentValue;

    public static float ResolveUdmfLinearity(float udmfValue, float currentValue) =>
        udmfValue >= 0f ? udmfValue : currentValue;

    public static int ResolveUdmfShadowMinQuality(int udmfValue, int currentValue) =>
        udmfValue >= 0 ? udmfValue : currentValue;

    public static (float SoftShadowRadius, float Linearity, int ShadowMinQuality) ApplyUdmfSpawnOverrides(
        float softUdmf,
        float softCurrent,
        float linearUdmf,
        float linearCurrent,
        int qualityUdmf,
        int qualityCurrent) =>
        (
            ResolveUdmfSoftShadowRadius(softUdmf, softCurrent),
            ResolveUdmfLinearity(linearUdmf, linearCurrent),
            ResolveUdmfShadowMinQuality(qualityUdmf, qualityCurrent));

    public static int ClearRecreateLightsFlag(int flags8) =>
        flags8 & ~VmADynLightAttach955Slice.Mf8RecreateLights;

    public static bool AttachedLightStartsInactive(bool activeFlag) => !activeFlag;

    public const bool DefaultVisibleToPlayer = true;

    public static bool ShouldLinkPreviousLight(object? nextLight) => nextLight is not null;

    public static byte LightTypeFromSpawnInt(int lightType) => (byte)lightType;

    public static double LightDefIntensityFromSpawn() => DefaultLightDefIntensity;

    public static double NormalizeSpawnAngleDegrees(double spawnAngleDegrees)
    {
        var normalized = spawnAngleDegrees % 360.0;
        if (normalized < 0)
            normalized += 360.0;
        return normalized;
    }

    public const bool DefaultCyclerIncrement = true;

    public static (double X, double Y, double Z) InitialOffMapPosition() =>
        (InvalidSpawnLightX, 0, 0);

    public static int ArenaAllocationSize(int dynamicLightSize, int poolCapacity) =>
        dynamicLightSize * poolCapacity;

    public static void ActivateAllAttachedLights(IEnumerable<object?> lights, Action<object> activate)
    {
        foreach (var light in lights)
        {
            if (light is not null)
                activate(light);
        }
    }
}
