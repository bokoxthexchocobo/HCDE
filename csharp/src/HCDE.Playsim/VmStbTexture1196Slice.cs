namespace HCDE.Playsim;

/// <summary>
/// stb_image textures from <c>stbtexture.cpp</c> (<b>1–155</b>).
/// Chunk <b>1196</b>. The info probe and the RGBA decode are host results.
/// Decoded pixels are copied as BGRA, and alpha 0 leaves the destination byte alone.
/// </summary>
public static class VmStbTexture1196Slice
{
    public const int Luminance = 1;
    public const int SeekSet = 0;
    public const int SeekCur = 1;

    public static readonly Dictionary<int, byte[]> Lumps = new();
    public static byte[,,] Rgb256k = new byte[64, 64, 64];
    public static Func<Reader, ProbeInfo> Probe = static _ => new ProbeInfo();
    public static Func<Reader, RgbaImage?> Load = static _ => null;
    public static Func<int, Reader> Open = OpenLump;

    public static void Reset()
    {
        Lumps.Clear();
        Rgb256k = new byte[64, 64, 64];
        Probe = static _ => new ProbeInfo();
        Load = static _ => null;
        Open = OpenLump;
    }

    public static Reader OpenLump(int lump)
    {
        Lumps.TryGetValue(lump, out var data);
        return new Reader { Data = data ?? Array.Empty<byte>() };
    }

    public sealed class ProbeInfo
    {
        public int Result;
        public int Width;
        public int Height;
        public int Components;
    }

    public sealed class RgbaImage
    {
        public int Width;
        public int Height;
        public byte[] Pixels = Array.Empty<byte>();
    }

    public sealed class Reader
    {
        public byte[] Data = Array.Empty<byte>();
        public int Position;

        public int Length => Data.Length;

        public void Seek(int offset, int origin)
        {
            Position = origin == SeekSet ? offset : Position + offset;
        }

        public int Read(byte[] dest, int size)
        {
            int available = Data.Length - Position;
            if (available < 0)
                available = 0;
            int count = size < available ? size : available;
            Buffer.BlockCopy(Data, Position, dest, 0, count);
            Position += count;
            return count;
        }

        public void Skip(int count)
        {
            Seek(count, SeekCur);
        }

        public int Eof()
        {
            return Position >= Length ? 1 : 0;
        }
    }

    public sealed class Bitmap
    {
        public byte[] Data = Array.Empty<byte>();
        public int Width;
        public int Height;
        public int Pitch;

        public void Create(int width, int height)
        {
            Pitch = width * 4;
            Width = width;
            Height = height;
            Data = new byte[4 * width * height];
        }
    }

    public sealed class Texture
    {
        public int SourceLump;
        public int Width;
        public int Height;
        public int LeftOffset;
        public int TopOffset;

        public Texture(int lump, int width, int height)
        {
            SourceLump = lump;
            Width = width;
            Height = height;
            LeftOffset = 0;
            TopOffset = 0;
        }

        public byte[] CreatePalettedPixels(int conversion, int frame = 0)
        {
            var bitmap = new Bitmap();
            bitmap.Create(Width, Height);
            CopyPixels(bitmap, conversion, frame);
            return PaletteFromBitmap(bitmap, conversion);
        }

        public int CopyPixels(Bitmap bitmap, int conversion, int frame = 0)
        {
            var lump = Open(SourceLump);
            var image = Load(lump);
            if (image != null)
            {
                CopyPixelDataRgb(bitmap, image.Pixels, image.Width, image.Height, 4, image.Width * 4);
            }

            return -1;
        }
    }

    public static Texture? TryCreate(Reader file, int lumpnum)
    {
        file.Seek(0, SeekSet);
        var info = Probe(file);
        if (info.Result == 1)
            return new Texture(lumpnum, info.Width, info.Height);
        return null;
    }

    public static byte[] PaletteFromBitmap(Bitmap bitmap, int conversion)
    {
        var data = bitmap.Data;
        int destAdv = bitmap.Height;
        int destRew = bitmap.Width * bitmap.Height - 1;
        var pixels = new byte[bitmap.Width * bitmap.Height];
        int dest = 0;
        int src = 0;
        bool luminance = conversion == Luminance;
        for (int y = bitmap.Height; y != 0; --y)
        {
            for (int x = bitmap.Width; x != 0; --x)
            {
                int b = data[src++];
                int g = data[src++];
                int r = data[src++];
                int a = data[src++];
                if (a < 128)
                    pixels[dest] = 0;
                else
                    pixels[dest] = RgbToPalette(luminance, r, g, b);
                dest += destAdv;
            }

            dest -= destRew;
        }

        return pixels;
    }

    public static byte RgbToPalette(bool wantLuminance, int r, int g, int b)
    {
        if (wantLuminance)
        {
            int lum = (r * 77 + g * 143 + b * 37) >> 8;
            return (byte)(lum * 255 / 255);
        }

        return Rgb256k[r >> 2, g >> 2, b >> 2];
    }

    public static void CopyPixelDataRgb(
        Bitmap bitmap, byte[] image, int srcWidth, int srcHeight, int stepX, int stepY)
    {
        int width = srcWidth;
        int height = srcHeight;
        if (width > bitmap.Width)
            width = bitmap.Width;
        if (width <= 0)
            return;
        if (height > bitmap.Height)
            height = bitmap.Height;
        if (height <= 0)
            return;

        for (int y = 0; y < height; y++)
        {
            int pin = y * stepY;
            int pout = y * bitmap.Pitch;
            for (int i = 0; i < width; i++)
            {
                int alpha = image[pin + 3];
                if (alpha != 0)
                {
                    bitmap.Data[pout] = image[pin + 2];
                    bitmap.Data[pout + 1] = image[pin + 1];
                    bitmap.Data[pout + 2] = image[pin];
                    bitmap.Data[pout + 3] = (byte)alpha;
                }

                pout += 4;
                pin += stepX;
            }
        }
    }
}
