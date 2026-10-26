namespace HCDE.Playsim;

/// <summary>Psprite overlay WOF actions from <c>p_pspr.cpp</c> (835–1084).</summary>
public static class VmPPSpriteOverlay
{
    public const int WofKeepx = 1;
    public const int WofKeepy = 1 << 1;
    public const int WofAdd = 1 << 2;
    public const int WofInterpolate = 1 << 3;
    public const int WofRelative = 1 << 4;
    public const int WofZeroy = 1 << 5;

    public const int PspWeapon = 1;
    public const int PspfInterpolate = 1 << 6;
    public const int PspaLeft = 0;
    public const int PspaRight = 2;
    public const int PspaTop = 0;
    public const int PspaBottom = 2;

    public sealed class OverlayPsprite
    {
        public double RotationCos { get; set; } = 1;
        public double RotationSin { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double ScaleX { get; set; } = 1;
        public double ScaleY { get; set; } = 1;
        public double PivotX { get; set; }
        public double PivotY { get; set; }
        public double RotationDegrees { get; set; }
        public bool InterpolateTic { get; set; }
        public int Flags { get; set; }
        public int HAlign { get; set; }
        public int VAlign { get; set; }
        public CoordSlot[] Coord { get; } = [new(), new(), new(), new()];
        public void ResetInterpolation() => InterpolateTic = false;
    }

    public sealed class CoordSlot
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public static bool BothAxesKept(int flags) => (flags & WofKeepx) != 0 && (flags & WofKeepy) != 0;

    public static void SyncRotationTrig(OverlayPsprite psp)
    {
        var rad = psp.RotationDegrees * (Math.PI / 180.0);
        psp.RotationCos = Math.Cos(rad);
        psp.RotationSin = Math.Sin(rad);
    }

    public static void HandleOverlayRelative(OverlayPsprite psp, ref double x, ref double y)
    {
        var wx = x;
        var wy = y;
        var c = psp.RotationCos;
        var s = psp.RotationSin;
        x = wx * c + wy * s;
        y = wx * s - wy * c;
    }

    static double ApplyAxis(double current, double incoming, int flags, bool axisX)
    {
        var keep = axisX ? (flags & WofKeepx) != 0 : (flags & WofKeepy) != 0;
        if (keep)
            return current;
        return (flags & WofAdd) != 0 ? current + incoming : incoming;
    }

    static void MarkInterpolationIfNeeded(OverlayPsprite pspr, int flags)
    {
        if ((flags & (WofAdd | WofInterpolate)) != 0)
            pspr.InterpolateTic = true;
    }

    public static bool OverlayVertexOffset(OverlayPsprite? pspr, int index, double x, double y, int flags)
    {
        if (pspr == null || index < 0 || index > 3 || BothAxesKept(flags))
            return false;
        pspr.Coord[index].X = ApplyAxis(pspr.Coord[index].X, x, flags, axisX: true);
        pspr.Coord[index].Y = ApplyAxis(pspr.Coord[index].Y, y, flags, axisX: false);
        MarkInterpolationIfNeeded(pspr, flags);
        return true;
    }

    public static bool OverlayScale(OverlayPsprite? pspr, double wx, double wy, int flags)
    {
        if (pspr == null || BothAxesKept(flags))
            return false;
        if ((flags & WofZeroy) == 0 && wy == 0)
            wy = wx;
        pspr.ScaleX = ApplyAxis(pspr.ScaleX, wx, flags, axisX: true);
        pspr.ScaleY = ApplyAxis(pspr.ScaleY, wy, flags, axisX: false);
        MarkInterpolationIfNeeded(pspr, flags);
        return true;
    }

    public static bool OverlayRotate(OverlayPsprite? pspr, double degrees, int flags)
    {
        if (pspr == null)
            return false;
        pspr.RotationDegrees = (flags & WofAdd) != 0 ? pspr.RotationDegrees + degrees : degrees;
        SyncRotationTrig(pspr);
        MarkInterpolationIfNeeded(pspr, flags);
        return true;
    }

    public static bool OverlayPivot(OverlayPsprite? pspr, double wx, double wy, int flags)
    {
        if (pspr == null || BothAxesKept(flags))
            return false;
        if ((flags & WofRelative) != 0)
            HandleOverlayRelative(pspr, ref wx, ref wy);
        pspr.PivotX = ApplyAxis(pspr.PivotX, wx, flags, axisX: true);
        pspr.PivotY = ApplyAxis(pspr.PivotY, wy, flags, axisX: false);
        MarkInterpolationIfNeeded(pspr, flags);
        return true;
    }

    public static void WeaponOffsetAction(OverlayPsprite? psp, double wx, double wy, int flags) =>
        OverlayOffset(psp, wx, wy, flags);

    public static bool OverlayFlags(OverlayPsprite? pspr, int flags, bool set)
    {
        if (pspr == null)
            return false;
        if (set)
            pspr.Flags |= flags;
        else
        {
            pspr.Flags &= ~flags;
            if ((flags & PspfInterpolate) != 0)
                pspr.ResetInterpolation();
        }
        return true;
    }

    public static bool OverlayPivotAlign(OverlayPsprite? pspr, int halign, int valign)
    {
        if (pspr == null)
            return false;
        if (halign >= PspaLeft && halign <= PspaRight)
            pspr.HAlign = halign;
        if (valign >= PspaTop && valign <= PspaBottom)
            pspr.VAlign = valign;
        return true;
    }

    public static void OverlayOffset(OverlayPsprite? psp, double wx, double wy, int flags)
    {
        if (psp == null || BothAxesKept(flags))
            return;
        if ((flags & WofRelative) != 0)
            HandleOverlayRelative(psp, ref wx, ref wy);
        psp.X = ApplyAxis(psp.X, wx, flags, axisX: true);
        psp.Y = ApplyAxis(psp.Y, wy, flags, axisX: false);
        if ((flags & (WofAdd | WofInterpolate)) == 0)
            psp.ResetInterpolation();
        else
            MarkInterpolationIfNeeded(psp, flags);
    }
}
