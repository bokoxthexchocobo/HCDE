namespace HCDE.Playsim;

/// <summary>Early <c>FThinkerCollection</c> helpers (<c>dthinker.cpp</c> 71–366, before <c>SerializeThinkers</c>).</summary>
public static class VmDThinkerCollectionEarly
{
    public const int StatTravelling = 4;
    public const int StatStatic = 5;
    public const int StatFirstThinking = ThinkerStat.FirstThinking;
    public const int MaxStatNum = ThinkerStat.MaxStatNum;

    public const int OfJustSpawned = 1 << 0;

    public sealed class ThinkerLinkModel
    {
        public int ObjectFlags { get; set; }
        public int StatNum { get; set; }
    }

    public sealed class ThinkerListOps
    {
        public Action<ThinkerLinkModel>? AddTail { get; set; }
        public Func<bool>? DoDestroyThinkers { get; set; }
        public Action<bool>? RemoveTravellers { get; set; }
        public Action? OnLoad { get; set; }
        public Func<ThinkerListOps?, int>? TickThinkers { get; set; }
        public Func<ThinkerListOps?, int>? ProfileThinkers { get; set; }
    }

    public sealed class CollectionModel
    {
        public ThinkerListOps[] Thinkers { get; init; } = Array.Empty<ThinkerListOps>();
        public ThinkerListOps[] FreshThinkers { get; init; } = Array.Empty<ThinkerListOps>();
        public int ThinkCount { get; set; }
    }

    public sealed class ProfileEntry
    {
        public string ClassName { get; init; } = "";
        public int NumCalls { get; set; }
        public double TimeMs { get; set; }
    }

    public sealed class RunThinkersContext
    {
        public CollectionModel Collection { get; init; } = null!;
        public int ProfileThinkers { get; set; }
        public int ProfileLimit { get; set; }
        public Dictionary<string, ProfileEntry>? Profiles { get; init; }
        public Action? ResetThinkCycles { get; init; }
        public Action? ClockThinkCycles { get; init; }
        public Action? UnclockThinkCycles { get; init; }
        public Action<string>? PrintLine { get; init; }
    }

    public sealed class DestroyAllContext
    {
        public CollectionModel Collection { get; init; } = null!;
        public bool FullGc { get; init; }
        public Action? FullGcAction { get; init; }
        public Action? ClearGlobalVmStack { get; init; }
        public Action<string>? FatalError { get; init; }
    }

    public sealed class CleanUpTravellersContext
    {
        public CollectionModel Collection { get; init; } = null!;
        public bool SaveGame { get; init; }
        public Action<int>? DestroyThinkersInList { get; init; }
    }

    public static void Link(CollectionModel collection, ThinkerLinkModel thinker, int statnum)
    {
        ThinkerListOps list;
        if ((thinker.ObjectFlags & OfJustSpawned) != 0 && statnum >= StatFirstThinking)
            list = collection.FreshThinkers[statnum];
        else
        {
            if (statnum != StatTravelling)
                thinker.ObjectFlags &= ~OfJustSpawned;
            list = collection.Thinkers[statnum];
        }

        list.AddTail?.Invoke(thinker);
        thinker.StatNum = statnum;
    }

    public static void RunThinkers(RunThinkersContext ctx)
    {
        ctx.Collection.ThinkCount = 0;
        ctx.ResetThinkCycles?.Invoke();
        ctx.ClockThinkCycles?.Invoke();

        if (ctx.ProfileThinkers == 0)
            RunThinkersNonProfile(ctx.Collection);
        else
            RunThinkersProfile(ctx);

        ctx.UnclockThinkCycles?.Invoke();
    }

    public static void DestroyAllThinkers(DestroyAllContext ctx)
    {
        var error = false;
        for (var i = 0; i <= MaxStatNum; i++)
        {
            if (i != StatTravelling && i != StatStatic)
            {
                error |= ctx.Collection.Thinkers[i].DoDestroyThinkers?.Invoke() == true;
                error |= ctx.Collection.FreshThinkers[i].DoDestroyThinkers?.Invoke() == true;
            }
        }

        error |= ctx.Collection.Thinkers[MaxStatNum + 1].DoDestroyThinkers?.Invoke() == true;
        if (ctx.FullGc)
            ctx.FullGcAction?.Invoke();

        if (error)
        {
            ctx.ClearGlobalVmStack?.Invoke();
            var message = "DestroyAllThinkers failed";
            if (ctx.FullGc)
                ctx.FatalError?.Invoke(message);
            else
                ctx.FatalError?.Invoke(message);
        }
    }

    public static void CleanUpTravellers(CleanUpTravellersContext ctx)
    {
        ctx.DestroyThinkersInList?.Invoke(StatTravelling);
        for (var i = 0; i <= MaxStatNum; i++)
        {
            ctx.Collection.FreshThinkers[i].RemoveTravellers?.Invoke(ctx.SaveGame);
            ctx.Collection.Thinkers[i].RemoveTravellers?.Invoke(ctx.SaveGame);
        }
    }

    public static void OnLoad(CollectionModel collection)
    {
        foreach (var list in collection.FreshThinkers)
            list.OnLoad?.Invoke();
        foreach (var list in collection.Thinkers)
            list.OnLoad?.Invoke();
    }

    private static void RunThinkersNonProfile(CollectionModel collection)
    {
        for (var i = StatFirstThinking; i <= MaxStatNum; i++)
            collection.Thinkers[i].TickThinkers?.Invoke(null);

        int count;
        do
        {
            count = 0;
            for (var i = StatFirstThinking; i <= MaxStatNum; i++)
                count += collection.FreshThinkers[i].TickThinkers?.Invoke(collection.Thinkers[i]) ?? 0;
        } while (count != 0);
    }

    private static void RunThinkersProfile(RunThinkersContext ctx)
    {
        ctx.Profiles?.Clear();
        var collection = ctx.Collection;

        for (var i = StatFirstThinking; i <= MaxStatNum; i++)
            collection.Thinkers[i].ProfileThinkers?.Invoke(null);

        int count;
        do
        {
            count = 0;
            for (var i = StatFirstThinking; i <= MaxStatNum; i++)
                count += collection.FreshThinkers[i].ProfileThinkers?.Invoke(collection.Thinkers[i]) ?? 0;
        } while (count != 0);

        var sorted = ctx.Profiles?.Values
            .OrderBy(entry => entry, Comparer<ProfileEntry>.Create((left, right) =>
                CompareProfileEntries(left, right, ctx.ProfileThinkers)))
            .ToList() ?? [];

        ctx.PrintLine?.Invoke("Total, ms   Averg, ms   Calls   Actor class");
        ctx.PrintLine?.Invoke("----------  ----------  ------  --------------------");

        var limit = ctx.ProfileLimit > 0 ? ctx.ProfileLimit : int.MaxValue;
        var printCount = Math.Min(limit, sorted.Count);
        for (var i = 0; i < printCount; i++)
        {
            var info = sorted[i];
            var avg = info.NumCalls != 0 ? info.TimeMs / info.NumCalls : 0;
            ctx.PrintLine?.Invoke($"{info.TimeMs,10:F6}  {avg,10:F6}  {info.NumCalls,6}  {info.ClassName}");
        }

        ctx.ProfileThinkers = 0;
    }

    internal static int CompareProfileEntries(ProfileEntry left, ProfileEntry right, int profileThinkers) =>
        profileThinkers switch
        {
            1 => string.Compare(left.ClassName, right.ClassName, StringComparison.OrdinalIgnoreCase),
            2 => string.Compare(right.ClassName, left.ClassName, StringComparison.OrdinalIgnoreCase),
            3 => left.NumCalls.CompareTo(right.NumCalls),
            4 => right.NumCalls.CompareTo(left.NumCalls),
            5 => CompareAverage(left, right),
            6 => CompareAverage(right, left),
            7 => left.TimeMs.CompareTo(right.TimeMs),
            _ => right.TimeMs.CompareTo(left.TimeMs),
        };

    private static int CompareAverage(ProfileEntry left, ProfileEntry right)
    {
        var leftAvg = left.NumCalls != 0 ? left.TimeMs / left.NumCalls : 0;
        var rightAvg = right.NumCalls != 0 ? right.TimeMs / right.NumCalls : 0;
        return leftAvg.CompareTo(rightAvg);
    }
}
