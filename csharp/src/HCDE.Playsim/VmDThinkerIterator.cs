namespace HCDE.Playsim;

public sealed class VmDThinkerIterator
{
    public const int MaxStatNum = VmDThinkerListOps.MaxStatNum;
    public const int StatFirstThinking = ThinkerStat.FirstThinking;
    public const int OfSentinel = VmDThinkerListOps.OfSentinel;

    private readonly VmDThinkerListOps.CollectionModel _pool;
    private readonly object _parentType;
    private int _stat;
    private bool _searchStats;
    private VmDThinkerListOps.ThinkerNodeModel? _currThinker;
    private bool _searchingFresh;
    private readonly Func<VmDThinkerListOps.ThinkerNodeModel, object, bool, bool> _typeMatch;

    public VmDThinkerIterator(
        VmDThinkerListOps.CollectionModel pool,
        object parentType,
        int statnum,
        bool clientSide,
        Func<VmDThinkerListOps.ThinkerNodeModel, object, bool, bool>? typeMatch = null)
    {
        _ = clientSide;
        _pool = pool;
        _parentType = parentType;
        if ((uint)statnum > MaxStatNum)
        {
            _stat = StatFirstThinking;
            _searchStats = true;
        }
        else
        {
            _stat = statnum;
            _searchStats = false;
        }

        _typeMatch = typeMatch ?? DefaultTypeMatch;
        Reinit();
    }

    public VmDThinkerIterator(
        VmDThinkerListOps.CollectionModel pool,
        object parentType,
        int statnum,
        VmDThinkerListOps.ThinkerNodeModel? prev,
        bool clientSide,
        Func<VmDThinkerListOps.ThinkerNodeModel, object, bool, bool>? typeMatch = null)
        : this(pool, parentType, statnum, clientSide, typeMatch)
    {
        if (prev == null || (prev.NextThinker?.ObjectFlags & OfSentinel) != 0)
            Reinit();
        else
        {
            _currThinker = prev.NextThinker;
            _searchingFresh = false;
        }
    }

    public void Reinit()
    {
        _currThinker = VmDThinkerListOps.GetHead(_pool.Thinkers[_stat]);
        _searchingFresh = false;
    }

    public VmDThinkerListOps.ThinkerNodeModel? Next(bool exact = false)
    {
        if (_parentType == null!)
            return null;

        do
        {
            do
            {
                if (_currThinker != null)
                {
                    while ((_currThinker.ObjectFlags & OfSentinel) == 0)
                    {
                        var thinker = _currThinker;
                        _currThinker = thinker.NextThinker;
                        if (_typeMatch(thinker, _parentType, exact))
                            return thinker;
                        if (_currThinker == null)
                            break;
                    }
                }

                _searchingFresh = !_searchingFresh;
                if (_searchingFresh)
                    _currThinker = VmDThinkerListOps.GetHead(_pool.FreshThinkers[_stat]);
            } while (_searchingFresh);

            if (_searchStats)
            {
                _stat++;
                if (_stat > MaxStatNum)
                    _stat = StatFirstThinking;
            }

            _currThinker = VmDThinkerListOps.GetHead(_pool.Thinkers[_stat]);
            _searchingFresh = false;
        } while (_searchStats && _stat != StatFirstThinking);

        return null;
    }

    private static bool DefaultTypeMatch(VmDThinkerListOps.ThinkerNodeModel thinker, object parentType, bool exact)
    {
        if (thinker.TypeName == null)
            return false;
        var name = thinker.TypeName();
        return exact
            ? ReferenceEquals(name, parentType)
            : string.Equals(name, parentType.ToString(), StringComparison.Ordinal);
    }
}
