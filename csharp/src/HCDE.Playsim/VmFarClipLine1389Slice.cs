namespace HCDE.Playsim;

/// <summary>
/// Far-clip construct, facing and portal rejects, and the ceiling and floor
/// plane marks from <c>r_farclip_line.cpp</c> 1-188. Chunk 1389.
/// Wall init, portal clipping, projection, and range lookup stay callees.
/// The ceiling projection call in this file is a comment.
/// </summary>
public static class VmFarClipLine1389Slice
{
    public const int XFlip = 0x0001;
    public enum Kind { Normal, FakeFloor, FakeCeiling, FakeBack }
    public sealed class AssertAbort(string message) : Exception(message);

    public sealed class Vec
    {
        public double X, Y;
        public static Vec Sub(Vec a, Vec b) => new() { X = a.X - b.X, Y = a.Y - b.Y };
    }

    public sealed class Mark
    {
        public short[] Top = Array.Empty<short>();
        public short[] Bottom = Array.Empty<short>();
    }

    public sealed class LevelPlane { public double V1, V2; }

    public sealed class Seg
    {
        public Vec V1 = new(), V2 = new();
        public bool HasDef = true;
    }

    public sealed class Sector
    {
        public LevelPlane Ceiling = new(), Floor = new();
    }

    public sealed class Sub { public Sector Sector = new(); }

    public sealed class Host
    {
        public Vec View = new();
        public int Width = 4, WindowLeft, WindowRight = 320, ViewHeight = 200, Mirror;
        public short Sx1, Sx2 = 2;
        public bool InitRejects, InSkybox, HasPortal, PortalClips, Asserts = true, Fake;
        public Kind Opaque;
        public short[] Ceiling = Array.Empty<short>(), Floor = Array.Empty<short>();
        public short[] FakeCeiling = Array.Empty<short>(), FakeFloor = Array.Empty<short>();
        public int Inits, Projects;
        public bool Flip, Clipped;
        public Func<Mark, int, int, Mark>? Range;
        public Action<short[]>? FillBottom;
    }

    public sealed class Far
    {
        public readonly Host Host;
        public Sector? Front;
        public Seg? Line;
        public Mark? Ceiling, Floor;
        public bool Prepped;
        public double Ceil1, Floor1, Ceil2, Floor2;
        public short[] Top = Array.Empty<short>(), Bottom = Array.Empty<short>();
        public Far(Host host) => Host = host;

        public void Render(Seg line, Sub? sub, Mark? floor, Mark? ceiling)
        {
            if (sub == null) throw new NullReferenceException();
            Front = sub.Sector;
            Line = line;
            Floor = floor;
            Ceiling = ceiling;
            var p1 = Vec.Sub(line.V1, Host.View);
            var p2 = Vec.Sub(line.V2, Host.View);
            if (p1.Y * (p1.X - p2.X) + p1.X * (p2.Y - p1.Y) >= 0) return;
            Host.Inits++;
            if (Host.InitRejects) return;
            if (Host.Sx1 >= Host.WindowRight || Host.Sx2 <= Host.WindowLeft) return;
            if (!line.HasDef) return;
            if (!Host.InSkybox && Host.HasPortal && Host.PortalClips) return;
            Ceil1 = Front.Ceiling.V1;
            Floor1 = Front.Floor.V1;
            Ceil2 = Front.Ceiling.V2;
            Floor2 = Front.Floor.V2;
            Prepped = false;
            Host.Clipped = true;
        }

        public bool Segment(int x1, int x2)
        {
            if (!Prepped)
            {
                Prepped = true;
                Host.Projects++;
                Host.Flip = (Host.Mirror & XFlip) != 0;
                if (Top.Length != Host.Width)
                {
                    Top = new short[Host.Width];
                    Bottom = new short[Host.Width];
                }
                Host.FillBottom?.Invoke(Bottom);
                Array.Copy(Bottom, Top, Host.Width);
            }
            for (int x = x1; x < x2; x++)
            {
                if (Top[x] < Host.Ceiling[x]) Top[x] = Host.Ceiling[x];
                if (Bottom[x] > Host.Floor[x]) Bottom[x] = Host.Floor[x];
            }
            MarkCeiling(x1, x2);
            MarkFloor(x1, x2);
            return true;
        }

        void MarkCeiling(int x1, int x2)
        {
            if (Ceiling == null) return;
            if (Host.Range != null) Ceiling = Host.Range(Ceiling, x1, x2);
            for (int x = x1; x < x2; x++)
            {
                short top = Host.Fake && Host.Opaque == Kind.FakeCeiling ? Host.FakeCeiling[x] : Host.Ceiling[x];
                short bottom = Top[x] < Host.Floor[x] ? Top[x] : Host.Floor[x];
                if (top < bottom)
                {
                    Ceiling.Top[x] = top;
                    Ceiling.Bottom[x] = bottom;
                }
            }
        }

        void MarkFloor(int x1, int x2)
        {
            if (Floor == null) return;
            if (Host.Range != null) Floor = Host.Range(Floor, x1, x2);
            for (int x = x1; x < x2; x++)
            {
                short top = Bottom[x] > Host.Ceiling[x] ? Bottom[x] : Host.Ceiling[x];
                short bottom = Host.Fake && Host.Opaque == Kind.FakeFloor ? Host.FakeFloor[x] : Host.Floor[x];
                if (top >= bottom) continue;
                if (Host.Asserts && bottom > Host.ViewHeight) throw new AssertAbort("bottom <= viewheight");
                Floor.Top[x] = top;
                Floor.Bottom[x] = bottom;
            }
        }
    }
}
