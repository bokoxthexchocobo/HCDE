namespace HCDE.Playsim;

/// <summary><c>PIT_CheckPortal</c> from <c>p_map.cpp</c> (1134–1206).</summary>
public static class VmPMapPitCheckPortal
{
    public const int FfcfNoceiling = 2;
    public const int FfcfNofloor = 1;

    public sealed class CheckPositionTm
    {
        public double CeilingZ { get; set; }
        public double FloorZ { get; set; }
        public double DropoffZ { get; set; }
        public int CeilingPic { get; set; }
        public int FloorPic { get; set; }
        public int FloorTerrain { get; set; }
    }

    public sealed class ThingState
    {
        public double Radius { get; set; }
        public double Z { get; set; }
        public Action<double>? AddZ { get; init; }
    }

    public sealed class PortalCheckContext
    {
        public int PortalFlags { get; init; }
        public double PosX { get; init; }
        public double PosY { get; init; }
        public double PosZ { get; init; }
        public int LineIndex { get; init; }
        public bool HasBacksector { get; init; }
        public Func<int, bool>? InRange { get; init; }
        public Func<double, double, double, int, int>? BoxOnLineSide { get; init; }
        public Func<int, int?>? PortalDestinationLine { get; init; }
        public Action<int, PortalTranslateCoords>? TranslatePortalXy { get; init; }
        public Action<int, PortalTranslateCoords>? TranslatePortalZ { get; init; }
        public Func<int, IEnumerable<int>>? LinesInBox { get; init; }
        public Func<int, double, double, double, LineOpening>? LineOpening { get; init; }
        public Func<int, double, double, (double X, double Y)>? FindRefPoint { get; init; }
        public Action<int, bool?>? SetBacksector { get; init; }
        public Func<int, bool>? LineHasBacksector { get; init; }
    }

    public readonly record struct LineOpening(double Top, double Bottom, double LowFloor, int CeilingPic, int FloorPic, int FloorTerrain);

    public sealed class PortalTranslateCoords
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }

    public static bool PitCheckPortal(PortalCheckContext cres, CheckPositionTm tm, ThingState thing)
    {
        if ((cres.PortalFlags & (FfcfNoceiling | FfcfNofloor)) != 0)
            return false;

        if (cres.InRange?.Invoke(cres.LineIndex) != true ||
            cres.BoxOnLineSide?.Invoke(thing.Radius, cres.PosX, cres.PosY, cres.LineIndex) != -1)
            return false;

        var destLine = cres.PortalDestinationLine?.Invoke(cres.LineIndex);
        if (destLine is null)
            return false;

        var coords = new PortalTranslateCoords { X = cres.PosX, Y = cres.PosY };
        cres.TranslatePortalXy?.Invoke(cres.LineIndex, coords);
        cres.TranslatePortalZ?.Invoke(cres.LineIndex, coords);
        var x = coords.X;
        var y = coords.Y;
        var zofs = coords.Z;

        var hadBack = cres.LineHasBacksector?.Invoke(destLine.Value) == true;
        if (!hadBack)
            cres.SetBacksector?.Invoke(destLine.Value, true);

        thing.AddZ?.Invoke(zofs);

        var ret = false;
        foreach (var ld in cres.LinesInBox?.Invoke(destLine.Value) ?? Array.Empty<int>())
        {
            if (cres.InRange?.Invoke(ld) != true ||
                cres.BoxOnLineSide?.Invoke(thing.Radius, x, y, ld) != -1)
                continue;

            if (cres.LineHasBacksector?.Invoke(ld) != true)
                continue;

            var refPt = cres.FindRefPoint?.Invoke(ld, x, y) ?? (x, y);
            var open = cres.LineOpening?.Invoke(ld, refPt.X, refPt.Y, cres.PosZ) ??
                       new LineOpening(double.MaxValue, double.MinValue, double.MaxValue, 0, 0, 0);

            if (open.Top - zofs < tm.CeilingZ)
            {
                tm.CeilingZ = open.Top - zofs;
                tm.CeilingPic = open.CeilingPic;
                ret = true;
            }

            if (open.Bottom - zofs > tm.FloorZ)
            {
                tm.FloorZ = open.Bottom - zofs;
                tm.FloorPic = open.FloorPic;
                tm.FloorTerrain = open.FloorTerrain;
                ret = true;
            }

            if (open.LowFloor - zofs < tm.DropoffZ)
                tm.DropoffZ = open.LowFloor - zofs;
        }

        thing.AddZ?.Invoke(-zofs);
        cres.SetBacksector?.Invoke(destLine.Value, hadBack ? true : null);
        return ret;
    }
}
