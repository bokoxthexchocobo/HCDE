namespace HCDE.Client;

/// <summary>Key name lookup from <c>c_bind.cpp</c> (<c>GetKeyFromName</c> / <c>GetConfigKeyFromName</c>).</summary>
public static partial class VmKeyNames
{
    public const int NumKeys = 0x1CB;

    private static readonly Dictionary<string, int> NameToKey = BuildNameTable();

    private static readonly (string Alias, string Actual)[] ConfigAliases =
    [
        ("LeftBracket", "["),
        ("RightBracket", "]"),
        ("Equals", "="),
        ("KP-Equals", "kp="),
        ("Semicolon", ";"),
        ("Colon", ":"),
    ];

    public static int GetKeyFromName(string name)
    {
        if (name.Length > 1 && name[0] == '#')
            return int.TryParse(name.AsSpan(1), out var n) ? n : 0;
        return NameToKey.TryGetValue(name, out var key) ? key : 0;
    }

    public static int GetConfigKeyFromName(string key)
    {
        var keynum = GetKeyFromName(key);
        if (keynum != 0) return keynum;
        foreach (var (alias, actual) in ConfigAliases)
        {
            if (string.Equals(key, alias, StringComparison.OrdinalIgnoreCase))
                return GetKeyFromName(actual);
        }

        return 0;
    }

    public static string ConfigKeyName(int keynum)
    {
        var name = KeyName(keynum);
        if (name.Length == 1)
        {
            return name switch
            {
                "[" => "LeftBracket",
                "]" => "RightBracket",
                "=" => "Equals",
                _ => name
            };
        }

        if (string.Equals(name, "kp=", StringComparison.OrdinalIgnoreCase)) return "KP-Equals";
        return name;
    }

    public static string KeyName(int key) =>
        VmKeyNamesTable.IndexToName.TryGetValue(key, out var name) ? name : $"Key_{key}";

    private static Dictionary<string, int> BuildNameTable()
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var (index, name) in VmKeyNamesTable.IndexToName)
            map[name] = index;
        return map;
    }
}
