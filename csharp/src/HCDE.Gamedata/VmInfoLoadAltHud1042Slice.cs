namespace HCDE.Gamedata;

/// <summary>
/// <c>GetSpriteIndex</c> push tail, <c>LoadAltHudStuff</c>, and <c>PClassActor::StaticInit</c> head from <c>info.cpp</c> (**301–450**).
/// Chunk **1042**; follows **1041** (**151–300**). Lookup/cache: uncredited <see cref="VmSpriteCatalog"/>; default switch seam: <see cref="VmAltHudLoader"/>.
/// </summary>
public static class VmInfoLoadAltHud1042Slice
{
    public const int InvalidTextureIndex = -1;

    public enum AltHudKeywordKind
    {
        Health,
        Berserk,
        InventoryClass,
    }

    public enum AltHudInventoryClassStatus
    {
        Ok,
        UnknownClass,
        NotInventory,
    }

    public readonly record struct SpriteDefDraft(string Name, int NumFrames, int SpriteFramesPointer);

    public readonly record struct DefaultAltHudPicNames(string HealthTexture, string? BerserkTexture);

    public readonly record struct AltHudInventoryApplyResult(
        AltHudInventoryClassStatus Status,
        bool ApplyIcon,
        string? IconTextureName,
        bool IconExplicitlyInvalid);

    public static SpriteDefDraft CreateSpriteDefDraft(string normalizedName) =>
        new(normalizedName, 0, 0);

    public static AltHudKeywordKind ClassifyAltHudKeyword(string keyword)
    {
        if (keyword.Equals("Health", StringComparison.OrdinalIgnoreCase))
            return AltHudKeywordKind.Health;
        if (keyword.Equals("Berserk", StringComparison.OrdinalIgnoreCase))
            return AltHudKeywordKind.Berserk;
        return AltHudKeywordKind.InventoryClass;
    }

    public static DefaultAltHudPicNames ResolveDefaultAltHudPicNames(VmGameTypeHud gameType) =>
        gameType switch
        {
            VmGameTypeHud.Heretic or VmGameTypeHud.Hexen =>
                new DefaultAltHudPicNames("ARTIPTN2", null),
            VmGameTypeHud.Strife => new DefaultAltHudPicNames("I_MDKT", null),
            _ => new DefaultAltHudPicNames("MEDIA0", "PSTRA0"),
        };

    public static bool IsExplicitNullAltHudTextureToken(string token) =>
        token.Equals("0", StringComparison.OrdinalIgnoreCase)
        || token.Equals("NULL", StringComparison.OrdinalIgnoreCase)
        || token.Length == 0;

    public static AltHudInventoryClassStatus EvaluateAltHudInventoryClass(bool classFound, bool isInventoryDescendant)
    {
        if (!classFound)
            return AltHudInventoryClassStatus.UnknownClass;
        if (!isInventoryDescendant)
            return AltHudInventoryClassStatus.NotInventory;
        return AltHudInventoryClassStatus.Ok;
    }

    public static AltHudInventoryApplyResult BuildAltHudInventoryApply(
        AltHudInventoryClassStatus classStatus,
        string textureToken)
    {
        if (classStatus != AltHudInventoryClassStatus.Ok)
            return new AltHudInventoryApplyResult(classStatus, false, null, false);

        if (IsExplicitNullAltHudTextureToken(textureToken))
            return new AltHudInventoryApplyResult(classStatus, true, null, true);

        return new AltHudInventoryApplyResult(classStatus, true, textureToken, false);
    }

    public static string FormatUnknownAltHudClassMessage(string className) =>
        $"Unknown item class '{className}' in ALTHUDCF";

    public static string FormatInvalidAltHudClassMessage(string className) =>
        $"Invalid item class '{className}' in ALTHUDCF";

    public static bool NeedsDefaultSpriteSeed(int spriteCount) => spriteCount == 0;

    public static IReadOnlyList<string> DefaultSpriteSeedNames { get; } = ["TNT1", "----", "####"];

    public static bool ShouldAddActorClassToRegistry(bool isDescendantOfActor) => isDescendantOfActor;

    public static void RunRegisterIdsForAllActorClasses(IReadOnlyList<Action> registerIdsCallbacks)
    {
        for (var i = 0; i < registerIdsCallbacks.Count; i++)
            registerIdsCallbacks[i]();
    }

    public static IReadOnlyList<string> StaticInitPostActorLoadSteps { get; } =
        ["LoadAltHudStuff", "InitBotStuff", "InitStaticHandlers"];
}
