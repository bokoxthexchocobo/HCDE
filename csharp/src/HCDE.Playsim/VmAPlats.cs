namespace HCDE.Playsim;

/// <summary><c>DPlat</c> tick/dispatch from <c>mapthinkers/a_plats.cpp</c> (76–468).</summary>
public static class VmAPlats
{
    public const int SecfSilentmove = 1 << 0;
    public const int PlanefBlocked = 1 << 0;

    public const int PlatUp = 0;
    public const int PlatDown = 1;
    public const int PlatWaiting = 2;
    public const int PlatInStasis = 3;

    public const int PlatPerpetualRaise = 0;
    public const int PlatDownWaitUpStay = 1;
    public const int PlatDownWaitUpStayStone = 2;
    public const int PlatUpWaitDownStay = 3;
    public const int PlatUpNearestWaitDownStay = 4;
    public const int PlatDownByValue = 5;
    public const int PlatUpByValue = 6;
    public const int PlatUpByValueStay = 7;
    public const int PlatRaiseAndStay = 8;
    public const int PlatToggle = 9;
    public const int PlatDownToNearestFloor = 10;
    public const int PlatDownToLowestCeiling = 11;
    public const int PlatRaiseAndStayLockout = 12;

    public sealed class PlatSector
    {
        public int Flags { get; set; }
        public int SeqType { get; set; } = -1;
        public string SeqName { get; set; } = "";
        public double FloorPlaneD { get; set; }
        public object Centerspot { get; init; } = new();
        public uint FloorPlaneFlags { get; set; }
        public Func<bool> PlaneMovingFloor { get; init; } = () => false;
        public Func<PlatSector, double, double, int, int, bool, VmADoorTickSound.MoveResult> MoveFloor { get; init; } =
            (_, _, _, _, _, _) => VmADoorTickSound.MoveResult.Ok;
        public Func<PlatSector, object, double> ZatPoint { get; init; } = (_, _) => 0;
        public Func<PlatSector, object, double, double> PointToDist { get; init; } = (_, _, _) => 0;
        public Action ClearSpecial { get; init; } = () => { };
        public Action<int, string?> SetFloorTextureFromLine { get; init; } = (_, _) => { };
        public Action StopSequence { get; init; } = () => { };
        public Action<string> StartNamedSequence { get; init; } = _ => { };
        public Action<int, string> StartSeqType { get; init; } = (_, _) => { };
    }

    public sealed class PlatThinker
    {
        public PlatSector Sector { get; set; } = null!;
        public int Type { get; set; }
        public double Speed { get; set; }
        public double Low { get; set; }
        public double High { get; set; }
        public int Wait { get; set; }
        public int Count { get; set; }
        public int Status { get; set; }
        public int OldStatus { get; set; }
        public int Crush { get; set; }
        public int Tag { get; set; }
        public Action DestroySelf { get; init; } = () => { };
    }

    public sealed class PlatLineContext
    {
        public object? Line { get; set; }
    }

    public sealed class HeightSpot
    {
        public object? Vertex { get; set; }
    }

    public sealed class EvDoPlatContext
    {
        public Func<int, PlatLineContext?, IEnumerable<int>> SectorTagIterator { get; init; } = (_, _) => Array.Empty<int>();
        public Func<PlatSector, PlatThinker> CreatePlat { get; init; } = s => new PlatThinker { Sector = s };
        public Func<PlatSector, HeightSpot, double> FindNextHighestFloor { get; init; } = (_, _) => 0;
        public Func<PlatSector, HeightSpot, double> FindLowestFloorSurrounding { get; init; } = (_, _) => 0;
        public Func<PlatSector, HeightSpot, double> FindHighestFloorSurrounding { get; init; } = (_, _) => 0;
        public Func<PlatSector, HeightSpot, double> FindNextLowestFloor { get; init; } = (_, _) => 0;
        public Func<PlatSector, HeightSpot, double> FindLowestCeilingSurrounding { get; init; } = (_, _) => 0;
        public Func<PlatSector, HeightSpot, double> FindLowestCeilingPoint { get; init; } = (_, _) => 0;
        public Func<int> RandomPlat { get; init; } = () => 0;
        public IReadOnlyList<PlatThinker> ExistingPlats { get; init; } = Array.Empty<PlatThinker>();
        public List<PlatSector> Sectors { get; init; } = new();
    }

    public static void PlayPlatSound(PlatThinker plat, string sound)
    {
        var sec = plat.Sector;
        if ((sec.Flags & SecfSilentmove) != 0) return;
        if (sec.SeqType >= 0)
            sec.StartSeqType(sec.SeqType, "SEQ_PLATFORM");
        else if (!string.IsNullOrEmpty(sec.SeqName))
            sec.StartNamedSequence(sec.SeqName);
        else
            sec.StartNamedSequence(sound);
    }

    public static string GetSoundByType(PlatThinker plat) =>
        plat.Type == PlatDownWaitUpStayStone ? "Floor" : "Platform";

    public static void Tick(PlatThinker plat)
    {
        var sec = plat.Sector;
        VmADoorTickSound.MoveResult res;
        switch (plat.Status)
        {
            case PlatUp:
                res = sec.MoveFloor(sec, plat.Speed, plat.High, plat.Crush, 1, false);
                if (res == VmADoorTickSound.MoveResult.Crushed && plat.Crush == -1)
                {
                    plat.Count = plat.Wait;
                    plat.Status = PlatDown;
                    PlayPlatSound(plat, GetSoundByType(plat));
                }
                else if (res == VmADoorTickSound.MoveResult.PastDest)
                {
                    sec.StopSequence();
                    if (plat.Type != PlatToggle)
                    {
                        plat.Count = plat.Wait;
                        plat.Status = PlatWaiting;
                        switch (plat.Type)
                        {
                            case PlatRaiseAndStayLockout:
                                sec.FloorPlaneFlags |= PlanefBlocked;
                                plat.DestroySelf();
                                break;
                            case PlatRaiseAndStay:
                            case PlatDownByValue:
                            case PlatDownWaitUpStay:
                            case PlatDownWaitUpStayStone:
                            case PlatUpByValueStay:
                            case PlatDownToNearestFloor:
                            case PlatDownToLowestCeiling:
                                plat.DestroySelf();
                                break;
                        }
                    }
                    else
                    {
                        plat.OldStatus = plat.Status;
                        plat.Status = PlatInStasis;
                    }
                }
                break;
            case PlatDown:
                res = sec.MoveFloor(sec, plat.Speed, plat.Low, -1, -1, false);
                if (res == VmADoorTickSound.MoveResult.PastDest)
                {
                    sec.StopSequence();
                    if (plat.Type != PlatToggle)
                    {
                        plat.Count = plat.Wait;
                        plat.Status = PlatWaiting;
                        if (plat.Type is PlatUpWaitDownStay or PlatUpNearestWaitDownStay or PlatUpByValue)
                            plat.DestroySelf();
                    }
                    else
                    {
                        plat.OldStatus = plat.Status;
                        plat.Status = PlatInStasis;
                    }
                }
                else if (res == VmADoorTickSound.MoveResult.Crushed && plat.Crush < 0 && plat.Type != PlatToggle)
                {
                    plat.Status = PlatUp;
                    plat.Count = plat.Wait;
                    PlayPlatSound(plat, GetSoundByType(plat));
                }

                if (plat.Type is PlatUpByValueStay or PlatRaiseAndStay or PlatRaiseAndStayLockout)
                    plat.DestroySelf();
                break;
            case PlatWaiting:
                if (plat.Count > 0 && --plat.Count == 0)
                {
                    plat.Status = sec.FloorPlaneD == plat.Low ? PlatUp : PlatDown;
                    if (plat.Type == PlatToggle)
                        sec.StartNamedSequence("Silence");
                    else
                        PlayPlatSound(plat, GetSoundByType(plat));
                }
                break;
        }
    }

    public static void Reactivate(PlatThinker plat)
    {
        if (plat.Type == PlatToggle)
            plat.Status = plat.OldStatus == PlatUp ? PlatDown : PlatUp;
        else
            plat.Status = plat.OldStatus;
    }

    public static void ActivateInStasisPlat(int tag, IReadOnlyList<PlatThinker> plats)
    {
        foreach (var scan in plats)
        {
            if (scan.Tag == tag && scan.Status == PlatInStasis)
                Reactivate(scan);
        }
    }

    public static bool EvDoPlat(
        EvDoPlatContext ctx,
        int tag,
        PlatLineContext? line,
        int type,
        double height,
        double speed,
        int delay,
        int lip,
        int change)
    {
        var rtn = false;
        if (tag != 0)
        {
            switch (type)
            {
                case PlatToggle:
                    rtn = true;
                    ActivateInStasisPlat(tag, ctx.ExistingPlats);
                    break;
                case PlatPerpetualRaise:
                    ActivateInStasisPlat(tag, ctx.ExistingPlats);
                    break;
            }
        }

        foreach (var secnum in ctx.SectorTagIterator(tag, line))
        {
            var sec = ctx.Sectors[secnum];
            if (sec.PlaneMovingFloor()) continue;
            rtn = true;
            var plat = ctx.CreatePlat(sec);
            plat.Type = type;
            plat.Crush = -1;
            plat.Tag = tag;
            plat.Speed = speed;
            plat.Wait = delay;
            plat.Low = sec.FloorPlaneD;
            if (change != 0)
            {
                if (line?.Line != null)
                    sec.SetFloorTextureFromLine(change, null);
                if (change == 1) sec.ClearSpecial();
            }

            var spot = new HeightSpot();
            double newheight;
            switch (type)
            {
                case PlatRaiseAndStay:
                case PlatRaiseAndStayLockout:
                    newheight = ctx.FindNextHighestFloor(sec, spot);
                    plat.High = sec.PointToDist(sec, spot.Vertex!, newheight);
                    plat.Low = sec.FloorPlaneD;
                    plat.Status = PlatUp;
                    PlayPlatSound(plat, "Floor");
                    sec.ClearSpecial();
                    break;
                case PlatUpByValue:
                case PlatUpByValueStay:
                    newheight = sec.ZatPoint(sec, sec.Centerspot) + height;
                    plat.High = sec.PointToDist(sec, sec.Centerspot, newheight);
                    plat.Low = sec.FloorPlaneD;
                    plat.Status = PlatUp;
                    PlayPlatSound(plat, "Floor");
                    break;
                case PlatDownByValue:
                    newheight = sec.ZatPoint(sec, sec.Centerspot) - height;
                    plat.Low = sec.PointToDist(sec, sec.Centerspot, newheight);
                    plat.High = sec.FloorPlaneD;
                    plat.Status = PlatDown;
                    PlayPlatSound(plat, "Floor");
                    break;
                case PlatDownWaitUpStay:
                case PlatDownWaitUpStayStone:
                    newheight = ctx.FindLowestFloorSurrounding(sec, spot) + lip;
                    plat.Low = sec.PointToDist(sec, spot.Vertex!, newheight);
                    if (plat.Low < sec.FloorPlaneD) plat.Low = sec.FloorPlaneD;
                    plat.High = sec.FloorPlaneD;
                    plat.Status = PlatDown;
                    PlayPlatSound(plat, GetSoundByType(plat));
                    break;
                case PlatUpNearestWaitDownStay:
                    newheight = ctx.FindNextHighestFloor(sec, spot);
                    plat.High = sec.PointToDist(sec, spot.Vertex!, newheight);
                    plat.Low = sec.FloorPlaneD;
                    if (plat.High > sec.FloorPlaneD) plat.High = sec.FloorPlaneD;
                    plat.Status = PlatUp;
                    PlayPlatSound(plat, "Platform");
                    break;
                case PlatUpWaitDownStay:
                    newheight = ctx.FindHighestFloorSurrounding(sec, spot);
                    plat.High = sec.PointToDist(sec, spot.Vertex!, newheight);
                    plat.Low = sec.FloorPlaneD;
                    if (plat.High > sec.FloorPlaneD) plat.High = sec.FloorPlaneD;
                    plat.Status = PlatUp;
                    PlayPlatSound(plat, "Platform");
                    break;
                case PlatPerpetualRaise:
                    newheight = ctx.FindLowestFloorSurrounding(sec, spot) + lip;
                    plat.Low = sec.PointToDist(sec, spot.Vertex!, newheight);
                    if (plat.Low < sec.FloorPlaneD) plat.Low = sec.FloorPlaneD;
                    newheight = ctx.FindHighestFloorSurrounding(sec, spot);
                    plat.High = sec.PointToDist(sec, spot.Vertex!, newheight);
                    if (plat.High > sec.FloorPlaneD) plat.High = sec.FloorPlaneD;
                    plat.Status = (ctx.RandomPlat() & 1) != 0 ? PlatUp : PlatDown;
                    PlayPlatSound(plat, "Platform");
                    break;
                case PlatToggle:
                    plat.Crush = 10;
                    newheight = ctx.FindLowestCeilingPoint(sec, spot);
                    plat.Low = sec.PointToDist(sec, spot.Vertex!, newheight);
                    plat.High = sec.FloorPlaneD;
                    plat.Status = PlatDown;
                    sec.StartNamedSequence("Silence");
                    break;
                case PlatDownToNearestFloor:
                    newheight = ctx.FindNextLowestFloor(sec, spot) + lip;
                    plat.Low = sec.PointToDist(sec, spot.Vertex!, newheight);
                    plat.Status = PlatDown;
                    plat.High = sec.FloorPlaneD;
                    PlayPlatSound(plat, "Platform");
                    break;
                case PlatDownToLowestCeiling:
                    newheight = ctx.FindLowestCeilingSurrounding(sec, spot);
                    plat.Low = sec.PointToDist(sec, spot.Vertex!, newheight);
                    plat.High = sec.FloorPlaneD;
                    if (plat.Low < sec.FloorPlaneD) plat.Low = sec.FloorPlaneD;
                    plat.Status = PlatDown;
                    PlayPlatSound(plat, "Platform");
                    break;
            }
        }

        return rtn;
    }
}
