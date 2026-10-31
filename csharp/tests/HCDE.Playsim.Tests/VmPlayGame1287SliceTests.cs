namespace HCDE.Playsim.Tests;

public class VmPlayGame1287SliceTests
{
    static VmWad1287 Wad(string name, string path)
    {
        return new VmWad1287 { Name = name, Path = path };
    }

    [Fact]
    public void Setup_FormatsTheWadLabelAndKeepsTheFirstAddon()
    {
        var page = new VmPlayGame1287();
        page.Build(
            new[]
            {
                Wad("Doom", "iwads/doom.wad"),
                Wad("Empty", "dir/"),
                Wad("Bare", "bare.wad"),
                Wad("Slashless", "C:\\doom.wad"),
            },
            2,
            "  Alpha.wad\r\nbeta.WAD |  alpha.wad  \n  ",
            " -fast",
            true);
        Assert.True(page.AddonsReadOnly);
        Assert.Equal(new[] { "Doom (doom.wad)", "Empty", "Bare (bare.wad)", "Slashless (C:\\doom.wad)" }, page.Games);
        Assert.Equal(2, page.Selected);
        Assert.Equal(" -fast", page.Parameters);
        Assert.True(page.SaveArgs);
        Assert.Equal(new[] { "Alpha.wad", "beta.WAD" }, page.Addons);
        Assert.Equal("Alpha.wad; beta.WAD", page.AddonEdit);
        Assert.Equal("Alpha.wad|beta.WAD", page.GetAddonFiles());

        var missed = new VmPlayGame1287();
        missed.Build(Array.Empty<VmWad1287>(), 0, "", "", false);
        Assert.Equal(-1, missed.Selected);
        Assert.Equal("", missed.Parameters);
        Assert.Equal("", missed.AddonEdit);

        Assert.True(page.OnFileDrop("  Alpha.wad "));
        Assert.Equal(2, page.Addons.Count);
        Assert.True(page.OnFileDrop(""));
        Assert.True(page.OnFileDrop("\u00A0X"));
        Assert.Equal("\u00A0X", page.Addons[2]);
        page.OnClear();
        Assert.Empty(page.Addons);
        Assert.Equal("", page.AddonEdit);

        page.DialogAccepted = false;
        page.DialogFiles.Add("kept-out.pk3");
        page.OnBrowse();
        Assert.Empty(page.Addons);
        page.DialogAccepted = true;
        page.DialogFiles.Add("gamma.pk3");
        page.OnBrowse();
        Assert.Equal(new[] { "kept-out.pk3", "gamma.pk3" }, page.Addons);

        page.Strings["PICKER_WELCOME"] = "Hello %s player %s";
        page.Strings["PICKER_VERSION"] = "v%s";
        page.Strings["PICKER_SELECT"] = "Pick";
        page.UpdateLanguage();
        Assert.Equal("Hello HCDE player HCDE", page.Welcome);
        Assert.Equal("v1.2", page.VersionText);
        Assert.Equal("Pick", page.SelectText);
        Assert.Equal("", page.SaveText);

        page.OnActivate();
        page.OnFocus();
        var values = page.Values();
        Assert.Equal(2, values.Iwad);
        Assert.Equal("kept-out.pk3|gamma.pk3", values.Addons);
        Assert.Equal(" -fast", values.Args);
        Assert.True(values.SaveArgs);
        Assert.Equal(1, page.StartCalls);
        Assert.Equal(1, page.FocusCalls);
    }

    [Fact]
    public void Layout_ClampsTheAddonRowAndTheListHeight()
    {
        var page = new VmPlayGame1287();
        page.Selected = 3;
        page.Layout();
        Assert.Equal(
            "welcome 0 10 100 12\n" +
            "version 100 10 100 18\n" +
            "select 0 38 200 10\n" +
            "save 0 386 200 14\n" +
            "params 0 360 200 24\n" +
            "paramsLabel 0 349 200 11\n" +
            "addons 0 317 9 24\n" +
            "browse 17 317 105 24\n" +
            "clear 130 317 70 24\n" +
            "addonsLabel 0 304 200 13\n" +
            "games 0 48 200 236",
            string.Join('\n', page.Frames));
        Assert.Equal(1, page.PlayCalls);
        Assert.Equal(3, page.ScrollTo);

        page.Width = 100;
        page.Height = 80;
        page.VersionHeight = 30;
        page.SelectHeight = 20;
        page.Layout();
        Assert.Equal(
            "welcome 0 10 50 12\n" +
            "version 50 10 50 30\n" +
            "select 0 50 100 20\n" +
            "save 0 66 100 14\n" +
            "params 0 40 100 24\n" +
            "paramsLabel 0 29 100 11\n" +
            "addons 0 -3 0 24\n" +
            "browse -83 -3 105 24\n" +
            "clear 30 -3 70 24\n" +
            "addonsLabel 0 -16 100 13\n" +
            "games 0 70 100 0",
            string.Join('\n', page.Frames));
    }
}
