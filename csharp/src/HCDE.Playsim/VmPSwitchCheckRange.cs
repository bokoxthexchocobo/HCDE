namespace HCDE.Playsim;

/// <summary>`P_CheckSwitchRange` and `P_StartButton` from <c>p_switch.cpp</c> (71–88, 98–234).</summary>
public static class VmPSwitchCheckRange
{
    public const int Ml3dmidtex = 1;
    public const int MlCheckswitchrange = 1 << 1;
    public const int Compatf2Checkswitchrange = 1;

    public sealed class User
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Top { get; set; }
        public double YawCos { get; set; }
        public double YawSin { get; set; }
        public int CompatFlags2 { get; set; }
    }

    public sealed class Line
    {
        public int Flags { get; set; }
        public bool OneSided { get; set; }
        public bool PolyobjWall { get; set; }
        public double FloorZ { get; set; }
        public double CeilingZ { get; set; }
        public double OpenTop { get; set; }
        public double OpenBottom { get; set; }
        public double OpenRange { get; set; }
        public bool TopIsSwitch { get; set; }
        public bool BottomIsSwitch { get; set; }
        public bool MidIsSwitch { get; set; }
    }

    public sealed class CheckContext
    {
        public Func<Line, int, (double x, double y)>? TouchPoint { get; init; }
        public Func<double, double, object?>? PointInSector { get; init; }
        public Func<Line, User, Line, bool>? TopSwitchReachable { get; init; }
        public Func<Line, User, Line, bool>? BottomSwitchReachable { get; init; }
        public Func<Line, int, double, double, bool>? MidtexReachable { get; init; }
    }

    public static int EffectiveSwitchFlags(Line line, bool hasMidTexture)
    {
        var flags = line.Flags;
        if (!hasMidTexture)
            flags &= ~Ml3dmidtex;
        return flags;
    }

    public static bool SwitchRangeChecksDisabled(int flags) =>
        (flags & (Ml3dmidtex | MlCheckswitchrange)) == 0;

    public static bool NullSideAllowsSwitch(object? side) => side == null;

    public static (double X, double Y) TouchPointOnLine(
        double lineX,
        double lineY,
        double lineDx,
        double lineDy,
        double intercept,
        bool polyobjWall) =>
        polyobjWall
            ? (lineX + lineDx * (intercept + 0.01), lineY + lineDy * (intercept + 0.01))
            : (lineX + lineDx * intercept, lineY + lineDy * intercept);

    public static bool TopSwitchReachableCompat(User user, double openTop, int compatFlags2) =>
        (compatFlags2 & Compatf2Checkswitchrange) != 0
            ? user.Top >= openTop
            : user.Top > openTop;

    public static bool BottomSwitchReachableCompat(User user, double openBottom, int compatFlags2) =>
        (compatFlags2 & Compatf2Checkswitchrange) != 0
            ? user.Z <= openBottom
            : user.Z < openBottom;

    public static bool MidtexVerticalReach(User user, double checkTop, double checkBot) =>
        user.Z < checkTop && user.Top > checkBot;

    public static void RepressActiveButton(ButtonThinker button) => button.Timer = 1;

    public static bool OneSidedSwitchReachable(User user, Line line) =>
        user.Top >= line.FloorZ && user.Z <= line.CeilingZ;

    public static bool OpenRangeCollapsed(User user, Line line) =>
        line.OpenRange <= 0 && OneSidedSwitchReachable(user, line);

    public static bool TryTopSwitchPath(User user, Line line, CheckContext ctx) =>
        line.TopIsSwitch && ctx.TopSwitchReachable?.Invoke(line, user, line) == true;

    public static bool TryBottomSwitchPath(User user, Line line, CheckContext ctx) =>
        line.BottomIsSwitch && ctx.BottomSwitchReachable?.Invoke(line, user, line) == true;

    public static bool TryMidtexSwitchPath(int flags, Line line, int sideno, User user, CheckContext ctx, double touchX, double touchY) =>
        (flags & Ml3dmidtex) != 0 || line.MidIsSwitch
            ? ctx.MidtexReachable?.Invoke(line, sideno, touchX, touchY) == true
            : false;

    public static bool FallbackOpenRangeReach(User user, Line line, bool compatMode) =>
        compatMode
            ? user.Top >= line.OpenTop || user.Z <= line.OpenBottom
            : user.Top > line.OpenTop || user.Z < line.OpenBottom;

    public static bool RunCheckSwitchRangePipeline(
        User user,
        Line line,
        int sideno,
        CheckContext ctx,
        bool hasMidTexture)
    {
        var flags = EffectiveSwitchFlags(line, hasMidTexture);
        if (SwitchRangeChecksDisabled(flags))
            return true;
        var pt = ctx.TouchPoint?.Invoke(line, sideno) ?? (user.X, user.Y);
        if (line.OneSided || line.PolyobjWall)
            return OneSidedSwitchReachable(user, line);
        if (line.OpenRange <= 0)
            return OneSidedSwitchReachable(user, line);
        if (line.TopIsSwitch)
        {
            if (TryTopSwitchPath(user, line, ctx))
                return true;
            if (TopSwitchReachableCompat(user, line.OpenTop, user.CompatFlags2))
                return true;
        }

        if (line.BottomIsSwitch)
        {
            if (TryBottomSwitchPath(user, line, ctx))
                return true;
            if (BottomSwitchReachableCompat(user, line.OpenBottom, user.CompatFlags2))
                return true;
        }
        if (TryMidtexSwitchPath(flags, line, sideno, user, ctx, pt.x, pt.y))
            return true;
        var compat = (user.CompatFlags2 & Compatf2Checkswitchrange) != 0;
        return FallbackOpenRangeReach(user, line, compat);
    }

    public static bool CheckSwitchRange(
        User user,
        Line line,
        int sideno,
        CheckContext ctx,
        bool hasMidTexture,
        object? side = null)
    {
        if (NullSideAllowsSwitch(side))
            return true;
        return RunCheckSwitchRangePipeline(user, line, sideno, ctx, hasMidTexture);
    }

    public sealed class ButtonThinker
    {
        public object? Side { get; set; }
        public int Timer { get; set; }
    }

    public static ButtonThinker? FindActiveButton(IEnumerable<ButtonThinker> active, object side)
    {
        foreach (var b in active)
        {
            if (ReferenceEquals(b.Side, side))
                return b;
        }

        return null;
    }

    public static bool StartButton(IEnumerable<ButtonThinker> active, object side, Action<object>? create)
    {
        var existing = FindActiveButton(active, side);
        if (existing != null)
        {
            RepressActiveButton(existing);
            return false;
        }

        create?.Invoke(side);
        return true;
    }
}
