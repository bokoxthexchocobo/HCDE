namespace HCDE.Playsim;

/// <summary>Fake-floor triggers, player land hooks, and TID hash from <c>p_mobj.cpp</c> (3214–3618).</summary>
public static class VmPMobjFakeFloorLand
{
    public const int SecspacHitFakeFloor = 1;
    public const int CfPredicting = 1;

    public sealed class FakeFloorActor
    {
        public bool HasPlayer { get; init; }
        public int PlayerCheats { get; init; }
        public double Z { get; init; }
        public double ViewHeight { get; init; }
    }

    public sealed class FakeFloorSector
    {
        public bool HasHeightSec { get; init; }
        public bool HasSecActTarget { get; init; }
        public double WaterZ { get; init; }
        public double CeilingWaterZ { get; init; }
        public bool FakeFloorOnly { get; init; }
    }

    public sealed class FakeFloorContext
    {
        public Action<int>? TriggerSectorAction { get; init; }
    }

    public static void CheckFakeFloorTriggers(
        FakeFloorContext ctx,
        FakeFloorActor mo,
        FakeFloorSector sec,
        double oldZ,
        bool oldZHasViewheight)
    {
        if (mo.HasPlayer && (mo.PlayerCheats & CfPredicting) != 0)
            return;
        if (!sec.HasHeightSec || !sec.HasSecActTarget)
            return;

        var viewheight = mo.HasPlayer ? mo.ViewHeight : mo.ViewHeight;
        if (oldZ > sec.WaterZ && mo.Z <= sec.WaterZ)
            ctx.TriggerSectorAction?.Invoke(SecspacHitFakeFloor);

        var newz = mo.Z + viewheight;
        if (!oldZHasViewheight)
            oldZ += viewheight;

        if (oldZ <= sec.WaterZ && newz > sec.WaterZ)
            ctx.TriggerSectorAction?.Invoke(2);
        else if (oldZ > sec.WaterZ && newz <= sec.WaterZ)
            ctx.TriggerSectorAction?.Invoke(3);

        if (!sec.FakeFloorOnly)
        {
            if (oldZ <= sec.CeilingWaterZ && newz > sec.CeilingWaterZ)
                ctx.TriggerSectorAction?.Invoke(4);
            else if (oldZ > sec.CeilingWaterZ && newz <= sec.CeilingWaterZ)
                ctx.TriggerSectorAction?.Invoke(5);
        }
    }

    public static void PlayerLandedOnThing(
        Action? squatView,
        Action? fallingDamage,
        Action? makeRumble,
        Action? gruntSound,
        bool predicting)
    {
        squatView?.Invoke();
        if (predicting)
            return;
        fallingDamage?.Invoke();
        makeRumble?.Invoke();
        gruntSound?.Invoke();
    }

    public static void CallFallAndSink(bool hasVirtual, Action? vmCall, Action<double, double>? nativeCall, double grav, double oldFloorZ)
    {
        if (hasVirtual)
            vmCall?.Invoke();
        else
            nativeCall?.Invoke(grav, oldFloorZ);
    }

    public sealed class HashNode
    {
        public int Tid { get; set; }
        public HashNode? Next { get; set; }
        public HashNode? PrevLink { get; set; }
    }

    public static void AddToHash(HashNode self, int tid, Func<int, HashNode?>? getSlot, Action<int, HashNode>? setSlot)
    {
        if (tid == 0)
        {
            self.Next = null;
            self.PrevLink = null;
            return;
        }

        var hash = tid & 127;
        var slot = getSlot?.Invoke(hash);
        self.Next = slot;
        setSlot?.Invoke(hash, self);
    }

    public static void RemoveFromHash(HashNode self)
    {
        if (self.Tid == 0 || self.PrevLink == null)
            return;
        self.Tid = 0;
        self.Next = null;
        self.PrevLink = null;
    }

    public static void SetTid(HashNode self, int newTid, Action? remove, Action? add)
    {
        remove?.Invoke();
        self.Tid = newTid;
        add?.Invoke();
    }
}
