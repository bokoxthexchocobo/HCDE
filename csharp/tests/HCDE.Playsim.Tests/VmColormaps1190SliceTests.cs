namespace HCDE.Playsim.Tests;

public class VmColormaps1190SliceTests
{
    static void Palette()
    {
        VmColormaps1190Slice.BaseColors = new VmColormaps1190Slice.Rgb[256];
        for (int i = 0; i < 256; i++)
            VmColormaps1190Slice.BaseColors[i] = new VmColormaps1190Slice.Rgb { R = i };
    }

    static void SeedSpecials()
    {
        VmColormaps1190Slice.InitSpecialColormaps = () =>
        {
            for (int i = 0; i < 6; i++)
                VmColormaps1190Slice.SpecialColormaps.Add(new VmColormaps1190Slice.SpecialMap { Id = "s" + i });
        };
    }

    [Fact]
    public void Init_BuildsBlendsFromTheFirstColormapLump()
    {
        VmColormaps1190Slice.Clear();
        Palette();
        SeedSpecials();
        VmColormaps1190Slice.FakeCmaps.Add(new VmColormaps1190Slice.FakeCmap { Name = "gone" });
        VmColormaps1190Slice.SpecialColormaps.Add(new VmColormaps1190Slice.SpecialMap { Id = "old" });
        var calls = new List<string>();
        var seed = VmColormaps1190Slice.InitSpecialColormaps;
        VmColormaps1190Slice.InitSpecialColormaps = () =>
        {
            calls.Add("init");
            seed!();
        };
        VmColormaps1190Slice.UpdateSpecialColormap = (_, _, _, _, _, _, _) => calls.Add("update");
        VmColormaps1190Slice.Lumps.Add(new VmColormaps1190Slice.Lump { Name = "SKIP", Colormap = false, Data = new byte[256] });
        VmColormaps1190Slice.Lumps.Add(new VmColormaps1190Slice.Lump
        {
            Name = "watermap",
            Colormap = true,
            Data = Enumerable.Repeat((byte)255, 256).ToArray()
        });
        VmColormaps1190Slice.Lumps.Add(new VmColormaps1190Slice.Lump
        {
            Name = "WATERMAP",
            Colormap = true,
            Data = Enumerable.Repeat((byte)1, 256).ToArray()
        });
        VmColormaps1190Slice.Lumps.Add(new VmColormaps1190Slice.Lump { Name = "SHORT", Colormap = true, Data = new byte[4] });
        VmColormaps1190Slice.Lumps.Add(new VmColormaps1190Slice.Lump
        {
            Name = "OTHER",
            Colormap = true,
            Data = new byte[256]
        });
        VmColormaps1190Slice.Lumps.Add(new VmColormaps1190Slice.Lump { Name = "COLORMAP", Colormap = true, Data = new byte[256] });

        VmColormaps1190Slice.Init(true);
        Assert.Equal(513, VmColormaps1190Slice.InvulFlags);
        Assert.Equal(new[] { "init" }, calls);
        Assert.Equal(5, VmColormaps1190Slice.FakeCmaps.Count);
        Assert.Equal("", VmColormaps1190Slice.FakeCmaps[0].Name);
        Assert.Equal(0u, VmColormaps1190Slice.FakeCmaps[0].Blend);
        var water = VmColormaps1190Slice.FakeCmaps[1];
        Assert.Equal("watermap", water.Name);
        Assert.Equal(1, water.Lump);
        Assert.Equal(0xFFFF0000u, water.Blend);
        Assert.Equal("SHORT", VmColormaps1190Slice.FakeCmaps[2].Name);
        Assert.Equal(0u, VmColormaps1190Slice.FakeCmaps[2].Blend);
        Assert.Equal(3, VmColormaps1190Slice.FakeCmaps[2].Lump);
        Assert.Equal(0xFF000000u, VmColormaps1190Slice.FakeCmaps[3].Blend);
        Assert.Equal("s5", VmColormaps1190Slice.SpecialColormaps[0].Id);
        Assert.NotSame(VmColormaps1190Slice.SpecialColormaps[5], VmColormaps1190Slice.SpecialColormaps[0]);
        Assert.Equal(1u, VmColormaps1190Slice.ColormapNumForName("WATERMAP"));
        Assert.Equal(0u, VmColormaps1190Slice.ColormapNumForName("COLORMAP"));
        Assert.DoesNotContain(VmColormaps1190Slice.FakeCmaps, map => map.Name == "gone");

        uint first = water.Blend;
        VmColormaps1190Slice.Init(false);
        Assert.Equal(first, VmColormaps1190Slice.FakeCmaps[1].Blend);
    }

    [Fact]
    public void ColormapNumForName_PrefersTheLastMatchAndWater()
    {
        VmColormaps1190Slice.Clear();
        VmColormaps1190Slice.FakeCmaps.Add(new VmColormaps1190Slice.FakeCmap());
        VmColormaps1190Slice.FakeCmaps.Add(new VmColormaps1190Slice.FakeCmap { Name = "FIRST", Blend = 0x11 });
        VmColormaps1190Slice.FakeCmaps.Add(new VmColormaps1190Slice.FakeCmap { Name = "WATERMAP", Blend = 0x22 });
        VmColormaps1190Slice.FakeCmaps.Add(new VmColormaps1190Slice.FakeCmap { Name = "FIRST", Blend = 0x33 });
        VmColormaps1190Slice.FakeCmaps.Add(new VmColormaps1190Slice.FakeCmap { Name = "ABCDEFGH" });

        Assert.Equal(0u, VmColormaps1190Slice.ColormapNumForName("colormap"));
        Assert.Equal(3u, VmColormaps1190Slice.ColormapNumForName("first"));
        Assert.Equal(2u, VmColormaps1190Slice.ColormapNumForName("watermap"));
        Assert.Equal(4u, VmColormaps1190Slice.ColormapNumForName("abcdefghZZ"));
        Assert.Equal(0u, VmColormaps1190Slice.ColormapNumForName("ABCDEFG"));
        Assert.Equal(0u, VmColormaps1190Slice.ColormapNumForName("nope"));

        VmColormaps1190Slice.FakeCmaps.RemoveAt(2);
        Assert.Equal(0x80004FA5u, VmColormaps1190Slice.ColormapNumForName("WATERMAP"));
        Assert.Equal(VmColormaps1190Slice.WaterMap, VmColormaps1190Slice.BlendForColormap(VmColormaps1190Slice.WaterMap));
        Assert.Equal(0x33u, VmColormaps1190Slice.BlendForColormap(2));
        Assert.Equal(0u, VmColormaps1190Slice.BlendForColormap(50));
        Assert.Equal(0u, VmColormaps1190Slice.BlendForColormap(0));
    }

    [Fact]
    public void Invulnerability_UsesCustomColorsOrRestoresTheRealInverse()
    {
        VmColormaps1190Slice.Clear();
        Assert.False(VmColormaps1190Slice.CustomizeInvulMap);
        Assert.Equal(0x00001au, VmColormaps1190Slice.Color1);
        Assert.Equal(0xa6a67au, VmColormaps1190Slice.Color2);
        for (int i = 0; i < 6; i++)
            VmColormaps1190Slice.SpecialColormaps.Add(new VmColormaps1190Slice.SpecialMap { Id = "s" + i });

        var calls = new List<(int Index, float R1, float G1, float B1, float R2, float G2, float B2)>();
        VmColormaps1190Slice.UpdateSpecialColormap = (index, r1, g1, b1, r2, g2, b2) =>
            calls.Add((index, r1, g1, b1, r2, g2, b2));

        VmColormaps1190Slice.SetCustomInvulColor1(0x00FF0000);
        Assert.Empty(calls);
        Assert.Equal("s0", VmColormaps1190Slice.SpecialColormaps[0].Id);

        VmColormaps1190Slice.SetCustomizeInvulMap(true);
        Assert.Single(calls);
        Assert.Equal(0, calls[0].Index);
        Assert.Equal(255f / 128f, calls[0].R1);
        Assert.Equal(0f, calls[0].G1);
        Assert.Equal(0f, calls[0].B1);
        Assert.Equal(166f / 128f, calls[0].R2);
        Assert.Equal(166f / 128f, calls[0].G2);
        Assert.Equal(122f / 128f, calls[0].B2);

        VmColormaps1190Slice.SetCustomizeInvulMap(false);
        Assert.Equal("s5", VmColormaps1190Slice.SpecialColormaps[0].Id);
        Assert.NotSame(VmColormaps1190Slice.SpecialColormaps[5], VmColormaps1190Slice.SpecialColormaps[0]);
        VmColormaps1190Slice.SpecialColormaps[5].Id = "later";
        Assert.Equal("s5", VmColormaps1190Slice.SpecialColormaps[0].Id);

        VmColormaps1190Slice.SetCustomizeInvulMap(true);
        VmColormaps1190Slice.SetCustomInvulColor2(0x00000080);
        Assert.Equal(3, calls.Count);
        Assert.Equal(128f / 128f, calls[2].B2);
        Assert.Equal(0f, calls[2].R2);
    }
}
