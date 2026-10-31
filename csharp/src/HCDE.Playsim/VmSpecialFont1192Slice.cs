namespace HCDE.Playsim;

/// <summary>
/// Explicit character-patch font from <c>specialfont.cpp</c> (<b>1–195</b>).
/// Chunk <b>1192</b>. Case maps, accent folding, the color matcher, and the luminosity
/// ramps are host data. The constructor still applies those results.
/// </summary>
public static class VmSpecialFont1192Slice
{
    public const int FontChar = 9;
    public const int OffsetsNotForFont = 512;
    public const int CrUntranslated = 11;
    public const int NumTextColorsCount = 26;
    public const int TranslationInternal = 0;
    public const float EqualEpsilon = 1f / 65536f;

    public static int NumTextColors = NumTextColorsCount;
    public static readonly Pal[] BaseColors = new Pal[256];
    public static Pal[] Ramp = System.Array.Empty<Pal>();
    public static Font? FirstFont;
    public static readonly List<GameTexture> Added = new();
    public static Action<GameTexture, GameTexture>? FontCharCreated;
    public static Func<int, int> UpperForLower = code => code;
    public static Func<int, bool> IsLower = _ => false;
    public static Func<int, int> StripAccent = code => code;
    public static Func<Pal, int> Pick = _ => 0;

    static readonly Dictionary<int, List<RemapTable>> Slots = new();

    public struct Pal
    {
        public int A;
        public int R;
        public int G;
        public int B;
    }

    public sealed class FatalError : Exception
    {
        public FatalError(string message)
            : base(message)
        {
            Detail = message;
        }

        public string Detail;
    }

    public sealed class Raster
    {
        public int Width;
        public int Height;
        public int LeftOffset;
        public int TopOffset;
        public byte[] Pixels = System.Array.Empty<byte>();
    }

    public sealed class Tex
    {
        public int Width;
        public int Height;
        public Raster? Image;
    }

    public sealed class GameTexture
    {
        public Tex? Base;
        public string? Name;
        public int UseType;
        public int Flags;
        public int TexelWidth;
        public int TexelHeight;
        public float DisplayWidth;
        public float DisplayHeight;
        public float ScaleX = 1;
        public float ScaleY = 1;
        public readonly int[] Left = new int[2];
        public readonly int[] Top = new int[2];

        public float DisplayTop(int adjusted)
        {
            return Top[adjusted] / ScaleY;
        }
    }

    public sealed class Glyph
    {
        public GameTexture? OriginalPic;
        public int XMove = int.MinValue;
    }

    public sealed class RemapTable
    {
        public bool ForFont;
        public int NumEntries = 256;
        public readonly Pal[] Palette = new Pal[256];
        public readonly int[] Remap = new int[256];

        public RemapTable Clone()
        {
            var copy = new RemapTable { ForFont = ForFont, NumEntries = NumEntries };
            System.Array.Copy(Palette, copy.Palette, 256);
            System.Array.Copy(Remap, copy.Remap, 256);
            return copy;
        }
    }

    public sealed class Font
    {
        public string Name = "";
        public int Lump;
        public int FirstChar;
        public int LastChar;
        public int FontHeight;
        public int SpaceWidth;
        public int GlobalKerning;
        public int TranslationType;
        public int Displacement;
        public short MinLum = -1;
        public short MaxLum = -1;
        public char Cursor = '_';
        public bool NoTranslate;
        public bool MixedCase;
        public readonly bool[] NoTranslateColor = new bool[256];
        public readonly List<Glyph> Chars = new();
        public readonly List<int> Translations = new();
        public Font? Next;

        public Font(string name, int first, int count, GameTexture?[] lumps, bool[] mask, int lump, bool donttranslate)
        {
            if (count < 0 || lumps.Length < count)
                throw new FatalError("character list is shorter than count");
            if (mask.Length < 256)
                throw new FatalError("notranslate mask is shorter than 256");
            Next = FirstFont;
            FirstFont = this;
            Lump = lump;
            Cursor = '_';
            System.Array.Copy(mask, NoTranslateColor, 256);
            NoTranslate = donttranslate;
            Name = name;
            for (var i = 0; i < count; i++)
                Chars.Add(new Glyph());
            FirstChar = first;
            LastChar = first + count - 1;
            FontHeight = 0;
            GlobalKerning = 0;
            for (var i = 0; i < count; i++)
            {
                var pic = lumps[i];
                if (pic != null)
                {
                    var height = (int)pic.DisplayHeight;
                    var yoffs = (int)pic.DisplayTop(0);
                    height += Math.Abs(yoffs);
                    if (height > FontHeight)
                        FontHeight = height;
                }

                if (pic != null)
                {
                    var made = MakeFontChar(pic);
                    Chars[i].OriginalPic = made;
                    Added.Add(made);
                    Chars[i].XMove = (int)made.DisplayWidth;
                    if (FontCharCreated != null)
                        FontCharCreated(pic, made);
                }
                else
                {
                    Chars[i].OriginalPic = null;
                    Chars[i].XMove = int.MinValue;
                }
            }

            var n = 'N' - first;
            if (n >= 0 && n < count && Chars[n].OriginalPic != null)
                SpaceWidth = (Chars[n].XMove + 1) / 2;
            else
                SpaceWidth = 4;
            FixXMoves(this);
        }

        public void LoadTranslations()
        {
            LoadBase(this);
            var empty = true;
            foreach (var glyph in Chars)
            {
                if (glyph.OriginalPic != null)
                {
                    empty = false;
                    break;
                }
            }

            if (empty)
                return;
            var needs = false;
            for (var i = 0; i < 256; i++)
            {
                if (NoTranslateColor[i])
                {
                    needs = true;
                    break;
                }
            }

            if (!needs)
                return;
            for (var t = 0; t < Translations.Count; t++)
            {
                var trans = Translations[t];
                if (!IsLuminosity(trans))
                    continue;
                var remap = new RemapTable { ForFont = true };
                var work = new byte[1024];
                for (var i = 0; i < 256; i++)
                {
                    var color = BaseColor(i);
                    work[i * 4] = (byte)color.B;
                    work[i * 4 + 1] = (byte)color.G;
                    work[i * 4 + 2] = (byte)color.R;
                    work[i * 4 + 3] = (byte)color.A;
                }

                ApplyLuminosity(trans, work, 256);
                for (var i = 0; i < 256; i++)
                {
                    if (!NoTranslateColor[i])
                    {
                        remap.Palette[i] = new Pal
                        {
                            A = work[i * 4 + 3],
                            R = work[i * 4 + 2],
                            G = work[i * 4 + 1],
                            B = work[i * 4]
                        };
                        remap.Remap[i] = Pick(remap.Palette[i]) & 255;
                    }
                    else
                    {
                        remap.Palette[i] = BaseColor(i);
                        remap.Remap[i] = i;
                    }
                }

                Translations[t] = StoreTranslation(TranslationInternal, remap);
            }
        }
    }

    public static void Clear()
    {
        NumTextColors = NumTextColorsCount;
        FirstFont = null;
        Added.Clear();
        Slots.Clear();
        Ramp = System.Array.Empty<Pal>();
        FontCharCreated = null;
        UpperForLower = code => code;
        IsLower = _ => false;
        StripAccent = code => code;
        Pick = _ => 0;
        var identity = new RemapTable();
        for (var i = 0; i < 256; i++)
            identity.Palette[i] = BaseColor(i);
        SlotList(TranslationInternal).Add(identity);
    }

    public static Font CreateSpecialFont(
        string name, int first, int count, GameTexture?[] lumps, bool[] mask, int lump, bool donttranslate)
    {
        return new Font(name, first, count, lumps, mask, lump, donttranslate);
    }

    public static int StoredCount(int slot)
    {
        return SlotList(slot).Count;
    }

    public static RemapTable Table(int id)
    {
        var slot = (id >> 16) & 255;
        var index = id & 0xffff;
        return SlotList(slot)[index];
    }

    public static int MakeLuminosity(int range, int min, int max)
    {
        return (1 << 30) | ((range & 0x3fff) << 16) | ((min & 255) << 8) | (max & 255);
    }

    public static bool IsLuminosity(int trans)
    {
        return trans > 0 && (trans & (1 << 30)) != 0;
    }

    public static void ApplyLuminosity(int translation, byte[] pixel, int size)
    {
        var colorrange = (translation >> 16) & 0x3fff;
        if (colorrange >= NumTextColors * 2)
            return;
        var lumMin = (translation >> 8) & 255;
        var lumMax = translation & 255;
        var lumRange = lumMax - lumMin + 1;
        for (var i = 0; i < size; i++)
        {
            var at = i * 4;
            var gray = Luminance(pixel[at + 2], pixel[at + 1], pixel[at]);
            var index = Clamp((gray - lumMin) * 255 / lumRange, 0, 255);
            var neu = RampAt(colorrange, index);
            if (gray < lumMin && lumMin != 0)
            {
                neu.R = neu.R * gray / lumMin;
                neu.G = neu.G * gray / lumMin;
                neu.B = neu.B * gray / lumMin;
            }
            else if (gray > lumMax && lumMax != 0)
            {
                neu.R = Clamp(neu.R * gray / lumMax, 0, 255);
                neu.G = Clamp(neu.G * gray / lumMax, 0, 255);
                neu.B = Clamp(neu.B * gray / lumMax, 0, 255);
            }

            pixel[at] = (byte)neu.B;
            pixel[at + 1] = (byte)neu.G;
            pixel[at + 2] = (byte)neu.R;
        }
    }

    public static GameTexture MakeFontChar(GameTexture source)
    {
        var tex = source.Base;
        var made = new GameTexture { Base = tex, Name = null, UseType = FontChar };
        if (tex != null)
        {
            made.TexelWidth = tex.Width;
            made.DisplayWidth = tex.Width;
            made.TexelHeight = tex.Height;
            made.DisplayHeight = tex.Height;
            var image = tex.Image;
            if (image != null)
            {
                made.Left[0] = made.Left[1] = image.LeftOffset;
                made.Top[0] = made.Top[1] = image.TopOffset;
            }
        }

        made.ScaleX = 1;
        made.ScaleY = 1;
        CopySize(made, source, true);
        return made;
    }

    static void CopySize(GameTexture dest, GameTexture source, bool forFont)
    {
        if (dest.Base != null && source.Base != null)
        {
            dest.Base.Width = source.Base.Width;
            dest.Base.Height = source.Base.Height;
            dest.TexelWidth = dest.Base.Width;
            dest.TexelHeight = dest.Base.Height;
        }

        SetDisplaySize(dest, source.DisplayWidth, source.DisplayHeight);
        if (!forFont || (source.Flags & OffsetsNotForFont) == 0)
        {
            dest.Left[0] = source.Left[0];
            dest.Top[0] = source.Top[0];
            dest.Left[1] = source.Left[1];
            dest.Top[1] = source.Top[1];
        }
    }

    static void SetDisplaySize(GameTexture tex, float w, float h)
    {
        tex.DisplayWidth = w;
        tex.DisplayHeight = h;
        tex.ScaleX = tex.TexelWidth / w;
        tex.ScaleY = tex.TexelHeight / h;
        if ((int)(tex.ScaleX * w) != tex.TexelWidth)
            tex.ScaleX += EqualEpsilon;
        if ((int)(tex.ScaleY * h) != tex.TexelHeight)
            tex.ScaleY += EqualEpsilon;
    }

    static void FixXMoves(Font font)
    {
        if (font.FirstChar < 'a' && font.LastChar >= 'z')
        {
            font.MixedCase = true;
            for (var i = 'a'; i <= 'z'; i++)
            {
                if (font.Chars[i - font.FirstChar].OriginalPic == null)
                {
                    font.MixedCase = false;
                    break;
                }
            }
        }

        for (var i = 0; i <= font.LastChar - font.FirstChar; i++)
        {
            var glyph = font.Chars[i];
            if (glyph.XMove == int.MinValue)
            {
                var code = i + font.FirstChar;
                if (IsLower(code))
                {
                    var upper = UpperForLower(code);
                    if (upper >= font.FirstChar && upper <= font.LastChar)
                    {
                        glyph.XMove = font.Chars[upper - font.FirstChar].XMove;
                        continue;
                    }
                }

                var bare = StripAccent(code);
                if (bare != code)
                {
                    var index = bare - font.FirstChar;
                    if (index >= 0)
                    {
                        glyph.XMove = font.Chars[index].XMove;
                        continue;
                    }
                }

                glyph.XMove = font.SpaceWidth;
            }

            if (glyph.OriginalPic != null)
            {
                var ofs = (int)glyph.OriginalPic.DisplayTop(0);
                if (ofs > font.Displacement)
                    font.Displacement = ofs;
            }
        }
    }

    static void LoadBase(Font font)
    {
        var span = font.LastChar - font.FirstChar + 1;
        var count = font.Chars.Count < span ? font.Chars.Count : span;
        if (count == 0)
            return;
        var minlum = 255;
        var maxlum = 0;
        for (var i = 0; i < count; i++)
        {
            var pic = font.Chars[i].OriginalPic;
            if (pic != null)
                Record(pic, ref minlum, ref maxlum);
        }

        if (font.MinLum >= 0 && font.MinLum < minlum)
            minlum = font.MinLum;
        if (font.MaxLum > maxlum)
            maxlum = font.MaxLum;
        font.Translations.Clear();
        for (var i = 0; i < NumTextColors; i++)
        {
            if (i == CrUntranslated)
                font.Translations.Add(0);
            else
                font.Translations.Add(MakeLuminosity(i * 2 + font.TranslationType, minlum, maxlum));
        }
    }

    static void Record(GameTexture pic, ref int minlum, ref int maxlum)
    {
        var image = pic.Base?.Image;
        if (image == null)
            return;
        var size = image.Width * image.Height;
        var pixels = image.Pixels;
        for (var x = 0; x < size; x++)
        {
            var xx = x * 4;
            if (xx + 3 >= pixels.Length || pixels[xx + 3] == 0)
                continue;
            var lum = Luminance(pixels[xx + 2], pixels[xx + 1], pixels[xx]);
            if (lum < minlum)
                minlum = lum;
            if (lum > maxlum)
                maxlum = lum;
        }
    }

    public static int StoreTranslation(int slot, RemapTable remap)
    {
        var list = SlotList(slot);
        for (var i = 0; i < list.Count; i++)
        {
            if (SamePalette(list[i], remap))
                return (slot << 16) | i;
        }

        if (list.Count >= 65535)
            throw new FatalError("Too many DECORATE translations");
        list.Add(remap.Clone());
        return (slot << 16) | (list.Count - 1);
    }

    static bool SamePalette(RemapTable left, RemapTable right)
    {
        if (left.NumEntries != right.NumEntries)
            return false;
        for (var i = 0; i < left.NumEntries; i++)
        {
            if (left.Palette[i].A != right.Palette[i].A || left.Palette[i].R != right.Palette[i].R
                || left.Palette[i].G != right.Palette[i].G || left.Palette[i].B != right.Palette[i].B)
                return false;
        }

        return true;
    }

    static List<RemapTable> SlotList(int slot)
    {
        if (!Slots.TryGetValue(slot, out var list))
        {
            list = new List<RemapTable>();
            Slots[slot] = list;
        }

        return list;
    }

    static Pal BaseColor(int index)
    {
        if (index < 0 || index >= BaseColors.Length)
            return default;
        return BaseColors[index];
    }

    static Pal RampAt(int colorrange, int index)
    {
        var at = colorrange * 256 + index;
        if ((uint)at >= (uint)Ramp.Length)
            return default;
        return Ramp[at];
    }

    static int Luminance(int r, int g, int b)
    {
        return (r * 77 + g * 143 + b * 37) >> 8;
    }

    static int Clamp(int value, int low, int high)
    {
        if (value < low)
            return low;
        if (value > high)
            return high;
        return value;
    }
}
