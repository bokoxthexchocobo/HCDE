namespace HCDE.Playsim;

/// <summary>
/// Thinker list link, destroy, save, traveller removal, OnLoad, tick, profile,
/// and the empty-link destructor from <c>dthinker.cpp</c> (444-808). Chunk 1330.
/// <c>SerializeThinkers</c> ends at line 443. <c>OnDestroy</c> at line 809 is
/// already credited. <c>Remove</c>, <c>CallTick</c>, and <c>CallPostBeginPlay</c>
/// stay callees. The fresh-list error and <c>DestroyThinkers failed</c> stay here.
/// </summary>
public sealed class VmThinkerList1330
{
    public const int MaxStatNum = 127;
    public const int OfEuthanizeMe = 1 << 5;
    public const int OfYesReallyDelete = 1 << 7;
    public const int OfJustSpawned = 1 << 8;
    public const int OfSentinel = 1 << 10;
    public const int OfSpawned = 1 << 12;
    public const int OfTravelling = 1 << 16;
    public const int PrintBold = 200;
    public const int PrintNoNotify = 1024;
    public const int PrintBoldNoNotify = PrintBold | PrintNoNotify;
    public const string TextRed = "\u001cG";
    public const string DestroyFailed = "DestroyThinkers failed";
    public const string FreshAlreadyTicked = "There is a thinker in the fresh list that has already ticked.\n";
    public const string VmDestroyHeader = "VM exception in DestroyThinkers:\n";

    public int ThinkCount;
    public Thinker? NextToThink;
    public readonly List<string> Barriers = new();
    public readonly List<string> Marks = new();
    public readonly List<PrintLine> Prints = new();
    public readonly List<Thinker> Serialized = new();
    public readonly List<int> DestroyOrder = new();
    public readonly Dictionary<string, ProfileInfo> Profiles = new(StringComparer.Ordinal);
    public readonly ThinkerList[] Thinkers = new ThinkerList[MaxStatNum + 2];
    public readonly ThinkerList[] Fresh = new ThinkerList[MaxStatNum + 1];

    int _nextId = 1000;

    public readonly record struct PrintLine(int Flags, string Text);

    public VmThinkerList1330()
    {
        for (int i = 0; i < Thinkers.Length; i++)
            Thinkers[i] = new ThinkerList(this);
        for (int i = 0; i < Fresh.Length; i++)
            Fresh[i] = new ThinkerList(this);
    }

    public Thinker? FirstThinker(int statnum)
    {
        if ((uint)statnum > MaxStatNum)
            statnum = MaxStatNum;
        Thinker? node = Thinkers[statnum].GetHead();
        if (node == null)
            node = Fresh[statnum].GetHead();
        return node;
    }

    public void MarkRoots()
    {
        for (int i = 0; i <= MaxStatNum; i++)
        {
            Mark(Thinkers[i].Sentinel, "live:" + i);
            Mark(Fresh[i].Sentinel, "fresh:" + i);
        }
        Mark(Thinkers[MaxStatNum + 1].Sentinel, "live:" + (MaxStatNum + 1));
    }

    public int AllocId()
    {
        int id = _nextId;
        _nextId++;
        return id;
    }

    public void Barrier(Thinker from, Thinker to)
    {
        Barriers.Add(from.Id + ">" + to.Id);
    }

    public void BarrierOne(Thinker node)
    {
        Barriers.Add("one:" + node.Id);
    }

    void Mark(Thinker? node, string slot)
    {
        Marks.Add(slot + ":" + (node == null ? 0 : node.Id));
    }

    public sealed class ProfileInfo
    {
        public int Calls;
        public int Clocks;
        public int Unclocks;
        public double Time;
    }

    public sealed class VmAbort : Exception
    {
        public VmAbort(string? detail, string trace)
        {
            Detail = detail;
            Trace = trace;
        }

        public string? Detail { get; }
        public string Trace { get; }
    }

    public sealed class RecoverableError : Exception
    {
        public RecoverableError(string message)
            : base(message)
        {
            Reported = message;
        }

        public string Reported { get; }
    }

    public sealed class Thinker
    {
        public int Id;
        public int Flags;
        public string TypeName = "";
        public double TickMillis;
        public bool HasOnLoad;
        public bool Destroyed;
        public bool Deleted;
        public int PostBegins;
        public int TickCalls;
        public int LoadCalls;
        public Thinker? Next;
        public Thinker? Prev;
        public VmThinkerList1330? World;
        public Action? OnDestroy;
        public Action? OnPostBegin;
        public Action? OnTick;
        public Action? OnLoad;

        public void Remove()
        {
            if (World == null)
                throw new InvalidOperationException("thinker has no world");
            if (this == World.NextToThink)
                World.NextToThink = Next;
            Thinker? prev = Prev;
            Thinker? next = Next;
            if (prev == null && next == null)
                return;
            if ((Flags & OfSentinel) == 0 && (prev == this || next == this))
                throw new InvalidOperationException("thinker points at itself");
            if (prev == null || next == null || prev.Next != this || next.Prev != this)
                throw new InvalidOperationException("thinker links are broken");
            prev.Next = next;
            next.Prev = prev;
            World.Barrier(prev, next);
            World.Barrier(next, prev);
            Next = null;
            Prev = null;
        }

        public void Destroy()
        {
            if (World != null)
                World.DestroyOrder.Add(Id);
            if (Next != null || Prev != null)
                Remove();
            if (OnDestroy != null)
                OnDestroy();
            Destroyed = true;
        }

        public void RunDestructor()
        {
            if (Next != null || Prev != null)
                throw new InvalidOperationException("thinker links are not null");
        }
    }

    public sealed class ThinkerList
    {
        readonly VmThinkerList1330 _host;
        public Thinker? Sentinel;

        public ThinkerList(VmThinkerList1330 host)
        {
            _host = host;
        }

        public void AddTail(Thinker thinker)
        {
            if (thinker.Prev != null || thinker.Next != null)
                throw new InvalidOperationException("thinker links are not null");
            if ((thinker.Flags & OfEuthanizeMe) != 0)
                throw new InvalidOperationException("thinker is scheduled for destruction");
            if (Sentinel == null)
            {
                var created = new Thinker
                {
                    Id = _host.AllocId(),
                    World = _host,
                    Flags = OfSentinel,
                };
                created.Next = created;
                created.Prev = created;
                Sentinel = created;
                _host.BarrierOne(created);
            }
            Thinker tail = Sentinel.Prev!;
            if (tail.Next != Sentinel)
                throw new InvalidOperationException("tail does not point at the sentinel");
            thinker.World = _host;
            thinker.Prev = tail;
            thinker.Next = Sentinel;
            tail.Next = thinker;
            Sentinel.Prev = thinker;
            _host.Barrier(thinker, tail);
            _host.Barrier(thinker, Sentinel);
            _host.Barrier(tail, thinker);
            _host.Barrier(Sentinel, thinker);
        }

        public Thinker? GetHead()
        {
            if (Sentinel == null || Sentinel.Next == Sentinel)
                return null;
            if (Sentinel.Next == null || Sentinel.Next.Prev != Sentinel)
                throw new InvalidOperationException("head does not point back at the sentinel");
            return Sentinel.Next;
        }

        public Thinker? GetTail()
        {
            if (Sentinel == null || Sentinel.Prev == Sentinel)
                return null;
            return Sentinel.Prev;
        }

        public bool IsEmpty()
        {
            return Sentinel == null || Sentinel.Next == null;
        }

        public void DestroyThinkers()
        {
            if (DoDestroyThinkers())
                throw new InvalidOperationException(DestroyFailed);
        }

        public bool DoDestroyThinkers()
        {
            bool error = false;
            if (Sentinel != null)
            {
                var toDelete = new List<Thinker>();
                Thinker? node = Sentinel.Next;
                while (node != Sentinel)
                {
                    if (node == null)
                        throw new InvalidOperationException("node is null");
                    Thinker? next = node.Next;
                    toDelete.Add(node);
                    node.Next = null;
                    node.Prev = null;
                    node = next;
                }
                Sentinel.Next = null;
                Sentinel.Prev = null;
                Sentinel.Destroy();
                Sentinel = null;
                for (int i = 0; i < toDelete.Count; i++)
                {
                    Thinker doomed = toDelete[i];
                    try
                    {
                        doomed.Destroy();
                    }
                    catch (VmAbort exception)
                    {
                        _host.Prints.Add(new PrintLine(0, VmDestroyHeader));
                        if (exception.Detail != null)
                            _host.Prints.Add(new PrintLine(PrintBoldNoNotify, TextRed + exception.Detail + "\n"));
                        _host.Prints.Add(new PrintLine(PrintBoldNoNotify, exception.Trace));
                        doomed.Flags |= OfYesReallyDelete;
                        doomed.Deleted = true;
                        error = true;
                    }
                    catch (RecoverableError exception)
                    {
                        _host.Prints.Add(new PrintLine(
                            PrintBoldNoNotify,
                            "Error in DestroyThinkers: " + exception.Reported + "\n"));
                        doomed.Flags |= OfYesReallyDelete;
                        doomed.Deleted = true;
                        error = true;
                    }
                }
            }
            return error;
        }

        public void SaveList()
        {
            Thinker? node = GetHead();
            if (node == null)
                return;
            while ((node.Flags & OfSentinel) == 0)
            {
                Thinker? next = node.Next;
                if (next == null || (next.Flags & OfEuthanizeMe) != 0)
                    throw new InvalidOperationException("next thinker is null or scheduled for destruction");
                _host.Serialized.Add(node);
                node = next;
            }
        }

        public void RemoveTravellers(bool saveGame)
        {
            Thinker? node = GetHead();
            if (node == null)
                return;
            while (node != Sentinel)
            {
                if (node == null)
                    throw new InvalidOperationException("node is null");
                _host.NextToThink = node.Next;
                if ((node.Flags & OfTravelling) != 0 && (node.Flags & OfEuthanizeMe) == 0)
                {
                    if (saveGame)
                        node.Flags &= ~OfTravelling;
                    else
                        node.Destroy();
                }
                node = _host.NextToThink;
            }
        }

        public void OnLoad()
        {
            Thinker? node = GetHead();
            if (node == null)
                return;
            while (node != Sentinel)
            {
                if (node == null)
                    throw new InvalidOperationException("node is null");
                _host.NextToThink = node.Next;
                if ((node.Flags & OfEuthanizeMe) == 0 && node.HasOnLoad)
                {
                    node.LoadCalls++;
                    if (node.OnLoad != null)
                        node.OnLoad();
                }
                node = _host.NextToThink;
            }
        }

        public int TickThinkers(ThinkerList? dest)
        {
            return Visit(dest, profile: false);
        }

        public int ProfileThinkers(ThinkerList? dest)
        {
            return Visit(dest, profile: true);
        }

        int Visit(ThinkerList? dest, bool profile)
        {
            int count = 0;
            Thinker? node = GetHead();
            if (node == null)
                return 0;
            while (node != Sentinel)
            {
                if (node == null)
                    throw new InvalidOperationException("node is null");
                count++;
                _host.NextToThink = node.Next;
                if ((node.Flags & OfJustSpawned) != 0)
                {
                    if (dest != null)
                    {
                        node.Remove();
                        dest.AddTail(node);
                    }
                    node.Flags |= OfSpawned;
                    node.PostBegins++;
                    if (node.OnPostBegin != null)
                        node.OnPostBegin();
                }
                else if (dest != null)
                {
                    throw new InvalidOperationException(FreshAlreadyTicked);
                }
                if ((node.Flags & OfEuthanizeMe) == 0)
                {
                    _host.ThinkCount++;
                    if (profile)
                        ProfileOne(node);
                    else
                        CallTick(node);
                    node.Flags &= ~OfJustSpawned;
                }
                node = _host.NextToThink;
            }
            return count;
        }

        void ProfileOne(Thinker node)
        {
            if (!_host.Profiles.TryGetValue(node.TypeName, out ProfileInfo? info))
            {
                info = new ProfileInfo();
                _host.Profiles.Add(node.TypeName, info);
            }
            info.Calls++;
            info.Clocks++;
            CallTick(node);
            info.Unclocks++;
            info.Time += node.TickMillis;
        }

        static void CallTick(Thinker node)
        {
            node.TickCalls++;
            if (node.OnTick != null)
                node.OnTick();
        }
    }
}
