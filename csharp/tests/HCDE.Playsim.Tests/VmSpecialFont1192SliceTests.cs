namespace HCDE.Playsim.Tests;

public class VmSpecialFont1192SliceTests
{
    [Fact]
    public void Create_CopiesPatchesAndMeasuresFromTheTallestGlyph()
    {
        VmSpecialFont1192Slice.Clear();
        var mask = new bool[256];
        mask[3] = true;
        var calls = new List<string>();
        VmSpecialFont1192Slice.FontCharCreated = (source, made) => calls.Add(source.Name + "->" + made.UseType);
        var narrow = Source("NARROW", 8, 8, 8, 8, top0: -3, top1: 1, imageTop: 2);
        var wide = Source("WIDE", 16, 16, 10.9f, 32, top0: 8, top1: 6, left0: 1, left1: 2, imageTop: 4, flags: VmSpecialFont1192Slice.OffsetsNotForFont);
        var font = VmSpecialFont1192Slice.CreateSpecialFont(
            "STCFN", 'M', 2, new[] { null, wide }, mask, 44, true);
        Assert.Equal('_', font.Cursor);
        Assert.Equal(44, font.Lump);
        Assert.True(font.NoTranslate);
        Assert.Equal(0, font.GlobalKerning);
        Assert.Equal('M', font.FirstChar);
        Assert.Equal('N', font.LastChar);
        Assert.True(font.NoTranslateColor[3]);
        mask[3] = false;
        Assert.True(font.NoTranslateColor[3]);
        Assert.Equal(48, font.FontHeight);
        Assert.Equal(5, font.SpaceWidth);
        Assert.Null(font.Chars[0].OriginalPic);
        Assert.Equal(5, font.Chars[0].XMove);
        var made = font.Chars[1].OriginalPic;
        Assert.NotNull(made);
        Assert.Equal(10, font.Chars[1].XMove);
        Assert.Equal(VmSpecialFont1192Slice.FontChar, made.UseType);
        Assert.Null(made.Name);
        Assert.Same(wide.Base, made.Base);
        Assert.Equal(4, made.Top[0]);
        Assert.Equal(4, made.Top[1]);
        Assert.Equal(8, (int)made.DisplayTop(0));
        Assert.Equal(8, font.Displacement);
        Assert.Equal(new[] { "WIDE->9" }, calls);
        Assert.Same(made, VmSpecialFont1192Slice.Added[0]);
        Assert.False(font.MixedCase);
        Assert.Same(font, VmSpecialFont1192Slice.FirstFont);

        var second = VmSpecialFont1192Slice.CreateSpecialFont(
            "EMPTY", 'A', 0, System.Array.Empty<VmSpecialFont1192Slice.GameTexture?>(), new bool[256], 1, false);
        Assert.Equal(4, second.SpaceWidth);
        Assert.Empty(second.Chars);
        Assert.Empty(second.Translations);
        second.LoadTranslations();
        Assert.Empty(second.Translations);
        Assert.Same(second, VmSpecialFont1192Slice.FirstFont);
        Assert.Same(font, second.Next);
        Assert.Equal(11, (int)narrow.DisplayHeight + Math.Abs((int)narrow.DisplayTop(0)));
        Assert.Equal(-3, (int)narrow.DisplayTop(0));
    }

    [Fact]
    public void FixXMoves_FillsMissingWidthsFromCaseAccentAndSpace()
    {
        VmSpecialFont1192Slice.Clear();
        VmSpecialFont1192Slice.IsLower = code => code is >= 'a' and <= 'z';
        VmSpecialFont1192Slice.UpperForLower = code => code is >= 'a' and <= 'z' ? code - 32 : code;
        VmSpecialFont1192Slice.StripAccent = code => code == 0xE9 ? 'e' : code;
        var count = 'c' - 'A' + 1;
        var lumps = new VmSpecialFont1192Slice.GameTexture?[count];
        lumps['A' - 'A'] = Source("A", 11, 8, 11, 8, top0: 5, imageTop: 5);
        lumps['N' - 'A'] = Source("N", 7, 8, 7, 8, top0: 1, imageTop: 1);
        var font = VmSpecialFont1192Slice.CreateSpecialFont("GAP", 'A', count, lumps, new bool[256], 2, false);
        Assert.Equal(4, font.SpaceWidth);
        Assert.Equal(11, font.Chars['A' - 'A'].XMove);
        Assert.Equal(4, font.Chars['B' - 'A'].XMove);
        Assert.Equal(11, font.Chars['a' - 'A'].XMove);
        Assert.Equal(5, font.Displacement);
        Assert.False(font.MixedCase);

        var countToAccent = 0xE9 - 'A' + 1;
        var every = new VmSpecialFont1192Slice.GameTexture?[countToAccent];
        for (var code = 'A'; code <= 'z'; code++)
        {
            if (code == 'B')
                continue;
            every[code - 'A'] = Source(((char)code).ToString(), 9, 6, 9, 6, top0: 2, imageTop: 2);
        }

        var mixed = VmSpecialFont1192Slice.CreateSpecialFont(
            "MIXED", 'A', countToAccent, every, new bool[256], 3, false);
        Assert.True(mixed.MixedCase);
        Assert.Equal(9, mixed.Chars[0xE9 - 'A'].XMove);
        Assert.Equal(2, mixed.Displacement);
        Assert.Equal(5, mixed.SpaceWidth);
    }

    [Fact]
    public void LoadTranslations_RebuildsLockedLuminosityColors()
    {
        for (var i = 0; i < 256; i++)
            VmSpecialFont1192Slice.BaseColors[i] = new VmSpecialFont1192Slice.Pal { A = 255, R = i };
        VmSpecialFont1192Slice.BaseColors[2] = new VmSpecialFont1192Slice.Pal { A = 200, G = 255 };
        VmSpecialFont1192Slice.BaseColors[3] = new VmSpecialFont1192Slice.Pal { A = 255, R = 127 };
        VmSpecialFont1192Slice.BaseColors[4] = new VmSpecialFont1192Slice.Pal { A = 9, R = 8, G = 7, B = 6 };
        VmSpecialFont1192Slice.BaseColors[5] = new VmSpecialFont1192Slice.Pal { A = 255, R = 255, G = 255, B = 255 };
        VmSpecialFont1192Slice.Clear();
        Assert.Equal(26, VmSpecialFont1192Slice.NumTextColors);
        Assert.Equal(11, VmSpecialFont1192Slice.CrUntranslated);
        var early = new byte[] { 10, 20, 30, 40 };
        var high = VmSpecialFont1192Slice.MakeLuminosity(100, 1, 2);
        VmSpecialFont1192Slice.ApplyLuminosity(high, early, 1);
        Assert.Equal(new byte[] { 10, 20, 30, 40 }, early);

        var below = new byte[] { 0, 0, 167, 9 };
        VmSpecialFont1192Slice.Ramp = new VmSpecialFont1192Slice.Pal[256];
        VmSpecialFont1192Slice.Ramp[0] = new VmSpecialFont1192Slice.Pal { A = 255, R = 80, G = 40, B = 20 };
        var desc = VmSpecialFont1192Slice.MakeLuminosity(0, 100, 200);
        VmSpecialFont1192Slice.ApplyLuminosity(desc, below, 1);
        Assert.Equal(10, below[0]);
        Assert.Equal(20, below[1]);
        Assert.Equal(40, below[2]);
        Assert.Equal(9, below[3]);

        var above = new byte[] { 255, 255, 255, 1, 0, 0, 0, 255 };
        VmSpecialFont1192Slice.Ramp[255] = new VmSpecialFont1192Slice.Pal { R = 10, G = 20, B = 30 };
        VmSpecialFont1192Slice.ApplyLuminosity(desc, above, 1);
        Assert.Equal(38, above[0]);
        Assert.Equal(25, above[1]);
        Assert.Equal(12, above[2]);
        Assert.Equal(1, above[3]);
        Assert.Equal(0, above[4]);

        var mask = new bool[256];
        mask[4] = true;
        var pic = Source("GLYPH", 2, 1, 2, 1, pixels: new byte[]
        {
            0, 0, 255, 255,
            0, 255, 0, 255,
            0, 0, 0, 0
        });
        var locked = VmSpecialFont1192Slice.CreateSpecialFont(
            "LOCK", 'A', 1, new[] { pic }, mask, 8, false);
        locked.TranslationType = 1;
        VmSpecialFont1192Slice.Ramp = new VmSpecialFont1192Slice.Pal[4 * 256];
        VmSpecialFont1192Slice.Ramp[3 * 256] = new VmSpecialFont1192Slice.Pal { A = 255, R = 200, G = 100, B = 50 };
        VmSpecialFont1192Slice.Ramp[3 * 256 + 251] = new VmSpecialFont1192Slice.Pal { A = 255, R = 251, G = 10, B = 20 };
        VmSpecialFont1192Slice.Ramp[3 * 256 + 255] = new VmSpecialFont1192Slice.Pal { A = 255, R = 10, G = 20, B = 30 };
        VmSpecialFont1192Slice.Pick = color => color.R;
        locked.LoadTranslations();
        Assert.Equal(26, locked.Translations.Count);
        Assert.Equal(0, locked.Translations[11]);
        Assert.False(VmSpecialFont1192Slice.IsLuminosity(locked.Translations[1]));
        Assert.NotEqual(locked.Translations[0], locked.Translations[1]);
        Assert.Equal(locked.Translations[0], locked.Translations[2]);
        var table = VmSpecialFont1192Slice.Table(locked.Translations[1]);
        Assert.True(table.ForFont);
        Assert.Equal(4, table.Remap[4]);
        Assert.Equal(8, table.Palette[4].R);
        Assert.Equal(7, table.Palette[4].G);
        Assert.Equal(6, table.Palette[4].B);
        Assert.Equal(9, table.Palette[4].A);
        Assert.Equal(251, table.Remap[2]);
        Assert.Equal(200, table.Palette[2].A);
        Assert.Equal(251, table.Palette[2].R);
        Assert.Equal(10, table.Palette[2].G);
        Assert.Equal(20, table.Palette[2].B);
        Assert.Equal(100, table.Palette[3].R);
        Assert.Equal(50, table.Palette[3].G);
        Assert.Equal(25, table.Palette[3].B);
        Assert.Equal(17, table.Palette[5].R);
        Assert.Equal(35, table.Palette[5].G);
        Assert.Equal(53, table.Palette[5].B);
        var before = VmSpecialFont1192Slice.StoredCount(0);
        var again = VmSpecialFont1192Slice.CreateSpecialFont(
            "LOCK2", 'A', 1, new[] { pic }, mask, 8, false);
        again.TranslationType = 1;
        again.LoadTranslations();
        Assert.Equal(before, VmSpecialFont1192Slice.StoredCount(0));
        Assert.Equal(locked.Translations[1], again.Translations[1]);

        var open = VmSpecialFont1192Slice.CreateSpecialFont(
            "OPEN", 'A', 1, new[] { pic }, new bool[256], 8, false);
        open.MinLum = 10;
        open.MaxLum = 200;
        var stores = VmSpecialFont1192Slice.StoredCount(0);
        open.LoadTranslations();
        Assert.Equal(stores, VmSpecialFont1192Slice.StoredCount(0));
        Assert.True(VmSpecialFont1192Slice.IsLuminosity(open.Translations[1]));
        Assert.Equal(10, (open.Translations[1] >> 8) & 255);
        Assert.Equal(200, open.Translations[1] & 255);
        Assert.Equal(0, open.Translations[11]);

        var blankMask = new bool[256];
        blankMask[1] = true;
        var blank = VmSpecialFont1192Slice.CreateSpecialFont(
            "BLANK", 'A', 2, new VmSpecialFont1192Slice.GameTexture?[] { null, null }, blankMask, 8, false);
        blank.LoadTranslations();
        Assert.Equal(VmSpecialFont1192Slice.MakeLuminosity(2, 255, 0), blank.Translations[1]);
        Assert.Equal(stores, VmSpecialFont1192Slice.StoredCount(0));
    }

    static VmSpecialFont1192Slice.GameTexture Source(
        string name, int texelW, int texelH, float displayW, float displayH,
        int top0 = 0, int top1 = 0, int left0 = 0, int left1 = 0, int imageTop = 0, int flags = 0, byte[]? pixels = null)
    {
        var image = new VmSpecialFont1192Slice.Raster
        {
            Width = pixels == null ? texelW : 2,
            Height = pixels == null ? texelH : 1,
            TopOffset = imageTop,
            Pixels = pixels ?? System.Array.Empty<byte>()
        };
        var tex = new VmSpecialFont1192Slice.GameTexture
        {
            Name = name,
            Base = new VmSpecialFont1192Slice.Tex { Width = texelW, Height = texelH, Image = image },
            Flags = flags,
            TexelWidth = texelW,
            TexelHeight = texelH,
            DisplayWidth = displayW,
            DisplayHeight = displayH,
            ScaleX = texelW / displayW,
            ScaleY = texelH / displayH
        };
        tex.Top[0] = top0;
        tex.Top[1] = top1;
        tex.Left[0] = left0;
        tex.Left[1] = left1;
        return tex;
    }
}
