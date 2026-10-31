namespace HCDE.Client;

/// <summary>
/// Win32 base-video construct, display-device selection, and adapter dump
/// from <c>win32basevideo.cpp</c> 1-204.
/// <c>vid_adapter</c> is an int cvar, default 0, flags archive and global config.
/// Adapter 0 asks for the primary monitor. Any other value enumerates, keeps the
/// first monitor until the 1-based index matches, and always continues the walk.
/// </summary>
public sealed class VmWin32BaseVideo1378Slice
{
    public const int CvarTypeInt = 1;
    public const int CvarArchive = 1;
    public const int CvarGlobalConfig = 1 << 10;
    public const int VidAdapterFlags = CvarArchive | CvarGlobalConfig;
    public const int VidAdapterDefault = 0;
    public const string VidAdapterName = "vid_adapter";
    public const int MonitorDefaultToPrimary = 1;
    public const int MonitorInfoPrimary = 1;
    public const int CchDeviceName = 32;
    public const int MoreInfoCapacity = 64;
    public const string TextColorBold = "\u001c+";

    public int VidAdapter { get; set; } = VidAdapterDefault;
    public string? DisplayDeviceName { get; set; }
    public long Monitor { get; private set; }
    public Action? ShowGameView { get; set; }
    public Func<int, int, int, long>? MonitorFromPoint { get; set; }
    public Action<long>? GetMonitorInfo { get; set; }
    public Func<long, MonitorInfo?>? GetMonitorInfoA { get; set; }
    public Action<Action<long>>? EnumDisplayMonitors { get; set; }
    public List<string> Lines { get; } = [];

    public sealed class MonitorInfo
    {
        public int Flags { get; init; }
        public int Left { get; init; }
        public int Top { get; init; }
        public int Right { get; init; }
        public int Bottom { get; init; }
        public string Device { get; init; } = "";
    }

    public void Construct()
    {
        if (ShowGameView == null)
            throw new InvalidOperationException("ShowGameView");
        ShowGameView();
        GetDisplayDeviceName();
    }

    public void GetDisplayDeviceName()
    {
        DisplayDeviceName = null;
        Monitor = 0;
        var state = new NameEnum();
        if (VidAdapter == 0)
            state.Found = PrimaryHandle();
        else
            Enumerate(monitor => VisitAdapter(monitor, state));
        if (state.Found == 0)
            return;
        var info = QueryA(state.Found);
        if (info == null)
            return;
        DisplayDeviceName = CopyDevice(info.Device);
        Monitor = state.Found;
    }

    public void DumpAdapters()
    {
        var selected = DisplayDeviceName;
        var index = 1;
        Enumerate(monitor =>
        {
            var more = "";
            var active = true;
            var info = QueryA(monitor);
            if (info != null)
            {
                var primary = (info.Flags & MonitorInfoPrimary) != 0;
                more = FormatMore(info);
                if (selected == null && !primary)
                    active = false;
                else if (selected != null && !string.Equals(selected, info.Device, StringComparison.Ordinal))
                    active = false;
            }

            var prefix = active ? TextColorBold : "";
            Lines.Add(prefix + index + ". " + more + "\n");
            index++;
        });
    }

    long PrimaryHandle()
    {
        if (MonitorFromPoint == null)
            throw new InvalidOperationException("MonitorFromPoint");
        return MonitorFromPoint(0, 0, MonitorDefaultToPrimary);
    }

    void VisitAdapter(long monitor, NameEnum state)
    {
        if (GetMonitorInfo == null)
            throw new InvalidOperationException("GetMonitorInfo");
        GetMonitorInfo(monitor);
        if (state.Cur == VidAdapter || state.Found == 0)
            state.Found = monitor;
        state.Cur++;
    }

    void Enumerate(Action<long> visit)
    {
        if (EnumDisplayMonitors == null)
            throw new InvalidOperationException("EnumDisplayMonitors");
        EnumDisplayMonitors(visit);
    }

    MonitorInfo? QueryA(long monitor)
    {
        if (GetMonitorInfoA == null)
            throw new InvalidOperationException("GetMonitorInfoA");
        return GetMonitorInfoA(monitor);
    }

    static string CopyDevice(string device)
    {
        var end = device.IndexOf('\0');
        if (end >= 0)
            device = device[..end];
        if (device.Length >= CchDeviceName)
            throw new InvalidOperationException("strcpy");
        return device;
    }

    static string FormatMore(MonitorInfo info)
    {
        var width = info.Right - info.Left;
        var height = info.Bottom - info.Top;
        var text = " [" + width + "x" + height + " @ (" + info.Left + "," + info.Top + ")]";
        if ((info.Flags & MonitorInfoPrimary) != 0)
            text += " (Primary)";
        var limit = MoreInfoCapacity - 1;
        return text.Length > limit ? text[..limit] : text;
    }

    sealed class NameEnum
    {
        public int Cur = 1;
        public long Found;
    }
}
