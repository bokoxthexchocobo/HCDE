namespace HCDE.Playsim.Tests;

public class VmColorSpace1211SliceTests
{
    static string Bits(VmColorSpace1211Slice color)
    {
        return $"{color.Type:X} {BitConverter.SingleToInt32Bits(color.V1):X8} {BitConverter.SingleToInt32Bits(color.V2):X8} {BitConverter.SingleToInt32Bits(color.V3):X8}";
    }

    [Fact]
    public void Rgb_RoundTripsOklabOklchAndRgb24()
    {
        var red = VmColorSpace1211Slice.Rgb(1f, 0f, 0f);
        Assert.Equal("0 3F800000 00000000 00000000", Bits(red));
        var lab = VmColorSpace1211Slice.Oklab(red);
        Assert.Equal("1 3F20C1AF 3E664281 3E00DDDA", Bits(lab));
        Assert.Equal("0 3F800000 00000000 00000000", Bits(red));
        var lch = VmColorSpace1211Slice.Oklch(red);
        Assert.Equal("2 3F20C1AF 3E83EF11 41E9DEFE", Bits(lch));
        Assert.Equal("0 3F800000 34374E99 00000000", Bits(VmColorSpace1211Slice.Rgb(lch)));
        Assert.Equal(0x00FF0000u, red.Rgb24());

        var packed = VmColorSpace1211Slice.Rgb(0x336699u);
        Assert.Equal("0 3E4CCCCD 3ECCCCCD 3F19999A", Bits(packed));
        Assert.Equal(0x00336699u, packed.Rgb24());
        Assert.Equal("1 3EE40E06 3D8DE113 3D901785", Bits(VmColorSpace1211Slice.Oklab(VmColorSpace1211Slice.Rgb(0.5f, 0.25f, 0.125f))));
        Assert.Equal("1 00000000 00000000 00000000", Bits(VmColorSpace1211Slice.Oklab(VmColorSpace1211Slice.Rgb(0f, 0f, 0f))));
        Assert.Equal("1 3F800000 2EB20468 33201718", Bits(VmColorSpace1211Slice.Oklab(VmColorSpace1211Slice.Rgb(1f, 1f, 1f))));

        var hue = VmColorSpace1211Slice.Oklch(0.7f, 0.1f, 30f);
        Assert.Equal("2 3F333333 3DCCCCCD 41F00000", Bits(hue));
        Assert.Equal("0 3F563457 3F06D0FE 3EF34D2D", Bits(VmColorSpace1211Slice.Rgb(hue)));
        Assert.Equal(0x00D58679u, hue.Rgb24());
        Assert.Equal("1 3F333333 3820A6B2 3806CD66", Bits(VmColorSpace1211Slice.Oklab(VmColorSpace1211Slice.Oklch(0.7f, 0.00005f, 40f))));

        var edge = VmColorSpace1211Slice.Oklch(VmColorSpace1211Slice.Oklab(0.5f, 0.00005f, 0.00005f));
        var keep = VmColorSpace1211Slice.Oklch(VmColorSpace1211Slice.Oklab(0.5f, 0.00007f, 0.00007f));
        Assert.Equal("2 3F000000 38944A81 7FC00000", Bits(edge));
        Assert.Equal("2 3F000000 38CF9B83 7FC00000", Bits(keep));
        Assert.Equal(VmColorSpace1211Slice.QuietNan, edge.V3);
        Assert.True(float.IsNaN(edge.V3));
    }

    [Fact]
    public void Mix_WrapsHueAndRejectsTheWrongSourceSpace()
    {
        var red = VmColorSpace1211Slice.Rgb(1f, 0f, 0f);
        var blue = VmColorSpace1211Slice.Rgb(0f, 0f, 1f);
        Assert.Equal("0 3F800000 00000000 00000000", Bits(VmColorSpace1211Slice.Mix(red, blue, 0f)));
        Assert.Equal("0 00000000 00000000 3F800000", Bits(VmColorSpace1211Slice.Mix(red, blue, 1f)));
        Assert.Equal("0 3F3ADB29 00000000 3F4294CC", Bits(VmColorSpace1211Slice.Mix(red, blue, 0.5f)));
        Assert.Equal("0 00000000 00000000 3F800000", Bits(VmColorSpace1211Slice.Mix(red, blue, 1.5f)));

        var high = VmColorSpace1211Slice.Oklch(0.6f, 0.12f, 350f);
        var low = VmColorSpace1211Slice.Oklch(0.6f, 0.12f, 10f);
        Assert.Equal("2 3F19999A 3DF5C28F 00000000", Bits(VmColorSpace1211Slice.Mix(high, low, 0.5f)));
        Assert.Equal("2 3F19999A 3DF5C28F 43AF0000", Bits(VmColorSpace1211Slice.Mix(high, low, -0.2f)));
        var gray = VmColorSpace1211Slice.Oklch(0.4f, 0f, VmColorSpace1211Slice.QuietNan);
        Assert.Equal("2 3EE66667 3CF5C28F 41200000", Bits(VmColorSpace1211Slice.Mix(gray, low, 0.25f)));

        var lab = VmColorSpace1211Slice.Oklab(red);
        var snapshot = lab.Clone();
        Assert.Throws<InvalidOperationException>(() => lab.RgbToOklab());
        Assert.Equal(Bits(snapshot), Bits(lab));
        Assert.Equal(VmColorSpace1211Slice.SpaceOklab, lab.Type);
    }
}
