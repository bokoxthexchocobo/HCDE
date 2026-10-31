using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Line return, texture manipulation, side parts, linked sectors, colormap,
/// sector planes, and the secplane repair from <c>p_saveg.cpp</c> 84-275.
/// Chunk 1388. <c>sector_t::Serialize</c> starts at 276.
/// </summary>
public static class VmSaveSidePlane1388Slice
{
    public sealed class Ptr
    {
        public string Id = "";
        public Ptr(string id) => Id = id;
        public override string ToString() => Id;
    }

    public sealed class Arc
    {
        public bool Writing = true, Rollback, RootObject = true, OpenObject = true, OpenArray = true;
        public int ReadCount = 3;
        public double? ReadX, ReadY, ReadZ, ReadD;
        public readonly List<bool> Nest = new();
        public readonly List<string> Log = new();
        public bool InObject => Nest.Count == 0 ? RootObject : Nest[^1];
        public bool CanSkip => !Rollback && Writing && InObject;
        public bool Reading => !Writing;
        public string Text() => string.Join("\n", Log);

        public bool BeginObject(string? key)
        {
            if (Writing)
            {
                if (InObject && key == null) throw new InvalidOperationException("missing element name");
                Log.Add(key == null ? "obj" : "obj:" + key);
                Nest.Add(true);
                return true;
            }
            if (!OpenObject) return false;
            Log.Add(key == null ? "obj" : "obj:" + key);
            Nest.Add(true);
            return true;
        }

        public void EndObject()
        {
            if (Writing && !InObject) throw new InvalidOperationException("EndObject call not inside an object");
            Log.Add("end");
            if (Nest.Count > 0) Nest.RemoveAt(Nest.Count - 1);
        }

        public bool BeginArray(string? key)
        {
            if (Writing)
            {
                if (InObject && key == null) throw new InvalidOperationException("missing element name");
                Log.Add(key == null ? "arr" : "arr:" + key);
                Nest.Add(false);
                return true;
            }
            if (!OpenArray) return false;
            Log.Add(key == null ? "arr" : "arr:" + key);
            Nest.Add(false);
            return true;
        }

        public void EndArray()
        {
            if (Writing && InObject) throw new InvalidOperationException("EndArray call not inside an array");
            Log.Add("endarr");
            if (Nest.Count > 0) Nest.RemoveAt(Nest.Count - 1);
        }
    }

    public sealed class Manip
    {
        public uint Add, Mod, Blend;
        public float Desat;
        public bool Same(Manip o) => Add == o.Add && Mod == o.Mod && Blend == o.Blend && Desat.Equals(o.Desat);
    }

    public sealed class Part
    {
        public double X, Y, Xs = 1, Ys = 1;
        public Ptr? Interp;
        public short Flags;
        public sbyte Skew;
        public int Tex;
        public Manip Fx = new();
        public uint C0, C1, Add;
        public bool Same(Part o) => X.Equals(o.X) && Y.Equals(o.Y) && Xs.Equals(o.Xs) && Ys.Equals(o.Ys)
            && ReferenceEquals(Interp, o.Interp) && Flags == o.Flags && Skew == o.Skew && Tex == o.Tex
            && Fx.Same(o.Fx) && C0 == o.C0 && C1 == o.C1 && Add == o.Add;
    }

    public sealed class Side
    {
        public Part[] Tex = { new(), new(), new() };
        public short Light;
        public ushort Flags;
        public Ptr? Decals;
        public bool SameTex(Side o)
        {
            for (int i = 0; i < 3; i++) if (!Tex[i].Same(o.Tex[i])) return false;
            return true;
        }
    }

    public sealed class Link { public Ptr? Sector; public int Type; }

    public sealed class Map
    {
        public uint Light = 0xffffff, Fade;
        public byte Desat, Blend;
        public ushort Fog;
        public bool Eq(Map o) => Light == o.Light && Fade == o.Fade && Desat == o.Desat && Blend == o.Blend && Fog == o.Fog;
    }

    public sealed class Xf { public double X, Y, Xs = 1, Ys = 1, BaseY, Angle, BaseAngle; }

    public sealed class Plane
    {
        public Xf Xf = new();
        public int Flags, Light, Texture, Sky0, Sky1;
        public double Alpha = 1, TexZ;
        public uint Glow;
        public float GlowHeight;
        public Manip Fx = new();
        public bool Same(Plane o) => Xf.X.Equals(o.Xf.X) && Xf.Y.Equals(o.Xf.Y) && Xf.Xs.Equals(o.Xf.Xs)
            && Xf.Ys.Equals(o.Xf.Ys) && Xf.Angle.Equals(o.Xf.Angle) && Xf.BaseY.Equals(o.Xf.BaseY)
            && Xf.BaseAngle.Equals(o.Xf.BaseAngle) && Flags == o.Flags && Light == o.Light && Texture == o.Texture
            && Alpha.Equals(o.Alpha) && TexZ.Equals(o.TexZ) && Glow == o.Glow && GlowHeight.Equals(o.GlowHeight)
            && Fx.Same(o.Fx) && Sky0 == o.Sky0 && Sky1 == o.Sky1;
    }

    public sealed class Sec
    {
        public double X, Y, Z, D, NegiC;
        public bool Dither;
        public bool Same(Sec o) => X.Equals(o.X) && Y.Equals(o.Y) && Z.Equals(o.Z) && D.Equals(o.D) && NegiC.Equals(o.NegiC) && Dither == o.Dither;
    }

    static string D(double v) => v.ToString("R", CultureInfo.InvariantCulture);
    static string G(float v) => v.ToString("R", CultureInfo.InvariantCulture);
    static string H(uint v) => v.ToString("X8", CultureInfo.InvariantCulture);
    static string P(object? o) => o == null ? "null" : o.ToString()!;
    static string I(int v) => v.ToString(CultureInfo.InvariantCulture);
    static void F(Arc a, string key, string value, bool withDef, string def)
        => a.Log.Add(withDef ? "f:" + key + ":" + value + "|" + def : "f:" + key + ":" + value);

    public static Arc CloseLine(Arc arc)
    {
        arc.Log.Add("return");
        return arc;
    }

    public static Arc Texture(Arc arc, string? key, Manip part, Manip? def)
    {
        if (arc.CanSkip && def != null && part.Same(def)) return arc;
        if (!arc.BeginObject(key)) return arc;
        var d = def ?? throw new NullReferenceException();
        F(arc, "addcolor", H(part.Add), true, H(d.Add));
        F(arc, "yoffset", H(part.Mod), true, H(d.Mod));
        F(arc, "xscale", H(part.Blend), true, H(d.Blend));
        F(arc, "yscale", G(part.Desat), true, G(d.Desat));
        arc.EndObject();
        return arc;
    }

    public static Arc WallPart(Arc arc, string? key, Part part, Part? def)
    {
        if (arc.CanSkip && def != null && part.Same(def)) return arc;
        if (!arc.BeginObject(key)) return arc;
        var d = def ?? throw new NullReferenceException();
        F(arc, "xoffset", D(part.X), true, D(d.X));
        F(arc, "yoffset", D(part.Y), true, D(d.Y));
        F(arc, "xscale", D(part.Xs), true, D(d.Xs));
        F(arc, "yscale", D(part.Ys), true, D(d.Ys));
        F(arc, "texture", I(part.Tex), true, I(d.Tex));
        F(arc, "interpolation", P(part.Interp), false, "");
        F(arc, "flags", I(part.Flags), true, I(d.Flags));
        F(arc, "skew", I(part.Skew), true, I(d.Skew));
        F(arc, "color1", H(part.C0), true, H(d.C0));
        F(arc, "color2", H(part.C1), true, H(d.C1));
        F(arc, "addcolor", H(part.Add), true, H(d.Add));
        Texture(arc, "texturefx", part.Fx, d.Fx);
        arc.EndObject();
        return arc;
    }

    static void Textures(Arc arc, Side side, Side def)
    {
        if (!arc.Rollback && arc.Writing && side.SameTex(def)) return;
        if (!arc.BeginArray("textures")) return;
        int n = 3;
        if (arc.Reading && arc.ReadCount < n) n = arc.ReadCount;
        for (int i = 0; i < n; i++) WallPart(arc, null, side.Tex[i], def.Tex[i]);
        arc.EndArray();
    }

    public static Arc Wall(Arc arc, string? key, Side side, Side? def)
    {
        if (!arc.BeginObject(key)) return arc;
        var d = def ?? throw new NullReferenceException();
        Textures(arc, side, d);
        F(arc, "light", I(side.Light), true, I(d.Light));
        F(arc, "flags", I(side.Flags), true, I(d.Flags));
        F(arc, "attacheddecals", P(side.Decals), false, "");
        arc.EndObject();
        return arc;
    }

    public static Arc Linked(Arc arc, string? key, Link ls, Link? def)
    {
        _ = def;
        if (!arc.BeginObject(key)) return arc;
        F(arc, "sector", P(ls.Sector), false, "");
        F(arc, "type", I(ls.Type), false, "");
        arc.EndObject();
        return arc;
    }

    public static Arc Colormap(Arc arc, string? key, Map port, Map? def)
    {
        if (arc.CanSkip && def != null && port.Eq(def)) return arc;
        if (!arc.BeginObject(key)) return arc;
        F(arc, "lightcolor", H(port.Light), false, "");
        F(arc, "fadecolor", H(port.Fade), false, "");
        F(arc, "desaturation", I(port.Desat), false, "");
        F(arc, "fogdensity", I(port.Fog), false, "");
        arc.EndObject();
        return arc;
    }

    public static Arc SectorPlane(Arc arc, string? key, Plane p, Plane? def)
    {
        if (arc.CanSkip && def != null && p.Same(def)) return arc;
        if (!arc.BeginObject(key)) return arc;
        var d = def ?? throw new NullReferenceException();
        var x = p.Xf; var y = d.Xf;
        F(arc, "xoffs", D(x.X), true, D(y.X));
        F(arc, "yoffs", D(x.Y), true, D(y.Y));
        F(arc, "xscale", D(x.Xs), true, D(y.Xs));
        F(arc, "yscale", D(x.Ys), true, D(y.Ys));
        F(arc, "angle", D(x.Angle), true, D(y.Angle));
        F(arc, "baseyoffs", D(x.BaseY), true, D(y.BaseY));
        F(arc, "baseangle", D(x.BaseAngle), true, D(y.BaseAngle));
        F(arc, "flags", I(p.Flags), true, I(d.Flags));
        F(arc, "light", I(p.Light), true, I(d.Light));
        F(arc, "texture", I(p.Texture), true, I(d.Texture));
        F(arc, "texz", D(p.TexZ), true, D(d.TexZ));
        F(arc, "alpha", D(p.Alpha), true, D(d.Alpha));
        F(arc, "glowcolor", H(p.Glow), true, H(d.Glow));
        F(arc, "glowheight", G(p.GlowHeight), true, G(d.GlowHeight));
        Texture(arc, "texturefx", p.Fx, d.Fx);
        arc.EndObject();
        return arc;
    }

    static void Normal(Arc arc, Sec p, Sec? def)
    {
        bool same = def != null && p.X.Equals(def.X) && p.Y.Equals(def.Y) && p.Z.Equals(def.Z);
        if (!arc.Rollback && arc.Writing && same) return;
        if (!arc.BeginArray("normal")) return;
        if (arc.Reading)
        {
            if (arc.ReadX.HasValue) p.X = arc.ReadX.Value;
            if (arc.ReadY.HasValue) p.Y = arc.ReadY.Value;
            if (arc.ReadZ.HasValue) p.Z = arc.ReadZ.Value;
        }
        string other = def == null ? "null" : D(def.X) + "," + D(def.Y) + "," + D(def.Z);
        arc.Log.Add("f64:" + D(p.X) + "," + D(p.Y) + "," + D(p.Z) + "|" + other);
        arc.EndArray();
    }

    public static Arc Secplane(Arc arc, string? key, Sec p, Sec? def)
    {
        if (arc.CanSkip && def != null && p.Same(def)) return arc;
        if (!arc.BeginObject(key)) return arc;
        Normal(arc, p, def);
        if (arc.Reading && arc.ReadD.HasValue) p.D = arc.ReadD.Value;
        F(arc, "d", D(p.D), true, def == null ? "null" : D(def.D));
        arc.EndObject();
        if (arc.Reading && p.Z != 0) p.NegiC = -1.0 / p.Z;
        return arc;
    }
}
