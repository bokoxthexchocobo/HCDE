namespace HCDE.Client;

/// <summary>Managed <c>FStringTable</c> storage, load, and lookups.</summary>
public sealed class VmStringTable
{
    private readonly Dictionary<uint, Dictionary<string, VmStringTableEntry>> allStrings = new();
    private readonly List<(uint LangId, Dictionary<string, VmStringTableEntry> Map)> currentLanguageSet = new();
    private readonly VmStringTableLangId langIds = new();
    private readonly Dictionary<string, string[]> macros = new(StringComparer.OrdinalIgnoreCase);
    private int defaultGender;

    internal VmStringTableLangId LangIds => langIds;

    public void SetDefaultGender(int gender) => defaultGender = gender;

    public VmStringTable()
    {
        DefaultTableId = VmNativeCrc32.Calc("en-*-US");
        GlobalTableId = VmNativeCrc32.Calc("*");
        OverrideTableId = VmNativeCrc32.Calc("**");
    }

    public uint DefaultTableId { get; }
    public uint GlobalTableId { get; }
    public uint OverrideTableId { get; }

    public void Clear() => allStrings.Clear();

    public void LoadStrings(IVmStringTableLumpSource lumps, string? language, Func<string, bool>? checkGame = null) =>
        VmStringTableLoadStrings.Load(this, lumps, language, checkGame);

    public void LoadLanguage(int fileNum, string text, Func<string, bool>? checkGame = null)
    {
        VmStringTableLoader.Load(this, fileNum, text, langIds, checkGame);
    }

    public void UpdateLanguage(string? language)
    {
        currentLanguageSet.Clear();
        if (string.IsNullOrEmpty(language)) language = "en-US";
        var id = langIds.GetId(language);
        VmLangId? fallback = null;
        if (!string.IsNullOrEmpty(id.Fallback))
            fallback = langIds.GetId(id.Fallback, useSystemLocaleForAuto: false);

        var seen = new HashSet<uint>();
        (uint LangId, bool _)[] order =
        [
            (OverrideTableId, false),
            (GlobalTableId, false),
            (id.Normalized, true),
            (fallback?.Normalized ?? 0, true),
            (id.Script, true),
            (fallback?.Script ?? 0, true),
            (id.Language, true),
            (fallback?.Language ?? 0, true),
            (DefaultTableId, true),
        ];
        foreach (var (langId, _) in order)
        {
            if (langId == 0 || !seen.Add(langId)) continue;
            AddCurrentMap(langId);
        }
    }

    public void InsertString(int fileNum, uint langId, string label, string value)
    {
        var expanded = VmStringTableMacros.ApplyMacros(this, value);
        var map = GetOrCreateMap(langId);
        if (!map.TryGetValue(label, out var entry))
        {
            entry = new VmStringTableEntry(fileNum);
            map[label] = entry;
        }

        for (var g = 0; g < 4; g++)
            entry.Set(g, expanded);
    }

    public void DeleteString(uint langId, string label)
    {
        if (allStrings.TryGetValue(langId, out var map))
            map.Remove(label);
    }

    public void DeleteForLabel(int fileNum, string label)
    {
        foreach (var map in allStrings.Values)
        {
            if (map.TryGetValue(label, out var entry) && entry.FileNum < fileNum)
                map.Remove(label);
        }
    }

    internal void ClearMacros() => macros.Clear();

    internal void RegisterMacro(string name, string[] replacements) => macros[name] = replacements;

    internal bool TryGetMacro(string name, out string[] replacements) =>
        macros.TryGetValue(name, out replacements!);

    public bool Exists(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        foreach (var tableId in new[] { DefaultTableId, GlobalTableId, OverrideTableId })
        {
            if (allStrings.TryGetValue(tableId, out var map) && map.ContainsKey(name))
                return true;
        }

        return false;
    }

    public string? CheckString(string name, out uint langTable, int gender = -1)
    {
        langTable = 0;
        if (string.IsNullOrEmpty(name)) return null;
        if (gender < 0) gender = defaultGender;
        gender = Math.Clamp(gender, 0, 3);
        VmStringTableEntry? best = null;
        uint bestLang = 0;
        foreach (var (lang, map) in currentLanguageSet)
        {
            if (!map.TryGetValue(name, out var item)) continue;
            if (best is not null && best.FileNum > item.FileNum) continue;
            best = item;
            bestLang = lang;
            var text = item.Get(gender);
            if (text.StartsWith("$$", StringComparison.Ordinal))
                return CheckString(text[2..], out langTable, gender);
            langTable = bestLang;
            return text;
        }

        return null;
    }

    public string? GetLanguageString(string name, uint langTable, int gender = -1)
    {
        if (string.IsNullOrEmpty(name)) return null;
        if (gender < 0) gender = defaultGender;
        gender = Math.Clamp(gender, 0, 3);
        if (!allStrings.TryGetValue(langTable, out var map)) return null;
        return map.TryGetValue(name, out var item) ? item.Get(gender) : null;
    }

    public string GetString(string name) => CheckString(name, out _) ?? name;

    public bool MatchDefaultString(string name, string content)
    {
        var c = GetLanguageString(name, DefaultTableId);
        if (c is null) return false;
        var checkKey = $"{name}_CHECK";
        var alt = GetLanguageString(checkKey, DefaultTableId);
        if (alt is not null) c = alt;
        var end = content.AsSpan().IndexOfAny('\n', '\r', '\t');
        var slice = end < 0 ? content : content[..end];
        return c.StartsWith(slice, StringComparison.OrdinalIgnoreCase);
    }

    private void AddCurrentMap(uint langId)
    {
        if (allStrings.TryGetValue(langId, out var map))
            currentLanguageSet.Add((langId, map));
    }

    private Dictionary<string, VmStringTableEntry> GetOrCreateMap(uint langId)
    {
        if (!allStrings.TryGetValue(langId, out var map))
        {
            map = new Dictionary<string, VmStringTableEntry>(StringComparer.OrdinalIgnoreCase);
            allStrings[langId] = map;
        }

        return map;
    }

    private sealed class VmStringTableEntry(int fileNum)
    {
        private readonly string?[] strings = new string?[4];
        public int FileNum { get; } = fileNum;
        public string Get(int gender) => strings[gender] ?? "";
        public void Set(int gender, string value) => strings[gender] = value;
    }
}
