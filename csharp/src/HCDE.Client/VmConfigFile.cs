using System.Text;

namespace HCDE.Client;

/// <summary>Managed <c>FConfigFile</c> from <c>configfile.cpp</c>.</summary>
public class VmConfigFile
{
    private readonly List<VmConfigSection> sections = new();
    private VmConfigSection? currentSection;
    private int currentEntryIndex;
    public string PathName { get; private set; } = "";
    public bool FileExisted { get; private set; }
    public bool OkayToWrite { get; set; } = true;

    public void ChangePathName(string path) => PathName = path;

    public void LoadFromText(string text)
    {
        FileExisted = ReadConfig(text);
    }

    public bool SetSection(string name, bool allowCreate = false)
    {
        var section = sections.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
        if (section is null && allowCreate)
        {
            section = NewSection(name);
        }

        if (section is null) return false;
        currentSection = section;
        currentEntryIndex = 0;
        return true;
    }

    public bool SetFirstSection()
    {
        if (sections.Count == 0) return false;
        currentSection = sections[0];
        currentEntryIndex = 0;
        return true;
    }

    public bool SetNextSection()
    {
        if (currentSection is null) return false;
        var index = sections.IndexOf(currentSection);
        if (index + 1 >= sections.Count) return false;
        currentSection = sections[index + 1];
        currentEntryIndex = 0;
        return true;
    }

    public string? GetCurrentSection() => currentSection?.Name;

    public string? GetValueForKey(string key) =>
        currentSection?.Entries.FirstOrDefault(e => string.Equals(e.Key, key, StringComparison.OrdinalIgnoreCase))?.Value;

    public void SetValueForKey(string key, string value, bool duplicates = false)
    {
        if (currentSection is null) return;
        var existing = currentSection.Entries.FirstOrDefault(e => string.Equals(e.Key, key, StringComparison.OrdinalIgnoreCase));
        if (existing is null || duplicates)
        {
            currentSection.Entries.Add(new VmConfigEntry(key, value));
        }
        else existing.Value = value;
    }

    public void EnsureValueForKey(string key, string value)
    {
        if (currentSection is null) return;
        if (currentSection.Entries.Any(e => string.Equals(e.Key, key, StringComparison.OrdinalIgnoreCase)
            && string.Equals(e.Value, value, StringComparison.OrdinalIgnoreCase))) return;
        currentSection.Entries.Add(new VmConfigEntry(key, value));
    }

    public bool NextInSection(out string key, out string value)
    {
        key = "";
        value = "";
        if (currentSection is null || currentEntryIndex >= currentSection.Entries.Count) return false;
        var entry = currentSection.Entries[currentEntryIndex++];
        key = entry.Key;
        value = entry.Value;
        return true;
    }

    public string WriteToText()
    {
        var builder = new StringBuilder();
        WriteCommentHeader(builder);
        foreach (var section in sections)
        {
            if (!string.IsNullOrEmpty(section.Note)) builder.Append(section.Note);
            builder.Append('[').Append(section.Name).Append("]\n");
            foreach (var entry in section.Entries)
            {
                if (entry.Value.Contains('\n') || entry.Value.Contains('\r'))
                {
                    var endTag = GenerateEndTag(entry.Value);
                    builder.Append(entry.Key).Append("<<<").Append(endTag).Append('\n')
                        .Append(entry.Value.TrimEnd('\r', '\n')).Append('\n')
                        .Append(">>>").Append(endTag).Append('\n');
                }
                else builder.Append(entry.Key).Append('=').Append(entry.Value).Append('\n');
            }

            builder.Append('\n');
        }

        return builder.ToString();
    }

    private void WriteCommentHeader(StringBuilder builder) { }

    private bool ReadConfig(string text)
    {
        sections.Clear();
        currentSection = null;
        VmConfigSection? section = null;
        using var reader = new StringReader(text);
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            var start = line.AsSpan().TrimStart();
            if (start.Length == 0 || start[0] == '#' || (start.Length > 1 && start[0] == '/' && start[1] == '/')) continue;
            var trimmed = line.TrimEnd();
            if (trimmed.Length == 0) continue;
            if (trimmed[0] == '[')
            {
                var name = trimmed.Trim('[', ']');
                section = NewSection(name);
                continue;
            }

            if (section is null) return false;
            var equal = trimmed.IndexOf('=');
            if (equal <= 0) continue;
            var key = trimmed[..equal].TrimEnd();
            var value = trimmed[(equal + 1)..].TrimStart();
            if (value.StartsWith("<<<", StringComparison.Ordinal) && value.Length > 3)
            {
                var endTag = value[3..];
                var multi = new StringBuilder();
                while ((line = reader.ReadLine()) is not null)
                {
                    if (line.StartsWith(">>>", StringComparison.Ordinal) && line.AsSpan(3).StartsWith(endTag.AsSpan()))
                        break;
                    multi.AppendLine(line);
                }

                section.Entries.Add(new VmConfigEntry(key, multi.ToString().TrimEnd('\r', '\n')));
            }
            else section.Entries.Add(new VmConfigEntry(key, value));
        }

        return true;
    }

    private VmConfigSection NewSection(string name)
    {
        var section = new VmConfigSection(name);
        sections.Add(section);
        return section;
    }

    private static string GenerateEndTag(string value)
    {
        while (true)
        {
            var tag = $"EOV-{Guid.NewGuid():N}"[..24];
            if (!value.Contains(tag, StringComparison.Ordinal)) return tag;
        }
    }

    private sealed class VmConfigSection(string name)
    {
        public string Name { get; } = name;
        public string Note { get; set; } = "";
        public List<VmConfigEntry> Entries { get; } = new();
    }

    private sealed class VmConfigEntry(string key, string value)
    {
        public string Key { get; } = key;
        public string Value { get; set; } = value;
    }
}
