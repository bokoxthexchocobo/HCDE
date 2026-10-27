namespace HCDE.Playsim;

/// <summary>`EV_DoChange` from <c>mapthinkers/a_floor.cpp</c> (**1098–1154**). Exclusive to chunk **920**; donut on **921**.</summary>
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
}
