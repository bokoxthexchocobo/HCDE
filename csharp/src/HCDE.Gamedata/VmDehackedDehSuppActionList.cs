namespace HCDE.Gamedata;

/// <summary><c>LoadDehSupp</c> <c>ActionList</c> section from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedDehSuppActionList
{
    public static string ResolveActionSymbol(string token)
    {
        if (token.Equals("NULL", StringComparison.OrdinalIgnoreCase))
            return string.Empty;
        if (token.Equals("CPosAttack", StringComparison.OrdinalIgnoreCase))
            return "A_CPosAttackDehacked";
        return "A_" + token;
    }

    public static bool TryParseActionToken(string token, out string? symbol)
    {
        symbol = null;
        if (token.Equals("NULL", StringComparison.OrdinalIgnoreCase))
        {
            symbol = null;
            return true;
        }

        if (string.IsNullOrWhiteSpace(token))
            return false;
        symbol = ResolveActionSymbol(token.Trim());
        return symbol.Length > 0;
    }

    public static List<string?> ParseActionList(IEnumerable<string> tokens)
    {
        var list = new List<string?>();
        foreach (var raw in tokens)
        {
            var token = raw.Trim();
            if (token == ",") continue;
            if (!TryParseActionToken(token, out var symbol))
                break;
            list.Add(symbol);
        }

        return list;
    }
}
