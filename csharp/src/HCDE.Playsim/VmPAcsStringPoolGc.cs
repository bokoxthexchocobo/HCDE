using System.Text;

namespace HCDE.Playsim;

/// <summary>
/// ACS string pool purge, find, and insert from <c>p_acs.cpp</c> (1129–1291).</summary>
public sealed class VmPAcsStringPoolGc
{
    public const int NumBuckets = 251;
    public const uint FreeEntry = 0xFFFFFFFE;
    public const uint NoEntry = 0xFFFFFFFF;
    public const int MinGcSize = 100;
    public const int LibraryIdMask = unchecked((int)0xFFF00000);
    public const int StrPoolLibraryIdOr = (int.MaxValue >> 20) << 20;

    private readonly List<PoolEntry> _pool = [];
    private readonly uint[] _buckets = new uint[NumBuckets];
    private uint _firstFree;
    public Action? CollectGlobalStrings { get; set; }

    public VmPAcsStringPoolGc()
    {
        for (var i = 0; i < _buckets.Length; i++)
            _buckets[i] = NoEntry;
    }

    public void Clear()
    {
        _pool.Clear();
        for (var i = 0; i < _buckets.Length; i++)
            _buckets[i] = NoEntry;
        _firstFree = 0;
    }

    public int AddString(string str)
    {
        var bytes = Encoding.UTF8.GetBytes(str);
        var h = VmAcsStringPool.SuperFastHash(bytes);
        var bucket = h % NumBuckets;
        var found = FindString(str, bytes.Length, h, bucket);
        if (found >= 0)
            return found | StrPoolLibraryIdOr;
        return InsertString(str, h, bucket);
    }

    public void UnlockAll()
    {
        foreach (var entry in _pool)
        {
            entry.Mark = false;
            entry.Locks.Clear();
        }
    }

    public void PurgeStrings()
    {
        for (var i = 0; i < _buckets.Length; i++)
            _buckets[i] = NoEntry;

        for (var i = 0; i < _pool.Count; i++)
        {
            var entry = _pool[i];
            if (entry.Next == FreeEntry) continue;
            if (entry.Locks.Count == 0 && !entry.Mark)
            {
                entry.Next = FreeEntry;
                entry.Str = "";
                if (i < _firstFree)
                    _firstFree = (uint)i;
            }
            else
            {
                var bucket = entry.Hash % NumBuckets;
                entry.Next = _buckets[bucket];
                _buckets[bucket] = (uint)i;
                entry.Mark = false;
            }
        }
    }

    public string? GetString(int strnum)
    {
        strnum &= ~LibraryIdMask;
        if ((uint)strnum < _pool.Count && _pool[strnum].Next != FreeEntry)
            return _pool[strnum].Str;
        return null;
    }

    private int FindString(string str, int len, uint h, uint bucket)
    {
        var i = _buckets[bucket];
        while (i != NoEntry)
        {
            var entry = _pool[(int)i];
            if (entry.Hash == h && entry.Str.Length == len && entry.Str == str)
                return (int)i;
            i = entry.Next;
        }

        return -1;
    }

    private int InsertString(string str, uint h, uint bucket)
    {
        var index = _firstFree;
        if (index >= MinGcSize && index == _pool.Count)
        {
            CollectGlobalStrings?.Invoke();
            index = _firstFree;
        }

        if (_firstFree >= StrPoolLibraryIdOr)
            return -1;

        if (index == _pool.Count)
        {
            var grow = Math.Max(MinGcSize, 1);
            for (var g = 0; g < grow; g++)
                _pool.Add(new PoolEntry());
            _firstFree++;
        }
        else
            FindFirstFreeEntry(_firstFree + 1);

        var entry = _pool[(int)index];
        entry.Str = str;
        entry.Hash = h;
        entry.Next = _buckets[bucket];
        entry.Mark = false;
        entry.Locks.Clear();
        _buckets[bucket] = index;
        return (int)index | StrPoolLibraryIdOr;
    }

    private void FindFirstFreeEntry(uint start)
    {
        while (start < _pool.Count && _pool[(int)start].Next != FreeEntry)
            start++;
        _firstFree = start;
    }

    public void ReadStrings(int poolSize, IEnumerable<(uint Index, string Str)> entries)
    {
        Clear();
        while (_pool.Count < poolSize)
            _pool.Add(new PoolEntry());
        foreach (var (index, str) in entries)
        {
            if (index >= _pool.Count) continue;
            var bytes = Encoding.UTF8.GetBytes(str);
            var h = VmAcsStringPool.SuperFastHash(bytes);
            var bucket = h % NumBuckets;
            var entry = _pool[(int)index];
            entry.Str = str;
            entry.Hash = h;
            entry.Next = _buckets[bucket];
            entry.Mark = false;
            entry.Locks.Clear();
            _buckets[bucket] = index;
        }

        FindFirstFreeEntry(_firstFree);
    }

    public void LockString(int levelnum, int strnum)
    {
        strnum &= ~LibraryIdMask;
        if ((uint)strnum >= _pool.Count) return;
        _pool[strnum].Lock(levelnum);
    }

    private sealed class PoolEntry
    {
        public string Str = "";
        public uint Hash;
        public uint Next = FreeEntry;
        public bool Mark;
        public List<int> Locks { get; } = [];

        public void Lock(int levelnum)
        {
            if (!Locks.Contains(levelnum))
                Locks.Add(levelnum);
        }
    }
}
