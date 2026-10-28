namespace HCDE.Client;

/// <summary>
/// SBarInfo coordinate parsing, flow-control, and main-block head from <c>sbarinfo.cpp</c> (**151–300**).
/// Chunk **1063**; <c>FScanner</c>, <c>V_ParseFontColor</c>, and command factory remain host-injected.
/// </summary>
public static class VmSbarinfoParser1063Slice
{
    public enum MainBlockHudFlag
    {
        None,
        ForceScaled,
        FullScreenOffsets,
        Unknown
    }

    public enum FlowBlockParseShape
    {
        BracedBlock,
        SingleCommand,
        MissingCommand
    }

    public static int ResolveSignedCoordinate(bool negative, int magnitude) =>
        negative ? -magnitude : magnitude;

    public static bool ValidateCenterKeyword(string token) =>
        token.Equals("center", StringComparison.OrdinalIgnoreCase);

    public static bool ShouldStoreRelativeCenter(bool fullScreenOffsets, bool hasCenterModifier) =>
        fullScreenOffsets && hasCenterModifier;

    public static string BuildNamedTranslationToken(string colorName) => $"[{colorName}]";

    public static bool IsMissingTranslation(int parsedColor) =>
        parsedColor == VmVFontParseInit1062Slice.ColorUndefined;

    public static bool ShouldSkipFlowTruthToggle(bool currentTruth, bool newTruth) =>
        currentTruth == newTruth;

    public static bool ShouldRetickOnTruthChange(bool blockPresent) => blockPresent;

    public static FlowBlockParseShape ClassifyFlowBlockStart(bool hasOpeningBrace, bool hasSingleCommand) =>
        hasOpeningBrace ? FlowBlockParseShape.BracedBlock :
        hasSingleCommand ? FlowBlockParseShape.SingleCommand :
        FlowBlockParseShape.MissingCommand;

    public static int SelectActiveFlowBranch(bool truth) => truth ? 1 : 0;

    public static bool TryParseNotPrefix(string? token, out bool negate)
    {
        negate = token != null && token.Equals("not", StringComparison.OrdinalIgnoreCase);
        return negate;
    }

    public static bool ShouldSwapFlowBranchesAfterNot(bool negate) => negate;

    public static bool MainBlockStartsTrue() => true;

    public const int FlowThenBranchIndex = 1;
    public const int FlowElseBranchIndex = 0;

    public static MainBlockHudFlag ClassifyMainBlockFlag(string token)
    {
        if (token.Equals("forcescaled", StringComparison.OrdinalIgnoreCase))
            return MainBlockHudFlag.ForceScaled;
        if (token.Equals("fullscreenoffsets", StringComparison.OrdinalIgnoreCase))
            return MainBlockHudFlag.FullScreenOffsets;
        return MainBlockHudFlag.Unknown;
    }

    public static bool ShouldContinueMainBlockFlagList(bool hasPipeSeparator, bool hasCommaSeparator) =>
        hasPipeSeparator || hasCommaSeparator;

    public static double CombineDrawAlpha(double blockAlpha, double incomingAlpha) =>
        blockAlpha * incomingAlpha;

    public static bool MainBlockParseHasLeadingComma(bool hasCommaAfterBlockName) =>
        hasCommaAfterBlockName;
}
