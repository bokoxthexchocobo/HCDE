namespace HCDE.Playsim;

/// <summary>
/// <c>SetDynamicLights</c> tail, <c>DeleteAttachedLights</c>, <c>FindUserLight</c>, <c>AttachLightDef</c>,
/// <c>AttachLightDirect</c>, and <c>RemoveLight</c> head from <c>a_dynlight.cpp</c> (**751–900**).
/// Exclusive to chunk **955**.
/// </summary>
public static class VmADynLightAttach955Slice
{
    public const uint UserLightNotFound = uint.MaxValue;
    public const int OfEuthanizeMe = 1 << 1;
    public const int Mf8RecreateLights = 1 << 0;
    public const int Level3LightCreated = 1 << 0;
    public const int AnyFrameAssociation = -1;

    public static bool ShouldAttachStateLightEntry(bool isNullSentinelMarker) => !isNullSentinelMarker;

    public static bool LightAssociationMatches(int sprite, int frame, int associationSprite, int associationFrame) =>
        associationSprite == sprite && (associationFrame == frame || associationFrame == AnyFrameAssociation);

    public static bool ShouldFallbackToStateLights(int attachedCount, int stateLightIndex) =>
        attachedCount == 0 && stateLightIndex > 0;

    public static bool ShouldDeactivateExtraLight(int count, int attachedLightCount) => count < attachedLightCount;

    public static bool AbortLightAttachWhenEuthanizing(int objectFlags) => (objectFlags & OfEuthanizeMe) != 0;

    public static bool LightDefaultsIndexValid(int index, int collectionSize) => index < collectionSize;

    public static int ApplyRecreateLightsActorFlags(int flags8) => flags8 | Mf8RecreateLights;

    public static int ApplyLevelLightCreatedFlags(int levelFlags3) => levelFlags3 | Level3LightCreated;

    public static uint FindUserLightIndex(ReadOnlySpan<string> names, string id, bool create, out bool created)
    {
        created = false;
        for (var i = 0; i < names.Length; i++)
        {
            if (names[i] == id)
                return (uint)i;
        }
        if (!create)
            return UserLightNotFound;
        created = true;
        return (uint)names.Length;
    }

    public static int ExtractColorRed(int color) => (color >> 16) & 0xFF;

    public static int ExtractColorGreen(int color) => (color >> 8) & 0xFF;

    public static int ExtractColorBlue(int color) => color & 0xFF;

    public static float[] AttachLightOffsetComponents(double ofsX, double ofsY, double ofsZ) =>
        [(float)ofsX, (float)ofsZ, (float)ofsY];

    public static double AttachLightTypeParameter(int lightType, double param) =>
        lightType == VmADynLightTick.PulseLight
            ? param * VmADynLightTick.Ticrate
            : param * 360.0;

    public static bool SpotPitchInRange(double spotPitch) => spotPitch >= -90.0 && spotPitch <= 90.0;

    public static bool RemoveLightIndexValid(uint index, int userLightCount) => index < (uint)userLightCount;

    public static bool ShouldCopyLightDefaults(int findIndex, int defaultsCount) =>
        findIndex < defaultsCount;

    public static int UserLightSlotAfterCreate(uint index, bool created, int previousCount) =>
        created ? previousCount : (int)index;

    public static bool HasAttachedLightSlot(int count, int attachedLightCount) => count < attachedLightCount;

    public static bool ReuseExistingAttachedLight(int count, int attachedLightCount) =>
        count < attachedLightCount;
}
