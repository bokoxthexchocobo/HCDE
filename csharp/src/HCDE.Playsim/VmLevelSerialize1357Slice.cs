namespace HCDE.Playsim;

/// <summary>
/// Drawn-subsector pack, line and sector portals, one polyobject, reverb id,
/// and level music serialize from <c>p_saveg.cpp</c> lines 336-592.
/// Chunk 1357. <c>SerializePlayers</c> starts at 593.
/// The int serializer is the archive. <c>S_FindEnvironment</c>,
/// <c>S_SerializeSounds</c>, <c>S_GetMusic</c>, <c>S_ChangeMusic</c>,
/// <c>SetMusic</c>, <c>SetMusicVolume</c>, <c>RotatePolyobj</c>, and
/// <c>MovePolyobj</c> stay injected. <c>isPrimaryLevel</c> is the level flag.
/// </summary>
public static class VmLevelSerialize1357Slice
{
    public const int Mapped = 0x00000100;
    public const int Drawn = 2;

    public sealed class Line { public int Flags; }
    public sealed class Seg { public Line? Linedef; }

    public sealed class Subsector
    {
        public int Flags;
        public int NumLines;
        public Seg[]? Lines;
    }

    public delegate int MusicReader(ref string? name);

    public sealed class Level
    {
        public List<Subsector> Subsectors = new();
        public int VertexCount;
        public bool Primary = true;
        public float MusicVolume;
        public Action<Arc>? SerializeSounds;
        public MusicReader? GetMusic;
        public Func<string?, int, bool>? ChangeMusic;
        public Action? SetMusic;
        public Action<float>? SetMusicVolume;
        public bool IsPrimaryLevel() => Primary;
    }

    public sealed class LinePortal
    {
        public object? Origin;
        public object? Destination;
        public double X;
        public double Y;
        public byte Type;
        public byte Flags;
        public byte DefFlags;
        public byte Align;
    }

    public sealed class SectorPortal
    {
        public int Type;
        public int Flags;
        public uint Partner;
        public int Plane;
        public object? Origin;
        public object? Destination;
        public double X;
        public double Y;
        public double PlaneZ;
        public object? Skybox;
    }

    public sealed class Poly
    {
        public double Angle;
        public double X;
        public double Y;
        public int OriginalCount;
        public object? Interpolation;
        public bool Blocked;
        public byte HasPortals;
        public object? Special;
        public object? Level;
        public Func<double, bool, bool>? Rotate;
        public Func<double, double, bool, bool>? Move;
    }

    public sealed class Reverb { public int Id; }
    public sealed class Zone { public Reverb? Environment; }
    public readonly record struct Slot(string Kind, string? Key, object? Value);

    public sealed class Arc
    {
        public bool Writing;
        public bool ArrayOpens = true;
        public bool ObjectOpens = true;
        public readonly List<Slot> Log = new();
        public readonly Queue<object?> Reads = new();
        public Action<string?>? OnField;
        public Func<int, Reverb?>? FindEnvironment;

        public bool BeginArray(string? key)
        {
            Add("begin-array", key, null);
            return ArrayOpens;
        }

        public void EndArray() => Add("end-array", null, null);

        public bool BeginObject(string? key)
        {
            Add("begin-object", key, null);
            return ObjectOpens;
        }

        public void EndObject() => Add("end-object", null, null);

        public void I32(string? key, ref int value) => Field("i32", key, ref value);
        public void U32(string? key, ref uint value) => Field("u32", key, ref value);
        public void F32(string? key, ref float value) => Field("f32", key, ref value);
        public void F64(string? key, ref double value) => Field("f64", key, ref value);
        public void Bool(string? key, ref bool value) => Field("bool", key, ref value);

        public void U8(string? key, ref byte value)
        {
            Touch(key);
            if (!Writing)
            {
                value = unchecked((byte)(int)Take()!);
            }

            Add("u8", key, value);
        }

        public void Vec(string? key, ref double x, ref double y)
        {
            Touch(key);
            if (!Writing)
            {
                (x, y) = ((double, double))Take()!;
            }

            Add("vec", key, (x, y));
        }

        public void Obj(string? key, ref object? value)
        {
            Touch(key);
            if (!Writing)
            {
                value = Take();
            }

            Add("obj", key, value);
        }

        public void Str(string? key, ref string? value)
        {
            Touch(key);
            if (Writing)
            {
                Add("str", key, CutAtNul(value));
                return;
            }

            value = (string?)Take();
            Add("str", key, value);
        }

        void Field<T>(string kind, string? key, ref T value)
        {
            Touch(key);
            if (!Writing)
            {
                value = (T)Take()!;
            }

            Add(kind, key, value);
        }

        void Touch(string? key) => OnField?.Invoke(key);

        object? Take()
        {
            if (Reads.Count == 0)
            {
                throw new InvalidOperationException("Serialize");
            }

            return Reads.Dequeue();
        }

        void Add(string kind, string? key, object? value) =>
            Log.Add(new Slot(kind, key, value));
    }

    public static void Recalculate(Level level)
    {
        foreach (Subsector sub in level.Subsectors)
        {
            for (int j = 0; j < sub.NumLines; j++)
            {
                Seg[] lines = sub.Lines ?? throw new NullReferenceException("Lines");
                Seg seg = lines[j];
                if (seg.Linedef != null && (seg.Linedef.Flags & Mapped) != 0)
                {
                    sub.Flags |= Drawn;
                }
            }
        }
    }

    public static Arc SerializeSubsectors(Level level, Arc arc, string? key)
    {
        int count = level.Subsectors.Count;
        if (arc.Writing)
        {
            string packed = Pack(level.Subsectors);
            if (arc.BeginArray(key))
            {
                int verts = level.VertexCount;
                int subs = count;
                string? text = packed;
                arc.I32(null, ref verts);
                arc.I32(null, ref subs);
                arc.Str(null, ref text);
                arc.EndArray();
            }
        }
        else if (arc.BeginArray(key))
        {
            int verts = 0;
            int subs = 0;
            string? text = null;
            arc.I32(null, ref verts);
            arc.I32(null, ref subs);
            arc.Str(null, ref text);
            arc.EndArray();
            bool success = verts == level.VertexCount && subs == count;
            if (success)
            {
                text = text ?? throw new NullReferenceException("str");
                success = Apply(level, text, count);
            }

            if (!success)
            {
                Recalculate(level);
            }
        }

        return arc;
    }

    public static Arc SerializeLine(Arc arc, string? key, LinePortal port, LinePortal? def)
    {
        _ = def;
        if (!arc.BeginObject(key))
        {
            return arc;
        }

        arc.Obj("origin", ref port.Origin);
        arc.Obj("destination", ref port.Destination);
        arc.Vec("displacement", ref port.X, ref port.Y);
        arc.U8("type", ref port.Type);
        arc.U8("flags", ref port.Flags);
        arc.U8("defflags", ref port.DefFlags);
        arc.U8("align", ref port.Align);
        arc.EndObject();
        return arc;
    }

    public static Arc SerializeSector(Arc arc, string? key, SectorPortal port, SectorPortal? def)
    {
        _ = def;
        if (!arc.BeginObject(key))
        {
            return arc;
        }

        arc.I32("type", ref port.Type);
        arc.I32("flags", ref port.Flags);
        arc.U32("partner", ref port.Partner);
        arc.I32("plane", ref port.Plane);
        arc.Obj("origin", ref port.Origin);
        arc.Obj("destination", ref port.Destination);
        arc.Vec("displacement", ref port.X, ref port.Y);
        arc.F64("planez", ref port.PlaneZ);
        arc.Obj("skybox", ref port.Skybox);
        arc.EndObject();
        return arc;
    }

    public static Arc SerializePoly(Arc arc, string? key, Poly poly, Poly? def)
    {
        _ = def;
        if (!arc.BeginObject(key))
        {
            return arc;
        }

        double angle = poly.Angle;
        double dx = poly.X;
        double dy = poly.Y;
        arc.F64("angle", ref angle);
        arc.Vec("pos", ref dx, ref dy);
        arc.Obj("interpolation", ref poly.Interpolation);
        arc.Bool("blocked", ref poly.Blocked);
        arc.U8("hasportals", ref poly.HasPortals);
        arc.Obj("specialdata", ref poly.Special);
        arc.Obj("level", ref poly.Level);
        arc.EndObject();
        if (arc.Writing || poly.OriginalCount <= 0)
        {
            return arc;
        }

        Need(poly.Rotate, "RotatePolyobj")(angle, true);
        dx -= poly.X;
        dy -= poly.Y;
        Need(poly.Move, "MovePolyobj")(dx, dy, true);
        return arc;
    }

    public static Arc SerializeReverb(Arc arc, string? key, ref Reverb? c, Reverb? def)
    {
        _ = def;
        int id = arc.Writing && c != null ? c.Id : 0;
        arc.I32(key, ref id);
        if (!arc.Writing)
        {
            c = Need(arc.FindEnvironment, "S_FindEnvironment")(id);
        }

        return arc;
    }

    public static Arc SerializeZone(Arc arc, string? key, Zone zone, Zone? def)
    {
        _ = def;
        Reverb? env = zone.Environment;
        Arc done = SerializeReverb(arc, key, ref env, null);
        zone.Environment = env;
        return done;
    }

    public static void SerializeSounds(Level level, Arc arc)
    {
        if (!level.IsPrimaryLevel())
        {
            return;
        }

        Need(level.SerializeSounds, "S_SerializeSounds")(arc);
        string? name = null;
        byte order = 0;
        float musvol = level.MusicVolume;
        if (arc.Writing)
        {
            order = unchecked((byte)Need(level.GetMusic, "S_GetMusic")(ref name));
        }

        arc.Str("musicname", ref name);
        arc.U8("musicorder", ref order);
        arc.F32("musicvolume", ref musvol);
        if (arc.Writing)
        {
            return;
        }

        if (!Need(level.ChangeMusic, "S_ChangeMusic")(name, order))
        {
            Need(level.SetMusic, "SetMusic")();
        }

        Need(level.SetMusicVolume, "SetMusicVolume")(musvol);
    }

    static string Pack(IReadOnlyList<Subsector> subs)
    {
        int n = subs.Count;
        var chars = new char[(n + 5) / 6];
        int p = 0;
        for (int i = 0; i < n; i += 6)
        {
            int by = 0;
            for (int j = 0; j < 6; j++)
            {
                if (i + j < n && (subs[i + j].Flags & Drawn) != 0)
                {
                    by |= 1 << j;
                }
            }

            chars[p++] = Glyph(by);
        }

        return new string(chars);
    }

    static char Glyph(int by)
    {
        if (by < 10) return (char)('0' + by);
        if (by < 36) return (char)('A' - 10 + by);
        if (by < 62) return (char)('a' - 36 + by);
        if (by == 62) return '-';
        return '+';
    }

    static bool Decode(char c, out int by)
    {
        if (c >= '0' && c <= '9') { by = c - '0'; return true; }
        if (c >= 'A' && c <= 'Z') { by = c - 'A' + 10; return true; }
        if (c >= 'a' && c <= 'z') { by = c - 'a' + 36; return true; }
        if (c == '-') { by = 62; return true; }
        if (c == '+') { by = 63; return true; }
        by = 0;
        return false;
    }

    static bool Apply(Level level, string text, int count)
    {
        int sub = 0;
        for (int i = 0; i < text.Length && text[i] != '\0'; i++)
        {
            if (!Decode(text[i], out int by))
            {
                return false;
            }

            for (int s = 0; s < 6; s++)
            {
                if (sub + s < count && (by & (1 << s)) != 0)
                {
                    level.Subsectors[sub + s].Flags |= Drawn;
                }
            }

            sub += 6;
        }

        return true;
    }

    static string? CutAtNul(string? value)
    {
        if (value == null)
        {
            return null;
        }

        int n = value.IndexOf('\0');
        return n < 0 ? value : value[..n];
    }

    static T Need<T>(T? hook, string name) where T : class =>
        hook ?? throw new InvalidOperationException(name);
}
