namespace HCDE.Playsim.Tests;

public class VmLauncherWindow1290SliceTests
{
    static VmLauncherWindow1290 Window(bool release, bool notify, string colors, float mix)
    {
        var window = new VmLauncherWindow1290
        {
            ScreenWidth = 1000,
            ScreenHeight = 800,
            BannerHeight = 48,
            ButtonHeight = 36,
        };
        window.Strings["PICKER_TAB_PLAY"] = "Play!";
        window.Strings["OPTMNU_TITLE"] = "Setup";
        var info = new VmLaunchInfo1290
        {
            IsNewRelease = release,
            NotifyNewRelease = notify,
            PrideColors = colors,
            PrideMix = mix,
        };
        window.Build(info);
        return window;
    }

    [Fact]
    public void Setup_PutsTheChangelogFirstAndStartsTheVisiblePage()
    {
        var window = Window(true, true, "list", 0.25f);
        Assert.Equal("HCDE", window.Title);
        Assert.Equal("list", window.PrideColors);
        Assert.Equal(0.25f, window.PrideMix);
        Assert.Equal(new[] { "PICKER_TAB_RELEASE", "Play!", "Setup", "PICKER_TAB_MULTI", "PICKER_TAB_ABOUT" }, window.Tabs);
        Assert.Equal(0, window.Current);
        Assert.Equal("release", window.Focused);
        Assert.Equal(1, window.InitTabsCalls);
        Assert.Equal(1, window.Play.LanguageCalls);
        Assert.Equal(1, window.ButtonLanguageCalls);
        Assert.False(window.IsInMultiplayer());
        Assert.False(window.IsHosting());

        window.Network.Hosting = true;
        window.Select(3);
        Assert.True(window.IsInMultiplayer());
        Assert.True(window.IsHosting());
        window.Network.Hosting = false;
        Assert.False(window.IsHosting());

        window.Start();
        Assert.True(window.Info.NetStart);
        Assert.Equal(1, window.Settings.ValueCalls);
        Assert.Equal(1, window.Network.ValueCalls);
        Assert.Equal(0, window.Play.ValueCalls);
        Assert.Equal(1, window.ExecResult ? 1 : 0);
        Assert.True(window.ExecResult);
        Assert.Equal(1, window.ExitLoopCalls);

        window.OpenAboutPage();
        Assert.Equal(4, window.Current);
        Assert.Equal("about", window.Focused);
        window.Start();
        Assert.False(window.Info.NetStart);
        Assert.Equal(1, window.Play.ValueCalls);
        Assert.Equal(2, window.Settings.ValueCalls);

        window.OnClose();
        Assert.False(window.ExecResult);
        Assert.Equal(3, window.ExitLoopCalls);

        window.SetUpdateNotice("ready", true);
        window.UpdatePlayButton();
        Assert.Equal("ready", window.Notice);
        Assert.True(window.NoticeVisible);
        Assert.Equal(1, window.NoticeCalls);
        Assert.Equal(2, window.ButtonLanguageCalls);

        var plain = Window(true, false, "", 0f);
        Assert.Equal(new[] { "Play!", "Setup", "PICKER_TAB_MULTI", "PICKER_TAB_ABOUT" }, plain.Tabs);
        Assert.Equal("play", plain.Focused);
        plain.Start();
        Assert.Equal(0, plain.Network.ValueCalls);
        Assert.Equal(1, plain.Play.ValueCalls);

        var modal = new VmLauncherWindow1290 { ScreenWidth = 1015, ScreenHeight = 900 };
        bool result = modal.ExecModal(new VmLaunchInfo1290(), page => page.Exit());
        Assert.False(result);
        Assert.Equal("window 200 100 615 700", modal.WindowFrame);
        Assert.Equal(1, modal.ShowCalls);
        Assert.Equal(1, modal.RunLoopCalls);
        Assert.Equal(1, modal.ExitLoopCalls);
    }

    [Fact]
    public void Layout_StacksTheBannerAndFloorsThePages()
    {
        var window = Window(false, false, "rainbow", 1f);
        Assert.Equal(
            "banner 0 0 0 48\n" +
            "buttons 0 -36 0 36\n" +
            "pages 0 48 0 0",
            string.Join('\n', window.Frames));

        window.Width = 615;
        window.Height = 700;
        window.Layout();
        Assert.Equal(
            "banner 0 0 615 48\n" +
            "buttons 0 664 615 36\n" +
            "pages 0 48 615 616",
            string.Join('\n', window.Frames));

        window.Width = 200;
        window.Height = 40;
        window.BannerHeight = 30;
        window.ButtonHeight = 20;
        window.Layout();
        Assert.Equal(
            "banner 0 0 200 30\n" +
            "buttons 0 20 200 20\n" +
            "pages 0 30 200 0",
            string.Join('\n', window.Frames));
    }
}
