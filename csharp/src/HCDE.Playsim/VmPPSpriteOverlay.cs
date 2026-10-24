namespace HCDE.Playsim;

public static class VmPPSpriteOverlay
{
    public const int WofKeepx = 1;
    public const int WofKeepy = 1 << 1;
    public const int WofAdd = 1 << 2;
    public const int WofInterpolate = 1 << 3;
    public const int WofRelative = 1 << 4;
    public const int WofZeroy = 1 << 5;

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

    public static void HandleOverlayRelative(OverlayPsprite psp, ref double x, ref double y)
    {
        var wx = x;
        var wy = y;
        var c = psp.RotationCos;
        var s = psp.RotationSin;
        x = wx * c + wy * s;
        y = wx * s - wy * c;
    }

    public static bool OverlayVertexOffset(OverlayPsprite? pspr, int index, double x, double y, int flags)
    {
        if (pspr == null || index < 0 || index > 3 || (flags & WofKeepx) != 0 && (flags & WofKeepy) != 0)
            return false;
        if ((flags & WofKeepx) == 0)
            pspr.Coord[index].X = (flags & WofAdd) != 0 ? pspr.Coord[index].X + x : x;
        if ((flags & WofKeepy) == 0)
            pspr.Coord[index].Y = (flags & WofAdd) != 0 ? pspr.Coord[index].Y + y : y;
        if ((flags & (WofAdd | WofInterpolate)) != 0)
            pspr.InterpolateTic = true;
        return true;
    }

    public static bool OverlayScale(OverlayPsprite? pspr, double wx, double wy, int flags)
    {
        if (pspr == null || (flags & WofKeepx) != 0 && (flags & WofKeepy) != 0)
            return false;
        if ((flags & WofZeroy) == 0 && wy == 0)
            wy = wx;
        if ((flags & WofKeepx) == 0)
            pspr.ScaleX = (flags & WofAdd) != 0 ? pspr.ScaleX + wx : wx;
        if ((flags & WofKeepy) == 0)
            pspr.ScaleY = (flags & WofAdd) != 0 ? pspr.ScaleY + wy : wy;
        if ((flags & (WofAdd | WofInterpolate)) != 0)
            pspr.InterpolateTic = true;
        return true;
    }

    public static bool OverlayRotate(OverlayPsprite? pspr, double degrees, int flags)
    {
        if (pspr == null)
            return false;
        pspr.RotationDegrees = (flags & WofAdd) != 0 ? pspr.RotationDegrees + degrees : degrees;
        if ((flags & (WofAdd | WofInterpolate)) != 0)
            pspr.InterpolateTic = true;
        return true;
    }

    public static bool OverlayPivot(OverlayPsprite? pspr, double wx, double wy, int flags)
    {
        if (pspr == null || (flags & WofKeepx) != 0 && (flags & WofKeepy) != 0)
            return false;
        if ((flags & WofRelative) != 0)
            HandleOverlayRelative(pspr, ref wx, ref wy);
        if ((flags & WofKeepx) == 0)
            pspr.PivotX = (flags & WofAdd) != 0 ? pspr.PivotX + wx : wx;
        if ((flags & WofKeepy) == 0)
            pspr.PivotY = (flags & WofAdd) != 0 ? pspr.PivotY + wy : wy;
        if ((flags & (WofAdd | WofInterpolate)) != 0)
            pspr.InterpolateTic = true;
        return true;
    }

    public const int PspWeapon = 1;
    public const int PspfInterpolate = 1 << 6;
    public const int PspaLeft = 0;
    public const int PspaRight = 2;
    public const int PspaTop = 0;
    public const int PspaBottom = 2;

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
        if (psp == null || (flags & WofKeepx) != 0 && (flags & WofKeepy) != 0)
            return;
        if ((flags & WofRelative) != 0)
            HandleOverlayRelative(psp, ref wx, ref wy);
        if ((flags & WofKeepx) == 0)
            psp.X = (flags & WofAdd) != 0 ? psp.X + wx : wx;
        if ((flags & WofKeepy) == 0)
            psp.Y = (flags & WofAdd) != 0 ? psp.Y + wy : wy;
        if ((flags & (WofAdd | WofInterpolate)) == 0)
            psp.ResetInterpolation();
    }
}
