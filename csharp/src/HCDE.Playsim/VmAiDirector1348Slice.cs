using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Monster AI director scaffold from <c>d_net_aidirector.cpp</c> (1-284).
/// Chunk 1348. Sweeps cluster living monsters. Regroup hints are stored
/// only while the hint cvar is on, and the named RNG is not drawn.
/// </summary>
public sealed class VmAiDirector1348
{
    public const int MfCorpse = 0x00100000;
    public const int Mf2Dormant = 0x10000000;
    public const int Mf3IsMonster = 0x00002000;
    public const string RngStream = "AIDirector";

    public readonly DirectorState1348 State = new();
    public bool Enable;
    public bool RegroupHint;
    public bool LocalAuthority;
    public int AuthorityQueries;
    public int SweepTics { get; private set; } = 7;
    public ulong Clock;
    public ulong ClockStep;
    public Level1348? Level;
    public readonly List<ObservedMonster1348> Observed = new();
    public readonly List<Cluster1348> Clusters = new();
    public readonly List<RegroupHint1348> Hints = new();

    public void SetSweepTics(int value)
    {
        if (value < 1)
            value = 1;
        if (value > 70)
            value = 70;
        SweepTics = value;
    }

    public DirectorState1348 ReadState() => State;

    public bool ShouldDriveAuthority()
    {
        if (!Enable)
            return false;
        AuthorityQueries++;
        if (!LocalAuthority)
            return false;
        return true;
    }

    public bool ReadRegroupHint(Actor1348? actor, OutInt1348? groupId, OutInt1348? storedTic)
    {
        if (actor == null)
            return false;
        foreach (var hint in Hints)
        {
            if (!ReferenceEquals(hint.Actor, actor))
                continue;
            if (groupId != null)
                groupId.Value = hint.GroupId;
            if (storedTic != null)
                storedTic.Value = hint.StoredTic;
            return true;
        }
        return false;
    }

    public bool ConsumeRegroupHint(Actor1348? actor, OutInt1348? groupId, OutInt1348? storedTic)
    {
        if (!ReadRegroupHint(actor, groupId, storedTic))
            return false;
        State.HintsConsumedTotal++;
        return true;
    }

    public bool ConsumeRegroupTarget(
        Actor1348? actor,
        OutDouble1348? x,
        OutDouble1348? y,
        OutInt1348? groupId,
        OutInt1348? storedTic)
    {
        var foundGroup = new OutInt1348(-1);
        var foundTic = new OutInt1348(0);
        if (!ReadRegroupHint(actor, foundGroup, foundTic))
            return false;
        if (foundGroup.Value < 0 || foundGroup.Value >= Clusters.Count)
            return false;
        var cluster = Clusters[foundGroup.Value];
        if (x != null)
            x.Value = cluster.X;
        if (y != null)
            y.Value = cluster.Y;
        if (groupId != null)
            groupId.Value = foundGroup.Value;
        if (storedTic != null)
            storedTic.Value = foundTic.Value;
        State.HintsConsumedTotal++;
        return true;
    }

    public void Tick()
    {
        if (!ShouldDriveAuthority())
        {
            State.AuthorityActive = false;
            State.HintsIssuedThisTic = 0;
            return;
        }

        State.AuthorityActive = true;
        State.TicksSinceStart++;
        State.LastTickWallclockMs = unchecked((long)MsTime());
        if (State.TicksSinceStart == 1 || State.TicksSinceStart % SweepTics == 0)
            Observe();
        else
            State.HintsIssuedThisTic = 0;
        _ = RngStream;
    }

    public string AiStatus()
    {
        var state = ReadState();
        return string.Format(
            CultureInfo.InvariantCulture,
            "\n=== HCDE AI Director ===\n" +
            "  sv_aidirector_enable      = {0}\n" +
            "  sv_aidirector_sweep_tics  = {1}\n" +
            "  authority-drives          = {2}\n" +
            "  authority-active          = {3}\n" +
            "  ticks-since-start         = {4}\n" +
            "  last-sweep-tic            = {5}\n" +
            "  sweep-count               = {6}\n" +
            "  last-sweep-wallclock-ms   = {7} (diagnostic only)\n" +
            "  max-sweep-wallclock-ms    = {8} (diagnostic only)\n" +
            "  monsters-observed         = {9}\n" +
            "  monsters-alive            = {10}\n" +
            "  monsters-dormant          = {11}\n" +
            "  monsters-corpse           = {12}\n" +
            "  groups-observed           = {13}\n" +
            "  largest-group-size        = {14}\n" +
            "  sv_aidirector_regroup_hint = {15}\n" +
            "  regroup-hints-stored      = {16}\n" +
            "  deterministic-rng         = pr_aidirector named stream; draws={17}\n" +
            "  hints-issued-this-tic     = {18}\n" +
            "  hints-issued-total        = {19}\n" +
            "  hints-consumed-total      = {20} (regroup direction biases applied)\n" +
            "  last-tick-wallclock-ms    = {21} (diagnostic only)\n" +
            "  phase                     = Phase 3: deterministic regroup chase-direction bias; no replication.\n" +
            "  see docs/HCDE_AIDIRECTOR_AUDIT.md for boundaries.\n" +
            "========================\n",
            Enable ? "on" : "off",
            SweepTics,
            ShouldDriveAuthority() ? "yes" : "no",
            state.AuthorityActive ? "yes" : "no",
            state.TicksSinceStart,
            state.LastSweepTic,
            state.SweepCount,
            state.LastSweepWallclockMs,
            state.MaxSweepWallclockMs,
            state.MonsterCountObserved,
            state.AliveMonsterCountObserved,
            state.DormantMonsterCountObserved,
            state.CorpseMonsterCountObserved,
            state.GroupCountObserved,
            state.LargestGroupSizeObserved,
            RegroupHint ? "on" : "off",
            state.RegroupHintsStored,
            state.DeterministicRngDraws,
            state.HintsIssuedThisTic,
            state.HintsIssuedTotal,
            state.HintsConsumedTotal,
            state.LastTickWallclockMs);
    }

    void Observe()
    {
        ulong sweepStart = MsTime();
        State.LastSweepTic = Level != null ? Level.Time : 0;
        State.SweepCount++;
        State.MonsterCountObserved = 0;
        State.AliveMonsterCountObserved = 0;
        State.DormantMonsterCountObserved = 0;
        State.CorpseMonsterCountObserved = 0;
        State.GroupCountObserved = 0;
        State.LargestGroupSizeObserved = 0;
        State.RegroupHintsStored = 0;
        State.HintsIssuedThisTic = 0;
        Observed.Clear();
        Clusters.Clear();
        Hints.Clear();
        if (Level == null)
            return;

        foreach (var actor in Level.Actors)
        {
            if ((actor.Flags3 & Mf3IsMonster) == 0)
                continue;
            State.MonsterCountObserved++;
            if ((actor.Flags2 & Mf2Dormant) != 0)
                State.DormantMonsterCountObserved++;
            if ((actor.Flags & MfCorpse) != 0 || actor.Health <= 0)
                State.CorpseMonsterCountObserved++;
            else
                State.AliveMonsterCountObserved++;
            if (actor.Health <= 0 || (actor.Flags & MfCorpse) != 0)
                continue;

            int groupId = -1;
            for (int i = 0; i < Clusters.Count; i++)
            {
                var cluster = Clusters[i];
                if (!WithinReach(cluster.X, cluster.Y, actor.X, actor.Y))
                    continue;
                groupId = i;
                double count = cluster.Count;
                double recip = 1.0 / (count + 1.0);
                cluster.X = (cluster.X * count + actor.X) * recip;
                cluster.Y = (cluster.Y * count + actor.Y) * recip;
                cluster.Count++;
                break;
            }
            if (groupId < 0)
            {
                Clusters.Add(new Cluster1348 { X = actor.X, Y = actor.Y, Count = 1 });
                groupId = Clusters.Count - 1;
            }
            Observed.Add(new ObservedMonster1348 { Actor = actor, GroupId = groupId });
            if (Clusters[groupId].Count > State.LargestGroupSizeObserved)
                State.LargestGroupSizeObserved = Clusters[groupId].Count;
        }

        State.GroupCountObserved = Clusters.Count;
        if (RegroupHint)
        {
            foreach (var observed in Observed)
            {
                if (observed.GroupId >= 0 && Clusters[observed.GroupId].Count >= 3)
                {
                    Hints.Add(new RegroupHint1348
                    {
                        Actor = observed.Actor,
                        GroupId = observed.GroupId,
                        StoredTic = State.LastSweepTic,
                    });
                    State.HintsIssuedThisTic++;
                }
            }
            State.RegroupHintsStored = Hints.Count;
            State.HintsIssuedTotal += State.HintsIssuedThisTic;
        }

        State.LastSweepWallclockMs = unchecked((long)(MsTime() - sweepStart));
        if (State.LastSweepWallclockMs > State.MaxSweepWallclockMs)
            State.MaxSweepWallclockMs = State.LastSweepWallclockMs;
    }

    static bool WithinReach(double cx, double cy, double x, double y)
    {
        double dx = cx - x;
        double dy = cy - y;
        return dx * dx + dy * dy <= 512.0 * 512.0;
    }

    ulong MsTime()
    {
        ulong now = Clock;
        Clock += ClockStep;
        return now;
    }
}

public sealed class DirectorState1348
{
    public bool AuthorityActive;
    public int TicksSinceStart;
    public long LastTickWallclockMs;
    public int LastSweepTic;
    public int SweepCount;
    public long LastSweepWallclockMs;
    public long MaxSweepWallclockMs;
    public int MonsterCountObserved;
    public int AliveMonsterCountObserved;
    public int DormantMonsterCountObserved;
    public int CorpseMonsterCountObserved;
    public int GroupCountObserved;
    public int LargestGroupSizeObserved;
    public int RegroupHintsStored;
    public int DeterministicRngDraws;
    public int HintsIssuedThisTic;
    public int HintsIssuedTotal;
    public int HintsConsumedTotal;
}

public sealed class Actor1348
{
    public int Flags;
    public int Flags2;
    public int Flags3;
    public int Health;
    public double X;
    public double Y;
}

public sealed class Level1348
{
    public int Time;
    public List<Actor1348> Actors = new();
}

public sealed class Cluster1348
{
    public double X;
    public double Y;
    public int Count;
}

public sealed class ObservedMonster1348
{
    public Actor1348? Actor;
    public int GroupId = -1;
}

public sealed class RegroupHint1348
{
    public Actor1348? Actor;
    public int GroupId = -1;
    public int StoredTic;
}

public sealed class OutInt1348
{
    public OutInt1348(int value) => Value = value;
    public int Value;
}

public sealed class OutDouble1348
{
    public OutDouble1348(double value) => Value = value;
    public double Value;
}
