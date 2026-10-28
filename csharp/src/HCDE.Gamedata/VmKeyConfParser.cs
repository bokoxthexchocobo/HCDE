namespace HCDE.Gamedata;

public sealed class VmKeyConfSection
{
    public string Title { get; set; } = "";
    public string IniName { get; set; } = "";
    public List<VmKeyConfAction> Actions { get; } = [];
}

public sealed class VmKeyConfAction
{
    public string Title { get; set; } = "";
    public string Command { get; set; } = "";
}

/// <summary>KEYCONF lump line parsing and <c>addkeysection</c> / <c>addmenukey</c> from <c>keysections.cpp</c>.</summary>
public static class VmKeyConfParser
{
    public static string BuildBindingsSectionName(string configName, string modName, bool doubleBindings) =>
        $"{configName}.{modName}.{(doubleBindings ? "Double" : "")}Bindings";

    public static string TruncateIniName(string iniName, int maxLen = 32) =>
        iniName.Length > maxLen ? iniName[..maxLen] : iniName;

    public static bool TryAddKeySection(
        List<VmKeyConfSection> sections,
        ref int currentSectionIndex,
        string menuTitle,
        string iniName,
        out bool created)
    {
        iniName = TruncateIniName(iniName);
        for (var i = 0; i < sections.Count; i++)
        {
            if (sections[i].Title.Equals(iniName, StringComparison.OrdinalIgnoreCase))
            {
                currentSectionIndex = i;
                created = false;
                return true;
            }
        }

        currentSectionIndex = sections.Count;
        sections.Add(new VmKeyConfSection { Title = menuTitle, IniName = iniName });
        created = true;
        return true;
    }

    public static bool TryAddMenuKey(
        IReadOnlyList<VmKeyConfSection> sections,
        int currentSectionIndex,
        string description,
        string command,
        List<string> errors)
    {
        if (currentSectionIndex < 0 || currentSectionIndex >= sections.Count)
        {
            errors.Add("You must use addkeysection first.");
            return false;
        }

        sections[currentSectionIndex].Actions.Add(new VmKeyConfAction { Title = description, Command = command });
        return true;
    }

    public static string StripKeyConfComments(string line)
    {
        var filtered = new List<char>();
        var inQuote = false;
        var prev = '\0';
        var inBlock = false;
        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (inBlock)
            {
                if (prev == '*' && ch == '/') inBlock = false;
                prev = ch;
                continue;
            }

            if (ch == '"') inQuote = !inQuote;
            else if (!inQuote && prev == '/' && ch == '/')
            {
                if (filtered.Count > 0) filtered.RemoveAt(filtered.Count - 1);
                break;
            }
            else if (!inQuote && prev == '/' && ch == '*')
            {
                if (filtered.Count > 0) filtered.RemoveAt(filtered.Count - 1);
                inBlock = true;
                prev = '\0';
                continue;
            }

            filtered.Add(ch);
            prev = ch;
        }

        return new string(filtered.ToArray()).Trim();
    }

    public static IReadOnlyList<string> ParseKeyConfLumpCommands(string lumpText)
    {
        var commands = new List<string>();
        var i = 0;
        var inBlockComment = false;
        while (i < lumpText.Length)
        {
            var lineStart = i;
            while (i < lumpText.Length && lumpText[i] is not '\n' and not '\r') i++;
            var line = lumpText[lineStart..i];
            while (i < lumpText.Length && lumpText[i] is '\n' or '\r') i++;

            var stripped = StripKeyConfCommentsWithBlockState(line, ref inBlockComment);
            if (stripped.Length > 0) commands.Add(stripped);
        }

        return commands;
    }

    private static string StripKeyConfCommentsWithBlockState(string line, ref bool inBlockComment)
    {
        var filtered = new List<char>();
        var inQuote = false;
        var prev = '\0';
        for (var pos = 0; pos < line.Length; pos++)
        {
            var ch = line[pos];
            if (inBlockComment)
            {
                if (prev == '*' && ch == '/')
                {
                    inBlockComment = false;
                    prev = '\0';
                }
                else prev = ch;
                continue;
            }

            if (ch == '"') inQuote = !inQuote;
            else if (!inQuote && prev == '/' && ch == '/')
            {
                if (filtered.Count > 0) filtered.RemoveAt(filtered.Count - 1);
                break;
            }
            else if (!inQuote && prev == '/' && ch == '*')
            {
                if (filtered.Count > 0) filtered.RemoveAt(filtered.Count - 1);
                inBlockComment = true;
                prev = '\0';
                continue;
            }

            filtered.Add(ch);
            prev = ch;
        }

        return new string(filtered.ToArray()).Trim();
    }
}
