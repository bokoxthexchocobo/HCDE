using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Thinker link, the tick loop, client lights, destruction, and OnLoad from
/// <c>dthinker.cpp</c> (1-373). Chunk 1328. <c>SerializeThinkers</c> at line 374
/// is already credited. List mutators, <c>WorldPaused</c>, dynamic-light setup,
/// <c>P_RunEffect</c>, the GC, and the two destroy errors stay callees.
/// </summary>
public sealed class VmThinkerRun1328
{
    public const int MaxStatNum = 127;
    public const int StatTravelling = 4;
    public const int StatStatic = 5;
    public const int StatFirstThinking = 32;
    public const int FreshListCount = MaxStatNum + 1;
    public const int ThinkerListCount = MaxStatNum + 2;
    public const int OfJustSpawned = 1 << 8;
    public const int Mf8RecreateLights = 0x00000100;
    public const int Level3LightCreated = 0x00080000;
    public const string TextYellow = "\u001cK";
    public const string TextWhite = "\u001cJ";
    public const string DestroyFailed = "DestroyAllThinkers failed";

    public int ThinkCount;
    public int BotWtg;
    public int ProfileThinkers;
    public uint ProfileLimit;
    public bool GlLights = true;
    public bool DynLights = true;
    public int RenderMode = 4;

    public readonly List<string> Cycles = new();
    public readonly List<string> Steps = new();
    public readonly List<string> Prints = new();
    public readonly List<TickCall> Ticks = new();
    public readonly List<Actor> Recreated = new();
    public readonly Dictionary<string, ProfileInfo> Profiles = new(StringComparer.Ordinal);

    public Func<bool, int, bool, int>? OnTick;
    public Func<bool, int, bool, int>? OnProfile;
    public Func<bool, int, Thinker, bool>? OnAddTail;
    public Func<bool, int, bool>? OnDestroy;
    public Action<int>? OnDestroyList;
    public Action<bool, int, bool>? OnRemoveTravellers;
    public Action<bool, int>? OnLoadList;
    public Func<bool, int>? WorldPaused;
    public int FullCollections;
    public int StackClears;
    public string? ErrorText;
    public string? FatalText;

    public readonly record struct TickCall(bool Profile, bool Fresh, int Stat, bool Dest);

    public sealed class ProfileInfo
    {
        public int Calls;
        public double Time;
    }

    public sealed class Thinker
    {
        public int ObjectFlags;
    }

    public sealed class Actor
    {
        public int Flags8;
        public int Effects;
        public int FountainColor;
        public bool RenderLocally = true;
        public int DynamicLights;
        public int EffectCalls;
        public int LastEffect = -1;
    }

    public sealed class DynLight
    {
        public DynLight? Next;
        public int Ticks;
        public Action<DynLight>? OnTick;

        public void Tick()
        {
            Ticks++;
            if (OnTick != null)
                OnTick(this);
        }
    }

    public sealed class Level
    {
        public int Flags3;
        public int LocalWorldTimer;
        public int LocalTimer;
        public bool Frozen;
        public int FrozenChecks;
        public DynLight? Lights;
        public readonly List<Actor> Actors = new();
        public readonly List<Actor> ClientActors = new();

        public bool IsFrozen()
        {
            FrozenChecks++;
            return Frozen;
        }
    }

    public void Link(Thinker thinker, int statnum)
    {
        bool fresh = (thinker.ObjectFlags & OfJustSpawned) != 0 && statnum >= StatFirstThinking;
        if (!fresh && statnum != StatTravelling)
            thinker.ObjectFlags &= ~OfJustSpawned;
        Steps.Add((fresh ? "fresh:" : "live:") + statnum + ":" + thinker.ObjectFlags);
        if (OnAddTail != null)
            OnAddTail(fresh, statnum, thinker);
    }

    public void RunThinkers(Level level)
    {
        _ = level;
        ThinkCount = 0;
        Cycles.Add("think.reset");
        Cycles.Add("bot.reset");
        Cycles.Add("action.reset");
        BotWtg = 0;
        Cycles.Add("think.clock");
        if (ProfileThinkers == 0)
        {
            Walk(profile: false);
        }
        else
        {
            Profiles.Clear();
            Walk(profile: true);
            PublishProfiles(ProfileThinkers);
            ProfileThinkers = 0;
        }
        Cycles.Add("think.unclock");
    }

    public void RecreateDynamicLights(Actor mobj, bool dolights, bool frozen)
    {
        Recreated.Add(mobj);
        if ((mobj.Flags8 & Mf8RecreateLights) != 0)
        {
            mobj.Flags8 &= ~Mf8RecreateLights;
            if (dolights)
                mobj.DynamicLights++;
        }
        if ((mobj.Effects != 0 || mobj.FountainColor != 0) && mobj.RenderLocally && !frozen)
        {
            mobj.EffectCalls++;
            mobj.LastEffect = mobj.Effects;
        }
    }

    public void RunClientSideThinkers(Level level)
    {
        bool dolights = (GlLights && RenderMode == 4) || (DynLights && RenderMode != 4);
        int pausedRaw = 0;
        if (WorldPaused != null)
            pausedRaw = WorldPaused(false);
        bool paused = pausedRaw != 0;
        level.Flags3 &= ~Level3LightCreated;
        level.LocalWorldTimer += paused ? 0 : 1;
        level.LocalTimer++;
        if (!paused)
            Walk(profile: false);
        RecreateAll(level, dolights);
        if (dolights && !paused)
            TickLights(level.Lights);
    }

    public void DestroyAllThinkers(bool fullgc = true)
    {
        bool error = false;
        for (int i = 0; i <= MaxStatNum; i++)
        {
            if (i == StatTravelling || i == StatStatic)
                continue;
            error |= DestroyOne(fresh: false, i);
            error |= DestroyOne(fresh: true, i);
        }
        error |= DestroyOne(fresh: false, MaxStatNum + 1);
        if (fullgc)
        {
            Steps.Add("fullgc");
            FullCollections++;
        }
        if (error)
        {
            Steps.Add("clear");
            StackClears++;
            if (fullgc)
            {
                Steps.Add("error");
                ErrorText = DestroyFailed;
            }
            else
            {
                Steps.Add("fatal");
                FatalText = DestroyFailed;
            }
        }
    }

    public void CleanUpTravellers(bool saveGame)
    {
        Steps.Add("list:" + StatTravelling);
        if (OnDestroyList != null)
            OnDestroyList(StatTravelling);
        for (int i = 0; i <= MaxStatNum; i++)
        {
            Steps.Add("fresh-remove:" + i + ":" + (saveGame ? 1 : 0));
            if (OnRemoveTravellers != null)
                OnRemoveTravellers(true, i, saveGame);
            Steps.Add("live-remove:" + i + ":" + (saveGame ? 1 : 0));
            if (OnRemoveTravellers != null)
                OnRemoveTravellers(false, i, saveGame);
        }
    }

    public void OnLoad()
    {
        for (int i = 0; i < FreshListCount; i++)
        {
            Steps.Add("fresh:" + i);
            if (OnLoadList != null)
                OnLoadList(true, i);
        }
        for (int i = 0; i < ThinkerListCount; i++)
        {
            Steps.Add("live:" + i);
            if (OnLoadList != null)
                OnLoadList(false, i);
        }
    }

    void Walk(bool profile)
    {
        for (int stat = StatFirstThinking; stat <= MaxStatNum; stat++)
            Invoke(profile, fresh: false, stat, dest: false);
        int count;
        do
        {
            count = 0;
            for (int stat = StatFirstThinking; stat <= MaxStatNum; stat++)
                count += Invoke(profile, fresh: true, stat, dest: true);
        }
        while (count != 0);
    }

    int Invoke(bool profile, bool fresh, int stat, bool dest)
    {
        Ticks.Add(new TickCall(profile, fresh, stat, dest));
        int found;
        if (profile)
            found = OnProfile == null ? 0 : OnProfile(fresh, stat, dest);
        else
            found = OnTick == null ? 0 : OnTick(fresh, stat, dest);
        return found;
    }

    bool DestroyOne(bool fresh, int stat)
    {
        bool bad = OnDestroy != null && OnDestroy(fresh, stat);
        Steps.Add((fresh ? "fresh-destroy:" : "live-destroy:") + stat + ":" + (bad ? 1 : 0));
        return bad;
    }

    void RecreateAll(Level level, bool dolights)
    {
        bool frozen = level.IsFrozen();
        for (int i = 0; i < level.Actors.Count; i++)
            RecreateDynamicLights(level.Actors[i], dolights, frozen);
        for (int i = 0; i < level.ClientActors.Count; i++)
            RecreateDynamicLights(level.ClientActors[i], dolights, frozen);
    }

    static void TickLights(DynLight? light)
    {
        while (light != null)
        {
            DynLight? next = light.Next;
            light.Tick();
            light = next;
        }
    }

    void PublishProfiles(int mode)
    {
        var sorted = new List<ProfileRow>();
        foreach (KeyValuePair<string, ProfileInfo> pair in Profiles)
            sorted.Add(new ProfileRow(pair.Key, pair.Value.Calls, pair.Value.Time));
        sorted.Sort((left, right) => CompareProfiles(left, right, mode));
        Prints.Add(TextYellow + "Total, ms   Averg, ms   Calls   Actor class\n");
        Prints.Add(TextYellow + "----------  ----------  ------  --------------------\n");
        uint limit = ProfileLimit > 0 ? ProfileLimit : uint.MaxValue;
        uint shown = Math.Min(limit, (uint)sorted.Count);
        for (uint i = 0; i < shown; i++)
            Prints.Add(FormatRow(sorted[(int)i], mode));
    }

    static int CompareProfiles(ProfileRow left, ProfileRow right, int mode)
    {
        switch (mode)
        {
            case 1:
                return StrIcmp(left.Name, right.Name);
            case 2:
                return StrIcmp(right.Name, left.Name);
            case 3:
                return left.Calls.CompareTo(right.Calls);
            case 4:
                return right.Calls.CompareTo(left.Calls);
            case 5:
                return Average(left).CompareTo(Average(right));
            case 6:
                return Average(right).CompareTo(Average(left));
            case 7:
                return left.Time.CompareTo(right.Time);
            default:
                return right.Time.CompareTo(left.Time);
        }
    }

    static string FormatRow(ProfileRow info, int mode)
    {
        double average = info.Time / info.Calls;
        string timeTone = mode >= 7 ? TextYellow : TextWhite;
        string averageTone = mode == 5 || mode == 6 ? TextYellow : TextWhite;
        string callTone = mode == 3 || mode == 4 ? TextYellow : TextWhite;
        string nameTone = mode == 1 || mode == 2 ? TextYellow : TextWhite;
        return timeTone + Width(info.Time) + "  "
            + averageTone + Width(average) + "  "
            + callTone + info.Calls.ToString(CultureInfo.InvariantCulture).PadLeft(6) + "  "
            + nameTone + info.Name + "\n";
    }

    static string Width(double value)
    {
        return value.ToString("0.000000", CultureInfo.InvariantCulture).PadLeft(10);
    }

    static double Average(ProfileRow row)
    {
        return row.Time / row.Calls;
    }

    static int StrIcmp(string left, string right)
    {
        int n = left.Length < right.Length ? left.Length : right.Length;
        for (int i = 0; i < n; i++)
        {
            int a = Fold(left[i]);
            int b = Fold(right[i]);
            if (a != b)
                return a - b;
        }
        return left.Length - right.Length;
    }

    static int Fold(char c)
    {
        if (c >= 'A' && c <= 'Z')
            return c + 32;
        return c;
    }

    readonly record struct ProfileRow(string Name, int Calls, double Time);
}
