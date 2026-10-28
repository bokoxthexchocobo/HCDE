using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Flat-sector adapter from <see cref="CombatTrace.TraceCore"/> into chunk 698 <see cref="VmPTraceLineCheck.LineCheck"/>.</summary>
public static class CombatTraceLineCheck
{
    public static bool LineBlocks(
        AuthoritySimulation sim,
        LevelLine line,
        double startX,
        double startY,
        double startZ,
        double hitX,
        double hitY,
        double hitZ,
        int currentSector,
        int wallMask,
        double signedDist,
        double frac,
        double enterDist,
        out int crossedSector)
    {
        ArgumentNullException.ThrowIfNull(sim);
        var sides = sim.Level.Sides;
        if ((uint)line.SideFront >= (uint)sides.Count)
        {
            crossedSector = currentSector;
            return true;
        }

        var front = sides[line.SideFront].Sector;
        var hasBack = !line.OneSided && (uint)line.SideBack < (uint)sides.Count;
        var back = hasBack ? sides[line.SideBack].Sector : -1;

        static int SecNum(object? sec) => sec is int i ? i : -1;

        double FloorZ(int sector, double _, double __) =>
            (uint)sector < (uint)sim.Floors.Length ? sim.Floors[sector] : 0;
        double CeilingZ(int sector, double _, double __) =>
            (uint)sector < (uint)sim.Ceilings.Length ? sim.Ceilings[sector] : 128;

        var lineFlags = (uint)line.Flags;
        if (hasBack)
            lineFlags |= VmPTraceLineCheck.MlTwoSided;

        var state = new VmPTraceLineCheck.LineCheckState
        {
            InterceptLine = line,
            Frac = frac,
            Dist = signedDist,
            HitX = hitX,
            HitY = hitY,
            HitZ = hitZ,
            StartX = startX,
            StartY = startY,
            StartZ = startZ,
            CurSector = currentSector,
            CurSectorNum = currentSector,
            LineFrontSector = front,
            LineFrontSectorNum = front,
            LineBackSector = hasBack ? back : null,
            LineBackSectorNum = back,
            LineHasBackSector = hasBack,
            LineFlags = lineFlags,
            WallMask = (uint)wallMask,
            EnterDist = enterDist,
            CompatTrace = false,
            PointOnLineSide = (px, py) =>
                (line.X2 - line.X1) * (py - line.Y1) - (line.Y2 - line.Y1) * (px - line.X1) > 0 ? 1 : 0,
            SectorFloorZAtPoint = (sec, x, y) => FloorZ(SecNum(sec), x, y),
            SectorCeilingZAtPoint = (sec, x, y) => CeilingZ(SecNum(sec), x, y),
            GetSectorTexture = (_, _) => 0,
            LevelSectorFromNum = (_, num) => num,
            CheckSectorPlane = (_, _) => true,
            GetFFloors = _ => Array.Empty<VmPTraceLineCheck.FFloorRover>(),
            CopySectorToDummy = (dst, src) =>
            {
                var num = SecNum(src);
                dst.FloorZAtPoint = (_, _) => FloorZ(num, 0, 0);
                dst.CeilingZAtPoint = (_, _) => CeilingZ(num, 0, 0);
                dst.FFloors = Array.Empty<VmPTraceLineCheck.FFloorRover>();
            },
            SetSourcePosition = static () => { },
        };

        if (VmPTraceLineCheck.LineCheck(state))
        {
            crossedSector = state.CurSectorNum;
            return false;
        }

        crossedSector = currentSector;
        return true;
    }
}
