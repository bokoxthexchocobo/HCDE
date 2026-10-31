using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Viewport scale modes and <c>vid_scale</c> commands from <c>r_videoscale.cpp</c> (<b>1–346</b>).
/// Chunk <b>1251</b>. The screen size, menu class, and special-UI callback are host seams.
/// <c>ActiveRatio</c> follows the caller in <c>v_draw.cpp</c> and is not credited here.
/// </summary>
public sealed class VmVideoScale1251
{
    public const int MinWidth = 320;
    public const int MinHeight = 200;
    public const int MinUiWidth = 640;
    public const int MinUiHeight = 400;
    public const int ModeCount = 8;

    public int ClientWidth = 1920;
    public int ClientHeight = 1080;
    public int MenuActive;
    public string? MenuKind;
    public Func<bool>? IsSpecialUi;
    public int VidAspect;
    public bool SizeNeeded;
    public int Mode;
    public int CustomWidth = MinWidth;
    public int CustomHeight = MinHeight;
    public bool Linear;
    public float PixelAspect = 1f;
    public float Factor = 1f;
    public bool Crop;
    public uint MinW = MinWidth;
    public uint MinH = MinHeight;
    public readonly bool[] Valid = { true, true, true, true, true, true, true, true };
    public readonly List<string> Log = new();
    float fillSx;
    float fillSy;
    float fillResult;
    float fill2Sx;
    float fill2Sy;
    float fill2Result;
    bool lastSpecial;
    static readonly float[] Forced =
    {
        4 / 3f,
        16 / 9f,
        16 / 10f,
        17 / 10f,
        5 / 4f,
        17 / 10f,
        64 / 27f,
    };
    public void SetCustomWidth(int self)
    {
        if (self < MinWidth)
            self = MinWidth;
        CustomWidth = self;
        SizeNeeded = true;
        Log.Add($"cvar width {CustomWidth} size=1");
    }
    public void SetCustomHeight(int self)
    {
        if (self < MinHeight)
            self = MinHeight;
        CustomHeight = self;
        SizeNeeded = true;
        Log.Add($"cvar height {CustomHeight} size=1");
    }
    public void SetLinear(bool self)
    {
        Linear = self;
        Log.Add($"cvar linear {(Linear ? 1 : 0)}");
    }
    public void SetPixelAspect(float self)
    {
        SizeNeeded = true;
        if ((double)self < 0.2 || (double)self > 5.0)
            self = 1f;
        PixelAspect = self;
        Log.Add($"cvar aspect {Fmt(PixelAspect)} size=1");
    }

    public void SetFactor(float self)
    {
        SizeNeeded = true;
        if ((double)self < 0.05 || (double)self > 2.0)
            self = 1f;
        Factor = self;
        bool show = Factor != 1f;
        Log.Add($"cvar factor {Fmt(Factor)} size=1 show={(show ? 1 : 0)}");
        if (show)
            Show();
    }

    public void SetMode(int self)
    {
        SizeNeeded = true;
        if (OutOfBounds(self))
            self = 0;
        Mode = self;
        Log.Add($"cvar mode {Mode} size=1");
    }

    public void SetCrop(bool self)
    {
        Crop = self;
        SizeNeeded = true;
        Log.Add($"cvar crop {(Crop ? 1 : 0)} size=1");
    }

    public void ForceMode(int self)
    {
        Mode = self;
    }

    public float Fill(uint width, uint height)
    {
        float sx = width;
        float sy = height;
        if (fillSx != sx || fillSy != sy)
        {
            if (sx <= 0f || sy <= 0f)
            {
                Log.Add("fill early 1");
                return 1f;
            }
            float ssx = MinUiWidth / sx;
            float ssy = MinUiHeight / sy;
            fillResult = ssx < ssy ? ssy : ssx;
            fillSx = sx;
            fillSy = sy;
            Log.Add($"fill miss {Fmt(fillResult)}");
        }
        else
        {
            Log.Add($"fill hit {Fmt(fillResult)}");
        }
        return fillResult;
    }

    public float Fill2(uint width, uint height)
    {
        float sx = width * 1.2f;
        float sy = height;
        if (fill2Sx != sx || fill2Sy != sy)
        {
            if (sx <= 0f || sy <= 0f)
            {
                Log.Add("fill2 early 1");
                return 1f;
            }
            float ssx = MinUiWidth / 1.2f / sx;
            float ssy = MinUiHeight / sy;
            fill2Result = ssx < ssy ? ssy : ssx;
            fill2Sx = sx;
            fill2Sy = sy;
            Log.Add($"fill2 miss {Fmt(fill2Result)}");
        }
        else
        {
            Log.Add($"fill2 hit {Fmt(fill2Result)}");
        }
        return fill2Result;
    }

    public uint MfillX(uint width, uint height)
    {
        return (uint)(width * Fill(width, height));
    }

    public uint MfillY(uint width, uint height)
    {
        return (uint)(height * Fill(width, height));
    }

    public uint MfillX2(uint width, uint height)
    {
        float left = width * Fill2(width, height);
        return (uint)(left * 1.2);
    }

    public uint MfillY2(uint width, uint height)
    {
        return (uint)(height * Fill2(width, height));
    }

    public uint WidthOf(int mode, uint width, uint height)
    {
        switch (mode)
        {
            case 0: return width;
            case 1: return MfillX(width, height);
            case 2: return 640;
            case 3: return 960;
            case 4: return 1280;
            case 5: return (uint)CustomWidth;
            case 6: return 320;
            default: return (uint)(MfillX2(width, height) * 1.2);
        }
    }

    public uint HeightOf(int mode, uint width, uint height)
    {
        switch (mode)
        {
            case 0: return height;
            case 1: return MfillY(width, height);
            case 2: return 400;
            case 3: return 600;
            case 4: return 800;
            case 5: return (uint)CustomHeight;
            case 6: return 200;
            default: return MfillY2(width, height);
        }
    }

    public int ScaledWidth(int width, int height)
    {
        if (VideoMenu())
        {
            Log.Add($"menu width {width}");
            return width;
        }
        ClampMode();
        Refresh();
        if (Crop && height > 0)
        {
            int beforeW = width;
            int beforeH = height;
            float ratio = ActiveRatio(width, height);
            if ((float)width / height > ratio)
                width = (int)(height * ratio);
            if ((float)width / height < ratio)
                height = (int)(width / ratio);
            if (width != beforeW || height != beforeH)
                Log.Add($"cropw {beforeW}x{beforeH} -> {width}x{height}");
        }
        int scaled = (int)(Factor * WidthOf(Mode, (uint)width, (uint)height));
        int result = Math.Max((int)MinW, scaled);
        Log.Add($"scaledw {result} mode={Mode} min={MinW}");
        return result;
    }

    public int ScaledHeight(int width, int height)
    {
        if (VideoMenu())
        {
            Log.Add($"menu height {height}");
            return height;
        }
        ClampMode();
        if (Crop && height > 0)
        {
            int beforeW = width;
            int beforeH = height;
            float ratio = ActiveRatio(width, height);
            if ((float)width / height < ratio)
                height = (int)(width / ratio);
            if ((float)width / height > ratio)
                width = (int)(height * ratio);
            if (width != beforeW || height != beforeH)
                Log.Add($"croph {beforeW}x{beforeH} -> {width}x{height}");
        }
        int scaled = (int)(Factor * HeightOf(Mode, (uint)width, (uint)height));
        int result = Math.Max((int)MinH, scaled);
        Log.Add($"scaledh {result} mode={Mode} min={MinH}");
        return result;
    }

    public float PixelAspectOf()
    {
        if (VideoMenu())
        {
            Log.Add("menu aspect 1.000000");
            return 1f;
        }
        ClampMode();
        float aspect = Mode == 5 ? PixelAspect : TableAspect(Mode);
        Log.Add($"aspect {Fmt(aspect)} mode={Mode}");
        return aspect;
    }

    public bool LinearScale()
    {
        if (VideoMenu())
        {
            Log.Add("menu linear 0");
            return false;
        }
        ClampMode();
        int x = ClientWidth;
        int y = ClientHeight;
        float aspect = PixelAspectOf();
        if (aspect > 1f)
            aspect = 1f / aspect;
        int sw = ScaledWidth(x, y);
        int sh = ScaledHeight(x, y);
        bool super = sw > x * aspect || sh > y * aspect;
        bool result = super || Linear;
        Log.Add($"linear super={(super ? 1 : 0)} flag={(Linear ? 1 : 0)} -> {(result ? 1 : 0)}");
        return result;
    }

    public void Show()
    {
        int x1 = ClientWidth;
        int y1 = ClientHeight;
        int x2 = ScaledWidth(x1, y1);
        int y2 = ScaledHeight(x1, y1);
        Log.Add($"Current vid_scalefactor: {Fmt(Factor)}");
        Log.Add($"Real resolution: {x1} x {y1}");
        Log.Add($"Emulated resolution: {x2} x {y2}");
    }

    public void CmdShow(string[] argv)
    {
        _ = argv;
        Show();
    }

    public void CmdToWidth(string[] argv)
    {
        if (argv.Length > 1)
        {
            SetFactor(1f);
            int denom = ScaledWidth(ClientWidth, ClientHeight);
            SetFactor((float)(Atof(argv[1]) / denom));
        }
    }

    public void CmdToHeight(string[] argv)
    {
        if (argv.Length > 1)
        {
            SetFactor(1f);
            int denom = ScaledHeight(ClientWidth, ClientHeight);
            SetFactor((float)(Atof(argv[1]) / denom));
        }
    }

    public void CmdSetScale(string[] argv)
    {
        if (argv.Length > 2)
        {
            SetCustomWidth(Atoi(argv[1]));
            SetCustomHeight(Atoi(argv[2]));
            if (argv.Length > 3)
            {
                SetLinear(Atob(argv[3]));
                if (argv.Length > 4)
                    SetPixelAspect((float)Atof(argv[4]));
            }
            SetMode(5);
            SetFactor(1f);
        }
        else
        {
            Log.Add("Usage: vid_setscale <x> <y> [bool linear] [float pixel-shape]");
            Log.Add("This command will create a custom viewport scaling mode.");
        }
    }

    public void CmdToLowest(string[] argv)
    {
        int method = 0;
        if (argv.Length > 1)
            method = Atoi(argv[1]);
        switch (method)
        {
            case 1:
                SetMode(5);
                SetFactor(1f);
                SetPixelAspect(1f);
                SetCustomWidth((int)MfillX((uint)ClientWidth, (uint)ClientHeight));
                SetCustomHeight((int)MfillY((uint)ClientWidth, (uint)ClientHeight));
                break;
            case 2:
                SetMode(1);
                SetFactor(1f);
                break;
            default:
                SetMode(0);
                SetFactor(Fill((uint)ClientWidth, (uint)ClientHeight));
                break;
        }
    }

    public float ActiveRatio(int width, int height)
    {
        float ratio = width / (float)height;
        int fake = FakeRatio();
        if (fake != -1)
            return Forced[fake];
        return ratio / PixelAspectOf();
    }

    int FakeRatio()
    {
        if (VidAspect < 1 || VidAspect > 6)
            return -1;
        int fake = VidAspect;
        if (fake == 3)
            return 0;
        if (fake == 5)
            return 3;
        return fake;
    }

    void Refresh()
    {
        bool special = IsSpecialUi == null || IsSpecialUi();
        if (special == lastSpecial)
            return;
        lastSpecial = special;
        SizeNeeded = true;
        if (!special)
        {
            MinW = MinWidth;
            MinH = MinHeight;
        }
        else
        {
            MinW = MinUiWidth;
            MinH = MinUiHeight;
        }
        Log.Add($"special {(special ? 1 : 0)} min {MinW} {MinH}");
    }

    void ClampMode()
    {
        if (OutOfBounds(Mode))
        {
            Mode = 0;
            Log.Add("mode clamp 0");
        }
    }

    bool OutOfBounds(int mode)
    {
        return mode < 0 || mode >= ModeCount || !Valid[mode];
    }

    bool VideoMenu()
    {
        return MenuActive != 0 && MenuKind == "VideoOptions";
    }

    static float TableAspect(int mode)
    {
        if (mode is 2 or 3 or 4 or 6 or 7)
            return 1.2f;
        return 1f;
    }

    public static string Fmt(float value)
    {
        return value.ToString("F6", CultureInfo.InvariantCulture);
    }

    public static int Atoi(string? text)
    {
        if (!Scan(text, false, out string token))
            return 0;
        if (int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            return value;
        return 0;
    }

    public static double Atof(string? text)
    {
        if (!Scan(text, true, out string token))
            return 0;
        if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            return value;
        return 0;
    }

    public static bool Atob(string? text)
    {
        if (text == null)
            return false;
        return text.Equals("true", StringComparison.OrdinalIgnoreCase)
            || text.Equals("1", StringComparison.OrdinalIgnoreCase);
    }

    static bool Scan(string? text, bool fraction, out string token)
    {
        token = "";
        if (string.IsNullOrEmpty(text))
            return false;
        int i = 0;
        while (i < text.Length && char.IsWhiteSpace(text[i]))
            i++;
        int start = i;
        if (i < text.Length && (text[i] == '+' || text[i] == '-'))
            i++;
        bool any = false;
        while (i < text.Length && char.IsDigit(text[i]))
        {
            any = true;
            i++;
        }
        if (fraction && i < text.Length && text[i] == '.')
        {
            i++;
            while (i < text.Length && char.IsDigit(text[i]))
            {
                any = true;
                i++;
            }
        }
        if (!any)
            return false;
        token = text[start..i];
        return true;
    }
}
