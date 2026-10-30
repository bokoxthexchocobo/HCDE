using System.Globalization;
using System.Text;

namespace HCDE.Playsim;

/// <summary>
/// IWAD labels, addon joining, and the launcher layout from <c>playgamepage.cpp</c>
/// (1-243). Chunk 1287. String lookup and the file dialog are host inputs.
/// </summary>
public sealed class VmWad1287
{
    public string Name = "";
    public string Path = "";
}

public sealed class VmPlayValues1287
{
    public int Iwad = -1;
    public string Addons = "";
    public string Args = "";
    public bool SaveArgs;
}

public sealed class VmPlayGame1287
{
    public const string Separator = "|";

    public List<string> Games = new();
    public List<string> Addons = new();
    public int Selected = -1;
    public string Parameters = "";
    public bool SaveArgs;
    public bool AddonsReadOnly;
    public string AddonEdit = "";
    public string Welcome = "";
    public string VersionText = "";
    public string SelectText = "";
    public string AddonsText = "";
    public string BrowseText = "";
    public string ClearText = "";
    public string ParamsText = "";
    public string SaveText = "";
    public List<string> Frames = new();
    public int StartCalls;
    public int FocusCalls;
    public int PlayCalls;
    public int ScrollTo = -1;
    public double Width = 200;
    public double Height = 400;
    public double WelcomeHeight = 12;
    public double VersionHeight = 18;
    public double SelectHeight = 10;
    public double SaveHeight = 14;
    public double ParamsHeight = 11;
    public double AddonsHeight = 13;
    public double ClearPref = 40;
    public double BrowsePref = 80;
    public bool DialogAccepted;
    public List<string> DialogFiles = new();
    public Dictionary<string, string> Strings = new();
    public string GameName = "HCDE";
    public string Version = "1.2";

    public void Build(IReadOnlyList<VmWad1287> wads, int defaultIwad, string addonFiles, string args, bool save)
    {
        AddonsReadOnly = true;
        SaveArgs = save;
        SetAddonFiles(addonFiles);
        if (args.Length != 0)
            Parameters = args;
        Games.Clear();
        foreach (VmWad1287 wad in wads)
        {
            int slash = wad.Path.LastIndexOf('/');
            string file = slash < 0 ? wad.Path : wad.Path.Substring(slash + 1);
            Games.Add(file.Length == 0 ? wad.Name : wad.Name + " (" + file + ")");
        }
        if (defaultIwad >= 0 && defaultIwad < wads.Count)
            Selected = defaultIwad;
    }

    public VmPlayValues1287 Values()
    {
        return new VmPlayValues1287
        {
            Iwad = Selected,
            Addons = GetAddonFiles(),
            Args = Parameters,
            SaveArgs = SaveArgs,
        };
    }

    public void UpdateLanguage()
    {
        SelectText = Lookup("PICKER_SELECT");
        AddonsText = Lookup("PICKER_ADDONS");
        BrowseText = Lookup("PICKER_ADDONS_BROWSE");
        ClearText = Lookup("PICKER_ADDONS_CLEAR");
        ParamsText = Lookup("PICKER_ADDPARM");
        Welcome = Substitute(Lookup("PICKER_WELCOME"), "%s", GameName);
        VersionText = Substitute(Lookup("PICKER_VERSION"), "%s", Version);
        SaveText = Lookup("PICKER_REMPARM");
    }

    public void OnActivate()
    {
        StartCalls++;
    }

    public void OnBrowse()
    {
        if (!DialogAccepted)
            return;
        foreach (string file in DialogFiles)
            AddAddon(file);
        UpdateEdit();
    }

    public void OnClear()
    {
        Addons.Clear();
        UpdateEdit();
    }

    public void OnFocus()
    {
        FocusCalls++;
    }

    public bool OnFileDrop(string path)
    {
        AddAddon(path);
        UpdateEdit();
        return true;
    }

    public void AddAddon(string path)
    {
        path = Strip(path);
        if (path.Length == 0)
            return;
        foreach (string existing in Addons)
        {
            if (CompareNoCase(existing, path) == 0)
                return;
        }
        Addons.Add(path);
    }

    public void SetAddonFiles(string files)
    {
        Addons.Clear();
        files = Substitute(files, "\r", Separator);
        files = Substitute(files, "\n", Separator);
        foreach (string path in SplitSkipEmpty(files, Separator))
            AddAddon(path);
        UpdateEdit();
    }

    public string GetAddonFiles()
    {
        var files = new StringBuilder();
        foreach (string file in Addons)
        {
            if (files.Length != 0)
                files.Append(Separator);
            files.Append(file);
        }
        return files.ToString();
    }

    public void UpdateEdit()
    {
        var text = new StringBuilder();
        foreach (string file in Addons)
        {
            if (text.Length != 0)
                text.Append("; ");
            text.Append(file);
        }
        AddonEdit = text.ToString();
    }

    public void Layout()
    {
        Frames.Clear();
        double y = 10.0;
        double half = Width * 0.5;
        Frame("welcome", 0.0, y, half, WelcomeHeight);
        Frame("version", half, y, half, VersionHeight);
        y += VersionHeight + 10.0;
        Frame("select", 0.0, y, Width, SelectHeight);
        y += SelectHeight;
        double listTop = y;
        y = Height - SaveHeight;
        Frame("save", 0.0, y, Width, SaveHeight);
        const double editHeight = 24.0;
        y -= editHeight + 2.0;
        Frame("params", 0.0, y, Width, editHeight);
        y -= ParamsHeight;
        Frame("paramsLabel", 0.0, y, Width, ParamsHeight);
        y -= 8.0;
        double clearWidth = Math.Max(ClearPref + 10.0, 70.0);
        double browseWidth = Math.Max(BrowsePref + 10.0, 105.0);
        y -= editHeight;
        Frame("addons", 0.0, y, Math.Max(Width - browseWidth - clearWidth - 16.0, 0.0), editHeight);
        Frame("browse", Width - browseWidth - clearWidth - 8.0, y, browseWidth, editHeight);
        Frame("clear", Width - clearWidth, y, clearWidth, editHeight);
        y -= AddonsHeight;
        Frame("addonsLabel", 0.0, y, Width, AddonsHeight);
        y -= 20.0;
        Frame("games", 0.0, listTop, Width, Math.Max(y - listTop, 0.0));
        PlayCalls++;
        ScrollTo = Selected;
    }

    void Frame(string name, double x, double y, double w, double h)
    {
        Frames.Add(name + " " + x.ToString("G9", CultureInfo.InvariantCulture) + " " + y.ToString("G9", CultureInfo.InvariantCulture) + " " + w.ToString("G9", CultureInfo.InvariantCulture) + " " + h.ToString("G9", CultureInfo.InvariantCulture));
    }

    string Lookup(string key)
    {
        return Strings.TryGetValue(key, out string? value) ? value : "";
    }

    public static string Substitute(string text, string oldText, string newText)
    {
        if (oldText.Length == 0)
            return text;
        return text.Replace(oldText, newText, StringComparison.Ordinal);
    }

    public static List<string> SplitSkipEmpty(string text, string delimiter)
    {
        var tokens = new List<string>();
        if (text.Length == 0)
            return tokens;
        int last = 0;
        while (last <= text.Length)
        {
            int pos = text.IndexOf(delimiter, last, StringComparison.Ordinal);
            if (pos < 0)
                pos = text.Length;
            if (pos != last)
                tokens.Add(text.Substring(last, pos - last));
            last = pos + delimiter.Length;
        }
        return tokens;
    }

    public static string Strip(string text)
    {
        if (text.Length == 0)
            return text;
        int i = 0;
        while (i < text.Length && text[i] < 128 && IsCSpace(text[i]))
            i++;
        int j = text.Length - 1;
        while (j >= i && text[j] < 128 && IsCSpace(text[j]))
            j--;
        if (i == 0 && j == text.Length - 1)
            return text;
        if (i > j)
            return "";
        return text.Substring(i, j - i + 1);
    }

    public static int CompareNoCase(string left, string right)
    {
        int count = Math.Min(left.Length, right.Length);
        for (int i = 0; i < count; i++)
        {
            int a = Fold(left[i]);
            int b = Fold(right[i]);
            if (a != b)
                return a - b;
        }
        return left.Length - right.Length;
    }

    static int Fold(char c)
    {
        if (c >= 'A' && c <= 'Z')
            return c + 32;
        return c;
    }

    static bool IsCSpace(char c)
    {
        return c == ' ' || c == '\t' || c == '\n' || c == '\v' || c == '\f' || c == '\r';
    }
}
