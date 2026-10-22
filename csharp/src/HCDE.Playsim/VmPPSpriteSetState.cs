namespace HCDE.Playsim;

/// <summary>Faithful port of <c>DPSprite::SetState</c> (<c>p_pspr.cpp</c> 485–639).</summary>
public static class VmPPSpriteSetState
{
    public const int PspWeapon = 1;
    public const int PspFlash = 2;
    public const int SprFixed = -1;
    public const int SprNochange = -2;
    public const int SufOverlay = 1;
    public const int SufWeapon = 2;
    public const int PspfCvarfast = 1;

    public sealed class StateModel
    {
        public int UseFlags { get; set; }
        public int Sprite { get; set; }
        public bool SameFrame { get; set; }
        public int Frame { get; set; }
        public int Tics { get; set; }
        public object? ActionFunc { get; set; }
        public bool ActionUnsafe { get; set; }
        public string ActionName { get; set; } = "";
        public double Misc1 { get; set; }
        public double Misc2 { get; set; }
        public StateModel? NextState { get; set; }
        public string Name { get; set; } = "";
    }

    public sealed class SetStateContext
    {
        public int Id { get; set; }
        public int Flags { get; set; }
        public StateModel? State { get; set; }
        public int Sprite { get; set; }
        public int Frame { get; set; }
        public int Tics { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double OldX { get; set; }
        public double OldY { get; set; }
        public int WeaponState { get; set; }
        public bool ProcessPending { get; set; }
        public bool Euthanize { get; set; }
        public object? OwnerMo { get; set; }
        public object? Caller { get; set; }
        public bool CallerIsWeapon { get; set; }
        public int SvFastWeapons { get; set; }
        public Func<StateModel, string>? StateName { get; init; }
        public Action<string>? LogError { get; init; }
        public Action? Destroy { get; init; }
        public Func<object?, object?, StateModel, int, bool>? MatchesCallerType { get; init; }
        public Func<object?, object?, StateModel, int, StateModel?>? CallAction { get; init; }
        public Action<string, int, int, int, int, int>? DebugTrace { get; init; }
    }

    public const int WfWeaponready = 1;
    public const int WfWeaponreadyalt = 1 << 1;
    public const int WfWeaponbobbing = 1 << 2;
    public const int WfWeaponswitchok = 1 << 3;
    public const int WfWeaponreloadok = 1 << 4;
    public const int WfWeaponzoomok = 1 << 5;
    public const int WfUser1ok = 1 << 6;
    public const int WfUser2ok = 1 << 7;
    public const int WfUser3ok = 1 << 8;
    public const int WfUser4ok = 1 << 9;

    public static void SetState(SetStateContext ctx, StateModel? newstate, bool pending)
    {
        if (ctx.Id == PspWeapon)
        {
            ctx.WeaponState &= ~(WfWeaponready | WfWeaponreadyalt | WfWeaponbobbing | WfWeaponswitchok
                | WfWeaponreloadok | WfWeaponzoomok | WfUser1ok | WfUser2ok | WfUser3ok | WfUser4ok);
        }

        ctx.ProcessPending = pending;
        var statelooplimit = 300000;

        do
        {
            if (--statelooplimit == 0)
            {
                ctx.LogError?.Invoke($"Infinite state loop in weapon state '{ctx.StateName?.Invoke(ctx.State!) ?? "?"}'");
                ctx.State = null;
                ctx.Destroy?.Invoke();
                return;
            }

            if (newstate == null)
            {
                ctx.Destroy?.Invoke();
                return;
            }

            if ((newstate.UseFlags & (SufOverlay | SufWeapon)) == 0)
            {
                ctx.LogError?.Invoke($"State {ctx.StateName?.Invoke(newstate) ?? "?"} not flagged for use in overlays or weapons");
                ctx.State = null;
                ctx.Destroy?.Invoke();
                return;
            }

            if ((newstate.UseFlags & SufWeapon) == 0 && ctx.CallerIsWeapon)
            {
                ctx.LogError?.Invoke($"State {ctx.StateName?.Invoke(newstate) ?? "?"} not flagged for use in weapons");
                ctx.State = null;
                ctx.Destroy?.Invoke();
                return;
            }

            ctx.State = newstate;

            if (newstate.Sprite != SprFixed)
            {
                if (!newstate.SameFrame)
                    ctx.Frame = newstate.Frame;
                if (newstate.Sprite != SprNochange)
                    ctx.Sprite = newstate.Sprite;
            }

            ctx.Tics = newstate.Tics;

            if ((ctx.Flags & PspfCvarfast) != 0)
            {
                if (ctx.SvFastWeapons == 2 && ctx.Id == PspWeapon)
                    ctx.Tics = newstate.ActionFunc == null ? 0 : 1;
                else if (ctx.SvFastWeapons == 3)
                    ctx.Tics = newstate.Tics != 0 ? 1 : 0;
                else if (ctx.SvFastWeapons != 0)
                    ctx.Tics = 1;
            }

            if (ctx.Id != PspFlash)
            {
                if (newstate.Misc1 != 0)
                {
                    ctx.OldX = ctx.X = newstate.Misc1;
                }

                if (newstate.Misc2 != 0)
                {
                    ctx.OldY = ctx.Y = newstate.Misc2;
                }
            }

            if (ctx.OwnerMo != null)
            {
                var actionSelf = ctx.OwnerMo;
                var actionOwner = ctx.Caller;
                if (newstate.ActionFunc != null && newstate.ActionUnsafe)
                {
                    ctx.LogError?.Invoke(
                        $"Unsafe state call in state {ctx.StateName?.Invoke(newstate) ?? "?"} to {newstate.ActionName} which accesses user variables. The action function has been removed from this state");
                    newstate.ActionFunc = null;
                }

                if (newstate.ActionFunc != null && ctx.Caller != null && !ctx.CallerIsWeapon
                    && ctx.MatchesCallerType?.Invoke(actionSelf, actionOwner, newstate, 0) == false
                    && ctx.MatchesCallerType?.Invoke(ctx.Caller, ctx.OwnerMo, newstate, 0) == true)
                {
                    actionSelf = ctx.Caller;
                    actionOwner = ctx.OwnerMo;
                }

                if (newstate.ActionFunc != null)
                {
                    var next = ctx.CallAction?.Invoke(actionSelf, actionOwner, newstate, ctx.Id);
                    if (ctx.Euthanize)
                        return;
                    if (next != null)
                    {
                        newstate = next;
                        ctx.Tics = 0;
                        continue;
                    }

                    if (ctx.State == null)
                    {
                        ctx.Destroy?.Invoke();
                        return;
                    }
                }
            }

            newstate = ctx.State?.NextState;
        } while (ctx.Tics == 0);

        if (ctx.State != null)
        {
            ctx.DebugTrace?.Invoke(
                ctx.StateName?.Invoke(ctx.State) ?? "?",
                ctx.Sprite,
                ctx.Frame,
                ctx.Tics,
                ctx.Id,
                0);
        }
    }
}
