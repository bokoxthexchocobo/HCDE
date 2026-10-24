namespace HCDE.Playsim;

/// <summary>Floor line specials in <c>a_floor.cpp</c> (539–552, 562–596, 607–788, 796–863, 1113–1154).</summary>
public static class VmAFloorEv
{
    public const int BuildUp = 0;
    public const int BuildDown = 1;
    public const int StairUseSpecials = 1;
    public const int StairSync = 2;
    public const int StairCrush = 4;
    public const int CompatfStairindex = 1;
    public const int Compatf2Floormove = 1;
    public const int TrigChangeOnly = 0;
    public const int NumChangeOnly = 1;
    public const int StairsSpecial1 = 26;
    public const int StairsSpecial2 = 27;

    public sealed class EvLine
    {
        public StairSector? FrontSector { get; set; }
    }

    public sealed class StairLine
    {
        public bool TwoSided { get; set; }
        public StairSector? FrontSector { get; set; }
        public StairSector? BackSector { get; set; }
    }

    public sealed class StairSector
    {
        public int SectorNum { get; set; }
        public int Stairlock { get; set; }
        public int Prevsec { get; set; } = -1;
        public int Nextsec { get; set; } = -1;
        public int Validcount { get; set; }
        public string FloorTexture { get; set; } = "";
        public double FloorPlaneD { get; set; }
        public VmAFloorTickElevator.VertexSpot Centerspot { get; } = new();
        public int Special { get; set; }
        public List<StairLine> Lines { get; } = new();
        public object? FloorData { get; set; }
        public Func<double, double, double, double>? PointToDist { get; set; }
        public double CenterFloor { get; set; }
    }

    public sealed class FloorEvContext
    {
        public int CompatFlags { get; init; }
        public int CompatFlags2 { get; init; }
        public Func<int, EvLine?, IEnumerable<int>>? SectorTagIterator { get; init; }
        public Func<int, StairSector?>? SectorByIndex { get; init; }
        public Func<int, int, IEnumerable<int>>? SectorTagIteratorCompat { get; init; }
        public VmAFloorCreate.CreateFloorContext? CreateFloor { get; init; }
        public Func<StairSector, VmAFloorCreate.FloorSector?>? ToCreateSector { get; init; }
        public Func<StairSector, bool>? PlaneMovingFloor { get; init; }
        public Func<StairSector, VmAFloorTickElevator.FloorThinker>? CreateStairThinker { get; init; }
        public Action<VmAFloorTickElevator.FloorThinker>? StartFloorSound { get; init; }
        public Func<StairSector, int, StairSector?>? NextSpecialSector { get; init; }
        public Func<StairSector, int>? BumpValidcount { get; init; }
        public Func<StairSector, VmAFloorTickElevator.VertexSpot, double>? FindHighestFloorPoint { get; init; }
        public Func<StairSector, StairLine, StairSector, StairSector?>? GetNextSector { get; init; }
        public Func<StairSector, bool>? IsFloorThinker { get; init; }
        public Func<StairSector, int>? GetFloorThinkerType { get; init; }
        public Action<StairSector>? StopSequence { get; init; }
        public Action<StairSector>? DestroyFloorData { get; init; }
        public Action<StairSector, string>? SetFloorTexture { get; init; }
        public Action<StairSector, StairSector>? TransferSpecial { get; init; }
        public Func<StairSector, double, StairSector?>? FindModelFloorSector { get; init; }
    }

    public static bool EvDoFloor(
        FloorEvContext ctx,
        int floortype,
        EvLine? line,
        int tag,
        double speed,
        double height,
        int crush,
        int change,
        bool hexencrush,
        bool hereticlower)
    {
        if (ctx.CreateFloor == null || ctx.ToCreateSector == null)
            return false;
        var rtn = false;
        foreach (var secnum in ctx.SectorTagIterator?.Invoke(tag, line) ?? Array.Empty<int>())
        {
            var stair = ctx.SectorByIndex?.Invoke(secnum);
            var sec = stair != null ? ctx.ToCreateSector(stair) : null;
            if (sec == null)
                continue;
            if (VmAFloorCreate.CreateFloor(ctx.CreateFloor, sec, floortype, ToCreateLine(line), speed, height, crush, change, hexencrush, hereticlower))
                rtn = true;
        }
        return rtn;
    }

    public static bool EvFloorCrushStop(FloorEvContext ctx, int tag, EvLine? line)
    {
        foreach (var secnum in ctx.SectorTagIterator?.Invoke(tag, line) ?? Array.Empty<int>())
        {
            var sec = ctx.SectorByIndex?.Invoke(secnum);
            if (sec?.FloorData == null)
                continue;
            if (ctx.IsFloorThinker?.Invoke(sec) == true &&
                ctx.GetFloorThinkerType?.Invoke(sec) == VmAFloorCreate.FloorRaiseAndCrush)
            {
                ctx.StopSequence?.Invoke(sec);
                ctx.DestroyFloorData?.Invoke(sec);
            }
        }
        return true;
    }

    public static bool EvStopFloor(FloorEvContext ctx, int tag, EvLine? line)
    {
        foreach (var secnum in ctx.SectorTagIterator?.Invoke(tag, line) ?? Array.Empty<int>())
        {
            var sec = ctx.SectorByIndex?.Invoke(secnum);
            if (sec?.FloorData == null)
                continue;
            ctx.StopSequence?.Invoke(sec);
            ctx.DestroyFloorData?.Invoke(sec);
        }
        return true;
    }

    public static bool EvDoChange(FloorEvContext ctx, EvLine? line, int changeType, int tag)
    {
        var rtn = false;
        foreach (var secnum in ctx.SectorTagIterator?.Invoke(tag, null) ?? Array.Empty<int>())
        {
            var sec = ctx.SectorByIndex?.Invoke(secnum);
            if (sec == null)
                continue;
            rtn = true;
            switch (changeType)
            {
                case TrigChangeOnly:
                    if (line?.FrontSector != null)
                    {
                        ctx.SetFloorTexture?.Invoke(sec, line.FrontSector.FloorTexture);
                        ctx.TransferSpecial?.Invoke(sec, line.FrontSector);
                    }
                    break;
                case NumChangeOnly:
                    var model = ctx.FindModelFloorSector?.Invoke(sec, sec.CenterFloor);
                    if (model != null)
                    {
                        ctx.SetFloorTexture?.Invoke(sec, model.FloorTexture);
                        ctx.TransferSpecial?.Invoke(sec, model);
                    }
                    break;
            }
        }
        return rtn;
    }

    public static bool EvDoDonut(FloorEvContext ctx, int tag, EvLine? line, double pillarspeed, double slimespeed)
    {
        var rtn = false;
        foreach (var secnum in ctx.SectorTagIterator?.Invoke(tag, line) ?? Array.Empty<int>())
        {
            var s1 = ctx.SectorByIndex?.Invoke(secnum);
            if (s1 == null || ctx.PlaneMovingFloor?.Invoke(s1) == true)
                continue;
            rtn = true;
            if (s1.Lines.Count == 0)
                continue;
            var s2 = ctx.GetNextSector?.Invoke(s1, s1.Lines[0], s1);
            if (s2 == null)
                continue;
            if ((ctx.CompatFlags2 & Compatf2Floormove) == 0 && ctx.PlaneMovingFloor?.Invoke(s2) == true)
                continue;

            foreach (var ln in s2.Lines)
            {
                if (ln.BackSector == null || ln.BackSector == s1)
                    continue;
                var s3 = ln.BackSector;
                var slime = ctx.CreateStairThinker?.Invoke(s2);
                if (slime != null)
                {
                    slime.Type = VmAFloorTickElevator.DonutRaise;
                    slime.Crush = -1;
                    slime.Direction = 1;
                    slime.Speed = slimespeed;
                    slime.Texture = s3.FloorTexture;
                    var spot = new VmAFloorTickElevator.VertexSpot();
                    var height = ctx.FindHighestFloorPoint?.Invoke(s3, spot) ?? 0;
                    slime.FloorDestDist = s2.PointToDist?.Invoke(spot.X, spot.Y, height) ?? 0;
                    ctx.StartFloorSound?.Invoke(slime);
                }
                var pillar = ctx.CreateStairThinker?.Invoke(s1);
                if (pillar != null)
                {
                    pillar.Type = VmAFloorCreate.FloorLowerToNearest;
                    pillar.Crush = -1;
                    pillar.Direction = -1;
                    pillar.Speed = pillarspeed;
                    var spot = new VmAFloorTickElevator.VertexSpot();
                    var height = ctx.FindHighestFloorPoint?.Invoke(s3, spot) ?? 0;
                    pillar.FloorDestDist = s1.PointToDist?.Invoke(spot.X, spot.Y, height) ?? 0;
                    ctx.StartFloorSound?.Invoke(pillar);
                }
                break;
            }
        }
        return rtn;
    }

    public static bool EvBuildStairs(
        FloorEvContext ctx,
        int tag,
        int type,
        EvLine? line,
        double stairsize,
        double speed,
        int delay,
        int reset,
        int igntxt,
        int usespecials)
    {
        if (speed == 0)
            return false;
        var persteptime = (int)(stairsize / speed);
        var rtn = false;
        var secnum = -1;
        var compatible = tag != 0 && (ctx.CompatFlags & CompatfStairindex) != 0;
        while (true)
        {
            var nextSecnums = ctx.SectorTagIteratorCompat?.Invoke(tag, secnum) ?? ctx.SectorTagIterator?.Invoke(tag, line) ?? Array.Empty<int>();
            var advanced = false;
            foreach (var n in nextSecnums)
            {
                if (compatible && n <= secnum)
                    continue;
                secnum = n;
                advanced = true;
                break;
            }
            if (!advanced)
                break;

            var sec = ctx.SectorByIndex?.Invoke(secnum);
            if (sec == null || ctx.PlaneMovingFloor?.Invoke(sec) == true || sec.Stairlock != 0)
                continue;

            rtn = true;
            var floor = ctx.CreateStairThinker?.Invoke(sec);
            if (floor == null)
                continue;
            floor.Direction = type == BuildUp ? 1 : -1;
            var stairstep = stairsize * floor.Direction;
            floor.Type = VmAFloorTickElevator.BuildStair;
            floor.ResetCount = reset;
            floor.OrgDist = sec.FloorPlaneD;
            floor.Delay = delay;
            floor.PauseTime = 0;
            floor.StepTime = persteptime;
            floor.PerStepTime = persteptime;
            floor.Crush = (usespecials & StairCrush) != 0 ? 10 : -1;
            floor.Hexencrush = true;
            floor.Speed = speed;
            var height = sec.CenterFloor + stairstep;
            floor.FloorDestDist = sec.PointToDist?.Invoke(sec.Centerspot.X, sec.Centerspot.Y, height) ?? 0;
            var texture = sec.FloorTexture;
            var osecnum = secnum;
            ctx.BumpValidcount?.Invoke(sec);
            sec.Validcount = ctx.BumpValidcount != null ? 1 : sec.Validcount;

            var prev = sec;
            while (true)
            {
                var ok = false;
                StairSector? tsec = null;
                var newsecnum = -1;
                if ((usespecials & StairUseSpecials) != 0)
                {
                    var want = sec.Special == StairsSpecial1 ? StairsSpecial2 : StairsSpecial1;
                    tsec = ctx.NextSpecialSector?.Invoke(sec, want);
                    ok = tsec != null;
                    if (ok && tsec != null)
                    {
                        newsecnum = tsec.SectorNum;
                        height += stairstep;
                        if (ctx.PlaneMovingFloor?.Invoke(tsec) == true || tsec.Stairlock != 0)
                        {
                            prev = sec;
                            sec = tsec;
                            continue;
                        }
                    }
                }
                else
                {
                    foreach (var ln in sec.Lines)
                    {
                        if (!ln.TwoSided)
                            continue;
                        tsec = ln.FrontSector;
                        newsecnum = tsec?.SectorNum ?? -1;
                        if (newsecnum != secnum)
                            continue;
                        tsec = ln.BackSector;
                        if (tsec == null)
                            continue;
                        newsecnum = tsec.SectorNum;
                        if (igntxt == 0 && tsec.FloorTexture != texture)
                            continue;
                        if ((ctx.CompatFlags & CompatfStairindex) != 0)
                            height += stairstep;
                        if (ctx.PlaneMovingFloor?.Invoke(tsec) == true || tsec.Stairlock != 0)
                            continue;
                        if ((ctx.CompatFlags & CompatfStairindex) == 0)
                            height += stairstep;
                        ok = true;
                        break;
                    }
                }

                if (!ok || tsec == null)
                    break;

                sec.Nextsec = newsecnum;
                tsec.Prevsec = secnum;
                tsec.Nextsec = -1;
                tsec.Stairlock = VmAFloorTickElevator.StairlockBuilding;
                prev = sec;
                sec = tsec;
                secnum = newsecnum;

                floor = ctx.CreateStairThinker?.Invoke(sec);
                if (floor == null)
                    break;
                ctx.StartFloorSound?.Invoke(floor);
                floor.Direction = type == BuildUp ? 1 : -1;
                floor.FloorDestDist = sec.PointToDist?.Invoke(0, 0, height) ?? 0;
                floor.Delay = delay;
                floor.PauseTime = 0;
                floor.StepTime = persteptime;
                floor.PerStepTime = persteptime;
                if ((usespecials & StairSync) != 0)
                {
                    var rise = height - sec.CenterFloor;
                    floor.Speed = speed * rise / stairstep;
                }
                else
                    floor.Speed = speed;
                floor.Type = VmAFloorTickElevator.BuildStair;
                floor.Crush = (usespecials & StairUseSpecials) == 0 && speed == 4 ? 10 : -1;
                floor.Hexencrush = false;
                floor.ResetCount = reset;
                floor.OrgDist = sec.FloorPlaneD;
            }
            var first = ctx.SectorByIndex?.Invoke(osecnum);
            if (first != null)
                first.Prevsec = -1;
            if (!compatible)
                break;
        }
        return rtn;
    }

    static VmAFloorCreate.FloorLine? ToCreateLine(EvLine? line)
    {
        if (line?.FrontSector == null)
            return null;
        return new VmAFloorCreate.FloorLine
        {
            FrontSector = new VmAFloorCreate.FloorSector
            {
                FloorTexture = line.FrontSector.FloorTexture,
                Special = new VmAFloorTickElevator.SectorSpecial { Tag = line.FrontSector.Special },
            },
        };
    }
}
