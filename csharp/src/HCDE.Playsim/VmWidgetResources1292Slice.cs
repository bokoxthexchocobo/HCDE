namespace HCDE.Playsim;

/// <summary>
/// Launcher resource open, theme selection, and font lookup from <c>widgetresourcedata.cpp</c>
/// (1-158). Chunk 1292. Archive bytes, the system theme, and the -file list are host inputs.
/// </summary>
public sealed class VmWidgetResources1292
{
    public const int ThemeAuto = 0;
    public const int ThemeDark = 1;
    public const int ThemeLight = 2;
    public const int ModeLight = 0;
    public const int ModeDark = 1;
    public const int SystemLight = 1;
    public const int SystemDark = 2;
    public const int SystemHighContrast = 8;

    public sealed class Failure : Exception
    {
        public readonly string Detail;

        public Failure(string message) : base(message)
        {
            Detail = message;
        }
    }

    public sealed class Archive
    {
        public Dictionary<string, byte[]> Entries = new(StringComparer.Ordinal);
    }

    public readonly struct FontFace
    {
        public FontFace(byte[] data, string language)
        {
            Data = data;
            Language = language;
        }

        public readonly byte[] Data;
        public readonly string Language;
    }

    static readonly (string File, string Language)[] Noto =
    {
        ("ui/noto/noto-sans-jp.ttf", "ja-*-*"),
        ("ui/noto/noto-sans-kr.ttf", "ko-*-*"),
        ("ui/noto/noto-sans-sc.ttf", "zh-Hans-*"),
        ("ui/noto/noto-sans.ttf", ""),
        ("ui/noto/noto-sans-armenian.ttf", ""),
        ("ui/noto/noto-sans-georgian.ttf", ""),
    };

    static readonly string[] SlotNames =
    {
        "main 0", "main 1", "header 0", "header 1",
        "button 0", "button 1", "hover 0", "hover 1",
        "click 0", "click 1", "border 0", "border 1",
    };

    int theme;

    public int Theme
    {
        get { return theme; }
        set
        {
            if (value < 0)
                value = 0;
            if (value > 2)
                value = 2;
            theme = value;
        }
    }

    public int SystemTheme;
    public bool ThemeAlreadyInstalled;
    public bool ThemeLoadsAttempted;
    public bool UseDark;
    public bool Contrast;
    public bool StyleSet;
    public int Mode = -1;
    public double LineEditTop;
    public byte[]? ThemeFromRoot;
    public byte[]? ThemeFromMods;
    public List<string> ExtraFiles = new();
    public List<string> Opened = new();
    public List<string> ThemeSlots = new();
    public Dictionary<string, Archive> Catalog = new(StringComparer.Ordinal);
    public Dictionary<string, byte[]?> Disk = new(StringComparer.Ordinal);

    List<Archive>? archives;
    bool pointerAssigned;

    public bool IsAvailable()
    {
        return pointerAssigned;
    }

    public void Init(string filename)
    {
        archives = new List<Archive>();
        Opened.Clear();
        pointerAssigned = true;
        Open(filename, true);
        foreach (var extra in ExtraFiles)
            Open(extra, false);

        int flags = SystemTheme;
        UseDark = Theme == ThemeDark || (Theme == ThemeAuto && (flags & SystemDark) != 0);
        Contrast = (flags & SystemHighContrast) != 0;
        Mode = UseDark ? ModeDark : ModeLight;
        if (!ThemeAlreadyInstalled)
        {
            ThemeAlreadyInstalled = true;
            ThemeLoadsAttempted = true;
            ThemeFromRoot = LoadFile("ui/theme.txt", true);
            ThemeFromMods = LoadFile("ui/theme.txt", false);
        }

        ThemeSlots.Clear();
        ThemeSlots.AddRange(SlotNames);
        StyleSet = true;
        LineEditTop = 6;
    }

    public void Close()
    {
        if (archives != null)
            archives.Clear();
    }

    public byte[] LoadFile(string name, bool root)
    {
        if (!pointerAssigned || archives == null)
            throw new Failure("InitWidgetResources has not been called");

        int start = root ? 0 : archives.Count - 1;
        for (int i = start; i >= 0; i--)
        {
            if ((uint)i >= (uint)archives.Count)
                continue;
            if (!archives[i].Entries.TryGetValue(name, out var bytes))
                continue;
            return bytes.ToArray();
        }

        throw new Failure("Unable to find " + name);
    }

    public byte[] LoadDisk(string name)
    {
        if (!Disk.TryGetValue(name, out var bytes) || bytes == null)
            return Array.Empty<byte>();
        return bytes.ToArray();
    }

    public List<FontFace> LoadFont(string name, bool root)
    {
        var faces = new List<FontFace>();
        if (SameAscii(name, "notosans"))
        {
            foreach (var face in Noto)
                faces.Add(new FontFace(LoadFile(face.File, root), face.Language));
            return faces;
        }

        faces.Add(new FontFace(LoadFile("ui/font/" + name + ".ttf", root), ""));
        return faces;
    }

    public byte[] LoadData(string name, bool root)
    {
        return LoadFile(name, root);
    }

    void Open(string path, bool required)
    {
        if (Catalog.TryGetValue(path, out var archive))
        {
            archives!.Add(archive);
            Opened.Add(path);
            return;
        }

        if (required)
            throw new Failure("Unable to open " + path);
    }

    static bool SameAscii(string left, string right)
    {
        if (left.Length != right.Length)
            return false;
        for (int i = 0; i < left.Length; i++)
        {
            int a = left[i];
            int b = right[i];
            if (a >= 'A' && a <= 'Z')
                a += 32;
            if (b >= 'A' && b <= 'Z')
                b += 32;
            if (a != b)
                return false;
        }

        return true;
    }
}
