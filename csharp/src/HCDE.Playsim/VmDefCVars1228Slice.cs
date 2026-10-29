namespace HCDE.Playsim;

/// <summary>
/// DEFCVARS loader from <c>d_defcvars.cpp</c> (<b>1–163</b>). Chunk <b>1228</b>.
/// Version 219 renames gamma and fullscreen, then stops at cd_drive.
/// Versions below 221 also stop at the old mouse cvars. Blacklisted names and
/// system-only cvars skip their value. Only archived cvars receive a default.
/// </summary>
public sealed class VmDefCVar1228
{
    public string Name = "";
    public int Flags;
    public string Default = "";
    public bool HasDefault;
}

public sealed class VmDefLump1228
{
    public int Index;
    public string[] Tokens = Array.Empty<string>();
}

public sealed class VmDefCVars1228
{
    public const int Archive = 1;
    public const int SystemOnly = 1 << 21;
    public const string LastRunVersion = "234";

    public readonly List<string> Log = new();
    public readonly List<VmDefCVar1228> CVars = new();
    public readonly List<VmDefLump1228> Lumps = new();
    public int IwadLastIndex;
    public Func<string, int?> ReadNumber = static token => null;

    static readonly string[] Blacklist =
    {
        "disablecrashlog", "gl_control_tear", "in_mouse", "joy_dinput", "joy_ps2raw", "joy_xinput",
        "k_allowfullscreentoggle", "k_mergekeys", "m_swapbuttons", "queryiwad_key", "vid_gpuswitch",
        "vr_enable_quadbuffered", "m_filter", "gl_debug", "vid_adapter", "sys_statsenabled47",
        "sys_statsenabled49", "anonstats_statsenabled411", "save_dir", "sys_statsport", "sys_statshost",
        "anonstats_port", "anonstats_host", "sentstats_hwr_done",
    };

    public void Grab()
    {
        int game = Atoi(LastRunVersion);
        foreach (VmDefLump1228 lump in Lumps)
        {
            if (lump.Index + 1 > IwadLastIndex)
            {
                Log.Add("wad \\x1cGCannot load DEFCVARS from a wadfile!");
                break;
            }

            if (!Parse(lump, game))
                return;
        }
    }

    bool Parse(VmDefLump1228 lump, int game)
    {
        string[] tokens = lump.Tokens;
        int pos = 0;
        bool Take(out string token)
        {
            if (pos >= tokens.Length)
            {
                token = "";
                return false;
            }

            token = tokens[pos++];
            return true;
        }

        Log.Add("lump " + lump.Index);
        if (!Take(out string cur))
            return Fatal("Missing string (unexpected end of file).");
        if (!Ieq(cur, "version"))
            return Fatal("Must declare version for defcvars! (currently: " + game + ")");
        if (!Take(out cur))
            return Fatal("Missing integer (unexpected end of file).");
        int? parsed = ReadNumber(cur);
        if (parsed == null)
            return Fatal("SC_GetNumber: Bad numeric constant \"" + cur + "\".");
        int lumpVersion = parsed.Value;
        if (lumpVersion > game)
            return Fatal("Unsupported version " + lumpVersion + " (" + game + " supported)");
        if (lumpVersion < 219)
            return Fatal("Version must be at least 219 (current version " + game + ")");

        while (Take(out cur))
        {
            string shown = cur;
            if (Ieq(cur, "set"))
            {
                if (!Take(out cur))
                    return Fatal("Missing string (unexpected end of file).");
                shown = cur;
            }

            string name = shown;
            if (lumpVersion < 220)
            {
                name = Lower(name);
                if (name == "gamma")
                    name = "vid_gamma";
                if (name == "fullscreen")
                    name = "vid_fullscreen";
                if (name == "cd_drive")
                    break;
            }

            if (lumpVersion < 221 && (name == "mouse_sensitivity" || name == "m_noprescale"))
                break;

            bool black = Listed(name);
            int id = Find(name);
            if (id >= 0 && (CVars[id].Flags & SystemOnly) != 0)
                black = true;
            if (black)
            {
                Log.Add("black '" + shown + "'");
                if (!Take(out cur))
                    return Fatal("Missing string (unexpected end of file).");
            }
            else if (id >= 0)
            {
                if ((CVars[id].Flags & Archive) != 0)
                {
                    if (!Take(out cur))
                        return Fatal("Missing string (unexpected end of file).");
                    CVars[id].Default = cur;
                    CVars[id].HasDefault = true;
                    Log.Add("default " + name + "=" + cur);
                }
                else
                {
                    Log.Add("nonconfig '" + shown + "'");
                    if (!Take(out cur))
                        return Fatal("Missing string (unexpected end of file).");
                }
            }
            else
            {
                Log.Add("unknown '" + shown + "'");
                if (!Take(out cur))
                    return Fatal("Missing string (unexpected end of file).");
            }
        }

        return true;
    }

    bool Fatal(string text)
    {
        Log.Add("fatal " + text);
        return false;
    }

    int Find(string name)
    {
        for (int i = 0; i < CVars.Count; i++)
            if (CVars[i].Name == name)
                return i;
        return -1;
    }

    static bool Listed(string current)
    {
        char head = current.Length == 0 ? '\0' : current[0];
        foreach (string name in Blacklist)
            if (name[0] == head && current == name)
                return true;
        return false;
    }

    static string Lower(string text)
    {
        var chars = text.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i];
            if (c >= 'A' && c <= 'Z')
                chars[i] = (char)(c + 32);
        }

        return new string(chars);
    }

    static bool Ieq(string text, string other) => text.Equals(other, StringComparison.OrdinalIgnoreCase);

    static int Atoi(string text)
    {
        int i = 0;
        while (i < text.Length && char.IsWhiteSpace(text[i]))
            i++;
        int sign = 1;
        if (i < text.Length && (text[i] == '+' || text[i] == '-'))
        {
            if (text[i] == '-')
                sign = -1;
            i++;
        }

        int value = 0;
        while (i < text.Length && text[i] >= '0' && text[i] <= '9')
        {
            value = value * 10 + (text[i] - '0');
            i++;
        }

        return sign * value;
    }

}
