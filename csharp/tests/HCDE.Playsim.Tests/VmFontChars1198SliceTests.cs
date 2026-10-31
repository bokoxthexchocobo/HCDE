namespace HCDE.Playsim.Tests;

public class VmFontChars1198SliceTests
{
    [Fact]
    public void CreatePalettedPixels_DecodesFon2RunsAndFon1Raw()
    {
        VmFontChars1198Slice.Reset();
        var fon2 = new byte[40];
        fon2[3] = (byte)'2';
        fon2[10] = 10;
        fon2[30] = unchecked((byte)-128);
        fon2[31] = 1;
        fon2[32] = 20;
        fon2[33] = 3;
        fon2[34] = unchecked((byte)-1);
        fon2[35] = 40;
        VmFontChars1198Slice.Lumps[4] = fon2;
        var glyph = new VmFontChars1198Slice.Glyph(4, 30, 2, 2, 6, 7);
        Assert.Equal(6, glyph.LeftOffset);
        Assert.Equal(7, glyph.TopOffset);
        Assert.Equal(new byte[] { 10, 10, 3, 10 }, glyph.CreatePalettedPixels(VmFontChars1198Slice.Luminance, 3));

        var plain = new byte[13];
        plain[0] = (byte)'F';
        plain[1] = (byte)'O';
        plain[2] = (byte)'N';
        plain[3] = (byte)'T';
        plain[8] = 3;
        plain[9] = 1;
        plain[10] = 2;
        plain[11] = 3;
        plain[12] = 4;
        VmFontChars1198Slice.Lumps[5] = plain;
        var other = new VmFontChars1198Slice.Glyph(5, 8, 2, 2, 0, 0);
        Assert.Equal(new byte[] { 1, 3, 2, 4 }, other.CreatePalettedPixels(0));

        var fon1 = new byte[24];
        fon1[3] = 0x1A;
        fon1[16] = 5;
        fon1[17] = 1;
        fon1[18] = 9;
        fon1[19] = 4;
        fon1[20] = 7;
        VmFontChars1198Slice.Lumps[6] = fon1;
        var raw = new VmFontChars1198Slice.Glyph(6, 17, 2, 2, 0, 0);
        Assert.Equal(new byte[] { 1, 4, 4, 4 }, raw.CreatePalettedPixels(0));

        fon1[16] = 0;
        fon1[17] = 200;
        Assert.Equal(200, raw.CreatePalettedPixels(0)[0]);

        VmFontChars1198Slice.Names[9] = "WIDGET";
        VmFontChars1198Slice.Lumps[9] = new byte[] { 1, 2, 3, 4 };
        var corrupt = new VmFontChars1198Slice.Glyph(9, 0, -1, 2, 0, 0);
        var error = Assert.Throws<VmFontChars1198Slice.FatalError>(() => corrupt.CreatePalettedPixels(0));
        Assert.Equal("The font WIDGET is corrupt", error.Detail);
    }

    [Fact]
    public void CopyPixels_ExpandsRemapAndSkipsTransparentEntries()
    {
        VmFontChars1198Slice.Reset();
        var fon2 = new byte[40];
        fon2[3] = (byte)'2';
        fon2[10] = 10;
        fon2[30] = unchecked((byte)-128);
        fon2[31] = 1;
        fon2[32] = 20;
        fon2[33] = 3;
        fon2[34] = unchecked((byte)-1);
        fon2[35] = 40;
        VmFontChars1198Slice.Lumps[4] = fon2;
        var palette = new VmFontChars1198Slice.Color[256];
        palette[10] = new VmFontChars1198Slice.Color { R = 1, G = 2, B = 3, A = 255 };
        palette[3] = new VmFontChars1198Slice.Color { R = 4, G = 5, B = 6, A = 0 };
        var glyph = new VmFontChars1198Slice.Glyph(4, 30, 2, 2, 0, 0) { SourceRemap = palette };

        var bitmap = new VmFontChars1198Slice.Bitmap();
        bitmap.Create(2, 2);
        Array.Fill(bitmap.Data, (byte)9);
        Assert.Equal(0, glyph.CopyPixels(bitmap, VmFontChars1198Slice.Luminance, 4));
        Assert.Equal(
            new byte[]
            {
                3, 2, 1, 255, 9, 9, 9, 9,
                3, 2, 1, 255, 3, 2, 1, 255,
            },
            bitmap.Data);

        var shortBitmap = new VmFontChars1198Slice.Bitmap();
        shortBitmap.Create(2, 1);
        Array.Fill(shortBitmap.Data, (byte)9);
        Assert.Equal(0, glyph.CopyPixels(shortBitmap, VmFontChars1198Slice.Normal));
        Assert.Equal(new byte[] { 3, 2, 1, 255, 9, 9, 9, 9 }, shortBitmap.Data);

        var missing = new VmFontChars1198Slice.Glyph(99, 0, 1, 1, 0, 0);
        var error = Assert.Throws<VmFontChars1198Slice.MissingLump>(() => missing.CopyPixels(bitmap, 0));
        Assert.Equal("OpenFileReader: 99 >= NumEntries", error.Detail);
    }
}
