namespace HCDE.Playsim;

/// <summary>
/// <c>DLevelScript::RunScript</c> print opcodes (<c>PCD_PRINTFIXED</c>, <c>PCD_PRINTNAME</c>, and script/map/world/global char-array prints)
/// from <c>p_acs.cpp</c> (**8941–9090**). Exclusive to chunk **1018**; follows **945** (**≤8849**).
/// </summary>
public static class VmPAcsVmOpcode1018Slice
{
    public const int MaxPlayers = 64;

    public static string FormatPrintFixed(int acsFixed) =>
        (acsFixed / 65536.0).ToString(System.Globalization.CultureInfo.InvariantCulture);

    public sealed class PrintNameContext
    {
        public string LevelName { get; init; } = "";
        public string MapName { get; init; } = "";
        public string NextMap { get; init; } = "";
        public string NextSecretMap { get; init; } = "";
        public string SkillName { get; init; } = "";
        public object? Activator { get; init; }
        public object? ActivatorPlayer { get; init; }
        public Func<int, bool>? ResolvePlayerIndex { get; init; }
        public Func<int, object?>? GetPlayerByIndex { get; init; }
        public Func<object?, string?>? PlayerDisplayName { get; init; }
        public Func<object?, string?>? ActorTag { get; init; }
    }

    public static bool TryResolvePrintNameSpecial(int stackValue, PrintNameContext ctx, out string text)
    {
        text = "";
        if (stackValue >= 0) return false;
        switch (stackValue)
        {
            case VmPAcsCallfunc925Slice.PrintNameLevelName:
                text = ctx.LevelName;
                return true;
            case VmPAcsCallfunc925Slice.PrintNameLevel:
                text = ctx.MapName.ToUpperInvariant();
                return true;
            case VmPAcsCallfunc925Slice.PrintNameNextLevel:
                text = ctx.NextMap.ToUpperInvariant();
                return true;
            case VmPAcsCallfunc925Slice.PrintNameNextSecret:
                text = ctx.NextSecretMap.ToUpperInvariant();
                return true;
            case VmPAcsCallfunc925Slice.PrintNameSkill:
                text = ctx.SkillName;
                return true;
            default:
                text = " ";
                return true;
        }
    }

    public static string ResolvePrintNamePlayer(int stackValue, PrintNameContext ctx)
    {
        if (stackValue == 0 || (uint)stackValue > MaxPlayers)
        {
            if (ctx.ActivatorPlayer is not null)
                return ctx.PlayerDisplayName?.Invoke(ctx.ActivatorPlayer) ?? "";
            if (ctx.Activator is not null)
                return ctx.ActorTag?.Invoke(ctx.Activator) ?? "";
            return " ";
        }

        if (ctx.ResolvePlayerIndex?.Invoke(stackValue - 1) == true
            && ctx.GetPlayerByIndex?.Invoke(stackValue - 1) is { } player)
        {
            return ctx.PlayerDisplayName?.Invoke(player) ?? "";
        }

        return $"Player {stackValue}";
    }

    public static string AppendCharArrayRun(
        Func<int, int, int> readChar,
        int arrayIndex,
        int offset,
        int capacity)
    {
        var work = "";
        var remaining = capacity;
        while (remaining-- > 0)
        {
            var c = readChar(arrayIndex, offset);
            if (c == 0) break;
            work += (char)c;
            offset++;
        }
        return work;
    }

    public static bool TryBuildPrintCharArray(
        List<int> stack,
        ref int sp,
        bool ranged,
        Func<int, int, int> readChar,
        out string text)
    {
        text = "";
        if (!VmPAcsRunscript926Slice.TryPopCharArrayParms(stack, ref sp, ranged, out var capacity, out var offset, out var index))
            return false;
        text = AppendCharArrayRun(readChar, index, offset, capacity);
        return true;
    }
}
