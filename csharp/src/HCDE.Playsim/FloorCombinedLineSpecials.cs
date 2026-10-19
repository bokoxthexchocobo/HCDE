using HCDE.MapLoader;

namespace HCDE.Playsim;

internal static class FloorCombinedLineSpecials
{
    public static bool? Execute(AuthoritySimulation sim, int special, int tag, int speed, int a2, int a3, int a4, LevelLine? line)
    {
        return special switch
        {
            95 => LowerBoth(sim, line, tag, speed, a2),
            96 => RaiseBoth(sim, line, tag, speed, a2),
            251 => LowerRaise(sim, line, tag, speed, a2, a3, a4),
            250 => Donut(sim, line, tag, speed, a2),
            241 => LineSpecials.ExecuteFloorSpecial(sim, 21, tag, speed, a2, a3, a4, line),
            235 or 236 => TransferFloor(sim, line, tag, special == 235, speed, a2, a3),
            _ => null,
        };
    }

    private static bool LowerBoth(AuthoritySimulation sim, LevelLine? line, int tag, int speed, int amount) =>
        LineSpecials.ExecuteFloorSpecial(sim, 20, tag, speed, amount, 0, 0, line) == true
        && CeilingActions.Start(sim, line, tag, CeilingTarget.LowerBy, speed / 8.0, amount: amount);

    private static bool RaiseBoth(AuthoritySimulation sim, LevelLine? line, int tag, int speed, int amount) =>
        LineSpecials.ExecuteFloorSpecial(sim, 23, tag, speed, amount, 0, 0, line) == true
        && CeilingActions.Start(sim, line, tag, CeilingTarget.RaiseBy, speed / 8.0, amount: amount);

    private static bool LowerRaise(AuthoritySimulation sim, LevelLine? line, int tag, int speed, int lower, int raise, int delay) =>
        LineSpecials.ExecuteFloorSpecial(sim, 20, tag, speed, lower, 0, 0, line) == true
        && LineSpecials.ExecuteFloorSpecial(sim, 23, tag, speed, raise, delay, 0, line) == true;

    private static bool Donut(AuthoritySimulation sim, LevelLine? line, int tag, int pillarSpeed, int surroundSpeed)
    {
        var started = false;
        foreach (var pillar in LineSpecials.TargetSectors(sim, line, tag, backSide: true))
        {
            if (sim.Motions.Any(m => m.SectorIndex == pillar && !m.MovesCeiling && !m.Completed)) continue;
            var poolSide = -1;
            foreach (var edge in sim.Level.Lines)
            {
                if (edge.OneSided || (uint)edge.SideFront >= (uint)sim.Level.Sides.Count || (uint)edge.SideBack >= (uint)sim.Level.Sides.Count) continue;
                var front = sim.Level.Sides[edge.SideFront].Sector;
                var back = sim.Level.Sides[edge.SideBack].Sector;
                if (front == pillar) { poolSide = back; break; }
                if (back == pillar) { poolSide = front; break; }
            }
            if (poolSide < 0) continue;
            if (sim.Motions.Any(m => m.SectorIndex == poolSide && !m.MovesCeiling && !m.Completed)) continue;
            var holeSector = -1;
            var holeHeight = double.NegativeInfinity;
            foreach (var boundary in sim.Level.Lines)
            {
                if ((uint)boundary.SideFront >= (uint)sim.Level.Sides.Count || (uint)boundary.SideBack >= (uint)sim.Level.Sides.Count) continue;
                var front = sim.Level.Sides[boundary.SideFront].Sector;
                var back = sim.Level.Sides[boundary.SideBack].Sector;
                var other = front == poolSide ? back : back == poolSide ? front : -1;
                if (other < 0 || other == poolSide || other == pillar) continue;
                var high = sim.Floors[other];
                foreach (var neighbor in LineSpecials.NeighborSectors(sim, other))
                    high = Math.Max(high, sim.Floors[neighbor]);
                if (high > holeHeight) { holeHeight = high; holeSector = other; }
                break;
            }
            if (holeSector < 0) continue;
            started |= LineSpecials.ExecuteFloorSpecial(sim, 23, tag, pillarSpeed, 4, 0, 0, line) == true;
            started |= LineSpecials.ExecuteFloorSpecial(sim, 21, sim.Level.Sectors[poolSide].Tag, surroundSpeed, 0, 0, 0, line) == true;
        }
        return started;
    }

    private static bool TransferFloor(AuthoritySimulation sim, LevelLine? line, int tag, bool triggerModel, int speed, int arg2, int arg3)
    {
        LevelSector? model = null;
        if (line is not null && (uint)line.SideFront < (uint)sim.Level.Sides.Count)
            model = sim.Level.Sectors[sim.Level.Sides[line.SideFront].Sector];
        return LineSpecials.ExecuteFloorSpecial(sim, 239, tag, speed, arg2, arg3, 0, line) == true
            || model is not null && LineSpecials.ExecuteFloorSpecial(sim, 23, tag, speed, arg2, 0, 0, line) == true;
    }
}
