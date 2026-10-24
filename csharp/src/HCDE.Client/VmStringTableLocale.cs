namespace HCDE.Client;

/// <summary><c>FStringTable::GetSystemLocale</c> and legacy language remap from <c>stringtable.cpp</c>.</summary>
public static class VmStringTableLocale
{
    private static readonly Dictionary<string, string> LegacyLanguageRemap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["by"] = "be",
        ["ena"] = "en-GB", ["enb"] = "en-GB", ["enc"] = "en-GB", ["eng"] = "en-GB",
        ["eni"] = "en-GB", ["enj"] = "en-GB", ["enl"] = "en-GB", ["ens"] = "en-GB",
        ["ent"] = "en-GB", ["enw"] = "en-GB", ["enz"] = "en-GB",
        ["default"] = "en-US", ["enu"] = "en-US",
        ["esa"] = "es-MX", ["esb"] = "es-MX", ["esc"] = "es-MX", ["esd"] = "es-MX",
        ["ese"] = "es-MX", ["esf"] = "es-MX", ["esg"] = "es-MX", ["esh"] = "es-MX",
        ["esi"] = "es-MX", ["esl"] = "es-MX", ["esm"] = "es-MX", ["esn"] = "es-MX",
        ["eso"] = "es-MX", ["esr"] = "es-MX", ["ess"] = "es-MX", ["esu"] = "es-MX",
        ["esv"] = "es-MX", ["esy"] = "es-MX", ["esz"] = "es-MX",
        ["ja"] = "ja-JP", ["jp"] = "ja-JP",
        ["nb"] = "nb-NO", ["no"] = "nb-NO",
        ["ptg"] = "pt",
        ["chi"] = "zh-Hant", ["cht"] = "zh-Hant",
        ["chs"] = "zh-Hans", ["zho"] = "zh-Hans",
    };

    public static string GetSystemLocale()
    {
        var lang = Environment.GetEnvironmentVariable("LC_ALL")
            ?? Environment.GetEnvironmentVariable("LC_MESSAGES")
            ?? Environment.GetEnvironmentVariable("LANG");
        if (string.IsNullOrEmpty(lang) || lang is "C" or "POSIX") return "en-US";
        var tag = lang;
        var dot = tag.IndexOfAny(['.', '@']);
        if (dot >= 0) tag = tag[..dot];
        return tag.Replace('_', '-');
    }

    public static bool RemapLegacyLanguage(ref string code)
    {
        var old = code;
        if (LegacyLanguageRemap.TryGetValue(code, out var mapped)) code = mapped;
        return !string.Equals(old, code, StringComparison.OrdinalIgnoreCase);
    }
}
