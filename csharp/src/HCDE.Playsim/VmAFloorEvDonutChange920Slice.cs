namespace HCDE.Playsim;

/// <summary>
/// `EV_DoDonut` and `EV_DoChange` from <c>mapthinkers/a_floor.cpp</c> (**796–863**, **1113–1154**).
/// Exclusive to chunk **920**; shared context types on <see cref="VmAFloorEv"/>.
/// </summary>
public static class VmAFloorEvDonutChange920Slice
{
    public static bool EvDoChange(VmAFloorEv.FloorEvContext ctx, VmAFloorEv.EvLine? line, int changeType, int tag)
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
                case VmAFloorEv.TrigChangeOnly:
                    if (line?.FrontSector != null)
                    {
                        ctx.SetFloorTexture?.Invoke(sec, line.FrontSector.FloorTexture);
                        ctx.TransferSpecial?.Invoke(sec, line.FrontSector);
                    }
                    break;
                case VmAFloorEv.NumChangeOnly:
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

    public static bool EvDoDonut(VmAFloorEv.FloorEvContext ctx, int tag, VmAFloorEv.EvLine? line, double pillarspeed, double slimespeed)
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
            if ((ctx.CompatFlags2 & VmAFloorEv.Compatf2Floormove) == 0 && ctx.PlaneMovingFloor?.Invoke(s2) == true)
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
                    slime.Hexencrush = false;
                    slime.Direction = 1;
                    slime.Speed = slimespeed;
                    slime.Instant = false;
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
                    pillar.Hexencrush = false;
                    pillar.Direction = -1;
                    pillar.Speed = pillarspeed;
                    pillar.Instant = false;
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
}
