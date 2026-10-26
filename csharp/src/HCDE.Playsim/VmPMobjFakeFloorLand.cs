namespace HCDE.Playsim;

/// <summary>Fake-floor triggers, player land hooks, and TID hash from <c>p_mobj.cpp</c> (3214–3618).</summary>
public static class VmPMobjFakeFloorLand
{
    public const int SecspacEyesDive = 1 << 6;
    public const int SecspacEyesSurface = 1 << 7;
    public const int SecspacEyesBelowC = 1 << 8;
    public const int SecspacEyesAboveC = 1 << 9;
    public const int SecspacHitFakeFloor = 1 << 10;
    public const int CfPredicting = 1;
    public const int SecmfFakeflooronly = 1;

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
        public int HeightSecMoreFlags { get; init; }
        public bool CheckFakeCeiling => (HeightSecMoreFlags & SecmfFakeflooronly) == 0;
    }

    public sealed class FakeFloorContext
    {
        public Action<int>? TriggerSectorAction { get; init; }
    }

    public static bool ShouldSkipFakeFloorTriggers(FakeFloorActor mo) =>
        mo.HasPlayer && (mo.PlayerCheats & CfPredicting) != 0;

    public static double ViewHeightForActor(FakeFloorActor mo, double actorHeight) =>
        mo.HasPlayer ? mo.ViewHeight : actorHeight;

    public static void TriggerFeetHitFakeFloor(FakeFloorContext ctx, double oldZ, double z, double waterZ)
    {
        if (oldZ > waterZ && z <= waterZ)
            ctx.TriggerSectorAction?.Invoke(SecspacHitFakeFloor);
    }

    public static void TriggerEyesFakeFloorTransitions(FakeFloorContext ctx, double oldZ, double newZ, double waterZ)
    {
        if (oldZ <= waterZ && newZ > waterZ)
            ctx.TriggerSectorAction?.Invoke(SecspacEyesSurface);
        else if (oldZ > waterZ && newZ <= waterZ)
            ctx.TriggerSectorAction?.Invoke(SecspacEyesDive);
    }

    public static void TriggerFakeCeilingTransitions(FakeFloorContext ctx, FakeFloorSector sec, double oldZ, double newZ)
    {
        if (!sec.CheckFakeCeiling)
            return;
        var waterZ = sec.CeilingWaterZ;
        if (oldZ <= waterZ && newZ > waterZ)
            ctx.TriggerSectorAction?.Invoke(SecspacEyesAboveC);
        else if (oldZ > waterZ && newZ <= waterZ)
            ctx.TriggerSectorAction?.Invoke(SecspacEyesBelowC);
    }

    public static void CheckFakeFloorTriggers(
        FakeFloorContext ctx,
        FakeFloorActor mo,
        FakeFloorSector sec,
        double oldZ,
        bool oldZHasViewheight,
        double actorHeight = 0)
    {
        if (ShouldSkipFakeFloorTriggers(mo))
            return;
        if (!sec.HasHeightSec || !sec.HasSecActTarget)
            return;

        var viewheight = ViewHeightForActor(mo, actorHeight);
        TriggerFeetHitFakeFloor(ctx, oldZ, mo.Z, sec.WaterZ);

        var newz = mo.Z + viewheight;
        if (!oldZHasViewheight)
            oldZ += viewheight;

        TriggerEyesFakeFloorTransitions(ctx, oldZ, newz, sec.WaterZ);
        TriggerFakeCeilingTransitions(ctx, sec, oldZ, newz);
    }

    public static void PlayerSquatView(Action? vmCall) => vmCall?.Invoke();

    public static void PlayerLandedMakeRumble(Action? vmCall) => vmCall?.Invoke();

    public static void PlayerDiedMakeRumble(Action? vmCall) => vmCall?.Invoke();

    public static void PlayerLandedMakeGruntSound(Action? vmCall) => vmCall?.Invoke();

    public static void PlayerLandedOnThing(
        Action? squatView,
        Action? fallingDamage,
        Action? makeRumble,
        Action? gruntSound,
        bool predicting)
    {
        PlayerSquatView(squatView);
        if (predicting)
            return;
        fallingDamage?.Invoke();
        PlayerLandedMakeRumble(makeRumble);
        PlayerLandedMakeGruntSound(gruntSound);
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
        public TidHashSlot? Slot { get; set; }
    }

    public sealed class TidHashSlot
    {
        public HashNode? Head { get; set; }
    }

    public static void AddToHash(HashNode self, int tid, TidHashSlot[] table)
    {
        if (tid == 0)
        {
            self.Next = null;
            self.PrevLink = null;
            return;
        }

        var slot = table[tid & 127];
        self.Slot = slot;
        self.Next = slot.Head;
        slot.Head = self;
        if (self.Next != null)
            self.Next.PrevLink = self;
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
        if (self.Tid == 0)
            return;
        var slot = self.Slot;
        if (slot != null && ReferenceEquals(slot.Head, self))
            slot.Head = self.Next;
        if (self.Next != null)
            self.Next.PrevLink = self.PrevLink;

        self.Tid = 0;
        self.Next = null;
        self.PrevLink = null;
        self.Slot = null;
    }

    public static bool ShouldSkipTidAssign(bool euthanizeMe) => euthanizeMe;

    public static void SetTid(HashNode self, int newTid, bool euthanizeMe, Action? remove, Action? add)
    {
        remove?.Invoke();
        if (ShouldSkipTidAssign(euthanizeMe))
            return;
        self.Tid = newTid;
        add?.Invoke();
    }

    public static void SetTid(HashNode self, int newTid, Action? remove, Action? add) =>
        SetTid(self, newTid, false, remove, add);
}
