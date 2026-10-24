using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Native <c>LS_Elevator_*</c> from <c>p_lnspec.cpp</c>.</summary>
internal static class ElevatorLineSpecials
{
    internal static bool Execute(AuthoritySimulation sim, int special, int tag, int speed, LevelLine? line)
    {
        if (tag == 0 && line is null) return false;
        var platSpeed = speed / 8.0;
        if (platSpeed <= 0) return false;
        var started = false;
        foreach (var sector in LineSpecials.TargetSectors(sim, line, tag, backSide: true))
        {
            if (sim.Motions.Any(motion => motion.SectorIndex == sector && motion.Kind == MotionKind.Elevator))
                continue;
            var floor = sim.Floors[sector];
            var ceiling = sim.Ceilings[sector];
            var delta = special switch
            {
                245 => NearestDelta(sim, sector, higher: true),
                247 => NearestDelta(sim, sector, higher: false),
                246 => 0,
                _ => 0,
            };
            if (special == 246)
            {
                var neighborFloors = LineSpecials.NeighborSectors(sim, sector).Select(index => sim.Floors[index]).ToArray();
                if (neighborFloors.Length == 0) continue;
                delta = neighborFloors.Min() - floor;
            }
            if (delta == 0 && special != 246) continue;
            sim.Motions.Add(new SectorMotion
            {
                Kind = MotionKind.Elevator,
                SectorIndex = sector,
                Tag = tag,
                ClosedFloor = floor,
                TargetFloor = floor + delta,
                ClosedCeiling = ceiling,
                TargetCeiling = ceiling + delta,
                Speed = platSpeed,
                FloorDirection = delta < 0 ? -1 : 1,
            });
            started = true;
        }
        return started;
    }

    internal static bool Tick(AuthoritySimulation sim, SectorMotion motion)
    {
        var floor = sim.Floors[motion.SectorIndex];
        var ceiling = sim.Ceilings[motion.SectorIndex];
        var oldFloor = floor;
        var oldCeiling = ceiling;
        var targetFloor = motion.TargetFloor;
        var targetCeiling = motion.TargetCeiling;
        if (motion.FloorDirection > 0)
        {
            var nextCeiling = Math.Min(targetCeiling, ceiling + motion.Speed);
            var nextFloor = Math.Min(targetFloor, floor + motion.Speed);
            var blocked = sim.Actors.Any(actor => actor.IsSectorMoverActor && ActorPhysics.BlocksSectorGeometry(actor)
                && ActorPhysics.TouchesSector(sim, actor, motion.SectorIndex)
                && (ActorPhysics.BlocksLoweringCeiling(sim, actor, nextCeiling)
                    || ActorPhysics.RaisedFloorActorZ(actor, ActorPhysics.SectorSupportFloor(sim, actor),
                        Math.Max(ActorPhysics.SectorSupportFloor(sim, actor), nextFloor))
                        + actor.Height.ToDouble() > ActorPhysics.SectorSupportCeiling(sim, actor)));
            if (blocked) { sim.Floors[motion.SectorIndex] = oldFloor; sim.Ceilings[motion.SectorIndex] = oldCeiling; return false; }
            floor = nextFloor;
            ceiling = nextCeiling;
        }
        else
        {
            var nextFloor = Math.Max(targetFloor, floor - motion.Speed);
            var nextCeiling = Math.Max(targetCeiling, ceiling - motion.Speed);
            var blocked = sim.Actors.Any(actor => actor.IsSectorMoverActor && ActorPhysics.BlocksSectorGeometry(actor)
                && ActorPhysics.TouchesSector(sim, actor, motion.SectorIndex)
                && (ActorPhysics.RaisedFloorActorZ(actor, ActorPhysics.SectorSupportFloor(sim, actor),
                        Math.Max(ActorPhysics.SectorSupportFloor(sim, actor), nextFloor))
                        + actor.Height.ToDouble() > ActorPhysics.SectorSupportCeiling(sim, actor)
                    || ActorPhysics.BlocksLoweringCeiling(sim, actor, nextCeiling)));
            if (blocked) { sim.Floors[motion.SectorIndex] = oldFloor; sim.Ceilings[motion.SectorIndex] = oldCeiling; return false; }
            floor = nextFloor;
            ceiling = nextCeiling;
        }
        sim.Floors[motion.SectorIndex] = floor;
        sim.Ceilings[motion.SectorIndex] = ceiling;
        return floor == targetFloor && ceiling == targetCeiling;
    }

    private static double NearestDelta(AuthoritySimulation sim, int sector, bool higher)
    {
        var current = sim.Floors[sector];
        var neighbors = LineSpecials.NeighborSectors(sim, sector).Select(index => sim.Floors[index]);
        if (higher)
        {
            var next = neighbors.Where(height => height > current).DefaultIfEmpty(current).Min();
            return next - current;
        }
        var lower = neighbors.Where(height => height < current).DefaultIfEmpty(current).Max();
        return lower - current;
    }
}
