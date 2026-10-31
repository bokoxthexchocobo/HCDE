using System.Globalization;
using System.Text;

namespace HCDE.Playsim;

/// <summary>
/// Startup flags, language options, and the settings layout from <c>settingspage.cpp</c>
/// (1-296). Chunk 1288. String lookup, file search, and text measurement are host inputs.
/// </summary>
public sealed class VmSettingsInfo1288
{
    public bool Fullscreen = true;
    public bool QueryIwad = true;
    public int StartFlags;
    public int Backend = 1;
    public string Language = "auto";
    public int FileLoad;
}

public sealed class VmLanguage1288
{
    public string Iso = "";
    public string Display = "";
}

public sealed class VmSettings1288
{
    public const int RequireNone = 0;
    public const int RequireDefault = 3;
    public const int RequireAll = 7;

    static readonly (string Key, int Flag)[] FileLoadOpts =
    {
        ("OPTVAL_LAX", RequireNone),
        ("OPTVAL_DEFAULT", RequireDefault),
        ("OPTVAL_STRICT", RequireAll),
        ("OPTVAL_CUSTOM", -1),
    };

    public bool Fullscreen;
    public bool DontAsk;
    public bool Autoload;
    public bool Lights;
    public bool Brightmaps;
    public bool Widescreen;
    public bool Support;
    public bool OpenGl;
    public bool Vulkan;
    public bool HideLanguage;
    public bool DropDownBelow;
    public int MaxDisplayItems;
    public int ExtraWadFlags;
    public int LoadSelected;
    public int ScrollTo;
    public int PlayCalls;
    public int UpdateCalls;
    public int LauncherLanguageCalls;
    public string RequestedLanguage = "";
    public string LangName = "";
    public string CanvasLanguage = "";
    public string LangLabel = "";
    public string LoadLabel = "";
    public string GeneralLabel = "";
    public string ExtrasLabel = "";
    public string FullscreenText = "";
    public string AutoloadText = "";
    public string DontAskText = "";
    public string LightsText = "";
    public string BrightmapsText = "";
    public string WidescreenText = "";
    public string SupportText = "";
    public string BackendLabel = "";
    public string VulkanText = "";
    public string OpenGlText = "";
    public List<string> LoadItems = new();
    public List<VmLanguage1288> Languages = new();
    public List<string> LangItems = new();
    public List<string> Frames = new();
    public Dictionary<string, string> Strings = new();
    public double Width = 400;
    public double Height = 500;
    public double LoadLabelWidth = 40;
    public double LoadListWidth = 80;
    public int Selected;

    public void Build(VmSettingsInfo1288 info, string menu, bool lightsPk3, bool brightmapsPk3, bool widescreenPk3)
    {
        Fullscreen = info.Fullscreen;
        DontAsk = !info.QueryIwad;
        Autoload = (info.StartFlags & 1) != 0;
        Lights = (info.StartFlags & 2) != 0;
        Brightmaps = (info.StartFlags & 4) != 0;
        Widescreen = (info.StartFlags & 8) != 0;
        Support = (info.StartFlags & 16) != 0;
        OpenGl = false;
        Vulkan = false;
        if (info.Backend == 0)
            OpenGl = true;
        else
            Vulkan = true;

        HideLanguage = false;
        Languages.Clear();
        try
        {
            var scan = new VmMenuScan1288(menu);
            while (scan.GetString())
            {
                if (!scan.Compare("OptionString"))
                    continue;
                scan.MustGetString();
                if (!scan.Compare("LanguageOptions"))
                    continue;
                scan.MustGetStringName("{");
                while (!scan.CheckString("}"))
                {
                    scan.MustGetString();
                    string iso = scan.Text;
                    scan.MustGetStringName(",");
                    scan.MustGetString();
                    Languages.Add(new VmLanguage1288 { Iso = iso, Display = scan.Text });
                }
            }
        }
        catch (Exception)
        {
            HideLanguage = true;
        }

        LangItems.Clear();
        Selected = 0;
        for (int i = 0; i < Languages.Count; i++)
        {
            LangItems.Add(Languages[i].Display);
            if (VmMenuScan1288.SameText(Languages[i].Iso, info.Language))
                Selected = i;
        }

        ExtraWadFlags = 0;
        if (lightsPk3)
            ExtraWadFlags |= 1;
        if (brightmapsPk3)
            ExtraWadFlags |= 2;
        if (widescreenPk3)
            ExtraWadFlags |= 4;

        LoadItems.Clear();
        int selected = FileLoadOpts.Length - 1;
        for (int i = 0; i < FileLoadOpts.Length; i++)
        {
            LoadItems.Add(Lookup(FileLoadOpts[i].Key));
            if (info.FileLoad == FileLoadOpts[i].Flag)
                selected = i;
        }
        LoadSelected = selected;
        MaxDisplayItems = 4;
        DropDownBelow = false;
    }

    public void OnOpenGlChanged(bool on)
    {
        if (on)
            Vulkan = false;
    }

    public void OnVulkanChanged(bool on)
    {
        if (on)
            OpenGl = false;
    }

    public VmSettingsInfo1288 SetValues(VmSettingsInfo1288 info)
    {
        info.Fullscreen = Fullscreen;
        info.QueryIwad = !DontAsk;
        info.Language = Languages[Selected].Iso;
        int flags = 0;
        if (Autoload)
            flags |= 1;
        if (Lights)
            flags |= 2;
        if (Brightmaps)
            flags |= 4;
        if (Widescreen)
            flags |= 8;
        if (Support)
            flags |= 16;
        info.StartFlags = flags;
        int behaviour = FileLoadOpts[LoadSelected].Flag;
        if (behaviour != -1)
            info.FileLoad = behaviour;
        info.Backend = OpenGl ? 0 : 1;
        return info;
    }

    public void UpdateLanguage()
    {
        CanvasLanguage = LangName;
        LangLabel = Lookup("OPTMNU_LANGUAGE");
        LoadLabel = Lookup("PICKER_FILELOADING");
        GeneralLabel = Lookup("PICKER_GENERAL");
        ExtrasLabel = Lookup("PICKER_EXTRA");
        FullscreenText = Lookup("PICKER_FULLSCREEN");
        AutoloadText = Lookup("PICKER_NOAUTOLOAD");
        DontAskText = Lookup("PICKER_DONTASK");
        LightsText = Lookup("PICKER_LIGHTS");
        BrightmapsText = Lookup("PICKER_BRIGHTMAPS");
        WidescreenText = Lookup("PICKER_WIDESCREEN");
        SupportText = Lookup("PICKER_SUPPORTWADS");
        for (int i = 0; i < FileLoadOpts.Length; i++)
            LoadItems[i] = Lookup(FileLoadOpts[i].Key);
        BackendLabel = Lookup("PICKER_PREFERBACKEND");
        VulkanText = Lookup("OPTVAL_VULKAN");
        OpenGlText = Lookup("OPTVAL_OPENGL");
    }

    public void OnLanguageChanged(int index)
    {
        Selected = index;
        RequestedLanguage = Languages[index].Iso;
        UpdateLanguage();
        Layout();
        UpdateCalls++;
        LauncherLanguageCalls++;
    }

    public void Layout()
    {
        Frames.Clear();
        const double panel = 160.0;
        const double row = 20.0;
        const double loadH = 30.0;
        double y = 0.0;
        double w = Width;
        double h = Height;
        Frame("general", 0.0, y, 190.0, row);
        y += row;
        Frame("fullscreen", 0.0, y, 190.0, row);
        y += row;
        Frame("autoload", 0.0, y, 190.0, row);
        y += row;
        Frame("dontAsk", 0.0, y, 190.0, row);
        y += row;
        Frame("support", 0.0, y, 190.0, row);
        y += row;
        double optionsBottom = y;

        double x = w / 2.0 - panel / 2.0;
        y = 0.0;
        Frame("backend", x, y, 190.0, row);
        y += row;
        Frame("vulkan", x, y, 190.0, row);
        y += row;
        Frame("opengl", x, y, 190.0, row);
        y += row;
        double backendsBottom = y;

        if (ExtraWadFlags != 0)
        {
            y = 0.0;
            Frame("extras", w - panel, y, panel, row);
            y += row;
            if ((ExtraWadFlags & 1) != 0)
            {
                Frame("lights", w - panel, y, panel, row);
                y += row;
            }
            if ((ExtraWadFlags & 2) != 0)
            {
                Frame("brightmaps", w - panel, y, panel, row);
                y += row;
            }
            if ((ExtraWadFlags & 4) != 0)
            {
                Frame("widescreen", w - panel, y, panel, row);
                y += row;
            }
        }

        y = Math.Max(y, Math.Max(optionsBottom, backendsBottom)) + 10.0;
        if (!HideLanguage)
        {
            Frame("language", 0.0, y, w, row);
            y += row;
            double listH = Math.Max(h - y - loadH - 4.0, 0.0);
            Frame("langList", 0.0, y, w, listH);
            y += listH + 4.0;
        }

        Frame("loadLabel", 0.0, y + (loadH - row) / 2.0, LoadLabelWidth, row);
        Frame("loadList", LoadLabelWidth + 4.0, y, LoadListWidth, loadH);
        PlayCalls++;
        ScrollTo = Selected;
    }

    public string Lookup(string key)
    {
        return Strings.TryGetValue(key, out string? value) ? value : key;
    }

    void Frame(string name, double x, double y, double width, double height)
    {
        Frames.Add(string.Join(' ', name, Num(x), Num(y), Num(width), Num(height)));
    }

    static string Num(double value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }
}

public sealed class VmMenuScan1288
{
    readonly string _text;
    int _at;
    bool _hold;

    public VmMenuScan1288(string text)
    {
        _text = Prepare(text);
    }

    public string Text { get; private set; } = "";

    public bool GetString()
    {
        if (_hold)
        {
            _hold = false;
            return true;
        }
        while (true)
        {
            while (_at < _text.Length && _text[_at] <= ' ' && _text[_at] != '\n')
                _at++;
            if (_at >= _text.Length)
                return false;
            char c = _text[_at];
            if (c == '\n')
            {
                _at++;
                if (_at >= _text.Length)
                    return false;
                continue;
            }
            if (c == '/' && _at + 1 < _text.Length && _text[_at + 1] == '*')
            {
                if (!SkipBlock())
                    return false;
                continue;
            }
            if (StartsLineComment())
            {
                if (!SkipLine())
                    return false;
                continue;
            }
            if (c == '"')
            {
                Text = ReadString();
                return true;
            }
            Text = ReadToken();
            return true;
        }
    }

    public void MustGetString()
    {
        if (!GetString())
            throw new InvalidOperationException("Missing string (unexpected end of file).");
    }

    public void MustGetStringName(string name)
    {
        MustGetString();
        if (!Compare(name))
            throw new InvalidOperationException("Expected '" + name + "', got '" + Text + "'.");
    }

    public bool CheckString(string name)
    {
        if (!GetString())
            return false;
        if (Compare(name))
            return true;
        _hold = true;
        return false;
    }

    public bool Compare(string name)
    {
        return SameText(Text, name);
    }

    public static bool SameText(string left, string right)
    {
        if (left.Length != right.Length)
            return false;
        for (int i = 0; i < left.Length; i++)
        {
            if (Fold(left[i]) != Fold(right[i]))
                return false;
        }
        return true;
    }

    static char Fold(char c)
    {
        return c is >= 'A' and <= 'Z' ? (char)(c + 32) : c;
    }

    static string Prepare(string text)
    {
        if (text.Length >= 3 && text[0] == '\u00EF' && text[1] == '\u00BB' && text[2] == '\u00BF')
            text = text.Substring(3);
        if (text.Length == 0 || text[^1] != '\n')
        {
            if (text.Length > 0 && text[^1] == '\0')
                text = text.Substring(0, text.Length - 1) + "\n";
            else
                text += "\n";
        }
        return text;
    }

    bool StartsLineComment()
    {
        if (_text[_at] == ';' || (_at + 1 < _text.Length && _text[_at] == '/' && _text[_at + 1] == '/'))
            return true;
        return _text.AsSpan(_at).StartsWith("#region") || _text.AsSpan(_at).StartsWith("#endregion");
    }

    bool SkipLine()
    {
        while (_at < _text.Length && _text[_at] != '\n')
            _at++;
        if (_at >= _text.Length)
            return false;
        _at++;
        return _at < _text.Length;
    }

    bool SkipBlock()
    {
        _at += 2;
        while (_at < _text.Length)
        {
            if (_text[_at] == '*' && _at + 1 < _text.Length && _text[_at + 1] == '/')
            {
                _at += 2;
                return true;
            }
            _at++;
        }
        return false;
    }

    string ReadString()
    {
        _at++;
        var body = new StringBuilder();
        while (_at < _text.Length)
        {
            if (_text[_at] == '\\' && _at + 1 < _text.Length && _text[_at + 1] == '"')
            {
                body.Append('"');
                _at += 2;
                continue;
            }
            if (_text[_at] == '\r' && _at + 1 < _text.Length && _text[_at + 1] == '\n')
                _at++;
            if (_at < _text.Length && _text[_at] == '"')
                break;
            body.Append(_text[_at]);
            _at++;
        }
        if (_at < _text.Length && _text[_at] == '"')
            _at++;
        return body.ToString();
    }

    string ReadToken()
    {
        char c = _text[_at];
        if (c is '{' or '}' or '|' or '=')
        {
            _at++;
            return c.ToString();
        }
        int start = _at;
        if (IsTok2(c))
            _at++;
        else
        {
            _at++;
            return _text.Substring(start, 1);
        }
        while (_at < _text.Length && IsTok2(_text[_at]))
            _at++;
        while (_at < _text.Length && _text[_at] == '/' && _at + 1 < _text.Length && IsTok2(_text[_at + 1]) && _text[_at + 1] != '*')
        {
            _at += 2;
            while (_at < _text.Length && IsTok2(_text[_at]))
                _at++;
        }
        return _text.Substring(start, _at - start);
    }

    static bool IsTok2(char c)
    {
        if (c <= ' ')
            return false;
        return "{}|=\"/;".IndexOf(c) < 0;
    }
}
