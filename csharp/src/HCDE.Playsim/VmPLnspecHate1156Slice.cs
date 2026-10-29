namespace HCDE.Playsim;

/// <summary>
/// <c>Thing_Hate</c> from <c>p_lnspec.cpp</c> (<b>1565–1727</b>). Chunk <b>1156</b>.
/// An exhausted hatee scan stops after one pass around the host list.
/// </summary>
public static class VmPLnspecHate1156Slice
{
    public const int Shootable = 0x00000004;
    public const int Dormant = 0x10000000;
    public const int NoSightCheck = 0x00000080;
    public const int HuntPlayers = unchecked((int)0x80000000);
    public const int NoHatePlayers = 0x00000001;

    public readonly record struct HateActor(
        int Id,
        int Tid,
        int Flags,
        int Flags2,
        int Flags3,
        int Flags4,
        int Health,
        bool SeeState,
        int TargetId,
        int TargetTid,
        int LastEnemyId,
        int LastEnemyTid,
        int GoalId,
        int TidToHate,
        int LastLookId,
        bool EnteredSee);

    public readonly record struct HateResult(
        bool Accepted,
        bool UsedActivator,
        HateActor Activator,
        IReadOnlyList<HateActor> Haters);

    public static HateResult Hate(
        int haterTid,
        int hateeTid,
        int mode,
        bool hasActivator,
        bool activatorIsPlayer,
        HateActor activator,
        IReadOnlyList<HateActor> hatees,
        IReadOnlyList<HateActor> haters)
    {
        int found = -1;
        bool nothing = false;
        if (hateeTid != 0)
        {
            found = FirstHatee(hatees, 0, -1);
            nothing = found < 0;
        }

        if (haterTid == 0)
        {
            if (hasActivator && activatorIsPlayer)
                return new HateResult(false, false, activator, haters);
            if (!hasActivator)
                return new HateResult(true, false, activator, haters);

            int cursor = found;
            HateActor updated = Apply(activator, hateeTid, mode, hasActivator, activator, hatees, found, nothing, ref cursor);
            return new HateResult(true, true, updated, haters);
        }

        var changed = new List<HateActor>(haters.Count);
        int scan = found;
        int next = FirstHater(haters, 0);
        int index = 0;
        while (index < haters.Count)
        {
            HateActor current = haters[index];
            if (index == next)
            {
                changed.Add(Apply(current, hateeTid, mode, hasActivator, activator, hatees, found, nothing, ref scan));
                next = FirstHater(haters, index + 1);
            }
            else
            {
                changed.Add(current);
            }

            index++;
        }

        return new HateResult(true, false, activator, changed);
    }

    private static HateActor Apply(
        HateActor hater,
        int hateeTid,
        int mode,
        bool hasActivator,
        HateActor activator,
        IReadOnlyList<HateActor> hatees,
        int found,
        bool nothing,
        ref int cursor)
    {
        int targetId = hater.TargetId;
        int targetTid = hater.TargetTid;
        int lastId = hater.LastEnemyId;
        int lastTid = hater.LastEnemyTid;
        int flags3 = hater.Flags3;
        int flags4 = hater.Flags4;
        int tidToHate = hater.TidToHate;
        int lastLook = hater.LastLookId;
        bool entered = false;
        if (!hater.SeeState)
        {
            return hater with { EnteredSee = false };
        }

        if (mode != 0)
        {
            tidToHate = hateeTid;
            lastLook = -1;
            if (hateeTid != 0)
            {
                if (targetId >= 0 && targetTid != hateeTid)
                {
                    targetId = -1;
                    targetTid = 0;
                }

                if (lastId >= 0 && lastTid != hateeTid)
                {
                    lastId = -1;
                    lastTid = 0;
                }
            }
        }

        flags3 = Bit(flags3, NoSightCheck, mode == 2 || mode == 4 || mode == 6);
        flags3 = Bit(flags3, HuntPlayers, mode == 3 || mode == 4);
        flags4 = Bit(flags4, NoHatePlayers, mode == 5 || mode == 6);

        bool have = false;
        int hateeId = -1;
        int hateeActorTid = 0;
        if (hateeTid == 0)
        {
            if (hasActivator)
            {
                have = true;
                hateeId = activator.Id;
                hateeActorTid = activator.Tid;
            }
        }
        else if (!nothing && mode == 0)
        {
            have = true;
            hateeId = hatees[found].Id;
            hateeActorTid = hatees[found].Tid;
        }
        else if (!nothing)
        {
            int next = NextHatee(hatees, ref cursor, hater.Id);
            if (next >= 0)
            {
                have = true;
                hateeId = hatees[next].Id;
                hateeActorTid = hatees[next].Tid;
            }
        }

        if (have && hateeId != hater.Id && (mode == 0 || (hater.GoalId >= 0 && targetId != hater.GoalId)))
        {
            if (targetId >= 0)
            {
                lastId = targetId;
                lastTid = targetTid;
            }

            targetId = hateeId;
            targetTid = hateeActorTid;
            if ((hater.Flags2 & Dormant) == 0 && hater.Health > 0)
                entered = true;
        }

        return hater with
        {
            Flags3 = flags3,
            Flags4 = flags4,
            TargetId = targetId,
            TargetTid = targetTid,
            LastEnemyId = lastId,
            LastEnemyTid = lastTid,
            TidToHate = tidToHate,
            LastLookId = lastLook,
            EnteredSee = entered,
        };
    }

    private static int FirstHatee(IReadOnlyList<HateActor> hatees, int start, int skipId)
    {
        for (int i = start; i < hatees.Count; i++)
        {
            if (hatees[i].Id != skipId && Acceptable(hatees[i]))
                return i;
        }

        return -1;
    }

    private static int NextHatee(IReadOnlyList<HateActor> hatees, ref int cursor, int selfId)
    {
        int count = hatees.Count;
        if (count == 0)
            return -1;
        for (int step = 1; step <= count; step++)
        {
            int index = (cursor + step) % count;
            HateActor candidate = hatees[index];
            if (candidate.Id != selfId && Acceptable(candidate))
            {
                cursor = index;
                return index;
            }
        }

        return -1;
    }

    private static int FirstHater(IReadOnlyList<HateActor> haters, int start)
    {
        for (int i = start; i < haters.Count; i++)
        {
            if (haters[i].Health > 0 && (haters[i].Flags & Shootable) != 0)
                return i;
        }

        return -1;
    }

    private static bool Acceptable(HateActor actor)
    {
        if ((actor.Flags & Shootable) == 0)
            return false;
        if (actor.Health <= 0)
            return false;
        if ((actor.Flags2 & Dormant) != 0)
            return false;
        return true;
    }

    private static int Bit(int flags, int bit, bool on)
    {
        if (on)
            return flags | bit;
        return flags & ~bit;
    }
}
