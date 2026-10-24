namespace HCDE.Playsim;

/// <summary>Faithful port of <c>FTraceInfo::LineCheck</c> (<c>p_trace.cpp</c> 374–659).</summary>
public static class VmPTraceLineCheck
{
    public const double EqualEpsilon = 1.0 / 65536;

    public const uint MlTwoSided = 0x00000004;
    public const int FfExists = 1;
    public const int FfShootThrough = 2;

    public const uint TracePCross = 0x0002;
    public const uint TraceImpact = 0x0004;
    public const uint Trace3DCallback = 0x0020;

    public const int TraceHitNone = 0;
    public const int TraceHitFloor = 1;
    public const int TraceHitCeiling = 2;
    public const int TraceHitWall = 3;

    public const int TierMiddle = 0;
    public const int TierUpper = 1;
    public const int TierLower = 2;
    public const int TierFFloor = 3;

    public const int SpacImpact = 1 << 3;
    public const int SpacPCross = 1 << 5;

    public const int TraceStop = 2;
    public const int TraceContinueOutOfBounds = 1;
    public const int TraceAbort = 3;
    public const int TraceSkip = 4;

    public const int SectorFloor = 0;
    public const int SectorCeiling = 1;

    public sealed class FFloorRover
    {
        public int Flags { get; set; }
        public object? Rover { get; set; }
        public Func<double, double, double> TopZAtPoint { get; set; } = (_, _) => 0;
        public Func<double, double, double> BottomZAtPoint { get; set; } = (_, _) => 0;
        public Func<double, double, double> TopZAtCenter { get; set; } = (_, _) => 0;
        public Func<double, double, double> BottomZAtCenter { get; set; } = (_, _) => 0;
        public int TopTexture { get; set; }
        public int BottomTexture { get; set; }
    }

    public sealed class SectorSnapshot
    {
        public Func<double, double, double> FloorZAtPoint { get; set; } = (_, _) => 0;
        public Func<double, double, double> CeilingZAtPoint { get; set; } = (_, _) => 128;
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public IReadOnlyList<FFloorRover> FFloors { get; set; } = Array.Empty<FFloorRover>();
        public int FloorTexture { get; set; }
        public int CeilingTexture { get; set; }
    }

    public sealed class LineCheckState
    {
        public object? InterceptLine { get; set; }
        public double Frac { get; set; }
        public double Dist { get; set; }
        public double HitX { get; set; }
        public double HitY { get; set; }
        public double HitZ { get; set; }

        public double StartX { get; set; }
        public double StartY { get; set; }
        public double StartZ { get; set; }

        public object? CurSector { get; set; }
        public int CurSectorNum { get; set; }

        public object? LineFrontSector { get; set; }
        public int LineFrontSectorNum { get; set; }
        public object? LineBackSector { get; set; }
        public int LineBackSectorNum { get; set; }
        public bool LineHasBackSector { get; set; }

        public uint LineFlags { get; set; }
        public int LineSpecial { get; set; }

        public uint TraceFlags { get; set; }
        public uint WallMask { get; set; }
        public object? IgnoreThis { get; set; }
        public int InShootThrough { get; set; }
        public bool CompatTrace { get; set; }
        public double EnterDist { get; set; }

        public int HitType { get; set; }
        public int Tier { get; set; }
        public int HitTexture { get; set; }
        public object? Sector { get; set; }
        public object? Line { get; set; }
        public int Side { get; set; }
        public double ResultDistance { get; set; }
        public double ResultFraction { get; set; }
        public double ResultHitPosX { get; set; }
        public double ResultHitPosY { get; set; }
        public double ResultHitPosZ { get; set; }
        public object? Ffloor { get; set; }
        public object? CrossedWater { get; set; }
        public double CrossedWaterPosX { get; set; }
        public double CrossedWaterPosY { get; set; }
        public double CrossedWaterPosZ { get; set; }
        public double CrossedWaterDistance { get; set; }

        public SectorSnapshot[] DummySector { get; } = [new SectorSnapshot(), new SectorSnapshot()];
        public int SectorSel { get; set; }

        public Func<double, double, int>? PointOnLineSide { get; set; }
        public Action<object?, object?, int, int>? ActivateLine { get; set; }
        public Func<object?, bool, bool>? CheckSectorPlane { get; set; }
        public Func<int>? TraceCallback { get; set; }
        public bool HasTraceCallback { get; set; }

        public Func<object?, double, double, double>? SectorFloorZAtPoint { get; set; }
        public Func<object?, double, double, double>? SectorCeilingZAtPoint { get; set; }
        public Func<object?, int, int>? GetSectorTexture { get; set; }
        public Func<object?, object?>? GetHeightSec { get; set; }
        public Func<object?, int, object?>? LevelSectorFromNum { get; set; }

        public Action<SectorSnapshot, object?>? CopySectorToDummy { get; set; }
        public Action<SectorSnapshot, FFloorRover>? SetFloorFromFFloorTop { get; set; }
        public Action<SectorSnapshot, FFloorRover>? SetCeilingFromFFloorBottom { get; set; }
        public Action? SetSourcePosition { get; set; }
        public Func<object?, IReadOnlyList<FFloorRover>>? GetFFloors { get; set; }
    }

    public static bool LineCheck(LineCheckState s, bool special3dpass = false)
    {
        int lineside;
        object? entersector;
        SectorSnapshot? enterSnapshot = null;

        double ff, fc, bf = 0, bc = 0;

        if (s.LineFrontSectorNum == s.CurSectorNum)
        {
            lineside = 0;
        }
        else if (s.LineHasBackSector && s.LineBackSectorNum == s.CurSectorNum)
        {
            lineside = 1;
        }
        else
        {
            if (!s.LineHasBackSector)
            {
                lineside = 0;
                s.CurSector = s.LineFrontSector;
                s.CurSectorNum = s.LineFrontSectorNum;
            }
            else
            {
                lineside = s.PointOnLineSide?.Invoke(s.StartX, s.StartY) ?? 0;
                s.CurSector = lineside != 0 ? s.LineBackSector : s.LineFrontSector;
                s.CurSectorNum = lineside != 0 ? s.LineBackSectorNum : s.LineFrontSectorNum;
            }
        }

        if ((s.LineFlags & MlTwoSided) == 0)
        {
            entersector = null;
        }
        else
        {
            entersector = lineside == 0 ? s.LineBackSector : s.LineFrontSector;

            if (s.CompatTrace && s.LineHasBackSector && ReferenceEquals(s.LineBackSector, s.LineFrontSector) && !special3dpass)
            {
                if ((s.TraceFlags & TracePCross) != 0)
                    s.ActivateLine?.Invoke(s.InterceptLine, s.IgnoreThis, lineside, SpacPCross);
                if ((s.TraceFlags & TraceImpact) != 0)
                    s.ActivateLine?.Invoke(s.InterceptLine, s.IgnoreThis, lineside, SpacImpact);
                return true;
            }
        }

        ff = s.SectorFloorZAtPoint!(s.CurSector, s.HitX, s.HitY);
        fc = s.SectorCeilingZAtPoint!(s.CurSector, s.HitX, s.HitY);

        if (entersector != null)
        {
            bf = s.SectorFloorZAtPoint!(entersector, s.HitX, s.HitY);
            bc = s.SectorCeilingZAtPoint!(entersector, s.HitX, s.HitY);
        }

        var hsec = s.GetHeightSec?.Invoke(s.CurSector);
        if (s.CrossedWater == null &&
            hsec != null &&
            s.StartZ > s.SectorFloorZAtPoint!(hsec, s.StartX, s.StartY) &&
            s.HitZ <= s.SectorFloorZAtPoint!(hsec, s.HitX, s.HitY))
        {
            if (s.CheckSectorPlane?.Invoke(hsec, true) == true)
            {
                s.CrossedWater = s.LevelSectorFromNum?.Invoke(s.CurSector, s.CurSectorNum);
                s.CrossedWaterPosX = s.ResultHitPosX;
                s.CrossedWaterPosY = s.ResultHitPosY;
                s.CrossedWaterPosZ = s.ResultHitPosZ;
                s.CrossedWaterDistance = 0;
            }
        }

        if (s.HitZ <= ff)
        {
            s.HitType = TraceHitFloor;
            s.HitTexture = s.GetSectorTexture!(s.CurSector, SectorFloor);
        }
        else if (s.HitZ >= fc)
        {
            s.HitType = TraceHitCeiling;
            s.HitTexture = s.GetSectorTexture!(s.CurSector, SectorCeiling);
        }
        else if (entersector == null ||
                 s.HitZ < bf || s.HitZ > bc ||
                 ((s.LineFlags & s.WallMask) != 0 && !special3dpass))
        {
            s.HitType = TraceHitWall;
            s.Tier =
                entersector == null ? TierMiddle :
                s.HitZ <= bf ? TierLower :
                s.HitZ >= bc ? TierUpper : TierMiddle;
            if ((s.TraceFlags & TraceImpact) != 0 && !special3dpass)
                s.ActivateLine?.Invoke(s.InterceptLine, s.IgnoreThis, lineside, SpacImpact);
        }
        else
        {
            var enterFFloors = s.GetFFloors?.Invoke(entersector) ?? Array.Empty<FFloorRover>();
            if (enterFFloors.Count > 0)
            {
                s.CopySectorToDummy?.Invoke(s.DummySector[s.SectorSel], entersector);
                enterSnapshot = s.DummySector[s.SectorSel];
                entersector = enterSnapshot;
                s.SectorSel ^= 1;

                bool setFloor = false, setCeiling = false;

                foreach (var rover in enterFFloors)
                {
                    int entershootthrough = (rover.Flags & FfShootThrough) != 0 ? 1 : 0;

                    if (entershootthrough != s.InShootThrough && (rover.Flags & FfExists) != 0)
                    {
                        double ff_bottom = rover.BottomZAtPoint(s.HitX, s.HitY);
                        double ff_top = rover.TopZAtPoint(s.HitX, s.HitY);

                        if (s.HitZ > ff_top)
                        {
                            if ((ff_top < bf && !setFloor) || Math.Abs(bf - ff_top) < EqualEpsilon)
                            {
                                double cf = enterSnapshot.FloorZAtPoint(enterSnapshot.CenterX, enterSnapshot.CenterY);
                                double ffc = rover.TopZAtCenter(enterSnapshot.CenterX, enterSnapshot.CenterY);
                                if (ffc > cf)
                                    bf = ff_top - EqualEpsilon;
                            }

                            if (bf < ff_top)
                            {
                                setFloor = true;
                                s.SetFloorFromFFloorTop?.Invoke(enterSnapshot, rover);
                                bf = ff_top;
                            }
                        }
                        else if (s.HitZ < ff_bottom)
                        {
                            if ((ff_bottom > bc && !setCeiling) || Math.Abs(bc - ff_bottom) < EqualEpsilon)
                            {
                                double cc = enterSnapshot.CeilingZAtPoint(enterSnapshot.CenterX, enterSnapshot.CenterY);
                                double fcc = rover.BottomZAtCenter(enterSnapshot.CenterX, enterSnapshot.CenterY);
                                if (fcc < cc)
                                    bc = ff_bottom + EqualEpsilon;
                            }

                            if (bc > ff_bottom)
                            {
                                setCeiling = true;
                                s.SetCeilingFromFFloorBottom?.Invoke(enterSnapshot, rover);
                                bc = ff_bottom;
                            }
                        }
                        else
                        {
                            s.HitType = TraceHitWall;
                            s.Tier = TierFFloor;
                            s.Ffloor = rover.Rover;
                            if ((s.TraceFlags & TraceImpact) != 0 && s.LineSpecial != 0)
                                s.ActivateLine?.Invoke(s.InterceptLine, s.IgnoreThis, lineside, SpacImpact);
                            goto cont;
                        }
                    }
                }
            }

            s.HitType = TraceHitNone;
            if (!special3dpass)
            {
                if ((s.TraceFlags & TracePCross) != 0)
                    s.ActivateLine?.Invoke(s.InterceptLine, s.IgnoreThis, lineside, SpacPCross);
                if ((s.TraceFlags & TraceImpact) != 0)
                    s.ActivateLine?.Invoke(s.InterceptLine, s.IgnoreThis, lineside, SpacImpact);
            }
        }

    cont:
        if (s.HitType != TraceHitNone)
        {
            s.Sector = s.LevelSectorFromNum?.Invoke(s.CurSector, s.CurSectorNum);

            if (s.HitType != TraceHitWall &&
                s.CheckSectorPlane?.Invoke(s.CurSector, s.HitType == TraceHitFloor) == false)
            {
                if (entersector == null)
                {
                    s.HitType = TraceHitWall;
                    s.Tier = TierMiddle;
                }
                else
                {
                    if (entersector is SectorSnapshot snap)
                    {
                        bf = snap.FloorZAtPoint(s.HitX, s.HitY);
                        bc = snap.CeilingZAtPoint(s.HitX, s.HitY);
                    }
                    else
                    {
                        bf = s.SectorFloorZAtPoint!(entersector, s.HitX, s.HitY);
                        bc = s.SectorCeilingZAtPoint!(entersector, s.HitX, s.HitY);
                    }

                    if (s.HitZ <= bf || s.HitZ >= bc)
                    {
                        s.HitType = TraceHitWall;
                        s.Tier =
                            s.HitZ <= bf ? TierLower :
                            s.HitZ >= bc ? TierUpper : TierMiddle;
                    }
                    else
                    {
                        s.HitType = TraceHitNone;
                    }
                }

                if (s.HitType == TraceHitWall && (s.TraceFlags & TraceImpact) != 0 && (!special3dpass || s.Tier != TierFFloor))
                    s.ActivateLine?.Invoke(s.InterceptLine, s.IgnoreThis, lineside, SpacImpact);
            }

            if (s.HitType == TraceHitWall)
            {
                s.ResultHitPosX = s.HitX;
                s.ResultHitPosY = s.HitY;
                s.ResultHitPosZ = s.HitZ;
                s.SetSourcePosition?.Invoke();
                s.ResultDistance = s.Dist;
                s.ResultFraction = s.Frac;
                s.Line = s.InterceptLine;
                s.Side = lineside;
            }
        }

        if (s.HasTraceCallback && s.HitType != TraceHitNone)
        {
            switch (s.TraceCallback?.Invoke() ?? TraceStop)
            {
                case TraceStop:
                    return false;

                case TraceContinueOutOfBounds:
                    return true;

                case TraceAbort:
                    s.HitType = TraceHitNone;
                    return false;

                case TraceSkip:
                    s.HitType = TraceHitNone;
                    {
                        var ffCount = entersector switch
                        {
                            SectorSnapshot snap => snap.FFloors.Count,
                            null => 0,
                            _ => s.GetFFloors?.Invoke(entersector)?.Count ?? 0,
                        };
                        if (!special3dpass && (s.TraceFlags & Trace3DCallback) != 0 && entersector != null && ffCount > 0)
                            return LineCheck(s, true);
                    }
                    break;

                default:
                    break;
            }
        }

        if (s.HitType == TraceHitNone && entersector != null)
        {
            s.CurSector = entersector;
            s.CurSectorNum = lineside == 0 ? s.LineBackSectorNum : s.LineFrontSectorNum;
            s.EnterDist = s.Dist;
            return true;
        }

        return false;
    }
}
