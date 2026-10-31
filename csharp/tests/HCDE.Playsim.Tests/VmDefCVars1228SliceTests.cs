using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmDefCVars1228SliceTests
{
    [Fact]
    public void Grab_RenamesLegacyNamesAndStopsTheLump()
    {
        string dump = Build();
        int cut = dump.IndexOf("---- edge\n", StringComparison.Ordinal);
        string head = dump[..cut];
        Assert.Contains("default vid_gamma=9\n", head, StringComparison.Ordinal);
        Assert.Contains("default vid_fullscreen=on\n", head, StringComparison.Ordinal);
        Assert.Contains("black 'SAVE_DIR'\n", head, StringComparison.Ordinal);
        Assert.Contains("black 'save_dir'\n", head, StringComparison.Ordinal);
        Assert.Contains("black 'custom_sys'\n", head, StringComparison.Ordinal);
        Assert.Contains("black 'in_mouse'\n", head, StringComparison.Ordinal);
        Assert.Contains("nonconfig 'temp'\n", head, StringComparison.Ordinal);
        Assert.Contains("unknown 'nope'\n", head, StringComparison.Ordinal);
        Assert.Contains("unknown 'CD_DRIVE'\n", head, StringComparison.Ordinal);
        Assert.Contains("default gamma=3\n", head, StringComparison.Ordinal);
        Assert.DoesNotContain("leftover", head, StringComparison.Ordinal);
        Assert.DoesNotContain("default vid_gamma=5\n", head, StringComparison.Ordinal);
        Assert.DoesNotContain("default vid_gamma=1\n", head, StringComparison.Ordinal);
        Assert.Equal(AddGolden, head);
    }

    [Fact]
    public void Grab_RejectsAWadLumpAndABadVersion()
    {
        string dump = Build();
        string tail = dump[dump.IndexOf("---- edge\n", StringComparison.Ordinal)..];
        Assert.Contains("wad \\x1cGCannot load DEFCVARS from a wadfile!\n", tail, StringComparison.Ordinal);
        Assert.Contains("fatal Unsupported version 235 (234 supported)\n", tail, StringComparison.Ordinal);
        Assert.Contains("fatal Version must be at least 219 (current version 234)\n", tail, StringComparison.Ordinal);
        Assert.Contains("fatal SC_GetNumber: Bad numeric constant \"08\".\n", tail, StringComparison.Ordinal);
        Assert.Contains("default gamma=HEX\n", tail, StringComparison.Ordinal);
        Assert.Contains("fatal Unsupported version 2147483647 (234 supported)\n", tail, StringComparison.Ordinal);
        Assert.Contains("default vid_gamma=plus\n", tail, StringComparison.Ordinal);
        Assert.Contains("fatal Missing string (unexpected end of file).\n", tail, StringComparison.Ordinal);
        Assert.Contains("fatal Missing integer (unexpected end of file).\n", tail, StringComparison.Ordinal);
        Assert.DoesNotContain("WAD", tail, StringComparison.Ordinal);
        Assert.DoesNotContain("SKIP", tail, StringComparison.Ordinal);
        Assert.DoesNotContain("LATER", tail, StringComparison.Ordinal);
        Assert.Equal(ReadGolden, tail);
    }

    const string AddGolden = """
        lump 1
        default vid_gamma=9
        default vid_fullscreen=on
        black 'SAVE_DIR'
        lump 2
        lump 3
        default mouse_sensitivity=4
        default m_noprescale=1
        black 'save_dir'
        black 'custom_sys'
        nonconfig 'temp'
        unknown 'nope'
        default vid_gamma=7
        black 'in_mouse'
        lump 4
        unknown 'CD_DRIVE'
        default gamma=3
        state vid_gamma=7 vid_fullscreen=on gamma=3 mouse_sensitivity=4 m_noprescale=1

        """;

    const string ReadGolden = """
        ---- edge
        lump 3
        default gamma=EDGE
        wad \x1cGCannot load DEFCVARS from a wadfile!
        state vid_gamma=7 vid_fullscreen=on gamma=EDGE mouse_sensitivity=4 m_noprescale=1
        lump 8
        fatal Unsupported version 235 (234 supported)
        lump 1
        fatal Version must be at least 219 (current version 234)
        lump 1
        fatal Must declare version for defcvars! (currently: 234)
        lump 1
        fatal Missing string (unexpected end of file).
        lump 1
        fatal SC_GetNumber: Bad numeric constant "08".
        lump 1
        default gamma=HEX
        lump 1
        fatal Version must be at least 219 (current version 234)
        lump 1
        fatal Unsupported version 2147483647 (234 supported)
        lump 1
        default vid_gamma=plus
        lump 1
        fatal Version must be at least 219 (current version 234)
        lump 1
        fatal Missing string (unexpected end of file).
        lump 1
        fatal Missing integer (unexpected end of file).
        lump 1
        lump 1
        unknown 'CD_DRIVE'
        default gamma=8
        wad \x1cGCannot load DEFCVARS from a wadfile!
        state vid_gamma=plus vid_fullscreen=on gamma=8 mouse_sensitivity=4 m_noprescale=1

        """;

    static int? ReadNumber(string text)
    {
        if (text == "MAXINT")
            return int.MaxValue;
        if (!TryStrtoll(text, out long big))
            return null;
        return unchecked((int)big);
    }

    static bool TryStrtoll(string text, out long value)
    {
        value = 0;
        int i = 0;
        while (i < text.Length && IsCSpace(text[i]))
            i++;
        int sign = 1;
        if (i < text.Length && (text[i] == '+' || text[i] == '-'))
        {
            if (text[i] == '-')
                sign = -1;
            i++;
        }

        int radix = 10;
        if (i < text.Length && text[i] == '0')
        {
            if (i + 2 < text.Length && (text[i + 1] == 'x' || text[i + 1] == 'X') && Digit(text[i + 2]) >= 0 && Digit(text[i + 2]) < 16)
            {
                radix = 16;
                i += 2;
            }
            else
            {
                radix = 8;
            }
        }

        bool any = false;
        while (i < text.Length)
        {
            int digit = Digit(text[i]);
            if (digit < 0 || digit >= radix)
                break;
            any = true;
            value = value * radix + digit;
            i++;
        }

        if (!any || i != text.Length)
            return false;
        if (sign < 0)
            value = -value;
        return true;
    }

    static bool IsCSpace(char c) => c is ' ' or '\t' or '\n' or '\r' or '\v' or '\f';

    static int Digit(char c)
    {
        if (c is >= '0' and <= '9')
            return c - '0';
        if (c is >= 'a' and <= 'z')
            return c - 'a' + 10;
        if (c is >= 'A' and <= 'Z')
            return c - 'A' + 10;
        return -1;
    }

    static VmDefLump1228 L(int index, params string[] tokens) => new() { Index = index, Tokens = tokens };

    static void State(List<string> lines, VmDefCVars1228 eng)
    {
        string text = "state";
        foreach (VmDefCVar1228 cvar in eng.CVars)
            if (cvar.HasDefault)
                text += " " + cvar.Name + "=" + cvar.Default;
        lines.Add(text);
    }

    static void Run(List<string> lines, VmDefCVars1228 eng, int iwad, params VmDefLump1228[] lumps)
    {
        eng.IwadLastIndex = iwad;
        eng.Lumps.Clear();
        eng.Lumps.AddRange(lumps);
        eng.Grab();
        foreach (string line in eng.Log)
            lines.Add(line);
        eng.Log.Clear();
    }

    public static string Build()
    {
        var lines = new List<string>();
        var eng = new VmDefCVars1228 { ReadNumber = ReadNumber };
        void Add(string name, int flags) => eng.CVars.Add(new VmDefCVar1228 { Name = name, Flags = flags });
        Add("vid_gamma", VmDefCVars1228.Archive);
        Add("vid_fullscreen", VmDefCVars1228.Archive);
        Add("gamma", VmDefCVars1228.Archive);
        Add("mouse_sensitivity", VmDefCVars1228.Archive);
        Add("m_noprescale", VmDefCVars1228.Archive);
        Add("custom_sys", VmDefCVars1228.Archive | VmDefCVars1228.SystemOnly);
        Add("temp", 0);
        Add("in_mouse", VmDefCVars1228.Archive);

        Run(lines, eng, 10,
            L(1, "version", "219", "GAMMA", "9", "fullscreen", "on", "SAVE_DIR", "secret", "cd_drive", "leftover", "vid_gamma", "5"),
            L(2, "version", "220", "mouse_sensitivity", "3", "vid_gamma", "1"),
            L(3, "version", "221", "mouse_sensitivity", "4", "m_noprescale", "1", "save_dir", "zz", "custom_sys", "aa", "temp", "bb", "nope", "cc", "set", "vid_gamma", "7", "in_mouse", "qq"),
            L(4, "version", "234", "CD_DRIVE", "x", "gamma", "3"));
        State(lines, eng);
        lines.Add("---- edge");
        Run(lines, eng, 4, L(3, "version", "234", "gamma", "EDGE"), L(4, "version", "234", "gamma", "WAD"));
        State(lines, eng);
        Run(lines, eng, 10, L(8, "version", "235", "gamma", "NO"), L(9, "version", "234", "gamma", "LATER"));
        Run(lines, eng, 10, L(1, "version", "218"));
        Run(lines, eng, 10, L(1, "nope", "1"));
        Run(lines, eng, 10, L(1));
        Run(lines, eng, 10, L(1, "version", "08"));
        Run(lines, eng, 10, L(1, "version", "0xEA", "gamma", "HEX"));
        Run(lines, eng, 10, L(1, "version", "010"));
        Run(lines, eng, 10, L(1, "version", "MAXINT"));
        Run(lines, eng, 10, L(1, "Version", "+219", "SET", "vid_gamma", "plus"));
        Run(lines, eng, 10, L(1, "version", "-1"));
        Run(lines, eng, 10, L(1, "version", "234", "vid_gamma"));
        Run(lines, eng, 10, L(1, "version"));
        Run(lines, eng, 10, L(1, "version", "219", "CD_DRIVE", "y", "gamma", "1"));
        Run(lines, eng, 10, L(1, "version", "220", "CD_DRIVE", "z", "gamma", "8"));
        Run(lines, eng, 4, L(9, "version", "234", "gamma", "SKIP"), L(1, "version", "234", "gamma", "ALSO"));
        State(lines, eng);
        return string.Join("\n", lines) + "\n";
    }
}
