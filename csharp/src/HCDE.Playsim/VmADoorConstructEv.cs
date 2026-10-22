namespace HCDE.Playsim;

/// <summary><c>DDoor::Construct</c> (overload) and <c>FLevelLocals::EV_DoDoor</c> from <c>a_doors.cpp</c> (355–538).</summary>
public static class VmADoorConstructEv
{
    public const int CompatNoDoorLight = 1 << 2;
    public const int DoorClose = 0;
    public const int DoorOpen = 1;
    public const int DoorRaise = 2;
    public const int DoorWaitRaise = 3;
    public const int DoorCloseWaitOpen = 4;
    public const int DoorWaitClose = 5;
    public const int SpacPush = 1 << 0;
    public const int SpacMpush = 1 << 1;

    public sealed class VertexSpot
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public sealed class EvLine
    {
        public EvSide? Side1 { get; set; }
        public int Activation { get; set; }
    }

    public sealed class EvSide
    {
        public VmADoorTickSound.DoorSector? Sector { get; set; }
    }

    public sealed class EvThing
    {
        public EvPlayer? Player { get; set; }
    }

    public sealed class EvPlayer
    {
        public object? Bot { get; set; }
    }

    public sealed class DoorConstructContext
    {
        public int CompatFlags { get; init; }
        public VmADoorTickSound.DoorTickContext? Tick { get; init; }
        public Func<VmADoorTickSound.DoorSector, VertexSpot?, double>? FindLowestCeilingSurrounding { get; init; }
        public Func<VmADoorTickSound.DoorSector, VertexSpot?, double>? FindHighestFloorPoint { get; init; }
        public Func<VmADoorTickSound.DoorSector, VertexSpot?, double>? FindLowestCeilingPoint { get; init; }
        public Func<object?, bool>? FloorDataIsLift { get; init; }
        public Func<double, double, double, double>? PointToDist { get; init; }
        public Action<int, string, double, double>? DiagTraceDoorMove { get; init; }
    }

    public sealed class EvDoDoorContext
    {
        public DoorConstructContext Construct { get; init; } = new();
        public Func<EvThing?, int, int, bool>? CheckKeys { get; init; }
        public Func<VmADoorTickSound.DoorSector, bool>? PlaneMovingCeiling { get; init; }
        public Func<VmADoorTickSound.DoorSector, VmADoorTickSound.DoorThinker?>? GetCeilingDoor { get; init; }
        public Func<VmADoorTickSound.DoorSector, int, int>? CheckSequence { get; init; }
        public Action<EvThing?>? PlayUseFailSound { get; init; }
        public bool NetRepeatableActionCooldown { get; init; }
        public int NetworkClientCount { get; init; }
        public Func<VmADoorTickSound.DoorSector, int, double, int, int, int, bool>? CreateDoorThinker { get; init; }
        public Func<int, IEnumerable<int>>? SectorTagIterator { get; init; }
        public Func<int, VmADoorTickSound.DoorSector?>? SectorByIndex { get; init; }
        public Action<int, int, int, double, int, double, double, int>? DiagTraceDoorEvent { get; init; }
    }

    public static void Construct(
        VmADoorTickSound.DoorThinker door,
        VmADoorTickSound.DoorSector sec,
        int type,
        double speed,
        int delay,
        int lightTag,
        int topcountdown,
        DoorConstructContext ctx)
    {
        door.Type = type;
        door.Speed = speed;
        door.TopWait = delay;
        door.TopCountdown = topcountdown;
        door.LightTag = lightTag;

        if ((ctx.CompatFlags & CompatNoDoorLight) != 0)
            door.LightTag = 0;

        var spot = new VertexSpot();
        double height;
        var tick = ctx.Tick ?? new VmADoorTickSound.DoorTickContext();

        switch (type)
        {
            case DoorClose:
                door.Direction = -1;
                height = ctx.FindLowestCeilingSurrounding?.Invoke(sec, spot) ?? 0;
                door.TopDist = ctx.PointToDist?.Invoke(spot.X, spot.Y, height - 4) ?? 0;
                VmADoorTickSound.DoorSound(door, false, tick);
                break;

            case DoorOpen:
            case DoorRaise:
                door.Direction = 1;
                height = ctx.FindLowestCeilingSurrounding?.Invoke(sec, spot) ?? 0;
                door.TopDist = ctx.PointToDist?.Invoke(spot.X, spot.Y, height - 4) ?? 0;
                if (door.TopDist != sec.CeilingPlaneD)
                    VmADoorTickSound.DoorSound(door, true, tick);
                break;

            case DoorCloseWaitOpen:
                door.TopDist = sec.CeilingPlaneD;
                door.Direction = -1;
                VmADoorTickSound.DoorSound(door, false, tick);
                break;

            case DoorWaitRaise:
                door.Direction = 2;
                height = ctx.FindLowestCeilingSurrounding?.Invoke(sec, spot) ?? 0;
                door.TopDist = ctx.PointToDist?.Invoke(spot.X, spot.Y, height - 4) ?? 0;
                break;

            case DoorWaitClose:
                door.Direction = 0;
                door.Type = DoorRaise;
                height = ctx.FindHighestFloorPoint?.Invoke(sec, spot) ?? 0;
                door.BotSpotX = spot.X;
                door.BotSpotY = spot.Y;
                door.BotDist = ctx.PointToDist?.Invoke(spot.X, spot.Y, height) ?? 0;
                door.OldFloorDist = sec.FloorPlaneD;
                door.TopDist = sec.CeilingPlaneD;
                break;
        }

        var floorData = sec.FloorData;
        if (floorData == null || ctx.FloorDataIsLift?.Invoke(floorData) != true)
        {
            height = ctx.FindHighestFloorPoint?.Invoke(sec, spot) ?? 0;
            door.BotSpotX = spot.X;
            door.BotSpotY = spot.Y;
            door.BotDist = ctx.PointToDist?.Invoke(spot.X, spot.Y, height) ?? 0;
        }
        else
        {
            height = ctx.FindLowestCeilingPoint?.Invoke(sec, spot) ?? 0;
            door.BotSpotX = spot.X;
            door.BotSpotY = spot.Y;
            door.BotDist = ctx.PointToDist?.Invoke(spot.X, spot.Y, height) ?? 0;
        }

        door.OldFloorDist = sec.FloorPlaneD;
        ctx.DiagTraceDoorMove?.Invoke(sec.SectorNum, "construct", sec.CenterCeiling, sec.CenterFloor);
    }

    public static bool EvDoDoor(
        EvDoDoorContext ctx,
        int type,
        EvLine? line,
        EvThing? thing,
        int tag,
        double speed,
        int delay,
        int lockArg,
        int lightTag,
        bool boomgen,
        int topcountdown)
    {
        var rtn = false;
        VmADoorTickSound.DoorSector? sec = null;
        int secnum;

        if (lockArg != 0 && ctx.CheckKeys?.Invoke(thing, lockArg, tag != 0 ? 1 : 0) != true)
            return false;

        if (tag == 0)
        {
            if (line == null)
                return false;

            if (line.Side1 == null)
            {
                ctx.PlayUseFailSound?.Invoke(thing);
                return false;
            }

            sec = line.Side1.Sector;
            if (sec == null)
                return false;
            secnum = sec.SectorNum;

            if (ctx.PlaneMovingCeiling?.Invoke(sec) == true)
            {
                if (boomgen)
                    return false;
                var door = ctx.GetCeilingDoor?.Invoke(sec);
                if (door != null)
                {
                    if (door.Type == DoorRaise && type == DoorRaise)
                    {
                        if (door.Direction == -1)
                        {
                            door.Direction = 1;
                            VmADoorTickSound.DoorSound(door, true, ctx.Construct.Tick ?? new VmADoorTickSound.DoorTickContext());
                        }
                        else if ((line.Activation & (SpacPush | SpacMpush)) == 0)
                        {
                            if (thing?.Player == null || thing.Player.Bot != null)
                                return false;

                            if (ctx.NetRepeatableActionCooldown && ctx.NetworkClientCount > 1)
                                return false;

                            door.Direction = -1;
                            var curSeq = ctx.CheckSequence?.Invoke(sec, 0) ?? 0;
                            VmADoorTickSound.DoorSound(door, false, ctx.Construct.Tick ?? new VmADoorTickSound.DoorTickContext
                            {
                                CurSeqMode = curSeq,
                            });
                            return true;
                        }

                        return false;
                    }
                }

                return false;
            }

            if (ctx.CreateDoorThinker?.Invoke(sec, type, speed, delay, lightTag, topcountdown) == true)
                rtn = true;
        }
        else
        {
            var it = ctx.SectorTagIterator?.Invoke(tag) ?? Array.Empty<int>();
            foreach (var sn in it)
            {
                secnum = sn;
                sec = ctx.SectorByIndex?.Invoke(secnum);
                if (sec == null)
                    continue;
                if (ctx.PlaneMovingCeiling?.Invoke(sec) == true)
                    continue;
                if (ctx.CreateDoorThinker?.Invoke(sec, type, speed, delay, lightTag, topcountdown) == true)
                    rtn = true;
            }
        }

        if (rtn)
        {
            var activatorPlayer = thing?.Player != null ? 0 : -1;
            var ceilZ = sec?.CenterCeiling ?? 0;
            var floorZ = sec?.CenterFloor ?? 0;
            ctx.DiagTraceDoorEvent?.Invoke(tag, type, 0, speed, sec?.SectorNum ?? -1, ceilZ, floorZ, activatorPlayer);
        }

        return rtn;
    }
}
