namespace HCDE.Playsim;

/// <summary>
/// Doom font selection from <c>doomfont.cpp</c> (<b>1–170</b>). Chunk <b>1188</b>.
/// Lump lookups and <c>V_GetFont</c> are host calls. This slice decides which font is built.
/// </summary>
public static class VmDoomFont1188Slice
{
    public const int FontStart = '!';
    public const int FontEnd = 255;
    public const int FontSize = FontEnd - FontStart + 1;
    public const int FontFolder = 1;

    public static Font? SmallFont;
    public static Font? OriginalSmallFont;
    public static Font? SmallFont2;
    public static Font? BigFont;
    public static Font? OriginalBigFont;
    public static Font? BigUpper;
    public static Font? ConFont;
    public static Font? IntermissionFont;
    public static Font? AlternativeSmallFont;
    public static Font? AlternativeBigFont;

    public sealed class FatalError : Exception
    {
        public FatalError(string message)
            : base(message)
        {
            Detail = message;
        }

        public string Detail;
    }

    public sealed class Font
    {
        public string Name = "";
        public string? NameTemplate;
        public string? FileTemplate;
        public int First;
        public int Count;
        public int Base;
        public int FdLump;
        public int SpaceWidth = -1;
        public bool NoTranslate;
        public bool IwadOnly;
        public bool DoomTemplate;
        public int Type;
        public char Cursor;
        public bool Deleted;
    }

    public sealed class Host
    {
        public readonly Dictionary<string, int> Names = new(StringComparer.OrdinalIgnoreCase);
        public readonly Dictionary<string, int> Graphics = new(StringComparer.OrdinalIgnoreCase);
        public readonly Dictionary<int, int> Containers = new();
        public int IwadNum;
        public bool Winum0;
        public Func<string, string?, Font?> GetFont = (_, _) => null;
        public Func<string, Font?> FindFont = _ => null;

        public int CheckNumForName(string name, bool graphics)
        {
            var table = graphics ? Graphics : Names;
            if (table.TryGetValue(name, out var lump))
                return lump;
            return -1;
        }

        public int GetFileContainer(int lump)
        {
            if (Containers.TryGetValue(lump, out var container))
                return container;
            return -1;
        }
    }

    public static void Clear()
    {
        SmallFont = null;
        OriginalSmallFont = null;
        SmallFont2 = null;
        BigFont = null;
        OriginalBigFont = null;
        BigUpper = null;
        ConFont = null;
        IntermissionFont = null;
        AlternativeSmallFont = null;
        AlternativeBigFont = null;
    }

    public static void Init(Host host)
    {
        SmallFont = host.GetFont("SmallFont", "SMALLFNT");
        if (SmallFont == null)
        {
            if (host.CheckNumForName("FONTA_S", false) >= 0)
            {
                var wadfile = -1;
                var a = host.CheckNumForName("FONTA33", true);
                if (a != -1)
                    wadfile = host.GetFileContainer(a);
                if (wadfile > host.IwadNum)
                    SmallFont = Make("SmallFont", "FONTA%02u", null, FontStart, FontSize, 1, -1);
                else
                    SmallFont = Make("SmallFont", "FONTA%02u", "defsmallfont", FontStart, FontSize, 1, -1);
                SmallFont.Cursor = '[';
            }
            else if (host.CheckNumForName("STCFN033", true) >= 0)
            {
                var wadfile = -1;
                var a = host.CheckNumForName("STCFN065", true);
                if (a != -1)
                    wadfile = host.GetFileContainer(a);
                if (wadfile > host.IwadNum)
                {
                    SmallFont = Make(
                        "SmallFont", "STCFN%.3d", null, FontStart, FontSize, FontStart, -1, -1, false, false, true);
                }
                else
                {
                    SmallFont = Make(
                        "SmallFont", "STCFN%.3d", "defsmallfont", FontStart, FontSize, FontStart, -1, -1, false, false, true);
                }
            }
        }

        if (host.CheckNumForName("FONTA_S", false) >= 0)
        {
            OriginalSmallFont = Make(
                "OriginalSmallFont", "FONTA%02u", "defsmallfont", FontStart, FontSize, 1, -1, -1, false, true);
            OriginalSmallFont.Cursor = '[';
        }
        else if (host.CheckNumForName("STCFN033", true) >= 0)
        {
            OriginalSmallFont = Make(
                "OriginalSmallFont", "STCFN%.3d", "defsmallfont", FontStart, FontSize, FontStart, -1, -1, false, true, true);
        }

        SmallFont2 = host.GetFont("SmallFont2", null);
        if (SmallFont2 == null)
        {
            if (host.CheckNumForName("STBFN033", true) >= 0)
            {
                SmallFont2 = Make(
                    "SmallFont2", "STBFN%.3d", "defsmallfont2", FontStart, FontSize, FontStart, -1);
            }
        }

        BigUpper = host.GetFont("BigUpper", null);
        BigFont = host.GetFont("BigFont", null);
        if (BigFont == null)
        {
            if (host.CheckNumForName("FONTB_S", false) >= 0)
                BigFont = Make("BigFont", "FONTB%02u", "defbigfont", FontStart, FontSize, 1, -1);
        }

        if (BigFont == null)
            BigFont = host.GetFont("BigFont", "ZBIGFONT");

        if (host.CheckNumForName("FONTB_S", false) >= 0)
        {
            OriginalBigFont = Make(
                "OriginalBigFont", "FONTB%02u", "defbigfont", FontStart, FontSize, 1, -1, -1, false, true);
        }
        else
        {
            OriginalBigFont = Make(
                "OriginalBigFont", null, "bigfont", FontStart, FontSize, 1, -1, -1, false, true);
        }

        if (BigUpper != null && BigFont!.Type != FontFolder && BigUpper.Type == FontFolder)
        {
            BigUpper.Deleted = true;
            BigUpper = BigFont;
        }

        if (BigUpper == null)
            BigUpper = BigFont;
        if (ConFont == null)
            ConFont = SmallFont;
        IntermissionFont = host.FindFont("IntermissionFont");
        if (IntermissionFont == null)
        {
            if (host.Winum0)
                IntermissionFont = host.FindFont("IntermissionFont_Doom");
            if (IntermissionFont == null)
                IntermissionFont = BigFont;
        }

        if (ConFont == null)
            throw new FatalError("Console font not found.");
        if (OriginalSmallFont == null)
            OriginalSmallFont = ConFont;
        if (SmallFont == null)
            SmallFont = OriginalSmallFont;
        if (SmallFont2 == null)
            SmallFont2 = SmallFont;
        if (BigFont == null)
            BigFont = OriginalBigFont;
        AlternativeSmallFont = OriginalSmallFont;
        AlternativeBigFont = OriginalBigFont;
    }

    static Font Make(
        string name,
        string? nameTemplate,
        string? fileTemplate,
        int first,
        int count,
        int baseChar,
        int fdLump,
        int spaceWidth = -1,
        bool noTranslate = false,
        bool iwadOnly = false,
        bool doomTemplate = false)
    {
        return new Font
        {
            Name = name,
            NameTemplate = nameTemplate,
            FileTemplate = fileTemplate,
            First = first,
            Count = count,
            Base = baseChar,
            FdLump = fdLump,
            SpaceWidth = spaceWidth,
            NoTranslate = noTranslate,
            IwadOnly = iwadOnly,
            DoomTemplate = doomTemplate,
        };
    }
}
