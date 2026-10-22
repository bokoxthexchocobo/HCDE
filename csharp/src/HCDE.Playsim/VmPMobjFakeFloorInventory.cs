namespace HCDE.Playsim;

/// <summary>Fake-floor triggers, landing hooks, inventory find/give, and CallFallAndSink from <c>p_mobj.cpp</c> (1064–1131, 3214–3454).</summary>
public static class VmPMobjFakeFloorInventory
{
    public const int CfPredicting = 1;
    public const int SecmfFakeflooronly = 1;
    public const int SecspacHitFakeFloor = 1;
    public const int SecspacEyesSurface = 2;
    public const int SecspacEyesDive = 3;
    public const int SecspacEyesAboveC = 4;
    public const int SecspacEyesBelowC = 5;

    public sealed class FakeFloorActor
    {
        public bool HasPlayer { get; set; }
        public int PlayerCheats { get; set; }
        public double Z { get; set; }
        public double Height { get; set; }
        public double ViewHeight { get; set; }
    }

    public sealed class FakeFloorSector
    {
        public bool HasHeightSec { get; set; }
        public bool HasSecActTarget { get; set; }
        public double WaterFloorZ { get; set; }
        public double WaterCeilingZ { get; set; }
        public int HeightSecFlags { get; set; }
    }

    public sealed class FakeFloorContext
    {
        public Action<FakeFloorActor, int>? TriggerSectorAction { get; init; }
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

        var viewheight = mo.HasPlayer ? mo.ViewHeight : mo.Height;

        if (oldZ > sec.WaterFloorZ && mo.Z <= sec.WaterFloorZ)
            ctx.TriggerSectorAction?.Invoke(mo, SecspacHitFakeFloor);

        var newZ = mo.Z + viewheight;
        if (!oldZHasViewheight)
            oldZ += viewheight;

        if (oldZ <= sec.WaterFloorZ && newZ > sec.WaterFloorZ)
            ctx.TriggerSectorAction?.Invoke(mo, SecspacEyesSurface);
        else if (oldZ > sec.WaterFloorZ && newZ <= sec.WaterFloorZ)
            ctx.TriggerSectorAction?.Invoke(mo, SecspacEyesDive);

        if ((sec.HeightSecFlags & SecmfFakeflooronly) != 0)
            return;

        if (oldZ <= sec.WaterCeilingZ && newZ > sec.WaterCeilingZ)
            ctx.TriggerSectorAction?.Invoke(mo, SecspacEyesAboveC);
        else if (oldZ > sec.WaterCeilingZ && newZ <= sec.WaterCeilingZ)
            ctx.TriggerSectorAction?.Invoke(mo, SecspacEyesBelowC);
    }

    public static void CheckFakeFloorTriggersAction(
        FakeFloorContext ctx,
        FakeFloorActor mo,
        FakeFloorSector sec,
        double oldZ,
        bool oldZHasViewheight) =>
        CheckFakeFloorTriggers(ctx, mo, sec, oldZ, oldZHasViewheight);

    public sealed class LandContext
    {
        public Action<object?, object?>? SquatView { get; init; }
        public Action<object?, object?>? FallingDamage { get; init; }
        public Action<object?, object?>? MakeRumble { get; init; }
        public Action<object?, object?>? MakeGruntSound { get; init; }
    }

    public static void PlayerLandedOnThing(LandContext ctx, object mo, object onMobj, int playerCheats)
    {
        ctx.SquatView?.Invoke(mo, onMobj);
        if ((playerCheats & CfPredicting) != 0)
            return;
        ctx.FallingDamage?.Invoke(mo, onMobj);
        ctx.MakeRumble?.Invoke(mo, onMobj);
        ctx.MakeGruntSound?.Invoke(mo, onMobj);
    }

    public sealed class FallSinkContext
    {
        public Func<object?, double, double, bool>? VmFallAndSink { get; init; }
        public Action<object?, double, double>? NativeFallAndSink { get; init; }
    }

    public static void CallFallAndSink(FallSinkContext ctx, object actor, double grav, double oldFloorZ)
    {
        if (ctx.VmFallAndSink?.Invoke(actor, grav, oldFloorZ) == true)
            return;
        ctx.NativeFallAndSink?.Invoke(actor, grav, oldFloorZ);
    }

    public sealed class InventoryItem
    {
        public string ClassName { get; set; } = "";
        public bool IsKindOf { get; set; }
        public InventoryItem? Next { get; set; }
    }

    public static InventoryItem? FindInventory(InventoryItem? head, string? typeName, bool subclass)
    {
        if (typeName == null)
            return null;
        for (var item = head; item != null; item = item.Next)
        {
            if (!subclass)
            {
                if (item.ClassName == typeName)
                    return item;
            }
            else if (item.IsKindOf || item.ClassName == typeName)
            {
                return item;
            }
        }

        return null;
    }

    public static InventoryItem? FindInventoryAction(InventoryItem? head, string? typeName, bool subclass) =>
        FindInventory(head, typeName, subclass);

    public sealed class GiveInventoryContext
    {
        public Func<string?, object?>? SpawnItem { get; init; }
        public Func<object?, object?, bool>? TryPickup { get; init; }
        public Action<object?>? DestroyItem { get; init; }
    }

    public static object? GiveInventoryType(GiveInventoryContext ctx, object actor, string? typeName)
    {
        if (typeName == null)
            return null;
        var item = ctx.SpawnItem?.Invoke(typeName);
        if (item == null)
            return null;
        if (ctx.TryPickup?.Invoke(item, actor) != true)
        {
            ctx.DestroyItem?.Invoke(item);
            return null;
        }

        return item;
    }

    public static object? GiveInventoryTypeAction(GiveInventoryContext ctx, object actor, string? typeName) =>
        GiveInventoryType(ctx, actor, typeName);
}
