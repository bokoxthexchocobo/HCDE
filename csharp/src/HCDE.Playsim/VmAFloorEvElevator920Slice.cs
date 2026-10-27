namespace HCDE.Playsim;

/// <summary>
/// `DElevator::StartFloorSound` and `EV_DoElevator` from <c>mapthinkers/a_floor.cpp</c> (**992–995**, **1010–1095**).
/// Exclusive to chunk **920**; defers `DElevator::Tick` (**941–984**, chunk **6** credit).
/// </summary>
public static class VmAFloorEvElevator920Slice
{
    public static void ElevatorStartFloorSound(
        VmAFloorTickElevator.ElevatorThinker elevator,
        Action<VmAFloorTickElevator.FloorSector>? startStaticFloorSound) =>
        startStaticFloorSound?.Invoke(elevator.Sector);

    public static bool EvDoElevator(
        VmAFloorTickElevator.EvDoElevatorContext ctx,
        VmAFloorTickElevator.FloorLine? line,
        int elevType,
        double speed,
        double height,
        int tag)
    {
        if (line == null && elevType == VmAFloorTickElevator.ElevateCurrent)
            return false;

        var rtn = false;
        foreach (var secnum in ctx.SectorTagIterator?.Invoke(tag) ?? Array.Empty<int>())
        {
            var sec = ctx.SectorByIndex?.Invoke(secnum);
            if (sec == null)
                continue;
            if (ctx.PlaneMovingFloor?.Invoke(sec) == true || sec.CeilingData != null)
                continue;

            rtn = true;
            var elevator = ctx.CreateElevator?.Invoke(sec);
            if (elevator == null)
                continue;
            elevator.Type = elevType;
            elevator.Speed = speed;
            VmAFloorTickElevator.StartElevatorFloorSound(elevator, ctx.Sound);

            var floorheight = sec.CenterFloor;
            var ceilingheight = sec.CenterCeiling;
            var spot = new VmAFloorTickElevator.VertexSpot();
            double newheight;

            switch (elevType)
            {
                case VmAFloorTickElevator.ElevateDown:
                    elevator.Direction = -1;
                    newheight = ctx.FindNextLowestFloor?.Invoke(sec, spot) ?? 0;
                    elevator.FloorDestDist = sec.PointToDist?.Invoke(spot.X, spot.Y, newheight) ?? 0;
                    newheight += (sec.CeilingZAtPoint?.Invoke(spot.X, spot.Y) ?? 0)
                        - (sec.FloorZAtPoint?.Invoke(spot.X, spot.Y) ?? 0);
                    elevator.CeilingDestDist = CeilingDist(sec, spot.X, spot.Y, newheight);
                    break;
                case VmAFloorTickElevator.ElevateUp:
                    elevator.Direction = 1;
                    newheight = ctx.FindNextHighestFloor?.Invoke(sec, spot) ?? 0;
                    elevator.FloorDestDist = sec.PointToDist?.Invoke(spot.X, spot.Y, newheight) ?? 0;
                    newheight += (sec.CeilingZAtPoint?.Invoke(spot.X, spot.Y) ?? 0)
                        - (sec.FloorZAtPoint?.Invoke(spot.X, spot.Y) ?? 0);
                    elevator.CeilingDestDist = CeilingDist(sec, spot.X, spot.Y, newheight);
                    break;
                case VmAFloorTickElevator.ElevateCurrent:
                    if (line?.FrontSector == null)
                        break;
                    newheight = line.FrontSector.FloorZAtPoint?.Invoke(line.V1.X, line.V1.Y) ?? 0;
                    elevator.FloorDestDist = sec.PointToDist?.Invoke(line.V1.X, line.V1.Y, newheight) ?? 0;
                    newheight += (sec.CeilingZAtPoint?.Invoke(line.V1.X, line.V1.Y) ?? 0)
                        - (sec.FloorZAtPoint?.Invoke(line.V1.X, line.V1.Y) ?? 0);
                    elevator.CeilingDestDist = CeilingDist(sec, line.V1.X, line.V1.Y, newheight);
                    elevator.Direction = elevator.FloorDestDist > sec.FloorPlaneD ? -1 : 1;
                    break;
                case VmAFloorTickElevator.ElevateRaise:
                    elevator.Direction = 1;
                    elevator.FloorDestDist = sec.PointToDist?.Invoke(sec.Centerspot.X, sec.Centerspot.Y, floorheight + height) ?? 0;
                    elevator.CeilingDestDist = CeilingDist(sec, sec.Centerspot.X, sec.Centerspot.Y, ceilingheight + height);
                    break;
                case VmAFloorTickElevator.ElevateLower:
                    elevator.Direction = -1;
                    elevator.FloorDestDist = sec.PointToDist?.Invoke(sec.Centerspot.X, sec.Centerspot.Y, floorheight - height) ?? 0;
                    elevator.CeilingDestDist = CeilingDist(sec, sec.Centerspot.X, sec.Centerspot.Y, ceilingheight - height);
                    break;
            }
        }

        return rtn;
    }

    static double CeilingDist(VmAFloorTickElevator.FloorSector sec, double x, double y, double z) =>
        sec.CeilingPointToDist?.Invoke(x, y, z) ?? sec.PointToDist?.Invoke(x, y, z) ?? 0;
}
