namespace HCDE.Playsim.Tests;

public class VmWidgetResources1292SliceTests
{
    static void AddNoto(VmWidgetResources1292.Archive archive)
    {
        archive.Entries["ui/noto/noto-sans-jp.ttf"] = new byte[] { 1 };
        archive.Entries["ui/noto/noto-sans-kr.ttf"] = new byte[] { 2 };
        archive.Entries["ui/noto/noto-sans-sc.ttf"] = new byte[] { 3 };
        archive.Entries["ui/noto/noto-sans.ttf"] = new byte[] { 4 };
        archive.Entries["ui/noto/noto-sans-armenian.ttf"] = new byte[] { 5 };
        archive.Entries["ui/noto/noto-sans-georgian.ttf"] = new byte[] { 6 };
    }

    [Fact]
    public void Setup_SearchesFromTheEndAndKeepsTheFirstArchive()
    {
        var widgets = new VmWidgetResources1292();
        Assert.False(widgets.IsAvailable());
        widgets.Theme = -4;
        Assert.Equal(0, widgets.Theme);
        widgets.Theme = 3;
        Assert.Equal(2, widgets.Theme);
        widgets.Theme = 0;
        widgets.SystemTheme = VmWidgetResources1292.SystemDark | VmWidgetResources1292.SystemHighContrast;

        var game = new VmWidgetResources1292.Archive();
        game.Entries["ui/theme.txt"] = new byte[] { 1, 2 };
        game.Entries["ui/empty.bin"] = Array.Empty<byte>();
        AddNoto(game);
        var mod = new VmWidgetResources1292.Archive();
        mod.Entries["ui/theme.txt"] = new byte[] { 3, 4 };
        mod.Entries["ui/noto/noto-sans.ttf"] = new byte[] { 9, 9 };
        mod.Entries["ui/font/doom.ttf"] = new byte[] { 8 };
        widgets.Catalog["game.pk3"] = game;
        widgets.Catalog["mod.pk3"] = mod;
        widgets.ExtraFiles.Add("missing.pk3");
        widgets.ExtraFiles.Add("mod.pk3");
        widgets.Disk["notes.txt"] = new byte[] { 7 };
        widgets.Disk["blank.txt"] = Array.Empty<byte>();
        widgets.Disk["gone.txt"] = null;

        widgets.Init("game.pk3");

        Assert.True(widgets.IsAvailable());
        Assert.True(widgets.UseDark);
        Assert.True(widgets.Contrast);
        Assert.Equal(VmWidgetResources1292.ModeDark, widgets.Mode);
        Assert.Equal(new byte[] { 1, 2 }, widgets.ThemeFromRoot);
        Assert.Equal(new byte[] { 3, 4 }, widgets.ThemeFromMods);
        Assert.Equal(new[] { "game.pk3", "mod.pk3" }, widgets.Opened);
        Assert.True(widgets.StyleSet);
        Assert.Equal(6.0, widgets.LineEditTop);
        Assert.Equal(12, widgets.ThemeSlots.Count);
        Assert.Equal("main 0", widgets.ThemeSlots[0]);
        Assert.Equal("border 1", widgets.ThemeSlots[11]);

        var fonts = widgets.LoadFont("NoToSaNs", false);
        Assert.Equal(6, fonts.Count);
        Assert.Equal("ja-*-*", fonts[0].Language);
        Assert.Equal(new byte[] { 1 }, fonts[0].Data);
        Assert.Equal("ko-*-*", fonts[1].Language);
        Assert.Equal("zh-Hans-*", fonts[2].Language);
        Assert.Equal("", fonts[3].Language);
        Assert.Equal(new byte[] { 9, 9 }, fonts[3].Data);
        Assert.Equal(new byte[] { 5 }, fonts[4].Data);
        Assert.Equal("", fonts[5].Language);
        Assert.Equal(new byte[] { 6 }, fonts[5].Data);
        Assert.Empty(widgets.LoadData("ui/empty.bin", true));
        Assert.Equal(new byte[] { 8 }, widgets.LoadData("ui/font/doom.ttf", false));
        var rootMiss = Assert.Throws<VmWidgetResources1292.Failure>(() => widgets.LoadFont("doom", true));
        Assert.Equal("Unable to find ui/font/doom.ttf", rootMiss.Message);
        var spaced = Assert.Throws<VmWidgetResources1292.Failure>(() => widgets.LoadFont("notosans ", false));
        Assert.Equal("Unable to find ui/font/notosans .ttf", spaced.Message);
        Assert.Equal(new byte[] { 7 }, widgets.LoadDisk("notes.txt"));
        Assert.Empty(widgets.LoadDisk("blank.txt"));
        Assert.Empty(widgets.LoadDisk("gone.txt"));
        Assert.Empty(widgets.LoadDisk("nope.txt"));
    }

    [Fact]
    public void Retry_SkipsThemeAfterAMissingFileAndLeavesThePointer()
    {
        var fresh = new VmWidgetResources1292();
        var early = Assert.Throws<VmWidgetResources1292.Failure>(() => fresh.LoadData("ui/theme.txt", true));
        Assert.Equal("InitWidgetResources has not been called", early.Message);

        var broken = new VmWidgetResources1292();
        var open = Assert.Throws<VmWidgetResources1292.Failure>(() => broken.Init("nope.pk3"));
        Assert.Equal("Unable to open nope.pk3", open.Message);
        Assert.True(broken.IsAvailable());
        Assert.False(broken.StyleSet);
        Assert.False(broken.ThemeAlreadyInstalled);
        Assert.Equal(-1, broken.Mode);

        var widgets = new VmWidgetResources1292();
        widgets.Theme = 1;
        widgets.SystemTheme = VmWidgetResources1292.SystemLight;
        widgets.Catalog["game.pk3"] = new VmWidgetResources1292.Archive();
        var fail = Assert.Throws<VmWidgetResources1292.Failure>(() => widgets.Init("game.pk3"));
        Assert.Equal("Unable to find ui/theme.txt", fail.Message);
        Assert.True(widgets.UseDark);
        Assert.False(widgets.Contrast);
        Assert.Equal(VmWidgetResources1292.ModeDark, widgets.Mode);
        Assert.True(widgets.ThemeAlreadyInstalled);
        Assert.True(widgets.ThemeLoadsAttempted);
        Assert.False(widgets.StyleSet);
        Assert.Empty(widgets.ThemeSlots);
        Assert.Null(widgets.ThemeFromRoot);

        widgets.Theme = 2;
        widgets.SystemTheme = VmWidgetResources1292.SystemDark | VmWidgetResources1292.SystemHighContrast;
        widgets.Init("game.pk3");
        Assert.False(widgets.UseDark);
        Assert.True(widgets.Contrast);
        Assert.Equal(VmWidgetResources1292.ModeLight, widgets.Mode);
        Assert.True(widgets.StyleSet);
        Assert.Equal(6.0, widgets.LineEditTop);
        Assert.Null(widgets.ThemeFromRoot);
        Assert.Equal(12, widgets.ThemeSlots.Count);
        Assert.Equal(new[] { "game.pk3" }, widgets.Opened);

        widgets.Close();
        Assert.True(widgets.IsAvailable());
        var after = Assert.Throws<VmWidgetResources1292.Failure>(() => widgets.LoadData("ui/theme.txt", false));
        Assert.Equal("Unable to find ui/theme.txt", after.Message);
    }
}
