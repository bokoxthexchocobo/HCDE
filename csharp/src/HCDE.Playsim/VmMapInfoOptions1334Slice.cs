using System.Globalization;
using System.Text;

namespace HCDE.Playsim;

/// <summary>
/// MAPINFO redirect, exit, light, fog, and shadow options from
/// <c>g_mapinfo.cpp</c> (1491-1826). Chunk 1334. ParseNextMap,
/// CheckEndSequence, the exit-text writers, V_GetColor, and
/// NormalizeHcdeShadowQualityCap stay callees fed by this world.
/// The notexturefill body starts at line 1827.
/// </summary>
public sealed class VmMapInfoOptions1334
{
    public const int FmtOld = 0;
    public const int FmtNew = 1;
    public const int X11NoFiles = 0;
    public const int X11Missing = 1;
    public const int X11Present = 2;
    public const int LightNotSet = -1;
    public const int BlendClamp = 0;
    public const int BlendColor = 1;
    public const int BlendNoClamp = 2;
    public const int ToneNone = 0;
    public const int ToneUncharted2 = 1;
    public const int ToneHejlDawson = 2;
    public const int ToneReinhard = 3;
    public const int ToneLinear = 4;
    public const int DefText = 1;
    public const int DefMusic = 2;
    public const int DefBackdrop = 4;
    public const int DefLookup = 8;
    public const int DefPic = 16;
    public const int EndPic = 0;
    public const int EndBunny = 1;
    public const int EndCast = 2;
    public const int EndDemon = 3;
    public const int ScrollLeft = 0;
    public const int ScrollUp = 2;

    public readonly record struct OptionSpec(string Name, bool Old, bool MacroInBand);

    public static readonly OptionSpec[] Catalog =
    {
        new("redirect", true, false),
        new("cvar_redirect", true, true),
        new("sndseq", true, true),
        new("sndinfo", true, true),
        new("soundinfo", true, true),
        new("translator", true, true),
        new("deathsequence", false, true),
        new("slideshow", false, true),
        new("bordertexture", true, true),
        new("f1", true, true),
        new("teamdamage", true, true),
        new("mapbackground", true, true),
        new("exittext", false, true),
        new("textmusic", false, true),
        new("textflat", false, true),
        new("textpic", false, true),
        new("defaultenvironment", false, true),
        new("hazardcolor", true, true),
        new("hazardflash", true, true),
        new("fogdensity", false, true),
        new("outsidefogdensity", false, true),
        new("skyfog", false, true),
        new("thickfogdistance", false, true),
        new("thickfogmultiplier", false, true),
        new("pixelratio", false, true),
        new("brightfog", false, true),
        new("lightmode", false, true),
        new("lightblendmode", false, true),
        new("hcde_shadowprofile", false, true),
        new("hcde_shadowqualitycap", false, true),
        new("hcde_shadowmaxlightscap", false, true),
        new("notexturefill", false, true),
    };

    public int Format = FmtNew;
    public bool HexenHack;
    public bool NoOctals;
    public bool TrailingTextCommaCompat;
    public bool ParseError;
    public int X11Mode = X11Present;
    public string X11Text = "";
    public int Number;
    public long BigNumber;
    public double Float;
    public string Token => current;
    public bool HasPending => alreadyGot || tokens.Count > 0;
    public string Pending => alreadyGot ? current : tokens.Peek();

    public string RedirectType = "";
    public string RedirectMapName = "";
    public string RedirectCVar = "";
    public string RedirectCVarMap = "";
    public string SndSeq = "";
    public string SoundInfo = "";
    public string Translator = "";
    public string DeathSequence = "";
    public string Slideshow = "";
    public string BorderTexture = "";
    public string F1Pic = "";
    public string MapBackground = "";
    public double TeamDamage;
    public int DefaultEnvironment;
    public int HazardColor;
    public int HazardFlash;
    public int FogDensity;
    public int OutsideFogDensity;
    public int SkyFog;
    public float ThickFogDistance;
    public float ThickFogMultiplier;
    public float PixelStretch;
    public sbyte BrightFog;
    public int LightMode;
    public int LightBlendMode;
    public int Tonemap;
    public int HcdeShadowProfile;
    public int HcdeShadowQualityCap;
    public int HcdeShadowMaxLights;
    public string LastPic = "";

    public readonly List<string> Messages = new();
    public readonly List<string> Warnings = new();
    public readonly List<string> Prints = new();
    public readonly Dictionary<string, int> Environments = new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, Finale> Finales = new(StringComparer.OrdinalIgnoreCase);
    public Finale? LastFinale;

    readonly Dictionary<string, Action> handlers;
    readonly Queue<string> tokens = new();
    readonly Dictionary<string, int> names = new(StringComparer.OrdinalIgnoreCase);
    readonly List<string> nameOrder = new();
    readonly Dictionary<int, ExitDef> exits = new();
    bool alreadyGot;
    string current = "";
    int generated;

    public sealed class ExitDef
    {
        public string Map = "";
        public string Text = "";
        public string Music = "";
        public string Backdrop = "";
        public int Order;
        public int Defined;
    }

    public sealed class Finale
    {
        public string Name = "";
        public int EndType = -1;
        public string Pic = "";
        public string Pic2 = "";
        public string Music = "";
        public bool MusicLooping = true;
        public bool PlayTheEnd;
        public string Link = "";
        public int Duration;
        public int ScrollDir = -1;
        public int ScrollDelay;
        public int ScrollTime;
    }

    public VmMapInfoOptions1334()
    {
        handlers = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase)
        {
            ["redirect"] = ApplyRedirect,
            ["cvar_redirect"] = ApplyCvarRedirect,
            ["sndseq"] = ApplySndSeq,
            ["sndinfo"] = ApplySndInfo,
            ["soundinfo"] = ApplySoundInfo,
            ["translator"] = ApplyTranslator,
            ["deathsequence"] = ApplyDeathSequence,
            ["slideshow"] = ApplySlideshow,
            ["bordertexture"] = ApplyBorder,
            ["f1"] = ApplyF1,
            ["teamdamage"] = ApplyTeamDamage,
            ["mapbackground"] = ApplyMapBackground,
            ["exittext"] = ApplyExitText,
            ["textmusic"] = ApplyTextMusic,
            ["textflat"] = ApplyTextFlat,
            ["textpic"] = ApplyTextPic,
            ["defaultenvironment"] = ApplyDefaultEnvironment,
            ["hazardcolor"] = ApplyHazardColor,
            ["hazardflash"] = ApplyHazardFlash,
            ["fogdensity"] = ApplyFogDensity,
            ["outsidefogdensity"] = ApplyOutsideFog,
            ["skyfog"] = ApplySkyFog,
            ["thickfogdistance"] = ApplyThickDistance,
            ["thickfogmultiplier"] = ApplyThickMultiplier,
            ["pixelratio"] = ApplyPixelRatio,
            ["brightfog"] = ApplyBrightFog,
            ["lightmode"] = ApplyLightMode,
            ["lightblendmode"] = ApplyLightBlend,
            ["hcde_shadowprofile"] = ApplyShadowProfile,
            ["hcde_shadowqualitycap"] = ApplyShadowQuality,
            ["hcde_shadowmaxlightscap"] = ApplyShadowMaxLights,
        };
    }

    public void Push(params string[] more)
    {
        foreach (string token in more)
            tokens.Enqueue(token);
    }

    public void Apply(string option)
    {
        if (option.Equals("notexturefill", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("notexturefill body starts at line 1827");
        if (!handlers.TryGetValue(option, out Action? act) || act == null)
            throw new InvalidOperationException("MAPINFO option is outside this band");
        act();
    }

    public string NameOf(int index) => index == 0 ? "" : nameOrder[index - 1];

    public ExitDef? FindExit(string map)
    {
        if (!names.TryGetValue(map, out int id))
            return null;
        exits.TryGetValue(id, out ExitDef? def);
        return def;
    }

    public static int NormalizeCap(int value)
    {
        if (value >= 8192) return 8192;
        if (value >= 4096) return 4096;
        if (value >= 2048) return 2048;
        if (value >= 1024) return 1024;
        if (value >= 512) return 512;
        if (value >= 256) return 256;
        return 128;
    }

    void ApplyRedirect()
    {
        ParseAssign();
        MustGetString();
        RedirectType = current;
        ParseComma();
        ParseNextMap(value => RedirectMapName = value);
    }

    void ApplyCvarRedirect()
    {
        ParseAssign();
        MustGetString();
        RedirectCVar = current;
        ParseComma();
        ParseNextMap(value => RedirectCVarMap = value);
    }

    void ApplySndSeq() => SndSeq = ReadAssignString();

    void ApplySndInfo() => SoundInfo = ReadAssignString();

    void ApplySoundInfo() => SoundInfo = ReadAssignString();

    void ApplyTranslator() => Translator = ReadAssignString();

    void ApplyDeathSequence() => DeathSequence = ReadAssignString();

    void ApplySlideshow() => Slideshow = ReadAssignString();

    void ApplyBorder()
    {
        ParseAssign();
        BorderTexture = ParseLumpOrTextureName();
    }

    void ApplyF1()
    {
        ParseAssign();
        F1Pic = ParseLumpOrTextureName();
    }

    void ApplyMapBackground()
    {
        ParseAssign();
        MapBackground = ParseLumpOrTextureName();
    }

    void ApplyTeamDamage()
    {
        ParseAssign();
        MustGetFloat();
        TeamDamage = Float;
    }

    void ApplyExitText()
    {
        ParseAssign();
        MustGetString();
        string map = current;
        ParseComma();
        ParseExitText(map);
    }

    void ApplyTextMusic()
    {
        ParseAssign();
        MustGetString();
        string map = current;
        ParseComma();
        ParseExitMusic(map);
    }

    void ApplyTextFlat()
    {
        ParseAssign();
        MustGetString();
        string map = current;
        ParseComma();
        ParseExitBackdrop(map, false);
    }

    void ApplyTextPic()
    {
        ParseAssign();
        MustGetString();
        string map = current;
        ParseComma();
        ParseExitBackdrop(map, true);
    }

    void ApplyDefaultEnvironment()
    {
        int id;
        ParseAssign();
        if (CheckNumber())
        {
            id = Number << 8;
            if (ParserCheckNumber())
                id |= Number;
        }
        else
        {
            MustGetString();
            if (!Environments.TryGetValue(current, out int found))
            {
                ScriptMessage($"Unknown sound environment '{current}'\n");
                id = 0;
            }
            else
            {
                id = found;
            }
        }
        DefaultEnvironment = id;
    }

    void ApplyHazardColor()
    {
        ParseAssign();
        MustGetString();
        HazardColor = GetColor(current);
    }

    void ApplyHazardFlash()
    {
        ParseAssign();
        MustGetString();
        HazardFlash = GetColor(current);
    }

    void ApplyFogDensity()
    {
        ParseAssign();
        MustGetNumber();
        FogDensity = Clamp(Number, 0, 512) >> 1;
    }

    void ApplyOutsideFog()
    {
        ParseAssign();
        MustGetNumber();
        OutsideFogDensity = Clamp(Number, 0, 512) >> 1;
    }

    void ApplySkyFog()
    {
        ParseAssign();
        MustGetNumber();
        SkyFog = Number;
    }

    void ApplyThickDistance()
    {
        ParseAssign();
        MustGetFloat();
        ThickFogDistance = (float)Float;
    }

    void ApplyThickMultiplier()
    {
        ParseAssign();
        MustGetFloat();
        float multiplier = (float)Float;
        if (multiplier > 0f)
            ThickFogMultiplier = multiplier;
    }

    void ApplyPixelRatio()
    {
        ParseAssign();
        MustGetFloat();
        PixelStretch = (float)Float;
    }

    void ApplyBrightFog()
    {
        ParseAssign();
        MustGetNumber();
        BrightFog = (sbyte)Number;
    }

    void ApplyLightMode()
    {
        ParseAssign();
        MustGetNumber();
        if (Number == 8 || Number == 16)
            LightMode = LightNotSet;
        else if (Number >= 0 && Number < 5)
            LightMode = Number;
        else
            ScriptMessage($"Invalid light mode {Number}");
    }

    void ApplyLightBlend()
    {
        ParseAssign();
        MustGetString();
        if (Compare("Default") || Compare("Clamp"))
        {
            LightBlendMode = BlendClamp;
        }
        else if (Compare("ColoredClamp"))
        {
            LightBlendMode = BlendColor;
        }
        else if (Compare("Unclamped"))
        {
            LightBlendMode = BlendNoClamp;
            if (CheckString(","))
            {
                MustGetString();
                if (Compare("None"))
                    Tonemap = ToneNone;
                else if (Compare("Linear"))
                    Tonemap = ToneLinear;
                else if (Compare("Uncharted2"))
                    Tonemap = ToneUncharted2;
                else if (Compare("HejlDawson"))
                    Tonemap = ToneHejlDawson;
                else if (Compare("Reinhard"))
                    Tonemap = ToneReinhard;
                else
                    ScriptMessage($"Invalid tonemap {current}");
            }
        }
        else
        {
            ScriptMessage($"Invalid light blend mode {current}");
        }
    }

    void ApplyShadowProfile()
    {
        ParseAssign();
        MustGetNumber();
        int profile = Number;
        if (profile < -1)
            profile = -1;
        else if (profile > 7)
            profile = 7;
        HcdeShadowProfile = profile;
    }

    void ApplyShadowQuality()
    {
        ParseAssign();
        MustGetNumber();
        HcdeShadowQualityCap = Number < 0 ? -1 : NormalizeCap(Number);
    }

    void ApplyShadowMaxLights()
    {
        ParseAssign();
        MustGetNumber();
        if (Number < 0)
            HcdeShadowMaxLights = -1;
        else if (Number > 1024)
            HcdeShadowMaxLights = 1024;
        else
            HcdeShadowMaxLights = Number;
    }

    string ReadAssignString()
    {
        ParseAssign();
        MustGetString();
        return current;
    }

    string ParseLumpOrTextureName()
    {
        MustGetString();
        return current;
    }

    void ParseAssign()
    {
        if (Format == FmtNew)
            MustGetStringName("=");
    }

    void ParseComma()
    {
        if (Format == FmtNew)
            MustGetStringName(",");
    }

    bool ParserCheckNumber()
    {
        if (Format == FmtNew)
        {
            if (CheckString(","))
            {
                MustGetNumber();
                return true;
            }
            return false;
        }
        return CheckNumber();
    }

    void ParseNextMap(Action<string> set)
    {
        if (CheckNumber())
        {
            set((HexenHack ? "&wt@" : "MAP") + TwoDigits(Number));
            return;
        }
        MustGetString();
        set(current);
        int seq = CheckEndSequence();
        if (seq != 0)
            set(EndSeqName(seq));
    }

    int CheckEndSequence()
    {
        if (Compare("endgame"))
        {
            if (!CheckString("{"))
            {
                UnGet();
                return Intern("Inter_Pic3");
            }
            return ParseEndGame();
        }
        if (current.Length >= 7 && current.AsSpan(0, 7).Equals("EndGame", StringComparison.OrdinalIgnoreCase))
        {
            char kind = current.Length > 7 ? current[7] : '\0';
            string seq = kind switch
            {
                '1' => "Inter_Pic1",
                '2' => "Inter_Pic2",
                '3' => "Inter_Bunny",
                'C' => "Inter_Cast",
                'W' => "Inter_Underwater",
                'S' => "Inter_Strife",
                _ => "Inter_Pic3",
            };
            return Intern(seq);
        }
        if (Compare("endpic"))
        {
            ParseComma();
            MustGetString();
            LastPic = current;
            return Intern("@EndPic_" + current);
        }
        if (Compare("endbunny")) return Intern("Inter_Bunny");
        if (Compare("endcast")) return Intern("Inter_Cast");
        if (Compare("enddemon")) return Intern("Inter_Demonscroll");
        if (Compare("endchess")) return Intern("Inter_Chess");
        if (Compare("endunderwater")) return Intern("Inter_Underwater");
        if (Compare("endbuystrife")) return Intern("Inter_BuyStrife");
        if (Compare("endtitle")) return Intern("Inter_Titlescreen");
        if (Compare("endsequence"))
        {
            ParseComma();
            MustGetString();
            return Intern(current);
        }
        return 0;
    }

    int ParseEndGame()
    {
        var fin = new Finale();
        while (!CheckString("}"))
        {
            MustGetString();
            if (Compare("pic"))
            {
                ParseAssign();
                MustGetString();
                fin.EndType = EndPic;
                fin.Pic = current;
            }
            else if (Compare("hscroll"))
            {
                ParseAssign();
                fin.EndType = EndBunny;
                MustGetString();
                fin.Pic = current;
                ParseComma();
                MustGetString();
                fin.Pic2 = current;
                if (ParserCheckNumber())
                    fin.PlayTheEnd = Number != 0;
            }
            else if (Compare("vscroll"))
            {
                ParseAssign();
                fin.EndType = EndDemon;
                MustGetString();
                fin.Pic = current;
                ParseComma();
                MustGetString();
                fin.Pic2 = current;
            }
            else if (Compare("cast"))
            {
                fin.EndType = EndCast;
                if (fin.Pic.Length == 0)
                    fin.Pic = "$bgcastcall";
            }
            else if (Compare("music"))
            {
                ParseAssign();
                MustGetString();
                fin.Music = current;
                if (ParserCheckNumber())
                    fin.MusicLooping = Number != 0;
            }
            else if (Format == FmtNew)
            {
                ScriptMessage($"Unknown property '{current}' found in endgame definition\n");
                SkipToNext();
            }
            else
            {
                ScriptError($"Unknown property '{current}' found in endgame definition\n");
            }
        }
        if (fin.EndType < 0)
            ScriptError("Endgame type was not defined");
        if (fin.EndType == EndBunny)
        {
            fin.ScrollDir = ScrollLeft;
            fin.ScrollDelay = 230;
            fin.ScrollTime = 640;
            fin.Duration = 1130;
            if (fin.PlayTheEnd)
                fin.Link = "TheEnd";
        }
        else if (fin.EndType == EndDemon)
        {
            fin.ScrollDir = ScrollUp;
            fin.ScrollDelay = 70;
            fin.ScrollTime = 600;
        }
        else if (fin.EndType == EndCast)
        {
            fin.Duration = 1;
            fin.Link = "Doom2Cast";
        }
        string seq = $"@EndSequence_{generated}_";
        generated++;
        fin.Name = seq;
        Finales[seq] = fin;
        LastFinale = fin;
        return Intern(seq);
    }

    void SkipToNext()
    {
        if (!CheckString("="))
            return;
        do
        {
            MustGetString();
        }
        while (CheckString(","));
    }

    void ParseExitText(string map)
    {
        (string text, bool lookup) = ParseLookupName();
        ExitDef def = Slot(map, out bool created);
        def.Text = text;
        if (lookup)
            def.Defined |= DefLookup;
        else if (!created)
            def.Defined &= ~DefLookup;
        def.Defined |= DefText;
    }

    void ParseExitMusic(string map)
    {
        (string music, int order) = ParseMusic();
        ExitDef def = Slot(map, out _);
        def.Music = music;
        def.Order = order;
        def.Defined |= DefMusic;
    }

    void ParseExitBackdrop(string map, bool ispic)
    {
        string drop = ParseLumpOrTextureName();
        ExitDef def = Slot(map, out bool created);
        def.Backdrop = drop;
        def.Defined |= DefBackdrop;
        if (ispic)
            def.Defined |= DefPic;
        else if (!created)
            def.Defined &= ~DefPic;
        if (created)
            def.Defined |= DefMusic;
    }

    (string Text, bool Lookup) ParseLookupName()
    {
        MustGetString();
        if (Compare("lookup"))
        {
            ParseComma();
            MustGetString();
            return (current, true);
        }
        if (current.Length > 0 && current[0] == '$')
            return (current.Substring(1), true);
        if (Format == FmtOld)
            return (current, false);
        UnGet();
        var built = new StringBuilder();
        bool reported = false;
        for (;;)
        {
            MustGetString();
            built.Append(current);
            built.Append('\n');
            if (!CheckString(","))
                break;
            if (!TrailingTextCommaCompat)
                continue;
            if (!GetString())
                break;
            if (Compare("}"))
            {
                UnGet();
                if (!reported)
                {
                    Prints.Add("HCDE: tolerated trailing comma in MAPINFO text list for known mod compatibility.\n");
                    reported = true;
                }
                break;
            }
            UnGet();
        }
        if (built.Length > 0)
            built.Length -= 1;
        return (built.ToString(), false);
    }

    (string Name, int Order) ParseMusic()
    {
        MustGetString();
        int order = 0;
        string name = current;
        int colon = current.IndexOf(':');
        if (colon >= 0)
        {
            order = Atoi(current.Substring(colon + 1));
            name = current.Substring(0, colon);
        }
        else if (ParserCheckNumber())
        {
            order = Number;
        }
        return (name, order);
    }

    ExitDef Slot(string map, out bool created)
    {
        int id = Intern(map);
        if (exits.TryGetValue(id, out ExitDef? def))
        {
            created = false;
            return def;
        }
        created = true;
        def = new ExitDef { Map = NameOf(id) };
        exits[id] = def;
        return def;
    }

    int Intern(string text)
    {
        if (names.TryGetValue(text, out int id))
            return id;
        int next = nameOrder.Count + 1;
        names[text] = next;
        nameOrder.Add(text);
        return next;
    }

    int GetColor(string text)
    {
        string named = ColorByName(text);
        return ColorFromString(named.Length > 0 ? named : text);
    }

    string ColorByName(string name)
    {
        if (X11Mode == X11NoFiles)
            return "";
        if (X11Mode == X11Missing)
        {
            Warnings.Add("X11R6RGB lump not found");
            return "";
        }
        string rgb = X11Text;
        int i = 0;
        int n = rgb.Length;
        int step = 0;
        int cr = 0;
        int cg = 0;
        int cb = 0;
        while (i < n)
        {
            if (rgb[i] <= ' ')
            {
                do
                {
                    i++;
                }
                while (i < n && rgb[i] <= ' ');
            }
            else if (step == 0 && rgb[i] == '!')
            {
                do
                {
                    i++;
                }
                while (i < n && rgb[i] != '\n');
            }
            else if (step < 3)
            {
                if (!TakeUInt(rgb, ref i, out int val))
                {
                    if (i < n)
                        Warnings.Add("X11R6RGB lump is corrupt");
                    return "";
                }
                if (step == 0) cr = val;
                else if (step == 1) cg = val;
                else cb = val;
                step++;
            }
            else
            {
                int end = i;
                while (end < n && rgb[end] != '\n')
                    end++;
                int trim = end;
                while (trim > i && rgb[trim - 1] <= ' ')
                    trim--;
                if (trim == i)
                {
                    Warnings.Add("X11R6RGB lump is corrupt");
                    return "";
                }
                string found = rgb.Substring(i, trim - i);
                if (found.Length == name.Length && found.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return $"#{cr:x2}{cg:x2}{cb:x2}";
                i = end;
                step = 0;
            }
        }
        return "";
    }

    int ColorFromString(string cstr)
    {
        if (cstr.Length > 0 && cstr[0] == '#')
        {
            if (cstr.Length == 7)
            {
                int r = ParseHex(cstr.Substring(1, 2));
                int g = ParseHex(cstr.Substring(3, 2));
                int b = ParseHex(cstr.Substring(5, 2));
                return (r << 16) | (g << 8) | b;
            }
            if (cstr.Length == 4)
            {
                int r = ParseHex(new string(cstr[1], 2));
                int g = ParseHex(new string(cstr[2], 2));
                int b = ParseHex(new string(cstr[3], 2));
                return (r << 16) | (g << 8) | b;
            }
            return 0;
        }
        if (cstr.Length == 6 && AllHex(cstr))
        {
            int color = 0;
            foreach (char ch in cstr)
                color = (color << 4) | Digit(ch, 16);
            return color;
        }
        int i = 0;
        int packed = 0;
        for (int comp = 0; comp < 3; comp++)
        {
            while (i < cstr.Length && cstr[i] <= ' ')
                i++;
            int start = i;
            while (i < cstr.Length && cstr[i] > ' ')
                i++;
            int len = i - start;
            int part = 0;
            if (len > 0)
            {
                int take = len == 1 ? 1 : 2;
                string val = cstr.Substring(start, take);
                if (take == 1)
                    val = new string(val[0], 2);
                part = ParseHex(val);
            }
            packed |= part << ((2 - comp) * 8);
        }
        return packed;
    }

    int ParseHex(string hex)
    {
        int num = 0;
        foreach (char ch in hex)
        {
            num <<= 4;
            int d = Digit(ch, 16);
            if (d < 0)
            {
                Warnings.Add($"Bad hex number: {hex}");
                return 0;
            }
            num += d;
        }
        return num;
    }

    bool GetString()
    {
        if (alreadyGot)
        {
            alreadyGot = false;
            return true;
        }
        if (tokens.Count == 0)
            return false;
        current = tokens.Dequeue();
        return true;
    }

    void UnGet() => alreadyGot = true;

    bool Compare(string text) => current.Equals(text, StringComparison.OrdinalIgnoreCase);

    void MustGetString()
    {
        if (!GetString())
            ScriptError("Missing string (unexpected end of file).");
    }

    void MustGetStringName(string name)
    {
        MustGetString();
        if (!Compare(name))
            ScriptError($"Expected '{name}', got '{current}'.");
    }

    bool CheckString(string name)
    {
        if (!GetString())
            return false;
        if (Compare(name))
            return true;
        UnGet();
        return false;
    }

    bool GetNumber()
    {
        if (!GetString())
            return false;
        if (current == "MAXINT")
        {
            Number = int.MaxValue;
            BigNumber = int.MaxValue;
            Float = Number;
            return true;
        }
        if (!WholeLong(current, out long big))
        {
            ScriptError($"SC_GetNumber: Bad numeric constant \"{current}\".");
            return false;
        }
        BigNumber = big;
        Number = (int)big;
        Float = Number;
        return true;
    }

    bool CheckNumber()
    {
        if (!GetString())
            return false;
        if (current.Length == 0)
        {
            UnGet();
            return false;
        }
        if (current == "MAXINT")
        {
            BigNumber = long.MaxValue;
            Number = int.MaxValue;
            Float = Number;
            return true;
        }
        if (!WholeLong(current, out long big))
        {
            UnGet();
            return false;
        }
        BigNumber = big;
        Number = (int)big;
        Float = Number;
        return true;
    }

    void MustGetNumber()
    {
        if (!GetNumber())
            ScriptError("Missing integer (unexpected end of file).");
    }

    bool GetFloat()
    {
        if (!GetString())
            return false;
        if (!WholeFloat(current, out double value))
        {
            ScriptError($"SC_GetFloat: Bad numeric constant \"{current}\".");
            return false;
        }
        Float = value;
        Number = Narrow(value);
        return true;
    }

    void MustGetFloat()
    {
        if (!GetFloat())
            ScriptError("Missing floating-point number (unexpected end of file).");
    }

    bool WholeLong(string text, out long value)
    {
        value = 0;
        if (text.Length == 0)
            return true;
        int i = 0;
        int n = text.Length;
        while (i < n && IsCSpace(text[i]))
            i++;
        if (i == n)
            return false;
        int sign = 1;
        if (text[i] == '+' || text[i] == '-')
        {
            if (text[i] == '-')
                sign = -1;
            i++;
        }
        int radix = 10;
        if (i < n && text[i] == '0')
        {
            if (i + 1 < n && (text[i + 1] == 'x' || text[i + 1] == 'X'))
            {
                radix = 16;
                i += 2;
            }
            else if (!NoOctals)
            {
                radix = 8;
            }
        }
        bool any = false;
        bool overflow = false;
        ulong mag = 0;
        while (i < n)
        {
            int d = Digit(text[i], radix);
            if (d < 0)
                break;
            any = true;
            if (overflow || mag > (ulong.MaxValue - (ulong)d) / (ulong)radix)
                overflow = true;
            else
                mag = mag * (ulong)radix + (ulong)d;
            i++;
        }
        if (!any || i != n)
            return false;
        if (overflow)
        {
            value = sign < 0 ? long.MinValue : long.MaxValue;
            return true;
        }
        if (sign < 0)
        {
            if (mag >= (ulong)long.MaxValue + 1UL)
                value = long.MinValue;
            else
                value = -(long)mag;
            return true;
        }
        value = mag > (ulong)long.MaxValue ? long.MaxValue : (long)mag;
        return true;
    }

    bool WholeFloat(string text, out double value)
    {
        value = 0;
        if (text.Length == 0)
            return true;
        int i = 0;
        int n = text.Length;
        while (i < n && IsCSpace(text[i]))
            i++;
        if (i == n)
            return false;
        int sign = 1;
        if (text[i] == '+' || text[i] == '-')
        {
            if (text[i] == '-')
                sign = -1;
            i++;
            if (i == n)
                return false;
        }
        if (Starts(text, i, "infinity"))
        {
            i += 8;
            if (i != n)
                return false;
            value = sign < 0 ? double.NegativeInfinity : double.PositiveInfinity;
            return true;
        }
        if (Starts(text, i, "inf"))
        {
            i += 3;
            if (i != n)
                return false;
            value = sign < 0 ? double.NegativeInfinity : double.PositiveInfinity;
            return true;
        }
        if (Starts(text, i, "nan"))
        {
            i += 3;
            if (i != n)
                return false;
            long bits = BitConverter.DoubleToInt64Bits(double.NaN);
            if (sign < 0)
                bits |= long.MinValue;
            value = BitConverter.Int64BitsToDouble(bits);
            return true;
        }
        if (i + 1 < n && text[i] == '0' && (text[i + 1] == 'x' || text[i + 1] == 'X'))
            return HexFloat(text, i + 2, sign, out value);
        if (!DecimalFloat(text, ref i, out double mag) || i != n)
            return false;
        value = sign < 0 ? -mag : mag;
        return true;
    }

    static bool HexFloat(string text, int i, int sign, out double value)
    {
        value = 0;
        int n = text.Length;
        double mag = 0;
        bool any = false;
        while (i < n)
        {
            int d = Digit(text[i], 16);
            if (d < 0)
                break;
            any = true;
            mag = mag * 16 + d;
            i++;
        }
        if (i < n && text[i] == '.')
        {
            i++;
            double scale = 1;
            while (i < n)
            {
                int d = Digit(text[i], 16);
                if (d < 0)
                    break;
                any = true;
                scale /= 16d;
                mag += d * scale;
                i++;
            }
        }
        if (!any)
            return false;
        if (i < n && (text[i] == 'p' || text[i] == 'P'))
        {
            int j = i + 1;
            int expSign = 1;
            if (j < n && (text[j] == '+' || text[j] == '-'))
            {
                if (text[j] == '-')
                    expSign = -1;
                j++;
            }
            int expStart = j;
            long exp = 0;
            while (j < n && text[j] >= '0' && text[j] <= '9')
            {
                exp = exp * 10 + (text[j] - '0');
                j++;
            }
            if (j == expStart)
                return false;
            i = j;
            if (exp > 100000)
                mag = expSign < 0 ? 0d : double.PositiveInfinity;
            else
                mag = Math.ScaleB(mag, expSign * (int)exp);
        }
        if (i != n)
            return false;
        value = sign < 0 ? -mag : mag;
        return true;
    }

    static bool DecimalFloat(string text, ref int i, out double value)
    {
        int n = text.Length;
        int start = i;
        bool digit = false;
        while (i < n && text[i] >= '0' && text[i] <= '9')
        {
            digit = true;
            i++;
        }
        if (i < n && text[i] == '.')
        {
            i++;
            while (i < n && text[i] >= '0' && text[i] <= '9')
            {
                digit = true;
                i++;
            }
        }
        if (!digit)
        {
            value = 0;
            return false;
        }
        int end = i;
        if (i < n && (text[i] == 'e' || text[i] == 'E'))
        {
            int j = i + 1;
            if (j < n && (text[j] == '+' || text[j] == '-'))
                j++;
            int expStart = j;
            while (j < n && text[j] >= '0' && text[j] <= '9')
                j++;
            if (j > expStart)
            {
                end = j;
                i = j;
            }
        }
        string slice = text.Substring(start, end - start);
        return double.TryParse(slice, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    void ScriptMessage(string message)
    {
        ParseError = true;
        Messages.Add(message);
    }

    void ScriptError(string message)
    {
        Messages.Add(message);
        throw new InvalidOperationException(message);
    }

    static int Clamp(int value, int lo, int hi)
    {
        if (value < lo)
            return lo;
        if (value > hi)
            return hi;
        return value;
    }

    static int Atoi(string text)
    {
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
        int value = 0;
        while (i < text.Length && text[i] >= '0' && text[i] <= '9')
        {
            value = value * 10 + (text[i] - '0');
            i++;
        }
        return sign < 0 ? -value : value;
    }

    static bool TakeUInt(string text, ref int i, out int value)
    {
        int start = i;
        long mag = 0;
        if (i >= text.Length || text[i] < '0' || text[i] > '9')
        {
            value = 0;
            return false;
        }
        while (i < text.Length && text[i] >= '0' && text[i] <= '9')
        {
            mag = mag * 10 + (text[i] - '0');
            i++;
        }
        value = (int)mag;
        return i != start;
    }

    static bool AllHex(string text)
    {
        foreach (char ch in text)
        {
            if (Digit(ch, 16) < 0)
                return false;
        }
        return true;
    }

    static int Narrow(double value)
    {
        if (double.IsNaN(value))
            return 0;
        if (value >= int.MaxValue)
            return int.MaxValue;
        if (value <= int.MinValue)
            return int.MinValue;
        return (int)value;
    }

    static string TwoDigits(int n)
    {
        string text = n.ToString(CultureInfo.InvariantCulture);
        if (n >= 0 && text.Length < 2)
            return text.PadLeft(2, '0');
        return text;
    }

    static string EndSeqName(int index) => "enDSeQ" + index.ToString("x4", CultureInfo.InvariantCulture);

    static bool Starts(string text, int i, string word)
    {
        if (i + word.Length > text.Length)
            return false;
        return text.AsSpan(i, word.Length).Equals(word, StringComparison.OrdinalIgnoreCase);
    }

    static bool IsCSpace(char c) => c == ' ' || c == '\t' || c == '\n' || c == '\v' || c == '\f' || c == '\r';

    static int Digit(char c, int radix)
    {
        int d = c >= '0' && c <= '9'
            ? c - '0'
            : c >= 'a' && c <= 'z'
                ? c - 'a' + 10
                : c >= 'A' && c <= 'Z'
                    ? c - 'A' + 10
                    : -1;
        if (d < 0 || d >= radix)
            return -1;
        return d;
    }
}
