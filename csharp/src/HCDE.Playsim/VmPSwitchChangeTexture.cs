namespace HCDE.Playsim;

/// <summary>
/// `P_ChangeSwitchTexture` and `DActiveButton` thinker from <c>p_switch.cpp</c> (243–310, 320–331, 380–452)
/// plus `FTextureAnimator::AddSwitchPair` from <c>anim_switches.cpp</c> (305–330).
/// </summary>
public static class VmPSwitchChangeTexture
{
    public const int ExitNormal = 0;
    public const int SideTop = 0;
    public const int SideMid = 1;
    public const int SideBottom = 2;
    public const int ButtonTime = 35;

    public sealed class SwitchDef
    {
        public int Sound { get; set; } = -1;
        public bool QuestPanel { get; set; }
        public int NumFrames { get; set; }
        public SwitchDef? PairDef { get; set; }
        public List<SwitchFrame> Frames { get; } = new();
    }

    public sealed class SwitchFrame
    {
        public int Texture { get; set; }
        public int TimeMin { get; set; }
        public int TimeRnd { get; set; }
    }

    public sealed class Side
    {
        public int TopTexture { get; set; }
        public int MidTexture { get; set; }
        public int BottomTexture { get; set; }
        public Action<int, int>? SetTexture { get; set; }
    }

    public sealed class ChangeContext
    {
        public Func<Side, SwitchDef?>? FindSwitch { get; init; }
        public Func<int, bool, bool>? StartButton { get; init; }
        public Func<bool, int>? FindExitSound { get; init; }
        public Func<int>? FindNormSound { get; init; }
        public Action<int>? PlaySound { get; init; }
        public Func<Side, (double x, double y)>? Midpoint { get; init; }
    }

    public static bool ChangeSwitchTexture(Side side, bool useAgain, int special, ChangeContext ctx, ref bool quest)
    {
        var sw = ctx.FindSwitch?.Invoke(side);
        if (sw == null)
        {
            quest = false;
            return false;
        }
        var part = side.TopTexture >= 0 && ctx.FindSwitch?.Invoke(side) == sw ? SideTop :
            SideMid;
        var texture = part;
        var sound = sw.Sound >= 0 ? sw.Sound : special is ExitNormal or 1 or 2 or 3
            ? ctx.FindExitSound?.Invoke(true) ?? -1
            : ctx.FindNormSound?.Invoke() ?? -1;
        side.SetTexture?.Invoke(texture, sw.Frames[0].Texture);
        var playsound = useAgain || sw.NumFrames > 1
            ? ctx.StartButton?.Invoke(texture, useAgain) != false
            : true;
        if (playsound && sound >= 0)
            ctx.PlaySound?.Invoke(sound);
        quest = sw.QuestPanel;
        return true;
    }

    public sealed class ActiveButton
    {
        public Side? Side { get; set; }
        public int Part { get; set; }
        public SwitchDef? SwitchDef { get; set; }
        public SwitchDef? ActiveDef { get; set; }
        public int Frame { get; set; } = -1;
        public int Timer { get; set; }
        public bool Flippable { get; set; }
        public bool Returning { get; set; }
        public bool Destroyed { get; set; }
        public Func<int, int>? RandomSwitchAnim { get; init; }
        public Action<int>? PlaySound { get; init; }
        public Action? Destroy { get; init; }
    }

    public static void ConstructButton(ActiveButton btn, Side side, int where, SwitchDef sw, bool useAgain)
    {
        btn.Side = side;
        btn.Part = where;
        btn.SwitchDef = sw;
        btn.Flippable = useAgain;
        btn.Returning = false;
        btn.Frame = -1;
        AdvanceFrame(btn);
    }

    public static void TickButton(ActiveButton btn)
    {
        if (btn.SwitchDef == null)
        {
            btn.Destroyed = true;
            btn.Destroy?.Invoke();
            return;
        }
        var def = btn.Returning ? btn.SwitchDef.PairDef : btn.SwitchDef;
        if (def == null)
            return;
        if (--btn.Timer != 0)
            return;
        if (btn.Frame == def.NumFrames - 1)
        {
            btn.Returning = true;
            def = btn.SwitchDef.PairDef;
            if (def != null)
            {
                btn.Frame = -1;
                if (def.Sound >= 0)
                    btn.PlaySound?.Invoke(def.Sound);
                btn.Flippable = false;
            }
            else
            {
                btn.Destroyed = true;
                btn.Destroy?.Invoke();
                return;
            }
        }
        var kill = AdvanceFrame(btn, def);
        btn.Side?.SetTexture?.Invoke(btn.Part, def.Frames[btn.Frame].Texture);
        if (kill)
        {
            btn.Destroyed = true;
            btn.Destroy?.Invoke();
        }
    }

    public static bool AdvanceFrame(ActiveButton btn, SwitchDef? defOverride = null)
    {
        var def = defOverride ?? (btn.Returning ? btn.SwitchDef?.PairDef : btn.SwitchDef);
        if (def == null)
            return true;
        if (++btn.Frame == def.NumFrames - 1)
            return btn.Flippable && (btn.Timer = ButtonTime) == 0;
        var frame = def.Frames[btn.Frame];
        btn.Timer = frame.TimeMin;
        if (frame.TimeRnd != 0)
            btn.Timer += btn.RandomSwitchAnim?.Invoke(frame.TimeRnd) ?? 0;
        return false;
    }

    public static void AddSwitchPair(Dictionary<int, SwitchDef> table, int offTex, int onTex, SwitchDef offDef, SwitchDef onDef)
    {
        table[offTex] = offDef;
        table[onTex] = onDef;
        offDef.PairDef = onDef;
        onDef.PairDef = offDef;
    }
}
