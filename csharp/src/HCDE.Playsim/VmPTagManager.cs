namespace HCDE.Playsim;

/// <summary><c>FTagManager</c> and tag iterators from <c>p_tags.cpp</c> (36–393, 404–464).</summary>
public sealed class VmPTagManager
{
    public const int TagHashSize = 256;

    public readonly record struct TagItem(int Target, int Tag, int NextTag);

    readonly List<TagItem> _allTags = new();
    readonly List<TagItem> _allIds = new();
    readonly List<int> _startForSector = new();
    readonly List<int> _startForLine = new();
    readonly int[] _tagHashFirst = new int[TagHashSize];
    readonly int[] _idHashFirst = new int[TagHashSize];
    int _sectorCount;

    public IReadOnlyList<TagItem> AllTags => _allTags;
    public IReadOnlyList<TagItem> AllIds => _allIds;
    public IReadOnlyList<int> IdHashFirst => _idHashFirst;

    public void SetSectorCount(int count) => _sectorCount = count;

    public void AddSectorTag(int sector, int tag)
    {
        if (tag == 0)
            return;
        while (_startForSector.Count <= sector)
            _startForSector.Add(-1);
        if (_startForSector[sector] == -1)
            _startForSector[sector] = _allTags.Count;
        else
        {
            for (var i = _startForSector[sector]; i < _allTags.Count; i++)
            {
                if (_allTags[i].Tag == tag)
                    return;
            }
        }
        _allTags.Add(new TagItem(sector, tag, -1));
    }

    public void RemoveSectorTags(int sect)
    {
        if (_startForSector.Count <= sect)
            return;
        var start = _startForSector[sect];
        if (start < 0)
            return;
        while (start < _allTags.Count && _allTags[start].Target == sect)
        {
            var t = _allTags[start];
            _allTags[start] = new TagItem(-1, -1, t.NextTag);
            start++;
        }
    }

    public void RemoveLineIds(int line)
    {
        if (_startForLine.Count <= line)
            return;
        var start = _startForLine[line];
        if (start < 0)
            return;
        while (start < _allIds.Count && _allIds[start].Target == line)
        {
            var t = _allIds[start];
            _allIds[start] = new TagItem(-1, -1, t.NextTag);
            start++;
        }
    }

    public void AddLineId(int line, int tag)
    {
        if (tag == -1)
            return;
        while (_startForLine.Count <= line)
            _startForLine.Add(-1);
        if (_startForLine[line] == -1)
            _startForLine[line] = _allIds.Count;
        else
        {
            for (var i = _startForLine[line]; i < _allIds.Count; i++)
            {
                if (_allIds[i].Tag == tag)
                    return;
            }
        }
        _allIds.Add(new TagItem(line, tag, -1));
    }

    public void HashTags()
    {
        _allTags.Add(new TagItem(-1, -1, -1));
        _allIds.Add(new TagItem(-1, -1, -1));
        Array.Fill(_tagHashFirst, -1);
        Array.Fill(_idHashFirst, -1);
        for (var i = _allTags.Count - 1; i >= 0; i--)
        {
            if (_allTags[i].Target < 0)
                continue;
            var hash = (int)(((uint)_allTags[i].Tag) % TagHashSize);
            var next = _tagHashFirst[hash];
            _allTags[i] = _allTags[i] with { NextTag = next };
            _tagHashFirst[hash] = i;
        }
        for (var i = _allIds.Count - 1; i >= 0; i--)
        {
            if (_allIds[i].Target < 0)
                continue;
            var hash = (int)(((uint)_allIds[i].Tag) % TagHashSize);
            var next = _idHashFirst[hash];
            _allIds[i] = _allIds[i] with { NextTag = next };
            _idHashFirst[hash] = i;
        }
    }

    public bool SectorHasTags(int sectorIndex) =>
        sectorIndex >= 0 && sectorIndex < _startForSector.Count && _startForSector[sectorIndex] != -1;

    public int GetFirstSectorTag(int sectorIndex) =>
        SectorHasTags(sectorIndex) ? _allTags[_startForSector[sectorIndex]].Tag : 0;

    public bool SectorHasTag(int sectorIndex, int tag)
    {
        if (!SectorHasTags(sectorIndex))
            return false;
        var ndx = _startForSector[sectorIndex];
        while (ndx < _allTags.Count && _allTags[ndx].Target == sectorIndex)
        {
            if (_allTags[ndx].Tag == tag)
                return true;
            ndx++;
        }
        return false;
    }

    public bool LineHasIds(int lineIndex) =>
        lineIndex >= 0 && lineIndex < _startForLine.Count && _startForLine[lineIndex] != -1;

    public int GetFirstLineId(int lineIndex) =>
        LineHasIds(lineIndex) ? _allIds[_startForLine[lineIndex]].Tag : 0;

    public bool LineHasId(int lineIndex, int tag)
    {
        if (!LineHasIds(lineIndex))
            return false;
        var ndx = _startForLine[lineIndex];
        while (ndx < _allIds.Count && _allIds[ndx].Target == lineIndex)
        {
            if (_allIds[ndx].Tag == tag)
                return true;
            ndx++;
        }
        return false;
    }

    public int CountSectorTags(int sectorIndex)
    {
        if (!SectorHasTags(sectorIndex))
            return 0;
        var n = _allTags.Count;
        var j = _startForSector[sectorIndex];
        var c = 0;
        while (j < n && _allTags[j].Target == sectorIndex)
        {
            j++;
            c++;
        }
        return c;
    }

    public int GetSectorTag(int sectorIndex, int index)
    {
        if (!SectorHasTags(sectorIndex))
            return 0;
        var n = _allTags.Count;
        var j = _startForSector[sectorIndex] + index;
        return j < n && _allTags[j].Target == sectorIndex ? _allTags[j].Tag : 0;
    }

    public int CountLineIds(int lineIndex)
    {
        if (!LineHasIds(lineIndex))
            return 0;
        var n = _allIds.Count;
        var j = _startForLine[lineIndex];
        var c = 0;
        while (j < n && _allIds[j].Target == lineIndex)
        {
            j++;
            c++;
        }
        return c;
    }

    public int GetLineId(int lineIndex, int index)
    {
        if (!LineHasIds(lineIndex))
            return 0;
        var n = _allIds.Count;
        var j = _startForLine[lineIndex] + index;
        return j < n && _allIds[j].Target == lineIndex ? _allIds[j].Tag : 0;
    }

    public sealed class SectorTagIterator
    {
        readonly VmPTagManager _mgr;
        readonly int _searchTag;
        int _start;
        readonly int _initialStart;

        public SectorTagIterator(VmPTagManager mgr, int searchTag, int hashStart, int sectorCount)
        {
            _mgr = mgr;
            _searchTag = searchTag;
            _initialStart = hashStart;
            _start = searchTag == int.MinValue ? hashStart : hashStart;
            _sectorCount = sectorCount;
        }

        readonly int _sectorCount;

        public int Next()
        {
            if (_searchTag == int.MinValue)
            {
                var ret = _start;
                _start = -1;
                return ret;
            }
            if (_searchTag != 0)
            {
                while (_start >= 0 && _mgr._allTags[_start].Tag != _searchTag)
                    _start = _mgr._allTags[_start].NextTag;
                if (_start == -1)
                    return -1;
                var ret = _mgr._allTags[_start].Target;
                _start = _mgr._allTags[_start].NextTag;
                return ret;
            }
            while (_start < _sectorCount && _mgr.SectorHasTags(_start))
                _start++;
            if (_start == _sectorCount)
                return -1;
            var r = _start;
            _start++;
            return r;
        }

        public int NextCompat(bool compat, int compatStart)
        {
            if (!compat)
                return Next();
            for (var i = compatStart + 1; i < _sectorCount; i++)
            {
                if (_mgr.SectorHasTag(i, _searchTag))
                    return i;
            }
            return -1;
        }
    }

    public SectorTagIterator CreateSectorTagIterator(int searchTag)
    {
        var start = searchTag == int.MinValue
            ? 0
            : searchTag == 0
                ? 0
                : _tagHashFirst[(int)(((uint)searchTag) % TagHashSize)];
        return new SectorTagIterator(this, searchTag, start, _sectorCount);
    }

    public sealed class LineIdIterator
    {
        readonly IReadOnlyList<TagItem> _allIds;
        readonly int _searchTag;
        int _start;

        public LineIdIterator(int searchTag, IReadOnlyList<int> idHashFirst, IReadOnlyList<TagItem> allIds)
        {
            _searchTag = searchTag;
            _allIds = allIds;
            _start = idHashFirst[(int)(((uint)searchTag) % TagHashSize)];
        }

        public int Next()
        {
            while (_start >= 0 && _allIds[_start].Tag != _searchTag)
                _start = _allIds[_start].NextTag;
            if (_start < 0)
                return -1;
            var ret = _allIds[_start].Target;
            _start = _allIds[_start].NextTag;
            return ret;
        }
    }
}
