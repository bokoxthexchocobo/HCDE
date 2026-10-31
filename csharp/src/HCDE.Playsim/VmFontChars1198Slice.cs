namespace HCDE.Playsim;

/// <summary>
/// FON1 and FON2 character pixels from <c>fontchars.cpp</c> (<b>1–167</b>).
/// Chunk <b>1198</b>. The lump bytes and the font name are host results.
/// Pixel runs are stored column-major. A transparent remap entry is left untouched.
/// </summary>
public static class VmFontChars1198Slice
{
    public const int Normal = 0;
    public const int Luminance = 1;
    public const int SeekSet = 0;
    public const int SeekCur = 1;

    public static readonly Dictionary<int, byte[]> Lumps = new();
    public static readonly Dictionary<int, string> Names = new();

    public static void Reset()
    {
        Lumps.Clear();
        Names.Clear();
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

    public sealed class MissingLump : Exception
    {
        public MissingLump(string message)
            : base(message)
        {
            Detail = message;
        }

        public string Detail;
    }

    public sealed class Reader
    {
        public byte[] Data = Array.Empty<byte>();
        public int Position;

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

        public byte ReadUInt8()
        {
            if (Position < 0 || Position >= Data.Length)
            {
                Position++;
                return 0;
            }

            return Data[Position++];
        }

        public sbyte ReadInt8() => (sbyte)ReadUInt8();
    }

    public struct Color
    {
        public byte R, G, B, A;
    }

    public sealed class Bitmap
    {
        public byte[] Data = Array.Empty<byte>();
        public int Width, Height, Pitch;

        public void Create(int width, int height)
        {
            Width = width;
            Height = height;
            Pitch = width * 4;
            Data = new byte[Pitch * height];
        }
    }

    public sealed class Glyph
    {
        public int SourceLump, SourcePos, Width, Height, LeftOffset, TopOffset;
        public Color[]? SourceRemap;

        public Glyph(int lump, int sourcePos, int width, int height, int left, int top)
        {
            SourceLump = lump;
            SourcePos = sourcePos;
            Width = width;
            Height = height;
            LeftOffset = left;
            TopOffset = top;
        }

        public byte[] CreatePalettedPixels(int conversion, int frame = 0)
        {
            var reader = Open(SourceLump);
            int destSize = unchecked(Width * Height);
            if (destSize < 0)
            {
                Names.TryGetValue(SourceLump, out var name);
                throw new FatalError($"The font {name} is corrupt");
            }

            byte max = 255;
            bool rle = true;
            var probe = new byte[4];
            reader.Read(probe, 4);
            if (probe[3] == (byte)'2')
            {
                var extra = new byte[7];
                reader.Read(extra, 7);
                max = extra[6];
                reader.Seek(SourcePos - 11, SeekCur);
            }
            else if (probe[3] == 0x1A)
            {
                var extra = new byte[13];
                reader.Read(extra, 13);
                max = (byte)(extra[12] - 1);
                reader.Seek(SourcePos - 17, SeekCur);
                rle = false;
            }
            else
            {
                reader.Seek(SourcePos - 4, SeekCur);
            }

            var pixels = new byte[destSize];
            int runLength = 0, setLength = 0, dest = 0;
            byte setValue = 0;
            int destAdv = Height;
            int destRew = destSize - 1;
            if (rle)
            {
                for (int y = Height; y != 0; --y)
                {
                    for (int x = Width; x != 0;)
                    {
                        if (runLength != 0)
                        {
                            byte color = reader.ReadUInt8();
                            pixels[dest] = color > max ? max : color;
                            dest += destAdv;
                            x--;
                            runLength--;
                        }
                        else if (setLength != 0)
                        {
                            pixels[dest] = setValue;
                            dest += destAdv;
                            x--;
                            setLength--;
                        }
                        else
                        {
                            sbyte code = reader.ReadInt8();
                            if (code >= 0)
                                runLength = code + 1;
                            else if (code != -128)
                            {
                                byte color = reader.ReadUInt8();
                                setLength = (-code) + 1;
                                setValue = color > max ? max : color;
                            }
                        }
                    }

                    dest -= destRew;
                }
            }
            else
            {
                for (int y = Height; y != 0; --y)
                {
                    for (int x = Width; x != 0; --x)
                    {
                        byte color = reader.ReadUInt8();
                        pixels[dest] = color > max ? max : color;
                        dest += destAdv;
                    }

                    dest -= destRew;
                }
            }

            return pixels;
        }

        public int CopyPixels(Bitmap bitmap, int conversion, int frame = 0)
        {
            if (conversion == Luminance)
                conversion = Normal;
            var pixels = CreatePalettedPixels(conversion, frame);
            var palette = SourceRemap;
            int width = Width > bitmap.Width ? bitmap.Width : Width;
            int height = Height > bitmap.Height ? bitmap.Height : Height;
            if (width <= 0 || height <= 0 || palette == null)
                return 0;
            for (int y = 0; y < height; y++)
            {
                int pout = y * bitmap.Pitch;
                for (int x = 0; x < width; x++)
                {
                    var color = palette[pixels[y + x * Height]];
                    if (color.A != 0)
                    {
                        bitmap.Data[pout] = color.B;
                        bitmap.Data[pout + 1] = color.G;
                        bitmap.Data[pout + 2] = color.R;
                        bitmap.Data[pout + 3] = color.A;
                    }

                    pout += 4;
                }
            }

            return 0;
        }
    }

    public static Reader Open(int lump)
    {
        if (!Lumps.TryGetValue(lump, out var data))
            throw new MissingLump($"OpenFileReader: {lump} >= NumEntries");
        return new Reader { Data = data };
    }
}
