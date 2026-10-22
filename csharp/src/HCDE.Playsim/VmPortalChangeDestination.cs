namespace HCDE.Playsim;

/// <summary><c>ChangePortalLine</c>, <c>ChangePortal</c>, <c>P_GetLineSide</c>, and <c>P_ClipLineToPortal</c> from <c>portal.cpp</c> (330–433).</summary>
public static class VmPortalChangeDestination
{
    public const double EqualEpsilon = 1.0 / 65536;
    public const int PorttLinked = 2;
    public const int PorttInteractive = 3;
    public const int PortfInteractive = 4;
    public const int PortfPolyobj = 1;
    public const int WallfPolyobj = 1;

    public sealed class LinePortalPort
    {
        public int Type { get; set; }
        public int Flags { get; set; }
        public int DefFlags { get; set; }
        public int? DestinationLineIndex { get; set; }
        public double SinRot { get; set; }
        public double CosRot { get; set; } = 1;
        public double AngleDiffDegrees { get; set; }
        public double OriginDeltaAngleDeg { get; set; }
        public double DestDeltaAngleDeg { get; set; }
        public bool OriginPolyobj { get; set; }
        public bool DestPolyobj { get; set; }
    }

    public static int GetLineSide(double posX, double posY, double v1X, double v1Y, double deltaX, double deltaY)
    {
        var v = (posY - v1Y) * deltaX + (v1X - posX) * deltaY;
        if (v < -EqualEpsilon)
            return -1;
        if (v > EqualEpsilon)
            return 1;
        return 0;
    }

    public static bool ClipLineToPortal(
        double lineV1X, double lineV1Y, double lineV2X, double lineV2Y,
        double portalV1X, double portalV1Y, double portalV2X, double portalV2Y,
        double portalDeltaX, double portalDeltaY,
        double viewX, double viewY)
    {
        var behind1 = GetLineSide(lineV1X, lineV1Y, portalV1X, portalV1Y, portalDeltaX, portalDeltaY);
        var behind2 = GetLineSide(lineV2X, lineV2Y, portalV1X, portalV1Y, portalDeltaX, portalDeltaY);

        if (behind1 == 0 && behind2 == 0)
            return true;
        if (behind2 == 0)
            behind2 = behind1;
        else if (behind1 == 0)
            behind1 = behind2;

        if (behind1 > 0 && behind2 > 0)
            return true;
        if (behind1 < 0 && behind2 < 0)
            return false;

        var lineDx = lineV2X - lineV1X;
        var lineDy = lineV2Y - lineV1Y;
        var viewside = GetLineSide(viewX, viewY, lineV1X, lineV1Y, lineDx, lineDy);
        var p1side = GetLineSide(portalV1X, portalV1Y, lineV1X, lineV1Y, lineDx, lineDy);
        var p2side = GetLineSide(portalV2X, portalV2Y, lineV1X, lineV1Y, lineDx, lineDy);
        if (p1side == 0)
            p1side = p2side;
        else if (p2side == 0)
            p2side = p1side;
        return p1side == p2side && viewside != p1side;
    }

    public static void SetPortalRotation(LinePortalPort port)
    {
        if (port.DestinationLineIndex is null)
            return;
        if (port.Type != PorttLinked)
        {
            var angleDeg = port.DestDeltaAngleDeg - port.OriginDeltaAngleDeg + 180;
            var rad = angleDeg * Math.PI / 180;
            port.SinRot = Math.Sin(rad);
            port.CosRot = Math.Cos(rad);
            port.AngleDiffDegrees = angleDeg;
            if (port.OriginPolyobj || port.DestPolyobj)
                port.Flags |= PortfPolyobj;
            else
                port.Flags &= ~PortfPolyobj;
        }
        else
        {
            port.SinRot = 0;
            port.CosRot = 1;
            port.AngleDiffDegrees = 0;
        }
    }

    public static bool ChangePortalLine(
        LinePortalPort port,
        int sourceLineIndex,
        int destId,
        int linePortalCount,
        Func<int, int, int?>? findPortalDestinationLine,
        Func<int, LinePortalPort?>? portalForLine,
        Func<int, (double OriginAngleDeg, double DestAngleDeg, bool OriginPoly, bool DestPoly)>? lineAngles)
    {
        if (sourceLineIndex >= linePortalCount)
            return false;
        if (port.Type == PorttLinked)
            return false;

        port.DestinationLineIndex = findPortalDestinationLine?.Invoke(sourceLineIndex, destId);

        if (port.DestinationLineIndex is null)
            port.Flags = 0;
        else if (port.Type == PorttInteractive)
        {
            var portd = portalForLine?.Invoke(port.DestinationLineIndex.Value);
            if (portd is not null && portd.Type == PorttInteractive && portd.DestinationLineIndex == sourceLineIndex)
            {
                port.Flags = port.DefFlags | PortfInteractive;
                portd.Flags = portd.DefFlags | PortfInteractive;
            }
            else
                port.Flags = port.DefFlags;

            if (portd is not null && lineAngles?.Invoke(port.DestinationLineIndex.Value) is { } angd)
            {
                portd.DestDeltaAngleDeg = angd.DestAngleDeg;
                portd.OriginDeltaAngleDeg = angd.OriginAngleDeg;
                portd.DestPolyobj = angd.DestPoly;
                portd.OriginPolyobj = angd.OriginPoly;
                SetPortalRotation(portd);
            }
        }
        else
            port.Flags = port.DefFlags;

        if (lineAngles?.Invoke(sourceLineIndex) is { } ang)
        {
            port.OriginDeltaAngleDeg = ang.OriginAngleDeg;
            port.DestDeltaAngleDeg = ang.DestAngleDeg;
            port.OriginPolyobj = ang.OriginPoly;
            port.DestPolyobj = ang.DestPoly;
        }

        SetPortalRotation(port);
        return true;
    }

    public static bool ChangePortal(
        int thisId,
        int destId,
        int sourceLineIndex,
        VmPortalBuildBlockmap.LineIdIterator lineIdIterator,
        Func<int, bool> changePortalLineForLine)
    {
        if (thisId == 0)
            return changePortalLineForLine(sourceLineIndex);
        var res = false;
        int lineno;
        while ((lineno = lineIdIterator.Next()) >= 0)
            res |= changePortalLineForLine(lineno);
        return res;
    }
}
