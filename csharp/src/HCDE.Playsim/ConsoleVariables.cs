using System.Globalization;

namespace HCDE.Playsim;

/// <summary>Native <c>ECVarType</c> values read by Actor.GetCVar.</summary>
public enum ConsoleVariableKind
{
    Bool,
    Int,
    Float,
    String,
    Color,
}

/// <summary>Native flag bits consulted by <c>GetCVar</c> and <c>G_GetUserCVar</c>.</summary>
[Flags]
public enum ConsoleVariableFlags
{
    None = 0,
    UserInfo = 1 << 1,
    Ignore = 1 << 14,
}

/// <summary>One represented console variable. Flag and mask redirects are not modeled.</summary>
public sealed class ConsoleVariable
{
    public const int MaxPlayers = 64;

    private ConsoleVariable(ConsoleVariableKind kind, ConsoleVariableFlags flags, bool boolean, int integer, float real, string text)
    {
        Kind = kind;
        Flags = flags;
        Bool = boolean;
        Int = integer;
        Float = real;
        Text = text;
    }

    public ConsoleVariableKind Kind { get; }
    public ConsoleVariableFlags Flags { get; }
    public bool Bool { get; }
    public int Int { get; }
    public float Float { get; }
    public string Text { get; }

    public static ConsoleVariable BoolValue(bool value, ConsoleVariableFlags flags = ConsoleVariableFlags.None)
        => new(ConsoleVariableKind.Bool, flags, value, 0, 0, "");

    public static ConsoleVariable IntValue(int value, ConsoleVariableFlags flags = ConsoleVariableFlags.None)
        => new(ConsoleVariableKind.Int, flags, false, value, 0, "");

    public static ConsoleVariable FloatValue(float value, ConsoleVariableFlags flags = ConsoleVariableFlags.None)
        => float.IsFinite(value) ? new(ConsoleVariableKind.Float, flags, false, 0, value, "")
            : throw new ArgumentOutOfRangeException(nameof(value));

    public static ConsoleVariable StringValue(string value, ConsoleVariableFlags flags = ConsoleVariableFlags.None)
        => new(ConsoleVariableKind.String, flags, false, 0, 0, value ?? throw new ArgumentNullException(nameof(value)));

    public static ConsoleVariable ColorValue(int rgb, ConsoleVariableFlags flags = ConsoleVariableFlags.None)
        => new(ConsoleVariableKind.Color, flags, false, rgb, 0, "");

    /// <summary>Native <c>GetGenericRep(CVAR_Float).Float</c>, widened back to double.</summary>
    public double AsFloat() => Kind switch
    {
        ConsoleVariableKind.Bool => Bool ? 1d : 0d,
        ConsoleVariableKind.Int or ConsoleVariableKind.Color => (float)Int,
        ConsoleVariableKind.Float => Float,
        ConsoleVariableKind.String => (float)CVarText.ParseFloat(Text),
        _ => 0d,
    };

    /// <summary>Native <c>GetGenericRep(CVAR_String).String</c>.</summary>
    public string AsString() => Kind switch
    {
        ConsoleVariableKind.Bool => Bool ? "true" : "false",
        ConsoleVariableKind.Int => Int.ToString(CultureInfo.InvariantCulture),
        ConsoleVariableKind.Float => CVarText.FormatFloat(Float),
        ConsoleVariableKind.String => Text,
        ConsoleVariableKind.Color => CVarText.FormatColor(Int),
        _ => "",
    };
}

/// <summary>
/// Session cvar map for <c>GetCVar(playernum, name)</c>. Values are engine config, not savegame state.
/// </summary>
public sealed class ConsoleVariables
{
    private readonly Dictionary<string, ConsoleVariable> _globals = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<int, Dictionary<string, ConsoleVariable>> _users = new();

    public void Set(string name, ConsoleVariable variable)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(variable);
        _globals[name] = variable;
    }

    public void SetUser(int playerNumber, string name, ConsoleVariable variable)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(variable);
        if (!_users.TryGetValue(playerNumber, out var table))
            _users[playerNumber] = table = new Dictionary<string, ConsoleVariable>(StringComparer.OrdinalIgnoreCase);
        table[name] = variable;
    }

    public double ReadFloat(int playerNumber, Func<int, bool> playerInGame, int consolePlayer, string name)
        => Resolve(playerNumber, playerInGame, consolePlayer, name)?.AsFloat() ?? 0d;

    public string ReadString(int playerNumber, Func<int, bool> playerInGame, int consolePlayer, string name)
        => Resolve(playerNumber, playerInGame, consolePlayer, name)?.AsString() ?? "";

    private ConsoleVariable? Resolve(int playerNumber, Func<int, bool> playerInGame, int consolePlayer, string name)
    {
        if (string.IsNullOrEmpty(name) || !_globals.TryGetValue(name, out var global)
            || global.Flags.HasFlag(ConsoleVariableFlags.Ignore)) return null;
        if (!global.Flags.HasFlag(ConsoleVariableFlags.UserInfo)) return global;
        var slot = playerNumber;
        if ((uint)slot >= ConsoleVariable.MaxPlayers || !playerInGame(slot))
        {
            slot = consolePlayer;
            if ((uint)slot >= ConsoleVariable.MaxPlayers || !playerInGame(slot)) return null;
        }
        return _users.TryGetValue(slot, out var table) && table.TryGetValue(name, out var user)
            && !user.Flags.HasFlag(ConsoleVariableFlags.Ignore) ? user : null;
    }
}

internal static class CVarText
{
    public static double ParseFloat(string text)
    {
        var i = 0;
        while (i < text.Length && IsCSpace(text[i])) i++;
        if (i >= text.Length) return 0;
        var sign = 1d;
        if (text[i] is '+' or '-')
        {
            if (text[i] == '-') sign = -1;
            i++;
        }
        if (i >= text.Length) return 0;
        var rest = text.AsSpan(i);
        if (rest.StartsWith("infinity", StringComparison.OrdinalIgnoreCase)) return sign * double.PositiveInfinity;
        if (rest.StartsWith("inf", StringComparison.OrdinalIgnoreCase)) return sign * double.PositiveInfinity;
        if (rest.StartsWith("nan", StringComparison.OrdinalIgnoreCase)) return double.NaN;
        if (i + 1 < text.Length && text[i] == '0' && text[i + 1] is 'x' or 'X') return sign * ParseHex(text, i + 2);
        return sign * ParseDecimal(text, i);
    }

    public static string FormatFloat(float value)
    {
        if (value == 0) return BitConverter.SingleToInt32Bits(value) < 0 ? "-0" : "0";
        return value.ToString("G6", CultureInfo.InvariantCulture).Replace('E', 'e');
    }

    public static string FormatColor(int rgb)
        => $"{(rgb >> 16) & 0xff:x2} {(rgb >> 8) & 0xff:x2} {rgb & 0xff:x2}";

    private static double ParseHex(string text, int i)
    {
        long digits = 0;
        var count = 0;
        var extraIntegerNibbles = 0;
        var fractionBits = 0;
        var sawDigit = false;
        while (i < text.Length && Hex(text[i]) is >= 0 and var digit)
        {
            sawDigit = true;
            if (count < 15)
            {
                digits = (digits << 4) + digit;
                count++;
            }
            else extraIntegerNibbles++;
            i++;
        }
        if (i < text.Length && text[i] == '.')
        {
            i++;
            while (i < text.Length && Hex(text[i]) is >= 0 and var digit)
            {
                sawDigit = true;
                if (count < 15)
                {
                    digits = (digits << 4) + digit;
                    count++;
                    fractionBits += 4;
                }
                i++;
            }
        }
        if (!sawDigit) return 0;
        var value = Math.ScaleB((double)digits, extraIntegerNibbles * 4 - fractionBits);
        if (i >= text.Length || text[i] is not ('p' or 'P')) return value;
        i++;
        var exponentSign = 1;
        if (i < text.Length && text[i] is '+' or '-')
        {
            if (text[i] == '-') exponentSign = -1;
            i++;
        }
        var exponent = 0;
        var sawExponent = false;
        while (i < text.Length && text[i] is >= '0' and <= '9')
        {
            sawExponent = true;
            exponent = exponent * 10 + (text[i] - '0');
            i++;
        }
        return sawExponent ? Math.ScaleB(value, exponentSign * exponent) : value;
    }

    private static double ParseDecimal(string text, int i)
    {
        var start = i;
        while (i < text.Length && text[i] is >= '0' and <= '9') i++;
        var sawDigit = i > start;
        if (i < text.Length && text[i] == '.')
        {
            i++;
            var fraction = i;
            while (i < text.Length && text[i] is >= '0' and <= '9') i++;
            sawDigit |= i > fraction;
        }
        if (!sawDigit) return 0;
        if (i < text.Length && text[i] is 'e' or 'E')
        {
            var exponent = i + 1;
            if (exponent < text.Length && text[exponent] is '+' or '-') exponent++;
            var exponentDigits = exponent;
            while (exponent < text.Length && text[exponent] is >= '0' and <= '9') exponent++;
            if (exponent > exponentDigits) i = exponent;
        }
        return double.Parse(text.AsSpan(start, i - start), NumberStyles.Float, CultureInfo.InvariantCulture);
    }

    private static bool IsCSpace(char c) => c is ' ' or '\t' or '\n' or '\v' or '\f' or '\r';

    private static int Hex(char c) => c switch
    {
        >= '0' and <= '9' => c - '0',
        >= 'a' and <= 'f' => c - 'a' + 10,
        >= 'A' and <= 'F' => c - 'A' + 10,
        _ => -1,
    };
}
