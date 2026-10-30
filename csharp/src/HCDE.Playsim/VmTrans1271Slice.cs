namespace HCDE.Playsim;

/// <summary>
/// Portal-clipped sprite columns and the masked 3D-floor pass from
/// <c>r_translucent_pass.cpp</c> (1-229). Chunk 1271. A column is clipped only
/// from the back side of a same-portal line, and the side test is strict past
/// the epsilon. Ceilings walk down while the height is at or above the view.
/// Floors walk up only while the height is below the view, and the plane is
/// drawn before the sprites on that pass. The drawn flag is cleared only when
/// the pass asks for a renew.
/// </summary>
public sealed class VmHeight1271
{
    public double Height;
    public VmHeight1271? Prev;
    public VmHeight1271? Next;
}

public sealed class VmLine1271
{
    public uint PortalIndex = uint.MaxValue;
    public int PortalFlags;
    public int Special;
    public int Front = 1;
    public double X;
    public double Y;
    public double Dx;
    public double Dy;

    public bool IsVisual => PortalIndex != uint.MaxValue && (PortalFlags & 1) != 0;
}

public sealed class VmPortalSeg1271
{
    public int Id;
    public bool HasCurline = true;
    public VmLine1271? Line;
    public int Side = 1;
    public int PortalUniq;
    public int X1;
    public int X2;
    public int Flags;
}

public sealed class VmMaskedSprite1271
{
    public int Id;
    public int PortalUniq;
    public double X;
    public double Y;
}

public sealed class VmClip3D1271
{
    public bool ClipBottom;
    public bool ClipTop;
    public bool Down2Up;
    public double SclipBottom;
    public double SclipTop;
}

public sealed class VmTrans1271
{
    public const int LineMirror = 182;
    public bool ThreadPresent = true;
    public bool MainThread;
    public bool Active;
    public int ClockSample;
    public long Counter;
    public double ViewZ;
    public bool InSkybox;
    public int PortalUniq;
    public VmHeight1271? HeightTop;
    public VmHeight1271? HeightCur;
    public bool Deleted;
    public readonly List<VmPortalSeg1271?> Segs = new();
    public readonly List<VmPortalSeg1271> PortalSegs = new();
    public readonly List<VmMaskedSprite1271?> Sprites = new();
    public readonly List<string> Log = new();

    public VmTrans1271(bool threadPresent = true)
    {
        ThreadPresent = threadPresent;
    }

    public void Deinit()
    {
        Log.Add("voxel");
    }

    public void Clear()
    {
        if (!ThreadPresent)
            throw new NullReferenceException();
        Sprites.Clear();
    }

    public void Collect()
    {
        if (!ThreadPresent)
            throw new NullReferenceException();
        PortalSegs.Clear();
        foreach (var seg in Segs)
        {
            if (seg == null)
                throw new NullReferenceException();
            if (!seg.HasCurline)
                continue;
            var line = seg.Line;
            if (line == null)
                continue;
            if (!line.IsVisual && line.Special != LineMirror)
                continue;
            if (seg.Side != line.Front)
                continue;
            PortalSegs.Add(seg);
        }
    }

    public bool ClipColumn(int x, double worldX, double worldY)
    {
        if (!ThreadPresent)
            throw new NullReferenceException();
        if (InSkybox)
            return false;
        foreach (var seg in PortalSegs)
        {
            if (seg.PortalUniq != PortalUniq)
                continue;
            var line = seg.Line;
            if (line == null)
                throw new NullReferenceException();
            double cross = (worldY - line.Y) * line.Dx + (line.X - worldX) * line.Dy;
            if (!(cross > (double)(1f / 65536f)))
                continue;
            if (x >= seg.X1 && x < seg.X2)
                return true;
        }

        return false;
    }

    public void Render()
    {
        if (!ThreadPresent)
            throw new NullReferenceException();
        if (MainThread)
            Clock();
        Collect();
        Log.Add("sort");
        Log.Add("groups");
        if (HeightTop == null)
        {
            DrawMasked(false, new VmClip3D1271());
        }
        else
        {
            for (var hl = HeightCur; hl != null && hl.Height >= ViewZ; hl = hl.Prev)
            {
                var clip = new VmClip3D1271 { ClipBottom = true, SclipBottom = hl.Height };
                if (hl.Next != null)
                {
                    clip.ClipTop = true;
                    clip.SclipTop = hl.Next.Height;
                }

                DrawMasked(true, clip);
                Log.Add(Plane(hl.Height));
            }

            DrawMasked(true, new VmClip3D1271
            {
                ClipTop = true,
                SclipTop = HeightTop.Height,
                Down2Up = true,
            });
            for (var hl = HeightTop; hl != null && hl.Height < ViewZ; hl = hl.Next)
            {
                Log.Add(Plane(hl.Height));
                var clip = new VmClip3D1271
                {
                    ClipBottom = true,
                    SclipBottom = hl.Height,
                    Down2Up = true,
                };
                if (hl.Next != null)
                {
                    clip.ClipTop = true;
                    clip.SclipTop = hl.Next.Height;
                }

                DrawMasked(true, clip);
            }

            Deleted = true;
            Log.Add("delete");
        }

        if (MainThread)
            Unclock();
    }

    void DrawMasked(bool renew, VmClip3D1271 clip)
    {
        int count = Sprites.Count;
        for (int i = count; i > 0; i--)
        {
            var sprite = Sprites[i - 1];
            if (sprite == null)
                throw new NullReferenceException();
            if (sprite.PortalUniq == PortalUniq)
                Log.Add("sprite " + sprite.Id + " " + ClipText(clip));
        }

        foreach (var ds in Segs)
        {
            if (ds == null)
                throw new NullReferenceException();
            if (ds.PortalUniq != PortalUniq)
                continue;
            if ((ds.Flags & 4) != 0 || (ds.Flags & 3) != 0 || (ds.Flags & 8) != 0)
            {
                Log.Add("seg " + ds.Id);
                if (renew)
                    Log.Add("undrawn " + ds.Id + " " + ds.X1 + " " + ds.X2);
            }
        }
    }

    void Clock()
    {
        if (!Active)
        {
            Log.Add("clock idle");
            return;
        }

        Counter -= ClockSample;
        Log.Add("clock");
    }

    void Unclock()
    {
        if (!Active)
        {
            Log.Add("unclock idle");
            return;
        }

        Counter += ClockSample;
        Log.Add("unclock");
    }

    static string Plane(double height) =>
        "plane " + height.ToString(System.Globalization.CultureInfo.InvariantCulture);

    static string ClipText(VmClip3D1271 clip)
    {
        var culture = System.Globalization.CultureInfo.InvariantCulture;
        return (clip.ClipTop ? "1" : "0") + " " + (clip.ClipBottom ? "1" : "0") + " " +
            (clip.Down2Up ? "1" : "0") + " " + clip.SclipTop.ToString(culture) + " " +
            clip.SclipBottom.ToString(culture);
    }
}
