using System.Globalization;
using System.Text;

namespace HCDE.Playsim;
/// <summary>Texture hacks, GC roots, and file-system messages from d_main.cpp 3646-3848.</summary>
public static class VmDMain1420Slice
{
    public const int BandStart = 3646;
    public const int BandEnd = 3848;
    public const int DimLine = 3644;
    public const int InitGame = 3856;
    public const int Doom = 1;
    public const int Heretic = 2;
    public const int Hexen = 4;
    public const int MapXx = 1;
    public const int Wall = 1;
    public const int MiscPatch = 8;
    public const int NullTex = 13;
    public const int MaxPlayers = 64;
    public const int FullConsole = 17;
    public const int Intro = 19;
    public const int Intermission = 20;
    public const int WipeNone = 0;
    public const int WipeMelt = 1;
    public const int WipeBurn = 2;
    public const int WipeFade = 3;
    public const int ForceFade = -2;
    public const int ForceBurn = -3;
    public const int ForceMelt = -4;
    public const int FsError = 1;
    public const int FsWarning = 2;
    public const int FsAttention = 3;
    public const int FsMessage = 4;
    public const int FsDebugWarn = 5;
    public const int FsDebugNotify = 6;
    public const int DevWarning = 2;
    public const int DevNotify = 3;
    public const string Red = "\u001cG";
    public const string Blue = "\u001cH";
    public const string Yellow = "\u001cK";
    public sealed class Obj
    {
        public bool Released;
        public bool Euthanize;
        public bool White = true;
        public bool Gray;
    }
    public sealed class LevelNode
    {
        public int Marks;
        public bool Automap;
        public int UiScales;
    }
    public sealed class Bar
    {
        public int Scales;
    }
    public sealed class Tex
    {
        public int UseType = Wall;
        public int Width;
        public float ScaleX = 1f;
        public readonly int[] Left = new int[2];
        public readonly int[] Top = new int[2];
    }
    public sealed class Part
    {
        public int OriginY;
        public int ImageHeight;
        public bool Image = true;
    }
    public sealed class Build
    {
        public string? Name = "";
        public int Width;
        public int Height;
        public List<Part> Parts = new();
        public int SizedW = int.MinValue;
        public int SizedH = int.MinValue;
    }
    public sealed class Text
    {
        public string? Value;
    }
    public sealed class State
    {
        public int GameType;
        public int Flags;
        public int BlankId = -1;
        public int BlankLooks;
        public List<Tex> Textures = new();
        public Tex? StatusTex;
        public List<string>? Sprites = new();
        public Obj? First;
        public Obj? Last;
        public Obj? Next;
        public List<Obj> Gray = new();
        public List<LevelNode?>? Levels = new();
        public readonly bool[] InGame = new bool[MaxPlayers];
        public int PlayerMarks;
        public int RollbackMarks;
        public int GameAction;
        public int Wipe = 7;
        public Bar? Status;
        public bool SizeNeeded;
        public LevelNode? Primary;
        public bool GameDead;
        public bool Console = true;
        public int Developer;
        public List<string> Prints = new();
        public Dictionary<string, byte[]?> Files = new();
    }
    public static void PatchTextures(State s)
    {
        if (s.GameType != Hexen) return;
        s.BlankLooks++;
        if (s.BlankId >= 0)
        {
            Tex tex = TextureAt(s, s.BlankId) ?? throw new NullReferenceException();
            tex.UseType = NullTex;
        }
    }
    public static void CheckForHacks(State s, Build b)
    {
        if (b.Parts.Count == 0) return;
        string name = b.Name ?? throw new NullReferenceException();
        if (s.GameType == Heretic && name.Length == 4 && name[0] == 'S' && name[1] == 'K' && name[2] == 'Y' &&
            name[3] >= '1' && name[3] <= '3' && b.Height == 128 && b.Parts.Count == 1)
        {
            b.Height = ImageHeight(b.Parts[0]);
            b.SizedW = b.Width;
            b.SizedH = b.Height;
            return;
        }
        if (s.GameType == Doom && (s.Flags & MapXx) == 0 && name.Length == 4 && b.Parts.Count == 1 &&
            b.Height == 128 && b.Parts[0].OriginY == -8 && name[0] == 'S' && name[1] == 'K' && name[2] == 'Y' && name[3] == '1')
        {
            b.Parts[0].OriginY = 0;
            return;
        }
        if (s.GameType == Doom && (s.Flags & MapXx) == 0 && SameName(name, "BIGDOOR7") && b.Parts.Count == 2 &&
            b.Height == 128 && b.Parts[0].OriginY == -4 && b.Parts[1].OriginY == -4)
        {
            b.Parts[0].OriginY = 0;
            b.Parts[1].OriginY = 0;
        }
    }
    public static void FixWideStatusBar(State s)
    {
        Tex? bar = s.StatusTex;
        if (bar != null && bar.Width > 320 && bar.ScaleX == 1f && bar.Left[0] == 0 && bar.Top[0] == 0)
        {
            int x = (bar.Width - 320) / 2;
            bar.Left[0] = x;
            bar.Top[0] = 0;
            bar.Left[1] = x;
            bar.Top[1] = 0;
        }
    }
    public static void CastSprite(Text dest, uint id, State s)
    {
        if (dest == null) throw new NullReferenceException();
        if (s.Sprites == null) throw new NullReferenceException();
        dest.Value = id >= (uint)s.Sprites.Count ? "TNT1" : s.Sprites[(int)id];
    }
    public static void MarkGameRoots(State s)
    {
        Mark(s, ref s.First);
        Mark(s, ref s.Last);
        if (s.Levels == null) throw new NullReferenceException();
        foreach (LevelNode? level in s.Levels)
        {
            if (level == null) throw new NullReferenceException();
            level.Marks++;
        }
        for (int i = 0; i < MaxPlayers; i++)
            if (s.InGame[i]) s.PlayerMarks++;
        Mark(s, ref s.Next);
        s.RollbackMarks++;
    }
    public static void ToggleFullConsole(State s) => s.GameAction = FullConsole;
    public static void StartCutscene(State s, bool blockUi) => s.GameAction = blockUi ? Intro : Intermission;
    public static void SetTransition(State s, int type)
    {
        if (type != WipeNone)
            s.Wipe = type == WipeBurn ? ForceBurn : type == WipeFade ? ForceFade : ForceMelt;
    }
    public static void HudScaleChanged(State s)
    {
        if (s.Status != null)
        {
            s.Status.Scales++;
            s.SizeNeeded = true;
        }
        if (s.Primary != null && s.Primary.Automap) s.Primary.UiScales++;
    }
    public static bool FileNameCheck(State s, string? baseName, string? path)
    {
        if (baseName == null) throw new NullReferenceException();
        if (!PrefixTextures(baseName)) return true;
        if (path == null) throw new NullReferenceException();
        if (!s.Files.TryGetValue(path, out byte[]? data) || data == null) return true;
        var check = new byte[3];
        int n = data.Length < 3 ? data.Length : 3;
        for (int i = 0; i < n; i++) check[i] = data[i];
        if (check[0] == (byte)'L' && check[1] == (byte)'Z' && check[2] == (byte)'4') return false;
        return true;
    }
    public static int FileSystemPrintf(State s, int level, string? fmt, params object?[] args)
    {
        string text = Format(fmt, args);
        switch (level)
        {
            case FsError: return Print(s, Red + text);
            case FsWarning: Print(s, Yellow + text); break;
            case FsAttention: Print(s, Blue + text); break;
            case FsMessage: Print(s, text); break;
            case FsDebugWarn: if (s.Developer >= DevWarning) Print(s, text); break;
            case FsDebugNotify: if (s.Developer >= DevNotify) Print(s, text); break;
        }
        return text.Length;
    }
    public static string Format(string? fmt, params object?[] args)
    {
        if (fmt == null) throw new NullReferenceException();
        var built = new StringBuilder();
        int arg = 0;
        for (int i = 0; i < fmt.Length; i++)
        {
            if (fmt[i] != '%') { built.Append(fmt[i]); continue; }
            if (i + 1 >= fmt.Length) { built.Append('%'); break; }
            char spec = fmt[++i];
            if (spec == '%') { built.Append('%'); continue; }
            if (arg >= args.Length) throw new NullReferenceException();
            object? value = args[arg++];
            if (spec == 's')
            {
                if (value == null) throw new NullReferenceException();
                built.Append(value);
            }
            else if (spec == 'd' || spec == 'i')
                built.Append(Convert.ToInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture));
            else if (spec == 'u')
                built.Append(Convert.ToUInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture));
            else throw new InvalidOperationException(spec.ToString());
        }
        return built.ToString();
    }
    static Tex? TextureAt(State s, int id)
    {
        if ((uint)id >= (uint)s.Textures.Count) return null;
        return s.Textures[id];
    }
    static int ImageHeight(Part part)
    {
        if (part == null || !part.Image) throw new NullReferenceException();
        return part.ImageHeight;
    }
    static bool SameName(string left, string right)
    {
        if (left.Length != right.Length) return false;
        for (int i = 0; i < left.Length; i++)
            if (Fold(left[i]) != Fold(right[i])) return false;
        return true;
    }
    static bool PrefixTextures(string name)
    {
        const string needle = "textures";
        for (int i = 0; i < needle.Length; i++)
        {
            char have = i < name.Length ? name[i] : '\0';
            if (Fold(have) != Fold(needle[i])) return false;
        }
        return true;
    }
    static char Fold(char c) => c is >= 'A' and <= 'Z' ? (char)(c + 32) : c;
    static void Mark(State s, ref Obj? obj)
    {
        if (obj == null || obj.Released) return;
        if (obj.Euthanize) { obj = null; return; }
        if (!obj.White) return;
        obj.White = false;
        obj.Gray = true;
        s.Gray.Add(obj);
    }
    static int Print(State s, string outline)
    {
        if (s.GameDead || !s.Console || outline.Length == 0) return 0;
        s.Prints.Add(outline);
        return outline.Length;
    }
}
