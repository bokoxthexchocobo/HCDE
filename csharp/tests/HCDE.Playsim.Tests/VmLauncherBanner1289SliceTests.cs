namespace HCDE.Playsim.Tests;

public class VmLauncherBanner1289SliceTests
{
    [Fact]
    public void Setup_RepeatsTheStripeAndPrintsTheClockFlag()
    {
        var page = new VmLauncherBanner1289();
        page.Build("bisexual", 1f);
        Assert.Equal(new uint[]
        {
            0xD60270, 0xD60270, 0xD60270,
            0x9B4F96, 0x9B4F96,
            0x0038A8, 0x0038A8, 0x0038A8,
        }, page.Solids);
        Assert.Equal(page.Solids, page.Mixed);
        Assert.Equal("ui/banner-light.png", page.LogoPath);
        Assert.Equal("", page.ListText);
        Assert.Equal(6, page.FlagIndex);

        page.HeaderR = 0.25f;
        page.HeaderG = 0.5f;
        page.HeaderB = 0.75f;
        page.Build("bisexual", 0f);
        uint bg = VmColorSpace1211Slice.Rgb(0.25f, 0.5f, 0.75f).Rgb24();
        Assert.Equal("ui/banner-dark.png", page.LogoPath);
        Assert.Equal(8, page.Mixed.Count);
        Assert.All(page.Mixed, color => Assert.Equal(bg, color));
        Assert.Equal(0xD60270u, page.Solids[0]);

        page.Build("bisexual", -2f);
        Assert.Equal("ui/banner-dark.png", page.LogoPath);
        Assert.Equal(page.Solids, page.Mixed);

        page.Build("Gay", 0.5f);
        Assert.Equal(14, page.Solids.Count);
        Assert.Equal(0x078D70u, page.Solids[0]);
        Assert.Equal(0x078D70u, page.Solids[1]);
        Assert.Equal(0x3D1A78u, page.Solids[13]);
        uint mixed = VmColorSpace1211Slice.Mix(
            VmColorSpace1211Slice.Rgb(0xFFFFFFu),
            VmColorSpace1211Slice.Rgb(0x078D70u),
            0.5f).Rgb24();
        Assert.Equal(mixed, page.Mixed[0]);
        Assert.NotEqual(page.Solids[0], page.Mixed[0]);

        var demi = new VmLauncherBanner1289();
        demi.Build("demisexual", 1f);
        Assert.Equal(18, demi.Solids.Count);
        Assert.Equal(0xFFFFFFu, demi.Solids[0]);
        Assert.Equal(0xFFFFFFu, demi.Solids[5]);
        Assert.Equal(0x6E0070u, demi.Solids[6]);
        Assert.Equal(0xD2D2D2u, demi.Solids[17]);

        page.NowMs = 22;
        page.LightMode = true;
        page.Build("LiSt", 1f);
        Assert.Equal(1, page.FlagIndex);
        Assert.Equal(10, page.Solids.Count);
        Assert.Equal(0xE28C00u, page.Solids[0]);
        Assert.StartsWith("Available flag colors: random, agender, aroace,", page.ListText);
        Assert.Contains("\nSelecting: aroace\n", page.ListText);
        Assert.EndsWith("Open an issue on our github if your flag is missing\n", page.ListText);
        Assert.Equal(22, page.ListText.Split(',').Length);
        Assert.Equal("ui/banner-light.png", page.LogoPath);

        page.Build("RANDOM", 1f);
        Assert.Equal("", page.ListText);
        Assert.Equal(1, page.FlagIndex);

        page.Build("", 1f);
        Assert.Empty(page.Solids);
        Assert.Equal(-1, page.FlagIndex);
        Assert.Equal("ui/banner-light.png", page.LogoPath);
        page.LightMode = false;
        page.Build("missing", 1f);
        Assert.Empty(page.Solids);
        Assert.Equal("ui/banner-light.png", page.LogoPath);
        page.Build("missing", 0f);
        Assert.Equal("ui/banner-dark.png", page.LogoPath);
    }

    [Fact]
    public void Layout_FloorsTheStripeAndCentersTheLogo()
    {
        var page = new VmLauncherBanner1289();
        page.LogoHeight = 30;
        page.Width = 100;
        page.LogoWidth = 10;
        page.Build("bisexual", 1f);
        Assert.Equal(30, page.PreferredHeight());
        page.Layout();
        Assert.Equal(
            "band 0 0 100 4\n" +
            "mix 45 0 10 4\n" +
            "band 0 3 100 4\n" +
            "mix 45 3 10 4\n" +
            "band 0 7 100 4\n" +
            "mix 45 7 10 4\n" +
            "band 0 11 100 4\n" +
            "mix 45 11 10 4\n" +
            "band 0 15 100 4\n" +
            "mix 45 15 10 4\n" +
            "band 0 18 100 4\n" +
            "mix 45 18 10 4\n" +
            "band 0 22 100 4\n" +
            "mix 45 22 10 4\n" +
            "band 0 26 100 4\n" +
            "mix 45 26 10 4\n" +
            "logo 0 0 100 30",
            string.Join('\n', page.Frames));

        page.Width = 10;
        page.LogoWidth = 30;
        page.LogoHeight = 16;
        page.Layout();
        Assert.Equal(
            "band 0 0 10 2\n" +
            "mix -10 0 30 2\n" +
            "band 0 2 10 2\n" +
            "mix -10 2 30 2\n" +
            "band 0 4 10 2\n" +
            "mix -10 4 30 2\n" +
            "band 0 6 10 2\n" +
            "mix -10 6 30 2\n" +
            "band 0 8 10 2\n" +
            "mix -10 8 30 2\n" +
            "band 0 10 10 2\n" +
            "mix -10 10 30 2\n" +
            "band 0 12 10 2\n" +
            "mix -10 12 30 2\n" +
            "band 0 14 10 2\n" +
            "mix -10 14 30 2\n" +
            "logo 0 0 10 16",
            string.Join('\n', page.Frames));

        page.Build("", 1f);
        page.Layout();
        Assert.Equal("logo 0 0 10 16", string.Join('\n', page.Frames));
    }
}
