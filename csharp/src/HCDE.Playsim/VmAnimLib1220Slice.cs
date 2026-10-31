namespace HCDE.Playsim;

/// <summary>
/// DeluxePaint large-page lookup and frame decoder from <c>animlib.cpp</c> (<b>1–290</b>).
/// Chunk <b>1220</b>. Header and page descriptors are little-endian on this host, so the in-place swaps keep the bytes.
/// <c>DrawFrame</c> paints from the previous frame through the frame before the one requested, then stores that request.
/// </summary>
public sealed class VmAnimLib1220
{
    public const int ImageBufferSize = 0x10000;
    public const int HeaderBytes = 128;

    public readonly byte[] ImageBuffer = new byte[ImageBufferSize];
    public readonly byte[] Palette = new byte[768];
    public int CurrentFrame;
    public ushort CurPage;
    public byte[]? File;
    public uint Id;
    public ushort MaxLps;
    public ushort NLps;
    public uint NRecords;
    public ushort MaxRecsPerLp;
    public ushort LpfTableOffset;
    public uint ContentType;
    public ushort Width;
    public ushort Height;
    public uint NFrames;
    public ushort FramesPerSecond;
    public ushort[] BaseRecord = System.Array.Empty<ushort>();
    public ushort[] PageRecords = System.Array.Empty<ushort>();
    public ushort[] PageBytes = System.Array.Empty<ushort>();

    int pageOrigin;

    public int Load(byte[] buffer, int length)
    {
        if (buffer == null)
            throw new NullReferenceException();
        if (length < 4 || buffer[0] != (byte)'L' || buffer[1] != (byte)'P' || buffer[2] != (byte)'F' || buffer[3] != (byte)' ')
            return -1;
        ulong remaining = unchecked((ulong)length);
        remaining -= (ulong)(HeaderBytes + 128 + 768);
        if (unchecked((int)remaining) < 0)
            return -1;

        File = buffer;
        CurPage = 0xffff;
        CurrentFrame = -1;
        Id = ReadU32(0);
        MaxLps = ReadU16(4);
        NLps = ReadU16(6);
        NRecords = ReadU32(8);
        MaxRecsPerLp = ReadU16(12);
        LpfTableOffset = ReadU16(14);
        ContentType = ReadU32(16);
        Width = ReadU16(20);
        Height = ReadU16(22);
        NFrames = ReadU32(64);
        FramesPerSecond = ReadU16(68);
        Poke32(0, LittleLong(Id));
        Poke16(4, LittleShort(MaxLps));
        Poke16(6, LittleShort(NLps));
        Poke32(8, LittleLong(NRecords));
        Poke16(12, LittleShort(MaxRecsPerLp));
        Poke16(14, LittleShort(LpfTableOffset));
        Poke32(16, LittleLong(ContentType));
        Poke16(20, LittleShort(Width));
        Poke16(22, LittleShort(Height));
        Poke32(64, LittleLong(NFrames));
        Poke16(68, LittleShort(FramesPerSecond));

        remaining -= (ulong)NLps * 6UL;
        if (unchecked((int)remaining) < 0)
            return -2;

        int cursor = HeaderBytes + 128;
        for (int pal = 0; pal < 768; pal += 3, cursor += 4)
        {
            Palette[pal + 2] = buffer[cursor];
            Palette[pal + 1] = buffer[cursor + 1];
            Palette[pal] = buffer[cursor + 2];
        }

        BaseRecord = new ushort[NLps];
        PageRecords = new ushort[NLps];
        PageBytes = new ushort[NLps];
        for (int i = 0; i < NLps; i++)
        {
            int at = cursor + i * 6;
            BaseRecord[i] = LittleShort(ReadU16(at));
            PageRecords[i] = LittleShort(ReadU16(at + 2));
            PageBytes[i] = LittleShort(ReadU16(at + 4));
            Poke16(at, BaseRecord[i]);
            Poke16(at + 2, PageRecords[i]);
            Poke16(at + 4, PageBytes[i]);
        }
        if (NRecords <= 0)
            return -1;
        return 0;
    }

    public byte[] DrawFrame(int frameNumber)
    {
        uint cnt = unchecked((uint)CurrentFrame);
        uint target = unchecked((uint)frameNumber);
        if (cnt > target)
            cnt = 0;
        do
        {
            DrawOne((ushort)cnt);
            cnt++;
        }
        while (cnt < target);
        CurrentFrame = frameNumber;
        return ImageBuffer;
    }

    void DrawOne(ushort frameNumber)
    {
        ushort page = FindPage(frameNumber);
        LoadPage(page);
        Render(frameNumber);
    }

    ushort FindPage(ushort frameNumber)
    {
        int i = CurPage & ~0xffff;
        int pages = NLps;
        bool restart = true;
        if (frameNumber < CurrentFrame)
        {
            i = 0;
            restart = false;
        }
        while (true)
        {
            for (; i < pages; i++)
            {
                int baseRecord = BaseRecord[i];
                int records = PageRecords[i];
                if (baseRecord <= frameNumber && frameNumber < baseRecord + records)
                    return (ushort)i;
            }
            if (restart && i == pages)
            {
                i = 0;
                restart = false;
                continue;
            }
            break;
        }
        return (ushort)i;
    }

    void LoadPage(ushort page)
    {
        if (CurPage == page)
            return;
        CurPage = page;
        pageOrigin = 0xb00 + page * ImageBufferSize + 8;
    }

    void Render(ushort frameNumber)
    {
        int page = CurPage;
        ushort frame = (ushort)(frameNumber - BaseRecord[page]);
        ushort offset = 0;
        while (frame != 0)
        {
            frame--;
            offset = (ushort)(offset + ReadU16(pageOrigin + frame * 2));
        }
        if (offset >= PageBytes[page])
            return;
        int src = pageOrigin + PageRecords[page] * 2 + offset + 4;
        if (File![src - 3] != 0)
        {
            int temp = ReadU16(src - 2);
            src += temp + (temp & 1);
        }
        Decode(src);
    }

    void Decode(int src)
    {
        int dst = 0;
        while (true)
        {
            byte count = File![src++];
            if (count == 0)
            {
                byte color = File[src + 1];
                count = File[src];
                src += 2;
                Fill(dst, color, count);
                dst += count;
                continue;
            }
            if ((count & 0x80) == 0)
            {
                Copy(dst, src, count);
                dst += count;
                src += count;
                continue;
            }
            count = (byte)(count & ~0x80);
            if (count > 0)
            {
                dst += count;
                continue;
            }
            int longCount = ReadU16(src);
            src += 2;
            if (longCount == 0)
                return;
            if ((longCount & 0x8000) == 0)
            {
                dst += longCount;
                continue;
            }
            longCount &= ~0x8000;
            if ((longCount & 0x4000) != 0)
            {
                byte color = File[src++];
                longCount &= ~0x4000;
                Fill(dst, color, longCount);
                dst += longCount;
                continue;
            }
            Copy(dst, src, longCount);
            dst += longCount;
            src += longCount;
        }
    }

    void Fill(int dst, byte color, int count)
    {
        for (int i = 0; i < count; i++)
            ImageBuffer[dst + i] = color;
    }

    void Copy(int dst, int src, int count)
    {
        for (int i = 0; i < count; i++)
            ImageBuffer[dst + i] = File![src + i];
    }

    ushort ReadU16(int at)
    {
        return (ushort)(File![at] | (File[at + 1] << 8));
    }

    uint ReadU32(int at)
    {
        return (uint)(File![at] | (File[at + 1] << 8) | (File[at + 2] << 16) | (File[at + 3] << 24));
    }

    void Poke16(int at, ushort value)
    {
        File![at] = (byte)value;
        File[at + 1] = (byte)(value >> 8);
    }

    void Poke32(int at, uint value)
    {
        File![at] = (byte)value;
        File[at + 1] = (byte)(value >> 8);
        File[at + 2] = (byte)(value >> 16);
        File[at + 3] = (byte)(value >> 24);
    }

    static ushort LittleShort(ushort value)
    {
        return value;
    }

    static uint LittleLong(uint value)
    {
        return value;
    }
}
