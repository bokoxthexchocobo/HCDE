namespace HCDE.Playsim;

/// <summary><c>FLevelLocals::EV_SilentLineTeleport</c> from <c>p_teleport.cpp</c> (471–643).</summary>
public static class VmPTeleportSilentLine
{
    public const int Mf2Noteleport = 0x08000000;
    public const double EqualEpsilon = 1.0 / 65536;
    public const int FudgeFactor = 10;

    public sealed class SilentLineTeleportState
    {
        public int Side { get; set; }
        public int Flags2 { get; set; }
        public bool LineNull { get; set; }
        public bool BackSidedefNull { get; set; }
        public int MatchLineIndex { get; set; }
        public int CurrentLineIndex { get; set; }
        public bool Reverse { get; set; }
        public double ThingX { get; set; }
        public double ThingY { get; set; }
        public double ThingZ { get; set; }
        public double ThingFloorZ { get; set; }
        public double VelX { get; set; }
        public double VelY { get; set; }
        public double YawDegrees { get; set; }
        public bool HasPlayerMo { get; set; }
        public double PlayerVelX { get; set; }
        public double PlayerVelY { get; set; }
        public double DeltaViewHeight { get; set; }
        public double ViewHeight { get; set; }
        public Func<int, IEnumerable<int>>? LineIdIterator { get; init; }
        public Func<int, SilentLineCandidate?>? GetCandidate { get; init; }
        public Func<double, double, int, bool>? PointOnLineSide { get; init; }
        public Func<double, double, double, bool>? TeleportMove { get; init; }
        public Action? CalcHeight { get; init; }
    }

    public sealed class SilentLineCandidate
    {
        public bool HasBacksector { get; set; } = true;
        public double DeltaAngleDegrees { get; set; }
        public double V1X { get; set; }
        public double V1Y { get; set; }
        public double DeltaX { get; set; }
        public double DeltaY { get; set; }
        public double SrcDeltaX { get; set; }
        public double SrcDeltaY { get; set; }
        public double SrcV1X { get; set; }
        public double SrcV1Y { get; set; }
        public double FrontFloorZ { get; set; }
        public double BackFloorZ { get; set; }
    }

    public static bool RejectsSilentLineTeleport(int side, int flags2, bool lineNull, bool backSidedefNull) =>
        side != 0 || (flags2 & Mf2Noteleport) != 0 || lineNull || backSidedefNull;

    public static void ComputeAlongSourceLine(
        double thingX,
        double thingY,
        double srcV1X,
        double srcV1Y,
        double srcDeltaX,
        double srcDeltaY,
        out double pos,
        out double nposX,
        out double nposY)
    {
        var den = srcDeltaX * srcDeltaX + srcDeltaY * srcDeltaY;
        if (den == 0)
        {
            pos = 0;
            nposX = 0;
            nposY = 0;
            return;
        }

        var num = (thingX - srcV1X) * srcDeltaX + (thingY - srcV1Y) * srcDeltaY;
        if (num <= 0)
            pos = 0;
        else if (num >= den)
            pos = 1;
        else
            pos = num / den;

        nposX = thingX - srcV1X - srcDeltaX * pos;
        nposY = thingY - srcV1Y - srcDeltaY * pos;
    }

    public static double AngleAfterReverseFlip(double angleDegrees, ref double pos, bool reverse)
    {
        if (!reverse)
        {
            angleDegrees += 180;
            pos = 1 - pos;
        }

        return angleDegrees;
    }

    public static void SinCosDegrees(double angleDegrees, out double c, out double sin)
    {
        var rad = angleDegrees * Math.PI / 180;
        c = Math.Cos(rad);
        sin = Math.Sin(rad);
    }

    public static void RotateOffset(double nposX, double nposY, double c, double sin, out double pX, out double pY)
    {
        pX = nposX * c - nposY * sin;
        pY = nposY * c + nposX * sin;
    }

    public static void InterpolateOnExitLine(
        ref double pX,
        ref double pY,
        double v1X,
        double v1Y,
        double deltaX,
        double deltaY,
        double pos)
    {
        pX += v1X + deltaX * pos;
        pY += v1Y + deltaY * pos;
    }

    public static bool StepDownAtExit(double frontFloorZ, double backFloorZ) => frontFloorZ < backFloorZ;

    public static int ExitSideForFudge(bool reverse, bool hasPlayerMo, bool stepdown) =>
        reverse || (hasPlayerMo && stepdown) ? 1 : 0;

    public static void FudgeOntoExitSide(
        ref double pX,
        ref double pY,
        double srcDeltaX,
        double srcDeltaY,
        int side,
        Func<double, double, int, bool>? pointOnLineSide)
    {
        var fudge = FudgeFactor;
        while (pointOnLineSide?.Invoke(pX, pY, side) != true && --fudge >= 0)
        {
            if (Math.Abs(srcDeltaX) > Math.Abs(srcDeltaY))
                pY -= (srcDeltaX < 0) != (side == 1) ? -EqualEpsilon : EqualEpsilon;
            else
                pX += (srcDeltaY < 0) != (side == 1) ? -EqualEpsilon : EqualEpsilon;
        }
    }

    public static double ExitGroundZ(double thingZ, double thingFloorZ, bool stepdown, double frontFloorZ, double backFloorZ)
    {
        var z = thingZ - thingFloorZ;
        z += stepdown ? backFloorZ : frontFloorZ;
        return z;
    }

    public static void RotatePlanarVelocity(ref double velX, ref double velY, double c, double sin)
    {
        var vx = velX;
        var vy = velY;
        velX = vx * c - vy * sin;
        velY = vy * c + vx * sin;
    }

    public static void AdjustPlayerViewAfterSilentTeleport(SilentLineTeleportState s, double c, double sin)
    {
        if (!s.HasPlayerMo)
            return;

        var pvx = s.PlayerVelX;
        var pvy = s.PlayerVelY;
        RotatePlanarVelocity(ref pvx, ref pvy, c, sin);
        s.PlayerVelX = pvx;
        s.PlayerVelY = pvy;
        var deltaviewheight = s.DeltaViewHeight;
        s.DeltaViewHeight = 0;
        s.CalcHeight?.Invoke();
        s.DeltaViewHeight = deltaviewheight;
    }

    public static bool EvSilentLineTeleport(SilentLineTeleportState s)
    {
        if (RejectsSilentLineTeleport(s.Side, s.Flags2, s.LineNull, s.BackSidedefNull))
            return false;

        foreach (var i in s.LineIdIterator?.Invoke(s.MatchLineIndex) ?? Array.Empty<int>())
        {
            if (i == s.CurrentLineIndex)
                continue;
            var cand = s.GetCandidate?.Invoke(i);
            if (cand == null || !cand.HasBacksector)
                continue;

            ComputeAlongSourceLine(
                s.ThingX,
                s.ThingY,
                cand.SrcV1X,
                cand.SrcV1Y,
                cand.SrcDeltaX,
                cand.SrcDeltaY,
                out var pos,
                out var nposX,
                out var nposY);

            var angle = AngleAfterReverseFlip(cand.DeltaAngleDegrees, ref pos, s.Reverse);
            SinCosDegrees(angle, out var c, out var sin);
            RotateOffset(nposX, nposY, c, sin, out var pX, out var pY);
            InterpolateOnExitLine(ref pX, ref pY, cand.V1X, cand.V1Y, cand.DeltaX, cand.DeltaY, pos);

            var stepdown = StepDownAtExit(cand.FrontFloorZ, cand.BackFloorZ);
            var side = ExitSideForFudge(s.Reverse, s.HasPlayerMo, stepdown);
            FudgeOntoExitSide(ref pX, ref pY, cand.SrcDeltaX, cand.SrcDeltaY, side, s.PointOnLineSide);

            var z = ExitGroundZ(s.ThingZ, s.ThingFloorZ, stepdown, cand.FrontFloorZ, cand.BackFloorZ);

            if (s.TeleportMove?.Invoke(pX, pY, z) != true)
                return false;

            s.YawDegrees += angle;
            var velX = s.VelX;
            var velY = s.VelY;
            RotatePlanarVelocity(ref velX, ref velY, c, sin);
            s.VelX = velX;
            s.VelY = velY;
            AdjustPlayerViewAfterSilentTeleport(s, c, sin);

            return true;
        }

        return false;
    }
}
