namespace HCDE.Playsim;

/// <summary>
/// Minisegs, splitter events, and loop angles from <c>nodebuild_gl.cpp</c> (1-382).
/// Chunk 1322. The event tree is chunk 1218. SplitSeg's vertex unlink is the
/// callee this file uses, so a forced sharer split keeps both vertex chains.
/// </summary>
public sealed class VmNodeGl1322
{
    public const uint None = uint.MaxValue;
    public const uint NoSide = 0xffffffffu;
    public const uint Angle180 = 0x80000000;
    public const uint AngleMax = 0xffffffff;
    public const uint AngleEpsilon = 5000;
    const double SideEpsilon = 6.5536;
    const double SideNear = 17179869184.0;

    public List<Vert> Vertices { get; } = new();
    public List<Seg> Segs { get; } = new();
    public List<Sharer> SplitSharers { get; } = new();
    public List<string> Warnings { get; } = new();
    public VmEventTree1218 Events { get; } = new();

    public double AddIntersection(Node node, int vertex)
    {
        Vert v = Vertices[vertex];
        double dist = ((double)v.X - node.X) * node.Dx + ((double)v.Y - node.Y) * node.Dy;
        if (Events.FindEvent(dist) == null)
        {
            VmEventNode1218 ev = Events.GetNewNode();
            ev.Distance = dist;
            ev.Vertex = vertex;
            ev.FrontSeg = None;
            Events.Insert(ev);
        }
        return dist;
    }

    public void FixSplitSharers(Node node)
    {
        for (int i = 0; i < SplitSharers.Count; i++)
        {
            uint seg = SplitSharers[i].Seg;
            int v2 = Segs[(int)seg].V2;
            VmEventNode1218? ev = Events.FindEvent(SplitSharers[i].Distance);
            if (ev == null)
                continue;
            VmEventNode1218? next;
            if (SplitSharers[i].Forward)
            {
                ev = After(ev);
                if (ev == null)
                    continue;
                next = After(ev);
            }
            else
            {
                ev = Before(ev);
                if (ev == null)
                    continue;
                next = Before(ev);
            }
            while (ev != null && next != null && ev.Vertex != v2)
            {
                uint created = SplitSeg(seg, ev.Vertex, 1);
                Segs[(int)created].Next = Segs[(int)seg].Next;
                Segs[(int)seg].Next = created;
                uint partner = Segs[(int)seg].Partner;
                if (partner != None)
                {
                    uint endPartner = SplitSeg(partner, ev.Vertex, 1);
                    Segs[(int)endPartner].Next = Segs[(int)partner].Next;
                    Segs[(int)partner].Next = endPartner;
                    Segs[(int)seg].Partner = endPartner;
                    Segs[(int)partner].Partner = created;
                }
                seg = created;
                if (SplitSharers[i].Forward)
                {
                    ev = next;
                    next = After(next);
                }
                else
                {
                    ev = next;
                    next = Before(next);
                }
            }
        }
    }

    public void AddMinisegs(Node node, uint splitseg, ref uint fset, ref uint bset)
    {
        VmEventNode1218? ev = Events.GetMinimum();
        VmEventNode1218? prev = null;
        while (ev != null)
        {
            if (prev != null)
            {
                uint fseg1 = CheckLoopStart(node.Dx, node.Dy, prev.Vertex, ev.Vertex);
                uint bseg1 = CheckLoopStart(-node.Dx, -node.Dy, ev.Vertex, prev.Vertex);
                uint fseg2 = CheckLoopEnd(node.Dx, node.Dy, ev.Vertex);
                uint bseg2 = CheckLoopEnd(-node.Dx, -node.Dy, prev.Vertex);
                if (fseg1 != None && bseg1 != None && fseg2 != None && bseg2 != None)
                {
                    uint front = AddMiniseg(prev.Vertex, ev.Vertex, None, fseg1, splitseg);
                    Segs[(int)front].Next = fset;
                    fset = front;
                    uint back = AddMiniseg(ev.Vertex, prev.Vertex, front, bseg1, splitseg);
                    Segs[(int)back].Next = bset;
                    bset = back;
                    Sector? frontSector = Segs[(int)fseg1].Front;
                    Sector? backSector = Segs[(int)bseg1].Front;
                    Segs[(int)front].Front = frontSector;
                    Segs[(int)front].Back = backSector;
                    Segs[(int)back].Front = backSector;
                    Segs[(int)back].Back = frontSector;
                    if (!ReferenceEquals(frontSector, backSector) &&
                        !ReferenceEquals(frontSector, Segs[(int)fseg1].Back) &&
                        !ReferenceEquals(backSector, Segs[(int)bseg1].Back))
                    {
                        Warnings.Add(prev.Vertex + ":" + ev.Vertex);
                    }
                }
            }
            prev = ev;
            ev = After(ev);
        }
    }

    public uint AddMiniseg(int v1, int v2, uint partner, uint seg1, uint splitseg)
    {
        Seg source = Segs[(int)seg1];
        var created = new Seg
        {
            Side = NoSide,
            Line = -1,
            Loop = 0,
            PlaneFront = true,
            Stored = None,
            Front = null,
            Back = null,
            Plane = splitseg != None ? Segs[(int)splitseg].Plane : -1,
            V1 = v1,
            V2 = v2,
            NextForVert = Vertices[v1].Segs,
            NextForVert2 = Vertices[v2].Segs2,
            Next = source.Next,
            Partner = partner
        };
        uint nseg = (uint)Segs.Count;
        Segs.Add(created);
        if (created.Partner != None)
            Segs[(int)partner].Partner = nseg;
        Vertices[v1].Segs = nseg;
        Vertices[v2].Segs2 = nseg;
        return nseg;
    }

    public uint CheckLoopStart(int dx, int dy, int vertex, int vertex2)
    {
        Vert v = Vertices[vertex];
        uint splitAngle = PointToAngle(dx, dy);
        uint segnum = v.Segs2;
        uint bestang = AngleMax;
        uint bestseg = None;
        while (segnum != None)
        {
            Seg seg = Segs[(int)segnum];
            Vert from = Vertices[seg.V1];
            uint segAngle = PointToAngle(from.X - v.X, from.Y - v.Y);
            uint diff = splitAngle - segAngle;
            bool onSplitter = diff < AngleEpsilon &&
                PointOnSide(from.X, from.Y, v.X, v.Y, dx, dy) == 0;
            if (!onSplitter && diff <= bestang)
            {
                bestang = diff;
                bestseg = segnum;
            }
            segnum = seg.NextForVert2;
        }
        if (bestseg == None)
            return None;
        segnum = v.Segs;
        while (segnum != None)
        {
            Seg seg = Segs[(int)segnum];
            if (seg.V2 == vertex2)
                return None;
            Vert to = Vertices[seg.V2];
            uint segAngle = PointToAngle(to.X - v.X, to.Y - v.Y);
            uint diff = splitAngle - segAngle;
            if (diff < bestang && seg.Partner != bestseg)
                return None;
            segnum = seg.NextForVert;
        }
        return bestseg;
    }

    public uint CheckLoopEnd(int dx, int dy, int vertex)
    {
        Vert v = Vertices[vertex];
        uint splitAngle = PointToAngle(dx, dy) + Angle180;
        uint segnum = v.Segs;
        uint bestang = AngleMax;
        uint bestseg = None;
        while (segnum != None)
        {
            Seg seg = Segs[(int)segnum];
            Vert to = Vertices[seg.V2];
            uint segAngle = PointToAngle(to.X - v.X, to.Y - v.Y);
            uint diff = segAngle - splitAngle;
            bool onSplitter = diff < AngleEpsilon &&
                PointOnSide(Vertices[seg.V1].X, Vertices[seg.V1].Y, v.X, v.Y, dx, dy) == 0;
            if (!onSplitter && diff <= bestang)
            {
                bestang = diff;
                bestseg = segnum;
            }
            segnum = seg.NextForVert;
        }
        if (bestseg == None)
            return None;
        segnum = v.Segs2;
        while (segnum != None)
        {
            Seg seg = Segs[(int)segnum];
            Vert from = Vertices[seg.V1];
            uint segAngle = PointToAngle(from.X - v.X, from.Y - v.Y);
            uint diff = segAngle - splitAngle;
            if (diff < bestang && seg.Partner != bestseg)
                return None;
            segnum = seg.NextForVert2;
        }
        return bestseg;
    }

    public static uint PointToAngle(int x, int y)
    {
        double rad2bam = (double)(1 << 30) / Math.PI;
        int truncated = (int)(Math.Atan2(y, x) * rad2bam);
        return (uint)truncated << 1;
    }

    public static int PointOnSide(int x, int y, int x1, int y1, int dx, int dy)
    {
        double ddx = dx;
        double ddy = dy;
        double sNum = ((double)y1 - y) * ddx - ((double)x1 - x) * ddy;
        if (Math.Abs(sNum) < SideNear)
        {
            double length = ddx * ddx + ddy * ddy;
            double dist = sNum * sNum / length;
            if (dist < SideEpsilon * SideEpsilon)
                return 0;
        }
        if (sNum > 0.0)
            return -1;
        return 1;
    }

    public uint SplitSeg(uint segnum, int splitvert, int v1InFront)
    {
        Seg created = Segs[(int)segnum].Copy();
        uint newnum = (uint)Segs.Count;
        if (v1InFront > 0)
        {
            created.V1 = splitvert;
            Segs[(int)segnum].V2 = splitvert;
            RemoveSegFromVert2(segnum, created.V2);
            created.NextForVert = Vertices[splitvert].Segs;
            Vertices[splitvert].Segs = newnum;
            created.NextForVert2 = Vertices[created.V2].Segs2;
            Vertices[created.V2].Segs2 = newnum;
            Segs[(int)segnum].NextForVert2 = Vertices[splitvert].Segs2;
            Vertices[splitvert].Segs2 = segnum;
        }
        else
        {
            int oldV1 = created.V1;
            Segs[(int)segnum].V1 = splitvert;
            created.V2 = splitvert;
            RemoveSegFromVert1(segnum, oldV1);
            created.NextForVert = Vertices[oldV1].Segs;
            Vertices[oldV1].Segs = newnum;
            created.NextForVert2 = Vertices[splitvert].Segs2;
            Vertices[splitvert].Segs2 = newnum;
            Segs[(int)segnum].NextForVert = Vertices[splitvert].Segs;
            Vertices[splitvert].Segs = segnum;
        }
        Segs.Add(created);
        return newnum;
    }

    void RemoveSegFromVert1(uint segnum, int vertnum)
    {
        Vert v = Vertices[vertnum];
        if (v.Segs == segnum)
        {
            v.Segs = Segs[(int)segnum].NextForVert;
            return;
        }
        uint prev = 0;
        uint curr = v.Segs;
        while (curr != None && curr != segnum)
        {
            prev = curr;
            curr = Segs[(int)curr].NextForVert;
        }
        if (curr == segnum)
            Segs[(int)prev].NextForVert = Segs[(int)curr].NextForVert;
    }

    void RemoveSegFromVert2(uint segnum, int vertnum)
    {
        Vert v = Vertices[vertnum];
        if (v.Segs2 == segnum)
        {
            v.Segs2 = Segs[(int)segnum].NextForVert2;
            return;
        }
        uint prev = 0;
        uint curr = v.Segs2;
        while (curr != None && curr != segnum)
        {
            prev = curr;
            curr = Segs[(int)curr].NextForVert2;
        }
        if (curr == segnum)
            Segs[(int)prev].NextForVert2 = Segs[(int)curr].NextForVert2;
    }

    VmEventNode1218? After(VmEventNode1218 node)
    {
        VmEventNode1218 next = Events.Successor(node);
        if (ReferenceEquals(next, Events.NilNode()))
            return null;
        return next;
    }

    VmEventNode1218? Before(VmEventNode1218 node)
    {
        VmEventNode1218 next = Events.Predecessor(node);
        if (ReferenceEquals(next, Events.NilNode()))
            return null;
        return next;
    }

    public sealed class Vert
    {
        public int X;
        public int Y;
        public uint Segs = None;
        public uint Segs2 = None;
    }

    public sealed class Seg
    {
        public int V1;
        public int V2;
        public uint Side = NoSide;
        public int Line = -1;
        public int Loop;
        public uint Next = None;
        public uint NextForVert = None;
        public uint NextForVert2 = None;
        public uint Partner = None;
        public uint Stored = None;
        public int Plane = -1;
        public bool PlaneFront = true;
        public Sector? Front;
        public Sector? Back;

        public Seg Copy()
        {
            return new Seg
            {
                V1 = V1,
                V2 = V2,
                Side = Side,
                Line = Line,
                Loop = Loop,
                Next = Next,
                NextForVert = NextForVert,
                NextForVert2 = NextForVert2,
                Partner = Partner,
                Stored = Stored,
                Plane = Plane,
                PlaneFront = PlaneFront,
                Front = Front,
                Back = Back
            };
        }
    }

    public sealed class Sector
    {
        public int Id;
    }

    public readonly struct Node
    {
        public int X { get; init; }
        public int Y { get; init; }
        public int Dx { get; init; }
        public int Dy { get; init; }
    }

    public sealed class Sharer
    {
        public double Distance;
        public uint Seg;
        public bool Forward;
    }
}
