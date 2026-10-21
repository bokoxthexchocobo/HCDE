namespace HCDE.Playsim;

/// <summary><c>P_InterceptVector</c> and <c>P_LineOpening</c> from <c>p_maputl.cpp</c> (78–239).</summary>
public static class VmPMapUtlLineOpening
{
    public const int FfcfOnly3dfloors = 1;
    public const int FfcfNoportals = 1 << 1;
    public const int FfcfNodropoff = 1 << 2;
    public const int Ffcf3drestrict = 1 << 3;
    public const int MlPortalconnect = 1;
    public const int Ml3dmidtex = 1 << 1;
    public const double LineOpenMin = -3.402823466e38;
    public const double LineOpenMax = 3.402823466e38;

    public readonly record struct Divline(double X, double Y, double Dx, double Dy);

    public sealed class LineOpeningState
    {
        public double Top { get; set; }
        public double Bottom { get; set; }
        public double LowFloor { get; set; }
        public double Range { get; set; }
        public int LowFloorThroughPortal { get; set; }
        public bool AboveMidtex { get; set; }
        public bool TouchMidtex { get; set; }
        public object? TopSec { get; set; }
        public object? BottomSec { get; set; }
    }

    public static double InterceptVector(Divline v2, Divline v1)
    {
        var den = v1.Dy * v2.Dx - v1.Dx * v2.Dy;
        if (den == 0) return 0;
        var num = (v1.X - v2.X) * v1.Dy + (v2.Y - v1.Y) * v1.Dx;
        return num / den;
    }

    public static void LineOpening(
        LineOpeningState open,
        bool hasActor,
        bool twoSided,
        int lineFlags,
        double posX,
        double posY,
        double? refX,
        double? refY,
        int flags,
        double frontCeilZ,
        double backCeilZ,
        double frontFloorZ,
        double backFloorZ,
        bool frontCeilPortal,
        bool backCeilPortal,
        bool frontFloorPortal,
        bool backFloorPortal,
        bool frontFloorSlope,
        bool backFloorSlope,
        Func<double, double, double, double>? nextLowestFloorAt,
        Action? applyXFloors,
        Func<bool>? apply3dMidtex)
    {
        if ((flags & FfcfOnly3dfloors) == 0)
        {
            if (!twoSided)
            {
                open.Range = 0;
                return;
            }

            var fc = frontCeilPortal ? LineOpenMax : frontCeilZ;
            var bc = backCeilPortal ? LineOpenMax : backCeilZ;
            var ff = frontFloorPortal ? LineOpenMin : frontFloorZ;
            var bf = backFloorPortal ? LineOpenMin : backFloorZ;

            if ((lineFlags & MlPortalconnect) != 0 && (flags & FfcfNoportals) == 0)
            {
                if (frontCeilPortal) fc = LineOpenMax;
                if (backCeilPortal) bc = LineOpenMax;
                if (frontFloorPortal) ff = LineOpenMin;
                if (backFloorPortal) bf = LineOpenMin;
            }

            open.Top = fc < bc ? fc : bc;

            open.LowFloorThroughPortal = 0;
            if (ff == LineOpenMin) open.LowFloorThroughPortal |= 1;
            if (bf == LineOpenMin) open.LowFloorThroughPortal |= 2;

            bool useFront;
            if (ff == LineOpenMin || bf == LineOpenMin || refX is null || Math.Abs(ff - bf) > 1.0 / 256)
            {
                useFront = ff > bf;
            }
            else if (!frontFloorSlope)
            {
                useFront = true;
            }
            else if (!backFloorSlope)
            {
                useFront = false;
            }
            else
            {
                useFront = refY is not null && PointOnLineSide(refX.Value, refY.Value, posX, posY);
            }

            open.LowFloorThroughPortal = 0;
            if (useFront)
            {
                open.Bottom = ff;
                if (bf != LineOpenMin) open.LowFloor = bf;
                else if ((flags & FfcfNodropoff) == 0)
                    open.LowFloor = nextLowestFloorAt?.Invoke(posX, posY, backFloorZ) ?? LineOpenMax;
            }
            else
            {
                open.Bottom = bf;
                if (ff != LineOpenMin) open.LowFloor = ff;
                else if ((flags & FfcfNodropoff) == 0)
                    open.LowFloor = nextLowestFloorAt?.Invoke(posX, posY, frontFloorZ) ?? LineOpenMax;
            }
        }
        else
        {
            open.Top = LineOpenMax;
            open.Bottom = LineOpenMin;
            open.LowFloor = LineOpenMax;
            open.LowFloorThroughPortal = 0;
        }

        if (hasActor)
            applyXFloors?.Invoke();

        if (hasActor && twoSided && (lineFlags & Ml3dmidtex) != 0)
            open.TouchMidtex = apply3dMidtex?.Invoke() == true;
        else
            open.AboveMidtex = open.TouchMidtex = false;

        open.Range = Math.Clamp(open.Top - open.Bottom, LineOpenMin, LineOpenMax);
    }

    public static bool PointOnLineSide(double refX, double refY, double lineX, double lineY) =>
        (refX - lineX) * (refY - lineY) >= 0;
}
