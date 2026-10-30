using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Modal frame, tab order, and the launcher layout from <c>launcherwindow.cpp</c>
/// (1-169). Chunk 1290. String lookup and the page objects are host inputs.
/// </summary>
public sealed class VmLaunchInfo1290
{
    public bool IsNewRelease = true;
    public bool NotifyNewRelease = true;
    public string PrideColors = "";
    public float PrideMix;
    public bool NetStart;
}

public sealed class VmLaunchPage1290
{
    public string Id = "";
    public int LanguageCalls;
    public int ValueCalls;
    public int FocusCalls;
    public bool Hosting;
}

public sealed class VmLauncherWindow1290
{
    public const string GameName = "HCDE";

    public VmLaunchInfo1290 Info = new();
    public string Title = "";
    public string PrideColors = "";
    public float PrideMix;
    public bool ExecResult;
    public int ShowCalls;
    public int RunLoopCalls;
    public int ExitLoopCalls;
    public int InitTabsCalls;
    public int ButtonLanguageCalls;
    public int NoticeCalls;
    public string Notice = "";
    public bool NoticeVisible;
    public string WindowFrame = "";
    public double ScreenWidth = 1920;
    public double ScreenHeight = 1080;
    public double Width;
    public double Height;
    public double BannerHeight = 48;
    public double ButtonHeight = 36;
    public int Current = -1;
    public string Focused = "";
    public List<string> Frames = new();
    public Dictionary<string, string> Strings = new();
    readonly List<VmLaunchPage1290> _pages = new();
    readonly List<string> _tabs = new();
    VmLaunchPage1290? _release;

    public IReadOnlyList<string> Tabs => _tabs;
    public VmLaunchPage1290 Play = new() { Id = "play" };
    public VmLaunchPage1290 Settings = new() { Id = "settings" };
    public VmLaunchPage1290 Network = new() { Id = "network" };
    public VmLaunchPage1290 About = new() { Id = "about" };

    public bool ExecModal(VmLaunchInfo1290 info, Action<VmLauncherWindow1290>? loop = null)
    {
        Build(info);
        double x = (ScreenWidth - 615.0) * 0.5;
        double y = (ScreenHeight - 700.0) * 0.5;
        WindowFrame = FrameText("window", x, y, 615.0, 700.0);
        ShowCalls++;
        RunLoopCalls++;
        loop?.Invoke(this);
        return ExecResult;
    }

    public void Build(VmLaunchInfo1290 info)
    {
        Info = info;
        Title = GameName;
        PrideColors = info.PrideColors;
        PrideMix = info.PrideMix;
        _pages.Clear();
        _tabs.Clear();
        _release = null;
        Play = new VmLaunchPage1290 { Id = "play" };
        Settings = new VmLaunchPage1290 { Id = "settings" };
        Network = new VmLaunchPage1290 { Id = "network" };
        About = new VmLaunchPage1290 { Id = "about" };
        if (info.IsNewRelease && info.NotifyNewRelease)
        {
            _release = new VmLaunchPage1290 { Id = "release" };
            Add(_release, "Changelog");
        }
        Add(Play, "Play");
        Add(Settings, "Settings");
        Add(Network, "Multiplayer");
        Add(About, "About");
        InitTabsCalls++;
        UpdateLanguage();
        SetCurrent(0);
    }

    public void UpdatePlayButton()
    {
        ButtonLanguageCalls++;
    }

    public void SetUpdateNotice(string text, bool visible)
    {
        Notice = text;
        NoticeVisible = visible;
        NoticeCalls++;
    }

    public void Select(int index)
    {
        SetCurrent(index);
    }

    public void OpenAboutPage()
    {
        SetCurrent(IndexOf(About));
    }

    public bool IsInMultiplayer()
    {
        return Current >= 0 && CurrentPage() == Network;
    }

    public bool IsHosting()
    {
        return IsInMultiplayer() && Network.Hosting;
    }

    public void Start()
    {
        Info.NetStart = IsInMultiplayer();
        Settings.ValueCalls++;
        if (Info.NetStart)
            Network.ValueCalls++;
        else
            Play.ValueCalls++;
        if (_release != null)
            _release.ValueCalls++;
        ExecResult = true;
        ExitLoopCalls++;
    }

    public void Exit()
    {
        ExecResult = false;
        ExitLoopCalls++;
    }

    public void OnClose()
    {
        Exit();
    }

    public void UpdateLanguage()
    {
        SetText(Play, Lookup("PICKER_TAB_PLAY"));
        SetText(Settings, Lookup("OPTMNU_TITLE"));
        SetText(Network, Lookup("PICKER_TAB_MULTI"));
        SetText(About, Lookup("PICKER_TAB_ABOUT"));
        Play.LanguageCalls++;
        Settings.LanguageCalls++;
        Network.LanguageCalls++;
        About.LanguageCalls++;
        if (_release != null)
        {
            SetText(_release, Lookup("PICKER_TAB_RELEASE"));
            _release.LanguageCalls++;
        }
        ButtonLanguageCalls++;
        Layout();
    }

    public void Layout()
    {
        Frames.Clear();
        double top = 0.0;
        double bottom = Height;
        Frames.Add(FrameText("banner", 0.0, top, Width, BannerHeight));
        top += BannerHeight;
        bottom -= ButtonHeight;
        Frames.Add(FrameText("buttons", 0.0, bottom, Width, ButtonHeight));
        Frames.Add(FrameText("pages", 0.0, top, Width, Math.Max(bottom - top, 0.0)));
    }

    public string Lookup(string key)
    {
        return Strings.TryGetValue(key, out string? value) ? value : key;
    }

    void Add(VmLaunchPage1290 page, string text)
    {
        _pages.Add(page);
        _tabs.Add(text);
    }

    void SetText(VmLaunchPage1290 page, string text)
    {
        _tabs[IndexOf(page)] = text;
    }

    void SetCurrent(int index)
    {
        Current = index;
        Focused = CurrentPage().Id;
        CurrentPage().FocusCalls++;
    }

    VmLaunchPage1290 CurrentPage()
    {
        return _pages[Current];
    }

    int IndexOf(VmLaunchPage1290 page)
    {
        return _pages.IndexOf(page);
    }

    static string FrameText(string name, double x, double y, double width, double height)
    {
        return string.Join(' ', name, Num(x), Num(y), Num(width), Num(height));
    }

    static string Num(double value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }
}
