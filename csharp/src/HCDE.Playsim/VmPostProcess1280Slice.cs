using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Scene post process, stereo flush, gamma present, and letterbox clear from
/// <c>gl_postprocess.cpp</c> (1-277). Chunk 1280. Passes, eyes, and the
/// present quad are host seams. Gamma is skipped when hardware gamma is on,
/// and a zero client size clears no border.
/// </summary>
public sealed class VmPostRect1280
{
    public int Left;
    public int Top;
    public int Width;
    public int Height;
}

public sealed class VmPostUniforms1280
{
    public float InvGamma;
    public float Contrast;
    public float Saturation;
    public float BlackPoint;
    public float WhitePoint;
    public int GrayFormula;
    public int HdrMode;
    public float ColorScale;
    public float ScaleX;
    public float ScaleY;
    public float OffsetX;
    public float OffsetY;
}

public sealed class VmPostProcess1280
{
    public const int PresentIndex = 8;
    public const int GameTicRate = 35;

    public int SceneWidth;
    public int SceneHeight;
    public int BufferWidth = 1;
    public int BufferHeight = 1;
    public int ClientWidth;
    public int ClientHeight;
    public int EyeCount = 1;
    public int ScreenWidth;
    public int ScreenHeight;
    public VmPostRect1280 Letterbox = new();
    public double DeltaTime;
    public ulong FrameTime;
    public int WorldTimer;
    public bool LinearScale;
    public bool HardwareGamma;
    public bool HdrActive;
    public bool Fullscreen;
    public float Gamma = 1f;
    public float Contrast = 1f;
    public float Saturation = 1f;
    public float BlackPoint;
    public float WhitePoint = 1f;
    public int SatFormula;
    public int DitherBpc;
    public VmPostUniforms1280 Uniforms = new();
    public List<string> Log = new();

    public void RenderScreenQuad()
    {
        Log.Add("quad " + PresentIndex + " 3");
    }

    public void PostProcessScene(int fixedColormap, float flash, Action? afterBloom)
    {
        int sceneWidth = SceneWidth;
        int sceneHeight = SceneHeight;
        float timeDelta = (float)DeltaTime;
        float time = (float)(FrameTime / 1000.0);
        float timeGame = (float)(WorldTimer / (double)GameTicRate);
        Log.Add("time " + Num(timeDelta) + " " + Num(time) + " " + Num(timeGame));
        Log.Add("pass1 " + fixedColormap + " " + sceneWidth + " " + sceneHeight);
        Log.Add("bind fb");
        if (afterBloom != null)
            afterBloom();
        Log.Add("pass2 " + fixedColormap + " " + Num(flash) + " " + sceneWidth + " " + sceneHeight);
    }

    public void AmbientOccludeScene(float m5)
    {
        Log.Add("ssao " + Num(m5) + " " + SceneWidth + " " + SceneHeight);
    }

    public void BlurScene(float gameInfoBlur)
    {
        int eyeCount = EyeCount;
        for (int i = 0; i < eyeCount; i++)
        {
            Log.Add("blur " + Num(gameInfoBlur) + " " + SceneWidth + " " + SceneHeight);
            if (eyeCount - i > 1)
                Log.Add("next eye " + eyeCount);
        }
    }

    public void ClearTonemapPalette()
    {
        Log.Add("tonemap clear");
    }

    public void Flush()
    {
        if (EyeCount == 1)
        {
            CopyToBackbuffer(null, true);
        }
        else
        {
            int eyeCount = EyeCount;
            for (int eye = 0; eye < eyeCount; eye++)
            {
                Log.Add("draw2d");
                if (eyeCount - eye > 1)
                    Log.Add("next eye " + eyeCount);
            }
            Log.Add("twod clear");
            Log.Add("present stereo");
        }
    }

    public void CopyToBackbuffer(VmPostRect1280? bounds, bool applyGamma)
    {
        Log.Add("draw2d");
        Log.Add("twod clear");
        Log.Add("shader screen");
        VmPostRect1280 box;
        if (bounds != null)
        {
            box = bounds;
        }
        else
        {
            ClearBorders();
            box = Letterbox;
        }
        Log.Add("bind tex 0");
        DrawPresentTexture(box, applyGamma);
    }

    public void DrawPresentTexture(VmPostRect1280 box, bool applyGamma)
    {
        Log.Add("viewport " + box.Left + " " + box.Top + " " + box.Width + " " + box.Height);
        Log.Add("dither 1");
        if (LinearScale)
            Log.Add("filter linear");
        else
            Log.Add("filter nearest");
        if (!applyGamma || HardwareGamma)
        {
            Uniforms.InvGamma = 1f;
            Uniforms.Contrast = 1f;
            Uniforms.Saturation = 1f;
            Uniforms.BlackPoint = 0f;
            Uniforms.WhitePoint = 1f;
        }
        else
        {
            Uniforms.InvGamma = 1f / Clamp(Gamma, 0.1f, 4f);
            Uniforms.Contrast = Clamp(Contrast, 0.1f, 3f);
            Uniforms.Saturation = Clamp(Saturation, -15f, 15f);
            Uniforms.BlackPoint = Clamp(BlackPoint, 0f, 1f);
            Uniforms.WhitePoint = Clamp(WhitePoint, 0f, 5f);
            Uniforms.GrayFormula = SatFormula;
        }
        if (HdrActive && Fullscreen)
        {
            Uniforms.HdrMode = 1;
            Uniforms.ColorScale = DitherBpc == -1 ? 1023f : (1 << DitherBpc) - 1;
        }
        else
        {
            Uniforms.HdrMode = 0;
            Uniforms.ColorScale = DitherBpc == -1 ? 255f : (1 << DitherBpc) - 1;
        }
        Uniforms.ScaleX = ScreenWidth / (float)BufferWidth;
        Uniforms.ScaleY = ScreenHeight / (float)BufferHeight;
        Uniforms.OffsetX = 0f;
        Uniforms.OffsetY = 0f;
        Log.Add(UniformText());
        RenderScreenQuad();
    }

    public void ClearBorders()
    {
        if (ClientWidth == 0 || ClientHeight == 0)
            return;
        Log.Add("viewport 0 0 " + ClientWidth + " " + ClientHeight);
        Log.Add("clear 0 0 0 1");
        Log.Add("scissor on");
        VmPostRect1280 box = Letterbox;
        if (box.Top > 0)
            Log.Add("scissor 0 0 " + ClientWidth + " " + box.Top);
        if (ClientHeight - box.Top - box.Height > 0)
            Log.Add("scissor 0 " + (box.Top + box.Height) + " " + ClientWidth + " " + (ClientHeight - box.Top - box.Height));
        if (box.Left > 0)
            Log.Add("scissor 0 " + box.Top + " " + box.Left + " " + box.Height);
        if (ClientWidth - box.Left - box.Width > 0)
            Log.Add("scissor " + (box.Left + box.Width) + " " + box.Top + " " + (ClientWidth - box.Left - box.Width) + " " + box.Height);
        Log.Add("scissor off");
    }

    public string UniformText()
    {
        return "uniform " + Num(Uniforms.InvGamma) + " " + Num(Uniforms.Contrast) + " " +
            Num(Uniforms.Saturation) + " " + Num(Uniforms.BlackPoint) + " " + Num(Uniforms.WhitePoint) +
            " gray " + Uniforms.GrayFormula + " hdr " + Uniforms.HdrMode + " scale " + Num(Uniforms.ColorScale) +
            " xy " + Num(Uniforms.ScaleX) + " " + Num(Uniforms.ScaleY) +
            " off " + Num(Uniforms.OffsetX) + " " + Num(Uniforms.OffsetY);
    }

    public static float Clamp(float value, float low, float high)
    {
        if (value < low)
            return low;
        if (value > high)
            return high;
        return value;
    }

    public static string Num(float value)
    {
        return value.ToString("G9", CultureInfo.InvariantCulture);
    }
}
