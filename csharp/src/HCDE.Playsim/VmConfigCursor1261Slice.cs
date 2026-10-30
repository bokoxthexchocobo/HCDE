namespace HCDE.Playsim;

/// <summary>
/// Section cursor, clear, delete, and key lookup from <c>configfile.cpp</c> (<b>214–465</b>).
/// Chunk <b>1261</b>. A failed section name leaves the cursor where it was. Clearing a section
/// drops its entries and does not move the entry cursor. Deleting a section advances the section
/// cursor and leaves that entry cursor on the detached entry. Deleting the first section clears
/// it and then faults, the same way the predecessor walk ends on a null pointer. Key removal is
/// case-insensitive and stops at the first match.
/// </summary>
public sealed class VmConfigCursor1261
{
    public readonly List<string> Log = new();
    public Section? Sections;
    public Section? Tail;
    public Section? Current;
    public Entry? CurrentEntry;

    public bool SetSection(string name, bool allowCreate)
    {
        Section? section = Find(name);
        if (section == null && allowCreate)
            section = NewSection(name);
        if (section != null)
        {
            Current = section;
            CurrentEntry = section.Root;
            Log.Add($"section {section.Name} {(section.Root == null ? "-" : section.Root.Key)}");
            return true;
        }

        Log.Add($"section miss {name}");
        return false;
    }

    public bool SetFirst()
    {
        Current = Sections;
        if (Current != null)
        {
            CurrentEntry = Current.Root;
            Log.Add($"first {Current.Name}");
            return true;
        }

        Log.Add("first none");
        return false;
    }

    public bool SetNext()
    {
        if (Current != null)
        {
            Current = Current.Next;
            if (Current != null)
            {
                CurrentEntry = Current.Root;
                Log.Add($"next {Current.Name}");
                return true;
            }
        }

        Log.Add("next none");
        return false;
    }

    public string? CurrentName()
    {
        if (Current == null)
            return null;
        return Current.Name;
    }

    public void Clear()
    {
        if (Current == null)
            return;

        Entry? entry = Current.Root;
        while (entry != null)
        {
            Entry? next = entry.Next;
            Log.Add($"clear {entry.Key}");
            entry = next;
        }

        Current.Root = null;
        Current.Tail = null;
        Log.Add("clear root");
    }

    public bool Delete()
    {
        if (Current == null)
        {
            Log.Add("delete none");
            return false;
        }

        Clear();
        Section? sec = Sections;
        while (sec != null && sec.Next != Current)
            sec = sec.Next;
        if (sec == null)
            throw new NullReferenceException();

        string dropped = Current.Name;
        sec.Next = Current.Next;
        if (Tail == Current)
            Tail = sec;
        Current = sec.Next;
        Log.Add(Current == null ? $"drop {dropped} none" : $"drop {dropped} {Current.Name}");
        return Current != null;
    }

    public void ClearKey(string key)
    {
        if (Current == null)
            throw new NullReferenceException();
        if (Current.Root == null)
        {
            Log.Add("key empty");
            return;
        }

        Entry? prev = null;
        Entry? probe = Current.Root;
        while (probe != null && !string.Equals(probe.Key, key, StringComparison.OrdinalIgnoreCase))
        {
            prev = probe;
            probe = probe.Next;
        }

        if (probe == null)
        {
            Log.Add($"key miss {key}");
            return;
        }

        if (prev == null)
            Current.Root = probe.Next;
        else
            prev.Next = probe.Next;
        if (Current.Tail == probe)
            Current.Tail = prev;
        Log.Add($"key drop {probe.Key}");
    }

    public bool SectionIsEmpty()
    {
        return Current == null || Current.Root == null;
    }

    public bool Next(out string key, out string? value)
    {
        if (CurrentEntry == null)
        {
            key = "";
            value = null;
            Log.Add("item none");
            return false;
        }

        key = CurrentEntry.Key;
        value = CurrentEntry.Value;
        Log.Add($"item {key} {value}");
        CurrentEntry = CurrentEntry.Next;
        return true;
    }

    public string? GetValue(string key)
    {
        if (Current == null)
            throw new NullReferenceException();

        Entry? probe = Current.Root;
        while (probe != null && !string.Equals(probe.Key, key, StringComparison.OrdinalIgnoreCase))
            probe = probe.Next;
        if (probe == null)
            return null;
        return probe.Value;
    }

    public Entry Add(string key, string value)
    {
        if (Current == null)
            throw new NullReferenceException();

        var entry = new Entry { Key = key, Value = value };
        if (Current.Tail == null)
            Current.Root = entry;
        else
            Current.Tail.Next = entry;
        Current.Tail = entry;
        Log.Add($"entry {key} {value}");
        return entry;
    }

    Section NewSection(string name)
    {
        Section? section = Find(name);
        if (section != null)
            return section;

        section = new Section { Name = name };
        if (Tail == null)
            Sections = section;
        else
            Tail.Next = section;
        Tail = section;
        Log.Add($"create {name}");
        return section;
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
    }
}
