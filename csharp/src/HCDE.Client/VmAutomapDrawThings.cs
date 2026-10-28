namespace HCDE.Client;

/// <summary><c>DAutomap::drawThings</c> and <c>drawKeys</c> policy from <c>am_map.cpp</c> (2991–3170).</summary>
public static class VmAutomapDrawThings
{
    public const uint Mf6NotOnAutomap = 1u << 6;
    public const uint RfInvisible = 1u << 0;
    public const uint Mf3IsMonster = 1u << 0;
    public const uint MfCorpse = 1u << 1;
    public const uint MfFriendly = 1u << 2;
    public const uint MfCountKill = 1u << 3;
    public const uint MfSpecial = 1u << 4;
    public const uint MfCountItem = 1u << 5;

    public static bool ShouldDrawThingOnMap(int amCheat, uint flags6, bool thingRenderStyles, uint renderFlags) =>
        amCheat > 0
        || (flags6 & Mf6NotOnAutomap) == 0
        || (thingRenderStyles && (renderFlags & RfInvisible) == 0 && (flags6 & Mf6NotOnAutomap) == 0);

    public static bool ShouldUseSpritePath(int amShowThingSprites, int sprite) =>
        amShowThingSprites > 0 && sprite > 0;

    public static int ComputeSpriteRotationIndex(
        double thingYawRadians,
        double spriteRotationRadians,
        bool multiRotationFrame,
        bool mapRotated,
        double cameraYawRadians)
    {
        var angle = Math.PI * 1.5 - thingYawRadians - spriteRotationRadians;
        if (multiRotationFrame)
            angle += Math.PI / 16;
        if (mapRotated)
            angle += cameraYawRadians - Math.PI / 2;
        var degrees = angle % (Math.PI * 2);
        if (degrees < 0)
            degrees += Math.PI * 2;
        return (int)(degrees * (16.0 / (Math.PI * 2)));
    }

    public static double ComputeSpriteScale(double scaleX, double scaleY, double scaleMtof) =>
        scaleX * (10.0 / 16.0) * scaleMtof;

    public enum ThingColorKind
    {
        Default,
        Friend,
        NoCountMonster,
        Monster,
        Item,
        CountItem,
        Suppressed,
    }

    public static ThingColorKind ClassifyThingTriangleColor(
        uint flags,
        uint flags3,
        bool isKeyKind,
        bool easyKey,
        bool showKeysAlways,
        bool showKeys,
        int keyMapColor)
    {
        if ((flags3 & Mf3IsMonster) != 0 && (flags & MfCorpse) == 0)
        {
            if ((flags & MfFriendly) != 0)
                return ThingColorKind.Friend;
            if ((flags & MfCountKill) == 0)
                return ThingColorKind.NoCountMonster;
            return ThingColorKind.Monster;
        }

        if ((flags & MfSpecial) == 0)
            return ThingColorKind.Default;

        if (isKeyKind)
        {
            if (easyKey || showKeysAlways)
                return ThingColorKind.Suppressed;
            if (showKeys && keyMapColor >= 0)
                return ThingColorKind.CountItem;
            return ThingColorKind.Item;
        }

        if ((flags & MfCountItem) != 0)
            return ThingColorKind.CountItem;
        return ThingColorKind.Item;
    }

    public static bool ShouldDrawRadiusDebugBox(int amCheat) => amCheat == 3 || amCheat == 6;

    public static bool IsInventoryKeyType(bool dehackedPickup, bool isKeyDescendant) =>
        !dehackedPickup && isKeyDescendant;

    public static bool ShouldDrawKeyIcon(bool easyKey, bool showKeysAlways, bool showKeys) =>
        easyKey || showKeysAlways || showKeys;
}
