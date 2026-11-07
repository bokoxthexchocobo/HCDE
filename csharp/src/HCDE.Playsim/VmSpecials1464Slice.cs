namespace HCDE.Playsim;

/// <summary>
/// SpawnFriction, Set3DFloor, and Spawn3DFloors from specials.cpp 860-1036.
/// Banners between those functions are stepped over. No credited or recognition lines sit in the span.
/// </summary>
public static class VmSpecials1464Slice
{
    public const int BandStart = 860;
    public const int BandEnd = 1036;
    public const int SetFriction = 219;
    public const int LightOnly = 50;
    public const int Set3D = 160;
    public const int SetContents = 161;
    public const int MapDoom = 1;
    public const int MapHexen = 2;
    public const int Exists = 0x1;
    public const int Solid = 0x2;
    public const int RenderPlanes = 0x8;
    public const int RenderAll = 0xC;
    public const int Swimmable = 0x10;
    public const int NoShade = 0x20;
    public const int BothPlanes = 0x200;
    public const int Translucent = 0x800;
    public const int FogFlag = 0x1000;
    public const int InvertPlanes = 0x2000;
    public const int AllSides = 0x4000;
    public const int DoubleShadow = 0x10000;
    public const int UpperTexture = 0x20000;
    public const int LowerTexture = 0x40000;
    public const int ThinFloor = 0x80000;
    public const int NoDamage = 0x100000;
    public const int Fix = 0x200000;
    public const int InvertSector = 0x400000;
    public const int SeeThrough = 0x2000000;
    public const int ShootThrough = 0x4000000;
    public const int FadeWalls = 0x8000000;
    public const int Additive = 0x10000000;
    public const int Flood = 0x20000000;
    public const int Reset = unchecked((int)0x80000000);
    public const int ColorMask = 0x00FFFFFF;

    static readonly int[] DefFlags =
    {
        0,
        Solid,
        Swimmable | BothPlanes | AllSides | ShootThrough | SeeThrough,
        ShootThrough | SeeThrough,
    };

    static readonly int[] FlagVals = { 512 + 2048, 2 + 512 + 2048, 512 + 1024 + 2048 };

    static readonly uint[] Vavoom =
    {
        0, 0x80825032, 0x96FF5000, 0x9632FF32, 0x96001905,
        0x96FF5000, 0x96A00A0A, 0x9680A080, 0x8080A080, 0x80004FA5,
    };

    public sealed class Sector
    {
        public int Tag;
        public int Fade = -1;
        public int Fog = -1;
        public List<Line> Lines = new();
    }

    public sealed class Line
    {
        public int Index;
        public int Special;
        public double Delta;
        public int[] Args = new int[5];
        public Sector Front = null!;
        public bool TopExists = true;
        public bool TopValid = true;
        public int TopIndex;
        public bool TopCleared;
    }

    public sealed class Floor
    {
        public Sector Dest = null!;
        public Sector Model = null!;
        public Line Source = null!;
        public int Flags;
        public int Alpha;
    }

    public sealed class Drag
    {
        public int Tag;
        public int Length;
        public bool Affect = false;
    }

    public sealed class World
    {
        public int MapType = MapDoom;
        public List<Sector> Sectors = new();
        public List<Line> Lines = new();
        public List<Floor> Floors = new();
        public List<Drag> Drags = new();
        public List<(int Line, int Id)> LineIds = new();
        public List<Sector> Recalculated = new();
    }

    public static void SpawnFriction(World w)
    {
        foreach (var line in w.Lines)
        {
            if (line.Special != SetFriction)
                continue;
            int length = line.Args[1] != 0 ? (line.Args[1] <= 200 ? line.Args[1] : 200) : (int)line.Delta;
            w.Drags.Add(new Drag { Tag = line.Args[0], Length = length });
            line.Special = 0;
        }
    }

    public static int Set3DFloor(World w, Line line, int param, int param2, int alpha)
    {
        var sec = line.Front;
        foreach (int s in Tagged(w, line.Args[0]))
        {
            int flags;
            int a;
            if (param == 0)
            {
                flags = Exists | RenderAll | Solid | InvertSector;
                a = 255;
                foreach (var l in sec.Lines)
                {
                    if (l.Special != SetContents || l.Front != sec)
                        continue;
                    a = Math.Clamp(l.Args[1], 0, 100);
                    if ((l.Args[2] & 1) != 0) flags &= ~Solid;
                    if ((l.Args[2] & 2) != 0) flags |= SeeThrough;
                    if ((l.Args[2] & 4) != 0) flags |= ShootThrough;
                    if ((l.Args[2] & 8) != 0) flags |= Additive;
                    if (a != 100) flags |= Translucent;
                    if (l.Args[0] != 0)
                    {
                        flags |= Swimmable | BothPlanes | AllSides | Flood;
                        l.Front.Fade = (int)(Vavoom[l.Args[0]] & ColorMask);
                        l.Front.Fog = 0;
                    }
                    a = (a * 255) / 100;
                    break;
                }
            }
            else if (param == 4)
            {
                flags = Exists | RenderPlanes | InvertPlanes | NoShade | Fix;
                if ((param2 & 1) != 0) flags |= SeeThrough;
                a = 255;
            }
            else
            {
                flags = DefFlags[param & 3] | Exists | RenderAll;
                if ((param & 4) != 0) flags |= AllSides | BothPlanes;
                if ((param & 16) != 0) flags ^= SeeThrough;
                if ((param & 32) != 0) flags ^= ShootThrough;
                if ((param2 & 1) != 0) flags |= NoShade;
                if ((param2 & 2) != 0) flags |= DoubleShadow;
                if ((param2 & 4) != 0) flags |= FogFlag;
                if ((param2 & 8) != 0) flags |= ThinFloor;
                if ((param2 & 16) != 0) flags |= UpperTexture;
                if ((param2 & 32) != 0) flags |= LowerTexture;
                if ((param2 & 64) != 0) flags |= Additive | Translucent;
                if ((param2 & 128) != 0 && (flags & Solid) == 0)
                    flags |= Flood | SeeThrough | ShootThrough;
                if ((param2 & 512) != 0) flags |= FadeWalls;
                if ((param2 & 1024) != 0) flags |= Reset;
                if ((param2 & 2048) != 0) flags |= NoDamage;
                a = alpha;
                if (!line.TopExists && a < 255)
                    a = -line.TopIndex;
                a = Math.Clamp(a, 0, 255);
                if (a == 0) flags &= ~(RenderAll | BothPlanes | AllSides);
                else if (a != 255) flags |= Translucent;
            }
            w.Floors.Add(new Floor { Dest = w.Sectors[s], Model = sec, Source = line, Flags = flags, Alpha = a });
        }
        if (!line.TopValid)
            line.TopCleared = true;
        return 1;
    }

    public static void Spawn3DFloors(World w)
    {
        foreach (var line in w.Lines)
        {
            switch (line.Special)
            {
                case LightOnly:
                    if (line.Args[1] < 0 || line.Args[1] > 2)
                        line.Args[1] = 0;
                    if (line.Args[0] != 0)
                        Set3DFloor(w, line, 3, FlagVals[line.Args[1]], 0);
                    break;
                case Set3D:
                    if (w.MapType == MapHexen)
                    {
                        if ((line.Args[1] & 8) != 0)
                            w.LineIds.Add((line.Index, line.Args[4]));
                        else
                        {
                            line.Args[0] += 256 * line.Args[4];
                            line.Args[4] = 0;
                        }
                    }
                    Set3DFloor(w, line, line.Args[1] & ~8, line.Args[2], line.Args[3]);
                    break;
                default:
                    continue;
            }
            line.Special = 0;
            for (int i = 0; i < 5; i++)
                line.Args[i] = 0;
        }
        foreach (var sec in w.Sectors)
            w.Recalculated.Add(sec);
    }

    static IEnumerable<int> Tagged(World w, int tag)
    {
        for (int i = 0; i < w.Sectors.Count; i++)
        {
            bool match = tag == 0 ? w.Sectors[i].Tag == 0 : w.Sectors[i].Tag == tag;
            if (match)
                yield return i;
        }
    }
}
