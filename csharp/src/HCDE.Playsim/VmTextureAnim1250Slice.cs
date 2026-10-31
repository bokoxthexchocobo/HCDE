namespace HCDE.Playsim;

/// <summary>
/// Texture animation setup from <c>animations.cpp</c> (<b>1–294</b>).
/// Chunk <b>1250</b>. Texture catalog, lump bytes, and hardware cleanup are host tables.
/// <c>InitAnimDefs</c> starts at line 295 and is not part of this slice.
/// The animation index map is left in place by <c>DeleteAll</c>.
/// </summary>
public sealed class VmTextureAnim1250
{
    public const int Wall = 1;
    public const int Flat = 2;
    public const int Override = 10;
    public const int Overridable = 2;
    public const int TicRate = 35;
    public const int Forward = 0;
    public const int Backward = 1;

    public bool DebugAnimated;
    public readonly List<string> Log = new();
    public readonly List<Tex> Textures = new();
    public readonly List<int> FileStarts = new();
    public readonly Dictionary<string, int> Lumps = new(StringComparer.Ordinal);
    public readonly Dictionary<int, byte[]> Bytes = new();
    public readonly Dictionary<int, int> Lengths = new();
    public readonly List<Anim> Anims = new();
    public readonly Dictionary<int, int> Indices = new();
    public readonly List<Fire> Fires = new();
    public readonly List<SwitchSlot?> Switches = new();
    public readonly List<int> Doors = new();

    public void DeleteAll()
    {
        for (int i = 0; i < Fires.Count; i++)
        {
            Fires[i].Clean(true);
            Log.Add($"clean {i} force=1");
        }
        int anims = Anims.Count;
        int fires = Fires.Count;
        int switches = Switches.Count;
        int doors = Doors.Count;
        Anims.Clear();
        Fires.Clear();
        for (int i = 0; i < Switches.Count; i++)
        {
            if (Switches[i] != null)
            {
                Switches[i]!.Freed = true;
                Switches[i] = null;
                Log.Add($"free switch {i}");
            }
        }
        Switches.Clear();
        Doors.Clear();
        Log.Add($"delete anims={anims} fires={fires} switches={switches} doors={doors} indices={Indices.Count}");
    }

    public Anim? AddAnim(Anim anim)
    {
        if (Indices.TryGetValue(anim.Base, out int index))
        {
            if ((uint)index >= (uint)Anims.Count)
            {
                Log.Add($"anim stale {index} {Anims.Count}");
                return null;
            }
            Anims[index] = anim;
            Log.Add($"anim replace idx={index} base={anim.Base}");
            return Anims[index];
        }
        Indices[anim.Base] = Anims.Count;
        Anims.Add(anim);
        Log.Add($"anim add idx={Anims.Count - 1} base={anim.Base}");
        return anim;
    }

    public Anim? AddSimple(int picnum, int animcount, uint speedmin, uint speedrange = 0)
    {
        int end = Plus(picnum, animcount - 1);
        Log.Add($"simple check pic={picnum} count={animcount} end={end}");
        if (!Compatible(picnum, end))
        {
            Log.Add("simple incompatible");
            return null;
        }
        var anim = new Anim
        {
            Cur = 0,
            Base = picnum,
            Count = animcount,
            Type = Forward,
            Discrete = false,
            Switch = 0,
            Frames = new[]
            {
                new Frame { SpeedMin = speedmin, SpeedRange = speedrange, Pic = picnum },
            },
        };
        Anim? stored = AddAnim(anim);
        if (stored == null)
            return null;
        Frame frame = stored.Frames[0];
        Log.Add($"simple stored count={stored.Count} nframe={stored.Frames.Length} speed={frame.SpeedMin} range={frame.SpeedRange} pic={frame.Pic} discrete=0");
        return stored;
    }

    public Anim? AddComplex(int picnum, IReadOnlyList<Frame> frames)
    {
        var copy = new Frame[frames.Count];
        for (int i = 0; i < frames.Count; i++)
        {
            Frame src = frames[i];
            copy[i] = new Frame { SpeedMin = src.SpeedMin, SpeedRange = src.SpeedRange, Pic = src.Pic };
        }
        var anim = new Anim
        {
            Base = picnum,
            Count = frames.Count,
            Cur = 0,
            Type = Forward,
            Discrete = true,
            Switch = 0,
            Frames = copy,
        };
        Anim? stored = AddAnim(anim);
        if (stored == null)
        {
            Log.Add("complex stale");
            return null;
        }
        if (stored.Frames.Length == 0)
        {
            Log.Add($"complex base={stored.Base} count=0 type={stored.Type} discrete=1");
            return stored;
        }
        Frame first = stored.Frames[0];
        string extra = stored.Frames.Length > 1
            ? $" f1={stored.Frames[1].SpeedMin},{stored.Frames[1].SpeedRange},{stored.Frames[1].Pic}"
            : "";
        Log.Add($"complex base={stored.Base} count={stored.Count} type={stored.Type} discrete=1 f0={first.SpeedMin},{first.SpeedRange},{first.Pic}{extra}");
        return stored;
    }

    public void InitAnimated()
    {
        if (!Lumps.TryGetValue("ANIMATED", out int lump) || lump < 0)
        {
            Log.Add("animated missing");
            return;
        }
        if (!Bytes.TryGetValue(lump, out byte[]? data) || data == null)
        {
            Log.Add("animated null");
            return;
        }
        int len = Lengths.TryGetValue(lump, out int forced) ? forced : data.Length;
        Log.Add($"animated lump={lump} len={len} flags={Overridable}");
        int animtype = Forward;
        int offset = 0;
        while (true)
        {
            if (offset >= len || offset >= data.Length)
            {
                Log.Add("Tried to read past end of ANIMATED lump.");
                return;
            }
            if (data[offset] == 0xFF)
            {
                Log.Add($"animated end off={offset}");
                return;
            }
            if (offset + 22 >= len || offset + 22 >= data.Length)
            {
                Log.Add("Tried to read past end of ANIMATED lump.");
                return;
            }
            byte flag = data[offset];
            bool wall = (flag & 1) != 0;
            int kind = wall ? Wall : Flat;
            string endName = CString(data, offset + 1, len);
            string startName = CString(data, offset + 10, len);
            int pic1 = Check(startName, kind);
            int pic2 = 0;
            string pic2Text;
            if (pic1 < 0)
            {
                pic2Text = "na";
            }
            else
            {
                pic2 = Check(endName, kind);
                pic2Text = pic2.ToString();
            }
            Log.Add($"rec off={offset} wall={(wall ? 1 : 0)} decalbit={((flag & 2) != 0 ? 1 : 0)} start={startName} end={endName} pic1={pic1} pic2={pic2Text}");
            if (pic1 < 0 || pic2 < 0)
            {
                Log.Add("rec skip missing");
                offset += 23;
                continue;
            }
            Tex? tex1 = Game(pic1);
            Tex? tex2 = Game(pic2);
            if (wall)
            {
                bool nodecals = (flag & 2) == 0;
                if (tex2 != null)
                    tex2.NoDecals = nodecals;
                if (tex1 != null)
                    tex1.NoDecals = nodecals;
                Log.Add($"nodecals {(nodecals ? 1 : 0)} {pic2} {pic1}");
            }
            uint speed = ReadLe(data, offset + 19);
            Log.Add($"speed {speed}");
            if (speed > 65535 && tex1 != null && tex1.Warp == 0)
            {
                tex1.Warp = 2;
                Log.Add($"warp {pic1} style=2");
                offset += 23;
                continue;
            }
            if (tex1 == null || tex2 == null)
            {
                Log.Add("tex null");
                offset += 23;
                continue;
            }
            if (tex1.Use != tex2.Use)
            {
                Log.Add("rec skip type");
                offset += 23;
                continue;
            }
            if (DebugAnimated)
            {
                Log.Add(
                    $"Defining animation '{tex1.Name}' (texture {pic1}, lump {tex1.Lump}, file {tex1.File}) to '{tex2.Name}' (texture {pic2}, lump {tex2.Lump}, file {tex2.File})");
            }
            if (pic1 == pic2)
            {
                Log.Add($"Animation {startName} in ANIMATED has only one frame");
                offset += 23;
                continue;
            }
            if (pic1 > pic2)
            {
                (pic1, pic2) = (pic2, pic1);
                animtype = Backward;
                Log.Add($"swap pic1={pic1} pic2={pic2} type={animtype}");
            }
            int count = pic2 - pic1 + 1;
            int scaled = Scale(unchecked((int)speed), 1000, TicRate);
            Anim? adef = AddSimple(pic1, count, (uint)scaled);
            if (adef != null)
            {
                adef.Type = animtype;
                Log.Add($"simple base={adef.Base} frames={adef.Count} type={adef.Type} speed={adef.Frames[0].SpeedMin}");
            }
            else
            {
                Log.Add("simple null");
            }
            offset += 23;
        }
    }

    public int Check(string? name, int use)
    {
        if (string.IsNullOrEmpty(name))
            return -1;
        if (name.Length == 1 && name[0] == '-')
            return 0;
        for (int i = 0; i < Textures.Count; i++)
        {
            Tex tex = Textures[i];
            if (!name.Equals(tex.Name, StringComparison.OrdinalIgnoreCase))
                continue;
            if (tex.Use == use || tex.Use == Override)
                return i;
        }
        return -1;
    }

    public int Plus(int id, int offset)
    {
        if (id <= 0)
            return id;
        if (id + offset >= Textures.Count)
            return -1;
        return id + offset;
    }

    public bool Compatible(int a, int b)
    {
        if ((uint)a >= (uint)Textures.Count || (uint)b >= (uint)Textures.Count)
            return false;
        Tex t1 = Textures[a];
        Tex t2 = Textures[b];
        if (t1.Use != t2.Use)
            return false;
        for (int i = 0; i < FileStarts.Count - 1; i++)
        {
            if (a >= FileStarts[i] && a < FileStarts[i + 1])
                return b >= FileStarts[i] && b < FileStarts[i + 1];
        }
        return false;
    }

    public static int Scale(int a, int b, int c)
    {
        return (int)(((long)a * b) / c);
    }

    public Tex? Game(int id)
    {
        if ((uint)id >= (uint)Textures.Count)
            return null;
        return Textures[id];
    }

    static string CString(byte[] data, int start, int len)
    {
        int i = start;
        int stop = Math.Min(len, data.Length);
        while (i < stop && data[i] != 0)
            i++;
        return System.Text.Encoding.ASCII.GetString(data, start, i - start);
    }

    static uint ReadLe(byte[] data, int at)
    {
        return data[at]
            | ((uint)data[at + 1] << 8)
            | ((uint)data[at + 2] << 16)
            | ((uint)data[at + 3] << 24);
    }

    public sealed class Tex
    {
        public string Name = "";
        public int Use;
        public int Lump;
        public int File;
        public int Warp;
        public bool NoDecals;
    }

    public sealed class Frame
    {
        public uint SpeedMin;
        public uint SpeedRange;
        public int Pic;
    }

    public sealed class Anim
    {
        public int Base;
        public int Count;
        public int Cur;
        public int Type;
        public bool Discrete;
        public ulong Switch;
        public Frame[] Frames = Array.Empty<Frame>();
    }

    public sealed class Fire
    {
        public bool Cleaned;

        public void Clean(bool force)
        {
            Cleaned = force;
        }
    }

    public sealed class SwitchSlot
    {
        public bool Freed;
    }
}
