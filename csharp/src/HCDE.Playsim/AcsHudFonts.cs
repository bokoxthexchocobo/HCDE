namespace HCDE.Playsim;

/// <summary>Native <c>V_GetFont</c> subset for ACS <c>CheckFont</c>.</summary>
internal static class AcsHudFonts
{
    private static readonly HashSet<string> Known = new(StringComparer.OrdinalIgnoreCase)
    {
        "SmallFont",
        "SMALLFNT",
        "BigFont",
        "BIGFONT",
        "DBIGFONT",
        "HBIGFONT",
        "SBIGFONT",
        "ConsoleFont",
        "CONFONT",
        "IndexFont",
        "INDEXFON",
        "SmallFont2",
        "OptBigFont",
        "IntermissionFont",
        "MessageFont",
        "MessageFontSmall",
        "MessageFontBig",
        "BudgetFont",
        "NewConsoleFont",
    };

    public static bool Exists(string? fontName) =>
        !string.IsNullOrWhiteSpace(fontName) && Known.Contains(fontName.Trim());
}
