namespace HCDE.Gamedata;

public enum VmUMapPlayerActionOption
{
    Disallow,
    Allow,
    Require,
    Unset
}

/// <summary>UMapInfo token helpers from <c>umapinfo.cpp</c>.</summary>
public static class VmUMapInfoTokenParser
{
    public static bool TryParseMultiString(IReadOnlyList<string> tokens, ref int i, out string result, List<string> errors)
    {
        result = "";
        if (i >= tokens.Count) { errors.Add("Expected string."); return false; }
        if (tokens[i].Equals("clear", StringComparison.OrdinalIgnoreCase))
        {
            i++;
            result = "-";
            return true;
        }

        if (tokens[i].StartsWith('"'))
        {
            var parts = new List<string>();
            while (i < tokens.Count && tokens[i].StartsWith('"'))
            {
                parts.Add(tokens[i].Trim('"'));
                i++;
                if (i < tokens.Count && tokens[i] == ",") i++;
            }

            result = string.Join('\n', parts);
            return true;
        }

        errors.Add("Either 'clear' or string constant expected");
        return false;
    }

    public static bool TryParseLumpName(string token, Span<char> buffer, List<string> errors)
    {
        if (token.Length > 8)
        {
            errors.Add("String too long. Maximum size is 8 characters.");
            return false;
        }

        buffer.Clear();
        token.ToUpperInvariant().AsSpan().CopyTo(buffer);
        return true;
    }

    public static bool TryParsePlayerActionOption(string token, out VmUMapPlayerActionOption option)
    {
        if (token.Equals("disallow", StringComparison.OrdinalIgnoreCase)) { option = VmUMapPlayerActionOption.Disallow; return true; }
        if (token.Equals("allow", StringComparison.OrdinalIgnoreCase)) { option = VmUMapPlayerActionOption.Allow; return true; }
        if (token.Equals("require", StringComparison.OrdinalIgnoreCase)) { option = VmUMapPlayerActionOption.Require; return true; }
        option = VmUMapPlayerActionOption.Unset;
        return false;
    }
}
