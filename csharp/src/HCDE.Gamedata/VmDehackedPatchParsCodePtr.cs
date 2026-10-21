namespace HCDE.Gamedata;

/// <summary>DEH <c>PatchPars</c> and <c>PatchCodePtrs</c> from <c>d_dehacked.cpp</c> (2731–2925).</summary>
public static class VmDehackedPatchParsCodePtr
{
    public static bool IsParKey(string line1) =>
        string.Equals(line1, "par", StringComparison.OrdinalIgnoreCase);

    public static int? FindFirstWhitespaceIndex(string line)
    {
        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c != '\r' && char.IsWhiteSpace(c)) return i;
        }

        return null;
    }

    public static bool TryParseParLine(string line2, out string mapName, out int par)
    {
        mapName = "";
        par = 0;
        var firstSpace = FindFirstWhitespaceIndex(line2);
        if (firstSpace is null) return false;
        var episode = line2[..firstSpace.Value];
        var rest = line2[(firstSpace.Value + 1)..].TrimStart();
        var secondSpace = FindFirstWhitespaceIndex(rest);
        if (secondSpace is null)
        {
            mapName = $"MAP{int.Parse(episode) % 100:00}";
            par = int.Parse(rest);
            return true;
        }

        var mission = rest[..secondSpace.Value];
        mapName = $"E{episode[0]}M{mission[0]}";
        par = int.Parse(rest[(secondSpace.Value + 1)..].TrimStart());
        return true;
    }

    public static bool FrameLineStartsWithFrame(string line1) =>
        line1.Length > 5
        && line1.StartsWith("Frame", StringComparison.OrdinalIgnoreCase)
        && char.IsWhiteSpace(line1[5]);

    public static int ParseFrameIndex(string line1) => int.Parse(line1[5..].TrimStart());

    public static bool IsNullPointer(string line2) =>
        string.Equals(line2.Trim(), "NULL", StringComparison.OrdinalIgnoreCase);

    public static string NormalizeActionSymbol(string line2)
    {
        var trimmed = line2.Trim();
        if (trimmed.Length >= 2 && (trimmed[0] == 'A' || trimmed[0] == 'a') && trimmed[1] == '_')
            return trimmed;
        return $"A_{trimmed}";
    }

    public static bool IsConsumeAmmoPointer(string symbol) =>
        string.Equals(symbol, "A_ConsumeAmmo", StringComparison.OrdinalIgnoreCase);

    public static string RemapCPosAttack(string symbol) =>
        string.Equals(symbol, "A_CPosAttack", StringComparison.OrdinalIgnoreCase) ? "A_CPosAttackDehacked" : symbol;

    public static bool TryResolveMbfAlias(string symbol, IReadOnlyList<(string Alias, string Name)> mbf, out string resolved, out bool wasMbf)
    {
        foreach (var entry in mbf)
        {
            if (string.Equals(symbol, entry.Alias, StringComparison.OrdinalIgnoreCase))
            {
                resolved = entry.Name;
                wasMbf = true;
                return true;
            }
        }

        resolved = symbol;
        wasMbf = false;
        return false;
    }

    public static bool IncompatibleVirtualPointer(bool isVirtual, bool hasRequiredArg) =>
        isVirtual || hasRequiredArg;

    public static void BubblePickupSpriteSort(Span<string> sprites, int changedIndex, string newSprite)
    {
        var i = changedIndex;
        while (i > 0 && string.CompareOrdinal(sprites[i - 1], newSprite) > 0)
        {
            (sprites[i - 1], sprites[i]) = (sprites[i], sprites[i - 1]);
            i--;
        }

        while (i + 1 < sprites.Length && string.CompareOrdinal(sprites[i + 1], newSprite) < 0)
        {
            (sprites[i + 1], sprites[i]) = (sprites[i], sprites[i + 1]);
            i++;
        }
    }
}
