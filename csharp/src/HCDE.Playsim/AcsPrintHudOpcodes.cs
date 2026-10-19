using System.Globalization;
using System.Text;
using HCDE.MapLoader;

namespace HCDE.Playsim;

internal static class AcsPrintHudOpcodes
{
    internal const int PrintNameLevelName = -1;
    internal const int PrintNameLevel = -2;
    internal const int PrintNameNextLevel = -3;
    internal const int PrintNameNextSecret = -4;
    internal const int PrintNameSkill = -5;

    internal static void AppendString(StringBuilder work, string? text)
    {
        if (!string.IsNullOrEmpty(text))
            work.Append(text);
    }

    internal static void AppendNumber(StringBuilder work, int value) =>
        work.Append(value.ToString(CultureInfo.InvariantCulture));

    internal static void AppendCharacter(StringBuilder work, int value) =>
        work.Append((char)value);

    internal static void AppendFixed(StringBuilder work, int fixedValue) =>
        work.Append(ToDouble(fixedValue).ToString(CultureInfo.InvariantCulture));

    internal static void AppendBinary(StringBuilder work, int value) =>
        work.Append(Convert.ToString(value, 2));

    internal static void AppendHex(StringBuilder work, int value) =>
        work.Append(value.ToString("X", CultureInfo.InvariantCulture));

    internal static string ResolveString(int stringId, string[] stringTable, AcsGlobalStrings globals) =>
        AcsStringIds.IsGlobalPool(stringId)
            ? globals.GetByIndex(AcsStringIds.GlobalIndex(stringId))
            : stringId >= 0 && stringId < stringTable.Length
                ? stringTable[stringId]
                : string.Empty;

    internal static void AppendPrintName(
        AuthoritySimulation sim,
        StringBuilder work,
        int selector,
        Actor? activator)
    {
        switch (selector)
        {
            case PrintNameLevelName:
                AppendString(work, sim.Level.MapName);
                return;
            case PrintNameLevel:
                AppendString(work, sim.Level.MapName.ToUpperInvariant());
                return;
            case PrintNameNextLevel:
            case PrintNameNextSecret:
                AppendString(work, string.Empty);
                return;
            case PrintNameSkill:
                AppendString(work, $"Skill {sim.Skill}");
                return;
            case 0:
            case > 8:
                if (activator is not null)
                    work.Append(' ');
                else
                    work.Append(' ');
                return;
            default:
                var playerIndex = selector - 1;
                if ((uint)playerIndex < (uint)sim.Players.Count())
                    work.AppendFormat(CultureInfo.InvariantCulture, "Player{0}", playerIndex + 1);
                else
                    work.AppendFormat(CultureInfo.InvariantCulture, "Player {0}", selector);
                return;
        }
    }

    internal static void AppendCharArray(StringBuilder work, ReadOnlySpan<int> cells)
    {
        foreach (var cell in cells)
        {
            if (cell == 0)
                break;
            work.Append((char)cell);
        }
    }

    internal static void AppendBind(StringBuilder work, string command)
    {
        if (string.IsNullOrEmpty(command))
            return;
        work.Append(CultureInfo.InvariantCulture, $"??? ({command})");
    }

    internal static void EndPrint(AuthoritySimulation sim, StringBuilder work, bool bold)
    {
        var text = work.ToString();
        work.Clear();
        sim.AppendAcsPrint(text);
        if (bold)
            sim.AppendAcsPrintBold(text);
    }

    internal static void EndLog(AuthoritySimulation sim, StringBuilder work)
    {
        sim.AppendAcsPrint(work + "\n");
        work.Clear();
    }

    internal static void EndHudMessage(
        AuthoritySimulation sim,
        StringBuilder work,
        bool bold,
        List<int> stack,
        int hudOptStart,
        string activeFont,
        int activeStyle)
    {
        var optStart = hudOptStart >= 0 ? hudOptStart : stack.Count;
        if (optStart < 6)
        {
            work.Clear();
            return;
        }

        var type = stack[optStart - 6];
        var id = stack[optStart - 5];
        var color = stack[optStart - 4];
        var x = ToFloat(stack[optStart - 3]);
        var y = ToFloat(stack[optStart - 2]);
        var holdTime = ToFloat(stack[optStart - 1]);
        var alpha = optStart < stack.Count ? ToFloat(stack[optStart]) : 1f;
        sim.RecordAcsHudMessage(new AcsHudMessage(
            work.ToString(),
            type,
            id,
            color,
            x,
            y,
            holdTime,
            alpha,
            bold,
            activeFont,
            activeStyle));
        work.Clear();
    }

    internal static double ToDouble(int fixedValue) => fixedValue / 65536.0;

    internal static float ToFloat(int fixedValue) => (float)ToDouble(fixedValue);

}

public readonly record struct AcsHudMessage(
    string Text,
    int Type,
    int Id,
    int Color,
    float X,
    float Y,
    float HoldTime,
    float Alpha,
    bool Bold,
    string Font,
    int Style);
