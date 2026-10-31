namespace HCDE.Playsim;
/// <summary>
/// Theme fallback colors, <c>theme.txt</c> commands, and layer mix from <c>themedata.cpp</c> (<b>1–166</b>).
/// Chunk <b>1202</b>. The two file loads are the pk3 bytes and then the mod bytes.
/// A blend between the endpoints uses linear sRGB. Native <c>Color::mix</c> is OKLCH in <c>colorspace.cpp</c>.
/// </summary>
public static class VmThemeData1202Slice
{
    public const int Light = 0;
    public const int Dark = 1;
    static readonly string[] Commands =
    {
        "[light]", "[dark]", "accent", "main", "header", "button", "hover", "click", "border",
    };
    public struct Colorf
    {
        public float R, G, B, A;
        public Colorf(float r, float g, float b, float a = 1) { R = r; G = g; B = b; A = a; }
    }
    public struct Layers
    {
        public Colorf Bg, Fg;
    }
    public struct Data
    {
        public Layers Main, Header, Button, Hover, Click, Border;
    }
    public sealed class ScriptError : Exception
    {
        public ScriptError(string message) : base(message) { Detail = message; }
        public string Detail;
    }
    public static Data LightData, DarkData;
    public static Colorf Accent;
    public static int Mode;
    public static bool Ready;
    public static readonly List<string> Warnings = new();
    public static void Reset()
    {
        LightData = default;
        DarkData = default;
        Accent = default;
        Mode = Light;
        Ready = false;
        Warnings.Clear();
    }
    public static Colorf FromRgb(int rgb)
    {
        uint packed = ((uint)rgb << 8) | 0xff;
        const float scale = 1f / 255f;
        return new Colorf((0xff & (packed >> 24)) * scale, (0xff & (packed >> 16)) * scale, (0xff & (packed >> 8)) * scale, (0xff & packed) * scale);
    }
    public static void Initialize(int mode, bool contrast, string? pk3 = null, string? mods = null)
    {
        if (Ready)
            return;
        SetMode(mode);
        Paint(ref LightData, 0xffffff, 0x000000, 0xffff00, 0x000000);
        Paint(ref DarkData, 0x000000, 0xffffff, 0x0000ff, 0xffffff);
        Accent = FromRgb(0x7f7f7f);
        Load(pk3, contrast);
        Load(mods, contrast);
    }
    public static void SetMode(int mode)
    {
        Mode = mode;
        Ready = true;
    }
    public static Data Active => Mode == Light ? LightData : DarkData;
    public static Colorf Mix(Layers color, float amount)
    {
        if (amount <= 0)
            return new Colorf(color.Bg.R, color.Bg.G, color.Bg.B);
        if (amount >= 1)
            return new Colorf(color.Fg.R, color.Fg.G, color.Fg.B);
        return new Colorf(
            color.Bg.R + (color.Fg.R - color.Bg.R) * amount,
            color.Bg.G + (color.Fg.G - color.Bg.G) * amount,
            color.Bg.B + (color.Fg.B - color.Bg.B) * amount);
    }
    public static Colorf GetAccent() => Accent;
    public static Colorf GetMain(float amount) => Mix(Active.Main, amount);
    public static Colorf GetHeader(float amount) => Mix(Active.Header, amount);
    public static Colorf GetButton(float amount) => Mix(Active.Button, amount);
    public static Colorf GetHover(float amount) => Mix(Active.Hover, amount);
    public static Colorf GetClick(float amount) => Mix(Active.Click, amount);
    public static Colorf GetBorder(float amount) => Mix(Active.Border, amount);
    static void Paint(ref Data theme, int background, int foreground, int secondBackground, int secondForeground)
    {
        var back = FromRgb(background);
        var fore = FromRgb(foreground);
        var back2 = FromRgb(secondBackground);
        var fore2 = FromRgb(secondForeground);
        theme.Main.Bg = theme.Header.Bg = theme.Button.Bg = back;
        theme.Main.Fg = theme.Header.Fg = theme.Button.Fg = fore;
        theme.Hover.Fg = theme.Click.Fg = theme.Border.Bg = fore2;
        theme.Hover.Bg = theme.Click.Bg = theme.Border.Fg = back2;
    }
    static void Load(string? text, bool contrast)
    {
        var scanner = new Scanner(text ?? "");
        int? selected = null;
        while (scanner.GetString())
        {
            if (scanner.Current.Length >= 2 && scanner.Current[0] == '/' && scanner.Current[1] == '/')
                continue;
            int command = Match(scanner.Current);
            switch (command)
            {
                case -1: Warnings.Add("Unknown theme command"); break;
                case 0: selected = Light; break;
                case 1: selected = Dark; break;
                case 2: Accent = FromRgb(Hex(scanner)); break;
            }
            if (selected == null)
            {
                Warnings.Add("Theme not selected");
                continue;
            }
            if (contrast)
                continue;
            var theme = selected == Light ? LightData : DarkData;
            switch (command)
            {
                case 3: Pair(scanner, ref theme.Main); break;
                case 4: Pair(scanner, ref theme.Header); break;
                case 5: Pair(scanner, ref theme.Button); break;
                case 6: Pair(scanner, ref theme.Hover); break;
                case 7: Pair(scanner, ref theme.Click); break;
                case 8: Pair(scanner, ref theme.Border); break;
            }
            if (selected == Light)
                LightData = theme;
            else
                DarkData = theme;
        }
    }
    static void Pair(Scanner scanner, ref Layers layers)
    {
        layers.Bg = FromRgb(Hex(scanner));
        layers.Fg = FromRgb(Hex(scanner));
    }
    static int Hex(Scanner scanner)
    {
        scanner.MustGetString();
        string token = scanner.Current;
        if (token.Length == 0 || token[0] != '#')
            return -1;
        int index = 1, digits = 0;
        long value = 0;
        while (index < token.Length && IsHex(token[index]))
        {
            value = (value << 4) + HexValue(token[index]);
            index++;
            digits++;
        }
        int rgb = (int)value;
        if (digits == 3)
        {
            rgb = ((rgb >> 8) & 0xF) << 16 | ((rgb >> 4) & 0xF) << 8 | (rgb & 0xF);
            rgb |= rgb << 4;
        }
        else if (digits != 6)
        {
            rgb = -1;
        }
        return rgb;
    }
    static bool IsHex(char c) => c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';
    static int HexValue(char c)
    {
        if (c is >= '0' and <= '9')
            return c - '0';
        if (c is >= 'a' and <= 'f')
            return c - 'a' + 10;
        return c - 'A' + 10;
    }
    static int Match(string text)
    {
        for (int i = 0; i < Commands.Length; i++)
        {
            if (string.Equals(Commands[i], text, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }
    sealed class Scanner
    {
        readonly string Text;
        int Index;
        public string Current = "";
        public Scanner(string text) => Text = text;
        public bool GetString()
        {
            while (Index < Text.Length)
            {
                char c = Text[Index];
                if (c is ' ' or '\t' or '\r' or '\v' or '\f' or '\0' or '\n')
                {
                    Index++;
                    continue;
                }
                if ((c == '/' && Index + 1 < Text.Length && (Text[Index + 1] == '/' || Text[Index + 1] == '*')) || c == ';' || StartsRegion())
                {
                    if (c == '/' && Text[Index + 1] == '*')
                    {
                        Index += 2;
                        while (Index + 1 < Text.Length && !(Text[Index] == '*' && Text[Index + 1] == '/'))
                            Index++;
                        Index = Index + 1 < Text.Length ? Index + 2 : Text.Length;
                    }
                    else
                    {
                        while (Index < Text.Length && Text[Index] != '\n')
                            Index++;
                    }
                    continue;
                }
                int start = Index;
                while (Index < Text.Length && !Stops(Text[Index]))
                    Index++;
                if (Index == start)
                    Index++;
                Current = Text[start..Index];
                return true;
            }
            return false;
        }
        public void MustGetString()
        {
            if (!GetString())
                throw new ScriptError("Missing string (unexpected end of file).");
        }
        bool StartsRegion() => Text.AsSpan(Index).StartsWith("#region") || Text.AsSpan(Index).StartsWith("#endregion");
        static bool Stops(char c) => c is ' ' or '\t' or '\r' or '\n' or '\v' or '\f' or '\0' or '{' or '}' or '|' or '=' or '"' or '/' or ';';
    }
}
