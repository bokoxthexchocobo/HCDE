namespace HCDE.Playsim;

/// <summary>Flag/raise/refire actions from <c>p_actionfunctions.cpp</c> (2767–2989).</summary>
public static class VmPActionRaiseFlags
{
    public const int MfCountkill = 1;
    public const int MfCountitem = 1 << 1;
    public const int Mf5Countsecret = 1;

    public static void ChangeFlag(Action<string, bool>? modFlag, string flagName, bool value) =>
        modFlag?.Invoke(flagName, value);

    public static bool CheckFlag(Func<object?>? copyPtr, Func<object?, string, bool>? checkFlag, int ptr, string flag)
    {
        var owner = copyPtr?.Invoke();
        return owner != null && checkFlag?.Invoke(owner, flag) == true;
    }

    public sealed class CountFlagsActor
    {
        public int Flags { get; set; }
        public int Flags5 { get; set; }
        public int Health { get; set; }
        public bool CountsAsKill { get; set; }
    }

    public sealed class LevelCounters
    {
        public int Monsters { get; set; }
        public int Items { get; set; }
        public int Secrets { get; set; }
    }

    public static void ChangeCountFlags(CountFlagsActor actor, LevelCounters level, int kill, int item, int secret)
    {
        if (actor.CountsAsKill && actor.Health > 0)
            level.Monsters--;
        if ((actor.Flags & MfCountitem) != 0)
            level.Items--;
        if ((actor.Flags5 & Mf5Countsecret) != 0)
            level.Secrets--;

        if (kill != -1)
            actor.Flags = kill == 0 ? actor.Flags & ~MfCountkill : actor.Flags | MfCountkill;
        if (item != -1)
            actor.Flags = item == 0 ? actor.Flags & ~MfCountitem : actor.Flags | MfCountitem;
        if (secret != -1)
            actor.Flags5 = secret == 0 ? actor.Flags5 & ~Mf5Countsecret : actor.Flags5 | Mf5Countsecret;

        if (actor.CountsAsKill && actor.Health > 0)
            level.Monsters++;
        if ((actor.Flags & MfCountitem) != 0)
            level.Items++;
        if ((actor.Flags5 & Mf5Countsecret) != 0)
            level.Secrets++;
    }

    public static void RaiseMaster(object? master, Action<object?, object?, int>? raise, object self, int flags)
    {
        if (master != null)
            raise?.Invoke(master, self, flags);
    }

    public static int RaiseChildren(IEnumerable<object> thinkers, object self, Func<object, bool> isChild, Action<object, object, int> raise, int flags)
    {
        var n = 0;
        foreach (var mo in thinkers)
        {
            if (isChild(mo))
            {
                raise(mo, self, flags);
                n++;
            }
        }

        return n;
    }

    public static bool MonsterRefire(
        int prob,
        int roll,
        object? target,
        int targetHealth,
        Func<bool>? hitFriend,
        Func<bool>? checkSight,
        Action? faceTarget)
    {
        faceTarget?.Invoke();
        if (roll < prob)
            return false;
        if (target == null || hitFriend?.Invoke() == true || targetHealth <= 0 || checkSight?.Invoke() == false)
            return true;
        return false;
    }

    public static double SetAngle(double current, double angle, bool absolute) =>
        absolute ? angle : current + angle;
}
