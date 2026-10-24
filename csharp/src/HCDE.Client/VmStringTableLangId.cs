namespace HCDE.Client;

public readonly record struct VmLangId(string Name, string? Fallback, uint Normalized, uint Script, uint Language);

/// <summary>Language ID resolution from <c>stringtable.cpp</c>.</summary>
public sealed class VmStringTableLangId
{
    private readonly Dictionary<string, VmLangId> langMap = new(StringComparer.OrdinalIgnoreCase);
    private static readonly (string Key, string Value)[] FallbackMappings =
    [
        ("en-*-AU", "en-GB"), ("en-*-CA", "en-GB"), ("en-*-SD", "en-GB"),
        ("zh-*-CN", "zh-Hans-CN"), ("zh-*-HK", "zh-Hant-HK"), ("zh-*-MO", "zh-Hant-MO"),
        ("zh-*-SG", "zh-Hans-SG"), ("zh-*-TW", "zh-Hant-TW"),
    ];

    public VmLangId GetId(string lang, bool useSystemLocaleForAuto = true)
    {
        if (string.Equals(lang, "auto", StringComparison.OrdinalIgnoreCase) && useSystemLocaleForAuto)
            lang = VmStringTableLocale.GetSystemLocale();
        VmStringTableLocale.RemapLegacyLanguage(ref lang);
        if (langMap.TryGetValue(lang, out var existing)) return existing;

        ExtractComponents(lang, out var language, out var script, out var region);
        var normalized = $"{language}-{script}-{region}";
        var scriptKey = $"{language}-{script}-*";
        var languageKey = $"{language}-*-*";
        if (langMap.TryGetValue(normalized, out existing)) return existing;

        var id = new VmLangId(
            normalized,
            GetFallback(normalized),
            VmNativeCrc32.Calc(normalized),
            VmNativeCrc32.Calc(scriptKey),
            VmNativeCrc32.Calc(languageKey));
        if (!string.Equals(lang, normalized, StringComparison.OrdinalIgnoreCase)) langMap[lang] = id;
        langMap[normalized] = id;
        return id;
    }

    public static string GetFallback(string normalized)
    {
        foreach (var (key, value) in FallbackMappings)
        {
            if (string.Equals(key, normalized, StringComparison.OrdinalIgnoreCase)) return value;
        }

        return "";
    }

    public static void ExtractComponents(string tag, out string lang, out string script, out string region)
    {
        lang = script = region = "*";
        var parts = tag.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0) return;
        lang = parts[0].ToLowerInvariant();
        if (parts.Length == 1) return;
        if (parts.Length == 2 && parts[1].Length == 2)
        {
            region = parts[1].ToUpperInvariant();
            return;
        }

        if (parts[1].Length == 4)
        {
            script = char.ToUpperInvariant(parts[1][0]) + parts[1][1..].ToLowerInvariant();
            if (parts.Length > 2 && parts[2].Length == 2) region = parts[2].ToUpperInvariant();
        }
    }
}
