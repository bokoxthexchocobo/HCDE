namespace HCDE.Playsim;

public static class ThinkerStat
{
    public const int MaxStatNum = 127;
    public const int Info = 0;
    public const int FirstThinking = 32;
    public const int Player = 33;
    public const int Default = 100;
}

public class Thinker
{
    internal Thinker? Next;
    internal Thinker? Previous;
    internal bool InList;

    public int StatNum { get; internal set; } = ThinkerStat.Default;
    public bool JustSpawned { get; internal set; } = true;
    public bool Destroyed { get; private set; }
    public int PostBeginCount { get; private set; }
    public int TickCount { get; private set; }

    public void Destroy() => Destroyed = true;

    public virtual void PostBeginPlay() => PostBeginCount++;

    public virtual void CallPostBeginPlay() => PostBeginPlay();

    public virtual void Tick() => TickCount++;
}

/// <summary>
/// Stat-ordered thinker lists. Fresh thinkers tick after the survivors from the previous tic,
/// matching <c>FThinkerCollection::RunThinkers</c> and <c>FThinkerList::TickThinkers</c>.
/// </summary>
public sealed class ThinkerCollection
{
    private readonly Thinker?[] _thinkers = new Thinker?[ThinkerStat.MaxStatNum + 1];
    private readonly Thinker?[] _fresh = new Thinker?[ThinkerStat.MaxStatNum + 1];
    private readonly GameTicClock _clock;
    private readonly HashSet<Thinker> _ticked = new();

    public ThinkerCollection(GameTicClock? clock = null) => _clock = clock ?? new GameTicClock();

    public GameTicClock Clock => _clock;

    public void Add(Thinker thinker, int statNum = ThinkerStat.Default)
    {
        if (statNum is < 0 or > ThinkerStat.MaxStatNum)
            throw new ArgumentOutOfRangeException(nameof(statNum));
        if (thinker.InList)
            Remove(thinker);

        thinker.StatNum = statNum;
        var fresh = thinker.JustSpawned && statNum >= ThinkerStat.FirstThinking;
        AddTail(fresh ? _fresh : _thinkers, thinker);
    }

    public void ChangeStatNum(Thinker thinker, int statNum)
    {
        var justSpawned = thinker.JustSpawned;
        Remove(thinker);
        thinker.JustSpawned = justSpawned;
        Add(thinker, statNum);
    }

    public void Run()
    {
        _ticked.Clear();
        for (var stat = ThinkerStat.FirstThinking; stat <= ThinkerStat.MaxStatNum; stat++)
            TickList(_thinkers, stat, destination: null);

        int count;
        do
        {
            count = 0;
            for (var stat = ThinkerStat.FirstThinking; stat <= ThinkerStat.MaxStatNum; stat++)
                count += TickList(_fresh, stat, _thinkers);
        }
        while (count != 0);

        _clock.Advance();
    }

    public IReadOnlyList<Thinker> ThinkersIn(int statNum)
    {
        var list = new List<Thinker>();
        for (var node = _thinkers[statNum]; node != null; node = node.Next)
            list.Add(node);
        return list;
    }

    private int TickList(Thinker?[] table, int stat, Thinker?[]? destination)
    {
        var count = 0;
        var pending = new List<Thinker>();
        for (var item = table[stat]; item != null; item = item.Next) pending.Add(item);
        foreach (var node in pending)
        {
            count++;
            if (!node.InList || node.StatNum != stat) continue;
            if (!node.Destroyed && node.JustSpawned)
            {
                if (destination != null)
                {
                    Unlink(table, node);
                    AddTail(destination, node);
                }

                node.JustSpawned = false;
                node.CallPostBeginPlay();
            }

            if (!node.Destroyed && _ticked.Add(node))
            {
                node.Tick();
                node.JustSpawned = false;
            }
            if (node.Destroyed) Remove(node);

        }

        return count;
    }

    private static void AddTail(Thinker?[] table, Thinker thinker)
    {
        var stat = thinker.StatNum;
        thinker.Next = null;
        var head = table[stat];
        if (head == null)
        {
            thinker.Previous = null;
            table[stat] = thinker;
        }
        else
        {
            var tail = head;
            while (tail.Next != null)
                tail = tail.Next;
            tail.Next = thinker;
            thinker.Previous = tail;
        }

        thinker.InList = true;
    }

    internal void Remove(Thinker thinker)
    {
        if (!thinker.InList)
            return;
        Unlink(ListFor(thinker), thinker);
    }

    private Thinker?[] ListFor(Thinker thinker)
    {
        var node = _fresh[thinker.StatNum];
        while (node != null)
        {
            if (ReferenceEquals(node, thinker))
                return _fresh;
            node = node.Next;
        }

        return _thinkers;
    }

    private static void Unlink(Thinker?[] table, Thinker thinker)
    {
        if (thinker.Previous != null)
            thinker.Previous.Next = thinker.Next;
        else if (ReferenceEquals(table[thinker.StatNum], thinker))
            table[thinker.StatNum] = thinker.Next;

        if (thinker.Next != null)
            thinker.Next.Previous = thinker.Previous;

        thinker.Next = null;
        thinker.Previous = null;
        thinker.InList = false;
    }
}
