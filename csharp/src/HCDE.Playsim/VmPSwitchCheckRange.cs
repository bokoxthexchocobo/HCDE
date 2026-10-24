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

    public static bool CheckSwitchRange(User user, Line line, int sideno, CheckContext ctx, bool hasMidTexture)
    {
        if ((line.Flags & (Ml3dmidtex | MlCheckswitchrange)) == 0)
            return true;
        var flags = line.Flags;
        if (!hasMidTexture)
            flags &= ~Ml3dmidtex;
        var pt = ctx.TouchPoint?.Invoke(line, sideno) ?? (user.X, user.Y);
        if (line.OneSided || line.PolyobjWall)
            return user.Top >= line.FloorZ && user.Z <= line.CeilingZ;
        if (line.OpenRange <= 0)
            return user.Top >= line.FloorZ && user.Z <= line.CeilingZ;
        if (line.TopIsSwitch && ctx.TopSwitchReachable?.Invoke(line, user, line) == true)
            return true;
        if (line.BottomIsSwitch && ctx.BottomSwitchReachable?.Invoke(line, user, line) == true)
            return true;
        if ((flags & Ml3dmidtex) != 0 || line.MidIsSwitch)
            return ctx.MidtexReachable?.Invoke(line, sideno, pt.x, pt.y) == true;
        var compat = (user.CompatFlags2 & Compatf2Checkswitchrange) != 0;
        return compat
            ? user.Top >= line.OpenTop || user.Z <= line.OpenBottom
            : user.Top > line.OpenTop || user.Z < line.OpenBottom;
    }

    public sealed class ButtonThinker
    {
        public object? Side { get; set; }
        public int Timer { get; set; }
    }

    public static bool StartButton(IEnumerable<ButtonThinker> active, object side, Action<object>? create)
    {
        foreach (var b in active)
        {
            if (ReferenceEquals(b.Side, side))
            {
                b.Timer = 1;
                return false;
            }
        }
        create?.Invoke(side);
        return true;
    }
}
