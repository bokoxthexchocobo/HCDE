using System.Text;

namespace HCDE.Playsim;

/// <summary><c>ACSStringPool</c> add/lock/mark/purge from <c>p_acs.cpp</c> (945–1274).</summary>
public sealed class VmAcsStringPool
{
    public const int NumBuckets = 251;
    public const uint FreeEntry = 0xFFFFFFFE;
    public const uint NoEntry = 0xFFFFFFFF;
    public const int MinGcSize = 100;
    public const int LibraryIdMask = unchecked((int)0xFFF00000);
    public const int LibraryIdShift = 20;
    public const int StrPoolLibraryId = int.MaxValue >> LibraryIdShift;
    public const int StrPoolLibraryIdOr = StrPoolLibraryId << LibraryIdShift;

    private readonly List<PoolEntry> _pool = [];
    private readonly uint[] _poolBuckets = new uint[NumBuckets];
    private uint _firstFreeEntry;

    public VmAcsStringPool()
    {
        for (var i = 0; i < _poolBuckets.Length; i++)
            _poolBuckets[i] = NoEntry;
        _firstFreeEntry = 0;
    }

    public void Clear()
    {
        _pool.Clear();
        for (var i = 0; i < _poolBuckets.Length; i++)
            _poolBuckets[i] = NoEntry;
        _firstFreeEntry = 0;
    }

    public int AddString(string? str)
    {
        str ??= "";
        var bytes = Encoding.UTF8.GetBytes(str);
        var h = SuperFastHash(bytes);
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

    public void LockStringArray(int levelnum, ReadOnlySpan<int> strnums)
    {
        foreach (var num in strnums)
        {
            if ((num & LibraryIdMask) != StrPoolLibraryIdOr) continue;
            var idx = num & ~LibraryIdMask;
            if ((uint)idx < _pool.Count)
                _pool[idx].Lock(levelnum);
        }
    }

    public void MarkStringArray(ReadOnlySpan<int> strnums)
    {
        foreach (var num in strnums)
        {
            if ((num & LibraryIdMask) != StrPoolLibraryIdOr) continue;
            var idx = num & ~LibraryIdMask;
            if ((uint)idx < _pool.Count)
                _pool[idx].Mark = true;
        }
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
        for (var i = 0; i < _poolBuckets.Length; i++)
            _poolBuckets[i] = NoEntry;

        for (var i = 0; i < _pool.Count; i++)
        {
            var entry = _pool[i];
            if (entry.Next == FreeEntry) continue;

            if (entry.Locks.Count == 0 && !entry.Mark)
            {
                entry.Next = FreeEntry;
                entry.Str = "";
                if (i < _firstFreeEntry)
                    _firstFreeEntry = (uint)i;
            }
            else
            {
                var bucket = entry.Hash % NumBuckets;
                entry.Next = _poolBuckets[bucket];
                _poolBuckets[bucket] = (uint)i;
                entry.Mark = false;
            }
        }
    }

    private int FindString(string str, int len, uint h, uint bucketnum)
    {
        var i = _poolBuckets[bucketnum];
        while (i != NoEntry)
        {
            var entry = _pool[(int)i];
            if (entry.Hash == h && entry.Str.Length == len && entry.Str == str)
                return (int)i;
            i = entry.Next;
        }
        return -1;
    }

    private int InsertString(string str, uint h, uint bucketnum)
    {
        var index = _firstFreeEntry;
        if (_firstFreeEntry >= StrPoolLibraryIdOr)
            return -1;

        if (index == _pool.Count)
        {
            _pool.Add(new PoolEntry());
            _firstFreeEntry++;
        }
        else
            FindFirstFreeEntry(_firstFreeEntry + 1);

        var entry = _pool[(int)index];
        entry.Str = str;
        entry.Hash = h;
        entry.Next = _poolBuckets[bucketnum];
        entry.Mark = false;
        entry.Locks.Clear();
        _poolBuckets[bucketnum] = index;
        return (int)index | StrPoolLibraryIdOr;
    }

    private void FindFirstFreeEntry(uint baseIndex)
    {
        while (baseIndex < _pool.Count && _pool[(int)baseIndex].Next != FreeEntry)
            baseIndex++;
        _firstFreeEntry = baseIndex;
    }

    public static uint SuperFastHash(ReadOnlySpan<byte> data)
    {
        if (data.Length == 0) return 0;
        var hash = (uint)data.Length;
        var rem = data.Length & 3;
        var dataIndex = 0;
        var len = data.Length - rem;
        while (dataIndex < len)
        {
            hash += BitConverter.ToUInt16(data.Slice(dataIndex, 2));
            var tmp = (uint)(BitConverter.ToUInt16(data.Slice(dataIndex + 2, 2)) << 11) ^ hash;
            hash = (hash << 16) ^ tmp;
            dataIndex += 4;
        }
        switch (rem)
        {
            case 3:
                hash += (uint)data[dataIndex + 2] << 16;
                hash += (uint)BitConverter.ToUInt16(data.Slice(dataIndex, 2));
                hash ^= hash << 16;
                hash ^= (uint)data[dataIndex + 2] << 18;
                hash += hash >> 11;
                break;
            case 2:
                hash += BitConverter.ToUInt16(data.Slice(dataIndex, 2));
                hash ^= hash << 11;
                hash += hash >> 17;
                break;
            case 1:
                hash += data[dataIndex];
                hash ^= hash << 10;
                hash += hash >> 1;
                break;
        }
        hash ^= hash << 3;
        hash += hash >> 5;
        hash ^= hash << 4;
        hash += hash >> 17;
        hash ^= hash << 25;
        hash += hash >> 6;
        return hash;
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
