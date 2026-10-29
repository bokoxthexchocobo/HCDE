namespace HCDE.Playsim.Tests;

public class VmThemeData1202SliceTests
{
    [Fact]
    public void Initialize_PaintsFallbacksThenAppliesThemeText()
    {
        VmThemeData1202Slice.Reset();
        VmThemeData1202Slice.Initialize(VmThemeData1202Slice.Light, false);
        Assert.Equal(VmThemeData1202Slice.FromRgb(0x7f7f7f), VmThemeData1202Slice.GetAccent());
        Assert.Equal(VmThemeData1202Slice.FromRgb(0xffffff), VmThemeData1202Slice.LightData.Main.Bg);
        Assert.Equal(VmThemeData1202Slice.FromRgb(0x000000), VmThemeData1202Slice.LightData.Main.Fg);
        Assert.Equal(VmThemeData1202Slice.FromRgb(0xffff00), VmThemeData1202Slice.LightData.Hover.Bg);
        Assert.Equal(VmThemeData1202Slice.FromRgb(0x000000), VmThemeData1202Slice.LightData.Border.Bg);
        Assert.Equal(VmThemeData1202Slice.FromRgb(0x0000ff), VmThemeData1202Slice.DarkData.Hover.Bg);
        Assert.Equal(VmThemeData1202Slice.FromRgb(0xffffff), VmThemeData1202Slice.DarkData.Main.Fg);

        VmThemeData1202Slice.Reset();
        const string pk3 = """
            main #fff #000
            // kept
            ; also kept
            /* block */
            [LIGHT]
            accent #abcxyz
            main #ff00aa #010203
            [dark]
            header #abc #def
            nope
            """;
        const string mods = """
            [dark]
            accent #00ff00
            button #123456 #abcdef
            """;
        VmThemeData1202Slice.Initialize(VmThemeData1202Slice.Dark, false, pk3, mods);
        Assert.Equal(VmThemeData1202Slice.FromRgb(0xFF00AA), VmThemeData1202Slice.LightData.Main.Bg);
        Assert.Equal(VmThemeData1202Slice.FromRgb(0x010203), VmThemeData1202Slice.LightData.Main.Fg);
        Assert.Equal(VmThemeData1202Slice.FromRgb(0xAABBCC), VmThemeData1202Slice.DarkData.Header.Bg);
        Assert.Equal(VmThemeData1202Slice.FromRgb(0xDDEEFF), VmThemeData1202Slice.DarkData.Header.Fg);
        Assert.Equal(VmThemeData1202Slice.FromRgb(0x123456), VmThemeData1202Slice.DarkData.Button.Bg);
        Assert.Equal(VmThemeData1202Slice.FromRgb(0xABCDEF), VmThemeData1202Slice.DarkData.Button.Fg);
        Assert.Equal(VmThemeData1202Slice.FromRgb(0x00FF00), VmThemeData1202Slice.Accent);
        Assert.Equal(VmThemeData1202Slice.FromRgb(-1), VmThemeData1202Slice.FromRgb(unchecked((int)0xffffffff)));
        Assert.Contains("Unknown theme command", VmThemeData1202Slice.Warnings);
        Assert.Contains("Theme not selected", VmThemeData1202Slice.Warnings);

        var frozen = VmThemeData1202Slice.Accent;
        VmThemeData1202Slice.Initialize(VmThemeData1202Slice.Light, false, "[light]\naccent #ffffff\n");
        Assert.Equal(frozen, VmThemeData1202Slice.Accent);
        Assert.Equal(VmThemeData1202Slice.Dark, VmThemeData1202Slice.Mode);
    }

    [Fact]
    public void Contrast_AndMix_KeepPairsAndBlendEndpoints()
    {
        VmThemeData1202Slice.Reset();
        VmThemeData1202Slice.Initialize(
            VmThemeData1202Slice.Light,
            true,
            "[light]\naccent #abc\nmain #ffffff #000000\n");
        Assert.Equal(VmThemeData1202Slice.FromRgb(0xAABBCC), VmThemeData1202Slice.GetAccent());
        Assert.Equal(VmThemeData1202Slice.FromRgb(0xffffff), VmThemeData1202Slice.GetMain(0));
        Assert.Equal(VmThemeData1202Slice.FromRgb(0x000000), VmThemeData1202Slice.GetMain(1));
        var mid = VmThemeData1202Slice.GetMain(0.5f);
        Assert.Equal(0.5f, mid.R);
        Assert.Equal(0.5f, mid.G);
        Assert.Equal(0.5f, mid.B);
        Assert.Equal(1f, mid.A);
        Assert.Equal(VmThemeData1202Slice.GetMain(-1), VmThemeData1202Slice.GetMain(0));
        Assert.Equal(VmThemeData1202Slice.GetHeader(2), VmThemeData1202Slice.FromRgb(0x000000));

        VmThemeData1202Slice.Reset();
        var missing = Assert.Throws<VmThemeData1202Slice.ScriptError>(
            () => VmThemeData1202Slice.Initialize(VmThemeData1202Slice.Light, false, "[light]\naccent\n"));
        Assert.Equal("Missing string (unexpected end of file).", missing.Detail);

        VmThemeData1202Slice.Reset();
        VmThemeData1202Slice.Initialize(VmThemeData1202Slice.Light, false, "main #fff #000\n");
        Assert.Equal(new[] { "Theme not selected", "Unknown theme command", "Theme not selected", "Unknown theme command", "Theme not selected" }, VmThemeData1202Slice.Warnings);
        Assert.Equal(VmThemeData1202Slice.FromRgb(0xffffff), VmThemeData1202Slice.LightData.Main.Bg);
    }
}
