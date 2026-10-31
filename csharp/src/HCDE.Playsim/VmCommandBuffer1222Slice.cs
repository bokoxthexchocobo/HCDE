namespace HCDE.Playsim;

/// <summary>
/// Console line editor from <c>c_commandbuffer.cpp</c> (<b>1–337</b>).
/// Chunk <b>1222</b>. Cell width is the font width divided by 9.
/// A wide character that the visible start lands inside is skipped.
/// Word motion stops on the character before a space, so a cursor already
/// on a word start keeps walking into the previous word.
/// </summary>
public sealed class VmCommandBuffer1222
{
    public const int ColorOrange = 8;
    public const int ColorYellow = 10;

    public readonly List<int> Text = new();
    public readonly List<int> Yank = new();
    public readonly List<string> Draws = new();
    public int CursorPos;
    public int StartPos;
    public int CursorPosCells;
    public int StartPosCells;
    public bool AppendToYankBuffer;
    public int ConCols;
    public int DrawerWidth = 320;
    public int DrawerHeight = 200;
    public int ScaleDivisor = 1;
    public Func<int, int>? NewCharWidth;
    public Func<int, int>? CurrentCharWidth;

    public VmCommandBuffer1222()
    {
        ConCols = 0;
    }

    public VmCommandBuffer1222(VmCommandBuffer1222 other)
    {
        Text.AddRange(other.Text);
        CursorPos = other.CursorPos;
        StartPos = other.StartPos;
        NewCharWidth = other.NewCharWidth;
        CurrentCharWidth = other.CurrentCharWidth;
        DrawerWidth = other.DrawerWidth;
        DrawerHeight = other.DrawerHeight;
        ScaleDivisor = other.ScaleDivisor;
    }

    public byte[] GetText()
    {
        var bytes = new List<byte>();
        for (int i = 0; i < Text.Count; i++)
            AppendUtf8(bytes, Text[i]);
        return bytes.ToArray();
    }

    public int CalcCellSize(int length)
    {
        int cellcount = 0;
        for (int i = 0; i < length; i++)
            cellcount += NewCharWidth!(Text[i]) / 9;
        return cellcount;
    }

    public int CharsForCells(int cellin, out bool overflow)
    {
        int chars = 0;
        int cells = cellin;
        while (cells > 0)
        {
            int w = NewCharWidth!(Text[chars]);
            chars++;
            cells -= w / 9;
        }
        overflow = cells < 0;
        return chars;
    }

    public void MakeStartPosGood()
    {
        if (CursorPos > Text.Count)
            CursorPos = Text.Count;
        if (StartPos > Text.Count)
            StartPos = Text.Count;
        CursorPosCells = CalcCellSize(CursorPos);
        StartPosCells = CalcCellSize(StartPos);
        int lengthCells = CalcCellSize(Text.Count);
        int n = StartPosCells;
        int cols = ConCols / ScaleDivisor;
        if (StartPosCells >= lengthCells)
            n = CursorPosCells - cols + 2;
        if (CursorPosCells - StartPosCells >= cols - 2)
            n = CursorPosCells - cols + 2;
        if (StartPosCells > CursorPosCells)
            n = CursorPosCells;
        StartPosCells = Math.Max(0, n);
        StartPos = CharsForCells(StartPosCells, out bool overflow);
        if (overflow)
        {
            StartPosCells++;
            StartPos = CharsForCells(StartPosCells, out overflow);
        }
    }

    public bool CursorStart()
    {
        bool moved = CursorPos != 0;
        CursorPos = 0;
        StartPos = 0;
        CursorPosCells = 0;
        StartPosCells = 0;
        return moved;
    }

    public bool CursorEnd()
    {
        bool moved = CursorPos != Text.Count;
        CursorPos = Text.Count;
        MakeStartPosGood();
        return moved;
    }

    public void CursorLeft()
    {
        if (CursorPos > 0)
        {
            CursorPos--;
            MakeStartPosGood();
        }
    }

    public void CursorRight()
    {
        if (CursorPos < Text.Count)
        {
            CursorPos++;
            MakeStartPosGood();
        }
    }

    public void CursorWordLeft()
    {
        if (CursorPos > 0)
        {
            do
                CursorPos--;
            while (CursorPos > 0 && Text[CursorPos - 1] != ' ');
            MakeStartPosGood();
        }
    }

    public void CursorWordRight()
    {
        if (CursorPos < Text.Count)
        {
            do
                CursorPos++;
            while (CursorPos < Text.Count && Text[CursorPos] != ' ');
            MakeStartPosGood();
        }
    }

    public void DeleteLeft()
    {
        if (CursorPos > 0)
        {
            CursorPos--;
            Text.RemoveAt(CursorPos);
            MakeStartPosGood();
        }
    }

    public void DeleteRight()
    {
        if (CursorPos < Text.Count)
        {
            Text.RemoveAt(CursorPos);
            MakeStartPosGood();
        }
    }

    public void DeleteWordLeft()
    {
        if (CursorPos > 0)
        {
            int now = CursorPos;
            CursorWordLeft();
            var cut = Text.GetRange(CursorPos, now - CursorPos);
            if (AppendToYankBuffer)
                Yank.InsertRange(0, cut);
            else
            {
                Yank.Clear();
                Yank.AddRange(cut);
            }
            Text.RemoveRange(CursorPos, now - CursorPos);
            MakeStartPosGood();
        }
    }

    public void DeleteLineLeft()
    {
        if (CursorPos > 0)
        {
            var cut = Text.GetRange(0, CursorPos);
            if (AppendToYankBuffer)
                Yank.InsertRange(0, cut);
            else
            {
                Yank.Clear();
                Yank.AddRange(cut);
            }
            Text.RemoveRange(0, CursorPos);
            CursorStart();
        }
    }

    public void DeleteLineRight()
    {
        if (CursorPos < Text.Count)
        {
            var cut = Text.GetRange(CursorPos, Text.Count - CursorPos);
            if (AppendToYankBuffer)
                Yank.AddRange(cut);
            else
            {
                Yank.Clear();
                Yank.AddRange(cut);
            }
            Text.RemoveRange(CursorPos, Text.Count - CursorPos);
            CursorEnd();
        }
    }

    public void AddChar(int character)
    {
        if (Text.Count == 0)
            Text.Add(character);
        else
            Text.Insert(CursorPos, character);
        CursorPos++;
        MakeStartPosGood();
    }

    public void AddString(byte[] clip)
    {
        if (clip.Length != 0)
        {
            int n = clip.Length;
            int brk = IndexOfBreak(clip);
            if (brk >= 0)
                n = brk;
            var build = new List<int>();
            int i = 0;
            int chr;
            while ((chr = ReadChar(clip, n, ref i)) != 0)
                build.Add(chr);
            if (Text.Count == 0)
                Text.AddRange(build);
            else
                Text.InsertRange(CursorPos, build);
            CursorPos += build.Count;
            MakeStartPosGood();
        }
    }

    public void SetString(byte[] str)
    {
        Text.Clear();
        int i = 0;
        int chr;
        while ((chr = ReadChar(str, str.Length, ref i)) != 0)
            Text.Add(chr);
        CursorEnd();
        MakeStartPosGood();
    }

    public void AddYankBuffer()
    {
        if (Yank.Count > 0)
        {
            if (Text.Count == 0)
                Text.AddRange(Yank);
            else
                Text.InsertRange(CursorPos, Yank);
            CursorPos += Yank.Count;
            MakeStartPosGood();
        }
    }

    public void Draw(int x, int y, int scale, bool cursor)
    {
        int prompt = CurrentCharWidth!(0x1c);
        int cellw = CurrentCharWidth!(0x0b);
        uint delta = unchecked((uint)CursorPosCells - (uint)StartPosCells);
        int cx = x + prompt + unchecked((int)delta) * cellw;
        string tail = Codes(StartPos);
        if (scale == 1)
        {
            Draws.Add("C1 " + ColorOrange + " " + x + " " + y + " 1c");
            Draws.Add("T1 " + ColorOrange + " " + (x + prompt) + " " + y + " " + tail);
            if (cursor)
                Draws.Add("C1 " + ColorYellow + " " + cx + " " + y + " b");
        }
        else
        {
            int vw = DrawerWidth / scale;
            int vh = DrawerHeight / scale;
            Draws.Add("C2 " + ColorOrange + " " + x + " " + y + " 1c " + vw + " " + vh + " 1");
            Draws.Add("T2 " + ColorOrange + " " + (x + prompt) + " " + y + " " + tail + " " + vw + " " + vh + " 1");
            if (cursor)
                Draws.Add("C2 " + ColorYellow + " " + cx + " " + y + " b " + vw + " " + vh + " 1");
        }
    }

    string Codes(int from)
    {
        var parts = new List<string>();
        for (int i = from; i < Text.Count; i++)
            parts.Add(unchecked((uint)Text[i]).ToString("x"));
        return string.Join(",", parts);
    }

    static int IndexOfBreak(byte[] clip)
    {
        for (int i = 0; i < clip.Length; i++)
        {
            if (clip[i] == '\r' || clip[i] == '\n' || clip[i] == '\b')
                return i;
        }
        return -1;
    }

    static int ReadChar(byte[] src, int n, ref int i)
    {
        int z = i >= n ? 0 : src[i];
        if (z < 192)
        {
            i++;
            if (z >= 128 && z < 160)
                return Win1252[z - 128];
            return z;
        }
        int chr = Utf8Decode(src, n, i, out int size);
        if (chr >= 0)
        {
            i += size;
            return chr;
        }
        i++;
        return z;
    }

    static int At(byte[] src, int n, int i)
    {
        if (i < 0 || i >= n)
            return 0;
        return src[i];
    }

    static int Utf8Decode(byte[] src, int n, int baseIndex, out int size)
    {
        int c = At(src, n, baseIndex);
        size = 1;
        if ((c & 0x80) == 0)
            return c;
        int c1 = At(src, n, baseIndex + 1);
        if (c1 < 0x80 || c1 >= 0xc0)
            return -1;
        c1 &= 0x3f;
        if ((c & 0xE0) == 0xC0)
        {
            int pair = ((c & 0x1F) << 6) | c1;
            if (pair >= 128)
            {
                size = 2;
                return pair;
            }
            return -1;
        }
        int c2 = At(src, n, baseIndex + 2);
        if (c2 < 0x80 || c2 >= 0xc0)
            return -1;
        c2 &= 0x3f;
        if ((c & 0xF0) == 0xE0)
        {
            int triple = ((c & 0x0F) << 12) | (c1 << 6) | c2;
            if (triple >= 2048 && (triple < 55296 || triple > 57343))
            {
                size = 3;
                return triple;
            }
            return -1;
        }
        int c3 = At(src, n, baseIndex + 3);
        if (c3 < 0x80 || c1 >= 0xc0)
            return -1;
        c3 &= 0x3f;
        if ((c & 0xF8) == 0xF0)
        {
            int quad = ((c & 0x07) << 18) | (c1 << 12) | (c2 << 6) | c3;
            if (quad >= 65536 && quad <= 1114111)
            {
                size = 4;
                return quad;
            }
        }
        return -1;
    }

    static void AppendUtf8(List<byte> dest, int codepoint)
    {
        if (codepoint < 0)
            return;
        if (codepoint < 0x80)
            dest.Add((byte)codepoint);
        else if (codepoint < 0x800)
        {
            dest.Add((byte)(0xC0 + ((codepoint & 0x7C0) >> 6)));
            dest.Add((byte)(0x80 + (codepoint & 0x03F)));
        }
        else if (codepoint < 0x10000)
        {
            dest.Add((byte)(0xE0 + ((codepoint & 0xF000) >> 12)));
            dest.Add((byte)(0x80 + ((codepoint & 0x0FC0) >> 6)));
            dest.Add((byte)(0x80 + (codepoint & 0x003F)));
        }
        else if (codepoint <= 0x10FFFF)
        {
            dest.Add((byte)(0xF0 + ((codepoint & 0x1C0000) >> 18)));
            dest.Add((byte)(0x80 + ((codepoint & 0x03F000) >> 12)));
            dest.Add((byte)(0x80 + ((codepoint & 0x000FC0) >> 6)));
            dest.Add((byte)(0x80 + (codepoint & 0x00003F)));
        }
    }

    static readonly int[] Win1252 =
    {
        0x20AC, 0x81, 0x201A, 0x0192, 0x201E, 0x2026, 0x2020, 0x2021,
        0x02C6, 0x2030, 0x0160, 0x2039, 0x0152, 0x8d, 0x017D, 0x8f,
        0x90, 0x2018, 0x2019, 0x201C, 0x201D, 0x2022, 0x2013, 0x2014,
        0x02DC, 0x2122, 0x0161, 0x203A, 0x0153, 0x9d, 0x017E, 0x0178,
    };
}
