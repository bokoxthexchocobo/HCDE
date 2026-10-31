using System.IO.Compression;
using System.Text;

namespace HCDE.Playsim;

/// <summary>
/// Fatal-error window, clipboard text, and the log viewer from <c>errorwindow.cpp</c> (1-359).
/// Chunk 1323. Color escapes and the Windows-1252 map are the callees this file uses.
/// </summary>
public sealed class VmErrorWindow1323
{
    public const int TextColorEscape = 0x1C;
    public const int NumTextColors = 26;
    public const int CrUndefined = -1;
    public const int CrYellow = 10;
    public const int CrUntranslated = 11;
    public const int CrGreen = 3;
    const int PrintChat = 3;
    const int PrintTeamChat = 4;

    static readonly int[] PrintColors = { 11, 5, 2, 3, 3, 11 };
    static readonly ushort[] Win1252 =
    {
        0x20AC, 0x81, 0x201A, 0x0192, 0x201E, 0x2026, 0x2020, 0x2021,
        0x02C6, 0x2030, 0x0160, 0x2039, 0x0152, 0x8D, 0x017D, 0x8F,
        0x90, 0x2018, 0x2019, 0x201C, 0x201D, 0x2022, 0x2013, 0x2014,
        0x02DC, 0x2122, 0x0161, 0x203A, 0x0153, 0x9D, 0x017E, 0x0178
    };
    static readonly (string Name, int Rgb)[] LogColors =
    {
        ("Brick", 0xCC3333),
        ("Tan", 0xD2B48C),
        ("Gray", 0xCCCCCC),
        ("Green", 0x00CC00),
        ("Brown", 0x996633),
        ("Gold", 0xFFCC00),
        ("Red", 0xFF5566),
        ("Blue", 0x9999FF),
        ("Orange", 0xFFAA00),
        ("White", 0xDFDFDF),
        ("Yellow", 0xEEEE33),
        ("Untranslated", 0xDFDFDF),
        ("Black", 0x000000),
        ("LightBlue", 0x33EEFF),
        ("Cream", 0xFFCC99),
        ("Olive", 0xD1D8A8),
        ("DarkGreen", 0x008C00),
        ("DarkRed", 0x800000),
        ("DarkBrown", 0x663333),
        ("Purple", 0x9966CC),
        ("DarkGray", 0x808080),
        ("Cyan", 0x00DDDD),
        ("Ice", 0x7C7C98),
        ("Fire", 0xD57604),
        ("Sapphire", 0x506CFC),
        ("Teal", 0x236773)
    };

    public string Game { get; set; } = "HCDE";
    public string Version { get; set; } = "1.0";
    public string GitTime { get; set; } = "now";
    public string Language { get; set; } = "enu";
    public double ScreenWidth { get; set; } = 1600;
    public double ScreenHeight { get; set; } = 900;
    public Dictionary<string, string> Strings { get; } = new(StringComparer.Ordinal);
    public string CanvasLanguage { get; private set; } = "";
    public string Caption { get; private set; } = "";
    public bool Shown { get; private set; }
    public bool LoopExited { get; private set; }
    public int LoopRuns { get; private set; }
    public string Clipboard { get; private set; } = "";
    public string? SavedPath { get; private set; }
    public byte[]? SavedZip { get; private set; }
    public bool OpenedWadDir { get; private set; }
    public bool OpenedConfigDir { get; private set; }
    public List<string> DialogFilters { get; } = new();
    public string DialogFilename { get; private set; } = "";
    public string DialogExtension { get; private set; } = "";
    public bool DialogAccepts { get; set; }
    public Action<VmErrorWindow1323>? DuringLoop { get; set; }
    public Window? Last { get; private set; }

    public string Text(string key)
    {
        if (Strings.TryGetValue(key, out string? value))
            return value;
        return key;
    }

    public bool ExecModal(string text, string log, byte[]? minidump)
    {
        var window = new Window(this, minidump ?? Array.Empty<byte>());
        Last = window;
        window.SetText(text, log);
        double width = 1200;
        double height = 700;
        window.SetFrame(
            (ScreenWidth - width) * 0.5,
            (ScreenHeight - height) * 0.5,
            width,
            height);
        window.Shown = true;
        Shown = true;
        LoopRuns++;
        DuringLoop?.Invoke(this);
        return window.Restart;
    }

    public sealed class Window
    {
        readonly VmErrorWindow1323 host;
        public byte[] MiniDump { get; }
        public bool Restart { get; private set; }
        public bool Shown { get; set; }
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }
        public string Focus { get; private set; } = "";
        public LogViewer Log { get; }
        public Button ClipboardButton { get; }
        public Button WadDirButton { get; }
        public Button ConfigDirButton { get; }
        public Button? RestartButton { get; }
        public Button? SaveReportButton { get; }
        public string ClipboardText { get; private set; } = "";

        public Window(VmErrorWindow1323 host, byte[] minidump)
        {
            this.host = host;
            MiniDump = minidump;
            host.CanvasLanguage = host.Language;
            host.Caption = host.Text("ERROR_FATAL") + " - " + host.Game + " " + host.Version + " (" + host.GitTime + ")";
            Log = new LogViewer(host);
            ClipboardButton = new Button(host.Text("ACTION_COPYTOCLIPBOARD"));
            ClipboardButton.Click = CopyClipboard;
            WadDirButton = new Button(host.Text("ACTION_OPENWADDIR"));
            WadDirButton.Click = () => host.OpenedWadDir = true;
            ConfigDirButton = new Button(host.Text("ACTION_OPENCONFIGDIR"));
            ConfigDirButton.Click = () => host.OpenedConfigDir = true;
            if (minidump.Length == 0)
            {
                RestartButton = new Button(host.Text("ACTION_RESTART"));
                RestartButton.Click = OnRestart;
            }
            else
            {
                SaveReportButton = new Button(host.Text("ERRORMNU_SAVE"));
                SaveReportButton.Click = OnSaveReport;
            }
            Focus = "log";
        }

        public void SetFrame(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public void SetText(string text, string log)
        {
            Log.SetText(text, log);
            var bytes = Encoding.Latin1.GetBytes(log);
            int cursor = 0;
            var built = new StringBuilder();
            while (cursor < bytes.Length)
            {
                int chr = NextChar(bytes, ref cursor);
                if (chr == 0)
                    break;
                if (chr == TextColorEscape)
                    continue;
                if (chr >= 0x1D && chr <= 0x1F)
                    chr = 0x2550;
                built.Append(EncodeUtf8(chr));
            }
            built.Append("\nExecution could not continue.\n");
            built.Append(text);
            built.Append('\n');
            ClipboardText = built.ToString();
            host.Clipboard = ClipboardText;
        }

        public void CopyClipboard()
        {
            host.Clipboard = ClipboardText;
        }

        public void OnRestart()
        {
            Restart = true;
            host.LoopExited = true;
        }

        public void OnSaveReport()
        {
            host.DialogFilters.Clear();
            host.DialogFilters.Add(host.Text("FILEPICKER_REPORTS") + "|*.zip");
            host.DialogFilters.Add(host.Text("FILEPICKER_ANY") + "|*.*");
            host.DialogFilename = "CrashReport.zip";
            host.DialogExtension = "zip";
            if (!host.DialogAccepts)
                return;
            byte[] zip = WriteReport(MiniDump, ClipboardText);
            host.SavedPath = host.DialogFilename;
            host.SavedZip = zip;
        }

        public void OnClose()
        {
            Restart = false;
            host.LoopExited = true;
        }

        public void OnGeometryChanged()
        {
            double x = Width;
            double y = Height - 15.0 - ClipboardButton.PreferredHeight;
            ClipboardButton.SetFrame(20.0, y, ClipboardButton.PreferredWidth, ClipboardButton.PreferredHeight);
            Place(RestartButton);
            Place(SaveReportButton);
            Place(ConfigDirButton);
            Place(WadDirButton);
            y -= 20.0;
            Log.SetFrame(0.0, 0.0, Width, y);

            void Place(Button? button)
            {
                if (button == null)
                    return;
                double bw = button.PreferredWidth;
                x -= 10.0 + bw;
                button.SetFrame(x - 10.0, y, bw, button.PreferredHeight);
            }
        }

        static byte[] WriteReport(byte[] dump, string log)
        {
            var memory = new MemoryStream();
            using (var zip = new ZipArchive(memory, ZipArchiveMode.Create, true))
            {
                WriteEntry(zip, "minidump.dmp", dump);
                WriteEntry(zip, "log.txt", Encoding.UTF8.GetBytes(log));
            }
            return memory.ToArray();
        }

        static void WriteEntry(ZipArchive zip, string name, byte[] payload)
        {
            ZipArchiveEntry entry = zip.CreateEntry(name, CompressionLevel.Optimal);
            using Stream stream = entry.Open();
            stream.Write(payload, 0, payload.Length);
        }
    }

    public sealed class LogViewer
    {
        readonly VmErrorWindow1323 host;
        public Scrollbar Bar { get; } = new();
        public List<Span> Lines { get; } = new();
        public double LeftInset { get; private set; }
        public double TopInset { get; private set; }
        public double RightInset { get; private set; }
        public double BottomInset { get; private set; }
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Width { get; private set; }
        public double Height { get; set; }
        public int Updates { get; private set; }
        public List<Painted> PaintedLines { get; } = new();
        public (int R, int G, int B)? FrameFill { get; private set; }

        public LogViewer(VmErrorWindow1323 host)
        {
            this.host = host;
            LeftInset = 8;
            TopInset = 8;
            RightInset = 3;
            BottomInset = 8;
        }

        public void SetFrame(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public void SetText(string text, string log)
        {
            Lines.Clear();
            byte[] bytes = Encoding.Latin1.GetBytes(log);
            int start = 0;
            while (start <= bytes.Length)
            {
                int end = Array.IndexOf(bytes, (byte)'\n', start);
                if (end < 0)
                {
                    Lines.Add(CreateLine(bytes, start, bytes.Length));
                    break;
                }
                Lines.Add(CreateLine(bytes, start, end));
                start = end + 1;
                if (start == bytes.Length)
                {
                    Lines.Add(CreateLine(bytes, start, start));
                    break;
                }
            }
            Lines.Add(CreateLine(Array.Empty<byte>(), 0, 0));
            var abort = new Span();
            abort.Add(host.Text("ERROR_ABORT"), 255, 170, 170);
            Lines.Add(abort);
            var message = new Span();
            message.Add(text, 255, 255, 170);
            Lines.Add(message);
            Bar.SetRanges(0, Lines.Count, 1, 100);
            Bar.SetPosition(Lines.Count - 1);
            Updates++;
        }

        public void OnGeometryChanged()
        {
            double sw = Bar.PreferredWidth;
            Bar.SetFrame(Width - sw, 0, sw, Height);
        }

        public void PaintFrame()
        {
            FrameFill = (38, 38, 38);
        }

        public void Paint()
        {
            PaintedLines.Clear();
            double width = Width - Bar.FrameWidth;
            double y = Height;
            int start = (int)Math.Min(
                Math.Round(Bar.Position + 1.0, MidpointRounding.AwayFromZero),
                Lines.Count);
            for (int i = start; i > 0 && y > 0.0; i--)
            {
                Span layout = Lines[i - 1];
                double top = y - layout.Height;
                PaintedLines.Add(new Painted(i - 1, top, layout.Text, layout.R, layout.G, layout.B, width));
                y -= layout.Height;
            }
        }

        public bool OnMouseWheel(int key)
        {
            if (key == 1)
                ScrollUp(4);
            else if (key == -1)
                ScrollDown(4);
            return true;
        }

        public void OnKey(string key)
        {
            if (key == "Home")
            {
                Bar.SetPosition(0);
                Updates++;
            }
            if (key == "End")
            {
                Bar.SetPosition(Bar.Max);
                Updates++;
            }
            else if (key == "PageUp")
                ScrollUp(20);
            else if (key == "PageDown")
                ScrollDown(20);
            else if (key == "Up")
                ScrollUp(4);
            else if (key == "Down")
                ScrollDown(4);
        }

        public void OnScrollbarScroll()
        {
            Updates++;
        }

        public void ScrollUp(int lines)
        {
            Bar.SetPosition(Math.Max(Bar.Position - lines, 0));
            Updates++;
        }

        public void ScrollDown(int lines)
        {
            Bar.SetPosition(Math.Min(Bar.Position + lines, Bar.Max));
            Updates++;
        }

        Span CreateLine(byte[] bytes, int start, int end)
        {
            var layout = new Span();
            int r = 255, g = 255, b = 255;
            var cur = new StringBuilder();
            int cursor = start;
            while (cursor < end)
            {
                int chr = NextChar(bytes, ref cursor);
                if (chr == 0)
                    break;
                if (chr != TextColorEscape)
                {
                    if (chr >= 0x1D && chr <= 0x1F)
                        chr = 0x2550;
                    cur.Append(char.ConvertFromUtf32(chr));
                    continue;
                }
                int range = ParseFontColor(bytes, ref cursor, CrUntranslated, CrYellow);
                if (range != CrUndefined)
                {
                    if (cur.Length > 0)
                        layout.Add(cur.ToString(), r, g, b);
                    cur.Clear();
                    (r, g, b) = LogColor(range);
                }
            }
            cur.Append(' ');
            layout.Add(cur.ToString(), r, g, b);
            return layout;
        }
    }

    public sealed class Span
    {
        public string Text { get; private set; } = "";
        public int R { get; private set; }
        public int G { get; private set; }
        public int B { get; private set; }
        public double Height { get; set; } = 16;
        public List<Piece> Pieces { get; } = new();

        public void Add(string text, int r, int g, int b)
        {
            Pieces.Add(new Piece(text, r, g, b));
            Text += text;
            R = r;
            G = g;
            B = b;
        }
    }

    public readonly record struct Piece(string Text, int R, int G, int B);
    public readonly record struct Painted(int Index, double Top, string Text, int R, int G, int B, double Width);

    public sealed class Button
    {
        public string Label { get; }
        public double PreferredWidth { get; set; } = 80;
        public double PreferredHeight { get; set; } = 30;
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }
        public Action? Click { get; set; }

        public Button(string label)
        {
            Label = label;
        }

        public void SetFrame(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }

    public sealed class Scrollbar
    {
        public double Min { get; private set; }
        public double Max { get; private set; } = 1;
        public double LineStep { get; private set; } = 1;
        public double PageStep { get; private set; } = 1;
        public double Position { get; private set; }
        public double PreferredWidth { get; set; } = 16;
        public double FrameX { get; private set; }
        public double FrameWidth { get; private set; }
        public double FrameHeight { get; private set; }

        public void SetRanges(double min, double max, double line, double page)
        {
            if (min >= max || line <= 0 || page <= 0)
                throw new InvalidOperationException("Scrollbar ranges out of bounds.");
            Min = min;
            Max = max;
            LineStep = line;
            PageStep = page;
            if (Position >= Max)
                Position = Max - 1;
            if (Position < Min)
                Position = Min;
        }

        public void SetPosition(double pos)
        {
            Position = pos;
            if (pos >= Max)
                Position = Max - 1;
            if (pos < Min)
                Position = Min;
        }

        public void SetFrame(double x, double y, double width, double height)
        {
            FrameX = x;
            _ = y;
            FrameWidth = width;
            FrameHeight = height;
        }
    }

    public static int NextChar(byte[] bytes, ref int cursor)
    {
        if (cursor >= bytes.Length)
            return 0;
        int z = bytes[cursor];
        if (z < 192)
        {
            cursor++;
            if (z >= 128 && z < 160)
                return Win1252[z - 128];
            return z;
        }
        int size = 1;
        int chr = DecodeUtf8(bytes, cursor, out size);
        if (chr >= 0)
        {
            cursor += size;
            return chr;
        }
        cursor++;
        return z;
    }

    public static int ParseFontColor(byte[] bytes, ref int cursor, int normal, int bold)
    {
        if (cursor >= bytes.Length)
        {
            return CrUndefined;
        }
        int raw = bytes[cursor];
        int next = cursor + 1;
        int color;
        if (raw == '-')
            color = normal;
        else if (raw == '+')
            color = bold;
        else if (raw == '!')
            color = PrintColors[PrintTeamChat];
        else if (raw == '*')
            color = PrintColors[PrintChat];
        else if (raw == '[')
        {
            int nameStart = next;
            while (next < bytes.Length && bytes[next] != (byte)']' && bytes[next] != 0)
                next++;
            string name = Encoding.Latin1.GetString(bytes, nameStart, next - nameStart);
            if (next < bytes.Length && bytes[next] != 0)
                next++;
            color = FindColor(name);
        }
        else if (raw >= 'A' && raw < NumTextColors + 'A')
            color = raw - 'A';
        else if (raw >= 'a' && raw < NumTextColors + 'a')
            color = raw - 'a';
        else
        {
            cursor = next - (raw == 0 ? 1 : 0);
            return CrUndefined;
        }
        cursor = next;
        return color;
    }

    public static int FindColor(string name)
    {
        for (int i = 0; i < LogColors.Length; i++)
        {
            if (string.Equals(LogColors[i].Name, name, StringComparison.OrdinalIgnoreCase))
                return i;
            if (i == 2 && name.Equals("Grey", StringComparison.OrdinalIgnoreCase))
                return i;
            if (i == 13 && name.Equals("Light Blue", StringComparison.OrdinalIgnoreCase))
                return i;
            if (i == 16 && name.Equals("Dark Green", StringComparison.OrdinalIgnoreCase))
                return i;
            if (i == 17 && name.Equals("Dark Red", StringComparison.OrdinalIgnoreCase))
                return i;
            if (i == 18 && name.Equals("Dark Brown", StringComparison.OrdinalIgnoreCase))
                return i;
            if (i == 20 && (name.Equals("DarkGrey", StringComparison.OrdinalIgnoreCase) ||
                            name.Equals("Dark Gray", StringComparison.OrdinalIgnoreCase) ||
                            name.Equals("Dark Grey", StringComparison.OrdinalIgnoreCase)))
                return i;
        }
        return CrUntranslated;
    }

    public static (int R, int G, int B) LogColor(int range)
    {
        if ((uint)range >= (uint)LogColors.Length)
            return (223, 223, 223);
        int rgb = LogColors[range].Rgb;
        return ((rgb >> 16) & 255, (rgb >> 8) & 255, rgb & 255);
    }

    public static string EncodeUtf8(int codepoint)
    {
        int size;
        byte[] buffer = new byte[4];
        if (EncodeRaw(codepoint, buffer, out size) != 0 || size <= 0)
            return "";
        return Encoding.UTF8.GetString(buffer, 0, size);
    }

    static int EncodeRaw(int codepoint, byte[] buffer, out int size)
    {
        size = 0;
        if (codepoint < 0)
            return -1;
        if (codepoint < 0x80)
        {
            buffer[0] = (byte)codepoint;
            size = 1;
        }
        else if (codepoint < 0x800)
        {
            buffer[0] = (byte)(0xC0 + ((codepoint & 0x7C0) >> 6));
            buffer[1] = (byte)(0x80 + (codepoint & 0x03F));
            size = 2;
        }
        else if (codepoint < 0x10000)
        {
            buffer[0] = (byte)(0xE0 + ((codepoint & 0xF000) >> 12));
            buffer[1] = (byte)(0x80 + ((codepoint & 0x0FC0) >> 6));
            buffer[2] = (byte)(0x80 + (codepoint & 0x003F));
            size = 3;
        }
        else if (codepoint <= 0x10FFFF)
        {
            buffer[0] = (byte)(0xF0 + ((codepoint & 0x1C0000) >> 18));
            buffer[1] = (byte)(0x80 + ((codepoint & 0x03F000) >> 12));
            buffer[2] = (byte)(0x80 + ((codepoint & 0x000FC0) >> 6));
            buffer[3] = (byte)(0x80 + (codepoint & 0x00003F));
            size = 4;
        }
        else
            return -1;
        return 0;
    }

    static int DecodeUtf8(byte[] src, int index, out int size)
    {
        size = 1;
        int c = src[index];
        if ((c & 0x80) == 0)
            return c;
        if (index + 1 >= src.Length)
            return -1;
        int c1 = src[index + 1];
        if (c1 < 0x80 || c1 >= 0xC0)
            return -1;
        c1 &= 0x3F;
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
        if (index + 2 >= src.Length)
            return -1;
        int c2 = src[index + 2];
        if (c2 < 0x80 || c2 >= 0xC0)
            return -1;
        c2 &= 0x3F;
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
        if (index + 3 >= src.Length)
            return -1;
        int c3 = src[index + 3];
        if (c3 < 0x80 || c1 >= 0xC0)
            return -1;
        c3 &= 0x3F;
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
}
