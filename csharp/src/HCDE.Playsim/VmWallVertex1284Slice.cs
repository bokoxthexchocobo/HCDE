namespace HCDE.Playsim;

/// <summary>
/// Wall vertex splits from <c>hw_walls_vertex.cpp</c> (1-288). Chunk 1284.
/// The ceiling and floor slope factors are computed and discarded, matching
/// the native functions that calculate them and never store them.
/// </summary>
public sealed class VmWallUv1284
{
    public float U;
    public float V;
}

public sealed class VmWallSeg1284
{
    public float SideFrac;
    public float X2;
    public float Y2;
}

public sealed class VmWallVertex1284
{
    public float X;
    public float Z;
    public float Y;
    public float U;
    public float V;
    public float Lu;
    public float Lv;
    public float LIndex;
}

public sealed class VmWallSide1284
{
    public int Flags;
    public int NumSegs;
    public List<VmWallSeg1284> Segs = new();
}

public sealed class VmWallHeight1284
{
    public int NumHeights;
    public float[] Heights = System.Array.Empty<float>();
}

public sealed class VmWallVertexBuild1284
{
    public const int NoSplitUpper = 16;
    public const int NoSplitLower = 32;
    public const int NoSplit = 64;
    public const int WallPoly = 64;
    public const int LoLeft = 0;
    public const int UpLeft = 1;
    public const int UpRight = 2;
    public const int LoRight = 3;

    public float X1;
    public float Y1;
    public float X2;
    public float Y2;
    public float FracLeft;
    public float FracRight = 1;
    public float[] ZTop = new float[2];
    public float[] ZBottom = new float[2];
    public float[] ZCeil = new float[2];
    public float[] ZFloor = new float[2];
    public VmWallUv1284[] Tex = { new(), new(), new(), new() };
    public VmWallUv1284[] Light = { new(), new(), new(), new() };
    public float LIndex;
    public int Flags;
    public bool Seamless = true;
    public VmWallSide1284? Side;
    public VmWallHeight1284? LeftHeights;
    public VmWallHeight1284? RightHeights;
    public int VertCount;
    public int VertIndex;
    public int Allocated;
    public int ReserveCursor;
    public List<VmWallVertex1284> Vertices = new();

    public void MakeVertices(bool noSplit)
    {
        if (VertCount != 0)
            return;
        bool split = Seamless && !noSplit && Side != null && (Side.Flags & WallPoly) == 0 && (Flags & NoSplit) == 0;
        int count = split ? CountVertices() : 4;
        Allocated = count;
        VertIndex = ReserveCursor;
        ReserveCursor += count;
        int before = Vertices.Count;
        CreateVertices(split);
        VertCount = Vertices.Count - before;
    }

    public int CountVertices()
    {
        uint ptr = 4;
        if (FracLeft == 0)
            CountLeftEdge(ref ptr);
        if (FracRight == 1)
            CountRightEdge(ref ptr);
        if ((Flags & NoSplitUpper) == 0)
            ptr += (uint)(Side!.NumSegs - 1);
        if ((Flags & NoSplitLower) == 0)
            ptr += (uint)(Side!.NumSegs - 1);
        return (int)ptr;
    }

    public int CreateVertices(bool split)
    {
        int before = Vertices.Count;
        Add(X1, ZBottom[0], Y1, Tex[LoLeft], Light[LoLeft]);
        if (split && FracLeft == 0)
            SplitLeftEdge();
        Add(X1, ZTop[0], Y1, Tex[UpLeft], Light[UpLeft]);
        if (split && !(((Flags & NoSplitUpper) != 0) && Side!.NumSegs > 1))
            SplitUpperEdge();
        Add(X2, ZTop[1], Y2, Tex[UpRight], Light[UpRight]);
        if (split && FracRight == 1)
            SplitRightEdge();
        Add(X2, ZBottom[1], Y2, Tex[LoRight], Light[LoRight]);
        if (split && (Flags & NoSplitLower) == 0 && Side!.NumSegs > 1)
            SplitLowerEdge();
        return Vertices.Count - before;
    }

    public void SplitUpperEdge()
    {
        float polyw = FracRight - FracLeft;
        float facu = (Tex[UpRight].U - Tex[UpLeft].U) / polyw;
        float facv = (Tex[UpRight].V - Tex[UpLeft].V) / polyw;
        float fact = (ZTop[1] - ZTop[0]) / polyw;
        float facc = (ZCeil[1] - ZCeil[0]) / polyw;
        float facf = (ZFloor[1] - ZFloor[0]) / polyw;
        float faclu = (Light[UpRight].U - Light[UpLeft].U) / polyw;
        float faclv = (Light[UpRight].V - Light[UpLeft].V) / polyw;
        _ = facc + facf;
        for (int i = 0; i < Side!.NumSegs - 1; i++)
        {
            VmWallSeg1284 seg = Side.Segs[i];
            if (seg.SideFrac <= FracLeft)
                continue;
            if (seg.SideFrac >= FracRight)
                return;
            float frac = seg.SideFrac - FracLeft;
            Add(seg.X2, ZTop[0] + fact * frac, seg.Y2, Tex[UpLeft].U + facu * frac, Tex[UpLeft].V + facv * frac, Light[UpLeft].U + faclu * frac, Light[UpLeft].V + faclv * frac);
        }
    }

    public void SplitLowerEdge()
    {
        float polyw = FracRight - FracLeft;
        float facu = (Tex[LoRight].U - Tex[LoLeft].U) / polyw;
        float facv = (Tex[LoRight].V - Tex[LoLeft].V) / polyw;
        float facb = (ZBottom[1] - ZBottom[0]) / polyw;
        float facc = (ZCeil[1] - ZCeil[0]) / polyw;
        float facf = (ZFloor[1] - ZFloor[0]) / polyw;
        float faclu = (Light[LoRight].U - Light[LoLeft].U) / polyw;
        float faclv = (Light[LoRight].V - Light[LoLeft].V) / polyw;
        _ = facc + facf;
        for (int i = Side!.NumSegs - 2; i >= 0; i--)
        {
            VmWallSeg1284 seg = Side.Segs[i];
            if (seg.SideFrac >= FracRight)
                continue;
            if (seg.SideFrac <= FracLeft)
                return;
            float frac = seg.SideFrac - FracLeft;
            Add(seg.X2, ZBottom[0] + facb * frac, seg.Y2, Tex[LoLeft].U + facu * frac, Tex[LoLeft].V + facv * frac, Light[LoLeft].U + faclu * frac, Light[LoLeft].V + faclv * frac);
        }
    }

    public void SplitLeftEdge()
    {
        if (LeftHeights == null)
            return;
        if (LeftHeights.NumHeights != 0)
        {
            int i = 0;
            float polyh = ZTop[0] - ZBottom[0];
            float factv = polyh != 0 ? (Tex[UpLeft].V - Tex[LoLeft].V) / polyh : 0;
            float factu = polyh != 0 ? (Tex[UpLeft].U - Tex[LoLeft].U) / polyh : 0;
            float factlv = polyh != 0 ? (Light[UpLeft].V - Light[LoLeft].V) / polyh : 0;
            float factlu = polyh != 0 ? (Light[UpLeft].U - Light[LoLeft].U) / polyh : 0;
            while (i < LeftHeights.NumHeights && LeftHeights.Heights[i] <= ZBottom[0])
                i++;
            while (i < LeftHeights.NumHeights && LeftHeights.Heights[i] < ZTop[0])
            {
                float height = LeftHeights.Heights[i];
                Add(X1, height, Y1, factu * (height - ZTop[0]) + Tex[UpLeft].U, factv * (height - ZTop[0]) + Tex[UpLeft].V, factlu * (height - ZTop[0]) + Light[UpLeft].U, factlv * (height - ZTop[0]) + Light[UpLeft].V);
                i++;
            }
        }
    }

    public void SplitRightEdge()
    {
        if (RightHeights == null)
            return;
        if (RightHeights.NumHeights != 0)
        {
            int i = RightHeights.NumHeights - 1;
            float polyh = ZTop[1] - ZBottom[1];
            float factv = polyh != 0 ? (Tex[UpRight].V - Tex[LoRight].V) / polyh : 0;
            float factu = polyh != 0 ? (Tex[UpRight].U - Tex[LoRight].U) / polyh : 0;
            float factlv = polyh != 0 ? (Light[UpRight].V - Light[LoRight].V) / polyh : 0;
            float factlu = polyh != 0 ? (Light[UpRight].U - Light[LoRight].U) / polyh : 0;
            while (i > 0 && RightHeights.Heights[i] >= ZTop[1])
                i--;
            while (i > 0 && RightHeights.Heights[i] > ZBottom[1])
            {
                float height = RightHeights.Heights[i];
                Add(X2, height, Y2, factu * (height - ZTop[1]) + Tex[UpRight].U, factv * (height - ZTop[1]) + Tex[UpRight].V, factlu * (height - ZTop[1]) + Light[UpRight].U, factlv * (height - ZTop[1]) + Light[UpRight].V);
                i--;
            }
        }
    }

    void CountLeftEdge(ref uint ptr)
    {
        if (LeftHeights == null)
            return;
        if (LeftHeights.NumHeights != 0)
        {
            int i = 0;
            while (i < LeftHeights.NumHeights && LeftHeights.Heights[i] <= ZBottom[0])
                i++;
            while (i < LeftHeights.NumHeights && LeftHeights.Heights[i] < ZTop[0])
            {
                ptr++;
                i++;
            }
        }
    }

    void CountRightEdge(ref uint ptr)
    {
        if (RightHeights == null)
            return;
        if (RightHeights.NumHeights != 0)
        {
            int i = RightHeights.NumHeights - 1;
            while (i > 0 && RightHeights.Heights[i] >= ZTop[1])
                i--;
            while (i > 0 && RightHeights.Heights[i] > ZBottom[1])
            {
                ptr++;
                i--;
            }
        }
    }

    void Add(float x, float z, float y, VmWallUv1284 tex, VmWallUv1284 light)
    {
        Add(x, z, y, tex.U, tex.V, light.U, light.V);
    }

    void Add(float x, float z, float y, float u, float v, float lu, float lv)
    {
        Vertices.Add(new VmWallVertex1284
        {
            X = x,
            Z = z,
            Y = y,
            U = u,
            V = v,
            Lu = lu,
            Lv = lv,
            LIndex = LIndex,
        });
    }
}
