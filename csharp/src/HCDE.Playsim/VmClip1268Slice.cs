namespace HCDE.Playsim;

/// <summary>
/// Solid-segment clip list from <c>r_clipsegment.cpp</c> (1-192). Chunk 1268.
/// Clear plants the two sentinels. A visitor that rejects a span leaves the list
/// unchanged. A solid span that only meets the next post slides that post backward.
/// The crunch copies one slot past the cursor, so a merged list can keep a trailing
/// zero post. A pass wall is drawn and does not close the opening.
/// </summary>
public sealed class VmClip1268
{
    readonly short[] _first = new short[32];
    readonly short[] _last = new short[32];
    int _end;
    Func<int, int, bool>? _render;

    public readonly List<string> Log = new();

    public void Clear(short left, short right)
    {
        Array.Clear(_first);
        Array.Clear(_last);
        _first[0] = -0x7fff;
        _last[0] = left;
        _first[1] = right;
        _last[1] = 0x7fff;
        _end = 2;
    }

    public bool Check(int first, int last)
    {
        int start = 0;
        while (_last[start] < first)
            start++;
        if (first < _first[start])
            return true;
        if (last > _last[start])
            return true;
        return false;
    }

    public bool IsVisible(int sx1, int sx2)
    {
        if (sx2 <= sx1)
            return false;
        int start = 0;
        while (_last[start] < sx2)
            start++;
        if (sx1 >= _first[start] && sx2 <= _last[start])
            return false;
        return true;
    }

    public bool Clip(int first, int last, bool solid, Func<int, int, bool>? render)
    {
        _render = render;
        int start = 0;
        while (_last[start] < first)
            start++;
        bool res = false;
        if (first < _first[start])
        {
            res = true;
            if (last <= _first[start])
            {
                if (!Draw(first, last))
                    return true;
                if (solid)
                {
                    if (last == _first[start])
                        _first[start] = (short)first;
                    else
                        Insert(start, first, last);
                }
                return true;
            }

            if (Draw(first, _first[start]) && solid)
                _first[start] = (short)first;
        }

        if (last <= _last[start])
            return res;

        bool clipsegment = false;
        int next = start;
        while (last >= _first[next + 1])
        {
            clipsegment = Draw(_last[next], _first[next + 1]);
            next++;
            if (last <= _last[next])
            {
                last = _last[next];
                goto crunch;
            }
        }

        clipsegment = Draw(_last[next], last);
    crunch:
        if (!clipsegment)
            return true;
        if (solid)
        {
            _last[start] = (short)last;
            if (next != start)
            {
                int i = 1;
                for (int j = _end - next; j > 0; i++, j--)
                    Copy(start + i, next + i);
                _end = start + i;
            }
        }

        return true;
    }

    public string Dump(string tag)
    {
        var text = tag + " end=" + _end.ToString(System.Globalization.CultureInfo.InvariantCulture);
        for (int i = 0; i < _end; i++)
            text += " [" + _first[i].ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + _last[i].ToString(System.Globalization.CultureInfo.InvariantCulture) + "]";
        return text;
    }

    void Insert(int start, int first, int last)
    {
        int next = _end;
        _end++;
        while (next != start)
        {
            Copy(next, next - 1);
            next--;
        }

        _first[next] = (short)first;
        _last[next] = (short)last;
    }

    bool Draw(int x1, int x2)
    {
        if (_render == null)
            throw new NullReferenceException();
        bool ok = _render(x1, x2);
        Log.Add("draw " + x1.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + x2.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + (ok ? "1" : "0"));
        return ok;
    }

    void Copy(int dst, int src)
    {
        _first[dst] = _first[src];
        _last[dst] = _last[src];
    }
}
