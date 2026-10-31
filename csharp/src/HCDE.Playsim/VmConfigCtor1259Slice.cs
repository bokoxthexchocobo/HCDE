namespace HCDE.Playsim;

/// <summary>
/// Config construction, copy, clear, and destruction from <c>configfile.cpp</c> (<b>1–178</b>).
/// Chunk <b>1259</b>. Reading the file is a host step. The path constructor calls that read and then
/// forces the writable and existed flags on. Copying sections reuses a case-insensitive name and
/// appends a new one. An entry allocation is the section object size plus the key length.
/// </summary>
public sealed class VmConfigCtor1259
{
    public const int SectionBytes = 40;

    public readonly List<string> Log = new();
    public Section? Sections;
    public Section? Current;
    public string Path = "";
    public bool OkayToWrite;
    public bool FileExisted;
    public Action<VmConfigCtor1259>? Load;
    Section? _tail;

    public static VmConfigCtor1259 Create()
    {
        var vm = new VmConfigCtor1259
        {
            OkayToWrite = true,
            FileExisted = true,
        };
        vm.Log.Add("ctor");
        return vm;
    }

    public static VmConfigCtor1259 Open(string path, Action<VmConfigCtor1259>? load)
    {
        var vm = new VmConfigCtor1259 { Load = load };
        vm.ChangePath(path);
        load?.Invoke(vm);
        vm.Log.Add("load");
        vm.OkayToWrite = true;
        vm.FileExisted = true;
        return vm;
    }

    public static VmConfigCtor1259 CopyOf(VmConfigCtor1259 other)
    {
        var vm = new VmConfigCtor1259();
        vm.ChangePath(other.Path);
        vm.Assign(other);
        vm.OkayToWrite = other.OkayToWrite;
        vm.FileExisted = other.FileExisted;
        vm.Log.Add($"copy write={(vm.OkayToWrite ? 1 : 0)} existed={(vm.FileExisted ? 1 : 0)}");
        return vm;
    }

    public void Assign(VmConfigCtor1259 other)
    {
        Clear();
        for (Section? from = other.Sections; from != null; from = from.Next)
        {
            Section to = NewSection(from.Name);
            for (Entry? entry = from.Root; entry != null; entry = entry.Next)
                NewEntry(to, entry.Key, entry.Value ?? "");
        }
        Log.Add("assign");
    }

    public void Clear()
    {
        Current = Sections;
        while (Current != null)
        {
            Section? next = Current.Next;
            FreeEntries(Current, "clear");
            Log.Add($"drop {Current.Name}");
            Current = next;
        }
        Sections = null;
        _tail = null;
        Log.Add("tail head");
    }

    public void Destroy()
    {
        Section? section = Sections;
        while (section != null)
        {
            Section? next = section.Next;
            FreeEntries(section, "free");
            Log.Add($"free section {section.Name}");
            section = next;
        }
        Sections = null;
        Current = null;
        _tail = null;
    }

    public void ChangePath(string? path)
    {
        Path = path ?? "";
        Log.Add($"path {Path}");
    }

    public Section NewSection(string name)
    {
        Section? section = Find(name);
        if (section == null)
        {
            section = new Section { Name = name };
            if (_tail == null)
                Sections = section;
            else
                _tail.Next = section;
            _tail = section;
            Log.Add($"section {name}");
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
        entry.Value = value;
        Log.Add($"entry {key} alloc={entry.AllocBytes} value={entry.Value}");
        return entry;
    }

    Section? Find(string name)
    {
        for (Section? section = Sections; section != null; section = section.Next)
        {
            if (string.Equals(section.Name, name, StringComparison.OrdinalIgnoreCase))
                return section;
        }
        return null;
    }

    void FreeEntries(Section section, string kind)
    {
        Entry? entry = section.Root;
        while (entry != null)
        {
            Entry? next = entry.Next;
            Log.Add($"{kind} value {entry.Key}");
            Log.Add($"{kind} entry {entry.Key} {entry.AllocBytes}");
            entry = next;
        }
        section.Root = null;
        section.Tail = null;
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
        public string Key = "";
        public string? Value;
        public Entry? Next;
        public int AllocBytes;
    }
}
