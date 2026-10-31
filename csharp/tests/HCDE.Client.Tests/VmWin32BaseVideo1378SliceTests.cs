namespace HCDE.Client.Tests;

public class VmWin32BaseVideo1378SliceTests
{
    [Fact]
    public void ConstructSelectsPrimaryOrEnumeratedAdapter()
    {
        Assert.Equal(1, VmWin32BaseVideo1378Slice.CvarTypeInt);
        Assert.Equal(1025, VmWin32BaseVideo1378Slice.VidAdapterFlags);
        Assert.Equal(0, VmWin32BaseVideo1378Slice.VidAdapterDefault);
        Assert.Equal("vid_adapter", VmWin32BaseVideo1378Slice.VidAdapterName);
        Assert.Equal("\u001c+", VmWin32BaseVideo1378Slice.TextColorBold);

        var order = new List<string>();
        var video = new VmWin32BaseVideo1378Slice
        {
            ShowGameView = () => order.Add("show"),
            MonitorFromPoint = (x, y, flags) =>
            {
                order.Add("point:" + x + "," + y + "," + flags);
                return 7;
            },
            GetMonitorInfoA = handle =>
            {
                order.Add("infoA:" + handle);
                return new VmWin32BaseVideo1378Slice.MonitorInfo { Device = "\\\\.\\DISPLAY1" };
            },
        };
        video.Construct();
        Assert.Equal(new[] { "show", "point:0,0,1", "infoA:7" }, order);
        Assert.Equal("\\\\.\\DISPLAY1", video.DisplayDeviceName);
        Assert.Equal(7L, video.Monitor);
        Assert.Null(video.GetMonitorInfo);

        video = PrimaryVideo();
        video.GetMonitorInfoA = _ => null;
        video.Construct();
        Assert.Null(video.DisplayDeviceName);
        Assert.Equal(0L, video.Monitor);

        video = PrimaryVideo();
        video.MonitorFromPoint = (_, _, _) => 0;
        var infoCalls = 0;
        video.GetMonitorInfoA = _ =>
        {
            infoCalls++;
            return new VmWin32BaseVideo1378Slice.MonitorInfo { Device = "nope" };
        };
        video.Construct();
        Assert.Equal(0, infoCalls);
        Assert.Null(video.DisplayDeviceName);

        var wide = new List<long>();
        video = EnumVideo(wide, out var seen);
        video.VidAdapter = 2;
        video.Construct();
        Assert.Equal(new long[] { 10, 20, 30 }, wide);
        Assert.Equal(20L, video.Monitor);
        Assert.Equal("MON20", video.DisplayDeviceName);
        Assert.DoesNotContain("point", seen);

        wide.Clear();
        video = EnumVideo(wide, out _);
        video.VidAdapter = 5;
        video.Construct();
        Assert.Equal(10L, video.Monitor);
        Assert.Equal(new long[] { 10, 20, 30 }, wide);

        wide.Clear();
        video = EnumVideo(wide, out _);
        video.VidAdapter = 1;
        video.Construct();
        Assert.Equal(10L, video.Monitor);

        wide.Clear();
        video = EnumVideo(wide, out _);
        video.VidAdapter = -1;
        video.Construct();
        Assert.Equal(10L, video.Monitor);

        wide.Clear();
        video = EnumVideo(wide, out _);
        video.VidAdapter = 9;
        video.GetMonitorInfo = handle =>
        {
            wide.Add(handle);
            if (handle == 10)
                video.VidAdapter = 2;
        };
        video.Construct();
        Assert.Equal(20L, video.Monitor);

        video = PrimaryVideo();
        video.GetMonitorInfoA = _ => new VmWin32BaseVideo1378Slice.MonitorInfo
        {
            Device = "ab\0cd",
        };
        video.GetDisplayDeviceName();
        Assert.Equal("ab", video.DisplayDeviceName);

        video = PrimaryVideo();
        video.GetMonitorInfoA = _ => new VmWin32BaseVideo1378Slice.MonitorInfo
        {
            Device = new string('n', 31),
        };
        video.GetDisplayDeviceName();
        Assert.Equal(31, video.DisplayDeviceName!.Length);

        video = PrimaryVideo();
        video.GetMonitorInfoA = _ => new VmWin32BaseVideo1378Slice.MonitorInfo
        {
            Device = new string('n', 32),
        };
        var overflow = Assert.Throws<InvalidOperationException>(() => video.GetDisplayDeviceName());
        Assert.Equal("strcpy", overflow.Message);
        Assert.Null(video.DisplayDeviceName);
        Assert.Equal(0L, video.Monitor);

        var missing = new VmWin32BaseVideo1378Slice();
        var show = Assert.Throws<InvalidOperationException>(() => missing.Construct());
        Assert.Equal("ShowGameView", show.Message);
    }

    [Fact]
    public void DumpAdaptersMarksTheSelectedMonitor()
    {
        var video = new VmWin32BaseVideo1378Slice();
        video.EnumDisplayMonitors = visit =>
        {
            visit(1);
            visit(2);
        };
        video.GetMonitorInfoA = handle => handle switch
        {
            1 => new VmWin32BaseVideo1378Slice.MonitorInfo
            {
                Flags = 1,
                Right = 1920,
                Bottom = 1080,
                Device = "\\\\.\\DISPLAY1",
            },
            _ => new VmWin32BaseVideo1378Slice.MonitorInfo
            {
                Flags = 0,
                Left = 1920,
                Top = 100,
                Right = 2720,
                Bottom = 700,
                Device = "\\\\.\\DISPLAY2",
            },
        };
        video.DumpAdapters();
        Assert.Equal(
            new[]
            {
                "\u001c+1.  [1920x1080 @ (0,0)] (Primary)\n",
                "2.  [800x600 @ (1920,100)]\n",
            },
            video.Lines);

        video.Lines.Clear();
        video.DisplayDeviceName = "\\\\.\\DISPLAY2";
        video.DumpAdapters();
        Assert.Equal("1.  [1920x1080 @ (0,0)] (Primary)\n", video.Lines[0]);
        Assert.Equal("\u001c+2.  [800x600 @ (1920,100)]\n", video.Lines[1]);

        video.Lines.Clear();
        video.DisplayDeviceName = "\\\\.\\display1";
        video.DumpAdapters();
        Assert.Equal("1.  [1920x1080 @ (0,0)] (Primary)\n", video.Lines[0]);

        video.Lines.Clear();
        video.DisplayDeviceName = "";
        video.DumpAdapters();
        Assert.Equal("1.  [1920x1080 @ (0,0)] (Primary)\n", video.Lines[0]);
        Assert.Equal("2.  [800x600 @ (1920,100)]\n", video.Lines[1]);

        video.Lines.Clear();
        video.DisplayDeviceName = "KEEP";
        video.EnumDisplayMonitors = visit =>
        {
            video.DisplayDeviceName = "OTHER";
            visit(1);
            visit(2);
        };
        video.GetMonitorInfoA = handle => new VmWin32BaseVideo1378Slice.MonitorInfo
        {
            Device = handle == 1 ? "KEEP" : "OTHER",
            Right = 10,
            Bottom = 4,
        };
        video.DumpAdapters();
        Assert.StartsWith("\u001c+1. ", video.Lines[0]);
        Assert.StartsWith("2. ", video.Lines[1]);

        video.Lines.Clear();
        video.DisplayDeviceName = "X";
        video.EnumDisplayMonitors = visit => visit(1);
        video.GetMonitorInfoA = _ => null;
        video.DumpAdapters();
        Assert.Equal("\u001c+1. \n", video.Lines[0]);

        video.Lines.Clear();
        video.DisplayDeviceName = null;
        video.GetMonitorInfoA = _ => new VmWin32BaseVideo1378Slice.MonitorInfo
        {
            Flags = 3,
            Left = -2147483647,
            Top = -2147483647,
            Right = 1,
            Bottom = 1,
            Device = "BIG",
        };
        video.DumpAdapters();
        Assert.Equal(
            "\u001c+1.  [-2147483648x-2147483648 @ (-2147483647,-2147483647)] (Primary\n",
            video.Lines[0]);
    }

    static VmWin32BaseVideo1378Slice PrimaryVideo()
    {
        return new VmWin32BaseVideo1378Slice
        {
            ShowGameView = () => { },
            MonitorFromPoint = (_, _, _) => 7,
            GetMonitorInfoA = _ => new VmWin32BaseVideo1378Slice.MonitorInfo { Device = "\\\\.\\DISPLAY1" },
        };
    }

    static VmWin32BaseVideo1378Slice EnumVideo(List<long> wide, out List<string> seen)
    {
        var trace = new List<string>();
        seen = trace;
        return new VmWin32BaseVideo1378Slice
        {
            ShowGameView = () => { },
            MonitorFromPoint = (_, _, _) =>
            {
                trace.Add("point");
                return 0;
            },
            GetMonitorInfo = wide.Add,
            GetMonitorInfoA = handle => new VmWin32BaseVideo1378Slice.MonitorInfo
            {
                Device = "MON" + handle,
            },
            EnumDisplayMonitors = visit =>
            {
                visit(10);
                visit(20);
                visit(30);
            },
        };
    }
}
