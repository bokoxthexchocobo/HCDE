namespace HCDE.Gamedata;

/// <summary><c>CheckEndSequence</c> / <c>MakeEndPic</c> from <c>intermission_parse.cpp</c>.</summary>
public static class VmEndSequenceResolver
{
    public static bool TryMakeEndPic(string pic, VmIntermissionRegistry registry, out string sequenceName)
    {
        sequenceName = $"@EndPic_{pic}";
        var desc = new VmIntermissionDescriptor();
        desc.Actions.Add(new VmIntermissionAction { Background = pic });
        registry.Replace(sequenceName, desc);
        return true;
    }

    public static bool TryResolveKeyword(string token, out string? sequenceName, out bool needsEndGameBody)
    {
        sequenceName = null;
        needsEndGameBody = false;
        if (token.Equals("endgame", StringComparison.OrdinalIgnoreCase))
        {
            needsEndGameBody = true;
            return true;
        }

        if (token.StartsWith("EndGame", StringComparison.OrdinalIgnoreCase) && token.Length >= 7)
        {
            sequenceName = token[7] switch
            {
                '1' => "Inter_Pic1",
                '2' => "Inter_Pic2",
                '3' => "Inter_Bunny",
                'C' or 'c' => "Inter_Cast",
                'W' or 'w' => "Inter_Underwater",
                'S' or 's' => "Inter_Strife",
                _ => "Inter_Pic3",
            };
            return true;
        }

        sequenceName = token.ToLowerInvariant() switch
        {
            "endpic" => null,
            "endbunny" => "Inter_Bunny",
            "endcast" => "Inter_Cast",
            "enddemon" => "Inter_Demonscroll",
            "endchess" => "Inter_Chess",
            "endunderwater" => "Inter_Underwater",
            "endbuystrife" => "Inter_BuyStrife",
            "endtitle" => "Inter_Titlescreen",
            "endsequence" => null,
            _ => null,
        };

        return sequenceName is not null || token.Equals("endpic", StringComparison.OrdinalIgnoreCase)
            || token.Equals("endsequence", StringComparison.OrdinalIgnoreCase);
    }

    public static bool TryResolveEndPicArgument(string followingToken, VmIntermissionRegistry registry, out string sequenceName)
        => TryMakeEndPic(followingToken, registry, out sequenceName);

    public static bool TryResolveEndSequenceArgument(string name) => !string.IsNullOrWhiteSpace(name);
}
