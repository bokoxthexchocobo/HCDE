namespace HCDE.Playsim;

/// <summary>3D midtex scroll/attach/opening helpers from <c>p_3dmidtex.cpp</c> (43–324).</summary>
public static class VmP3dMidtex
{
    public const int Ml3dMidtex = 1 << 0;
    public const int Ml3dMidtexImpass = 1 << 1;
    public const int MfMissile = 1 << 2;
    public const int BounceMbf = 1 << 3;

    public sealed class MidtexPlane
    {
        public List<LineStub> AttachedLines { get; } = new();
        public List<SectorStub> AttachedSectors { get; } = new();
    }

    public sealed class SectorStub
    {
        public int Index { get; init; }
        public MidtexPlane FloorMidtex { get; } = new();
        public MidtexPlane CeilingMidtex { get; } = new();
        public IEnumerable<LineStub> Lines { get; init; } = Array.Empty<LineStub>();
        public MidtexPlane Plane(bool ceiling) => ceiling ? CeilingMidtex : FloorMidtex;
    }

    public sealed class LineStub
    {
        public int Index { get; init; }
        public SectorStub? FrontSector { get; init; }
        public SectorStub? BackSector { get; init; }
        public int Flags { get; init; }
        public SideStub[] Sides { get; } = { new(), new() };
        public bool HasTwoSides => FrontSector != null && BackSector != null;
        public bool Is3dMidtex => (Flags & Ml3dMidtex) != 0;
    }

    public sealed class SideStub
    {
        public double MidTextureYOffset { get; set; }
        public void AddTextureYOffset(int _, double move) => MidTextureYOffset += move;
        public object? SetInterpolation(int _) => new object();
    }

    public sealed class ScrollContext
    {
        public required Func<SectorStub, int, double, int, bool, bool, bool> ChangeSector { get; init; }
    }

    public static bool Scroll3dMidtex(SectorStub sector, int crush, double move, bool ceiling, bool instant, ScrollContext ctx)
    {
        var scrollplane = sector.Plane(ceiling);
        foreach (var line in scrollplane.AttachedLines)
        {
            line.Sides[0].AddTextureYOffset(1, move);
            line.Sides[1].AddTextureYOffset(1, move);
        }
        var res = false;
        foreach (var sec in scrollplane.AttachedSectors)
            res |= ctx.ChangeSector(sec, crush, move, 2, true, instant);
        return !res;
    }

    public static void Start3dMidtexInterpolations(List<object> list, SectorStub sector, bool ceiling)
    {
        var scrollplane = sector.Plane(ceiling);
        foreach (var line in scrollplane.AttachedLines)
        {
            list.Add(line.Sides[0].SetInterpolation(1)!);
            list.Add(line.Sides[1].SetInterpolation(1)!);
        }
    }

    public sealed class AttachContext
    {
        public required IEnumerable<LineStub> AllLines { get; init; }
        public required IEnumerable<SectorStub> AllSectors { get; init; }
        public required Func<int, IEnumerable<int>> LineIdsByIterator { get; init; }
        public required Func<int, IEnumerable<int>> SectorIndicesByTag { get; init; }
        public required Func<LineStub, int, bool> LineHasId { get; init; }
    }

    public static void Attach3dMidtexLinesToSector(
        SectorStub sector,
        int lineid,
        int tag,
        bool ceiling,
        AttachContext ctx)
    {
        if (lineid == 0 && tag == 0)
            return;
        var scrollplane = sector.Plane(ceiling);
        var linesList = ctx.AllLines as IList<LineStub> ?? ctx.AllLines.ToList();
        var sectorsList = ctx.AllSectors as IList<SectorStub> ?? ctx.AllSectors.ToList();
        var lineCount = linesList.Count;
        var maxLineIndex = linesList.Count == 0 ? 0 : linesList.Max(l => l.Index);
        var maxSecIndex = sectorsList.Count == 0 ? 0 : sectorsList.Max(s => s.Index);
        var foundLines = new byte[(Math.Max(lineCount, maxLineIndex + 1) + 7) / 8];
        var foundSectors = new byte[(maxSecIndex + 8) / 8];
        foreach (var line in scrollplane.AttachedLines)
        {
            var idx = line.Index;
            foundLines[idx >> 3] |= (byte)(1 << (idx & 7));
        }
        foreach (var sec in scrollplane.AttachedSectors)
        {
            var idx = sec.Index;
            foundSectors[idx >> 3] |= (byte)(1 << (idx & 7));
        }
        scrollplane.AttachedLines.Clear();
        scrollplane.AttachedSectors.Clear();
        if (tag == 0)
        {
            foreach (var lineIndex in ctx.LineIdsByIterator(lineid))
            {
                var ln = ctx.AllLines.ElementAt(lineIndex);
                if (!ln.HasTwoSides || !ln.Is3dMidtex)
                    continue;
                foundLines[lineIndex >> 3] |= (byte)(1 << (lineIndex & 7));
            }
        }
        else
        {
            foreach (var secIndex in ctx.SectorIndicesByTag(tag))
            {
                var sec = ctx.AllSectors.ElementAt(secIndex);
                foreach (var ln in sec.Lines)
                {
                    if (lineid != 0 && !ctx.LineHasId(ln, lineid))
                        continue;
                    if (!ln.HasTwoSides || !ln.Is3dMidtex)
                        continue;
                    var lineno = ln.Index;
                    foundLines[lineno >> 3] |= (byte)(1 << (lineno & 7));
                }
            }
        }
        foreach (var line in linesList)
        {
            var i = line.Index;
            if ((foundLines[i >> 3] & (1 << (i & 7))) == 0)
                continue;
            scrollplane.AttachedLines.Add(line);
            MarkSector(foundSectors, line.FrontSector!.Index);
            MarkSector(foundSectors, line.BackSector!.Index);
        }
        foreach (var sec in sectorsList)
        {
            var i = sec.Index;
            if ((foundSectors[i >> 3] & (1 << (i & 7))) == 0)
                continue;
            scrollplane.AttachedSectors.Add(sec);
        }
    }

    static void MarkSector(byte[] foundSectors, int index)
    {
        foundSectors[index >> 3] |= (byte)(1 << (index & 7));
    }

    public sealed class TextureContext
    {
        public required Func<object, bool> TextureValid { get; init; }
        public required Func<object, double, double, (double RowOffset, double RenderHeight)> TexCoordInfo { get; init; }
        public required Func<SectorStub, int, double> PlaneTexZ { get; init; }
        public int LevelForceWorldPanning { get; init; }
        public int MlDontpegbottom { get; init; }
    }

    public static bool GetMidTexturePosition(
        LineStub line,
        int sideno,
        TextureContext tex,
        out double top,
        out double bottom)
    {
        top = bottom = 0;
        if (!line.HasTwoSides)
            return false;
        if (sideno < 0 || sideno > 1)
            return false;
        var side = line.Sides[sideno];
        var texnum = new object();
        if (!tex.TextureValid(texnum))
            return false;
        var yScale = 1.0;
        var (rowOffset, textureHeight) = tex.TexCoordInfo(texnum, yScale, side.MidTextureYOffset);
        var yOffset = rowOffset;
        if ((line.Flags & tex.MlDontpegbottom) != 0)
        {
            bottom = yOffset + Math.Max(
                tex.PlaneTexZ(line.FrontSector!, 0),
                tex.PlaneTexZ(line.BackSector!, 0));
            top = bottom + textureHeight;
        }
        else
        {
            top = yOffset + Math.Min(
                tex.PlaneTexZ(line.FrontSector!, 1),
                tex.PlaneTexZ(line.BackSector!, 1));
            bottom = top - textureHeight;
        }
        return true;
    }

    public sealed class LineOpening
    {
        public double Top { get; set; } = double.MaxValue;
        public double Bottom { get; set; } = double.MinValue;
        public bool AboveMidtex { get; set; }
        public object? FloorPic { get; set; }
        public object? CeilingPic { get; set; }
    }

    public sealed class ThingStub
    {
        public int Flags { get; init; }
        public int BounceFlags { get; init; }
        public double Z { get; init; }
        public double CenterZ { get; init; }
        public double MaxStepHeight { get; init; }
    }

    public static bool LineOpening3dMidtex(ThingStub thing, LineStub line, LineOpening open, bool restrict, TextureContext tex)
    {
        if ((line.Flags & Ml3dMidtexImpass) != 0
            && ((thing.Flags & MfMissile) != 0 || (thing.BounceFlags & BounceMbf) != 0))
            return false;
        if (!GetMidTexturePosition(line, 0, tex, out var tt, out var tb))
            return false;
        open.AboveMidtex = false;
        if (thing.CenterZ < (tt + tb) / 2)
        {
            if (tb < open.Top)
            {
                open.Top = tb;
                open.CeilingPic = new object();
            }
        }
        else
        {
            if (tt > open.Bottom && (!restrict || thing.Z >= tt))
            {
                open.Bottom = tt;
                open.AboveMidtex = true;
                open.FloorPic = new object();
            }
            return Math.Abs(thing.Z - tt) <= thing.MaxStepHeight;
        }
        return false;
    }
}
