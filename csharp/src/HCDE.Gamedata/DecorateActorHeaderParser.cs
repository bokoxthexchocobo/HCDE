namespace HCDE.Gamedata;

/// <summary>Native <c>ParseActorHeader</c> / <c>CreateNewActor</c> name and metadata parsing without ZScript class registration.</summary>
public static class DecorateActorHeaderParser
{
    public const int MaxDoomEdNum = 32767;

    public sealed class ActorHeaderParseResult
    {
        public string TypeName { get; set; } = "Actor";
        public string ParentName { get; set; } = "Actor";
        public string? ReplacesType { get; set; }
        public int DoomEdNum { get; set; } = -1;
        public bool SawNativeKeyword { get; set; }
        public List<string> Messages { get; } = new();
    }

    public static bool TryParse(IReadOnlyList<string> tokens, ref int index, ActorHeaderParseResult result)
    {
        if (index >= tokens.Count)
        {
            result.Messages.Add("Expected actor type name.");
            return false;
        }

        var typeToken = tokens[index++];
        ParseTypeParentToken(typeToken, result, out var consumedPair);
        if (!consumedPair && index < tokens.Count)
        {
            var next = tokens[index];
            if (next.StartsWith(':'))
            {
                index++;
                var parent = next.Length > 1 ? next[1..] : null;
                if (string.IsNullOrEmpty(parent))
                {
                    if (index >= tokens.Count)
                    {
                        result.Messages.Add("Expected parent type after ':'.");
                        return false;
                    }

                    parent = tokens[index++];
                }

                result.ParentName = parent;
            }
        }

        while (index < tokens.Count)
        {
            var token = tokens[index];
            if (token.Equals("replaces", StringComparison.OrdinalIgnoreCase))
            {
                index++;
                if (index >= tokens.Count)
                {
                    result.Messages.Add("Expected type name after 'replaces'.");
                    return false;
                }

                result.ReplacesType = tokens[index++];
                if (string.Equals(result.ReplacesType, result.TypeName, StringComparison.OrdinalIgnoreCase))
                    result.Messages.Add($"Cannot replace class {result.TypeName} with itself.");
                continue;
            }

            if (token.Equals("native", StringComparison.OrdinalIgnoreCase))
            {
                result.SawNativeKeyword = true;
                result.Messages.Add("Cannot define native classes in DECORATE.");
                index++;
                continue;
            }

            if (int.TryParse(token, out var edNum))
            {
                index++;
                if (edNum < -1 || edNum > MaxDoomEdNum)
                    result.Messages.Add("DoomEdNum must be in the range [-1,32767]");
                else
                    result.DoomEdNum = edNum;
                continue;
            }

            break;
        }

        return ValidateInheritance(result);
    }

    public static bool ValidateInheritance(ActorHeaderParseResult result)
    {
        if (string.Equals(result.TypeName, result.ParentName, StringComparison.OrdinalIgnoreCase))
        {
            result.Messages.Add($"'{result.TypeName}' inherits from a class with the same name.");
            return false;
        }

        return true;
    }

    private static void ParseTypeParentToken(string token, ActorHeaderParseResult result, out bool consumedPair)
    {
        consumedPair = false;
        var colon = token.IndexOf(':');
        if (colon >= 0)
        {
            result.TypeName = token[..colon];
            var parent = token[(colon + 1)..];
            if (!string.IsNullOrWhiteSpace(parent))
                result.ParentName = parent.Trim();
            consumedPair = true;
            return;
        }

        result.TypeName = token;
    }
}
