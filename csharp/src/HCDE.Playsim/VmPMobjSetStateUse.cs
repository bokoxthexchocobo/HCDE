namespace HCDE.Playsim;

/// <summary><c>SetState</c>, `DestroyAllInventory`, and `UseInventory` from <c>p_mobj.cpp</c> (856–1035).</summary>
public static class VmPMobjSetStateUse
{
    public const int SufActor = 1;
    public const int LoopLimit = 300_000;
    public const int SprFixed = -1;
    public const int SprNochange = -2;

    public sealed class StateNode
    {
        public int UseFlags { get; set; }
        public int Sprite { get; set; }
        public int Frame { get; set; }
        public bool SameFrame { get; set; }
        public int Tics { get; set; }
        public bool HasAction { get; set; }
        public StateNode? Next { get; set; }
        public string Name { get; set; } = "State";
    }

    public sealed class SetStateActor
    {
        public StateNode? State { get; set; }
        public int Tics { get; set; }
        public int Sprite { get; set; }
        public int Frame { get; set; }
        public int RenderFlags { get; set; }
        public int Flags4 { get; set; }
        public int Flags8 { get; set; }
        public bool HasPlayer { get; set; }
        public bool Destroyed { get; set; }
        public bool Euthanize { get; set; }
    }

    public sealed class SetStateContext
    {
        public Action<string, string>? LogInfiniteLoop { get; init; }
        public Action<string, string>? LogBadState { get; init; }
        public Func<StateNode, SetStateActor, StateNode?, bool>? CallAction { get; init; }
        public Func<StateNode, int>? GetTics { get; init; }
        public Func<SetStateActor, int, int>? PickSkinSprite { get; init; }
        public Action<SetStateActor>? Destroy { get; init; }
        public Action<SetStateActor>? RecreateLights { get; init; }
    }

    public static bool SetState(SetStateContext ctx, SetStateActor actor, StateNode? newState, bool noFunction)
    {
        var loop = LoopLimit;
        while (true)
        {
            if (--loop <= 0)
            {
                ctx.LogInfiniteLoop?.Invoke("Actor", newState?.Name ?? "null");
                actor.State = null;
                ctx.Destroy?.Invoke(actor);
                return false;
            }

            if (newState == null)
            {
                actor.State = null;
                ctx.Destroy?.Invoke(actor);
                return false;
            }

            if ((newState.UseFlags & SufActor) == 0)
            {
                ctx.LogBadState?.Invoke(newState.Name, newState.Name);
                actor.State = null;
                ctx.Destroy?.Invoke(actor);
                return false;
            }

            var prevSprite = actor.State?.Sprite ?? -1;
            actor.State = newState;
            actor.Tics = ctx.GetTics?.Invoke(newState) ?? newState.Tics;
            var newsprite = newState.Sprite;
            if (newsprite != SprFixed)
            {
                if (!newState.SameFrame)
                    actor.Frame = newState.Frame;
                if (newsprite != SprNochange)
                    actor.Sprite = newsprite != prevSprite ? newsprite : actor.Sprite;
            }

            StateNode? jump = null;
            if (!noFunction && newState.HasAction)
            {
                if (ctx.CallAction?.Invoke(newState, actor, null) == true)
                {
                    if (actor.Euthanize)
                        return false;
                    jump = newState.Next;
                    actor.Tics = 0;
                }
            }

            if (jump == null)
                newState = newState.Next;
            else
                newState = jump;

            if (actor.Tics != 0)
                break;
        }

        ctx.RecreateLights?.Invoke(actor);
        return !actor.Destroyed;
    }

    public static void DestroyAllInventory(List<object> inventory, Action<object>? destroyItem)
    {
        var toDelete = new List<object>(inventory);
        inventory.Clear();
        foreach (var item in toDelete)
            destroyItem?.Invoke(item);
    }

    public static bool UseInventory(bool hasVirtual, Func<bool>? vmCall)
    {
        if (hasVirtual)
            return vmCall?.Invoke() ?? false;
        return false;
    }

    public static bool SetStateAction(SetStateContext ctx, SetStateActor actor, StateNode? state, bool noFunction) =>
        SetState(ctx, actor, state, noFunction);

    public static void DestroyAllInventoryAction(List<object> inventory, Action<object>? destroyItem) =>
        DestroyAllInventory(inventory, destroyItem);
}
