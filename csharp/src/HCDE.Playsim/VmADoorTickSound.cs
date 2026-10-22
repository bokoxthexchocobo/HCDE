namespace HCDE.Playsim;

/// <summary><c>DDoor::Tick</c> and <c>DDoor::DoorSound</c> from <c>a_doors.cpp</c> (80–343).</summary>
public static class VmADoorTickSound
{
    public const int SecfSilentmove = 1 << 14;
    public const int DoorRaise = 0;
    public const int DoorClose = 1;
    public const int DoorCloseWaitOpen = 2;
    public const int DoorWaitRaise = 3;
    public const int DoorOpen = 4;
    public const int GameHeretic = 1;
    public const int GameStrife = 2;

    public enum MoveResult
    {
        Ok = 0,
        PastDest = 1,
        Crushed = 2,
    }

    public sealed class DoorSector
    {
        public uint Flags { get; set; }
        public int SeqType { get; set; } = -1;
        public string SeqName { get; set; } = "";
        public double FloorPlaneD { get; set; }
        public double CeilingPlaneD { get; set; }
        public object? FloorData { get; set; }
        public object? CeilingData { get; set; }
        public int SectorNum { get; set; }
        public double CenterCeiling { get; set; }
        public double CenterFloor { get; set; }
        public List<DoorLine> Lines { get; } = new();
        public Func<double, double, double>? CeilingZAtPoint { get; set; }
        public Func<double, double, double>? PointToDist { get; set; }
    }

    public sealed class DoorLine
    {
        public DoorSector? BackSector { get; set; }
        public DoorSide? Side0 { get; set; }
    }

    public sealed class DoorSide
    {
        public string TopTexture { get; set; } = "";
    }

    public sealed class DoorThinker
    {
        public required DoorSector Sector { get; init; }
        public int Type { get; set; }
        public double TopDist { get; set; }
        public double BotSpotX { get; set; }
        public double BotSpotY { get; set; }
        public double BotDist { get; set; }
        public double OldFloorDist { get; set; }
        public double Speed { get; set; }
        public int Direction { get; set; }
        public int TopWait { get; set; }
        public int TopCountdown { get; set; }
        public int LightTag { get; set; }
        public bool Destroyed { get; set; }
    }

    public sealed class DoorTickContext
    {
        public Func<object?, bool>? FloorDataIsLift { get; init; }
        public Func<DoorSector, double, double, int, int, bool, MoveResult>? MoveCeiling { get; init; }
        public Action<int, double>? LightTurnOnPartway { get; init; }
        public Action<DoorSector>? StopSequence { get; init; }
        public Action<int, string, double, double>? DiagTraceDoorMove { get; init; }
        public int GameType { get; init; }
        public Func<int, int, int, int, bool>? SeqModesSameByType { get; init; }
        public Func<string, int, int, bool>? SeqModesSameByName { get; init; }
        public Action<DoorSector, int, int, int, int>? StartSequenceType { get; init; }
        public Action<DoorThinker>? RefreshBotDist { get; init; }
        public Action<DoorSector, int, string, int>? StartSequenceName { get; init; }
        public Func<string, int, int, bool>? SeqModesSameSound { get; init; }
        public Action<DoorSector, int, string, int>? StartSequenceSound { get; init; }
        public int? CurSeqMode { get; init; }
    }

    public static void Tick(DoorThinker door, DoorTickContext ctx)
    {
        var sec = door.Sector;
        if (sec.FloorPlaneD != door.OldFloorDist)
        {
            var floorData = sec.FloorData;
            if (floorData == null || ctx.FloorDataIsLift?.Invoke(floorData) != true)
            {
                door.OldFloorDist = sec.FloorPlaneD;
                ctx.RefreshBotDist?.Invoke(door);
            }
        }

        switch (door.Direction)
        {
            case 0:
                if (--door.TopCountdown == 0)
                {
                    switch (door.Type)
                    {
                        case DoorRaise:
                            door.Direction = -1;
                            DoorSound(door, true, ctx);
                            break;
                        case DoorCloseWaitOpen:
                            door.Direction = 1;
                            DoorSound(door, true, ctx);
                            break;
                    }
                }
                break;
            case 2:
                if (--door.TopCountdown == 0)
                {
                    switch (door.Type)
                    {
                        case DoorWaitRaise:
                            door.Direction = 1;
                            door.Type = DoorRaise;
                            DoorSound(door, true, ctx);
                            break;
                    }
                }
                break;
            case -1:
            {
                var res = ctx.MoveCeiling?.Invoke(sec, door.Speed, door.BotDist, -1, door.Direction, false)
                    ?? MoveResult.Ok;
                if (door.LightTag != 0 && door.TopDist != -sec.FloorPlaneD)
                {
                    var part = (sec.CeilingPlaneD + sec.FloorPlaneD) / (door.TopDist + sec.FloorPlaneD);
                    ctx.LightTurnOnPartway?.Invoke(door.LightTag, part);
                }
                if (res == MoveResult.PastDest)
                {
                    ctx.StopSequence?.Invoke(sec);
                    switch (door.Type)
                    {
                        case DoorRaise:
                        case DoorClose:
                            ctx.DiagTraceDoorMove?.Invoke(sec.SectorNum, "finished-closed", sec.CenterCeiling, sec.CenterFloor);
                            sec.CeilingData = null;
                            door.Destroyed = true;
                            break;
                        case DoorCloseWaitOpen:
                            door.Direction = 0;
                            door.TopCountdown = door.TopWait;
                            break;
                    }
                }
                else if (res == MoveResult.Crushed)
                {
                    switch (door.Type)
                    {
                        case DoorClose:
                            break;
                        default:
                            door.Direction = 1;
                            DoorSound(door, true, ctx);
                            break;
                    }
                }
                break;
            }
            case 1:
            {
                var res = ctx.MoveCeiling?.Invoke(sec, door.Speed, door.TopDist, -1, door.Direction, false)
                    ?? MoveResult.Ok;
                if (door.LightTag != 0 && door.TopDist != -sec.FloorPlaneD)
                {
                    var part = (sec.CeilingPlaneD + sec.FloorPlaneD) / (door.TopDist + sec.FloorPlaneD);
                    ctx.LightTurnOnPartway?.Invoke(door.LightTag, part);
                }
                if (res == MoveResult.PastDest)
                {
                    ctx.StopSequence?.Invoke(sec);
                    switch (door.Type)
                    {
                        case DoorRaise:
                            door.Direction = 0;
                            door.TopCountdown = door.TopWait;
                            break;
                        case DoorCloseWaitOpen:
                        case DoorOpen:
                            ctx.DiagTraceDoorMove?.Invoke(sec.SectorNum, "finished-open", sec.CenterCeiling, sec.CenterFloor);
                            sec.CeilingData = null;
                            door.Destroyed = true;
                            break;
                    }
                }
                else if (res == MoveResult.Crushed)
                {
                    switch (door.Type)
                    {
                        case DoorRaise:
                        case DoorWaitRaise:
                            door.Direction = -1;
                            DoorSound(door, false, ctx);
                            break;
                    }
                }
                break;
            }
        }
    }

    public static void DoorSound(DoorThinker door, bool raise, DoorTickContext ctx)
    {
        var sec = door.Sector;
        var choice = raise ? 0 : 1;
        if ((sec.Flags & SecfSilentmove) != 0)
            return;
        if (door.Speed >= 8)
            choice += 2;
        var curMode = ctx.CurSeqMode;
        if (sec.SeqType >= 0)
        {
            if (curMode == null || ctx.SeqModesSameByType?.Invoke(sec.SeqType, 0, choice, curMode.Value) != true)
                ctx.StartSequenceType?.Invoke(sec, 0, sec.SeqType, 3, choice);
            return;
        }
        if (!string.IsNullOrEmpty(sec.SeqName))
        {
            if (curMode == null || ctx.SeqModesSameByName?.Invoke(sec.SeqName, choice, curMode.Value) != true)
                ctx.StartSequenceName?.Invoke(sec, 0, sec.SeqName, choice);
            return;
        }
        var snd = ResolveDefaultDoorSound(ctx.GameType, sec);
        if (curMode == null || ctx.SeqModesSameSound?.Invoke(snd, choice, curMode.Value) != true)
            ctx.StartSequenceSound?.Invoke(sec, 0, snd, choice);
    }

    private static string ResolveDefaultDoorSound(int gameType, DoorSector sec)
    {
        var snd = "DoorNormal";
        if (gameType == GameHeretic)
            return "HereticDoor";
        if (gameType != GameStrife)
            return snd;
        snd = "DoorSmallMetal";
        foreach (var line in sec.Lines)
        {
            if (line.BackSector == null)
                continue;
            var texname = line.Side0?.TopTexture ?? "";
            if (texname.Length < 4 || texname[0] != 'D' || texname[1] != 'O' || texname[2] != 'R')
                continue;
            switch (texname[3])
            {
                case 'S':
                    snd = "DoorStone";
                    break;
                case 'M':
                    if (texname.Length > 4 && texname[4] == 'L')
                        snd = "DoorLargeMetal";
                    break;
                case 'W':
                    if (texname.Length > 4 && texname[4] == 'L')
                        snd = "DoorLargeWood";
                    else
                        snd = "DoorSmallWood";
                    break;
            }
        }
        return snd;
    }
}
