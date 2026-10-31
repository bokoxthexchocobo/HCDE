namespace HCDE.Playsim;

/// <summary>
/// Console text buffer from <c>c_consolebuffer.cpp</c> (<b>1–166</b>).
/// Chunk <b>1194</b>. Line breaking is a host call. Print colors are the console table.
/// An empty string has no terminator, so it is stored as an open line.
/// </summary>
public static class VmConsoleBuffer1194Slice
{
    public const int NewLine = 0;
    public const int AppendLine = 1;
    public const int ReplaceLine = 2;
    public const int PrintLow = 0;
    public const int PrintMedium = 1;
    public const int PrintHigh = 2;
    public const int PrintChat = 3;
    public const int PrintTeamChat = 4;
    public const int PrintLog = 5;
    public const int PrintBold = 200;
    public const int PrintLevels = 5;
    public const char TextColorEscape = '\u001c';
    public const string TextColorTan = "\u001cB";
    public const string TextColorGreen = "\u001cD";

    public static readonly int[] PrintColors = { 11, 5, 2, 3, 3, 11, 0 };

    public static Func<Font, int, string, List<BrokenLine>> BreakLines = (_, _, text) =>
        new List<BrokenLine> { new BrokenLine { Width = text.Length, Text = text } };

    public static void ClearHost()
    {
        BreakLines = (_, _, text) => new List<BrokenLine> { new BrokenLine { Width = text.Length, Text = text } };
        PrintColors[0] = 11;
        PrintColors[1] = 5;
        PrintColors[2] = 2;
        PrintColors[3] = 3;
        PrintColors[4] = 3;
        PrintColors[5] = 11;
        PrintColors[6] = 0;
    }

    public sealed class Font
    {
        public string Name = "";
    }

    public sealed class BrokenLine
    {
        public int Width;
        public string Text = "";
    }

    public sealed class Buffer
    {
        public readonly List<string> Text = new();
        public readonly List<List<BrokenLine>> BrokenParagraphs = new();
        public readonly List<int> BrokenStart = new() { 0 };
        public readonly List<BrokenLine> Lines = new();
        public int AddType = NewLine;
        public int TextLines;
        public bool WasCleared = true;
        public Font? LastFont;
        public int LastDisplayWidth = -1;
        public bool LastLineNeedsUpdate;

        public void AddText(int printLevel, string text)
        {
            var build = TextColorTan;
            if (AddType == ReplaceLine)
            {
                if (Text.Count > 0)
                    Text.RemoveAt(Text.Count - 1);
                LastLineNeedsUpdate = true;
            }
            else if (AddType == AppendLine)
            {
                if (Text.Count > 0)
                {
                    build = Text[Text.Count - 1];
                    Text.RemoveAt(Text.Count - 1);
                }

                printLevel = -1;
                LastLineNeedsUpdate = true;
            }

            if (printLevel >= 0 && printLevel != PrintHigh)
            {
                if (printLevel == PrintBold)
                    build = TextColorGreen;
                else if (printLevel < PrintLevels)
                    build = "" + TextColorEscape + (char)(PrintColors[printLevel] + 'A');
            }

            var size = text.Length;
            if (size > 0 && text[size - 1] == '\r')
            {
                size--;
                AddType = ReplaceLine;
            }
            else if (size > 0 && text[size - 1] == '\n')
            {
                size--;
                AddType = NewLine;
            }
            else
            {
                AddType = AppendLine;
            }

            Text.Add(build + text.Substring(0, size));
        }

        public void FormatText(Font font, int displayWidth)
        {
            if (!ReferenceEquals(font, LastFont) || displayWidth != LastDisplayWidth || WasCleared)
            {
                if (WasCleared)
                    LastLineNeedsUpdate = false;
                BrokenParagraphs.Clear();
                BrokenStart.Clear();
                BrokenStart.Add(0);
                Lines.Clear();
                LastFont = font;
                LastDisplayWidth = displayWidth;
                WasCleared = false;
            }

            var brokenSize = BrokenParagraphs.Count;
            if (brokenSize == Text.Count && LastLineNeedsUpdate && brokenSize > 0)
            {
                brokenSize--;
                BrokenParagraphs.RemoveAt(BrokenParagraphs.Count - 1);
            }

            var keep = BrokenStart[brokenSize];
            if (Lines.Count > keep)
                Lines.RemoveRange(keep, Lines.Count - keep);
            if (BrokenStart.Count > brokenSize)
                BrokenStart.RemoveRange(brokenSize, BrokenStart.Count - brokenSize);
            for (var i = brokenSize; i < Text.Count; i++)
            {
                var paragraph = BreakLines(font, displayWidth, Text[i]);
                BrokenParagraphs.Add(paragraph);
                BrokenStart.Add(Lines.Count);
                Lines.AddRange(paragraph);
            }

            TextLines = Lines.Count;
            BrokenStart.Add(TextLines);
            LastLineNeedsUpdate = false;
        }

        public void ResizeBuffer(int newSize)
        {
            if (Text.Count > newSize)
            {
                var delete = Text.Count - newSize;
                Text.RemoveRange(0, delete);
                WasCleared = true;
            }
        }

        public void Clear()
        {
            WasCleared = true;
            Text.Clear();
        }
    }
}
