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
        var started = LineSpecials.ExecuteFloorSpecial(sim, 23, tag, pillarSpeed, 4, 0, 0, line) == true;
        foreach (var sector in LineSpecials.TargetSectors(sim, line, tag, backSide: true))
        {
            foreach (var neighbor in LineSpecials.NeighborSectors(sim, sector))
            {
                if (sim.Level.Sectors[neighbor].MatchesTag(tag)) continue;
                started |= LineSpecials.ExecuteFloorSpecial(sim, 21, sim.Level.Sectors[neighbor].Tag, surroundSpeed, 0, 0, 0, line) == true;
            }
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
