namespace HCDE.Playsim;

/// <summary>
/// Config section and entry linking from <c>configfile.cpp</c> (<b>482–639</b>).
/// Chunk <b>1256</b>. Opening the path and reading the file are host steps. Entry values are replaced
/// by copying the new text. The entry allocation uses the section object size plus the key length.
/// </summary>
public sealed class VmConfigFile1256
{
    public const int SectionBytes = 40;

    public readonly List<string> Log = new();
    public Section? Sections;
    public Section? Current;
    public string Path = "";
    public bool FileExisted;
    public Func<string, bool>? Open;
    public Func<bool>? Read;

    Section? _tail;

    public void Ensure(string key, string value)
    {
        if (Current == null)
        {
            Log.Add("ensure none");
            return;
        }
        for (Entry? probe = Current.Root; probe != null; probe = probe.Next)
        {
            if (Same(probe.Key, key) && Same(probe.Value, value))
            {
                Log.Add($"ensure hit {probe.Key}={probe.Value}");
                return;
            }
        }
        NewEntry(Current, key, value);
        Log.Add($"ensure add {key}={value}");
    }

    public Section? FindSection(string name)
    {
        for (Section? section = Sections; section != null; section = section.Next)
        {
            if (Same(section.Name, name))
                return section;
        }
        return null;
    }

    public void Rename(string oldName, string newName)
    {
        Section? section = FindSection(oldName);
        if (section != null)
        {
            section.Name = newName;
            Log.Add($"rename {oldName} -> {newName}");
        }
        else
        {
            Log.Add($"rename miss {oldName}");
        }
    }

    public Entry? FindEntry(Section section, string key)
    {
        for (Entry? probe = section.Root; probe != null; probe = probe.Next)
        {
            if (Same(probe.Key, key))
                return probe;
        }
        return null;
    }

    public Section NewSection(string name)
    {
        Section? section = FindSection(name);
        if (section == null)
        {
            section = new Section { Name = name };
            if (_tail == null)
                Sections = section;
            else
                _tail.Next = section;
            _tail = section;
            Log.Add($"section new {name}");
        }
        else
        {
            Log.Add($"section keep {section.Name}");
        }
        return section;
    }

    public Entry NewEntry(Section section, string key, string value)
    {
        var entry = new Entry
        {
            Key = key,
            AllocBytes = SectionBytes + key.Length,
        };
        if (section.Tail == null)
            section.Root = entry;
        else
            section.Tail.Next = entry;
        section.Tail = entry;
        entry.SetValue(value);
        Log.Add($"entry {key} alloc={entry.AllocBytes} value={entry.Value}");
        return entry;
    }

    public void Load()
    {
        FileExisted = false;
        if (Open == null || !Open(Path))
        {
            Log.Add("load closed");
            return;
        }
        bool ok = Read != null && Read();
        FileExisted = ok;
        Log.Add($"load existed={(ok ? 1 : 0)}");
    }

    public static bool Same(string? left, string? right)
    {
        return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
    }

    public sealed class Section
    {
        public string Name = "";
        public Entry? Root;
        public Entry? Tail;
        public Section? Next;
    }

    public sealed class Entry
    {
        public string? Value;
        public Entry? Next;
        public string Key = "";
        public int AllocBytes;

        public void SetValue(string value)
        {
            Value = value;
        }
    }
}
