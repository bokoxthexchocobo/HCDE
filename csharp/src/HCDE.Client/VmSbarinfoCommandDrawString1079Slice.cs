namespace HCDE.Client;

/// <summary>
/// <c>CommandDrawString::RealignString</c>, <c>SetStringToTag</c>, and <c>CommandDrawNumber</c> parse head from <c>sbarinfo_commands.cpp</c> (**901–1050**).
/// Chunk **1079**; follows **1078** TIME/global tick seam (**903–907** excluded). Alignment cases match native <c>StringAlignment</c> order.
/// </summary>
public static class VmSbarinfoCommandDrawString1079Slice
{
    public const int InvalidActorClassCache = -1;
    public const char ProportionalSpacingSentinel = '\0';

    public enum NativeStringAlignment
    {
        Right,
        Left,
        Center
    }

    public enum DrawNumberValueKind
    {
        Constant,
        Inventory,
        Health,
        Armor,
        Ammo1,
        Ammo2,
        Ammo1Capacity,
        Ammo2Capacity,
        Score
    }

    public static double ComputeProportionalRealignWidth(double stringWidth, int spacing, int charCount) =>
        stringWidth + spacing * charCount;

    public static double ComputeMonospaceRealignWidth(int charWidth, int spacing, int charCount) =>
        (charWidth + spacing) * charCount;

    public static int ApplyRealignHorizontalOffset(
        NativeStringAlignment alignment,
        int startX,
        double totalWidth)
    {
        return alignment switch
        {
            NativeStringAlignment.Left => startX,
            NativeStringAlignment.Right => startX - (int)totalWidth,
            NativeStringAlignment.Center => startX - (int)(totalWidth / 2),
            _ => startX
        };
    }

    public static int ResolveDrawStringXAfterRealign(
        NativeStringAlignment alignment,
        int startX,
        char spacingCharacter,
        double stringWidth,
        int spacing,
        int charCount,
        int monospaceCharWidth)
    {
        var width = spacingCharacter == ProportionalSpacingSentinel
            ? ComputeProportionalRealignWidth(stringWidth, spacing, charCount)
            : ComputeMonospaceRealignWidth(monospaceCharWidth, spacing, charCount);
        return ApplyRealignHorizontalOffset(alignment, startX, width);
    }

    public static (int Cache, string Text, bool ShouldRealign) ResolveTagStringWhenActorPresent(
        int priorCache,
        nint actorClassPointer,
        string actorTag)
    {
        if (priorCache == (int)actorClassPointer)
            return (priorCache, actorTag, false);
        return ((int)actorClassPointer, actorTag, true);
    }

    public static (int Cache, string Text) ResolveTagStringWhenActorMissing() =>
        (InvalidActorClassCache, string.Empty);

    public static (DrawNumberValueKind Kind, bool ParsedIntConst) ResolveDrawNumberValueSource(bool sawIntConst)
    {
        if (sawIntConst)
            return (DrawNumberValueKind.Constant, true);
        return (DrawNumberValueKind.Inventory, false);
    }

    public static (bool Recognized, DrawNumberValueKind Kind) TryClassifyDrawNumberValueKeyword(string identifier)
    {
        if (identifier.Equals("health", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.Health);
        if (identifier.Equals("armor", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.Armor);
        if (identifier.Equals("ammo1", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.Ammo1);
        if (identifier.Equals("ammo2", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.Ammo2);
        if (identifier.Equals("ammo1capacity", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.Ammo1Capacity);
        if (identifier.Equals("ammo2capacity", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.Ammo2Capacity);
        if (identifier.Equals("score", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberValueKind.Score);
        return (false, DrawNumberValueKind.Inventory);
    }
}
