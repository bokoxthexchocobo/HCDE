namespace HCDE.Playsim;

/// <summary>
/// Build ANM textures from <c>anmtexture.cpp</c> (<b>1–165</b>).
/// Chunk <b>1197</b>. ANIM_LoadAnim and the color matcher are host results.
/// A successful probe is always 320 by 200. Frame 1 is the frame that is drawn.
/// </summary>
public static class VmAnmTexture1197Slice
{
    public const int FixedWidth = 320;
    public const int FixedHeight = 200;
    public const int SeekSet = 0;

    public static readonly Dictionary<int, byte[]> Lumps = new();
    public static Func<byte[], int, Anim?> Load = NoLoad;
    public static Func<int, int, int, byte> Pick = NoPick;

    public static void Reset()
    {
        Lumps.Clear();
        Load = NoLoad;
        Pick = NoPick;
    }

    static Anim? NoLoad(byte[] bytes, int length)
    {
        return null;
    }

    static byte NoPick(int r, int g, int b)
    {
        return 0;
    }

    public sealed class MissingLump : Exception
    {
        public MissingLump(string message)
            : base(message)
        {
            Detail = message;
        }

        public string Detail;
    }

    public sealed class Anim
    {
        public int Result;
        public int Frames;
        public byte[] Palette = Array.Empty<byte>();
        public Func<int, byte[]> Draw = static _ => Array.Empty<byte>();
    }

    public sealed class Reader
    {
        public byte[] Data = Array.Empty<byte>();
        public int Position;
        public int ReportedLength = -1;

        public int Length => ReportedLength >= 0 ? ReportedLength : Data.Length;

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

        public byte[]? ReadPadded(int padding)
        {
            int length = Length;
            if (length <= 0)
                return Array.Empty<byte>();
            if (Position > Data.Length || length > Data.Length - Position)
                return null;
            var buffer = new byte[length + padding];
            Buffer.BlockCopy(Data, Position, buffer, 0, length);
            Position += length;
            return buffer;
        }
    }

    public sealed class Bitmap
    {
        public byte[] Data = Array.Empty<byte>();
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

        public void ReadFrame(byte[] pixels, byte[] palette)
        {
            if (!Lumps.TryGetValue(SourceLump, out var lump))
                throw new MissingLump($"ReadFile: {SourceLump} >= NumEntries");
            var anim = Load(lump, lump.Length);
            if (anim != null && anim.Result >= 0 && anim.Frames >= 1)
            {
                Buffer.BlockCopy(anim.Palette, 0, palette, 0, 768);
                Buffer.BlockCopy(anim.Draw(1), 0, pixels, 0, Width * Height);
                return;
            }

            Array.Clear(pixels, 0, Width * Height);
            Array.Clear(palette, 0, 768);
        }

        public byte[] CreatePalettedPixels(int conversion, int frame = 0)
        {
            var raw = new byte[Width * Height];
            var palette = new byte[768];
            ReadFrame(raw, palette);
            var remap = new byte[256];
            for (int i = 0; i < 256; i++)
                remap[i] = Pick(palette[i * 3], palette[i * 3 + 1], palette[i * 3 + 2]);
            var pixels = new byte[Width * Height];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                    pixels[x * Height + y] = remap[raw[x + y * Width]];
            }

            return pixels;
        }

        public int CopyPixels(Bitmap bitmap, int conversion, int frame = 0)
        {
            var raw = new byte[Width * Height];
            var palette = new byte[768];
            ReadFrame(raw, palette);
            for (int i = 0; i < Width * Height; i++)
            {
                int offset = i * 4;
                int index = raw[i];
                bitmap.Data[offset] = palette[index * 3 + 2];
                bitmap.Data[offset + 1] = palette[index * 3 + 1];
                bitmap.Data[offset + 2] = palette[index * 3];
                bitmap.Data[offset + 3] = 255;
            }

            return -1;
        }
    }

    public static Texture? TryCreate(Reader file, int lumpnum)
    {
        file.Seek(0, SeekSet);
        var check = new byte[4];
        if (file.Read(check, 4) < 4)
            return null;
        if (check[0] != (byte)'L' || check[1] != (byte)'P' || check[2] != (byte)'F' || check[3] != (byte)' ')
            return null;
        file.Seek(0, SeekSet);
        var buffer = file.ReadPadded(1);
        if (buffer == null || buffer.Length < 4)
            return null;
        var anim = Load(buffer, buffer.Length - 1);
        if (anim == null || anim.Result < 0 || anim.Frames < 1)
            return null;
        return new Texture(lumpnum, FixedWidth, FixedHeight);
    }
}
