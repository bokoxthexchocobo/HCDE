namespace HCDE.Playsim.Tests;

public class VmSettings1288SliceTests
{
    const string Menu =
        "\u00EF\u00BB\u00BFOptionString \"Cursors\"\n" +
        "{\n" +
        "  \"nope\", \"Skip\"\n" +
        "}\n" +
        "optionstring ; not this\n" +
        "\"LanguageOptions\"\n" +
        "{\n" +
        "  /* block */\n" +
        "  \"en,US\", \"say \\\"hi\\\"\"\n" +
        "  \"de\", \"line\r\nnext\" // trailing\n" +
        "  \"EN\", \"Second\"\n" +
        "}\n" +
        "#region noise\n" +
        "OptionString \"LanguageOptions\"\n" +
        "{\n" +
        "  \"fr\", \"Francais\"\n" +
        "}\n";

    static VmSettings1288 Page(int fileLoad, int flags, int backend, string language, string menu, bool lights, bool bright, bool wide)
    {
        var page = new VmSettings1288();
        page.Strings["OPTVAL_LAX"] = "Lax";
        page.Strings["PICKER_GENERAL"] = "General";
        page.Build(new VmSettingsInfo1288
        {
            Fullscreen = true,
            QueryIwad = false,
            StartFlags = flags,
            Backend = backend,
            Language = language,
            FileLoad = fileLoad,
        }, menu, lights, bright, wide);
        return page;
    }

    [Fact]
    public void Setup_ParsesTheLastLanguageAndKeepsACustomLoad()
    {
        var page = Page(7, 21, 5, "en", Menu, true, false, true);
        Assert.True(page.Fullscreen);
        Assert.True(page.DontAsk);
        Assert.True(page.Autoload);
        Assert.False(page.Lights);
        Assert.True(page.Brightmaps);
        Assert.False(page.Widescreen);
        Assert.True(page.Support);
        Assert.False(page.OpenGl);
        Assert.True(page.Vulkan);
        Assert.False(page.HideLanguage);
        Assert.Equal(5, page.ExtraWadFlags);
        Assert.Equal(4, page.MaxDisplayItems);
        Assert.False(page.DropDownBelow);
        Assert.Equal(2, page.LoadSelected);
        Assert.Equal(new[] { "Lax", "OPTVAL_DEFAULT", "OPTVAL_STRICT", "OPTVAL_CUSTOM" }, page.LoadItems);
        Assert.Equal(new[] { "en,US", "de", "EN", "fr" }, page.Languages.Select(lang => lang.Iso));
        Assert.Equal(new[] { "say \"hi\"", "line\nnext", "Second", "Francais" }, page.Languages.Select(lang => lang.Display));
        Assert.Equal(2, page.Selected);
        Assert.Equal(page.LangItems, page.Languages.Select(lang => lang.Display));

        page.Support = false;
        page.Lights = true;
        page.DontAsk = false;
        page.Fullscreen = false;
        var written = page.SetValues(new VmSettingsInfo1288 { FileLoad = 99 });
        Assert.False(written.Fullscreen);
        Assert.True(written.QueryIwad);
        Assert.Equal("EN", written.Language);
        Assert.Equal(7, written.StartFlags);
        Assert.Equal(7, written.FileLoad);
        Assert.Equal(1, written.Backend);

        page.LoadSelected = 3;
        written = page.SetValues(new VmSettingsInfo1288 { FileLoad = 99 });
        Assert.Equal(99, written.FileLoad);
        page.Vulkan = true;
        page.OnOpenGlChanged(false);
        Assert.True(page.Vulkan);
        page.OpenGl = true;
        page.OnOpenGlChanged(true);
        Assert.False(page.Vulkan);
        Assert.Equal(0, page.SetValues(new VmSettingsInfo1288()).Backend);
        page.OpenGl = false;
        Assert.False(page.Vulkan);
        Assert.Equal(1, page.SetValues(new VmSettingsInfo1288()).Backend);
        page.OpenGl = true;
        page.Vulkan = true;
        Assert.Equal(0, page.SetValues(new VmSettingsInfo1288()).Backend);

        var all = Page(-1, -1, 0, "fr", Menu, false, false, false);
        Assert.True(all.Autoload && all.Lights && all.Brightmaps && all.Widescreen && all.Support);
        Assert.True(all.OpenGl);
        Assert.False(all.Vulkan);
        Assert.Equal(3, all.LoadSelected);
        Assert.Equal(0, all.ExtraWadFlags);
        Assert.Equal(3, all.Selected);
        var kept = all.SetValues(new VmSettingsInfo1288 { FileLoad = -1 });
        Assert.Equal(-1, kept.FileLoad);
        Assert.Equal(31, kept.StartFlags);
        Assert.Equal(0, kept.Backend);

        Assert.Equal(0, Page(0, 0, 1, "auto", "\n", false, false, false).LoadSelected);
        Assert.Equal(1, Page(3, 0, 1, "auto", "\n", false, false, false).LoadSelected);
        Assert.Equal(3, Page(99, 0, 1, "auto", "\n", false, false, false).LoadSelected);
        var empty = Page(0, 0, 1, "auto", "", false, false, false);
        Assert.False(empty.HideLanguage);
        Assert.Empty(empty.Languages);
        Assert.Throws<ArgumentOutOfRangeException>(() => empty.SetValues(new VmSettingsInfo1288()));

        page.LangName = "resolved";
        page.Strings["PICKER_PREFERBACKEND"] = "Backend";
        page.OnLanguageChanged(1);
        Assert.Equal("de", page.RequestedLanguage);
        Assert.Equal("resolved", page.CanvasLanguage);
        Assert.Equal("Backend", page.BackendLabel);
        Assert.Equal("PICKER_LIGHTS", page.LightsText);
        Assert.Equal("Second", page.LangItems[2]);
        Assert.Equal(1, page.UpdateCalls);
        Assert.Equal(1, page.LauncherLanguageCalls);
        Assert.Equal(1, page.PlayCalls);
        Assert.Equal(1, page.ScrollTo);
    }

    [Fact]
    public void Layout_StacksTheColumnsAndHidesABrokenList()
    {
        const string broken =
            "OptionString \"LanguageOptions\"\n{\n\"de\", \"Deutsch\"\n\"fr\" \"Francais\"\n}\n";
        var hidden = Page(0, 0, 1, "de", broken, false, false, false);
        Assert.True(hidden.HideLanguage);
        Assert.Equal(new[] { "de" }, hidden.Languages.Select(lang => lang.Iso));
        Assert.Equal("Deutsch", hidden.LangItems[0]);
        hidden.Width = 201;
        hidden.Height = 40;
        hidden.LoadLabelWidth = 40;
        hidden.LoadListWidth = 80;
        hidden.Layout();
        Assert.Equal(
            "general 0 0 190 20\n" +
            "fullscreen 0 20 190 20\n" +
            "autoload 0 40 190 20\n" +
            "dontAsk 0 60 190 20\n" +
            "support 0 80 190 20\n" +
            "backend 20.5 0 190 20\n" +
            "vulkan 20.5 20 190 20\n" +
            "opengl 20.5 40 190 20\n" +
            "loadLabel 0 115 40 20\n" +
            "loadList 44 110 80 30",
            string.Join('\n', hidden.Frames));
        Assert.Equal(0, hidden.ScrollTo);

        var shown = Page(0, 0, 1, "de", "OptionString \"LanguageOptions\" { \"de\", \"Deutsch\" \"fr\", \"French\" }\n", true, false, true);
        Assert.False(shown.HideLanguage);
        Assert.Equal(new[] { "de", "fr" }, shown.Languages.Select(lang => lang.Iso));
        shown.Width = 400;
        shown.Height = 500;
        shown.Layout();
        Assert.Equal(
            "general 0 0 190 20\n" +
            "fullscreen 0 20 190 20\n" +
            "autoload 0 40 190 20\n" +
            "dontAsk 0 60 190 20\n" +
            "support 0 80 190 20\n" +
            "backend 120 0 190 20\n" +
            "vulkan 120 20 190 20\n" +
            "opengl 120 40 190 20\n" +
            "extras 240 0 160 20\n" +
            "lights 240 20 160 20\n" +
            "widescreen 240 40 160 20\n" +
            "language 0 110 400 20\n" +
            "langList 0 130 400 336\n" +
            "loadLabel 0 475 40 20\n" +
            "loadList 44 470 80 30",
            string.Join('\n', shown.Frames));

        shown.Height = 50;
        shown.ExtraWadFlags = 0;
        shown.Layout();
        Assert.Equal(
            "general 0 0 190 20\n" +
            "fullscreen 0 20 190 20\n" +
            "autoload 0 40 190 20\n" +
            "dontAsk 0 60 190 20\n" +
            "support 0 80 190 20\n" +
            "backend 120 0 190 20\n" +
            "vulkan 120 20 190 20\n" +
            "opengl 120 40 190 20\n" +
            "language 0 110 400 20\n" +
            "langList 0 130 400 0\n" +
            "loadLabel 0 139 40 20\n" +
            "loadList 44 134 80 30",
            string.Join('\n', shown.Frames));
    }
}
