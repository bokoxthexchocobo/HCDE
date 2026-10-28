namespace HCDE.Playsim;

/// <summary>TID lookup, missile damage, howl, and skullfly slam from <c>p_mobj.cpp</c> (3629–3827).</summary>
public static class VmPMobjTidMissileSlam
{
    public const int Mf8Onlyslamsolid = 1 << 0;
    public const int MfSolid = 1 << 0;
    public const int MfShootable = 1 << 2;
    public const int MfSkullfly = 1 << 25;
    public const int Mf2Dormant = 1 << 1;
    public const int Mf8Retargetafterslam = 1 << 0;
    public const int IntMax = int.MaxValue;

    public sealed class TidNode
    {
        public int Tid { get; set; }
        public TidNode? Next { get; set; }
    }

    public sealed class TidLevelContext
    {
        public Func<int, bool, TidNode?>? GetHashHead { get; init; }
        public Func<int>? GenUniqueTidRand { get; init; }
    }

    public static int TidHashIndex(int tid) => tid & 127;

    public static bool IsTidUsed(TidLevelContext ctx, int tid, bool clientside)
    {
        var probe = ctx.GetHashHead?.Invoke(TidHashIndex(tid), clientside);
        while (probe != null)
        {
            if (probe.Tid == tid)
                return true;
            probe = probe.Next;
        }
        return false;
    }

    public static int ClampLinearLimit(int startTid, int limit)
    {
        if (startTid > IntMax - limit + 1)
            return IntMax;
        return limit + startTid - 1;
    }

    public static int FindUniqueTidLinear(TidLevelContext ctx, int startTid, int limitEnd, bool clientside)
    {
        for (var tid = startTid; tid <= limitEnd; tid++)
        {
            if (tid != 0 && !IsTidUsed(ctx, tid, clientside))
                return tid;
        }
        return 0;
    }

    public static int FindUniqueTidRandomBatch(TidLevelContext ctx, int batchLimit, bool clientside)
    {
        var limit = batchLimit == 0 ? IntMax : batchLimit;
        for (var i = 0; i < limit; i += 5)
        {
            var raw = ctx.GenUniqueTidRand?.Invoke() ?? 1;
            var tid = raw & IntMax;
            tid = FindUniqueTid(ctx, tid == 0 ? 1 : tid, 5, clientside);
            if (tid != 0)
                return tid;
        }
        return 0;
    }

    public static int FindUniqueTid(TidLevelContext ctx, int startTid, int limit, bool clientside)
    {
        if (startTid != 0)
        {
            var limitEnd = ClampLinearLimit(startTid, limit);
            return FindUniqueTidLinear(ctx, startTid, limitEnd, clientside);
        }
        return FindUniqueTidRandomBatch(ctx, limit, clientside);
    }

    public sealed class MissileDamageActor
    {
        public int DamageVal { get; set; }
        public bool HasDamageFunc { get; set; }
    }

    public sealed class MissileDamageContext
    {
        public Func<MissileDamageActor, int>? VmDamageCall { get; init; }
        public Func<int>? RollMissileDamage { get; init; }
    }

    public static int GetMissileDamage(MissileDamageContext ctx, MissileDamageActor self, int mask, int add)
    {
        if (self.DamageVal >= 0)
        {
            if (mask == 0)
                return add * self.DamageVal;
            var roll = ctx.RollMissileDamage?.Invoke() ?? 0;
            return ((roll & mask) + add) * self.DamageVal;
        }
        if (!self.HasDamageFunc)
            return 0;
        return ctx.VmDamageCall?.Invoke(self) ?? 0;
    }

    public sealed class HowlContext
    {
        public Func<bool>? IsPlayingHowl { get; init; }
        public Action? PlayHowl { get; init; }
    }

    public static void Howl(HowlContext ctx)
    {
        if (ctx.IsPlayingHowl?.Invoke() == true)
            return;
        ctx.PlayHowl?.Invoke();
    }

    public sealed class SlamActor
    {
        public int Flags8 { get; set; }
        public int Flags { get; set; }
        public int Flags2 { get; set; }
        public int Health { get; set; }
        public bool HasSlamState { get; set; }
        public bool HasSeeState { get; set; }
        public bool ClearedSkullfly { get; set; }
        public bool ZeroedVel { get; set; }
        public int Tics { get; set; } = -1;
    }

    public sealed class SlamThing
    {
        public int Flags { get; set; }
    }

    public sealed class SlamContext
    {
        public MissileDamageContext? Damage { get; init; }
        public Func<SlamActor, SlamThing, int, int>? DamageMobj { get; init; }
        public Action<int, SlamThing, SlamActor>? TraceBleed { get; init; }
        public Action? SetSlamState { get; init; }
        public Action? SetSeeState { get; init; }
        public Action? SetIdle { get; init; }
    }

    public static bool OnlySlamSolidPass(int flags8, int thingFlags) =>
        (flags8 & Mf8Onlyslamsolid) != 0
        && (thingFlags & (MfSolid | MfShootable)) == 0;

    public static bool Slam(SlamContext ctx, SlamActor self, SlamThing thing, MissileDamageActor damageSelf)
    {
        if (OnlySlamSolidPass(self.Flags8, thing.Flags))
            return true;

        if ((self.Flags & MfSkullfly) != 0)
        {
            self.Flags &= ~MfSkullfly;
            self.ClearedSkullfly = true;
        }
        self.ZeroedVel = true;

        if (self.Health <= 0)
            return false;

        if ((self.Flags2 & Mf2Dormant) != 0)
        {
            ctx.SetIdle?.Invoke();
            self.Tics = -1;
            return false;
        }

        var dam = GetMissileDamage(ctx.Damage!, damageSelf, 7, 1);
        var newDam = ctx.DamageMobj?.Invoke(self, thing, dam) ?? dam;
        ctx.TraceBleed?.Invoke(newDam > 0 ? newDam : dam, thing, self);

        if (self.Health <= 0)
            return false;

        if (self.HasSlamState)
            ctx.SetSlamState?.Invoke();
        else if (self.HasSeeState && (self.Flags8 & Mf8Retargetafterslam) == 0)
            ctx.SetSeeState?.Invoke();
        else
            ctx.SetIdle?.Invoke();

        return false;
    }

    public sealed class CallSlamContext
    {
        public SlamContext? Native { get; init; }
        public Func<SlamActor, SlamThing, bool>? VirtualSlam { get; init; }
    }

    public static bool CallSlam(
        CallSlamContext ctx,
        bool hasVirtual,
        SlamActor self,
        SlamThing thing,
        MissileDamageActor damageSelf)
    {
        if (hasVirtual)
            return ctx.VirtualSlam?.Invoke(self, thing) ?? false;
        return Slam(ctx.Native!, self, thing, damageSelf);
    }
}
