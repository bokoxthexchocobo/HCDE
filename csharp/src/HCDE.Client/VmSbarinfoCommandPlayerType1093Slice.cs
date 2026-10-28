namespace HCDE.Client;

/// <summary>
/// <c>CommandPlayerType</c> parse loop tail and descendant <c>Tick</c> from <c>sbarinfo_commands.cpp</c> (**3001–3031**).
/// Chunk **1093**; **1092** owns parse head through class lookup (**2985–3000**).
/// </summary>
public static class VmSbarinfoCommandPlayerType1093Slice
{
    public static bool ResolvePlayerTypeClassListLoopContinues(
        bool nextIdentifierPresent,
        bool nextStringConstPresent) => nextIdentifierPresent || nextStringConstPresent;

    public static bool ResolvePlayerTypeFlowControlTruthFromDescendants(
        bool playerClassMissing,
        bool spawnClassDescendsFromAnyListedType) =>
        !playerClassMissing && spawnClassDescendsFromAnyListedType;
}

/// <summary>
/// <c>CommandHasWeaponPiece</c> from the same native band (**3038–3080**).
/// </summary>
public static class VmSbarinfoCommandHasWeaponPiece1093Slice
{
    public const int DefaultWeaponPieceIndex = 1;

    public static int CreateHasWeaponPieceDefaultPieceIndex() => DefaultWeaponPieceIndex;

    public static bool ShouldReplaceHasWeaponPieceWeaponClass(bool classFound, bool isWeaponKind) =>
        !classFound || !isWeaponKind;

    public static (bool Valid, int NormalizedPiece) ValidateHasWeaponPieceNumber(int pieceNumber) =>
        pieceNumber < 1
            ? (Valid: false, NormalizedPiece: DefaultWeaponPieceIndex)
            : (Valid: true, NormalizedPiece: pieceNumber);

    public static bool ResolveHasWeaponPieceFlowControlTruth(
        bool pieceWeaponMatches,
        int pieceMask,
        int pieceIndex) =>
        pieceWeaponMatches && (pieceMask & (1 << (pieceIndex - 1))) != 0;
}

/// <summary>
/// <c>CommandDrawGem</c> ctor/draw/parse head from the same native band (**3088–3150**).
/// </summary>
public static class VmSbarinfoCommandDrawGem1093Slice
{
    public const int MissingImageIndex = -1;
    public const int DefaultChainSize = 1;

    public readonly record struct DrawGemCtorDefaults(
        bool Wiggle,
        bool Translatable,
        bool Armor,
        bool Reverse,
        int Chain,
        int Gem,
        int LeftPadding,
        int RightPadding,
        int ChainSize,
        int InterpolationSpeed,
        double DrawValue,
        int GoalValue,
        int ChainWiggle);

    public static DrawGemCtorDefaults CreateDrawGemCtorDefaults() =>
        new(
            Wiggle: false,
            Translatable: false,
            Armor: false,
            Reverse: false,
            Chain: MissingImageIndex,
            Gem: MissingImageIndex,
            LeftPadding: 0,
            RightPadding: 0,
            ChainSize: DefaultChainSize,
            InterpolationSpeed: 0,
            DrawValue: 0,
            GoalValue: 0,
            ChainWiggle: 0);

    public static (bool DrawChain, bool DrawGemOverlay) ResolveDrawGemDrawOutcome(
        bool chainImageMissing,
        bool gemImagePresent) =>
        chainImageMissing
            ? (DrawChain: false, DrawGemOverlay: false)
            : (DrawChain: true, DrawGemOverlay: gemImagePresent);
        bool wiggleEnabled,
        double drawValue,
        int goalValue,
        int baseY,
        int chainWiggle) =>
        wiggleEnabled && drawValue != goalValue ? baseY + chainWiggle : baseY;

    public static int ComputeDrawGemChainHorizontalOffset(
        double chainWidth,
        int leftPadding,
        int rightPadding,
        double drawValue,
        int chainSize)
    {
        var span = chainWidth - leftPadding - rightPadding;
        var offset = (int)(span / 100.0 * drawValue);
        return offset % chainSize;
    }

    public enum DrawGemParseFlag
    {
        None,
        Wiggle,
        Translatable,
        Armor,
        Interpolate,
        Reverse,
        Unknown
    }

    public static (bool Recognized, DrawGemParseFlag Flag) TryClassifyDrawGemParseFlagKeyword(string identifier)
    {
        if (identifier.Equals("wiggle", StringComparison.OrdinalIgnoreCase))
            return (true, DrawGemParseFlag.Wiggle);
        if (identifier.Equals("translatable", StringComparison.OrdinalIgnoreCase))
            return (true, DrawGemParseFlag.Translatable);
        if (identifier.Equals("armor", StringComparison.OrdinalIgnoreCase))
            return (true, DrawGemParseFlag.Armor);
        if (identifier.Equals("interpolate", StringComparison.OrdinalIgnoreCase))
            return (true, DrawGemParseFlag.Interpolate);
        if (identifier.Equals("reverse", StringComparison.OrdinalIgnoreCase))
            return (true, DrawGemParseFlag.Reverse);
        return (false, DrawGemParseFlag.Unknown);
    }

    public static (bool ContinueFlagLoop, bool RequireCommaBeforeChainImage) ResolveDrawGemParseFlagListContinuation(
        bool nextTokenIsPipe) =>
        nextTokenIsPipe
            ? (ContinueFlagLoop: true, RequireCommaBeforeChainImage: false)
            : (ContinueFlagLoop: false, RequireCommaBeforeChainImage: true);

    public static int ApplyDrawGemSignedPaddingToken(bool negative, int magnitude) =>
        (negative ? -1 : 1) * magnitude;
}
