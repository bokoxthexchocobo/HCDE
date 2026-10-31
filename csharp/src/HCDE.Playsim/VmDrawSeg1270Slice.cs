namespace HCDE.Playsim;

/// <summary>
/// Portal segment stacks and sprite clip groups from <c>r_drawsegment.cpp</c> (1-223).
/// Chunk 1270. Segment zero is the newest push. Groups walk that order in bands of
/// 100. A top silhouette keeps the greater column and a bottom silhouette keeps the
/// lesser one, including a negative sample. Clip pointers are stored minus x1.
/// An undrawn range restores the backup only when the range was drawn, and the
/// drawn flag drops before that copy.
/// </summary>
public sealed class VmFrameMem1270
{
    public int Allocs;

    public short[] Alloc(int count)
    {
        Allocs++;
        return new short[count];
    }
}

public sealed class VmDrawSeg1270
{
    public int Id;
    public short X1;
    public short X2;
    public float Sz1;
    public float Sz2;
    public byte Silhouette;
    public short[]? Top;
    public int TopOrigin;
    public short[]? Bottom;
    public int BottomOrigin;
}

public sealed class VmSegGroup1270
{
    public int Begin;
    public int End;
    public short X1;
    public short X2;
    public float Near;
    public float Far;
    public short[] Top = [];
    public short[] Bottom = [];
}

public sealed class VmDrawList1270
{
    public int ViewHeight;
    public VmFrameMem1270? Thread = new();
    public readonly List<VmSegGroup1270> Groups = new();
    readonly List<VmDrawSeg1270?> _segs = new();
    readonly List<int> _starts = new();
    readonly List<VmDrawSeg1270?> _trans = new();
    readonly List<int> _tstarts = new();
    readonly short[] _top = new short[256];
    readonly short[] _bottom = new short[256];

    public void Clear()
    {
        _segs.Clear();
        _starts.Clear();
        _starts.Add(0);
        _trans.Clear();
        _tstarts.Clear();
        _tstarts.Add(0);
    }

    public void PushPortal()
    {
        _starts.Add(_segs.Count);
        _tstarts.Add(_trans.Count);
    }

    public void PopPortal()
    {
        int at = Last(_starts);
        _segs.RemoveRange(at, _segs.Count - at);
        _starts.RemoveAt(_starts.Count - 1);
        int transAt = Last(_tstarts);
        _trans.RemoveRange(transAt, _trans.Count - transAt);
        _tstarts.RemoveAt(_tstarts.Count - 1);
    }

    public void Push(VmDrawSeg1270? segment) => _segs.Add(segment);

    public void PushTranslucent(VmDrawSeg1270? segment) => _trans.Add(segment);

    public int SegmentsCount() => _segs.Count - Last(_starts);

    public int TranslucentCount() => _trans.Count - Last(_tstarts);

    public VmDrawSeg1270? Segment(int index) => _segs[_segs.Count - 1 - index];

    public VmDrawSeg1270? Translucent(int index) => _trans[_trans.Count - 1 - index];

    public void Build()
    {
        if (Thread == null)
            throw new NullReferenceException();
        Groups.Clear();
        int count = SegmentsCount();
        const int groupSize = 100;
        for (int index = 0; index < count; index += groupSize)
        {
            var ds = Segment(index);
            if (ds == null)
                throw new NullReferenceException();
            var group = new VmSegGroup1270
            {
                Begin = index,
                End = Math.Min(index + groupSize, count),
                X1 = ds.X1,
                X2 = ds.X2,
                Near = Math.Min(ds.Sz1, ds.Sz2),
                Far = Math.Max(ds.Sz1, ds.Sz2),
            };
            for (int groupIndex = group.Begin + 1; groupIndex < group.End; groupIndex++)
            {
                ds = Segment(groupIndex);
                if (ds == null)
                    throw new NullReferenceException();
                group.X1 = (short)Math.Min(group.X1, ds.X1);
                group.X2 = (short)Math.Max(group.X2, ds.X2);
                group.Near = Math.Min(group.Near, ds.Sz1);
                group.Near = Math.Min(group.Near, ds.Sz2);
                group.Far = Math.Max(ds.Sz1, group.Far);
                group.Far = Math.Max(ds.Sz2, group.Far);
            }

            for (int x = group.X1; x < group.X2; x++)
            {
                _top[x] = 0;
                _bottom[x] = unchecked((short)ViewHeight);
            }

            for (int groupIndex = group.Begin; groupIndex < group.End; groupIndex++)
            {
                ds = Segment(groupIndex);
                if (ds == null)
                    throw new NullReferenceException();
                if ((ds.Silhouette & 1) != 0)
                    MergeMin(_bottom, ds.Bottom, ds.BottomOrigin, ds.X1, ds.X2 - ds.X1);
                if ((ds.Silhouette & 2) != 0)
                    MergeMax(_top, ds.Top, ds.TopOrigin, ds.X1, ds.X2 - ds.X1);
            }

            int width = group.X2 - group.X1;
            var top = Thread.Alloc(width);
            var bottom = Thread.Alloc(width);
            for (int i = 0; i < width; i++)
            {
                top[i] = _top[group.X1 + i];
                bottom[i] = _bottom[group.X1 + i];
            }

            group.Top = top;
            group.Bottom = bottom;
            Groups.Add(group);
        }
    }

    static void MergeMin(short[] dest, short[]? src, int origin, int x1, int count)
    {
        int i = count;
        int x = x1;
        do
        {
            if (src == null)
                throw new NullReferenceException();
            short sample = src[x - origin];
            if (dest[x] > sample)
                dest[x] = sample;
            x++;
        }
        while (--i != 0);
    }

    static void MergeMax(short[] dest, short[]? src, int origin, int x1, int count)
    {
        int i = count;
        int x = x1;
        do
        {
            if (src == null)
                throw new NullReferenceException();
            short sample = src[x - origin];
            if (dest[x] < sample)
                dest[x] = sample;
            x++;
        }
        while (--i != 0);
    }

    static int Last(List<int> stack)
    {
        if (stack.Count == 0)
            throw new IndexOutOfRangeException();
        return stack[^1];
    }
}

public sealed class VmDrawClip1270
{
    public int ViewHeight;
    public bool SprClipped;
    short[]? _top;
    int _topOrigin;
    short[]? _bottom;
    int _bottomOrigin;
    short[]? _backup;
    int _backupOrigin;

    public void SetTopClip(VmFrameMem1270? mem, int x1, int x2, short[]? ceiling)
    {
        var data = Allocate(mem, x2 - x1);
        if (ceiling == null)
            throw new NullReferenceException();
        for (int i = 0; i < data.Length; i++)
            data[i] = ceiling[x1 + i];
        _top = data;
        _topOrigin = x1;
    }

    public void SetTopClip(VmFrameMem1270? mem, int x1, int x2, short value)
    {
        var data = Allocate(mem, x2 - x1);
        for (int i = 0; i < data.Length; i++)
            data[i] = value;
        _top = data;
        _topOrigin = x1;
    }

    public void SetBottomClip(VmFrameMem1270? mem, int x1, int x2, short[]? floor)
    {
        var data = Allocate(mem, x2 - x1);
        if (floor == null)
            throw new NullReferenceException();
        for (int i = 0; i < data.Length; i++)
            data[i] = floor[x1 + i];
        _bottom = data;
        _bottomOrigin = x1;
    }

    public void SetBottomClip(VmFrameMem1270? mem, int x1, int x2, short value)
    {
        var data = Allocate(mem, x2 - x1);
        for (int i = 0; i < data.Length; i++)
            data[i] = value;
        _bottom = data;
        _bottomOrigin = x1;
    }

    public void SetBackupClip(VmFrameMem1270? mem, int x1, int x2, short[]? ceiling)
    {
        var data = Allocate(mem, x2 - x1);
        if (ceiling == null)
            throw new NullReferenceException();
        for (int i = 0; i < data.Length; i++)
            data[i] = ceiling[x1 + i];
        _backup = data;
        _backupOrigin = x1;
    }

    public void SetRangeDrawn(int x1, int x2)
    {
        SprClipped = true;
        if (_top == null)
            throw new NullReferenceException();
        short value = unchecked((short)(ushort)ViewHeight);
        int count = x2 - x1;
        for (int i = 0; i < count; i++)
            _top[x1 - _topOrigin + i] = value;
    }

    public void SetRangeUndrawn(int x1, int x2)
    {
        if (!SprClipped)
            return;
        SprClipped = false;
        if (_top == null || _backup == null)
            throw new NullReferenceException();
        int count = x2 - x1;
        for (int i = 0; i < count; i++)
            _top[x1 - _topOrigin + i] = _backup[x1 - _backupOrigin + i];
    }

    public short TopAt(int x)
    {
        if (_top == null)
            throw new NullReferenceException();
        return _top[x - _topOrigin];
    }

    public short BottomAt(int x)
    {
        if (_bottom == null)
            throw new NullReferenceException();
        return _bottom[x - _bottomOrigin];
    }

    static short[] Allocate(VmFrameMem1270? mem, int count)
    {
        if (mem == null)
            throw new NullReferenceException();
        return mem.Alloc(count);
    }
}
