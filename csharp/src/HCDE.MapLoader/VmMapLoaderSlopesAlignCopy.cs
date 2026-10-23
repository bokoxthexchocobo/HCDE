namespace HCDE.MapLoader;

/// <summary><c>slopes.cpp</c> line align / copy passes (439–588).</summary>
public static partial class VmMapLoaderSlopes
{
    public static void AlignPlane(SlopeLevel level, SlopeSector sec, SlopeLine line, int which)
    {
        if (line.BackSector is null)
            return;

        var refvert = sec.Lines[0].V1;
        var bestdist = 0.0;
        foreach (var ln in sec.Lines)
        {
            for (var i = 0; i < 2; i++)
            {
                var vert = i == 0 ? ln.V1 : ln.V2;
                var dist = Math.Abs(
                    (line.V1.Y - vert.Y) * line.Delta().X -
                    (line.V1.X - vert.X) * line.Delta().Y);
                if (dist > bestdist)
                {
                    bestdist = dist;
                    refvert = vert;
                }
            }
        }

        var refsec = line.FrontSector == sec ? line.BackSector : line.FrontSector;
        if (refsec is null)
            return;

        var srcplane = which == 0 ? sec.FloorPlane : sec.CeilingPlane;
        var srcheight = sec.GetPlaneTexZ(which == 1);
        var destheight = refsec.GetPlaneTexZ(which == 1);

        var v1 = (line.Delta().X, line.Delta().Y, 0.0);
        var v2 = (
            refvert.X - line.V1.X,
            refvert.Y - line.V1.Y,
            srcheight - destheight);
        var cross = Unit(Cross(v1, v2));

        if ((cross.Z < 0 && which == 0) || (cross.Z > 0 && which == 1))
            cross = (-cross.X, -cross.Y, -cross.Z);

        var distPlane = -cross.X * line.V1.X - cross.Y * line.V1.Y - cross.Z * destheight;
        srcplane.Set(cross.X, cross.Y, cross.Z, distPlane);
    }

    public static void SetSlopes(SlopeLevel level)
    {
        foreach (var line in level.Lines)
        {
            if (line.Special != PlaneAlign)
                continue;
            line.Special = 0;
            if (line.BackSector is null)
                continue;
            for (var s = 0; s < 2; s++)
            {
                var bits = line.Args[s] & 3;
                if (s == 1 && bits == 0)
                    bits = (line.Args[0] >> 2) & 3;
                if (bits == 1 && line.FrontSector is not null)
                    AlignPlane(level, line.FrontSector, line, s);
                else if (bits == 2 && line.BackSector is not null)
                    AlignPlane(level, line.BackSector, line, s);
            }
        }
    }

    public static void CopySlopes(SlopeLevel level)
    {
        foreach (var line in level.Lines)
        {
            if (line.Special != PlaneCopy)
                continue;
            line.Special = 0;
            var limit = line.BackSector is not null ? 4 : 2;
            for (var s = 0; s < limit; s++)
            {
                if (line.Args[s] == 0)
                    continue;
                var dest = (s & 2) == 2 ? line.BackSector : line.FrontSector;
                if (dest is null)
                    continue;
                CopyPlane(level, line.Args[s], dest, (s & 1) == 1);
            }

            if (line.BackSector is null || line.FrontSector is null)
                continue;
            if ((line.Args[4] & 3) == 1)
                line.BackSector.FloorPlane.CopyFrom(line.FrontSector.FloorPlane);
            else if ((line.Args[4] & 3) == 2)
                line.FrontSector.FloorPlane.CopyFrom(line.BackSector.FloorPlane);
            if ((line.Args[4] & 12) == 4)
                line.BackSector.CeilingPlane.CopyFrom(line.FrontSector.CeilingPlane);
            else if ((line.Args[4] & 12) == 8)
                line.FrontSector.CeilingPlane.CopyFrom(line.BackSector.CeilingPlane);
        }
    }
}
