namespace HCDE.Playsim;

/// <summary>
/// Generic startup screen from <c>startscreen_generic.cpp</c> (<b>1–184</b>). Chunk <b>1229</b>.
/// BOOTLOGO is centered when either side is under 1280×960 and cropped from the
/// origin otherwise. The accent color is latched once. A notch is painted only
/// when it moves forward. The overlay keeps the last eight formatted lines.
/// </summary>
public sealed class VmGenericStart1229
{
    public const int NsGraphics = 15;
    public const int Notches = 400;

    public static int AccentBgra;
    static int Latched;
    static byte BarB = 255;
    static byte BarG = 255;
    static byte BarR = 255;
    static byte BarA = 255;

    public readonly List<string> Log = new();
    public int MaxPos;
    public int CurPos;
    public int NotchPos;
    public int BitmapW;
    public int BitmapH;
    public bool HasTexture;
    public bool HasBuffer;
    public bool HasFont;
    public int FontHeight;
    public string[]? Lines;
    public int LineCount = -1;

    public static void ResetAccent()
    {
        AccentBgra = 0;
        Latched = 0;
        BarB = 255;
        BarG = 255;
        BarR = 255;
        BarA = 255;
    }

    public static VmGenericStart1229 Create(int maxProgress, int lump, bool imageReady, int imageW, int imageH)
    {
        var screen = new VmGenericStart1229(maxProgress, lump, imageReady, imageW, imageH);
        screen.Log.Add("countdown");
        return screen;
    }

    VmGenericStart1229(int maxProgress, int lump, bool imageReady, int imageW, int imageH)
    {
        MaxPos = maxProgress;
        Log.Add("lookup BOOTLOGO ns=" + NsGraphics + " lump=" + lump);
        BitmapW = 640 * 2;
        BitmapH = 480 * 2;
        Log.Add("bitmap " + BitmapW + "x" + BitmapH);
        Clear(0, 0, BitmapW, BitmapH, 0, 0, 0, 255);
        if (lump == -1)
            return;
        if (!imageReady)
        {
            Log.Add("image none");
            return;
        }

        Log.Add("image w=" + imageW + " h=" + imageH + " style=0");
        if (imageW < BitmapW || imageH < BitmapH)
        {
            int x = 320 * 2 - imageW / 2;
            int y = 220 * 2 - imageH / 2;
            Log.Add("blit center x=" + x + " y=" + y);
        }
        else
        {
            Log.Add("blit crop x=0 y=0 w=" + BitmapW + " h=" + BitmapH);
        }
    }

    public bool DoProgress(int advance)
    {
        LatchAccent();
        bool done = BaseProgress(advance);
        if (MaxPos > 0)
        {
            int notch = (int)((long)CurPos * Notches / MaxPos);
            if (notch < 0)
                notch = 0;
            else if (notch > Notches)
                notch = Notches;
            if (notch > NotchPos)
            {
                Log.Add("notch pos=" + notch + " prev=" + NotchPos + " draw");
                Clear((320 - 100) * 2, 480 * 2 - 30, notch, 4 * 2, BarB, BarG, BarR, BarA);
                NotchPos = notch;
                if (HasTexture)
                    Log.Add("clean 1");
            }
            else
            {
                Log.Add("notch pos=" + notch + " prev=" + NotchPos + " hold");
            }
        }

        return done;
    }

    bool BaseProgress(int advance)
    {
        if (advance > 0 && advance == 1 && MaxPos > 0)
        {
            int headroom = Math.Max(MaxPos / 8, 1024);
            if (CurPos + advance >= MaxPos - headroom)
                MaxPos += Math.Max(MaxPos / 2, 2048);
        }

        CurPos = Math.Min(CurPos + advance, MaxPos);
        Log.Add("base adv=" + advance + " cur=" + CurPos + " max=" + MaxPos + " done=1");
        return true;
    }

    void LatchAccent()
    {
        if (Latched != 0 || BarA == 0)
            return;
        uint argb = (uint)AccentBgra;
        Latched = AccentBgra;
        BarR = (byte)((argb >> 16) & 0xff);
        BarG = (byte)((argb >> 8) & 0xff);
        BarB = (byte)(argb & 0xff);
        BarA = (byte)((argb >> 24) & 0xff);
        Log.Add("accent b=" + BarB + " g=" + BarG + " r=" + BarR + " a=" + BarA);
    }

    public void StartupStatus(string message, int colors, bool center)
    {
        Log.Add("msg startup colors=" + colors + " center=" + (center ? 1 : 0) + " [" + message + "]");
    }

    public void Loading(string message, int colors)
    {
        if (message.Length == 0)
            Log.Add("phase skip");
        else
            Log.Add("phase [" + message + "]");
        Log.Add("msg loading colors=" + colors + " [" + message + "]");
    }

    public void Append(string status)
    {
        Log.Add("msg append [" + status + "]");
    }

    public bool DrawOverlay()
    {
        if (!HasBuffer)
        {
            Log.Add("overlay skip buffer");
            return false;
        }

        if (!HasFont)
        {
            Log.Add("overlay skip font");
            return false;
        }

        int panelW = BitmapW - 16;
        int panelH = FontHeight * 8 + 8;
        int count = LineCount >= 0 ? LineCount : (Lines == null ? 0 : Lines.Length);
        Log.Add("overlay format panel=" + panelW + " lines=" + count + " ptr=" + (Lines == null ? 0 : 1));
        if (Lines == null || count == 0)
        {
            Log.Add("overlay no");
            return false;
        }

        Clear(0, 0, BitmapW, panelH, 0, 0, 0, 180);
        int show = Math.Min(count, 8);
        int start = count - show;
        for (int i = 0; i < show; i++)
        {
            Log.Add(
                "draw i=" + i + " x=1 y=" + i + ".5 [" + Lines[start + i]
                + "] fb=170 fg=170 fr=170 fa=255 bb=0 bg=0 br=0 ba=180");
        }

        Log.Add("overlay yes");
        return true;
    }

    void Clear(int x, int y, int w, int h, int b, int g, int r, int a)
    {
        Log.Add("clear x=" + x + " y=" + y + " w=" + w + " h=" + h + " b=" + b + " g=" + g + " r=" + r + " a=" + a);
    }
}
