namespace HCDE.Playsim;

/// <summary>
/// putc through openwads from m_misc.cpp 440-802.
/// The PCX struct above this band stays uncredited. Response-file loading ends at line 406.
/// </summary>
public static class VmMisc1433Slice
{
    public const int BandStart = 440;
    public const int BandEnd = 802;
    public const int Pal = 0;
    public const int Rgb = 1;
    public const int Bgra = 2;

    public sealed class State
    {
        public string? Type = "png";
        public string? ShotArg;
        public string ShotDir = "";
        public string Screenshots = "";
        public string? AppData = "";
        public string? Home = "";
        public string Game = "Doom";
        public string Version = "";
        public string Error = "TXT_SCREENSHOTERR";
        public DateTime? When;
        public bool Quiet;
        public bool LongNames;
        public bool FailOpen;
        public bool CreateOk = true;
        public bool AppendOk = true;
        public bool FinishOk = true;
        public bool ConfigMissing = true;
        public bool WriteOk = true;
        public HashSet<string> Existing = new(StringComparer.Ordinal);
        public Dictionary<string, List<byte>> Files = new(StringComparer.Ordinal);
        public List<string> Lines = new();
        public List<string> Created = new();
        public List<string?> Requested = new();
        public List<string> Png = new();
        public string? Opened;
        public int Width = 1;
        public int Height = 1;
        public int Pitch = 1;
        public int Color = Pal;
        public float Gamma = 1f;
        public byte[] Buffer = Array.Empty<byte>();
        public byte[]? Palette;
    }

    public static void WritePcx(List<byte>? file, byte[]? buffer, byte[]? palette, int kind, int width, int height, int pitch)
    {
        if (file == null || buffer == null) throw new NullReferenceException();
        file.Add(10);
        file.Add(5);
        file.Add(1);
        file.Add(8);
        U16(file, 0);
        U16(file, 0);
        U16(file, width - 1);
        U16(file, height - 1);
        U16(file, 75);
        U16(file, 75);
        for (int i = 0; i < 48; i++) file.Add(0);
        file.Add(0);
        file.Add((byte)(kind == Pal ? 1 : 3));
        U16(file, width + (width & 1));
        U16(file, 1);
        for (int i = 0; i < 58; i++) file.Add(0);
        int offset = 0;
        for (int y = height; y > 0; y--)
        {
            byte[] row;
            if (kind == Pal) row = Slice(buffer, offset, width);
            else if (kind == Rgb) row = Planes(buffer, offset, width, 3, 0, 1, 2);
            else if (kind == Bgra) row = Planes(buffer, offset, width, 4, 2, 1, 0);
            else return;
            offset += pitch;
            Pack(file, row, width);
        }
        if (kind != Pal) return;
        if (palette == null) throw new NullReferenceException();
        file.Add(12);
        for (int x = 0; x < 256; x++)
        {
            file.Add(palette[x * 3]);
            file.Add(palette[x * 3 + 1]);
            file.Add(palette[x * 3 + 2]);
        }
    }

    public static void WritePng(State s, int kind, int width, int height, int pitch, float gamma, byte[]? palette)
    {
        if (s == null) throw new NullReferenceException();
        string text = "HCDE " + (s.Version ?? "");
        if (text.Length > 99) text = text.Substring(0, 99);
        s.Png.Add("create");
        if (!s.CreateOk || !Append(s, text) || !Finish(s)) s.Lines.Add(s.Error + "\n");
        s.Gamma = gamma;
        s.Color = kind;
        s.Width = width;
        s.Height = height;
        s.Pitch = pitch;
        s.Palette = palette;
    }

    public static bool FindFree(State s, ref string full, string? ext)
    {
        if (s.Game == null || ext == null) throw new NullReferenceException();
        for (int i = 0; i <= 9999; i++)
        {
            string name = s.When == null
                ? full + "Screenshot_" + s.Game + "_" + i.ToString("D4") + "." + ext
                : i == 0
                    ? full + "Screenshot_" + s.Game + "_" + s.When.Value.ToString("yyyyMMdd") + "_" + s.When.Value.ToString("HHmmss") + "." + ext
                    : full + "Screenshot_" + s.Game + "_" + s.When.Value.ToString("yyyyMMdd") + "_" + s.When.Value.ToString("HHmmss") + "_" + i.ToString("D2") + "." + ext;
            if (s.Existing.Contains(name)) continue;
            full = name;
            return true;
        }
        return false;
    }

    public static void Shot(State s, string? filename)
    {
        if (s.Type == null) throw new NullReferenceException();
        bool pcx = Same(s.Type, "pcx");
        string name;
        if (filename == null || filename.Length == 0)
        {
            name = !string.IsNullOrEmpty(s.ShotArg) ? s.ShotArg : s.ShotDir.Length != 0 ? s.ShotDir : s.Screenshots;
            if (name.Length > 0 && name[name.Length - 1] != '/' && name[name.Length - 1] != '\\') name += "/";
            name = Nice(s, name);
            s.Created.Add(name);
            if (!FindFree(s, ref name, pcx ? "pcx" : "png"))
            {
                s.Lines.Add("M_ScreenShot: Delete some screenshots\n");
                return;
            }
        }
        else name = Extend(filename, pcx ? ".pcx" : ".png");
        if (s.Buffer.Length == 0)
        {
            if (!s.Quiet) s.Lines.Add("Could not create screenshot.\n");
            return;
        }
        if (s.FailOpen)
        {
            s.Lines.Add("Could not open " + name + "\n");
            return;
        }
        var file = new List<byte>();
        s.Files[name] = file;
        if (pcx) WritePcx(file, s.Buffer, s.Palette, s.Color, s.Width, s.Height, s.Pitch);
        else WritePng(s, s.Color, s.Width, s.Height, s.Pitch, s.Gamma, s.Palette);
        if (s.Quiet) return;
        int slash = -1;
        if (!s.LongNames) slash = LastSep(name);
        s.Lines.Add("Captured " + name.Substring(slash + 1) + "\n");
    }

    public static void Command(State s, IReadOnlyList<string> argv)
    {
        s.Requested.Add(argv.Count == 1 ? null : argv[1]);
    }

    public static void OpenShots(State s)
    {
        string name = !string.IsNullOrEmpty(s.ShotArg) ? s.ShotArg : s.ShotDir.Length != 0 ? s.ShotDir : s.Screenshots;
        if (name.Length > 0 && name[name.Length - 1] != '/' && name[name.Length - 1] != '\\') name += "/";
        name = Nice(s, name);
        s.Created.Add(name);
        s.Opened = name;
    }

    public static int SaveConfig(State s) => s.ConfigMissing || s.WriteOk ? 1 : 0;

    public static int SaveAction(State s) => SaveConfig(s);

    public static void OpenWads(State s)
    {
        string name = s.AppData ?? "";
        if (name.Length > 0 && name[name.Length - 1] != '/' && name[name.Length - 1] != '\\') name += "/";
        name = Nice(s, name);
        s.Created.Add(name);
        s.Opened = name;
    }

    static bool Append(State s, string text)
    {
        if (!s.CreateOk) return false;
        s.Png.Add("software:" + text);
        return s.AppendOk;
    }

    static bool Finish(State s)
    {
        if (!s.AppendOk) return false;
        s.Png.Add("finish");
        return s.FinishOk;
    }

    static void Pack(List<byte> file, byte[] row, int width)
    {
        int p = 0;
        int color = row[p++];
        int run = 1;
        for (int x = row.Length - 1; x > 0; x--)
        {
            if (row[p] == color) run++;
            else
            {
                Flush(file, run, color);
                run = 1;
                color = row[p];
            }
            p++;
        }
        Flush(file, run, color);
        if ((width & 1) != 0) file.Add(0);
    }

    static void Flush(List<byte> file, int run, int color)
    {
        if (run > 1 || color >= 0xC0)
        {
            while (run > 63)
            {
                file.Add(0xFF);
                file.Add((byte)color);
                run -= 63;
            }
            if (run > 0) file.Add((byte)(0xC0 + run));
        }
        if (run > 0) file.Add((byte)color);
    }

    static byte[] Planes(byte[] buffer, int offset, int width, int step, int r, int g, int b)
    {
        var row = new byte[width * 3];
        for (int i = 0; i < width; i++)
        {
            int at = offset + i * step;
            row[i] = buffer[at + r];
            row[i + width] = buffer[at + g];
            row[i + width * 2] = buffer[at + b];
        }
        return row;
    }

    static byte[] Slice(byte[] buffer, int offset, int width)
    {
        var row = new byte[width];
        System.Buffer.BlockCopy(buffer, offset, row, 0, width);
        return row;
    }

    static string Extend(string path, string ext)
    {
        int i = path.Length - 1;
        while (i > 0 && path[i] != '/')
        {
            if (path[i] == '.') return path;
            i--;
        }
        return path + ext;
    }

    static string Nice(State s, string? path)
    {
        if (string.IsNullOrEmpty(path)) return "";
        if (path[0] == '~' && (path.Length == 1 || path[1] == '/')) return (s.Home ?? "") + (path.Length == 1 ? "" : path.Substring(1));
        return path;
    }

    static int LastSep(string name)
    {
        for (int i = name.Length - 1; i >= 0; i--)
            if (name[i] is ':' or '/' or '\\') return i;
        return -1;
    }

    static bool Same(string a, string b)
    {
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
        {
            int ca = a[i] is >= 'A' and <= 'Z' ? a[i] + 32 : a[i];
            int cb = b[i] is >= 'A' and <= 'Z' ? b[i] + 32 : b[i];
            if (ca != cb) return false;
        }
        return true;
    }

    static void U16(List<byte> file, int value)
    {
        file.Add((byte)value);
        file.Add((byte)(value >> 8));
    }
}
