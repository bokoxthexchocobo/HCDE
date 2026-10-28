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

    public sealed class SetStateSeams
    {
        public Func<StateModel?, string>? StateName { get; init; }
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

    public static void SetState(
        VmPPSpriteLayers.PspriteModel psp,
        StateModel? newstate,
        bool pending,
        SetStateSeams? seams = null)
    {
        if (psp.Id == PspWeapon)
        {
            psp.WeaponState &= ~(WfWeaponready | WfWeaponreadyalt | WfWeaponbobbing | WfWeaponswitchok
                | WfWeaponreloadok | WfWeaponzoomok | WfUser1ok | WfUser2ok | WfUser3ok | WfUser4ok);
        }

        psp.ProcessPending = pending;
        var statelooplimit = 300000;

        do
        {
            if (--statelooplimit == 0)
            {
                seams?.LogError?.Invoke(
                    $"Infinite state loop in weapon state '{seams.StateName?.Invoke(psp.State!) ?? "?"}'");
                psp.State = null;
                seams?.Destroy?.Invoke();
                return;
            }

            if (newstate == null)
            {
                seams?.Destroy?.Invoke();
                return;
            }

            if ((newstate.UseFlags & (SufOverlay | SufWeapon)) == 0)
            {
                seams?.LogError?.Invoke(
                    $"State {seams?.StateName?.Invoke(newstate) ?? "?"} not flagged for use in overlays or weapons");
                psp.State = null;
                seams?.Destroy?.Invoke();
                return;
            }

            if ((newstate.UseFlags & SufWeapon) == 0 && psp.CallerIsWeapon)
            {
                seams?.LogError?.Invoke(
                    $"State {seams?.StateName?.Invoke(newstate) ?? "?"} not flagged for use in weapons");
                psp.State = null;
                seams?.Destroy?.Invoke();
                return;
            }

            psp.State = newstate;

            if (newstate.Sprite != SprFixed)
            {
                if (!newstate.SameFrame)
                    psp.Frame = newstate.Frame;
                if (newstate.Sprite != SprNochange)
                    psp.Sprite = newstate.Sprite;
            }

            psp.Tics = newstate.Tics;

            if ((psp.Flags & PspfCvarfast) != 0)
            {
                if (psp.SvFastWeapons == 2 && psp.Id == PspWeapon)
                    psp.Tics = newstate.ActionFunc == null ? 0 : 1;
                else if (psp.SvFastWeapons == 3)
                    psp.Tics = newstate.Tics != 0 ? 1 : 0;
                else if (psp.SvFastWeapons != 0)
                    psp.Tics = 1;
            }

            if (psp.Id != PspFlash)
            {
                if (newstate.Misc1 != 0)
                {
                    psp.OldX = psp.X = newstate.Misc1;
                }

                if (newstate.Misc2 != 0)
                {
                    psp.OldY = psp.Y = newstate.Misc2;
                }
            }

            if (psp.OwnerMo != null)
            {
                var actionSelf = psp.OwnerMo;
                var actionOwner = psp.Caller;
                if (newstate.ActionFunc != null && newstate.ActionUnsafe)
                {
                    seams?.LogError?.Invoke(
                        $"Unsafe state call in state {seams?.StateName?.Invoke(newstate) ?? "?"} to {newstate.ActionName} which accesses user variables. The action function has been removed from this state");
                    newstate.ActionFunc = null;
                }

                if (newstate.ActionFunc != null && psp.Caller != null && !psp.CallerIsWeapon
                    && seams?.MatchesCallerType?.Invoke(actionSelf, actionOwner, newstate, 0) == false
                    && seams?.MatchesCallerType?.Invoke(psp.Caller, psp.OwnerMo, newstate, 0) == true)
                {
                    actionSelf = psp.Caller;
                    actionOwner = psp.OwnerMo;
                }

                if (newstate.ActionFunc != null)
                {
                    var next = seams?.CallAction?.Invoke(actionSelf, actionOwner, newstate, psp.Id);
                    if (psp.Euthanize)
                        return;
                    if (next != null)
                    {
                        newstate = next;
                        psp.Tics = 0;
                        continue;
                    }

                    if (psp.State == null)
                    {
                        seams?.Destroy?.Invoke();
                        return;
                    }
                }
            }

            newstate = psp.State?.NextState;
        } while (psp.Tics == 0);

        if (psp.State != null)
        {
            seams?.DebugTrace?.Invoke(
                seams.StateName?.Invoke(psp.State) ?? "?",
                psp.Sprite,
                psp.Frame,
                psp.Tics,
                psp.Id,
                0);
        }
    }
}
