namespace HCDE.Playsim;

/// <summary>
/// Notify-line stacking, fade, and the centered suffix from <c>c_notifybuffer.cpp</c>
/// (<b>1–220</b>). Chunk <b>1226</b>.
/// Identical text pops the live line and draws <c>(xN)</c> on the last row.
/// A fading line starts a new stack. A status-bar swallow still keeps the pop.
/// Print flags above bit 9 are masked off before the base add. A negative
/// scale cvar clamps to 0. Fade starts only when fewer than 6 tics remain.
/// </summary>
public sealed class VmNotifyText1226
{
    public int TimeOut;
    public int Ticker;
    public int PrintLevel;
    public string Text = "";
}

public sealed class VmNotifyBuffer1226
{
    public const int PrintTypes = 1023;
    public const int PrintLevels = 5;
    public const int ColorUntranslated = 11;
    public const int FadeTics = 6;
    public const int StateDemo = 3;
    public const int StateFullConsole = 4;

    public readonly List<VmNotifyText1226> Text = new();
    public readonly List<string> Log = new();
    public readonly int[] PrintColors = { 11, 5, 2, 3, 3, 11 };
    public int Top;
    public int TopGoal;
    public int CountedIdentical;
    public string LastNotify = "";
    public bool ShowMessages = true;
    public bool StackIdent;
    public int GameState;
    public int NotifyLines = 4;
    public int ScaleText;
    public bool CenterNotify;
    public bool PulseText;
    public double NotifyTime = 3;
    public bool GenericUi;
    public int DisplayWidth = 320;
    public int DrawerWidth = 320;
    public int DrawerHeight = 200;
    public int MsTime;
    public int FontDisplacement;
    public int FontHeight = 8;
    public int NewSmallFont = 1;
    public int AlternativeSmallFont = 2;
    public bool HasStatusBar;
    public bool HasFlush = true;
    public bool HasProcess = true;
    public Func<int, string, int> ProcessNotify = static (level, text) => text.Length == -level ? 1 : 0;
    public Func<bool, int> ScaleOf = static generic => 1;
    public Func<string, int> WidthOf = static text => text.Length;
    public int FlushCalls;

    public VmNotifyBuffer1226()
    {
        NotifyLines = 4;
    }

    public void SetConsoleNotifyBuffer()
    {
        Log.Add("bind");
    }

    public void SetScaleText(int value)
    {
        if (value < 0)
            value = 0;
        ScaleText = value;
        Log.Add("scale " + ScaleText);
    }

    public void SetNotifyLines(int value)
    {
        NotifyLines = value;
        Shift(value);
    }

    public void Shift(int maxLines)
    {
        if (maxLines >= 0 && Text.Count > maxLines)
            Text.RemoveRange(0, Text.Count - maxLines);
        Log.Add("shift " + maxLines + " n=" + Text.Count);
    }

    public void Clear()
    {
        Text.Clear();
        CountedIdentical = 0;
        LastNotify = "";
        int flush = 0;
        if (HasStatusBar && HasFlush)
        {
            FlushCalls++;
            flush = 1;
        }
        Log.Add("clear flush=" + flush + " n=" + Text.Count + " ident=" + CountedIdentical);
    }

    public void AddString(int printLevel, string source)
    {
        if (!ShowMessages || source.Length == 0 || GameState == StateDemo || GameState == StateFullConsole || NotifyLines == 0)
        {
            Log.Add("skip");
            return;
        }
        bool popped = false;
        if (StackIdent && Text.Count > 0 && string.CompareOrdinal(source, LastNotify) == 0)
        {
            VmNotifyText1226 last = Text[Text.Count - 1];
            if (last.Ticker < last.TimeOut)
            {
                CountedIdentical++;
                Text.RemoveAt(Text.Count - 1);
                popped = true;
            }
            else
            {
                LastNotify = source;
                CountedIdentical = 1;
            }
        }
        else
        {
            LastNotify = source;
            CountedIdentical = 1;
        }
        Log.Add("stack n=" + CountedIdentical + " pop=" + (popped ? 1 : 0) + " last=" + LastNotify);
        if (HasStatusBar && HasProcess && ProcessNotify(printLevel, source) != 0)
        {
            Log.Add("swallow");
            return;
        }
        int scale = ScaleOf(GenericUi);
        int width = DisplayWidth / scale;
        int font = GenericUi ? NewSmallFont : AlternativeSmallFont;
        Log.Add("add lvl=" + (printLevel & PrintTypes) + " font=" + font + " w=" + width + " time=" + NotifyTime.ToString("G", System.Globalization.CultureInfo.InvariantCulture) + " lines=" + NotifyLines + " text=" + source);
    }

    public void Draw()
    {
        int line = Top + FontDisplacement;
        bool canSkip = true;
        for (int i = 0; i < Text.Count; i++)
        {
            VmNotifyText1226 notify = Text[i];
            if (notify.TimeOut == 0)
            {
                Log.Add("idle " + i);
                continue;
            }
            int remain = notify.TimeOut - notify.Ticker;
            if (remain > 0)
            {
                double alpha = remain < FadeTics ? 1.0 * remain / FadeTics : 1;
                if (PulseText)
                    alpha *= 0.7 + 0.3 * Math.Sin(MsTime / 100.0);
                int color = notify.PrintLevel >= PrintLevels ? ColorUntranslated : PrintColors[notify.PrintLevel];
                int scale = ScaleOf(GenericUi);
                string suffix = "";
                int suffixWidth = 0;
                if (StackIdent && i == Text.Count - 1 && CountedIdentical > 1)
                {
                    suffix = " (x" + CountedIdentical + ")";
                    suffixWidth = WidthOf(suffix);
                }
                int textWidth = WidthOf(notify.Text);
                int total = textWidth + suffixWidth;
                int x = 0;
                if (CenterNotify)
                    x = (DrawerWidth / scale - total) / 2;
                int vw = DrawerWidth / scale;
                int vh = DrawerHeight / scale;
                Log.Add("D " + color + " " + x + " " + line + " " + vw + " " + vh + " " + alpha.ToString("F10", System.Globalization.CultureInfo.InvariantCulture) + " " + notify.Text);
                if (suffixWidth > 0)
                    Log.Add("S " + color + " " + (x + textWidth) + " " + line + " " + vw + " " + vh + " " + alpha.ToString("F10", System.Globalization.CultureInfo.InvariantCulture) + " " + suffix);
                line += FontHeight;
                canSkip = false;
            }
            else
            {
                notify.TimeOut = 0;
                Log.Add("expire " + i);
            }
        }
        if (canSkip)
            Top = TopGoal;
        Log.Add((canSkip ? "top " : "keep ") + Top);
    }
}
