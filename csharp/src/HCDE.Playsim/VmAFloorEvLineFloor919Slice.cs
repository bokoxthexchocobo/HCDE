namespace HCDE.Playsim;

/// <summary>
/// `EV_DoFloor`, `EV_FloorCrushStop`, `EV_StopFloor`, and `EV_BuildStairs` from <c>mapthinkers/a_floor.cpp</c> (**539–788**).
/// Exclusive to chunk **919**; defers `EV_DoDonut` (**796+**) and chunk **6** waggle **941+**.
/// </summary>
public static class VmAFloorEvLineFloor919Slice
{
    public static bool EvDoFloor(
        VmAFloorEv.FloorEvContext ctx,
        int floortype,
        VmAFloorEv.EvLine? line,
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

    public static bool EvFloorCrushStop(VmAFloorEv.FloorEvContext ctx, int tag, VmAFloorEv.EvLine? line)
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

    public static bool EvStopFloor(VmAFloorEv.FloorEvContext ctx, int tag, VmAFloorEv.EvLine? line)
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

    public static bool EvBuildStairs(
        VmAFloorEv.FloorEvContext ctx,
        int tag,
        int type,
        VmAFloorEv.EvLine? line,
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
        var compatible = tag != 0 && (ctx.CompatFlags & VmAFloorEv.CompatfStairindex) != 0;
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
            floor.Direction = type == VmAFloorEv.BuildUp ? 1 : -1;
            var stairstep = stairsize * floor.Direction;
            floor.Type = VmAFloorTickElevator.BuildStair;
            floor.ResetCount = reset;
            floor.OrgDist = sec.FloorPlaneD;
            floor.Delay = delay;
            floor.PauseTime = 0;
            floor.StepTime = persteptime;
            floor.PerStepTime = persteptime;
            floor.Instant = false;
            floor.Crush = (usespecials & VmAFloorEv.StairCrush) != 0 ? 10 : -1;
            floor.Hexencrush = true;
            floor.Speed = speed;
            var height = sec.CenterFloor + stairstep;
            floor.FloorDestDist = sec.PointToDist?.Invoke(sec.Centerspot.X, sec.Centerspot.Y, height) ?? 0;
            var texture = sec.FloorTexture;
            var osecnum = secnum;
            ctx.BumpValidcount?.Invoke(sec);
            if (ctx.BumpValidcount != null)
                sec.Validcount = 1;

            while (true)
            {
                var ok = false;
                VmAFloorEv.StairSector? tsec = null;
                var newsecnum = -1;
                if ((usespecials & VmAFloorEv.StairUseSpecials) != 0)
                {
                    var want = sec.Special == VmAFloorEv.StairsSpecial1 ? VmAFloorEv.StairsSpecial2 : VmAFloorEv.StairsSpecial1;
                    tsec = ctx.NextSpecialSector?.Invoke(sec, want);
                    ok = tsec != null;
                    if (ok && tsec != null)
                    {
                        tsec.Validcount = sec.Validcount;
                        newsecnum = tsec.SectorNum;
                        height += stairstep;
                        if (ctx.PlaneMovingFloor?.Invoke(tsec) == true || tsec.Stairlock != 0)
                        {
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
                        if ((ctx.CompatFlags & VmAFloorEv.CompatfStairindex) != 0)
                            height += stairstep;
                        if (ctx.PlaneMovingFloor?.Invoke(tsec) == true || tsec.Stairlock != 0)
                            continue;
                        if ((ctx.CompatFlags & VmAFloorEv.CompatfStairindex) == 0)
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
                sec = tsec;
                secnum = newsecnum;

                floor = ctx.CreateStairThinker?.Invoke(sec);
                if (floor == null)
                    break;
                ctx.StartFloorSound?.Invoke(floor);
                floor.Direction = type == VmAFloorEv.BuildUp ? 1 : -1;
                floor.FloorDestDist = sec.PointToDist?.Invoke(0, 0, height) ?? 0;
                floor.Delay = delay;
                floor.PauseTime = 0;
                floor.StepTime = persteptime;
                floor.PerStepTime = persteptime;
                if ((usespecials & VmAFloorEv.StairSync) != 0)
                {
                    var rise = height - sec.CenterFloor;
                    floor.Speed = speed * rise / stairstep;
                }
                else
                    floor.Speed = speed;
                floor.Type = VmAFloorTickElevator.BuildStair;
                floor.Crush = (usespecials & VmAFloorEv.StairUseSpecials) == 0 && speed == 4 ? 10 : -1;
                floor.Hexencrush = false;
                floor.ResetCount = reset;
                floor.OrgDist = sec.FloorPlaneD;
                floor.Instant = false;
            }
            var first = ctx.SectorByIndex?.Invoke(osecnum);
            if (first != null)
                first.Prevsec = -1;
            if (!compatible)
                break;
        }
        return rtn;
    }

    static VmAFloorCreate.FloorLine? ToCreateLine(VmAFloorEv.EvLine? line)
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
