namespace HCDE.Playsim;

/// <summary>
/// One-argument <c>DDoor::Construct</c>, the spawn construct, and <c>EV_DoDoor</c>
/// from <c>a_doors.cpp</c> (343–547).
/// </summary>
public static class VmDoorConstruct1377Slice
{
    public const int DoorClose = 0;
    public const int DoorOpen = 1;
    public const int DoorRaise = 2;
    public const int DoorWaitRaise = 3;
    public const int DoorCloseWaitOpen = 4;
    public const int DoorWaitClose = 5;
    public const int CompatNoDoorLight = 1 << 9;
    public const int SpacPush = 1 << 4;
    public const int SpacMPush = 1 << 9;
    public const int ChanVoice = 2;
    public const int ChanCeiling = 2;
    public const float AttnNorm = 1f;
    public const string UseFailSoundName = "*usefail";
    public const string AnimatedDoorClassName = "DAnimatedDoor";

    public sealed class Spot
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public sealed class Sector
    {
        public int SectorNum { get; set; }
        public double FloorPlaneD { get; set; }
        public double CeilingPlaneD { get; set; }
        public double CenterCeiling { get; set; }
        public double CenterFloor { get; set; }
        public object? FloorData { get; set; }
        public Door? CeilingDoor { get; set; }
        public Func<Spot, double, double>? PointToDist { get; set; }
    }

    public sealed class Line
    {
        public Sector? Back { get; set; }
        public int Activation { get; set; }
    }

    public sealed class Thing
    {
        public bool HasPlayer { get; set; }
        public bool Bot { get; set; }
    }

    public sealed class Door
    {
        public Sector? Sector { get; set; }
        public int Type { get; set; }
        public double Speed { get; set; }
        public int TopWait { get; set; }
        public int TopCountdown { get; set; }
        public int LightTag { get; set; }
        public int Direction { get; set; }
        public double TopDist { get; set; }
        public double BotDist { get; set; }
        public double OldFloorDist { get; set; }
        public Spot BotSpot { get; } = new();
        public int SuperCalls { get; set; }
    }

    public sealed class SoundCall
    {
        public Door? Door { get; init; }
        public bool Raise { get; init; }
        public object? Sequence { get; init; }
    }

    public sealed class FailCall
    {
        public Thing? Thing { get; init; }
        public int Channel { get; init; }
        public int Flags { get; init; }
        public string Name { get; init; } = "";
        public double Volume { get; init; }
        public float Attenuation { get; init; }
    }

    public sealed class Hooks
    {
        public int CompatFlags { get; set; }
        public Func<Sector, Spot, double>? LowestCeilingSurrounding { get; set; }
        public Func<Sector, Spot, double>? HighestFloorPoint { get; set; }
        public Func<Sector, Spot, double>? LowestCeilingPoint { get; set; }
        public Func<object?, bool>? IsPlat { get; set; }
        public Func<object?, bool>? IsLift { get; set; }
        public Func<Sector, int, object?>? CheckSequence { get; set; }
        public Func<Thing?, int, bool, bool>? CheckKeys { get; set; }
        public Func<Sector, bool>? PlaneMoving { get; set; }
        public bool Cooldown { get; set; }
        public int Clients { get; set; }
        public Func<Sector, int, double, int, int, int, bool>? CreateThinker { get; set; }
        public Func<int, IReadOnlyList<int>>? TaggedSectors { get; set; }
        public Func<int, Sector?>? SectorAt { get; set; }
        public Func<Thing, int>? PlayerIndex { get; set; }
        public List<SoundCall> Sounds { get; } = new();
        public List<FailCall> Failures { get; } = new();
        public List<(int Tag, int Type, int Zero, double Speed, int Sector, double Ceil, double Floor, int Player)> Events { get; } = new();
        public List<(int Sector, string Kind, double Ceil, double Floor)> Moves { get; } = new();
    }

    public static void ConstructSector(Door door, Sector sector)
    {
        door.Sector = sector;
        door.SuperCalls++;
    }

    public static void Construct(
        Door door, Sector sec, int type, double speed, int delay, int lightTag, int topCountdown, Hooks hooks)
    {
        ConstructSector(door, sec);
        door.Type = type;
        door.Speed = speed;
        door.TopWait = delay;
        door.TopCountdown = topCountdown;
        door.LightTag = lightTag;
        if ((hooks.CompatFlags & CompatNoDoorLight) != 0)
            door.LightTag = 0;

        var spot = new Spot();
        switch (type)
        {
            case DoorClose:
                door.Direction = -1;
                door.TopDist = Dist(sec, spot, Lowest(hooks, sec, spot) - 4);
                Play(hooks, door, false, null);
                break;
            case DoorOpen:
            case DoorRaise:
                door.Direction = 1;
                door.TopDist = Dist(sec, spot, Lowest(hooks, sec, spot) - 4);
                if (door.TopDist != sec.CeilingPlaneD)
                    Play(hooks, door, true, null);
                break;
            case DoorCloseWaitOpen:
                door.TopDist = sec.CeilingPlaneD;
                door.Direction = -1;
                Play(hooks, door, false, null);
                break;
            case DoorWaitRaise:
                door.Direction = 2;
                door.TopDist = Dist(sec, spot, Lowest(hooks, sec, spot) - 4);
                break;
            case DoorWaitClose:
                door.Direction = 0;
                door.Type = DoorRaise;
                door.BotDist = Dist(sec, door.BotSpot, Highest(hooks, sec, door.BotSpot));
                door.OldFloorDist = sec.FloorPlaneD;
                door.TopDist = sec.CeilingPlaneD;
                break;
        }

        var floor = sec.FloorData;
        if (floor == null || hooks.IsPlat?.Invoke(floor) != true || hooks.IsLift?.Invoke(floor) != true)
            door.BotDist = Dist(sec, door.BotSpot, Highest(hooks, sec, door.BotSpot));
        else
            door.BotDist = Dist(sec, door.BotSpot, LowestCeiling(hooks, sec, door.BotSpot));
        door.OldFloorDist = sec.FloorPlaneD;
        hooks.Moves.Add((sec.SectorNum, "construct", sec.CenterCeiling, sec.CenterFloor));
    }

    public static bool EvDoDoor(
        Hooks hooks, int type, Line? line, Thing? thing, int tag, double speed, int delay, int lockNum,
        int lightTag, bool boomgen, int topCountdown)
    {
        var created = false;
        Sector? sec = null;
        if (lockNum != 0 && hooks.CheckKeys?.Invoke(thing, lockNum, tag != 0) != true)
            return false;

        if (tag == 0)
        {
            if (line == null)
                return false;
            if (line.Back == null)
            {
                hooks.Failures.Add(new FailCall
                {
                    Thing = thing,
                    Channel = ChanVoice,
                    Flags = 0,
                    Name = UseFailSoundName,
                    Volume = 1,
                    Attenuation = AttnNorm,
                });
                return false;
            }

            sec = line.Back;
            if (hooks.PlaneMoving?.Invoke(sec) == true)
            {
                if (boomgen)
                    return false;
                var door = sec.CeilingDoor;
                if (door != null && door.Type == DoorRaise && type == DoorRaise)
                {
                    if (door.Direction == -1)
                    {
                        door.Direction = 1;
                        Play(hooks, door, true, null);
                    }
                    else if ((line.Activation & (SpacPush | SpacMPush)) == 0)
                    {
                        if (thing == null)
                            throw new InvalidOperationException("thing");
                        if (!thing.HasPlayer || thing.Bot)
                            return false;
                        if (hooks.Cooldown && hooks.Clients > 1)
                            return false;
                        door.Direction = -1;
                        var seq = hooks.CheckSequence?.Invoke(sec, ChanCeiling);
                        Play(hooks, door, false, seq);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return false;
            }

            if (hooks.CreateThinker?.Invoke(sec, type, speed, delay, lightTag, topCountdown) == true)
                created = true;
        }
        else
        {
            foreach (var index in hooks.TaggedSectors?.Invoke(tag) ?? Array.Empty<int>())
            {
                sec = hooks.SectorAt?.Invoke(index) ?? throw new InvalidOperationException("sector");
                if (hooks.PlaneMoving?.Invoke(sec) == true)
                    continue;
                if (hooks.CreateThinker?.Invoke(sec, type, speed, delay, lightTag, topCountdown) == true)
                    created = true;
            }
        }

        if (!created)
            return false;

        var player = thing is { HasPlayer: true } ? hooks.PlayerIndex?.Invoke(thing) ?? throw new InvalidOperationException("players") : -1;
        hooks.Events.Add((tag, type, 0, speed, sec?.SectorNum ?? -1, sec?.CenterCeiling ?? 0, sec?.CenterFloor ?? 0, player));
        return true;
    }

    static double Lowest(Hooks hooks, Sector sec, Spot spot) =>
        hooks.LowestCeilingSurrounding?.Invoke(sec, spot) ?? throw new InvalidOperationException("FindLowestCeilingSurrounding");

    static double Highest(Hooks hooks, Sector sec, Spot spot) =>
        hooks.HighestFloorPoint?.Invoke(sec, spot) ?? throw new InvalidOperationException("FindHighestFloorPoint");

    static double LowestCeiling(Hooks hooks, Sector sec, Spot spot) =>
        hooks.LowestCeilingPoint?.Invoke(sec, spot) ?? throw new InvalidOperationException("FindLowestCeilingPoint");

    static double Dist(Sector sec, Spot spot, double height) =>
        sec.PointToDist?.Invoke(spot, height) ?? throw new InvalidOperationException("PointToDist");

    static void Play(Hooks hooks, Door door, bool raise, object? sequence) =>
        hooks.Sounds.Add(new SoundCall { Door = door, Raise = raise, Sequence = sequence });
}
