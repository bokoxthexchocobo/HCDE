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

    public static bool EvSilentLineTeleport(SilentLineTeleportState s)
    {
        if (s.Side != 0 || (s.Flags2 & Mf2Noteleport) != 0 || s.LineNull || s.BackSidedefNull)
            return false;

        foreach (var i in s.LineIdIterator?.Invoke(s.MatchLineIndex) ?? Array.Empty<int>())
        {
            if (i == s.CurrentLineIndex)
                continue;
            var cand = s.GetCandidate?.Invoke(i);
            if (cand == null)
                continue;

            var den = cand.SrcDeltaX * cand.SrcDeltaX + cand.SrcDeltaY * cand.SrcDeltaY;
            double pos;
            double nposX, nposY;
            if (den == 0)
            {
                pos = 0;
                nposX = nposY = 0;
            }
            else
            {
                var num = (s.ThingX - cand.SrcV1X) * cand.SrcDeltaX + (s.ThingY - cand.SrcV1Y) * cand.SrcDeltaY;
                pos = num <= 0 ? 0 : num >= den ? 1 : num / den;
                nposX = s.ThingX - cand.SrcV1X - cand.SrcDeltaX * pos;
                nposY = s.ThingY - cand.SrcV1Y - cand.SrcDeltaY * pos;
            }

            var angle = cand.DeltaAngleDegrees;
            if (!s.Reverse)
            {
                angle += 180;
                pos = 1 - pos;
            }

            var rad = angle * Math.PI / 180;
            var c = Math.Cos(rad);
            var sin = Math.Sin(rad);
            var pX = nposX * c - nposY * sin;
            var pY = nposY * c + nposX * sin;
            pX += cand.V1X + cand.DeltaX * pos;
            pY += cand.V1Y + cand.DeltaY * pos;

            var stepdown = cand.FrontFloorZ < cand.BackFloorZ;
            var z = s.ThingZ - s.ThingFloorZ;
            var side = s.Reverse || (s.HasPlayerMo && stepdown) ? 1 : 0;
            var fudge = FudgeFactor;
            while (s.PointOnLineSide?.Invoke(pX, pY, side) != true && --fudge >= 0)
            {
                if (Math.Abs(cand.SrcDeltaX) > Math.Abs(cand.SrcDeltaY))
                    pY -= (cand.SrcDeltaX < 0) != (side == 1) ? -EqualEpsilon : EqualEpsilon;
                else
                    pX += (cand.SrcDeltaY < 0) != (side == 1) ? -EqualEpsilon : EqualEpsilon;
            }

            z += stepdown ? cand.BackFloorZ : cand.FrontFloorZ;

            if (s.TeleportMove?.Invoke(pX, pY, z) != true)
                return false;

            s.YawDegrees += angle;
            var vx = s.VelX;
            var vy = s.VelY;
            s.VelX = vx * c - vy * sin;
            s.VelY = vy * c + vx * sin;

            if (s.HasPlayerMo)
            {
                vx = s.PlayerVelX;
                vy = s.PlayerVelY;
                s.PlayerVelX = vx * c - vy * sin;
                s.PlayerVelY = vy * c + vx * sin;
                var deltaviewheight = s.DeltaViewHeight;
                s.DeltaViewHeight = 0;
                s.CalcHeight?.Invoke();
                s.DeltaViewHeight = deltaviewheight;
            }

            return true;
        }

        return false;
    }
}
