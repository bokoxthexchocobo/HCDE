namespace HCDE.Playsim;

/// <summary>
/// Counted malloc, the size prefix, and the free report from
/// <c>m_alloc.cpp</c> (1-201). Chunk 1316.
/// </summary>
public sealed class VmCountedAlloc1316
{
    public const int SizeTBytes = 8;

    readonly bool _prefixed;
    readonly bool _sunMsize;
    int _next = 1;
    readonly Dictionary<int, Slot> _live = new();

    public VmCountedAlloc1316(bool prefixed, bool sunMsize)
    {
        _prefixed = prefixed;
        _sunMsize = sunMsize;
    }

    public Func<int, ulong>? QuerySize { get; set; }

    public int FailuresLeft { get; set; }

    public List<ulong> Attempts { get; } = new();

    public List<CountReport> Reports { get; } = new();

    public List<int> Freed { get; } = new();

    public List<int> ReallocTargets { get; } = new();

    public ulong LastCleared { get; private set; }

    public int Malloc(ulong size)
    {
        return Allocate(size, null, 0, false);
    }

    public int MallocDbg(ulong size, string file, int line)
    {
        return Allocate(size, file, line, true);
    }

    public int Realloc(int block, ulong size)
    {
        return Resize(block, size, null, 0, false);
    }

    public int ReallocDbg(int block, ulong size, string file, int line)
    {
        return Resize(block, size, file, line, true);
    }

    public int Calloc(ulong count, ulong size)
    {
        ulong bytes = unchecked(count * size);
        int block = Malloc(bytes);
        Clear(block, bytes);
        return block;
    }

    public void Free(int block)
    {
        if (block == 0)
            return;

        Reports.Add(new CountReport("dealloc", Msize(block), 0));
        Slot slot = _live[block];
        Freed.Add(_prefixed ? slot.Base : slot.User);
        _live.Remove(block);
    }

    public int Base(int block)
    {
        return _live[block].Base;
    }

    public ulong Stored(int block)
    {
        return _live[block].Stored;
    }

    public byte[] Bytes(int block)
    {
        return _live[block].Bytes;
    }

    public bool Contains(int block)
    {
        return _live.ContainsKey(block);
    }

    int Allocate(ulong size, string? file, int line, bool debug)
    {
        ulong heapSize = size;
        if (_prefixed)
            heapSize += (ulong)SizeTBytes;

        int heap = Take(heapSize);
        if (heap == 0)
            throw new InvalidOperationException(MallocFailure(size, file, line, debug));

        int user = heap;
        ulong stored = 0;
        if (_prefixed)
        {
            stored = size;
            user = _next++;
        }

        _live[user] = new Slot
        {
            User = user,
            Base = heap,
            Requested = size,
            Stored = stored,
            Bytes = Paint(size),
        };
        Reports.Add(new CountReport("alloc", Msize(user), 0));
        return user;
    }

    int Resize(int block, ulong size, string? file, int line, bool debug)
    {
        if (_prefixed && block == 0)
            return Allocate(size, file, line, debug);

        ulong oldSize = block == 0 ? 0 : Msize(block);
        if (block == 0)
            ReallocTargets.Add(0);
        else
            ReallocTargets.Add(_prefixed ? _live[block].Base : block);

        ulong heapSize = size;
        if (_prefixed)
            heapSize += (ulong)SizeTBytes;

        int heap = Take(heapSize);
        if (heap == 0)
            throw new InvalidOperationException(ReallocFailure(size, file, line, debug));

        if (block != 0)
            _live.Remove(block);

        int user = heap;
        ulong stored = 0;
        if (_prefixed)
        {
            stored = size;
            user = _next++;
        }

        _live[user] = new Slot
        {
            User = user,
            Base = heap,
            Requested = size,
            Stored = stored,
            Bytes = Paint(size),
        };
        Reports.Add(new CountReport("realloc", oldSize, Msize(user)));
        return user;
    }

    void Clear(int block, ulong bytes)
    {
        LastCleared = bytes;
        byte[] data = _live[block].Bytes;
        int count = bytes > (ulong)data.Length ? data.Length : (int)bytes;
        Array.Clear(data, 0, count);
    }

    int Take(ulong heapSize)
    {
        Attempts.Add(heapSize);
        if (FailuresLeft > 0)
        {
            FailuresLeft--;
            return 0;
        }

        return _next++;
    }

    ulong Msize(int block)
    {
        if (_sunMsize)
            return 0;

        Func<int, ulong>? query = QuerySize;
        if (query != null)
            return query(block);

        return _live[block].Requested;
    }

    static byte[] Paint(ulong size)
    {
        if (size > 4096)
            return Array.Empty<byte>();

        var data = new byte[(int)size];
        Array.Fill(data, (byte)0xA5);
        return data;
    }

    static string MallocFailure(ulong size, string? file, int line, bool debug)
    {
        if (debug)
            return $"Could not malloc {size} bytes in {file}, line {line}";

        return $"Could not malloc {size} bytes";
    }

    static string ReallocFailure(ulong size, string? file, int line, bool debug)
    {
        if (debug)
            return $"Could not realloc {size} bytes in {file}, line {line}";

        return $"Could not realloc {size} bytes";
    }

    sealed class Slot
    {
        public int User;
        public int Base;
        public ulong Requested;
        public ulong Stored;
        public byte[] Bytes = Array.Empty<byte>();
    }
}

public readonly record struct CountReport(string Kind, ulong Before, ulong After);
