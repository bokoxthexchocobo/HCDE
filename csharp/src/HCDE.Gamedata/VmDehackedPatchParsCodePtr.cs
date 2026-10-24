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

    public static int RunPatchParsNative(
        Func<(int Result, string Line1, string Line2)> getLine,
        Func<string, bool> levelExists,
        Action<string, int> setParTime,
        Action<string> logUnknownKey,
        Action logNeedDataAfterPar,
        Action<string> logMissingMap)
    {
        var result = 0;
        while (true)
        {
            (result, var line1, var line2) = getLine();
            if (result == 0)
                break;

            if (result == 1)
            {
                logUnknownKey(line1);
                continue;
            }

            if (!IsParKey(line1))
                return result;

            if (!TryParseParLine(line2, out var mapName, out var par))
            {
                logNeedDataAfterPar();
                continue;
            }

            if (!levelExists(mapName))
            {
                logMissingMap(mapName);
                continue;
            }

            setParTime(mapName, par);
        }

        return result;
    }

    public readonly record struct CodePointerSymbol(bool IsVirtual, bool HasRequiredArg);

    public static int RunPatchCodePtrsNative(
        Func<(int Result, string Line1, string Line2)> getLine,
        Func<int, object?> findState,
        Action<int> logFrameOutOfRange,
        Action<object?> setNullPointer,
        Action<object?, string, int> setResolvedPointer,
        Func<string, CodePointerSymbol?> findSymbol,
        Action<int, string> logUnknownPointer,
        Action<int, string> logIncompatiblePointer,
        IReadOnlyList<(string Alias, string Name)> mbfAliases,
        Action<object?> markConsumeAmmo)
    {
        while (true)
        {
            var (result, line1, line2) = getLine();
            if (result != 1)
                return result;

            if (!FrameLineStartsWithFrame(line1))
                continue;

            var frame = ParseFrameIndex(line1);
            var state = findState(frame);
            if (state is null)
            {
                logFrameOutOfRange(frame);
                continue;
            }

            if (IsNullPointer(line2))
            {
                setNullPointer(state);
                continue;
            }

            var symname = NormalizeActionSymbol(line2);
            if (IsConsumeAmmoPointer(symname))
                markConsumeAmmo(state);

            TryResolveMbfAlias(symname, mbfAliases, out symname, out var wasMbf);
            symname = RemapCPosAttack(symname);

            var symInfo = findSymbol(symname);
            if (symInfo is null)
            {
                logUnknownPointer(frame, line2);
                continue;
            }

            if (!wasMbf && IncompatibleVirtualPointer(symInfo.Value.IsVirtual, symInfo.Value.HasRequiredArg))
            {
                logIncompatiblePointer(frame, line2);
                continue;
            }

            setResolvedPointer(state, symname, frame);
        }
    }
}
