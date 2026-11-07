namespace HCDE.Playsim;

/// <summary>
/// MapLoader::SpawnScrollers from specials.cpp 1199-1434. The banner at 1193-1198 stays uncredited.
/// </summary>
public static class VmSpecials1463Slice
{
    public const int BandStart = 1199;
    public const int BandEnd = 1434;
    public const int CopyScroller = 58;
    public const int ScrollLeft = 100;
    public const int ScrollRight = 101;
    public const int ScrollUp = 102;
    public const int ScrollDown = 103;
    public const int ScrollBoth = 221;
    public const int ScrollModel = 222;
    public const int ScrollFloor = 223;
    public const int ScrollCeiling = 224;
    public const int ScrollOffsets = 225;
    public const int ScSide = 0;
    public const int Floor = 1;
    public const int Ceiling = 2;
    public const int Carry = 3;
    public const int PartsAll = 7;

    public sealed class Sector
    {
        public int Tag;
    }

    public sealed class Side
    {
        public Sector Sector = null!;
        public double MidX;
        public double MidY;
    }

    public sealed class Line
    {
        public int Special;
        public int Id;
        public double Dx;
        public double Dy;
        public int[] Args = new int[5];
        public Sector Front = null!;
        public Side Side = null!;
    }

    public sealed class Scroller
    {
        public string Form = "typed";
        public int Type;
        public double X, Y;
        public Sector? Control;
        public Sector? Sector;
        public int SideLine = -1;
        public int WallLine = -1;
        public int Accel;
        public int Parts = PartsAll;
    }

    public sealed class World
    {
        public List<Sector> Sectors = new();
        public List<Line> Lines = new();
        public List<Scroller> Scrollers = new();
        public List<string> Warnings = new();
    }

    static readonly Dictionary<int, (string Name, int MapArgs)> Info = new()
    {
        [CopyScroller] = ("Sector_CopyScroller", 2),
        [ScrollLeft] = ("Scroll_Texture_Left", 2),
        [ScrollRight] = ("Scroll_Texture_Right", 2),
        [ScrollUp] = ("Scroll_Texture_Up", 2),
        [ScrollDown] = ("Scroll_Texture_Down", 2),
        [ScrollBoth] = ("Scroll_Texture_Both", 5),
        [ScrollModel] = ("Scroll_Texture_Model", 2),
        [ScrollFloor] = ("Scroll_Floor", 5),
        [ScrollCeiling] = ("Scroll_Ceiling", 5),
        [ScrollOffsets] = ("Scroll_Texture_Offsets", 4),
    };

    public static void Spawn(World w)
    {
        var copies = new List<int>();
        for (int i = 0; i < w.Lines.Count; i++)
        {
            var line = w.Lines[i];
            if (line.Special != CopyScroller)
                continue;
            if (!HasTag(line.Front, line.Args[0]))
                copies.Add(i);
            line.Special = 0;
        }
        for (int i = 0; i < w.Lines.Count; i++)
            Apply(w, w.Lines[i], i, copies);
    }

    static void Apply(World w, Line line, int index, List<int> copies)
    {
        int special = line.Special;
        Warn(w, line, index, special);
        line.Special = 0;
        double dx = 0, dy = 0;
        Sector? control = null;
        int accel = 0;
        if (special == ScrollCeiling || special == ScrollFloor || special == ScrollModel)
        {
            if ((line.Args[1] & 3) != 0)
            {
                control = line.Side.Sector;
                if ((line.Args[1] & 2) != 0)
                    accel = 1;
            }
            if (special == ScrollModel || (line.Args[1] & 4) != 0)
            {
                dx = line.Dx / 32.0;
                dy = line.Dy / 32.0;
            }
            else
            {
                dx = (line.Args[3] - 128) / 32.0;
                dy = (line.Args[4] - 128) / 32.0;
            }
        }
        switch (special)
        {
            case ScrollCeiling:
                foreach (int s in Tagged(w, line.Args[0]))
                    AddTyped(w, Ceiling, -dx, dy, control, w.Sectors[s], -1, accel, PartsAll);
                foreach (int c in copies)
                {
                    var copy = w.Lines[c];
                    if (copy.Args[0] == line.Args[0] && (copy.Args[1] & 1) != 0)
                        AddTyped(w, Ceiling, -dx, dy, control, copy.Front, -1, accel, PartsAll);
                }
                break;
            case ScrollFloor:
                if (line.Args[2] != 1)
                {
                    foreach (int s in Tagged(w, line.Args[0]))
                        AddTyped(w, Floor, -dx, dy, control, w.Sectors[s], -1, accel, PartsAll);
                    foreach (int c in copies)
                    {
                        var copy = w.Lines[c];
                        if (copy.Args[0] == line.Args[0] && (copy.Args[1] & 2) != 0)
                            AddTyped(w, Floor, -dx, dy, control, copy.Front, -1, accel, PartsAll);
                    }
                }
                if (line.Args[2] > 0)
                {
                    foreach (int s in Tagged(w, line.Args[0]))
                        AddTyped(w, Carry, dx, dy, control, w.Sectors[s], -1, accel, PartsAll);
                    foreach (int c in copies)
                    {
                        var copy = w.Lines[c];
                        if (copy.Args[0] == line.Args[0] && (copy.Args[1] & 4) != 0)
                            AddTyped(w, Carry, dx, dy, control, copy.Front, -1, accel, PartsAll);
                    }
                }
                break;
            case ScrollModel:
                if (line.Args[0] != 0)
                {
                    foreach (int s in LineIds(w, line.Args[0]))
                    {
                        if (s != index)
                            AddWall(w, dx, dy, s, control, accel);
                    }
                }
                else
                    AddWall(w, dx, dy, index, control, accel);
                break;
            case ScrollOffsets:
            {
                int divider = Math.Max(1, line.Args[3]);
                var side = line.Side;
                if ((line.Args[2] & 3) != 0)
                {
                    control = side.Sector;
                    if ((line.Args[2] & 2) != 0)
                        accel = 1;
                }
                double ox = -side.MidX / divider;
                double oy = side.MidY / divider;
                int parts = ScrollParts(line.Args[0]);
                if (line.Args[1] == 0)
                    AddTyped(w, ScSide, ox, oy, control, null, index, accel, parts);
                else
                {
                    foreach (int s in LineIds(w, line.Args[1]))
                        AddTyped(w, ScSide, ox, oy, control, null, s, accel, parts);
                }
                break;
            }
            case ScrollLeft:
                line.Special = special;
                AddTyped(w, ScSide, line.Args[0] / 64.0, 0, null, null, index, accel, ScrollParts(line.Args[1]));
                break;
            case ScrollRight:
                line.Special = special;
                AddTyped(w, ScSide, -line.Args[0] / 64.0, 0, null, null, index, accel, ScrollParts(line.Args[1]));
                break;
            case ScrollUp:
                line.Special = special;
                AddTyped(w, ScSide, 0, line.Args[0] / 64.0, null, null, index, accel, ScrollParts(line.Args[1]));
                break;
            case ScrollDown:
                line.Special = special;
                AddTyped(w, ScSide, 0, -line.Args[0] / 64.0, null, null, index, accel, ScrollParts(line.Args[1]));
                break;
            case ScrollBoth:
                if (line.Args[0] == 0)
                {
                    dx = (line.Args[1] - line.Args[2]) / 64.0;
                    dy = (line.Args[4] - line.Args[3]) / 64.0;
                    AddTyped(w, ScSide, dx, dy, null, null, index, accel, PartsAll);
                }
                break;
            default:
                line.Special = special;
                break;
        }
    }

    static void Warn(World w, Line line, int index, int special)
    {
        if (!Info.TryGetValue(special, out var spec))
            return;
        for (int arg = spec.MapArgs; arg < line.Args.Length; arg++)
        {
            if (line.Args[arg] == 0)
                continue;
            w.Warnings.Add($"Line {index} (type {special}:{spec.Name}), arg {arg + 1} is {line.Args[arg]} (should be 0)\n");
        }
    }

    static int ScrollParts(int i) => i <= 0 || (i & ~7) != 0 ? PartsAll : i;

    static void AddTyped(World w, int type, double x, double y, Sector? control, Sector? sector, int sideLine, int accel, int parts)
    {
        w.Scrollers.Add(new Scroller
        {
            Form = "typed",
            Type = type,
            X = x,
            Y = y,
            Control = control,
            Sector = sector,
            SideLine = sideLine,
            Accel = accel,
            Parts = parts,
        });
    }

    static void AddWall(World w, double x, double y, int wallLine, Sector? control, int accel)
    {
        w.Scrollers.Add(new Scroller
        {
            Form = "wall",
            X = x,
            Y = y,
            WallLine = wallLine,
            Control = control,
            Accel = accel,
            Parts = PartsAll,
        });
    }

    static bool HasTag(Sector sector, int tag) => sector.Tag != 0 && sector.Tag == tag;

    static IEnumerable<int> Tagged(World w, int tag)
    {
        for (int i = 0; i < w.Sectors.Count; i++)
        {
            bool match = tag == 0 ? w.Sectors[i].Tag == 0 : w.Sectors[i].Tag == tag;
            if (match)
                yield return i;
        }
    }

    static IEnumerable<int> LineIds(World w, int id)
    {
        for (int i = 0; i < w.Lines.Count; i++)
        {
            if (w.Lines[i].Id == id)
                yield return i;
        }
    }
}
