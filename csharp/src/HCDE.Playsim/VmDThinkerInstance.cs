namespace HCDE.Playsim;

public static class VmDThinkerInstance
{
    public const int MaxStatNum = VmDThinkerListOps.MaxStatNum;
    public const int StatStatic = VmDThinkerCollectionEarly.StatStatic;
    public const int StatTravelling = VmDThinkerCollectionEarly.StatTravelling;
    public const int OfSpawned = 1 << 3;
    public const int OfTravelling = VmDThinkerListDestroy.OfTravelling;
    public const int OfEuthanizeMe = VmDThinkerListOps.OfEuthanizeMe;
    public const int OfSentinel = VmDThinkerListOps.OfSentinel;

    public sealed class ChangeStatContext
    {
        public bool Travelling { get; init; }
        public Action<string>? Printf { get; init; }
        public Action<VmDThinkerListOps.ThinkerNodeModel, int>? Link { get; init; }
        public Func<VmDThinkerListOps.ThinkerNodeModel, bool>? IsClientSide { get; init; }
        public Action<VmDThinkerListOps.ThinkerNodeModel, int>? LinkClient { get; init; }
        public Action<VmDThinkerListOps.ThinkerNodeModel, int>? LinkServer { get; init; }
    }

    public sealed class TravellingContext
    {
        public bool Travelling { get; init; }
        public Action<string>? Printf { get; init; }
        public Action<VmDThinkerListOps.ThinkerNodeModel>? AddToTravellingList { get; init; }
        public Func<VmDThinkerListOps.ThinkerNodeModel, bool>? HasUnmorphedFlag { get; init; }
    }

    public static void OnDestroy(VmDThinkerListOps.ThinkerNodeModel self)
    {
        var linked = self.NextThinker != null || self.PrevThinker != null;
        if (linked && self.NextThinker != null)
            Remove(self, null);
        self.StatNum = -1;
    }

    public static void Serialize(VmDThinkerListOps.ThinkerNodeModel self, Action<string, object?> write)
    {
        write("level", self.Level);
        write("statnum", self.StatNum);
    }

    public static void Remove(VmDThinkerListOps.ThinkerNodeModel self, VmDThinkerListOps.ThinkerNodeModel? nextToThink)
    {
        if (ReferenceEquals(nextToThink, self))
            nextToThink = self.NextThinker;

        var prev = self.PrevThinker;
        var next = self.NextThinker;
        if (prev == null && next == null)
            return;

        prev!.NextThinker = next;
        next!.PrevThinker = prev;
        self.NextThinker = null;
        self.PrevThinker = null;
    }

    public static void PostBeginPlay(VmDThinkerListOps.ThinkerNodeModel _) { }

    public static void CallPostBeginPlay(VmDThinkerListOps.ThinkerNodeModel self)
    {
        self.ObjectFlags |= OfSpawned;
        if (self.HasVirtualPostBeginPlay)
            self.VirtualPostBeginPlay?.Invoke();
        else
            PostBeginPlay(self);
    }

    public static void Tick(VmDThinkerListOps.ThinkerNodeModel _) { }

    public static void CallTick(VmDThinkerListOps.ThinkerNodeModel self)
    {
        if (self.HasVirtualTick)
            self.VirtualTick?.Invoke();
        else
            Tick(self);
    }

    public static nuint PropagateMark(
        VmDThinkerListOps.ThinkerNodeModel self,
        Action<VmDThinkerListOps.ThinkerNodeModel?> mark,
        Func<nuint> superMark)
    {
        mark(self.NextThinker);
        mark(self.PrevThinker);
        return superMark();
    }

    public static void ChangeStatNum(VmDThinkerListOps.ThinkerNodeModel self, int statnum, ChangeStatContext ctx)
    {
        if ((uint)statnum > MaxStatNum)
            statnum = MaxStatNum;
        Remove(self, null);
        if (ctx.IsClientSide?.Invoke(self) == true)
            ctx.LinkClient?.Invoke(self, statnum);
        else
            ctx.LinkServer?.Invoke(self, statnum);
        if (statnum != StatTravelling)
            self.StatNum = statnum;
    }

    public static void ChangeStatNumAction(VmDThinkerListOps.ThinkerNodeModel self, int statnum, ChangeStatContext ctx)
    {
        if ((self.ObjectFlags & OfTravelling) != 0)
        {
            ctx.Printf?.Invoke("Thinkers cannot be moved while travelling");
            return;
        }

        if (statnum == StatStatic && self.IsKindOf?.Invoke("Actor") == true)
        {
            ctx.Printf?.Invoke("Actors cannot be added to STAT_STATIC");
            return;
        }

        if (statnum == StatTravelling)
        {
            ctx.Printf?.Invoke("Thinkers cannot be added to STAT_TRAVELLING manually");
            return;
        }

        ChangeStatNum(self, statnum, ctx);
    }

    public static void AddToTravellingList(VmDThinkerListOps.ThinkerNodeModel self, TravellingContext ctx)
    {
        if (!ctx.Travelling)
        {
            ctx.Printf?.Invoke("Thinkers can only be set to travel on level change");
            return;
        }

        if (self.IsKindOf?.Invoke("Inventory") == true && self.PointerVar?.Invoke("Owner") != null)
        {
            ctx.Printf?.Invoke("Owned Inventory items must travel with their owner on level change");
            return;
        }

        if (self.IsKindOf?.Invoke("Bot") == true)
        {
            ctx.Printf?.Invoke("Bot Thinkers must travel with their owner on level change");
            return;
        }

        if (self.IsKindOf?.Invoke("Actor") == true && ctx.HasUnmorphedFlag?.Invoke(self) == true)
        {
            ctx.Printf?.Invoke("Unmorphed Actors must travel with their owner on level change");
            return;
        }

        var nativeType = self.NativeClassTypeName?.Invoke() ?? "Thinker";
        if (nativeType != "Thinker" && nativeType != "Actor")
        {
            ctx.Printf?.Invoke("Native thinkers cannot travel");
            return;
        }

        ctx.AddToTravellingList?.Invoke(self);
    }

    public static (int Mode, int Limit, bool Parsed) ParseProfileThinkersCommand(IReadOnlyList<string> argv)
    {
        if (argv.Count == 2 || argv.Count == 3)
        {
            var str = argv[1];
            var ascend = true;
            if (str.StartsWith('+'))
                str = str[1..];
            else if (str.StartsWith('-'))
            {
                ascend = false;
                str = str[1..];
            }

            var mode = 0;
            if (str.Length > 0)
            {
                mode = str[0] switch
                {
                    't' => ascend ? 7 : 8,
                    'a' => ascend ? 5 : 6,
                    '#' => ascend ? 3 : 4,
                    'c' => ascend ? 1 : 2,
                    _ => int.TryParse(str, out var parsed) ? parsed : 0
                };
            }

            var limit = argv.Count == 3 && int.TryParse(argv[2], out var lim) ? lim : 0;
            return (mode, limit, true);
        }

        return (0, 0, false);
    }

    public static string ProfileThinkersUsage() =>
        "Usage: profilethinkers [+|-][t|a|#|c] [limit]";
}
