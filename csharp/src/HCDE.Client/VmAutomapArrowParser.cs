namespace HCDE.Client;

/// <summary>`AM_ParseArrow` / `AM_StaticInit` from <c>am_map.cpp</c> (853–917).</summary>
public readonly record struct VmAutomapVectorLine(double Ax, double Ay, double Bx, double By);

public static class VmAutomapArrowParser
{
    public const int DefaultMarkPointCount = 10;

    public static int ComputeArrowRadius(double playerRadius = VmAutomapCvarModel.PlayerRadius) =>
        (int)(8 * playerRadius / 7);

    public static VmAutomapVectorLine ScaleSegment(double x1, double y1, double x2, double y2, int radius) =>
        new(x1 * radius, y1 * radius, x2 * radius, y2 * radius);

    public static IReadOnlyList<VmAutomapVectorLine> ParseArrowLump(string text, int radius)
    {
        var lines = new List<VmAutomapVectorLine>();
        var i = 0;
        while (TryReadSegment(text, ref i, radius, out var seg))
            lines.Add(seg);
        return lines;
    }

    public static bool TryReadSegment(string text, ref int index, int radius, out VmAutomapVectorLine line)
    {
        line = default;
        if (!TrySkipTo('(', text, ref index))
            return false;
        if (!TryReadFloat(text, ref index, out var x1) || !TryReadComma(text, ref index) ||
            !TryReadFloat(text, ref index, out var y1) || !TrySkipTo(')', text, ref index) ||
            !TrySkipTo(',', text, ref index) || !TrySkipTo('(', text, ref index) ||
            !TryReadFloat(text, ref index, out var x2) || !TryReadComma(text, ref index) ||
            !TryReadFloat(text, ref index, out var y2) || !TrySkipTo(')', text, ref index))
            return false;

        line = ScaleSegment(x1, y1, x2, y2, radius);
        return true;
    }

    public static string FormatMarkTextureName(int index) => $"AMMNUM{index}";

    public static void ValidatePrimaryArrowDefined(int mapArrowLineCount)
    {
        if (mapArrowLineCount == 0)
            throw new InvalidOperationException("No automap arrow defined");
    }

    public static VmAutomapVectorLine[] DefaultThinTriangle => new[]
    {
        ScaleSegment(-.5, -.7, 1, 0, ComputeArrowRadius()),
        ScaleSegment(1, 0, -.5, .7, ComputeArrowRadius()),
        ScaleSegment(-.5, .7, -.5, -.7, ComputeArrowRadius()),
    };

    private static bool TrySkipTo(char ch, string text, ref int index)
    {
        while (index < text.Length)
        {
            if (text[index] == ch)
            {
                index++;
                return true;
            }

            index++;
        }

        return false;
    }

    private static bool TryReadComma(string text, ref int index)
    {
        while (index < text.Length && char.IsWhiteSpace(text[index]))
            index++;
        if (index >= text.Length || text[index] != ',')
            return false;
        index++;
        return true;
    }

    private static bool TryReadFloat(string text, ref int index, out double value)
    {
        while (index < text.Length && char.IsWhiteSpace(text[index]))
            index++;
        var start = index;
        while (index < text.Length && (char.IsDigit(text[index]) || text[index] == '.' || text[index] == '-' || text[index] == '+'))
            index++;
        if (start == index)
        {
            value = 0;
            return false;
        }

        return double.TryParse(text.AsSpan(start, index - start), out value);
    }
}
