using System.Text;

namespace HCDE.Playsim;

/// <summary>
/// ACS stack list and string-pool add/lock/mark from <c>p_acs.cpp</c> (882–901, 945–1119).</summary>
public sealed class VmPAcsStackStringPool
{
    public static int UaLong(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 4) return 0;
        return bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24);
    }

    public const int StackSize = 4096;
    public const int NumBuckets = 251;
    public const uint FreeEntry = 0xFFFFFFFE;
    public const uint NoEntry = 0xFFFFFFFF;
    public const int LibraryIdMask = unchecked((int)0xFFF00000);
    public const int StrPoolLibraryIdOr = (int.MaxValue >> 20) << 20;

    public sealed class AcsStack
    {
        public int Sp { get; set; }
        public AcsStack? Next { get; private set; }
        public AcsStack? Prev { get; private set; }
        public static AcsStack? Head { get; private set; }
        public int[] Buffer { get; } = new int[StackSize];

        public AcsStack()
        {
            Sp = 0;
            Next = Head;
            Prev = null;
            if (Head is not null)
                Head.Prev = this;
            Head = this;
        }

        public void Unlink()
        {
            if (Next is not null)
                Next.Prev = Prev;
            if (Prev is null)
                Head = Next;
            else
                Prev.Next = Next;
        }
    }

    private readonly List<PoolEntry> _pool = [];
    private readonly uint[] _buckets = new uint[NumBuckets];
    private uint _firstFree;

    public VmPAcsStackStringPool()
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

    public int AddString(string? str)
    {
        str ??= "";
        var bytes = Encoding.UTF8.GetBytes(str);
        var h = VmAcsStringPool.SuperFastHash(bytes);
        var bucket = h % NumBuckets;
        var found = FindString(str, bytes.Length, h, bucket);
        if (found >= 0)
            return found | StrPoolLibraryIdOr;
        return InsertString(str, h, bucket);
    }

    public string? GetString(int strnum)
    {
        if ((strnum & LibraryIdMask) != StrPoolLibraryIdOr)
            return null;
        strnum &= ~LibraryIdMask;
        if ((uint)strnum < _pool.Count && _pool[strnum].Next != FreeEntry)
            return _pool[strnum].Str;
        return null;
    }

    public void LockString(int levelnum, int strnum)
    {
        strnum &= ~LibraryIdMask;
        if ((uint)strnum >= _pool.Count) return;
        _pool[strnum].Lock(levelnum);
    }

    public void MarkString(int strnum)
    {
        strnum &= ~LibraryIdMask;
        if ((uint)strnum >= _pool.Count) return;
        _pool[strnum].Mark = true;
    }

    public void LockStringArray(int levelnum, ReadOnlySpan<int> ids)
    {
        foreach (var num in ids)
        {
            if ((num & LibraryIdMask) != StrPoolLibraryIdOr) continue;
            var idx = num & ~LibraryIdMask;
            if ((uint)idx < _pool.Count)
                _pool[idx].Lock(levelnum);
        }
    }

    public void MarkStringArray(ReadOnlySpan<int> ids)
    {
        foreach (var num in ids)
        {
            if ((num & LibraryIdMask) != StrPoolLibraryIdOr) continue;
            var idx = num & ~LibraryIdMask;
            if ((uint)idx < _pool.Count)
                _pool[idx].Mark = true;
        }
    }

    public void MarkStringMap(IReadOnlyDictionary<int, int> values)
    {
        foreach (var num in values.Values)
        {
            if ((num & LibraryIdMask) != StrPoolLibraryIdOr) continue;
            var idx = num & ~LibraryIdMask;
            if ((uint)idx < _pool.Count)
                _pool[idx].Mark = true;
        }
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
        if (index >= StrPoolLibraryIdOr)
            return -1;
        if (index == _pool.Count)
        {
            _pool.Add(new PoolEntry());
            _firstFree++;
        }
        else
        {
            while (_firstFree < _pool.Count && _pool[(int)_firstFree].Next != FreeEntry)
                _firstFree++;
        }

        var entry = _pool[(int)index];
        entry.Str = str;
        entry.Hash = h;
        entry.Next = _buckets[bucket];
        entry.Mark = false;
        entry.Locks.Clear();
        _buckets[bucket] = index;
        return (int)index | StrPoolLibraryIdOr;
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
